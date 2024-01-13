namespace Session.Model
{
	public readonly partial struct QuestStatistics
	{
		public QuestStatistics(int completionCount, int failureCount, long lastStartTime, long lastCompletionTime)
		{
			_completionCount = completionCount;
			_failureCount = failureCount;
			_lastStartTime = lastStartTime;
			_lastCompletionTime = lastCompletionTime;
		}
	}
}
