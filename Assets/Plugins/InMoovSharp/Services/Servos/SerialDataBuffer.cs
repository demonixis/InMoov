using System;

namespace Demonixis.InMoovSharp.Services
{
    [Serializable]
    public sealed class SerialDataBuffer
    {
        public byte[] DataBuffer { get; private set; }

        public SerialDataBuffer()
        {
            DataBuffer = new byte[SerialPortManager.MaximumServoCount];
        }

        public void SetValue(int pinNumber, byte value, bool enabled)
        {
            if (pinNumber < SerialPortManager.PinStart || pinNumber > SerialPortManager.PinEndMega)
            {
                Robot.Log($"Pin {pinNumber} is not a valid pin number");
                return;
            }

            var index = pinNumber - SerialPortManager.PinStart;
            DataBuffer[index] = enabled ? value : byte.MaxValue;
        }

        public static byte[] GetClearedBuffer()
        {
            var buffer = new byte[SerialPortManager.MaximumServoCount];
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