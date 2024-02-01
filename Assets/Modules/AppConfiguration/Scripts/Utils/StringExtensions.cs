using System;

namespace AppConfiguration.Utils
{
    internal static class StringExtensions
    {
        public static string ToPublicPropertyName(this string name)
        {
            var length = name.Length;
            var index = 0;

            while (index < length && !char.IsLetter(name, index)) 
                index++;

            return FirstCharToUpperCase(name, index);
        }

        public static string FirstCharToUpperCase(this string name, int startIndex = 0)
        {
            var length = name.Length;
            if (startIndex >= length)
                throw new InvalidOperationException();

            if (char.IsUpper(name, startIndex))
            {
                if (startIndex > 0)
                    return name.Substring(startIndex);
                else
                    return name;
            }

            var data = name.ToCharArray(startIndex, name.Length - startIndex);
            data[0] = char.ToUpper(data[0]);

            return new string(data);
        }

        public static string FirstCharToLowerCase(this string name, int startIndex = 0)
        {
            var length = name.Length;
            if (startIndex >= length)
                throw new InvalidOperationException();

            if (char.IsLower(name, startIndex))
            {
                if (startIndex > 0)
                    return name.Substring(startIndex);
                else
                    return name;
            }

            var data = name.ToCharArray(startIndex, name.Length - startIndex);
            data[0] = char.ToLower(data[0]);

            return new string(data);
        }
    }
}
