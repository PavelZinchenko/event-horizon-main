using System.Collections.Generic;
using CombatSimulation.Core;
using CombatSimulation.Physics;
using CombatView.Effects;
using UnityEngine;

namespace CombatView
{
    // The sound and effect triggers of one live bullet (migration phase 5b, step 6), as legacy BulletFactory built them.
    // Bullet actions: PlaySoundAction, PlayEffectAction, AttachEffectAction, SpawnEffectAction and the sound of
    // SpawnBulletsAction, each with its own cooldown. Collision actions on the bullet's own hits: PlayHitSoundAction,
    // ShowHitEffectAction, ShowMultipleHitEffectsAction and SpawnHitEffectAction. Times are battle seconds (tick x tick
    // duration), as legacy used Time.fixedTime. Legacy skipped short effects off screen; the view does not.
    internal sealed class BulletTriggerPlayer
    {
        public BulletTriggerPlayer(IReadOnlyList<BulletSfx> triggers, EffectPool effects, CombatSounds sounds)
        {
            _triggers = triggers;
            _effects = effects;
            _sounds = sounds;
            _soundTimes = new float[triggers.Count];
            _effectTimes = new float[triggers.Count];
            _bulletEffects = new IEffect[triggers.Count];
            for (var i = 0; i < triggers.Count; ++i)
                _soundTimes[i] = _effectTimes[i] = float.NegativeInfinity;
        }

        // The spawn event: the Created sounds. Legacy plays a plain one at once and a loop until the bullet goes.
        public void Start()
        {
            for (var i = 0; i < _triggers.Count; ++i)
                if (_triggers[i].Condition == BulletSfxCondition.Created)
                    PlaySound(i, float.NegativeInfinity);
        }

        // The first pose: the Created effects. lifetime: the seconds the bullet has left.
        public void Show(in BulletPose pose, float lifetime)
        {
            for (var i = 0; i < _triggers.Count; ++i)
                if (_triggers[i].Condition == BulletSfxCondition.Created)
                    RunEffect(i, pose, lifetime, float.NegativeInfinity);
        }

        // Each tick with a pose: the Cooldown triggers, the first at once.
        public void Tick(float time, in BulletPose pose, float lifetime)
        {
            for (var i = 0; i < _triggers.Count; ++i)
            {
                if (_triggers[i].Condition != BulletSfxCondition.Cooldown)
                    continue;
                PlaySound(i, time);
                RunEffect(i, pose, lifetime, time);
            }
        }

        // Legacy Bullet.OnCollision: the bullet actions on Hit. They run for the bullet that hits, and for a bullet that
        // is hit or disarmed.
        public void Collide(float time, in BulletPose pose, float lifetime)
        {
            for (var i = 0; i < _triggers.Count; ++i)
            {
                var trigger = _triggers[i];
                if (trigger.Condition != BulletSfxCondition.Hit)
                    continue;
                if (trigger.IsSpawnSound)
                    PlaySound(i, time);
                else if (!trigger.IsStatic && trigger.UseBulletPosition)
                    RunEffect(i, pose, lifetime, time);
            }
        }

        // Legacy BulletCollisionBehaviour: the collision actions of one hit by this bullet. A contact is new unless the
        // bullet hit the same target in the previous tick. Effects drift with the target; Hit effects are as large as
        // Size x the bullet's world scale, static ones as Size x BodySize.
        public void Hit(long tick, EntityId target, Vec2 point, Vector2 targetVelocity, in BulletPose pose)
        {
            var isNew = !_targetTicks.TryGetValue(target, out var last) || last < tick - 1;
            _targetTicks[target] = tick;
            var time = tick * Battle.TickDuration;

            for (var i = 0; i < _triggers.Count; ++i)
            {
                var trigger = _triggers[i];
                if (trigger.Condition != BulletSfxCondition.Hit || trigger.IsSpawnSound)
                    continue;

                if (!trigger.Sound.IsEmpty && (isNew || !trigger.OncePerCollision) &&
                    time - _soundTimes[i] >= trigger.Cooldown)
                {
                    _sounds.Play(trigger.Sound);
                    _soundTimes[i] = time;
                }

                if (trigger.Effect == null)
                    continue;
                if (trigger.IsStatic)
                    SpawnHitEffect(i, time, isNew, point, targetVelocity, pose);
                else if (!trigger.UseBulletPosition)
                    ShowHitEffect(i, target, point, targetVelocity, pose.WorldScale);
            }
        }

