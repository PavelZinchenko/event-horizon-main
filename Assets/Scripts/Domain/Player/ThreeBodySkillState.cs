using UnityEngine;

namespace GameServices.Player
{
    public static class ThreeBodySkillState
    {
        private const string AdvancedRadarKey = "Preview7.ThreeBody1.AdvancedRadar";
        private const string EngineThrottleKey = "Preview14.EngineThrottle";

        public static bool AdvancedRadarUnlocked => PlayerPrefs.GetInt(AdvancedRadarKey, 0) != 0;
        public static float RadarRangeMultiplier => AdvancedRadarUnlocked ? 1.2f : 1f;
        public static bool EngineThrottleEnabled => PlayerPrefs.GetInt(EngineThrottleKey, 0) != 0;

        public static void UnlockAdvancedRadar()
        {
            PlayerPrefs.SetInt(AdvancedRadarKey, 1);
            PlayerPrefs.Save();
        }

        public static void ResetAdvancedRadar()
        {
            PlayerPrefs.DeleteKey(AdvancedRadarKey);
            PlayerPrefs.Save();
        }

        public static void SetEngineThrottle(bool enabled)
        {
            PlayerPrefs.SetInt(EngineThrottleKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
