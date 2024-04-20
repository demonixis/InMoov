using System;

namespace Demonixis.InMoovSharp.Services
{
    public abstract class DevBoardDataManager
    {
        public event Action<bool, DevBoardData> ConnectionChanged;

        public abstract bool IsConnected(int cardId);
        public abstract void Initialize();
        public abstract void SendData(int cardId, SerialDataBuffer buffer);
        public abstract bool Connect(DevBoardData serialData);
        public abstract void Disconnect(int cardId);

        public virtual void Dispose()
        {
            ConnectionChanged = null;
        }

        protected void NotifyConnectionChanged(bool connected, DevBoardData data)
        {
            ConnectionChanged?.Invoke(connected, data);
        }
    }
}