        // The removal: the bullet actions of its conditions at its last pose, then legacy Bullet.Dispose. An effect on
        // the bullet stays in the world at its own size (PlayEffectAction) or goes with it (AttachEffectAction); a
        // looped sound stops. Without a pose only the sounds play.
        public void Remove(BulletTriggerConditions conditions, in BulletPose pose, bool hasPose)
        {
            if (conditions != BulletTriggerConditions.None)
            {
                // Legacy runs these on the bullet and detaches them at once: they start in the world.
                var world = new BulletPose(null, pose.Point, pose.Degrees, pose.BodySize, pose.WorldScale);
                for (var i = 0; i < _triggers.Count; ++i)
                {
                    if (!Fired(_triggers[i].Condition, conditions))
                        continue;
                    PlaySound(i, float.NegativeInfinity);
                    if (hasPose)
                        RunEffect(i, world, 0f, float.NegativeInfinity);
                }
            }

            for (var i = 0; i < _bulletEffects.Length; ++i)
            {
                var effect = _bulletEffects[i];
                if (effect == null || !effect.IsAlive || IsHitEffect(_triggers[i]))
                    continue;
                if (hasPose && !_triggers[i].SyncLifetime)
                    _effects.Detach(effect, pose.Point, pose.Degrees);
                else
                    effect.Dispose();
            }

            if (_loops != null)
                foreach (var id in _loops)
                    _sounds.Stop(id);
        }

        // A ShowHitEffectAction effect is in the world from the start; the bullet's removal leaves it.
        private static bool IsHitEffect(BulletSfx trigger) =>
            trigger.Condition == BulletSfxCondition.Hit && !trigger.IsStatic && !trigger.UseBulletPosition;

        private static bool Fired(BulletSfxCondition condition, BulletTriggerConditions conditions)
        {
            switch (condition)
            {
                case BulletSfxCondition.Destroyed: return (conditions & BulletTriggerConditions.Destroyed) != 0;
                case BulletSfxCondition.Disarmed: return (conditions & BulletTriggerConditions.Disarmed) != 0;
                case BulletSfxCondition.Expired: return (conditions & BulletTriggerConditions.Expired) != 0;
                case BulletSfxCondition.Detonated: return (conditions & BulletTriggerConditions.Detonated) != 0;
                default: return false;
            }
        }

        // PlaySoundAction or the sound of SpawnBulletsAction. A loop of a bullet action stops with the bullet.
        private void PlaySound(int index, float time)
        {
            var trigger = _triggers[index];
            if (trigger.Sound.IsEmpty || !Ready(_soundTimes, index, time, trigger.Cooldown))
                return;

            if (!trigger.Sound.Loop || trigger.IsSpawnSound)
            {
                _sounds.Play(trigger.Sound);
                return;
            }

            var id = _sounds.Start(trigger.Sound);
            if (id != 0)
                (_loops ??= new List<int>()).Add(id);
        }

        // A bullet action with an effect. SpawnEffectAction: a new effect at the bullet, Size x BodySize. PlayEffectAction
        // and AttachEffectAction: one effect on the bullet, run again at each call; the attached one lasts the bullet's
        // remaining lifetime. A pose without a root (a removal) starts a PlayEffectAction effect in the world, where legacy
        // Detach would leave it, and drops an AttachEffectAction one, which legacy disposed with the bullet.
        private void RunEffect(int index, in BulletPose pose, float lifetime, float time)
        {
            var trigger = _triggers[index];
            if (trigger.Effect == null || trigger.IsSpawnSound || !Ready(_effectTimes, index, time, trigger.Cooldown))
                return;

            if (trigger.IsStatic)
            {
                var effect = _effects.Create(trigger.Effect, pose.Point);
                if (effect == null)
                    return;
                effect.Rotation = pose.Degrees;
                effect.Color = trigger.Color;
                effect.Size = trigger.Size * pose.BodySize;
                effect.Run(trigger.Lifetime, Vector2.zero, 0f);
                return;
            }

            if (!pose.Root)
            {
                if (trigger.SyncLifetime)
                    return;
                var detached = _effects.Create(trigger.Effect, pose.Point);
                if (detached == null)
                    return;
                detached.Rotation = pose.Degrees;
                detached.Color = trigger.Color;
                detached.Size = trigger.Size;
                detached.Run(trigger.Lifetime, Vector2.zero, 0f);
                return;
            }

            var attached = _bulletEffects[index];
            if (attached == null || !attached.IsAlive)
            {
                attached = _effects.Create(trigger.Effect, pose.Root);
                _bulletEffects[index] = attached;
                if (attached == null)
                    return;
                attached.Color = trigger.Color;
                attached.Size = trigger.Size;
            }

            attached.Run(trigger.SyncLifetime ? lifetime : trigger.Lifetime, Vector2.zero, 0f);
        }

