namespace Session.v1
{
	public readonly partial struct PurchaseInfo
	{
		public PurchaseInfo(int quantity, long time)
		{
			_quantity = quantity;
			_time = time;
		}
	}
}
