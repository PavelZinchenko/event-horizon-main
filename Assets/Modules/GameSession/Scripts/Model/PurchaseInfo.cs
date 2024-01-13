using System;

namespace Session.Model
{
	public readonly partial struct PurchaseInfo
	{
		public PurchaseInfo(int quantity, long time)
		{
			_quantity = quantity;
			_time = time;
		}

		public int CalculateQuantity(long renewalTime, long currentTime)
		{
			if (renewalTime <= 0)
			{
				return Quantity;
			}
			else
			{
				var count = (currentTime - Time) / renewalTime;
				return (int)Math.Max(0L, Quantity - count);
			}
		}
	}
}
