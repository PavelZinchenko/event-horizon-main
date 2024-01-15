namespace GameDiagnostics
{
	public class UnityLogger : ILogger
	{
		public void Log(string message = null) => UnityEngine.Debug.Log(message);
		public void LogWarning(string message = null) => UnityEngine.Debug.LogWarning(message);
		public void LogError(string message = null) => UnityEngine.Debug.LogError(message);
		public void LogException(System.Exception e) => UnityEngine.Debug.LogException(e);
	}
}
