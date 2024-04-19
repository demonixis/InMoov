using System;
using System.Collections;

namespace Demonixis.InMoovSharp.Systems
{
    public abstract class RobotSystem : IDisposable
    {
        public bool Started { get; protected set; }

        public virtual void Initialize()
        {
            if (Started) return;
            SafeInitialize();
            Started = true;
        }

        protected virtual void SafeInitialize()
        {
        }

        public virtual void Dispose()
        {
            Started = false;
        }

        public void SetActive(bool active)
        {
            Robot.Instance.WhenStarted(() =>
            {
                InternalSetActive(active);
            });
        }

        private void InternalSetActive(bool active)
        {
            switch (active)
            {
                case true when !Started:
                    Initialize();
                    break;
                case false when Started:
                    Dispose();
                    break;
            }

            Started = active;
        }

        protected void StartCoroutine(IEnumerator coroutine)
        {
            Robot.Instance.CoroutineManager.Start(this, coroutine);
        }

        protected void StopAllCoroutines()
        {
            Robot.Instance.CoroutineManager.StopAll(this);
        }
    }
}