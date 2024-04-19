using Demonixis.InMoovSharp.Services;
using Demonixis.InMoovSharp.Utils;
using System.Collections;

namespace Demonixis.InMoovSharp.Systems
{
    public class JawMechanism : RobotSystem
    {
        private ServoMixerService _servoMixerService;

        public bool IsSpeaking { get; private set; }
        public float JawOpenTime { get; set; } = 0.15f;
        public float JawCloseTime { get; set; } = 0.1f;
        public byte JawAmplitude { get; set; } = 45;
        public byte JawNeutralOffset { get; set; } = 5;

        protected override void SafeInitialize()
        {
            var robot = Robot.Instance;
            _servoMixerService = robot.GetService<ServoMixerService>();

            var speechSynthesis = robot.GetService<SpeechSynthesisService>();
            speechSynthesis.SpeechStarted += SpeechSynthesisOnSpeechStarted;
            speechSynthesis.SpeechJustFinished += SpeechSynthesisOnSpeechFinished;
        }

        private void SpeechSynthesisOnSpeechStarted(string message)
        {
            if (IsSpeaking)
            {
                StopAllCoroutines();
            }

            StartCoroutine(MoveJaw(message));
        }

        private void SpeechSynthesisOnSpeechFinished()
        {
            IsSpeaking = false;
        }

        private IEnumerator MoveJaw(string sentence)
        {
            if (string.IsNullOrEmpty(sentence)) yield break;

            IsSpeaking = true;

            var data = _servoMixerService.GetServoData(ServoIdentifier.Jaw);
            var openMouthValue = (byte)(data.Neutral + JawAmplitude);
            var closeMouthValue = (byte)(data.Neutral + JawNeutralOffset);

            _servoMixerService.SetServoValueInServo(ServoIdentifier.Jaw, data.Neutral);

            while (IsSpeaking)
            {
                _servoMixerService.SetServoValueInServo(ServoIdentifier.Jaw, openMouthValue);
                yield return CoroutineFactory.WaitForSeconds(JawOpenTime);
                _servoMixerService.SetServoValueInServo(ServoIdentifier.Jaw, closeMouthValue);
                yield return CoroutineFactory.WaitForSeconds(JawCloseTime);
            }

            _servoMixerService.SetServoValueInServo(ServoIdentifier.Jaw, data.Neutral);
        }
    }
}