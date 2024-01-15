namespace GameDiagnostics
{
	public static class Assert
	{
		[System.Diagnostics.Conditional("UNITY_EDITOR")] 
		public static void IsTrue(bool condition, string message = null)
		{
			if (!condition) Throw("Condition is false");
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void IsFalse(bool condition)
		{
			if (condition) Throw("Condition is true");
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void AreEqual(uint first, uint second)
		{
			if (first != second) Throw($"Condition is false: {first} == {second}");
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void Less(uint first, uint second)
		{
			if (first >= second) Throw($"Condition is false: {first} < {second}");
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void Less(int first, int second)
		{
			if (first >= second) Throw($"Condition is false: {first} < {second}");
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void Greater(uint first, uint second)
		{
			if (first <= second) Throw($"Condition is false: {first} > {second}");
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		private static void Throw(string message)
		{
			if (string.IsNullOrEmpty(message))
				throw new System.InvalidOperationException();
			else
				throw new System.InvalidOperationException(message);
		}
	}
}
