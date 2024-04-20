using Demonixis.InMoovSharp.Settings;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

namespace Demonixis.InMoovSharp.Services
{
    [Serializable]
    public sealed class SerialPortManager : DevBoardDataManager
    {
        public const string SerialFilename = "serial.json";
        private Dictionary<int, SerialPort> _serialPorts;
        private Dictionary<int, DevBoardData> _devBoardMapping;
        private bool _disposed;

        public SerialPortManager()
        {
            _serialPorts = new();
            _devBoardMapping = new();
        }

        public override bool IsConnected(int cardId)
        {
            if (!_serialPorts.ContainsKey(cardId))
                return false;

            return _serialPorts[cardId].IsOpen;
        }

        public override void Initialize()
        {
            var savedData = SaveGame.LoadData<DevBoardData[]>(SerialFilename, "Config");

            if (savedData == null || savedData.Length <= 0) return;

            foreach (var data in savedData)
                Connect(data);
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_disposed) return;

            var serialData = _devBoardMapping.Values.ToArray();    
            SaveGame.SaveData(serialData, SerialFilename, "Config");

            foreach (var serial in _serialPorts)
            {
                var serialPort = serial.Value;
                if (serialPort == null) continue;

                if (serialPort.IsOpen)
                {
                    var board = _devBoardMapping[serial.Key].Board;
                    var clearBuffer = SerialDataBuffer.GetClearedBuffer(board);
                    serialPort.Write(clearBuffer, 0, clearBuffer.Length);
                }
                serial.Value.Dispose();
            }

            _serialPorts.Clear();
            _devBoardMapping.Clear();
            _disposed = true;
        }

        public override void SendData(int cardId, SerialDataBuffer buffer)
        {
#if UNITY_EDITOR
            //if (_logFirstTrame && cardId == 0)
            //Robot.Log($"{cardId}_{buffer}");
#endif
            if (_serialPorts.TryGetValue(cardId, out SerialPort serialPort))
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

        public override bool Connect(DevBoardData serialData)
        {
            var cardId = serialData.CardId;

            if (_serialPorts.ContainsKey(cardId))
                return false;

            SerialPort serialPort = null;

            try
            {
                serialPort = new SerialPort(serialData.ConnectionData, DevBoardUtils.GetBaudRate(serialData.Board));
                serialPort.Open();

                if (serialPort.IsOpen)
                {
                    serialPort.ErrorReceived += (sender, e) => Robot.Log($"Error {e}");
                    serialPort.DataReceived += (sender, e) => { };// Robot.Log($"Data Received: {e}");
                    _serialPorts.Add(cardId, serialPort);
                    _devBoardMapping.Add(cardId, serialData);
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
            if (_devBoardMapping.TryGetValue(cardId, out DevBoardData serialData))
            {
                _serialPorts[cardId].Dispose();
                _serialPorts.Remove(cardId);
                _devBoardMapping.Remove(cardId);
                NotifyConnectionChanged(false, serialData);
            }
        }
    }
}