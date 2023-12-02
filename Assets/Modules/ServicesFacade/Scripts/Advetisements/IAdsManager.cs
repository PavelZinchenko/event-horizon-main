using CommonComponents.Utils;

namespace Services.Advertisements
{
    public enum AdStatus
    {
        NotLaded,
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

    public class RewardedVideoCompletedSignal : SmartWeakSignal
    {
        public class Trigger : TriggerBase {}
    }

    public class AdStatusChangedSignal : SmartWeakSignal<AdStatus>
    {
        public class Trigger : TriggerBase { }
    }
}
