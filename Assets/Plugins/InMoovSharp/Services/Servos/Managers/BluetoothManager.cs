using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;

namespace Demonixis.InMoovSharp.Services
{
    [Serializable]
    public sealed class BluetoothManager : DevBoardDataManager
    {
        private Dictionary<int, BluetoothClient> _clients;
        private Dictionary<int, DevBoardData> _devBoardMapping;

        public BluetoothManager()
        {
            _clients = new();
            _devBoardMapping = new();
        }

        public override bool IsConnected(int cardId)
        {
            return _devBoardMapping.ContainsKey(cardId);
        }

        public override void Initialize()
        {
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (var client in _clients.Values)
            {
                client.Dispose();
            }

            _clients.Clear();
            _devBoardMapping.Clear();
        }

        public override void SendData(int cardId, SerialDataBuffer buffer)
        {
            if (_clients.TryGetValue(cardId, out BluetoothClient client))
            {
                var stream = client.GetStream();
                if (stream.CanWrite)
                {
                    var data = buffer.DataBuffer;
                    stream.WriteAsync(data, 0, data.Length);
                }
            }
        }

        public override bool Connect(DevBoardData serialData)
        {
            var cardId = serialData.CardId;
            if (_clients.ContainsKey(cardId))
                return false;

            var bluetoothNetwork = DevBoardUtils.DefaultBluetoothName;

            if (!string.IsNullOrEmpty(serialData.ConnectionData))
            {
                bluetoothNetwork = serialData.ConnectionData;
            }

            var client = new BluetoothClient();

            BluetoothDeviceInfo device = null;
            foreach (var dev in client.DiscoverDevices())
            {
                if (dev.DeviceName.Contains(bluetoothNetwork))
                {
                    device = dev;
                    break;
                }
            }

            if (device == null)
            {
                return false;
            }

            if (!device.Authenticated)
            {
                BluetoothSecurity.PairRequest(device.DeviceAddress, "1234");
            }

            device.Refresh();
            client.Connect(device.DeviceAddress, BluetoothService.SerialPort);

            _clients.Add(cardId, client);
            _devBoardMapping.Add(cardId, serialData);
            NotifyConnectionChanged(true, serialData);

            return true;
        }

        public override void Disconnect(int cardId)
        {
            if (_devBoardMapping.TryGetValue(cardId, out var data))
            {
                var client = _clients[cardId];
                client.Dispose();

                _clients.Remove(cardId);
                _devBoardMapping.Remove(cardId);

                NotifyConnectionChanged(false, data);
            }
        }
    }
}