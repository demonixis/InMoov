using System;

namespace Demonixis.InMoovSharp.Services
{
    public enum ServoMixageType
    {
        None = 0,
        SameValue,
        InverseValue,
    }

    [Serializable]
    public struct ServoData
    {
        public ServoIdentifier Id;
        public string ServoName;
        public bool Inverse;
        public byte Min;
        public byte Neutral;
        public byte Max;
        public byte Speed;
        public byte PinId;
        public int CardId;
        public bool Enabled;
        public byte Value;
        public byte ScaleValueTo180;
        public byte AutoDisableDelay;

        [NonSerialized]
        public bool Sleeping;
        [NonSerialized]
        public bool Timestamp;

        public ServoIdentifier MixedServo;
        public ServoMixageType MixageType;

        public override string ToString()
        {
            return
                $"{Id} - Min: {Min} / Max: {Max} / Neutral: {Neutral} / Invert: {Inverse} / Card: {CardId} / Pin: {PinId}";
        }

        public static ServoData New(ServoIdentifier servoId)
        {
            return new ServoData
            {
                Id = servoId,
                ServoName = $"{servoId}",
                Inverse = false,
                Min = 0,
                Max = 180,
                Neutral = 90,
                Speed = 1,
                PinId = 1,
                CardId = (int)DevBoardIds.None,
                Enabled = false,
                Value = 90,
                ScaleValueTo180 = 0,
                MixedServo = ServoIdentifier.None,
                MixageType = ServoMixageType.None,
                AutoDisableDelay = 1
            };
        }
    }

    [Serializable]
    public enum ServoIdentifier
    {
        // Eyes
        EyeX = 0,
        EyeY,
        EyelidLeft,
        EyeLidRight,

        // Jaw
        Jaw,

        // Head
        HeadYaw,
        HeadPitch,
        HeadRollPrimary,
        HeadRollSecondary,

        // Torso + Stomach
        PelvisYawPrimary,
        PelvisYawSecondary,
        PelvisPitchPrimary,
        PelvisPitchSecondary,
        PelvisRollPrimary,
        PelvisRollSecondary,

        // Left Shoulder + Arm
        LeftShoulderYaw,
        LeftShoulderPitch,
        LeftShoulderRoll,
        LeftElbowYaw,
        LeftElbowPitch,
        LeftElbowRoll,
        LeftWristYaw,
        LeftWristPitch,
        LeftWristRoll,

        // Left Hand
        LeftFingerThumb,
        LeftFingerIndex,
        LeftFingerMiddle,
        LeftFingerRing,
        LeftFingerPinky,

        // Right Shoulder + Arm
        RightShoulderYaw,
        RightShoulderPitch,
        RightShoulderRoll,
        RightElbowYaw,
        RightElbowPitch,
        RightElbowRoll,
        RightWristYaw,
        RightWristPitch,
        RightWristRoll,

        // Right Hand
        RightFingerThumb,
        RightFingerIndex,
        RightFingerMiddle,
        RightFingerRing,
        RightFingerPinky,

        // Misc
        None
    }
}