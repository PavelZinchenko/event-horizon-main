namespace Session.Model
{
	public readonly partial struct PurchaseInfo
	{
		public PurchaseInfo(int quantity, int time)
		{
			_quantity = quantity;
			_time = time;
		}
	}
}
