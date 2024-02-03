using GameServices.Player;
using GameStateMachine.States;
using Gui.Windows;
using Services.Gui;
using Services.Messenger;
using UnityEngine;
using UnityEngine.UI;
using Gui.Theme;
using Zenject;

namespace ViewModel
{
	public class FlightConfirmationViewModel : MonoBehaviour
	{
	    [Inject] private readonly PlayerResources _playerResources;
        [Inject] private readonly MotherShip _motherShip;
	    [Inject] private readonly StartTravelSignal.Trigger _travelTrigger;
	    [Inject] private readonly IMessenger _messenger;

        public Text FuelText;
		public Image FuelIcon;
		public Button ConfirmButton;

        [SerializeField] private ThemeColor _normalColor = ThemeColor.Text;
        [SerializeField] private ThemeColor _notEnoughColor = ThemeColor.ErrorText;

        private Color NormalColor => UiTheme.Current.GetColor(_normalColor);
		private Color NotEnoughColor => UiTheme.Current.GetColor(_notEnoughColor);

	    public void Initialize(WindowArgs args)
	    {
	        var starId = args.Get<int>(0);

			var required = _motherShip.CalculateRequiredFuel(_motherShip.CurrentStar.Id, starId);
			var haveFuel = _playerResources.Fuel >= required;

			FuelText.text = required.ToString();
			FuelText.color = haveFuel ? NormalColor : NotEnoughColor;
			FuelIcon.color = haveFuel ? NormalColor : NotEnoughColor;

            _messenger.Broadcast<int>(EventType.FocusedPositionChanged, starId);
			ConfirmButton.interactable = haveFuel;
		}

	    public void OnWindowClosed()
	    {
            _messenger.Broadcast<int>(EventType.FocusedPositionChanged, -1);
        }

        public void ConfirmButtonClicked()
		{
            GetComponent<AnimatedWindow>().Close(WindowExitCode.Ok);
        }
	}
}
