using System;

namespace GameDatabase.Utils
{
	public static class Serialization
	{
		public static int DeserializeInt(byte[] data, ref int index)
		{
			return (int)data[index++] | (((int)data[index++]) << 8) | (((int)data[index++]) << 16) | (((int)data[index++]) << 24);
		}			

		public static string DeserializeString(byte[] data, ref int index)
		{
			int count = BitConverter.ToInt32(data, index);
			index += sizeof(int);
			if (count == 0) 
				return string.Empty;
			var result = System.Text.Encoding.UTF8.GetString(data, index, count);
			index += count;
			return result;
        }
            
		public static byte[] DeserializeByteArray(byte[] data, ref int index)
		{
		    var length = DeserializeInt(data, ref index);
		    if (length <= 0)
		        return new byte[] {};

            var array = new byte[length];
		    Array.Copy(data, index, array, 0, length);
		    index += length;
		    return array;
		}
	}
}
