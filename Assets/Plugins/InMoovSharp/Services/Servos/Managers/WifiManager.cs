using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Demonixis.InMoovSharp.Services
{
    [Serializable]
    public sealed class WifiManager : DevBoardDataManager
    {
        private Dictionary<int, TcpClient> _clients;
        private Dictionary<int, DevBoardData> _devBoardMapping;

        public WifiManager()
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
            if (_clients.TryGetValue(cardId, out TcpClient client))
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

            var ip = DevBoardUtils.WifiLocalIP;
            var port = DevBoardUtils.WifiLocalPort;

            if (!string.IsNullOrEmpty(serialData.ConnectionData))
            {
                var tmp = serialData.ConnectionData.Split(':');
                if (tmp.Length == 2)
                {
                    if (IPAddress.TryParse(tmp[0], out var address))
                        ip = address.ToString();

                    byte.TryParse(tmp[1], out port);
                }
            }

            var client = new TcpClient(ip, port);
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