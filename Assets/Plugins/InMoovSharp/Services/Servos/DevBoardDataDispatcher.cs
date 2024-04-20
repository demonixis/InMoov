using System;

namespace Demonixis.InMoovSharp.Services
{
    public sealed class DevBoardDataDispatcher
    {
        private DevBoardDataManager _serialBoardManager;
        private DevBoardDataManager _bluetoothManager;
        private DevBoardDataManager _wifiManager;
        private DevBoardDataManager _bluetoothLEManager;

        public event Action<bool, DevBoardData> ConnectionChanged;

        public DevBoardDataDispatcher()
        {
            _serialBoardManager = new SerialPortManager();
            _serialBoardManager.ConnectionChanged += OnConnectionChanged;
            _bluetoothManager = new BluetoothManager();
            _bluetoothManager.ConnectionChanged += OnConnectionChanged;
            _bluetoothLEManager = new BluetoothLEManager();
            _bluetoothLEManager.ConnectionChanged += OnConnectionChanged;
            _wifiManager = new WifiManager();
            _wifiManager.ConnectionChanged += OnConnectionChanged;
        }

        private void OnConnectionChanged(bool connected, DevBoardData devBoard)
        {
            ConnectionChanged?.Invoke(connected, devBoard);
        }

        public void Dispose()
        {
            _serialBoardManager.Dispose();
            _bluetoothManager.Dispose();
            _wifiManager.Dispose();
            _bluetoothLEManager.Dispose();
        }

        public void SetDataManager(DevBoardConnections connection, DevBoardDataManager manager)
        {
            if (connection == DevBoardConnections.USB)
            {
                _serialBoardManager.Dispose();
                _serialBoardManager = manager;
            }
            else if (connection == DevBoardConnections.Bluetooth)
            {
                _bluetoothManager.Dispose();
                _bluetoothManager = manager;
            }
            else if (connection == DevBoardConnections.BluetoothLE)
            {
                _bluetoothLEManager.Dispose();
                _bluetoothLEManager = manager;
            }
            else if (connection == DevBoardConnections.Wifi)
            {
                _wifiManager.Dispose();
                _wifiManager = manager;
            }
        }

        public void SendData(DevBoardData data, SerialDataBuffer buffer)
        {
            var id = data.CardId;
            if (data.BoardConnection == DevBoardConnections.USB)
                _serialBoardManager.SendData(id, buffer);
            else if (data.BoardConnection == DevBoardConnections.Bluetooth)
                _bluetoothManager.SendData(id, buffer);
            else if (data.BoardConnection == DevBoardConnections.BluetoothLE)
                _bluetoothLEManager.SendData(id, buffer);
            else if (data.BoardConnection == DevBoardConnections.Wifi)
                _wifiManager.SendData(id, buffer);
        }
    }
}