        // ShowHitEffectAction: one effect at the latest hit, run again at each; OncePerCollision keeps one per target.
        private void ShowHitEffect(int index, EntityId target, Vec2 point, Vector2 velocity, float worldScale)
        {
            var trigger = _triggers[index];
            IEffect effect;
            Dictionary<EntityId, IEffect> perTarget = null;
            if (trigger.OncePerCollision)
            {
                _targetEffects ??= new Dictionary<EntityId, IEffect>[_triggers.Count];
                perTarget = _targetEffects[index] ??= new Dictionary<EntityId, IEffect>();
                perTarget.TryGetValue(target, out effect);
            }
            else
                effect = _bulletEffects[index];

            if (effect == null || !effect.IsAlive)
            {
                effect = _effects.Create(trigger.Effect, point);
                if (effect == null)
                    return;
                effect.Color = trigger.Color;
                if (perTarget != null)
                    perTarget[target] = effect;
                else
                    _bulletEffects[index] = effect;
            }
            else
                _effects.MoveTo(effect, point);

            effect.Size = trigger.Size * worldScale;
            effect.Run(trigger.Lifetime, velocity, 0f);
        }

        // SpawnHitEffectAction: a new effect at each hit, turned as the bullet; OncePerCollision takes new contacts only.
        private void SpawnHitEffect(int index, float time, bool isNew, Vec2 point, Vector2 velocity, in BulletPose pose)
        {
            var trigger = _triggers[index];
            if (time - _effectTimes[index] < trigger.Cooldown || trigger.OncePerCollision && !isNew)
                return;

            _effectTimes[index] = time;
            var effect = _effects.Create(trigger.Effect, point);
            if (effect == null)
                return;
            effect.Rotation = pose.Degrees;
            effect.Color = trigger.Color;
            effect.Size = trigger.Size * pose.BodySize;
            effect.Run(trigger.Lifetime, velocity, 0f);
        }

        // Legacy Cooldown.TryUpdate: the first call passes. A call without a time (a condition that fires once) always
        // passes.
        private static bool Ready(float[] times, int index, float time, float cooldown)
        {
            if (float.IsNegativeInfinity(time))
                return true;
            if (cooldown > 0f && time - times[index] < cooldown)
                return false;
            times[index] = time;
            return true;
        }

        private readonly IReadOnlyList<BulletSfx> _triggers;
        private readonly EffectPool _effects;
        private readonly CombatSounds _sounds;
        private readonly float[] _soundTimes;
        private readonly float[] _effectTimes;

        // Per trigger: the effect on the bullet (bullet actions) or the latest hit effect (ShowHitEffectAction).
        private readonly IEffect[] _bulletEffects;
        private Dictionary<EntityId, IEffect>[] _targetEffects;
        private readonly Dictionary<EntityId, long> _targetTicks = new Dictionary<EntityId, long>();
        private List<int> _loops;
    }

    // Where a bullet is for its triggers: its root (null when it has none to carry effects), its drawn point and angle,
    // the legacy BodySize and its world scale (BodySize x the aura power).
    internal readonly struct BulletPose
    {
        public BulletPose(Transform root, Vec2 point, float degrees, float bodySize, float worldScale)
        {
            Root = root;
            Point = point;
            Degrees = degrees;
            BodySize = bodySize;
            WorldScale = worldScale;
        }

        public readonly Transform Root;
        public readonly Vec2 Point;
        public readonly float Degrees;
        public readonly float BodySize;
        public readonly float WorldScale;
    }
}
