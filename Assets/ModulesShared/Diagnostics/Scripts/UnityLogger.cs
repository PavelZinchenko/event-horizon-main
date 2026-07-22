namespace GameDiagnostics
{
	public class UnityLogger : ILogger
	{
        public static readonly ILogger Instance = new UnityLogger();
        
        public void Log(string message, UnityEngine.GameObject context) => UnityEngine.Debug.Log(message, context);
		public void LogWarning(string message, UnityEngine.GameObject context) => UnityEngine.Debug.LogWarning(message, context);
		public void LogError(string message, UnityEngine.GameObject context) => UnityEngine.Debug.LogError(message, context);
		public void LogException(System.Exception e, UnityEngine.GameObject context) => UnityEngine.Debug.LogException(e, context);
	}
}
