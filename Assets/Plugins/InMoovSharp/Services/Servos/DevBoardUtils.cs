using System;

namespace Demonixis.InMoovSharp.Services
{
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

    public enum DevBoardUsages
    {
        ServoManagement,
        Sensors,
        Other
    }

    [Serializable]
    public struct DevBoardData : IEquatable<DevBoardData>
    {
        public int CardId;
        public string ConnectionData;
        public DevBoards Board;
        public DevBoardConnections BoardConnection;
        public DevBoardUsages Usages;

        public bool Equals(DevBoardData other)
        {
            return CardId == other.CardId &&
                   ConnectionData == other.ConnectionData &&
                   Board == other.Board &&
                   BoardConnection == other.BoardConnection &&
                   Usages == other.Usages;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CardId, ConnectionData, Board, BoardConnection, Usages);
        }
    }

    public static class DevBoardUtils
    {
        public const int ArduinoPinStart = 2;
        public const int ArduinoPinEnd = 13;
        public const int ArduinoMegaPinEnd = 53;
        public const int ESP32PinStart = 12;
        public const int ESP32PinEnd = 27;
        public const int MaxSupportedDevBoard = 8;
        public const string DefaultBluetoothName = "InMoovSharpBT";
        public const string DefaultBluetoothLEName = "InMoovSharpBT-LE";
        public const string BluetoothLEServiceUUID = "4fafc201-1fb5-459e-8fcc-c5c9c331914b";
        public const string BluetoothLECharacteristicUUID = "beb5483e-36e1-4688-b7f5-ea07361b26a8";
        public const string WifiLocalIP = "192.168.1.1";
        public const byte WifiLocalPort = 80;
        public const int DefaultArduinoBaudRate = 9600;
        public const int DefaultESP32BaudRate = 115200;

        public static int GetBaudRate(DevBoards boardType)
        {
            if (boardType == DevBoards.ESP32)
                return DefaultESP32BaudRate;

            return DefaultArduinoBaudRate;
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
