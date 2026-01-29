using System;

namespace Common.Can
{
    /// <summary>
    /// Enumerates supported door states for the CAN door status frame.
    /// </summary>
    public enum DoorState : byte
    {
        /// <summary>Door is closed.</summary>
        Closed = 0,
        /// <summary>Door is open.</summary>
        Open = 1,
        /// <summary>Door movement is obstructed.</summary>
        Obstructed = 2
    }

    /// <summary>
    /// Helper for building and parsing the standardized door state CAN frame.
    /// </summary>
    /// <remarks>
    /// Contract (Classic CAN):
    /// - Identifier: 0x200 + doorId (standard 11-bit ID).
    /// - DLC: 2 bytes.
    /// - Data[0]: doorId (0..255).
    /// - Data[1]: DoorState enum value.
    /// This layout is intentionally simple so future PCAN integrations can mirror it.
    /// </remarks>
    public static class DoorStateFrame
    {
        /// <summary>Base CAN identifier for door state frames.</summary>
        public const uint BaseId = 0x200u;

        /// <summary>Payload length in bytes.</summary>
        public const byte PayloadLength = 2;

        /// <summary>
        /// Builds a door state frame for the provided door and state.
        /// </summary>
        public static CanFrame Create(byte doorId, DoorState state)
        {
            return new CanFrame
            {
                Id = BaseId + doorId,
                Dlc = PayloadLength,
                Data = new[] { doorId, (byte)state },
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Attempts to parse a door state frame, validating identifier, payload, and enum value.
        /// </summary>
        public static bool TryParse(CanFrame frame, out byte doorId, out DoorState state)
        {
            doorId = 0;
            state = DoorState.Closed;

            if (frame == null)
            {
                return false;
            }

            if (frame.Data == null || frame.Data.Length < PayloadLength || frame.Dlc < PayloadLength)
            {
                return false;
            }

            if (frame.Id < BaseId || frame.Id > BaseId + byte.MaxValue)
            {
                return false;
            }

            doorId = (byte)(frame.Id - BaseId);

            if (frame.Data[0] != doorId)
            {
                return false;
            }

            if (!Enum.IsDefined(typeof(DoorState), frame.Data[1]))
            {
                return false;
            }

            state = (DoorState)frame.Data[1];
            return true;
        }
    }
}
