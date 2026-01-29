using System;
 
namespace Common.Can
{
    /// <summary>
    /// Represents one CAN (Classic CAN) frame.
    /// </summary>
    public sealed class CanFrame
    {
        /// <summary>11-bit (standard) or 29-bit (extended) identifier stored in a 32-bit container.</summary>
        public uint Id { get; set; }

        /// <summary>Data length code: 0..8 for Classic CAN.</summary>
        public byte Dlc { get; set; }

        /// <summary>Payload bytes. For Classic CAN, length is typically 0..8.</summary>
        public byte[] Data { get; set; } = Array.Empty<byte>();

        /// <summary>Timestamp when the frame was received or created.</summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public override string ToString()
        {
            // Simple debug formatting: ID, DLC, and hex data.
            var hex = (Data == null || Data.Length == 0)
                ? ""
                : BitConverter.ToString(Data);

            return $"ID=0x{Id:X} DLC={Dlc} DATA={hex} TS={Timestamp:O}";
        }
    }
}
