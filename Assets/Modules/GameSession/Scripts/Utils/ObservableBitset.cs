using System.Collections.Generic;

namespace Session.Utils
{
	public struct ObservableBitset
	{
		private readonly List<uint> _indices; // sorted list of ids where value changes. Last value is always true
		private readonly IDataChangedCallback _callback;

		public int LastIndex => (int)GetLastIndex() - 1;
		public int GroupCount => _indices.Count;

		public bool Get(uint key)
		{
			FindGroup(key, out bool value);
			return value;
		}

		public bool Set(uint key, bool value)
		{
            return value ? Add(key) : Remove(key);
		}

		public bool Add(uint key)
		{
			var group = FindGroup(key, out bool value);
			if (value) return false;

			if (group == _indices.Count)
				AddNewBit(key);
			else
				InvertBit(group, key);

            return true;
		}

		public bool Remove(uint key)
		{
			var group = FindGroup(key, out bool value);
			if (!value) return false;

			if (group <= _indices.Count)
				InvertBit(group, key);

            return true;
		}

		public void Clear() => _indices.Clear();

		public override string ToString()
		{
			var sb = new System.Text.StringBuilder();
			uint lastValue = 0;
			foreach (var value in _indices)
			{
				sb.Append(value - lastValue);
				sb.Append('.');
				lastValue = value;
			}

			return sb.ToString();
		}

        public bool Equals(ObservableBitset other)
        {
            if (_indices == other._indices) return true;
            if (_indices.Count != other._indices.Count) return false;

            for (int i = 0; i < _indices.Count; ++i)
                if (_indices[i] != other._indices[i])
                    return false;

            return true;
        }

        private int FindGroup(uint key, out bool value)
		{
			int size = _indices.Count;

			if (key >= GetLastIndex())
			{
				value = false;
				return size;
			}

			var group = _indices.BinarySearch(key);
			group = group < 0 ? ~group : group + 1;
			value = ((size + group) & 1) != 0;
			return group;
		}

		private void AddNewBit(uint key)
		{
			var size = _indices.Count;
			var lastIndex = GetLastIndex();

			if (size > 0 && key == lastIndex)
			{
				_indices[size - 1]++;
			}
			else
			{
				if (key > lastIndex) _indices.Add(key);
				_indices.Add(key + 1);
			}

			_callback?.OnDataChanged();
		}

		private void InvertBit(int group, uint key)
		{
			var index = _indices[group];
			var prevIndex = group > 0 ? _indices[group - 1] : 0;

			if (key > prevIndex && key + 1 < index)
				BreakGroup(group, key);
			else if (index == prevIndex+1)
				RemoveGroup(group);
			else if (key == prevIndex)
				InvertLeftMostBit(group);
			else if (key + 1 == index)
				InvertRightMostBit(group);

			_callback?.OnDataChanged();
		}

		private void BreakGroup(int group, uint key)
		{
			_indices.Insert(group, key+1);
			_indices.Insert(group, key);
		}

		private void InvertLeftMostBit(int group)
		{
			if (group > 0)
				_indices[group-1]++;
			else
				_indices.Insert(0, 1);
		}

		private void InvertRightMostBit(int group)
		{
			_indices[group]--;
		}

		private void RemoveGroup(int gropu)
		{
			if (gropu > 0)
				_indices.RemoveRange(gropu - 1, 2);
			else
				_indices.RemoveAt(gropu);
		}

		private uint GetLastIndex()
		{
			var count = _indices.Count;
			return count == 0 ? 0 : _indices[count - 1];
		}

		public void Assign(ObservableBitset other)
		{
			_indices.Clear();
			_indices.AddRange(other._indices);
			_callback?.OnDataChanged();
		}

		public ObservableBitset(IDataChangedCallback callback)
		{
			_callback = callback;
			_indices = new();
		}

		public ObservableBitset(SessionDataReader reader, EncodingType encoding, IDataChangedCallback callback)
		{
			_callback = callback;
			var size = reader.ReadInt(encoding);
			_indices = new(size);
			uint itemCount = 0;
			for (int i = 0; i < size; ++i)
			{
				itemCount += reader.ReadUint(encoding) + 1;
				_indices.Add(itemCount);
			}
		}

		public void Serialize(SessionDataWriter writer, EncodingType encoding)
		{
			writer.WriteInt(_indices.Count, encoding);
			uint lastValue = 0;
			foreach (var value in _indices)
			{
				writer.WriteUint(value - lastValue - 1, encoding);
				lastValue = value;
			}
		}
	}
}
