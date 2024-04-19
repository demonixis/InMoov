using System.Collections;
using System.Collections.Generic;

namespace Demonixis.InMoovSharp.Utils
{
    public class CoroutineManager
    {
        private Dictionary<object, List<IEnumerator>> _userRoutines;

        public CoroutineManager()
        {
            _userRoutines = new Dictionary<object, List<IEnumerator>>();
        }

        public void Start(object owner, IEnumerator routine)
        {
            if (!_userRoutines.ContainsKey(owner))
                _userRoutines.Add(owner, new List<IEnumerator>());

            _userRoutines[owner].Add(routine);
        }

        public void Stop(object owner, IEnumerator routine)
        {
            if (!_userRoutines.ContainsKey(owner)) return;
            _userRoutines[owner].Remove(routine);
        }

        public void StopAll(object owner)
        {
            if (!_userRoutines.ContainsKey(owner)) return;
            _userRoutines[owner].Clear();
        }

        public void ClearAll()
        {
            _userRoutines.Clear();
        }

        public void Update()
        {
            foreach (var keyValue in _userRoutines)
            {
                UpdateRoutines(keyValue.Value);
            }
        }

        private void UpdateRoutines(List<IEnumerator> routines)
        {
            if (routines.Count == 0) return;

            for (var i = 0; i < routines.Count; i++)
            {
                if (routines[i].Current is IEnumerator)
                {
                    if (MoveNext((IEnumerator)routines[i].Current))
                        continue;
                }

                if (!routines[i].MoveNext())
                    routines.RemoveAt(i--);
            }
        }

        private bool MoveNext(IEnumerator routine)
        {
            if (routine.Current is IEnumerator)
            {
                if (MoveNext((IEnumerator)routine.Current))
                    return true;
            }

            return routine.MoveNext();
        }
    }
}
