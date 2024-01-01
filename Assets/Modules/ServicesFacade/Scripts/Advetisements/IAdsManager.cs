using CommonComponents.Signals;

namespace Services.Advertisements
{
    public enum AdStatus
    {
        NotLoaded,
        Busy,
        Ready,
        ShowFailed,
        LoadFailed,
    }

    public interface IAdsManager
    {
        bool AdsEnabled { get; }
        void ShowRewardedVideo();
        void ShowInterstitial();
        void CancelAd();
        AdStatus Status { get; }
    }

    public class RewardedVideoCompletedSignal : SmartWeakSignal<RewardedVideoCompletedSignal> {}
    public class AdStatusChangedSignal : SmartWeakSignal<AdStatusChangedSignal, AdStatus> {}
}
