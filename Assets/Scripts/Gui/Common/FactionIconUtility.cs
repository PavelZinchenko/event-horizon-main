using System.Linq;
using GameDatabase.DataModel;
using UnityEngine;
using UnityEngine.UI;

namespace Gui.Common
{
    public static class FactionIconUtility
    {
        public static void Apply(Image image, Faction faction, float size = 64f)
        {
            if (image == null || faction == null || string.IsNullOrEmpty(faction.Icon)) return;
            var sprites = Resources.LoadAll<Sprite>("Textures/Factions/" + faction.Icon);
            var sprite = sprites.FirstOrDefault() ?? Resources.Load<Sprite>("Textures/Factions/" + faction.Icon);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.color = Color.white;
                image.preserveAspect = true;
            }
            var rect = image.rectTransform;
            if (rect != null && size > 0)
                rect.sizeDelta = new Vector2(size, size);
        }
    }
}
