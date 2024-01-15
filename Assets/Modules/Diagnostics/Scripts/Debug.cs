namespace GameDiagnostics
{
	public interface ILogger
	{
		void Log(string message = null);
		void LogWarning(string message = null);
		void LogError(string message = null);
		void LogException(System.Exception e);
	}

	public static class Debug
	{
		private static ILogger _logger = new UnityLogger();

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void Log(string message) => _logger.Log(message);

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void LogWarning(string message) => _logger.LogWarning(message);

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void LogError(string message) => _logger.LogError(message);

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void LogException(System.Exception e) => _logger.LogException(e);
	}

	public static class Trace
	{
		private static ILogger _logger = new UnityLogger();

		public static void Log(string message) => _logger.Log(message);
		public static void LogWarning(string message) => _logger.LogWarning(message);
		public static void LogError(string message) => _logger.LogError(message);
		public static void LogException(System.Exception e) => _logger.LogException(e);
	}
}
