using System.Collections.Generic;
using CombatSimulation.Core;
using CombatView;
using EffectColorMode = CombatView.Effects.ColorMode;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Extensions;
using Services.Resources;
using UnityEngine;

namespace Combat.Integration.Core
{
    // Turns the bullet sources of the mapper into views (phase 4, step 5, "Views (5)"), once per definition at its first
    // spawn. Phase 5b, step 5: one BulletLook per BulletPrefab record, as legacy PrefabCache.GetBulletPrefab made one
    // prefab per record; the renderer's shape prefabs apply it as the legacy initializers did.
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

            // A beam bolt draws as a beam line (phase 5, step 4e). A charged copy (step 4f) has the source of its
            // uncharged definition, at legacy SizeMultiplier 0.5 + 0.5 x charge.
            var isLine = spawn.IsBeam || spawn.BulletDefinition.IsBeamBolt;
            object sourceKey = spawn.IsBeam ? spawn.BeamDefinition.Uncharged : (object)spawn.BulletDefinition.Uncharged;
            var charge = spawn.IsBeam ? spawn.BeamDefinition.Charge : spawn.BulletDefinition.Charge;
            visual = _sources.TryGetValue(sourceKey, out var source)
                ? Create(source, isLine, spawn.BulletDefinition, 0.5f + 0.5f * charge)
                : null;
            _visuals.Add(key, visual);
            return visual;
        }

        private BulletVisual Create(in CoreBulletSource source, bool isBeam, BulletDefinition bullet, float sizeScale)
        {
            if (source.Hidden)
                return BulletVisual.Hidden;

            // Legacy BulletFactory.ConfigureView: the view colour is the weapon's bullet colour; the root scale BodySize.
            var parent = source.ParentColor.ToUnity();
            var size = source.BodySize * sizeScale;
            var prefab = source.Prefab;
            var flash = Flash(source.HitEffect, parent, size);

            // Legacy BulletFactory: without a record the bullet has no look of its own (the AreaOfEffect prefab is
            // empty); its trigger effects draw it. Until step 6 gives them their effects, the core scene draws the legacy
            // flash glow at the area's diameter in its colour, half transparent (phase 5, step 3), and a plain beam
            // ("Views (5)", item 5).
            if (prefab == null)
                return isBeam
                    ? new BulletVisual(_plainBeam ??= PlainBeam(), 1f, parent, flash)
                    : new BulletVisual(_area ??= Area(_areaSprite), bullet.Scale, parent, flash);

            if (!_looks.TryGetValue(prefab, out var look))
            {
                look = new BulletLook(Kind(prefab.Shape), _resourceLocator.GetSprite(prefab.Image), prefab.MainColor.ToUnity(),
                    (EffectColorMode)prefab.MainColorMode, prefab.SecondColor.ToUnity(), (EffectColorMode)prefab.SecondColorMode,
                    prefab.Size, prefab.Margins);
                _looks.Add(prefab, look);
            }

            return new BulletVisual(look, size, parent, flash);
        }

        // Legacy PrefabCache.GetBulletPrefab: the shape picks the prefab.
        private static BulletViewKind Kind(BulletShape shape)
        {
            switch (shape)
            {
                case BulletShape.Rocket: return BulletViewKind.Rocket;
                case BulletShape.LaserBeam: return BulletViewKind.Laser;
                case BulletShape.LightningBolt: return BulletViewKind.Lightning;
                case BulletShape.EnergyBeam: return BulletViewKind.EnergyBeam;
                case BulletShape.PiercingLaser: return BulletViewKind.PiercingLaser;
                case BulletShape.Spark: return BulletViewKind.Spark;
                case BulletShape.Mine: return BulletViewKind.Mine;
                case BulletShape.Wave: return BulletViewKind.Arc;
                case BulletShape.BlackHole: return BulletViewKind.BlackHole;
                case BulletShape.Harpoon: return BulletViewKind.Harpoon;
                case BulletShape.CircularSaw: return BulletViewKind.CircularSaw;
                default: return BulletViewKind.Projectile;
            }
        }

        // The projectile prefab's sprite (alpha scale 1.5) at a diameter of 1, its colour times half the area colour.
        private static BulletLook Area(Sprite sprite) =>
            new BulletLook(BulletViewKind.Projectile, sprite, new Color(1f, 1f, 1f, AreaAlpha / ProjectileAlphaScale),
                EffectColorMode.Multiply, Color.white, EffectColorMode.TakeFromOwner, ScaleToDiameter(sprite, 1f), 0f);

        // The laser prefab without a texture or border, PlainBeamWidth wide.
        private static BulletLook PlainBeam() =>
            new BulletLook(BulletViewKind.Laser, null, Color.white, EffectColorMode.TakeFromOwner, Color.white,
                EffectColorMode.TakeFromOwner, PlainBeamWidth, 0f);

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

        // The CommonProjectile prefab's SpriteView._alphaScale.
        private const float ProjectileAlphaScale = 1.5f;

        private readonly IReadOnlyDictionary<object, CoreBulletSource> _sources;
        private readonly IResourceLocator _resourceLocator;
        private readonly Sprite _areaSprite;
        private readonly Dictionary<object, BulletVisual> _visuals = new Dictionary<object, BulletVisual>();
        private readonly Dictionary<BulletPrefab, BulletLook> _looks = new Dictionary<BulletPrefab, BulletLook>();
        private BulletLook _area;
        private BulletLook _plainBeam;
    }
}
