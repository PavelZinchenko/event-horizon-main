using System;
using System.Drawing;

namespace EditorDatabase
{
    public static class Helpers
    {
        public static Color ColorFromString(string color)
        {
            if (string.IsNullOrEmpty(color) || color[0] != '#')
                return Color.White;

            if (color.Length == 7)
                return Color.FromArgb((int)(Convert.ToUInt32(color.Substring(1), 16) | 0xff000000));
            else if (color.Length == 9)
                return Color.FromArgb((int)Convert.ToUInt32(color.Substring(1), 16));
            else
                return Color.White;
        }

        public static string ColorToString(Color color)
        {
            if (color.ToArgb() == -1) return string.Empty;

            if (color.A == 0xff)
                return "#" + (color.ToArgb() & 0xffffff).ToString("X6");
            else
                return "#" + color.ToArgb().ToString("X8");
        }

        public static string ValidateColorString(string color)
        {
            if (string.IsNullOrEmpty(color))
                return _colorWhite;

            var index = color[0] == '#' ? 1 : 0;
            try
            {
                var value = Convert.ToUInt32(color.Substring(index), 16);
                return "#" + (value > 0xffffff ? value.ToString("X8") : value.ToString("X6"));
            }
            catch (Exception)
            {
                return _colorWhite;
            }
        }

        public static string FileName(string file)
        {
            if (string.IsNullOrEmpty(file))
                return string.Empty;

            var namePosition = file.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1;
            return file.Substring(namePosition, file.Length - namePosition - 5); // trim ".json"        
        }

        private const string _colorWhite = "#ffffff";
    }
}
