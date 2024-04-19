using System;

namespace Demonixis.InMoovSharp.Services
{
    public abstract class DevBoardDataManager
    {
        public event Action<bool, DevBoardConnectionData> ConnectionChanged;

        public abstract bool IsConnected(int cardId);
        public abstract void Initialize();
        public abstract void SendData(int cardId, SerialDataBuffer buffer);
        public abstract bool Connect(DevBoardConnectionData serialData);
        public abstract void Disconnect(int cardId);

        public virtual void Dispose()
        {
            ConnectionChanged = null;
        }

        protected void NotifyConnectionChanged(bool connected, DevBoardConnectionData data)
        {
            ConnectionChanged?.Invoke(connected, data);
        }
    }
}
