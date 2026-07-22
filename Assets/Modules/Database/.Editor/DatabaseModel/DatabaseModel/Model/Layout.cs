using System;
using System.Text;

namespace EditorDatabase.Model
{
    public class Layout
    {
        public Layout(string data)
        {
            Data = data;
        }

        public string Data
        {
            get { return _data.ToString(); }
            set
            {
                _data.Clear();

                if (string.IsNullOrEmpty(value))
                {
                    _data.Append(_defaultValue);
                    _size = 1;
					_cellcount = 0;
                    return;
                }

                _data.Append(value);
				_cellcount = value.Count(cell => cell != _defaultValue);

				var length = value.Length;
                _size = (int)Math.Sqrt(length);
                if (_size*_size == length)
                    return;

                _size++;
                _data.Append(_defaultValue, _size*_size - length);
            }
        }

        public char this[int x, int y]
        {
            get
            {
                if (x < 0 || x >= _size || y < 0 || y >= _size)
                    return _defaultValue;

                return _data[y*_size + x];
            }
            set
            {
                if (x < 0 || x >= _size || y < 0 || y >= _size)
                    return;

                _data[y*_size + x] = value;
            }
        }

        public char this[int index]
        {
            get { return index < 0 || index >= _data.Length ? _defaultValue : _data[index]; }
            set
            {
                if (index < 0 || index >= _data.Length)
                    return;

                _data[index] = value;
            }
        }

		public int CellCount => _cellcount;
		
		public int Size
        {
            get { return _size; }
            set
            {
                if (value == _size || value < 1)
                    return;

                var data = new StringBuilder(value*value);
                for (var i = 0; i < value; ++i)
                    for (var j = 0; j < value; ++j)
                        data.Append(this[j, i]);

                _data = data;
                _size = value;
            }
        }

        public void Shift(int dx, int dy)
        {
            if (dx == 0 && dy == 0)
                return;

            var data = new StringBuilder(Size*Size);
            for (var i = 0; i < Size; ++i)
                for (var j = 0; j < Size; ++j)
                    data.Append(this[j+dx, i+dy]);

            _data = data;
        }

        private int _size = 1;
        private int _cellcount;
        private StringBuilder _data = new StringBuilder(_defaultValue);
        private const char _defaultValue = '0';
    }
}
