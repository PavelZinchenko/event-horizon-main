using System.Collections.Generic;
using CombatView.Effects;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using Services.Resources;
using EffectColorMode = CombatView.Effects.ColorMode;

namespace Combat.Integration.Core
{
    // Database VisualEffect records as the combat view's plain EffectRecord (migration phase 5b, step 3). One record per
    // database record, built on first use; the images resolve here, as the sprites of the other views do.
    public sealed class CoreVisualEffects
    {
        public CoreVisualEffects(IResourceLocator resourceLocator)
        {
            _resourceLocator = resourceLocator;
        }

        // Null for no record or a record without elements. An element of an unknown type is left out: legacy has no
        // prefab for it and draws nothing.
        public EffectRecord Get(VisualEffect effect)
        {
            if (effect == null || effect.Elements.Count == 0)
                return null;
            if (_records.TryGetValue(effect, out var record))
                return record;

            var elements = new List<EffectElement>(effect.Elements.Count);
            foreach (var element in effect.Elements)
                if (TryGetKind(element.Type, out var kind))
                    elements.Add(ToElement(element, kind));

            record = new EffectRecord(elements);
            _records.Add(effect, record);
            return record;
        }

        public EffectElement ToElement(VisualEffectElement element, EffectKind kind) => new EffectElement(
            kind, _resourceLocator.GetSprite(element.Image), (EffectColorMode)element.ColorMode, element.Color.ToUnity(),
            element.Quantity, element.Size, element.GrowthRate, element.TurnRate, element.StartTime, element.Lifetime,
            element.ParticleSize, element.Offset.ToUnity(), element.Rotation, element.Loop, element.Inverse,
            element.UseRealTime);

        // The view enum keeps the database order; Laser comes with the database edits of step 7.
        public static bool TryGetKind(VisualEffectType type, out EffectKind kind)
        {
            switch (type)
            {
                case VisualEffectType.Flash: kind = EffectKind.Flash; return true;
                case VisualEffectType.FlashAdditive: kind = EffectKind.FlashAdditive; return true;
                case VisualEffectType.Shockwave: kind = EffectKind.Shockwave; return true;
                case VisualEffectType.Smoke: kind = EffectKind.Smoke; return true;
                case VisualEffectType.SmokeAdditive: kind = EffectKind.SmokeAdditive; return true;
                case VisualEffectType.Shake: kind = EffectKind.Shake; return true;
                case VisualEffectType.Spark: kind = EffectKind.Spark; return true;
                case VisualEffectType.Lightning: kind = EffectKind.Lightning; return true;
                case VisualEffectType.LightningStrike: kind = EffectKind.LightningStrike; return true;
                case VisualEffectType.Sprite: kind = EffectKind.Sprite; return true;
                default: kind = default; return false;
            }
        }

        private readonly IResourceLocator _resourceLocator;
        private readonly Dictionary<VisualEffect, EffectRecord> _records = new Dictionary<VisualEffect, EffectRecord>();
    }
}
