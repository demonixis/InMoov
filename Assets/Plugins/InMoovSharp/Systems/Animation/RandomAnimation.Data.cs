using Demonixis.InMoovSharp.Services;
using System;

namespace Demonixis.InMoovSharp.Systems
{
    public partial class RandomAnimation : RobotSystem
    {
        [Serializable]
        public class ServoAnimation
        {
            private int _index;

            public ServoIdentifier Servo;
            public bool RandomRange;
            public byte Min;
            public byte Max;
            public byte[] Sequence;
            public float Frequency;

            public int Cursor
            {
                get => _index;
                set
                {
                    _index = value;

                    if (_index < 0)
                        _index = Sequence.Length - 1;
                    else if (_index > Sequence.Length)
                        _index = 0;
                }
            }

            public static ServoAnimation New(ServoIdentifier id, byte min, byte max, float freq)
            {
                return new ServoAnimation
                {
                    Servo = id,
                    RandomRange = true,
                    Min = min,
                    Max = max,
                    Frequency = freq
                };
            }

            public byte NextValue => (byte)(Sequence != null ? Sequence[Cursor] : 0);
        }
    }
}
