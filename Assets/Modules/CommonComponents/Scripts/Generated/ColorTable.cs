// This code was automatically generated.
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.

using UnityEngine;

namespace AppConfiguration
{
    public partial class ColorTable
    {
        public static readonly Color QualityPoor = new(0.7411765f,0.7411765f,0.7411765f,1f);
        public static readonly Color QualityCommon = new(0.5019608f,0.9411765f,0.9411765f,1f);
        public static readonly Color QualityGood = new(0.5019608f,1f,0.5019608f,1f);
        public static readonly Color QualityExcellent = new(0.9411765f,0.6235294f,1f,1f);
        public static readonly Color QualityLegendary = new(1f,0.8745098f,0.3176471f,1f);

        public enum Quality
        {
            QualityPoor,
            QualityCommon,
            QualityGood,
            QualityExcellent,
            QualityLegendary,
        }

        public static Color QualityColor(Quality quality)
        {
            switch (quality)
            {
                case Quality.QualityPoor: return QualityPoor;
                case Quality.QualityCommon: return QualityCommon;
                case Quality.QualityGood: return QualityGood;
                case Quality.QualityExcellent: return QualityExcellent;
                case Quality.QualityLegendary: return QualityLegendary;
                default: throw new System.InvalidOperationException();
            }
        }

        public static readonly Color DefaultTextColor = new(0.5019608f,1f,1f,1f);
        public static readonly Color PremiumItemColor = new(1f,0.9411765f,0.627451f,1f);
        public static readonly Color TokensColor = new(0.5019608f,0.5019608f,1f,1f);
        public static readonly Color SnowflakesColor = new(0.7529412f,1f,1f,1f);

    }
}
