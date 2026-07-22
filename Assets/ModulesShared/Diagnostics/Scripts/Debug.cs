namespace GameDiagnostics
{
	public interface ILogger
	{
		void Log(string message, UnityEngine.GameObject context = null);
		void LogWarning(string message, UnityEngine.GameObject context = null);
		void LogError(string message, UnityEngine.GameObject context = null);
		void LogException(System.Exception e, UnityEngine.GameObject context = null);
	}

	public static class Debug
	{
		private static ILogger Logger => UnityLogger.Instance;

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void Log(string message, UnityEngine.GameObject context = null) => Logger.Log(message, context);

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void LogWarning(string message, UnityEngine.GameObject context = null) => Logger.LogWarning(message, context);

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void LogError(string message, UnityEngine.GameObject context = null) => Logger.LogError(message, context);

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void LogException(System.Exception e, UnityEngine.GameObject context = null) => Logger.LogException(e, context);
	}

	public static class Trace
	{
        private static ILogger _logger;
        public static ILogger Logger { get => _logger ?? UnityLogger.Instance; set => _logger = value; }

        public static void Log(string message) => Logger.Log(message);
		public static void LogWarning(string message) => Logger.LogWarning(message);
		public static void LogError(string message) => Logger.LogError(message);
		public static void LogException(System.Exception e) => Logger.LogException(e);
	}
}
