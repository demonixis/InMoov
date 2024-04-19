using Demonixis.InMoovSharp.Settings;
using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace Demonixis.InMoovSharp.Services
{
    [Serializable]
    public sealed class SerialPortManager : DevBoardDataManager
    {
        public const string SerialFilename = "serial.json";
        private Dictionary<DevBoardConnectionData, SerialPort> _serialPorts;
        private bool _disposed;

        public SerialPortManager()
        {
            _serialPorts = new Dictionary<DevBoardConnectionData, SerialPort>();
        }

        public override bool IsConnected(int cardId)
        {
            foreach (var keyValue in _serialPorts)
            {
                if (keyValue.Key.CardId == cardId && keyValue.Value.IsOpen)
                    return true;
            }

            return false;
        }

        public override void Initialize()
        {
            var savedData = SaveGame.LoadData<DevBoardConnectionData[]>(SerialFilename, "Config");

            if (savedData == null || savedData.Length <= 0) return;
            
            foreach (var data in savedData)
                Connect(data);
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_disposed) return;

            var serialData = new DevBoardConnectionData[_serialPorts.Count];
            var i = 0;
            foreach (var keyValue in _serialPorts)
                serialData[i++] = keyValue.Key;

            SaveGame.SaveData(serialData, SerialFilename, "Config");

            foreach (var serial in _serialPorts)
            {
                var serialPort = serial.Value;
                if (serialPort == null) continue;

                if (serialPort.IsOpen)
                {
                    var clearBuffer = SerialDataBuffer.GetClearedBuffer(serial.Key.Board);
                    serialPort.Write(clearBuffer, 0, clearBuffer.Length);
                }
                serial.Value.Dispose();
            }

            _disposed = true;
        }

        public override void SendData(int cardId, SerialDataBuffer buffer)
        {
#if UNITY_EDITOR
            //if (_logFirstTrame && cardId == 0)
                //Robot.Log($"{cardId}_{buffer}");
#endif
            if (TryGetSerialPort(cardId, out SerialPort serialPort))
                serialPort.Write(buffer.DataBuffer, 0, buffer.DataBuffer.Length);
        }

        private bool TryGetSerialData(int cardId, out DevBoardConnectionData serialData)
        {
            if (_serialPorts.Count > 0)
            {
                foreach (var keyValue in _serialPorts)
                {
                    if (keyValue.Key.CardId == cardId)
                    {
                        serialData = keyValue.Key;
                        return true;
                    }
                }
            }
            serialData = new DevBoardConnectionData();
            return false;
        }

        private bool TryGetSerialPort(int cardId, out SerialPort serial)
        {
            if (_serialPorts.Count > 0)
            {
                foreach (var keyValue in _serialPorts)
                {
                    if (keyValue.Key.CardId == cardId)
                    {
                        serial = keyValue.Value;
                        return true;
                    }
                }
            }

            serial = null;
            return false;
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

        public override bool Connect(DevBoardConnectionData serialData)
        {
            int cardId = serialData.CardId;

            if (_serialPorts.ContainsKey(serialData)) return false;

            SerialPort serialPort = null;

            try
            {
                serialPort = new SerialPort(serialData.PortName, DevBoardData.GetBaudRate(serialData.Board));
                serialPort.Open();

                if (serialPort.IsOpen)
                {
                    serialPort.ErrorReceived += (sender, e) => Robot.Log($"Error {e}");
                    serialPort.DataReceived += (sender, e) => { };// Robot.Log($"Data Received: {e}");
                    _serialPorts.Add(serialData, serialPort);
                    NotifyConnectionChanged(true, serialData);
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

        public override void Disconnect(int cardId)
        {
            if (TryGetSerialData(cardId, out DevBoardConnectionData serialData))
            {
                _serialPorts[serialData].Dispose();
                _serialPorts.Remove(serialData);
                NotifyConnectionChanged(false, serialData);
            }
        }
    }
}