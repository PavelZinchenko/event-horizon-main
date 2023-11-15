using UnityEngine;

namespace Services.RateGame
{
    public interface IRateGameService
    {
        bool CanRateGame { get; }
        void RateGame();
    }

    public class NullRateGameService : IRateGameService
    {
        public bool CanRateGame => false;
        public void RateGame() {}
    }

    public class AndroidRateGameService : IRateGameService
    {
        public bool CanRateGame => true;
        
        public void RateGame()
        {
            Application.OpenURL("market://details?id=" + AppConfig.bundleIdentifier);
        }
    }

    public class IosRateGameService : IRateGameService
    {
        public bool CanRateGame => true;

        public void RateGame()
        {
            Application.OpenURL("itms-apps://itunes.apple.com/app/" + AppConfig.bundleIdentifier);
        }
    }
}
