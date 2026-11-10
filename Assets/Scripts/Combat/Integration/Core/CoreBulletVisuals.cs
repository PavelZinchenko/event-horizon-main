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
    // prefab per record; the renderer's shape prefabs apply it as the legacy initializers did. Step 6: the sound and
    // effect triggers of the ammunition (legacy BulletFactory.BulletTriggerBuilder), and the sounds of the SpawnBullet
    // triggers that the core runs.
    public sealed class CoreBulletVisuals
    {
        public CoreBulletVisuals(IReadOnlyDictionary<object, CoreBulletSource> sources,
            IReadOnlyDictionary<BulletTriggerDefinition, BulletTrigger_SpawnBullet> spawnTriggers,
            IResourceLocator resourceLocator, CoreVisualEffects effects, CoreSoundOutput sounds, Sprite areaSprite)
        {
            _sources = sources;
            _spawnTriggers = spawnTriggers;
            _resourceLocator = resourceLocator;
            _effects = effects;
            _sounds = sounds;
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
            // uncharged definition, at legacy SizeMultiplier 0.5 + 0.5 x charge; its triggers are copies of the uncharged
            // ones in the same order.
            var isLine = spawn.IsBeam || spawn.BulletDefinition.IsBeamBolt;
            object sourceKey = spawn.IsBeam ? spawn.BeamDefinition.Uncharged : (object)spawn.BulletDefinition.Uncharged;
            var charge = spawn.IsBeam ? spawn.BeamDefinition.Charge : spawn.BulletDefinition.Charge;
            var coreTriggers = spawn.IsBeam ? spawn.BeamDefinition.Uncharged.Triggers : spawn.BulletDefinition.Uncharged.Triggers;
            visual = _sources.TryGetValue(sourceKey, out var source)
                ? Create(source, isLine, spawn.BulletDefinition, 0.5f + 0.5f * charge, coreTriggers)
                : null;
            _visuals.Add(key, visual);
            return visual;
        }

        private BulletVisual Create(in CoreBulletSource source, bool isBeam, BulletDefinition bullet, float sizeScale,
            IReadOnlyList<BulletTriggerDefinition> coreTriggers)
        {
            // Legacy BulletFactory.ConfigureView: the view colour is the weapon's bullet colour; the root scale BodySize.
            var parent = source.ParentColor.ToUnity();
            var size = source.BodySize * sizeScale;
            var prefab = source.Prefab;
            var triggers = source.Ammunition != null ? Triggers(source.Ammunition, coreTriggers, parent) : null;

            // Legacy BulletFactory: without a record the bullet has no look of its own (the AreaOfEffect prefab is
            // empty); its trigger effects draw it (step 6). A beam keeps a plain line ("Views (5)", item 5). The
            // detonator explosion has no ammunition: it keeps the step 3 glow, the legacy flash in its colour at half
            // opacity, until its effect record (step 7).
            if (prefab == null)
            {
                if (isBeam)
                    return new BulletVisual(_plainBeam ??= PlainBeam(), 1f, parent, triggers);
                if (source.Ammunition == null)
                    return new BulletVisual(_area ??= Area(_areaSprite), bullet.Scale, parent);
                return new BulletVisual(null, size, parent, triggers);
            }

            if (!_looks.TryGetValue(prefab, out var look))
            {
                look = new BulletLook(Kind(prefab.Shape), _resourceLocator.GetSprite(prefab.Image), prefab.MainColor.ToUnity(),
                    (EffectColorMode)prefab.MainColorMode, prefab.SecondColor.ToUnity(), (EffectColorMode)prefab.SecondColorMode,
                    prefab.Size, prefab.Margins);
                _looks.Add(prefab, look);
            }

            return new BulletVisual(look, size, parent, triggers);
        }

        // Legacy BulletTriggerBuilder.Build: PlaySfx and SpawnStaticSfx in database order, with the trigger colour by its
        // mode on the bullet colour and a size of 1 when unset. Then the sound of each SpawnBullet trigger that the core
        // runs: one at the nesting limit has no action in legacy and is not among them. Not copied: legacy plays an
        // OutOfAmmo sound or effect at the creation (its condition maps to none) when a spawn reached the nesting limit.
        // No built-in ammunition has one.
        private List<BulletSfx> Triggers(Ammunition ammunition, IReadOnlyList<BulletTriggerDefinition> coreTriggers,
            Color parent)
        {
            List<BulletSfx> result = null;
            foreach (var trigger in ammunition.Triggers)
            {
                if (!TryGetCondition(trigger.Condition, out var condition))
                    continue;

                switch (trigger)
                {
                    case BulletTrigger_PlaySfx sfx:
                        Add(ref result, condition, sfx.AudioClip, sfx.VisualEffect, sfx.ColorMode.Apply(sfx.Color.ToUnity(), parent),
                            sfx.Size, sfx.Lifetime, sfx.Cooldown, false, sfx.OncePerCollision, sfx.UseBulletPosition,
                            sfx.SyncLifetimeWithBullet);
                        break;
                    case BulletTrigger_SpawnStaticSfx sfx:
                        Add(ref result, condition, sfx.AudioClip, sfx.VisualEffect, sfx.ColorMode.Apply(sfx.Color.ToUnity(), parent),
                            sfx.Size, sfx.Lifetime, sfx.Cooldown, true, sfx.OncePerCollision, false, false);
                        break;
                }
            }

            foreach (var core in coreTriggers)
            {
                if (!_spawnTriggers.TryGetValue(core, out var spawn))
                    continue;
                var sound = _sounds.Resolve(spawn.AudioClip);
                if (sound.IsEmpty)
                    continue;
                // An OutOfAmmo spawn has the conditions of the spawns that reached the limit.
                for (var condition = BulletSfxCondition.Created; condition <= BulletSfxCondition.Detonated; ++condition)
                    if ((core.Conditions & ToCore(condition)) != 0)
                        (result ??= new List<BulletSfx>()).Add(BulletSfx.SpawnSound(condition, sound, spawn.Cooldown));
            }

            return result;
        }

        private void Add(ref List<BulletSfx> result, BulletSfxCondition condition, GameDatabase.Model.AudioClipId clip,
            VisualEffect effect, Color color, float size, float lifetime, float cooldown, bool isStatic,
            bool oncePerCollision, bool useBulletPosition, bool syncLifetime)
        {
            var sound = _sounds.Resolve(clip);
            var record = _effects.Get(effect);
            if (sound.IsEmpty && record == null)
                return;

            (result ??= new List<BulletSfx>()).Add(new BulletSfx(condition, sound, record, color, size > 0f ? size : 1f,
                lifetime, cooldown, isStatic, oncePerCollision, useBulletPosition, syncLifetime, false));
        }

        private static bool TryGetCondition(BulletTriggerCondition condition, out BulletSfxCondition result)
        {
            switch (condition)
            {
                case BulletTriggerCondition.Created: result = BulletSfxCondition.Created; return true;
                case BulletTriggerCondition.Destroyed: result = BulletSfxCondition.Destroyed; return true;
                case BulletTriggerCondition.Hit: result = BulletSfxCondition.Hit; return true;
                case BulletTriggerCondition.Disarmed: result = BulletSfxCondition.Disarmed; return true;
                case BulletTriggerCondition.Expired: result = BulletSfxCondition.Expired; return true;
                case BulletTriggerCondition.Detonated: result = BulletSfxCondition.Detonated; return true;
                case BulletTriggerCondition.Cooldown: result = BulletSfxCondition.Cooldown; return true;
                default: result = default; return false;
            }
        }

        private static BulletTriggerConditions ToCore(BulletSfxCondition condition)
        {
            switch (condition)
            {
                case BulletSfxCondition.Created: return BulletTriggerConditions.Created;
                case BulletSfxCondition.Destroyed: return BulletTriggerConditions.Destroyed;
                case BulletSfxCondition.Hit: return BulletTriggerConditions.Hit;
                case BulletSfxCondition.Disarmed: return BulletTriggerConditions.Disarmed;
                case BulletSfxCondition.Expired: return BulletTriggerConditions.Expired;
                case BulletSfxCondition.Detonated: return BulletTriggerConditions.Detonated;
                default: return BulletTriggerConditions.None;
            }
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
        private readonly IReadOnlyDictionary<BulletTriggerDefinition, BulletTrigger_SpawnBullet> _spawnTriggers;
        private readonly IResourceLocator _resourceLocator;
        private readonly CoreVisualEffects _effects;
        private readonly CoreSoundOutput _sounds;
        private readonly Sprite _areaSprite;
        private readonly Dictionary<object, BulletVisual> _visuals = new Dictionary<object, BulletVisual>();
        private readonly Dictionary<BulletPrefab, BulletLook> _looks = new Dictionary<BulletPrefab, BulletLook>();
        private BulletLook _area;
        private BulletLook _plainBeam;
    }
}
