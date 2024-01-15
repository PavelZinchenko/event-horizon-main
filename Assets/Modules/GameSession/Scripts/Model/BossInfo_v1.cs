namespace Session.v1
{
	public readonly partial struct BossInfo
	{
		public BossInfo(int count, long time)
		{
			_lastDefeatTime = time;
			_defeatCount = count;
		}
	}
}
