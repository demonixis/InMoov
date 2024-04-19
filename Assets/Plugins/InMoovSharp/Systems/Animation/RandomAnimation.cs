using Demonixis.InMoovSharp.Services;
using Demonixis.InMoovSharp.Utils;
using InMoov.Core.Utils;
using System;
using System.Collections;

namespace Demonixis.InMoovSharp.Systems
{
    public partial class RandomAnimation : RobotSystem
    {
        private ServoMixerService _servoMixerService;
        private GesturePlayer _gesturePlayer;

        public ServoAnimation[] ServoActions;
        public bool RandomHandGestures { get; set; } = true;
        public bool RandomArmGestures { get; set; } = true;
        public float[] HandGestureFrequency { get; set; } = new[] { 1.5f, 6.5f };
        public float[] ArmGestureFrequency { get; set; } = new[] { 2.5f, 6.5f };

        public RandomAnimation()
        {
            ServoActions = SetupDefaultValues();
        }

        public override void Dispose()
        {
            base.Dispose();
            StopAllCoroutines();
        }

        protected override void SafeInitialize()
        {
            if (_servoMixerService == null)
            {
                _servoMixerService = Robot.Instance.GetService<ServoMixerService>();
                _gesturePlayer = new GesturePlayer(_servoMixerService);
            }

            foreach (var action in ServoActions)
                StartCoroutine(PlayAnimationAction(action));

            if (RandomHandGestures)
                StartCoroutine(PlayHandGesture());

            if (RandomArmGestures)
                StartCoroutine(PlayArmGesture());

            Started = true;
        }

        private IEnumerator PlayAnimationAction(ServoAnimation servoAnimation)
        {
            while (Started)
            {
                var value = servoAnimation.RandomRange
                    ? (byte)RandomHelper.Range(servoAnimation.Min, servoAnimation.Max)
                    : servoAnimation.NextValue;

                _servoMixerService.SetServoValueInServo(servoAnimation.Servo, value);

                yield return CoroutineFactory.WaitForSeconds(servoAnimation.Frequency);
            }
        }

        private IEnumerator PlayHandGesture()
        {
            var names = Enum.GetNames(typeof(HandGestures));

            while (Started)
            {
                var side = RandomHelper.Range(0, 100) % 3;
                var gestureId = RandomHelper.Range(0, 100) % names.Length;
                var gesture = (HandGestures)gestureId;

                if (side == 2)
                {
                    _gesturePlayer.ApplyHandGesture(true, gesture);
                    _gesturePlayer.ApplyHandGesture(false, gesture);
                }
                else
                {
                    _gesturePlayer.ApplyHandGesture(side == 0, gesture);
                }

                yield return CoroutineFactory.WaitForSeconds(RandomHelper.Range(HandGestureFrequency[0], HandGestureFrequency[1]));
            }
        }

        private IEnumerator PlayArmGesture()
        {
            var names = Enum.GetNames(typeof(ArmGestures));

            while (Started)
            {
                var side = RandomHelper.Range(0, 100) % 3;
                var gestureId = RandomHelper.Range(0, 100) % names.Length;
                var gesture = (ArmGestures)gestureId;

                if (side == 2)
                {
                    _gesturePlayer.ApplyArmGesture(true, gesture);
                    _gesturePlayer.ApplyArmGesture(false, gesture);
                }
                else
                {
                    _gesturePlayer.ApplyArmGesture(side == 0, gesture);
                }

                yield return CoroutineFactory.WaitForSeconds(RandomHelper.Range(ArmGestureFrequency[0], ArmGestureFrequency[1]));
            }
        }

        public ServoAnimation[] SetupDefaultValues()
        {
            return new[]
            {
                ServoAnimation.New(ServoIdentifier.EyeX, 0, 180, 3.0f),
                ServoAnimation.New(ServoIdentifier.EyeY, 0, 180, 3.2f),
                ServoAnimation.New(ServoIdentifier.HeadPitch, 0, 180, 5f),
                ServoAnimation.New(ServoIdentifier.HeadYaw, 0, 180, 4f),
                ServoAnimation.New(ServoIdentifier.PelvisPitchPrimary, 80, 120, 4.5f),
                ServoAnimation.New(ServoIdentifier.LeftShoulderYaw, 45, 165, 4.5f),
                ServoAnimation.New(ServoIdentifier.LeftShoulderPitch, 85, 165, 4.5f),
                ServoAnimation.New(ServoIdentifier.LeftShoulderRoll, 0, 165, 4.5f),
                ServoAnimation.New(ServoIdentifier.LeftElbowPitch, 0, 180, 4.5f),
                ServoAnimation.New(ServoIdentifier.LeftWristRoll, 0, 180, 4.5f),
                ServoAnimation.New(ServoIdentifier.RightShoulderYaw, 45, 165, 4.5f),
                ServoAnimation.New(ServoIdentifier.RightShoulderPitch, 85, 165, 4.5f),
                ServoAnimation.New(ServoIdentifier.RightShoulderRoll, 0, 180, 4.5f),
                ServoAnimation.New(ServoIdentifier.RightElbowPitch, 0, 180, 4.5f),
                ServoAnimation.New(ServoIdentifier.RightWristRoll, 0, 180, 4.5f),
            };
        }
    }
}