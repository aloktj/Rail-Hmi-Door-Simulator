using System;

namespace Common.Transport.Ipc
{
    public interface IIpcCanBusLogger
    {
        void LogError(string message, Exception exception);
    }
}
