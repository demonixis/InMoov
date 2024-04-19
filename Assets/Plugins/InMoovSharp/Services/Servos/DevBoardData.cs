using System;
using System.Drawing;

namespace Demonixis.InMoovSharp.Services
{
    public enum DevBoardIds
    {
        Left = 0,
        Right,
        Card3,
        Card4,
        Card5,
        Card6,
        None
    }

    public enum DevBoards
    {
        ArduinoStandard,
        ArduinoMega,
        ESP32
    }

    public enum DevBoardConnections
    {
        USB,
        Bluetooth,
        BluetoothLE,
        Wifi
    }

    [Serializable]
    public struct DevBoardConnectionData : IEquatable<DevBoardConnectionData>
    {
        public int CardId;
        public string PortName;
        public DevBoards Board;
        public DevBoardConnections BoardConnection;

        public bool Equals(DevBoardConnectionData other)
        {
            return CardId == other.CardId &&
                   PortName == other.PortName &&
                   Board == other.Board &&
                   BoardConnection == other.BoardConnection;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CardId, PortName, Board, BoardConnection);
        }
    }

    public static class DevBoardData
    {
        public const int ArduinoPinStart = 2;
        public const int ArduinoPinEnd = 13;
        public const int ArduinoMegaPinEnd = 53;
        public const int ESP32PinStart = 12;
        public const int ESP32PinEnd = 27;
        public const string BluetoothName = "InMoovSharpBT";
        public const string BluetoothLEName = "InMoovSharpBT-LE";
        public const string BluetoothLEServiceUUID = "4fafc201-1fb5-459e-8fcc-c5c9c331914b";
        public const string BluetoothLECharacteristicUUID = "beb5483e-36e1-4688-b7f5-ea07361b26a8";
        public const string WifiSSID = "InMoovSharpWifi";
        public const string WifiPassword = "inmoovsharp";
        public const string WifiLocalIP = "192.168.1.1";
        public const int DefaultBaudRate = 9600;
        public const int DefaultESP32BaudRate = 115200;

        public static int GetBaudRate(DevBoards boardType)
        {
            if (boardType == DevBoards.ESP32)
                return DefaultESP32BaudRate;

            return DefaultBaudRate;
        }

        public static int GetMaximumServo(DevBoards boardType)
        {
            if (boardType == DevBoards.ArduinoMega)
                return ArduinoMegaPinEnd - ArduinoPinStart;
            else if (boardType == DevBoards.ESP32)
                return ESP32PinEnd - ESP32PinStart;

            return ArduinoPinEnd - ArduinoPinStart;
        }

        public static void GetPinStartEnd(DevBoards boardType, out int pinStart, out int pinEnd)
        {
            if (boardType == DevBoards.ArduinoMega)
            {
                pinStart = ArduinoPinStart;
                pinEnd = ArduinoMegaPinEnd;
            }
            else if (boardType == DevBoards.ESP32)
            {
                pinStart = ESP32PinStart;
                pinEnd = ESP32PinEnd;
            }
            else
            {
                pinStart = ArduinoPinStart;
                pinEnd = ArduinoPinEnd;
            }
        }
    }
}
