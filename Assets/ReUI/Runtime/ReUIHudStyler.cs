using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace ReUI
{
    internal static class ReUIHudStyler
    {
        private static readonly Color LifeGreen = new(0.42f, 1.00f, 0.50f, 0.92f);
        private static readonly Color ShieldBlue = new(0.35f, 0.68f, 1.00f, 0.92f);
        private static readonly Color EnergyYellow = new(1.00f, 0.88f, 0.30f, 0.92f);

        private static Type _progressBarType;
        private static FieldInfo _progressBarImageField;
        private static bool _reflectionInitialized;

        internal static void Apply(Canvas canvas)
        {
            if (canvas == null || canvas.gameObject.scene.name != "CombatScene") return;
            StyleCombatHud(canvas.transform);
        }

        private static void StyleCombatHud(Transform root)
        {
            Graphic[] graphics = root.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                Graphic graphic = graphics[i];
                if (!IsProgressBar(graphic)) continue;

                string name = graphic.gameObject.name;
                if (name == "ArmorPoints" || name == "HitPoints")
                    MakeContinuous(graphic, LifeGreen);
                else if (name == "ShieldPoints")
                    MakeContinuous(graphic, ShieldBlue);
                else if (name == "EnergyPoints")
                    MakeContinuous(graphic, EnergyYellow);
            }

            // Preserve the three resource channels beside the ship portrait. The
            // generic text pass must never flatten them into one white/blue color.
            Text[] texts = root.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                Text text = texts[i];
                string path = BuildPath(text.transform, 8);
                if (text.name == "ResourceValue0" || path.Contains("ArmorPoints") || path.Contains("HitPoints"))
                    StyleResourceValue(text, LifeGreen);
                else if (text.name == "ResourceValue1" || path.Contains("ShieldPoints"))
                    StyleResourceValue(text, ShieldBlue);
                else if (text.name == "ResourceValue2" || path.Contains("EnergyPoints"))
                    StyleResourceValue(text, EnergyYellow);
            }
        }

        private static void StyleResourceValue(Text text, Color color)
        {
            if (text == null) return;
            text.color = color;
            text.fontStyle = FontStyle.Bold;

            Shadow shadow = text.GetComponent<Shadow>();
            if (shadow == null) shadow = text.gameObject.AddComponent<Shadow>();
            shadow.enabled = true;
            shadow.effectColor = new Color(0f, 0.01f, 0.03f, 0.90f);
            shadow.effectDistance = new Vector2(1f, -1f);
            shadow.useGraphicAlpha = true;
        }

        private static void MakeContinuous(Graphic graphic, Color color)
        {
            InitializeReflection(graphic.GetType());
            if (_progressBarImageField != null && _progressBarImageField.DeclaringType != null &&
                _progressBarImageField.DeclaringType.IsInstanceOfType(graphic))
            {
                // ProgressBar tiles its private sprite across the filled area. Clearing
                // that sprite makes it use Unity's solid white texture, yielding one
                // continuous bar while retaining the original X/Y fill calculations.
                _progressBarImageField.SetValue(graphic, null);
            }

            graphic.color = color;
            graphic.raycastTarget = false;
            graphic.material = null;
            graphic.SetVerticesDirty();
            graphic.SetMaterialDirty();
        }

        private static bool IsProgressBar(Graphic graphic)
        {
            if (graphic == null) return false;
            Type type = graphic.GetType();
            return type.FullName == "Gui.Controls.ProgressBar";
        }

        private static void InitializeReflection(Type type)
        {
            if (_reflectionInitialized) return;
            _reflectionInitialized = true;
            _progressBarType = type != null && type.FullName == "Gui.Controls.ProgressBar" ? type : null;
            _progressBarImageField = _progressBarType?.GetField("_image", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private static string BuildPath(Transform transform, int maxDepth)
        {
            string path = transform != null ? transform.name : string.Empty;
            Transform current = transform?.parent;
            int depth = 0;
            while (current != null && depth++ < maxDepth)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }
    }
}
