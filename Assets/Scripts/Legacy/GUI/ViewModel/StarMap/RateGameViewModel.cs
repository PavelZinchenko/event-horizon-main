using GameServices.Settings;
using Services.RateGame;
using UnityEngine;
using Zenject;

namespace ViewModel
{
	public class RateGameViewModel : MonoBehaviour
	{
        [Inject] private readonly GameServices.Player.MotherShip _motherShip;
	    [Inject] private readonly GameSettings _gameSettings;
		[Inject] private readonly IRateGameService _rateGame;

        public RectTransform Content;

		public void OnEnable()
		{
            var available = 
				_motherShip.CurrentStar.Level >= 10 && 
				!_gameSettings.RateButtonClicked && 
				Application.internetReachability != NetworkReachability.NotReachable &&
                _rateGame.CanRateGame;
        }

        public void RateButtonClicked()
		{
			_rateGame.RateGame();
			UnityEngine.Debug.Log("RateButtonClicked");
			Content.gameObject.SetActive(false);
			_gameSettings.RateButtonClicked = true;
		}
	}
}
