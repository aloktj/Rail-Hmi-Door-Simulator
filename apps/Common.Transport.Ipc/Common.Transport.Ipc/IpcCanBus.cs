using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Common.Can;

namespace Common.Transport.Ipc
{
    /// <summary>
    /// Named-pipe based CAN bus (IPC) for Windows. Supports Server (HMI) and Client (Door).
    /// - Server accepts multiple clients and raises FrameReceived for incoming frames.
    /// - Client connects to server and can Send frames.
    /// </summary>
    public sealed class IpcCanBus : ICanBus
    {
        private readonly string _pipeName;
        private readonly IpcCanBusRole _role;
        private readonly Action<string, Exception> _log;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        // Server side
        private readonly List<NamedPipeServerStream> _serverClients = new List<NamedPipeServerStream>();
        private readonly object _lock = new object();

        // Client side
        private NamedPipeClientStream _client;

        public event Action<CanFrame> FrameReceived;

        public IpcCanBus(string pipeName, IpcCanBusRole role)
            : this(pipeName, role, (Action<string, Exception>)null)
        {
        }

        public IpcCanBus(string pipeName, IpcCanBusRole role, IIpcCanBusLogger logger)
            : this(pipeName, role, logger == null ? null : logger.LogError)
        {
        }

        public IpcCanBus(string pipeName, IpcCanBusRole role, Action<string, Exception> log)
        {
            if (string.IsNullOrWhiteSpace(pipeName)) throw new ArgumentException("pipeName is required");
            _pipeName = pipeName;
            _role = role;
            _log = log;
        }

        /// <summary>Starts background loops (server accept loop or client connect loop).</summary>
        public void Start()
        {
            if (_role == IpcCanBusRole.Server)
            {
                Task.Run(() => ServerAcceptLoop(_cts.Token));
            }
            else
            {
                Task.Run(() => ClientConnectLoop(_cts.Token));
            }
        }

        public void Send(CanFrame frame)
        {
            if (frame == null) return;

            if (_role == IpcCanBusRole.Client)
            {
                // Client sends to server
                var c = _client;
                if (c == null || !c.IsConnected) return;

                try
                {
                    WriteFrame(c, frame);
                    c.Flush();
                }
                catch (Exception ex)
                {
                    LogFailure("Send", ex);
                }
            }
            else
            {
                // Server can broadcast to all connected clients (optional)
                List<NamedPipeServerStream> snapshot;
                lock (_lock)
                {
                    snapshot = new List<NamedPipeServerStream>(_serverClients);
                }

                foreach (var s in snapshot)
                {
                    try
                    {
                        if (s.IsConnected)
                        {
                            WriteFrame(s, frame);
                            s.Flush();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogFailure("Send", ex);
                    }
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();

            try { _client?.Dispose(); } catch { }

            lock (_lock)
            {
                foreach (var s in _serverClients)
                {
                    try { s.Dispose(); } catch { }
                }
                _serverClients.Clear();
            }

            _cts.Dispose();
        }

        // --------------------------
        // Server implementation
        // --------------------------
        private void ServerAcceptLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                NamedPipeServerStream server = null;
                try
                {
                    // Create a new server instance for the next client connection
                    server = new NamedPipeServerStream(
                        _pipeName,
                        PipeDirection.InOut,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous);

                    server.WaitForConnection();

                    lock (_lock) _serverClients.Add(server);

                    // Start a read loop for this client
                    var clientStream = server;
                    Task.Run(() => ServerClientReadLoop(clientStream, ct));
                    server = null; // ownership transferred
                }
                catch (Exception ex)
                {
                    try { server?.Dispose(); } catch { }
                    if (ct.IsCancellationRequested) break;
                    LogFailure("ServerAcceptLoop", ex);
                    Thread.Sleep(200);
                }
            }
        }

        private void ServerClientReadLoop(NamedPipeServerStream client, CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested && client.IsConnected)
                {
                    var frame = ReadFrame(client);
                    if (frame == null) break;

                    FrameReceived?.Invoke(frame);
                }
            }
            catch (Exception ex)
            {
                LogFailure("ServerClientReadLoop", ex);
            }
            finally
            {
                lock (_lock) _serverClients.Remove(client);
                try { client.Dispose(); } catch { }
            }
        }

