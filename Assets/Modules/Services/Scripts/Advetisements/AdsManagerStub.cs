namespace Services.Advertisements
{
    public class AdsManagerStub : IAdsManager
    {
        public bool AdsEnabled { get; } = false;
        public void ShowRewardedVideo() {}
        public void ShowInterstitial() {}
        public void CancelAd() {}

        AdStatus IAdsManager.Status => AdStatus.NotLoaded;
    }
}
