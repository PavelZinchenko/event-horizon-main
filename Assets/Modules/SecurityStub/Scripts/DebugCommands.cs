namespace Security
{
	public static class DebugCommands
	{
		public static int GetHashCode(string data)
		{
			return System.Math.Abs(data.GetHashCode()) % 100000;
		}

		public static int Decode(string command, int hash)
		{
			return int.TryParse(command, out var result) ? result : -1;
		}

		public static string Encode(int id, int hash)
		{
			return id.ToString();
		}
	}
}
