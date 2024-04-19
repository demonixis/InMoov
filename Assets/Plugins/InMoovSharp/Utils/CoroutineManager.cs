using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demonixis.InMoovSharp.Utils
{
    public class CoroutineManager
    {
        private Dictionary<object, List<IEnumerator>> _userRoutines;
        private bool _updating;

        public CoroutineManager()
        {
            _userRoutines = new Dictionary<object, List<IEnumerator>>();
        }

        public async void Start(object owner, IEnumerator routine)
        {
            while (_updating)
                await Task.Delay(1);

            if (!_userRoutines.ContainsKey(owner))
                _userRoutines.Add(owner, new List<IEnumerator>());

            _userRoutines[owner].Add(routine);
        }

        public async void Stop(object owner, IEnumerator routine)
        {
            while (_updating)
                await Task.Delay(1);

            if (!_userRoutines.ContainsKey(owner)) return;
            _userRoutines[owner].Remove(routine);
        }

        public async void StopAll(object owner)
        {
            while (_updating)
                await Task.Delay(1);

            if (!_userRoutines.ContainsKey(owner)) return;
            _userRoutines[owner].Clear();
        }

        public async void ClearAll()
        {
            while (_updating)
                await Task.Delay(1);

            _userRoutines.Clear();
        }

        public void Update()
        {
            _updating = true;

            foreach (var keyValue in _userRoutines)
            {
                UpdateRoutines(keyValue.Value);
            }

            _updating = false;
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
