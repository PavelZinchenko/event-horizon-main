using System;

namespace Services.IapStorage
{
	public class EmptyStorage : IIapStorage
	{
	    public void Read(Action<IapData> onCompleteAction) { onCompleteAction(new IapData()); }
		public void Write(IapData data) {}
	}
}

