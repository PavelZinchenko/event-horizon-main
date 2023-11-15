using System.IO;
using UnityEngine;
using AppConfiguration.Utils;

namespace AppConfiguration
{
    [CreateAssetMenu(fileName = "ColorTable", menuName = "AppConfig/ColorTable", order = 1)]
    public partial class ColorTable : ScriptableObject
    {
        [SerializeField] private string _generatedFilePath = "Modules/CommonComponents/Scripts/Generated/ColorTable.cs";

        [SerializeField] private Color _qualityPoor;
        [SerializeField] private Color _qualityCommon;
        [SerializeField] private Color _qualityGood;
        [SerializeField] private Color _qualityExcellent;
        [SerializeField] private Color _qualityLegendary;

        [SerializeField] private Color _defaultTextColor;
        [SerializeField] private Color _premiumItemColor;
        [SerializeField] private Color _tokensColor;
        [SerializeField] private Color _snowflakesColor;

#if UNITY_EDITOR
        private const string _colorGetterName = "QualityColor";
        private const string _colorEnumName = "Quality";

        [ContextMenu("Ganerate Code")]
        private void GenerateScript()
        {
            var code = new SettingsClassBuilder(nameof(ColorTable))
                .SetNamespace(nameof(AppConfiguration))
                .AddColor(nameof(_qualityPoor), _qualityPoor)
                .AddColor(nameof(_qualityCommon), _qualityCommon)
                .AddColor(nameof(_qualityGood), _qualityGood)
                .AddColor(nameof(_qualityExcellent), _qualityExcellent)
                .AddColor(nameof(_qualityLegendary), _qualityLegendary)
                .WithColorGetter(_colorGetterName, _colorEnumName)
                .StartNewSection()
                .AddColor(nameof(_defaultTextColor), _defaultTextColor)
                .AddColor(nameof(_premiumItemColor), _premiumItemColor)
                .AddColor(nameof(_tokensColor), _tokensColor)
                .AddColor(nameof(_snowflakesColor), _snowflakesColor)

                .AsPartial()
                .Build();

            File.WriteAllText(Path.Combine(Application.dataPath, _generatedFilePath), code);
        }
#endif
    }
}
