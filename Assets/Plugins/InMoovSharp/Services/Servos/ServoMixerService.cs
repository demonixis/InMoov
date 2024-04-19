using Demonixis.InMoovSharp.Settings;
using Demonixis.InMoovSharp.Utils;
using System;
using System.Collections;

namespace Demonixis.InMoovSharp.Services
{
    public class ServoMixerService : RobotService
    {
        public const string ServoMixerFilename = "servos.json";

        private DevBoardDataManager? _devBoardManager;
        private ServoData[] _servoData;
        private SerialDataBuffer[] _serialDataBuffer;

        public bool Running { get; protected set; }
        public float UpdateInterval { get; set; } = 1.0f / 30.0f;

        public DevBoardDataManager? DevBoardManager => _devBoardManager;

        public ServoMixerService()
        {
            _devBoardManager = null;

            // Initialize data
            var names = Enum.GetNames(typeof(ServoIdentifier));
            _servoData = new ServoData[names.Length];
            for (var i = 0; i < _servoData.Length; i++)
                _servoData[i] = ServoData.New((ServoIdentifier)i);

            names = Enum.GetNames(typeof(DevBoardIds));
            _serialDataBuffer = new SerialDataBuffer[names.Length];
        }

        public void SetDevBoardDataManager(DevBoardDataManager serialPortManager)
        {
            if (_devBoardManager != null)
            {
                _devBoardManager.Dispose();
                _devBoardManager = null;
            }

            _devBoardManager = serialPortManager;
            _devBoardManager.ConnectionChanged += OnDevBoardManagerConnectionChanged;
            _devBoardManager.Initialize();
        }

        private void OnDevBoardManagerConnectionChanged(bool connected, DevBoardConnectionData data)
        {
            _serialDataBuffer[data.CardId] = connected ? new SerialDataBuffer(data.Board) : null;
        }

        protected override void SafeInitialize()
        {
            // Load saved data and apply them
            var servoMixerData = SaveGame.LoadData<ServoData[]>(ServoMixerFilename, "Config");

            if (servoMixerData != null && servoMixerData.Length == _servoData.Length)
                _servoData = servoMixerData;

            if (_devBoardManager == null)
            {
                var serialPortManager = new SerialPortManager();
                SetDevBoardDataManager(serialPortManager);
            }

            StopAllCoroutines();
            StartCoroutine(ServoLoop());
        }

        public override void SetPaused(bool paused)
        {
            Paused = !paused;

            if (Paused && Running)
                StopAllCoroutines();
            else if (!Paused && !Running)
                StartCoroutine(ServoLoop());
        }

        public override void Dispose()
        {
            SaveGame.SaveData(_servoData, ServoMixerFilename, "Config");
            StopAllCoroutines();
            _devBoardManager?.Dispose();
        }

        private IEnumerator ServoLoop()
        {
            Running = true;

            while (Running)
            {
                // Clear previous values.
                for (var i = 0; i < _serialDataBuffer.Length; i++)
                    _serialDataBuffer[i]?.ClearData();

                // Map new values.
                for (var i = 0; i < _servoData.Length; i++)
                {
                    var data = _servoData[i];
                    var cardIndex = (int)data.CardId;

                    if (cardIndex < 0) continue;

                    // TODO add neutral offset
                    //var offset = 90 - data.Neutral;
                    //data.Value = (byte)(data.Value + offset);

                    ClampServoValue(data.Min, data.Max, ref data.Value);

                    if (data.Inverse)
                        InvertServoValue(ref data.Value);

                    if (data.Enabled && data.MixageType != ServoMixageType.None)
                    {
                        switch (data.MixageType)
                        {
                            case ServoMixageType.SameValue:
                                SetRawServoValue(data.MixedServo, data.Value);
                                break;
                            case ServoMixageType.InverseValue:
                                SetRawServoValue(data.MixedServo, (byte)(180 - data.Value));
                                break;
                        }
                    }

                    _serialDataBuffer[cardIndex]?.SetValue(data.PinId, data.Value, data.Enabled && !data.Sleeping);
                }

                // Send values.
                if (!Paused && _devBoardManager != null)
                {
                    for (var i = 0; i < _serialDataBuffer.Length; i++)
                        _devBoardManager.SendData(i, _serialDataBuffer[i]);
                }

                yield return CoroutineFactory.WaitForSeconds(UpdateInterval);
            }
        }

        public int GetServoValue(ServoIdentifier servoId)
        {
            return _servoData[(int)servoId].Value;
        }

        public void SetServoValueInEuler(ServoIdentifier servoId, float rawValue)
        {
            ref var data = ref _servoData[(int)servoId];
            var value = ServoConverter.UnityRotationToServo(rawValue, data.ScaleValueTo180 > 0);

            data.Value = value;
        }

        public void SetServoValueInServo(ServoIdentifier servoId, byte value)
        {
            ref var data = ref _servoData[(int)servoId];
            data.Value = value;
        }

        public void SetRawServoValue(ServoIdentifier servoId, byte value)
        {
            ref var data = ref _servoData[(int)servoId];
            data.Value = value;
        }

        private void ClampServoValue(byte min, byte max, ref byte value)
        {
            value = Math.Max(min, value);
            value = Math.Min(max, value);
        }

        private void InvertServoValue(ref byte value)
        {
            var inverted = 180 - value;
            value = (byte)inverted;
        }

        public void SetServosToNeutral()
        {
            for (var i = 0; i < _servoData.Length; i++)
            {
                ref var data = ref _servoData[i];
                data.Value = data.Neutral;
            }
        }

        public void SetServoData(ServoIdentifier servoId, ref ServoData data)
        {
            var index = (int)servoId;
            _servoData[index] = data;
        }

        public ServoData GetServoData(ServoIdentifier id)
        {
            return _servoData[(int)id];
        }
    }
}