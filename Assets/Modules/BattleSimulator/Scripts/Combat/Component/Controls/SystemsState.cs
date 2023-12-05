using System.Collections;

namespace Combat.Component.Controls
{
    public readonly struct SystemsState
    {
        public static SystemsState Create(int count = 0)
        {
            return new SystemsState(new BitArray(count), null);
        }

        public SystemsState(BitArray systems, System.Action dataChangedCallback)
        {
            _systems = systems;
            _dataChangedCallback = dataChangedCallback;
        }

        public int Count => _systems.Count;

        public bool GetState(int index)
        {
            if (index < 0 || index >= _systems.Count) return false;
            return _systems.Get(index);
        }

        public void SetState(int index, bool active)
        {
            if (index < 0) return;
            if (index >= _systems.Count)
            {
                if (!active) return;
                _systems.Length = index + 1;
            }

            _systems[index] = active;
            _dataChangedCallback?.Invoke();
        }

        public void Assign(SystemsState other)
        {
            var source = other._systems;
            if (_systems.Length != source.Length)
                _systems.Length = source.Length;

            _systems.SetAll(false);
            _systems.Or(source);
            _dataChangedCallback?.Invoke();
        }

        public bool this[int index]
        {
            get => GetState(index);
            set => SetState(index, value);
        }

        public void Clear()
        {
            _systems?.SetAll(false);
            _dataChangedCallback?.Invoke();
        }

        private readonly BitArray _systems;
        private readonly System.Action _dataChangedCallback;
    }
}
