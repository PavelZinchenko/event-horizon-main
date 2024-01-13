using Session.Model;

namespace Session.Content
{
	public interface IIapData
	{
		bool SupporterPack { get; set; }
		bool RemoveAds { get; set; }
		int PurchasedStars { get; set; }
	}

	public class IapData : IIapData, ISessionDataContent
	{
		private readonly SaveGameData _data;
		private readonly StarsValueChangedSignal.Trigger _starsValueChangedTrigger;

		public IapData(SaveGameData sessionData, StarsValueChangedSignal.Trigger starsValueChangedTrigger)
		{
			_data = sessionData;
			_starsValueChangedTrigger = starsValueChangedTrigger;
		}

		public bool RemoveAds
		{
			get => _data.Iap.RemoveAds || _data.Iap.SupporterPack || _data.Iap.Stars > 0;
			set => _data.Iap.RemoveAds = value;
		}

		public bool SupporterPack
		{
			get => _data.Iap.SupporterPack;
			set
			{
				if (SupporterPack == value)
					return;

				_data.Iap.SupporterPack = value;
				_starsValueChangedTrigger.Fire(_data.Resources.Stars + ExtraStarCount);
			}
		}

		public int PurchasedStars
		{
			get => _data.Iap.Stars;
			set => _data.Iap.Stars = value;
		}

		private int ExtraStarCount => _data.Iap.SupporterPack ? 100 : 0; // TODO: remove
	}
}
