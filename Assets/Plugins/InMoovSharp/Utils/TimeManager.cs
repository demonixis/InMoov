using System.Diagnostics;

namespace Demonixis.InMoovSharp.Utils
{
    public sealed class TimeManager
    {
        private static TimeManager _instance;
        private Stopwatch _watch;
        private float _lastTime;

        public float DeltaTime { get; private set; }
        public float TimeScale { get; set; } = 1.0f;
        public float RealtimeSinceStartup => _watch.ElapsedMilliseconds / 1000.0f;

        public static TimeManager Instance
        {
            get
            {
                _instance ??= new TimeManager();
                return _instance;
            }
        }

        public TimeManager()
        {
            _watch = new Stopwatch();
            _watch.Start();
        }

        public void Update()
        {
            var current = RealtimeSinceStartup;
            DeltaTime = (current - _lastTime) * TimeScale;
            _lastTime = current;
        }
    }
}
