using Demonixis.InMoovSharp.Settings;
using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace Demonixis.InMoovSharp.Services
{
    [Serializable]
    public struct SerialData
    {
        public int CardId;
        public string PortName;
    }

    public enum ArduinoIdentifiers
    {
        Left = 0,
        Right,
        Card3,
        Card4,
        Card5,
        Card6,
        None
    }

    [Serializable]
    public sealed class SerialPortManager
    {
        public const int PinStart = 2;
        public const int PinEndMega = 53;
        public const string SerialFilename = "serial.json";
        public const int DefaultBaudRate = 9600;
        private Dictionary<int, SerialPort> _serialPorts;
        private bool _disposed;

        public static int MaximumServoCount => PinEndMega - PinStart;

        public SerialPortManager()
        {
            _serialPorts = new Dictionary<int, SerialPort>();
        }

        public bool IsConnected(int cardId)
        {
            return _serialPorts.ContainsKey(cardId) && _serialPorts[cardId].IsOpen;
        }

        public void Initialize()
        {
            var savedData = SaveGame.LoadData<SerialData[]>(SerialFilename, "Config");

            if (savedData == null || savedData.Length <= 0) return;
            foreach (var data in savedData)
                Connect(data.CardId, data.PortName);
        }

        public void Dispose()
        {
            if (_disposed) return;

            var serialData = new SerialData[_serialPorts.Count];
            var i = 0;
            foreach (var keyValue in _serialPorts)
            {
                serialData[i++] = new SerialData
                {
                    CardId = keyValue.Key,
                    PortName = keyValue.Value.PortName
                };
            }

            SaveGame.SaveData(serialData, SerialFilename, "Config");

            var clearBuffer = SerialDataBuffer.GetClearedBuffer();
            foreach (var serial in _serialPorts)
            {
                var serialPort = serial.Value;
                if (serialPort == null) continue;

                if (serialPort.IsOpen)
                    serialPort.Write(clearBuffer, 0, clearBuffer.Length);

                serial.Value.Dispose();
            }

            _disposed = true;
        }

        public void SendData(int cardId, SerialDataBuffer buffer)
        {
#if UNITY_EDITOR
            //if (_logFirstTrame && cardId == 0)
                //Robot.Log($"{cardId}_{buffer}");
#endif

            if (_serialPorts == null || !_serialPorts.ContainsKey(cardId)) return;

            var serialPort = _serialPorts[cardId];
            if (serialPort == null) return;
            serialPort.Write(buffer.DataBuffer, 0, buffer.DataBuffer.Length);
        }

        private void Update()
        {
            if (_serialPorts == null) return;

            foreach (var sp in _serialPorts)
            {
                if (sp.Value == null) continue;

                var result = sp.Value.ReadExisting();
#if UNITY_EDITOR
                //if (_logArduino && !string.IsNullOrEmpty(result))
                    //Robot.Log(result);
#endif
            }
        }

        public bool Connect(int cardId, string serialName)
        {
            if (_serialPorts.ContainsKey(cardId)) return false;

            SerialPort serialPort = null;

            try
            {
                serialPort = new SerialPort(serialName, DefaultBaudRate);
                serialPort.Open();

                if (serialPort.IsOpen)
                {
                    serialPort.ErrorReceived += (sender, e) => Robot.Log($"Error {e}");
                    serialPort.DataReceived += (sender, e) => { };// Robot.Log($"Data Received: {e}");
                    _serialPorts.Add(cardId, serialPort);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Robot.Log(ex.Message);
            }

            if (serialPort != null)
            {
                if (serialPort.IsOpen)
                    serialPort.Close();

                serialPort.Dispose();
            }

            return false;
        }

        public void Disconnect(int cardId)
        {
            if (!_serialPorts.ContainsKey(cardId)) return;

            _serialPorts[cardId].Dispose();
            _serialPorts.Remove(cardId);
        }
    }
}