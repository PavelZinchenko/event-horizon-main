using UnityEngine;

namespace ThreeBody
{
    public enum CombatMapSize
    {
        Small,
        Medium,
        Large,
        Huge,
        Colossal
    }

    public static class CombatMapSizeSettings
    {
        private const string PlayerPrefsKey = "ThreeBody.CombatMapSize";

        public static CombatMapSize Selected
        {
            get => (CombatMapSize)Mathf.Clamp(PlayerPrefs.GetInt(PlayerPrefsKey, (int)CombatMapSize.Medium), 0, 4);
            set
            {
                PlayerPrefs.SetInt(PlayerPrefsKey, (int)value);
                PlayerPrefs.Save();
            }
        }

        public static float Multiplier => GetMultiplier(Selected);

        public static int Scale(int originalSize)
        {
            return Mathf.Max(50, Mathf.RoundToInt(originalSize * Multiplier));
        }

        public static CombatMapSize Next()
        {
            Selected = (CombatMapSize)(((int)Selected + 1) % 5);
            return Selected;
        }

        public static float GetMultiplier(CombatMapSize size)
        {
            return size switch
            {
                CombatMapSize.Small => 0.5f,
                CombatMapSize.Medium => 1f,
                CombatMapSize.Large => 5f,
                CombatMapSize.Huge => 20f,
                CombatMapSize.Colossal => 100f,
                _ => 1f
            };
        }

        public static string GetDisplayName(CombatMapSize size)
        {
            return size switch
            {
                CombatMapSize.Small => "小",
                CombatMapSize.Medium => "中",
                CombatMapSize.Large => "大",
                CombatMapSize.Huge => "巨大",
                CombatMapSize.Colossal => "超大",
                _ => "中"
            };
        }

        public static string GetDisplayText()
        {
            return $"战斗地图：{GetDisplayName(Selected)}（原版 × {Multiplier:0.#}）";
        }
    }
}
