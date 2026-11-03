using System.Collections.Generic;
using CombatSimulation.Core;
using CombatView;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Extensions;
using Services.Resources;
using UnityEngine;

namespace Combat.Integration.Core
{
    // Turns the bullet sources of the mapper into views (phase 4, step 5, "Views (5)"), once per definition at its first
    // spawn. Legacy sources: PrefabCache.GetBulletPrefab, ProjectileInitializer, TrailView and BeamInitializer.
    public sealed class CoreBulletVisuals
    {
        public CoreBulletVisuals(IReadOnlyDictionary<object, CoreBulletSource> sources, IResourceLocator resourceLocator,
            Sprite areaSprite)
        {
            _sources = sources;
            _resourceLocator = resourceLocator;
            _areaSprite = areaSprite;
        }

        // Null when the mapper recorded no source for the spawn's definition.
        public BulletVisual Get(in BattleEvent spawn)
        {
            object key = spawn.IsBeam ? spawn.BeamDefinition : spawn.BulletDefinition;
            if (key == null)
                return null;
            if (_visuals.TryGetValue(key, out var visual))
                return visual;

            // A beam bolt draws as a beam line (phase 5, step 4e).
            var isLine = spawn.IsBeam || spawn.BulletDefinition.IsBeamBolt;
            visual = _sources.TryGetValue(key, out var source) ? Create(source, isLine, spawn.BulletDefinition) : null;
            _visuals.Add(key, visual);
            return visual;
        }

        private BulletVisual Create(in CoreBulletSource source, bool isBeam, BulletDefinition bullet)
        {
            var parent = source.ParentColor.ToUnity();
            var size = source.BodySize;
            var prefab = source.Prefab;
            var flash = Flash(source.HitEffect, parent, size);

            // Legacy BulletFactory: without a record the bullet has no look of its own; its trigger effects draw it.
            // Until H11 decides the area views, the core scene draws the legacy flash glow at the area's diameter in its
            // colour, half transparent (phase 5, step 3).
            if (prefab == null)
            {
                if (isBeam)
                    return BulletVisual.ForLine(null, PlainBeamWidth, 0f, parent, flash);
                var area = parent;
                area.a *= AreaAlpha;
                return BulletVisual.ForSprite(_areaSprite, ScaleToDiameter(_areaSprite, bullet.Scale), area, flash);
            }

            var sprite = _resourceLocator.GetSprite(prefab.Image);
            var color = prefab.MainColorMode.Apply(prefab.MainColor.ToUnity(), parent);
            // Legacy BeamInitializer: thickness Size x BodySize. Only LaserView (laser and piercing laser) has a border,
            // Margins x BodySize; LightningView (energy beam, lightning) stretches the whole image.
            if (isBeam)
            {
                var border = prefab.Shape == BulletShape.LaserBeam || prefab.Shape == BulletShape.PiercingLaser ? prefab.Margins * size : 0f;
                return BulletVisual.ForLine(sprite, prefab.Size * size, border, color, flash);
            }

            // Legacy root scale BodySize, view scale Size.
            var spriteScale = prefab.Size * size;
            if (prefab.Shape != BulletShape.Rocket)
                return BulletVisual.ForSprite(sprite, spriteScale, color, flash);

            // Legacy TrailView: width 1 x the trail object's scale (0.5 x BodySize), half alpha.
            var trailColor = prefab.SecondColorMode.Apply(prefab.SecondColor.ToUnity(), parent);
            trailColor.a *= TrailAlpha;
            return BulletVisual.ForRocket(sprite, spriteScale, color, trailColor, TrailScale * size,
                (prefab.Margins + TrailGap) * size, flash);
        }

        // Legacy CreateVisualEffect and ShowHitEffectAction: the Hit trigger's size (1 when unset) x the bullet's world
        // scale (BodySize), its colour and lifetime. Without one legacy shows nothing; D11 keeps a generic flash of
        // size 1 in the bullet's parent colour.
        private static HitFlash Flash(BulletTrigger_PlaySfx trigger, Color parent, float bodySize)
        {
            if (trigger == null)
                return new HitFlash(bodySize, parent, GenericFlashDuration);

            var size = trigger.Size > 0f ? trigger.Size : 1f;
            var duration = trigger.Lifetime > 0f ? trigger.Lifetime : GenericFlashDuration;
            return new HitFlash(size * bodySize, trigger.ColorMode.Apply(trigger.Color.ToUnity(), parent), duration);
        }

        private const float GenericFlashDuration = 0.2f;
        private const float AreaAlpha = 0.5f;

        private static float ScaleToDiameter(Sprite sprite, float diameter)
        {
            if (!sprite)
                return diameter;

            var extent = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
            return extent > 0f ? diameter / extent : diameter;
        }

        // "Views (5)", item 5.
        private const float PlainBeamWidth = 0.2f;

        // Legacy CommonRocket prefab: trail scale, its gap behind the engine, and TrailView._alpha.
        private const float TrailScale = 0.5f;
        private const float TrailGap = 0.1f;
        private const float TrailAlpha = 0.5f;

        private readonly IReadOnlyDictionary<object, CoreBulletSource> _sources;
        private readonly IResourceLocator _resourceLocator;
        private readonly Sprite _areaSprite;
        private readonly Dictionary<object, BulletVisual> _visuals = new Dictionary<object, BulletVisual>();
    }
}
