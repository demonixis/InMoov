namespace Demonixis.InMoovSharp.Utils
{
    public static class ServoConverter
    {
        public static byte UnityRotationToServo(float rotation, bool scaleTo180)
        {
            rotation %= 360;
            rotation += 90; // Center is 0 on Unity but 90 in Arduino

            if (rotation > 180)
                rotation = 360 - rotation;

            return (byte)rotation;
        }
    }
}