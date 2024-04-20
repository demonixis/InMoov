using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;

namespace Demonixis.InMoovSharp.Services
{
    [Serializable]
    public sealed class BluetoothLEManager : DevBoardDataManager
    {
        private Dictionary<int, DevBoardData> _devBoardMapping;

        public BluetoothLEManager()
        {
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

            _devBoardMapping.Clear();
        }

        public override void SendData(int cardId, SerialDataBuffer buffer)
        {
        }

        public override bool Connect(DevBoardData serialData)
        {
            return false;
        }

        public override void Disconnect(int cardId)
        {
        }
    }
}