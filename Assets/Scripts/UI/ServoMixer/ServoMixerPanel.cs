using Demonixis.InMoovSharp.Services;
using Demonixis.InMoovUnity;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Demonixis.InMoov.UI
{
    public sealed class ServoMixerPanel : MonoBehaviour
    {
        private ServoMixerService _servoMixerService;
        private ServoIdentifier _currentServo;
        private ServoData _currentData;
        private SliderTextValue[] _sliderTextValues;

        [Header("Servo List"), SerializeField] private RectTransform _servoList;

        [SerializeField] private GameObject _servoItemPrefab;

        [Header("Servo Settings"), SerializeField]
        private TextMeshProUGUI _servoName;

        [SerializeField] private TMP_Dropdown _servoCardId;
        [SerializeField] private TMP_Dropdown _servoPinId;
        [SerializeField] private Toggle _servoEnabled;
        [SerializeField] private Toggle _servoReversed;
        [SerializeField] private Slider _servoNeutral;
        [SerializeField] private Slider _servoMin;
        [SerializeField] private Slider _servoMax;
        [SerializeField] private Slider _servoSpeed;
        [SerializeField] private Slider _servoValue;
        [SerializeField] private TMP_Dropdown _mixedServo;
        [SerializeField] private TMP_Dropdown _mixages;

        private void Start()
        {
            UnityRobotProxy.Instance.OnRobotReady(Initialize);
        }

        private void Initialize(UnityRobotProxy unityRobot)
        {
            var robot = unityRobot.Robot;
            _sliderTextValues = GetComponentsInChildren<SliderTextValue>();
            _currentData = ServoData.New(ServoIdentifier.None);
            _servoMixerService = robot.GetService<ServoMixerService>();

            foreach (Transform child in _servoList)
                Destroy(child.gameObject);

            var names = Enum.GetNames(typeof(ServoIdentifier));

            _mixedServo.options.Clear();

            for (var i = 0; i < names.Length; i++)
            {
                var item = Instantiate(_servoItemPrefab, _servoList);
                ResetTransform(item.transform);

                var servoItem = item.AddComponent<ServoMixerItem>();
                servoItem.Setup((ServoIdentifier) i);
                servoItem.Clicked += OnServoClicked;

                _mixedServo.options.Add(new TMP_Dropdown.OptionData(names[i]));
            }

            names = Enum.GetNames(typeof(ServoMixageType));
            _mixages.options.Clear();
            foreach (var item in names)
                _mixages.options.Add(new TMP_Dropdown.OptionData(item));

            names = Enum.GetNames(typeof(DevBoardIds));
            _servoCardId.options.Clear();
            foreach (var item in names)
                _servoCardId.options.Add(new TMP_Dropdown.OptionData(item));

            _servoPinId.options.Clear();
            for (var i = 2; i <= 63; i++)
                _servoPinId.options.Add(new TMP_Dropdown.OptionData($"Pin #{i}"));

            // Bind events
            _mixedServo.SetValueWithoutNotify((int) _currentData.MixedServo);
            _mixedServo.RefreshShownValue();
            _mixedServo.onValueChanged.AddListener(i =>
            {
                _currentData.MixedServo = (ServoIdentifier) i;
                UpdateDataOnArduino();
            });

            _mixages.SetValueWithoutNotify((int) _currentData.MixageType);
            _mixages.RefreshShownValue();
            _mixages.onValueChanged.AddListener(i =>
            {
                _currentData.MixageType = (ServoMixageType) i;
                UpdateDataOnArduino();
            });

            _servoPinId.SetValueWithoutNotify((int) _currentData.PinId);
            _servoPinId.RefreshShownValue();
            _servoPinId.onValueChanged.AddListener(i =>
            {
                _currentData.PinId = (byte) (_servoPinId.value + 2);
                UpdateDataOnArduino();
            });

            _servoCardId.SetValueWithoutNotify((int) _currentData.CardId);
            _servoCardId.RefreshShownValue();
            _servoCardId.onValueChanged.AddListener(i =>
            {
                _currentData.CardId = i;
                UpdateDataOnArduino();
            });

            _servoEnabled.SetIsOnWithoutNotify(_currentData.Enabled);
            _servoEnabled.onValueChanged.AddListener(b =>
            {
                _currentData.Enabled = b;
                UpdateDataOnArduino();
            });

            _servoReversed.SetIsOnWithoutNotify(_currentData.Inverse);
            _servoReversed.onValueChanged.AddListener(b =>
            {
                _currentData.Inverse = b;
                UpdateDataOnArduino();
            });

            SetupSlider(_servoNeutral, 0, 180, _currentData.Neutral, i =>
            {
                _currentData.Neutral = (byte) i;
                UpdateDataOnArduino();
            });

            SetupSlider(_servoMin, 0, 180, _currentData.Min, i =>
            {
                _currentData.Min = (byte) i;
                UpdateDataOnArduino();
            });

            SetupSlider(_servoMax, 0, 180, _currentData.Max, i =>
            {
                _currentData.Max = (byte) i;
                UpdateDataOnArduino();
            });

            SetupSlider(_servoSpeed, 10, 100, _currentData.Speed, i =>
            {
                _currentData.Speed = (byte) i;
                UpdateDataOnArduino();
            });

            SetupSlider(_servoValue, 0, 180, _currentData.Value, i =>
            {
                _currentData.Value = (byte) i;
                UpdateDataOnArduino();
            });
        }

        private void SetupSlider(Slider slider, float min, float max, float value, UnityAction<float> onValueChanged)
        {
            slider.minValue = min;
            slider.maxValue = max;
            slider.SetValueWithoutNotify(value);
            slider.onValueChanged.AddListener(onValueChanged);
        }

        private void UpdateDataOnArduino()
        {
            _servoMixerService.SetServoData(_currentServo, ref _currentData);
        }

        private void OnServoClicked(ServoIdentifier id)
        {
            var data = _servoMixerService.GetServoData(id);
            UpdateServoDetails(id, data);
        }

        private void UpdateServoDetails(ServoIdentifier id, ServoData data)
        {
            _currentServo = id;
            _currentData = data;

            _servoName.text = $"{id}";
            _servoCardId.SetValueWithoutNotify(data.CardId);
            _servoEnabled.SetIsOnWithoutNotify(data.Enabled);
            _servoReversed.SetIsOnWithoutNotify(data.Inverse);
            _servoNeutral.SetValueWithoutNotify(data.Neutral);
            _servoMin.SetValueWithoutNotify(data.Min);
            _servoMax.SetValueWithoutNotify(data.Max);
            _servoSpeed.SetValueWithoutNotify(data.Speed);
            _servoValue.SetValueWithoutNotify(data.Value);

            var pinValue = FindPinValueFromPinId(data.PinId);
            _servoPinId.SetValueWithoutNotify(pinValue);
            _servoPinId.RefreshShownValue();

            _servoCardId.SetValueWithoutNotify(data.CardId);
            _servoCardId.RefreshShownValue();

            _mixages.SetValueWithoutNotify((int) data.MixageType);
            _mixedServo.SetValueWithoutNotify((int) data.MixedServo);

            foreach (var item in _sliderTextValues)
                item.RefreshValue();
        }

        private int FindPinValueFromPinId(int pinId)
        {
            for (var i = 0; i < _servoPinId.options.Count; i++)
            {
                var tmp = _servoPinId.options[i].text.Split('#');
                if (tmp.Length != 2)
                {
                    Debug.LogException(new UnityException($"The item {_servoPinId.options[i]} hasn't a valid name."));
                }

                var value = int.Parse(tmp[1]);
                if (value == pinId)
                    return i;
            }

            return 0;
        }

        public static void ResetTransform(Transform target)
        {
            target.localPosition = Vector3.zero;
            target.localRotation = Quaternion.identity;
            target.localScale = Vector3.one;
        }

        public void SetEveryoneToNeutral()
        {
            SetServoValue(data => data.Neutral);
        }

        public void SetEveryoneToMin()
        {
            SetServoValue(data => data.Min);
        }

        public void SetEveryoneToMax()
        {
            SetServoValue(data => data.Max);
        }

        public void ComputeNeutrals()
        {
            SetServoValue(data => (byte)(data.Max - data.Min));
        }

        private void SetServoValue(Func<ServoData, byte> getValueCallback)
        {
            var names = Enum.GetNames(typeof(ServoIdentifier));

            for (var i = 0; i < names.Length; i++)
            {
                var data = _servoMixerService.GetServoData((ServoIdentifier)i);
                data.Value = getValueCallback(data);
                _servoMixerService.SetServoData((ServoIdentifier)i, ref data);
            }
        }
    }
}