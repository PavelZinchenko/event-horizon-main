namespace Session.Model
{
	public readonly partial struct BossInfo
	{
		public BossInfo(int count, int time)
		{
			_lastDefeatTime = time;
			_defeatCount = count;
		}
	}
}
