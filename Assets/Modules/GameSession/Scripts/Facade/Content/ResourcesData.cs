using Session.Model;
using Session.Utils;

namespace Session.Content
{
	public interface IResourcesData
	{
		long Money { get; set; }
		long Stars { get; set; }
		int Fuel { get; set; }
		int Tokens { get; set; }
		public ObservableInventory<int> Resources { get; }
	}

	public class ResourcesData : IResourcesData, ISessionDataContent, IDataChangedCallback
	{
		private readonly SaveGameData _data;
		private readonly FuelValueChangedSignal.Trigger _fuelValueChangedTrigger;
		private readonly MoneyValueChangedSignal.Trigger _moneyValueChangedTrigger;
		private readonly StarsValueChangedSignal.Trigger _starsValueChangedTrigger;
		private readonly TokensValueChangedSignal.Trigger _tokensValueChangedTrigger;
		private readonly ResourcesChangedSignal.Trigger _specialResourcesChangedTrigger;

		public ResourcesData(
			SaveGameData sessionData,
			FuelValueChangedSignal.Trigger fuelValueChangedTrigger,
			MoneyValueChangedSignal.Trigger moneyValueChangedTrigger,
			StarsValueChangedSignal.Trigger starsValueChangedTrigger,
			TokensValueChangedSignal.Trigger tokensValueChangedTrigger,
			ResourcesChangedSignal.Trigger specialResourcesChangedTrigger)
		{
			_data = sessionData;
			_fuelValueChangedTrigger = fuelValueChangedTrigger;
			_moneyValueChangedTrigger = moneyValueChangedTrigger;
			_starsValueChangedTrigger = starsValueChangedTrigger;
			_tokensValueChangedTrigger = tokensValueChangedTrigger;
			_specialResourcesChangedTrigger = specialResourcesChangedTrigger;
		}

		public long Money
		{
			get => _data.Resources.Money;
			set
			{
				if (Money == value) return;
				_data.Resources.Money = value;
				_moneyValueChangedTrigger.Fire(value);
			}
		}

		public int Fuel
		{
			get => _data.Resources.Fuel;
			set
			{
				if (Fuel == value) return;
				_data.Resources.Fuel = value;
				_fuelValueChangedTrigger.Fire(value);
			}
		}

		public int Tokens
		{
			get => _data.Resources.Tokens;
			set
			{
				if (Tokens == value) return;
				_data.Resources.Tokens = value;
				_tokensValueChangedTrigger.Fire(value);
			}
		}

		public long Stars
		{
			get => _data.Resources.Stars + ExtraStarCount;
			set
			{
				if (Stars == value) return;
				_data.Resources.Stars = value - ExtraStarCount;
				_starsValueChangedTrigger.Fire(value);
			}
		}

		public ObservableInventory<int> Resources => new(_data.Resources.Resources, this);

		public void OnDataChanged()
		{
			_data.Resources.OnDataChanged();
			_specialResourcesChangedTrigger.Fire();
		}

		private int ExtraStarCount => _data.Iap.SupporterPack ? 100 : 0; // TODO: remove
	}
}
