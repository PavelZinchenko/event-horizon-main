using System;
using System.Text;

namespace Utils
{
    public static class Base64Encoder
    {
        public static string EncodeUrlBase64(string base64)
        {
            var builder = new StringBuilder(base64);
            builder.Replace('+', '-');
            builder.Replace('/', '_');
            while (builder.Length > 0 && builder[builder.Length - 1] == '=')
                builder.Remove(builder.Length - 1, 1);

            return builder.ToString();
        }

        public static string DecodeUrlBase64(string urlBase64)
        {
            var builder = new StringBuilder(urlBase64);
            builder.Replace('-', '+');
            builder.Replace('_', '/');
            while (builder.Length % 4 != 0)
                builder.Append('=');

            return builder.ToString();
        }
    }
}
