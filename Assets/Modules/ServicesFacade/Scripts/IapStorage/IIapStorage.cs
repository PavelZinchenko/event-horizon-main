using System;
using UnityEngine;
using Utils;

namespace Services.IapStorage
{
	[Serializable]
	public struct IapData
	{
		//public static IapData FromSession(ISessionData session)
		//{
		//	return new IapData
		//	{
		//		PurchasedStars = session.Purchases.PurchasedStars,
		//		SupporterPack = session.Purchases.SupporterPack ? 1 : 0,
		//	};
		//}

		public static IapData operator+(IapData first, IapData second)
		{
			return new IapData
			{
				PurchasedStars = Mathf.Max(first.PurchasedStars, second.PurchasedStars),
				SupporterPack = first.SupporterPack | second.SupporterPack,
				RemoveAds = first.RemoveAds | second.RemoveAds,
			};
		}

        public int PurchasedStars;
        public int SupporterPack;
        public int RemoveAds;
	}

	public interface IIapStorage
	{
	    void Read(Action<IapData> onCompleteAction);
		void Write(IapData data);
	}

    public class IapDataSavedSignal : SmartWeakSignal
    {
        public class Trigger : TriggerBase { }
    }
}
