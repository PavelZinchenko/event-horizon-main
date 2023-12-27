using Game;
using GameServices.Player;
using Services.Messenger;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using CommonComponents;

namespace ViewModel
{
	public class StatusPanelViewModel : MonoBehaviour
	{
	    [Inject] private readonly PlayerResources _playerResources;
	    [Inject] private readonly HolidayManager _holidayManager;

		public Text CreditsText;
		public Text FuelText;
		public GameObject OutOfFuelLabel;
		public GameObject StarPanel;
	    public Text StarsText;
	    public GameObject SnowflakesPanel;
        public Text SnowflakesText;


        [Inject]
	    private void Initialize(IMessenger messenger)
	    {
            messenger.AddListener<Money>(EventType.MoneyValueChanged, OnMoneyValueChanged);
            messenger.AddListener<int>(EventType.FuelValueChanged, OnFuelValueChanged);
            messenger.AddListener<Money>(EventType.StarsValueChanged, OnStarsValueChanged);
	        messenger.AddListener(EventType.SpecialResourcesChanged, OnSpecialResourcesChanged);

            Credits = _playerResources.Money;
            Fuel = _playerResources.Fuel;
            Stars = _playerResources.Stars;
	        Snowflakes = _playerResources.Snowflakes;
	    }

        public Money Credits
		{
			get { return _credits; }
			set
			{
				if (_credits != value)
				{
					_credits = value;
					CreditsText.text = _credits.ToString();
				}
			}
		}

	    public Money Stars
	    {
	        get { return _stars; }
	        set
	        {
#if IAP_DISABLED
	            _stars = 0;
	            StarPanel.gameObject.SetActive(false);
#else
                if (_stars != value)
	            {
	                _stars = value;
	                StarPanel.gameObject.SetActive(true);
	                StarsText.text = _stars.ToString();
	            }
#endif
            }
        }

	    public int Snowflakes
	    {
	        get { return _snowflakes; }
	        set
	        {
	            if (!_holidayManager.IsChristmas)
	            {
	                _snowflakes = 0;
	                SnowflakesPanel.gameObject.SetActive(false);
	            }
	            else if (_snowflakes != value)
	            {
	                _snowflakes = value;
	                SnowflakesPanel.gameObject.SetActive(true);
	                SnowflakesText.text = _snowflakes.ToString();
	            }
	        }
	    }

		public int Fuel
		{
			get { return _fuel; }
			set
			{
				if (_fuel != value)
				{
					_fuel = value;
					OutOfFuelLabel.SetActive(_fuel == 0);
					FuelText.text = _fuel > 0 ? _fuel.ToString() : string.Empty;
				}
			}
		}

	    private void OnMoneyValueChanged(Money value)
	    {
	        Credits = value;
	    }

        private void OnFuelValueChanged(int value)
        {
            Fuel = value;
        }

        private void OnStarsValueChanged(Money value)
        {
            Stars = value;
        }

	    private void OnSpecialResourcesChanged()
	    {
	        Snowflakes = _playerResources.Snowflakes;
	    }

		private int _fuel = -1;
        private int _snowflakes = -1;
		private Money _credits = -1;
		private Money _stars = -1;
	}
}