        // --------------------------
        // Client implementation
        // --------------------------
        private void ClientConnectLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (_client == null)
                    {
                        _client = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                    }

                    if (!_client.IsConnected)
                    {
                        _client.Connect(2000); // 2s timeout, retry if server not up yet
                        Task.Run(() => ClientReadLoop(_client, ct));
                    }

                    // Keep loop idle if connected
                    Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                    try { _client?.Dispose(); } catch { }
                    _client = null;
                    LogFailure("ClientConnectLoop", ex);
                    Thread.Sleep(500);
                }
            }
        }

        private void ClientReadLoop(NamedPipeClientStream client, CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested && client.IsConnected)
                {
                    var frame = ReadFrame(client);
                    if (frame == null) break;
                    FrameReceived?.Invoke(frame);
                }
            }
            catch (Exception ex)
            {
                LogFailure("ClientReadLoop", ex);
            }
        }

        // --------------------------
        // Framing: length-prefixed binary
        // --------------------------
        private static void WriteFrame(Stream stream, CanFrame frame)
        {
            // Payload: Id(uint32) + Dlc(byte) + Ticks(int64) + Data(dlc bytes)
            var dlc = frame.Dlc;
            if (dlc > 8) dlc = 8;

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(frame.Id);
                bw.Write(dlc);
                bw.Write(frame.Timestamp.ToUniversalTime().Ticks);

                var data = frame.Data ?? Array.Empty<byte>();
                for (int i = 0; i < dlc; i++)
                {
                    bw.Write(i < data.Length ? data[i] : (byte)0);
                }

                bw.Flush();
                var payload = ms.ToArray();

                using (var outBw = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
                {
                    outBw.Write(payload.Length);
                    outBw.Write(payload);
                }
            }
        }

        private CanFrame ReadFrame(Stream stream)
        {
            using (var br = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                // If stream closed, this will throw/end
                int len;
                try
                {
                    len = br.ReadInt32();
                }
                catch (Exception ex)
                {
                    LogFailure("ReadFrame", ex);
                    return null;
                }

                if (len <= 0 || len > 1024)
                {
                    LogFailure("ReadFrame", new InvalidDataException($"Invalid frame length {len}."));
                    return null; // sanity
                }

                var payload = br.ReadBytes(len);
                if (payload.Length != len)
                {
                    LogFailure("ReadFrame", new EndOfStreamException($"Expected {len} bytes, received {payload.Length}."));
                    return null;
                }

                using (var ms = new MemoryStream(payload))
                using (var inBr = new BinaryReader(ms))
                {
                    try
                    {
                        var id = inBr.ReadUInt32();
                        var dlc = inBr.ReadByte();
                        if (dlc > 8) dlc = 8;
                        var ticks = inBr.ReadInt64();

                        var data = new byte[dlc];
                        for (int i = 0; i < dlc; i++)
                            data[i] = inBr.ReadByte();

                        return new CanFrame
                        {
                            Id = id,
                            Dlc = dlc,
                            Data = data,
                            Timestamp = new DateTime(ticks, DateTimeKind.Utc)
                        };
                    }
                    catch (Exception ex)
                    {
                        LogFailure("ReadFrame", ex);
                        return null;
                    }
                }
            }
        }

        private void LogFailure(string operation, Exception exception)
        {
            if (_log == null || exception == null)
            {
                return;
            }

            var message = $"IpcCanBus {_role} pipe '{_pipeName}' {operation} failed.";
            _log(message, exception);
        }
    }
}
