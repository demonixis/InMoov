using System;

namespace Demonixis.InMoovSharp.Services
{
    [Serializable]
    public sealed class SerialDataBuffer
    {
        public byte[] DataBuffer { get; private set; }
        public int MaximumServoCount { get; private set; }
        public int PinStart { get; private set; }
        public int PinEnd { get; private set; }

        public SerialDataBuffer(DevBoards boardType)
        {
            DevBoardData.GetPinStartEnd(boardType, out int pinStart, out int pinEnd);
            PinStart = pinStart;
            PinEnd = pinEnd;
            MaximumServoCount = DevBoardData.GetMaximumServo(boardType);
            DataBuffer = new byte[MaximumServoCount];
        }

        public void SetValue(int pinNumber, byte value, bool enabled)
        {
            if (pinNumber < PinStart || pinNumber > PinEnd)
            {
                Robot.Log($"Pin {pinNumber} is not a valid pin number");
                return;
            }

            var index = pinNumber - PinStart;
            DataBuffer[index] = enabled ? value : byte.MaxValue;
        }

        public static byte[] GetClearedBuffer(DevBoards boardType)
        {
            var maximumServoCount = DevBoardData.GetMaximumServo(boardType);
            var buffer = new byte[maximumServoCount];
            for (var i = 0; i < buffer.Length; i++)
                buffer[i] = byte.MaxValue;
            return buffer;
        }

        public void ClearData()
        {
            for (var i = 0; i < DataBuffer.Length; i++)
                DataBuffer[i] = byte.MaxValue;
        }

        public override string ToString()
        {
            return string.Join(":", DataBuffer);
        }
    }
}