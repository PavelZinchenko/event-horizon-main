namespace Session.Model
{
	public readonly partial struct QuestProgress
	{
		public QuestProgress(int seed, int activeNode, long startTime)
		{
			_seed = seed; ;
			_activeNode = activeNode;
			_startTime = startTime;
		}
	}
}
