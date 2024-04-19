using Demonixis.InMoovSharp.Services;

namespace Demonixis.InMoovSharp.Systems
{
    public class GesturePlayer
    {
        private ServoMixerService _servoMixerService;

        public GesturePlayer(ServoMixerService servoMixerService)
        {
            _servoMixerService = servoMixerService;
        }

        #region Hand Gesture Management

        public void ApplyHandGesture(bool left, HandGestures gesture)
        {
            switch (gesture)
            {
                case HandGestures.Close:
                    ApplyHandGesture(left, 1, 1, 1, 1, 1);
                    break;
                case HandGestures.Open:
                    ApplyHandGesture(left, 0, 0, 0, 0, 0);
                    break;
                case HandGestures.Point:
                    ApplyHandGesture(left, 1, 0, 1, 1, 1);
                    break;
                case HandGestures.Rock:
                    ApplyHandGesture(left, 0, 0, 1, 1, 0);
                    break;
                case HandGestures.ThumbUp:
                    ApplyHandGesture(left, 0, 1, 1, 1, 1);
                    break;
                case HandGestures.ThumbIndexUp:
                    ApplyHandGesture(left, 0, 0, 1, 1, 1);
                    break;
                case HandGestures.Fuck:
                    ApplyHandGesture(left, 1, 1, 0, 1, 1);
                    break;
                case HandGestures.Grab:
                    ApplyHandGesture(left, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f);
                    break;
                case HandGestures.Bunny:
                    ApplyHandGesture(left, 1, 0, 0, 1, 1);
                    break;
            }
        }

        private void ApplyHandGesture(bool left, float thumb, float index, float middle, float ring, float pinky)
        {
            var fingers = new[]
            {
                left ? ServoIdentifier.LeftFingerThumb : ServoIdentifier.RightFingerThumb,
                left ? ServoIdentifier.LeftFingerIndex : ServoIdentifier.RightFingerIndex,
                left ? ServoIdentifier.LeftFingerMiddle : ServoIdentifier.RightFingerMiddle,
                left ? ServoIdentifier.LeftFingerRing : ServoIdentifier.RightFingerRing,
                left ? ServoIdentifier.LeftFingerPinky : ServoIdentifier.RightFingerPinky
            };

            var values = new float[] { thumb, index, middle, ring, pinky };
            for (var i = 0; i < fingers.Length; i++)
                _servoMixerService.SetServoValueInServo(fingers[i], (byte)(values[i] * 180));
        }

        #endregion

        #region Arm Gesture Management

        private void OnArmGestureClicked(int obj)
        {
            var gesture = (ArmGestures)obj;
            ApplyArmGesture(true, gesture);
            ApplyArmGesture(false, gesture);
        }

        public void ApplyArmGesture(bool left, ArmGestures gesture)
        {
            switch (gesture)
            {

            }
        }

        private void ApplyArmGesture(bool left, float shoulderYaw, float shoulderPitch, float shoulderRoll, float elbowPitch, float wristRoll)
        {
            var bones = new[]
            {
                left ? ServoIdentifier.LeftShoulderYaw : ServoIdentifier.RightShoulderYaw,
                left ? ServoIdentifier.LeftShoulderPitch : ServoIdentifier.RightShoulderPitch,
                left ? ServoIdentifier.LeftShoulderRoll : ServoIdentifier.RightShoulderRoll,
                left ? ServoIdentifier.LeftElbowPitch : ServoIdentifier.RightElbowPitch,
                left ? ServoIdentifier.LeftWristRoll : ServoIdentifier.RightWristRoll
            };

            var values = new float[] { shoulderYaw, shoulderPitch, shoulderRoll, elbowPitch, wristRoll };
            for (var i = 0; i < bones.Length; i++)
                _servoMixerService.SetServoValueInServo(bones[i], (byte)(values[i] * 180));
        }

        #endregion
    }
}
