namespace Session.Model
{
	public readonly partial struct HangarSlotInfo
	{
		public HangarSlotInfo(int index, int shipId)
		{
			_index = index;
			_shipId = shipId;
		}
	}
}
