using UnityEngine;

namespace GameServices.Player
{
    public static class ThreeBodySkillState
    {
        private const string AdvancedRadarKey = "Preview7.ThreeBody1.AdvancedRadar";
        private const string CollaborativeCombatKey = "Alpha13.ThreeBody1.CollaborativeCombat";
        private const string GiantCannonsKey = "Alpha15.ThreeBody1.GiantCannons";
        private const string EngineThrottleKey = "Preview14.EngineThrottle";
        private const string EngineThrottleLimitKey = "Preview18.EngineThrottleLimit";

        public static bool AdvancedRadarUnlocked => PlayerPrefs.GetInt(AdvancedRadarKey, 0) != 0;
        public static bool CollaborativeCombatUnlocked => PlayerPrefs.GetInt(CollaborativeCombatKey, 0) != 0;
        public static bool GiantCannonsUnlocked => PlayerPrefs.GetInt(GiantCannonsKey, 0) != 0;
        public static float RadarRangeMultiplier => AdvancedRadarUnlocked ? 1.2f : 1f;
        public static bool EngineThrottleEnabled => PlayerPrefs.GetInt(EngineThrottleKey, 0) != 0;
        public static float EngineThrottleLimit => Mathf.Clamp(PlayerPrefs.GetFloat(EngineThrottleLimitKey, 40f), 20f, 120f);

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

        public static void UnlockCollaborativeCombat()
        {
            PlayerPrefs.SetInt(CollaborativeCombatKey, 1);
            PlayerPrefs.Save();
        }

        public static void UnlockGiantCannons()
        {
            PlayerPrefs.SetInt(GiantCannonsKey, 1);
            PlayerPrefs.Save();
        }

        public static void ResetThreeBody1()
        {
            PlayerPrefs.DeleteKey(AdvancedRadarKey);
            PlayerPrefs.DeleteKey(CollaborativeCombatKey);
            PlayerPrefs.DeleteKey(GiantCannonsKey);
            PlayerPrefs.Save();
        }

        public static void SetEngineThrottle(bool enabled)
        {
            PlayerPrefs.SetInt(EngineThrottleKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void SetEngineThrottleLimit(float value)
        {
            PlayerPrefs.SetFloat(EngineThrottleLimitKey, Mathf.Clamp(Mathf.Round(value), 20f, 120f));
            PlayerPrefs.Save();
        }
    }
}
