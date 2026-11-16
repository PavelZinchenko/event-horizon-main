using Constructor.Satellites;
using System.Collections;
using System.Collections.Generic;

namespace Constructor.Ships
{
    public class SatelliteCollection : ISatelliteCollection
    {
        private readonly List<ISatellite> _satellites = new();
        private bool _dataChanged;

        public int UsedMountPoints => _satellites.Count;

        public ISatellite this[int index]
        {
            get
            {
                if (index < 0 || index >= _satellites.Count) return null;
                return _satellites[index];
            }
            set
            {
                if (index < 0)
                    throw new System.IndexOutOfRangeException();

                for (int i = _satellites.Count; i <= index; ++i)
                    _satellites.Add(null);

                if (_satellites[index] == value) return;
                _satellites[index] = value;
                _dataChanged = true;
            }
        }

        public bool DataChanged
        {
            get
            {
                if (_dataChanged)
                    return true;

                foreach (var satellite in _satellites)
                    if (satellite != null && satellite.DataChanged)
                        return true;

                return false;
            }
            set
            {
                _dataChanged = value;
                if (_dataChanged)
                    return;

                foreach (var satellite in _satellites)
                    if (satellite != null)
                        satellite.DataChanged = false;
            }
        }

        public IEnumerator<ISatellite> GetEnumerator() => _satellites.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _satellites.GetEnumerator();
    }
}
