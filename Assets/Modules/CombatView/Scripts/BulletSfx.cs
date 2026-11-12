using CombatView.Effects;
using UnityEngine;

namespace CombatView
{
    // The legacy BulletTriggerCondition of a sound or effect trigger (migration phase 5b, step 6), without Undefined and
    // OutOfAmmo.
    public enum BulletSfxCondition : byte
    {
        Created,
        Destroyed,
        Hit,
        Disarmed,
        Expired,
        Detonated,
        Cooldown,
    }

    // One sound or effect trigger of a bullet definition (migration phase 5b, step 6) as plain data: a legacy
    // BulletTrigger_PlaySfx or BulletTrigger_SpawnStaticSfx, or the sound of a SpawnBullet trigger that the core runs.
    // The adapter resolves the clip and the record, and applies the trigger's colour mode to the bullet colour. Size is
    // the trigger size, 1 when unset.
    public sealed class BulletSfx
    {
        public BulletSfx(BulletSfxCondition condition, SoundClip sound, EffectRecord effect, Color color, float size,
            float lifetime, float cooldown, bool isStatic, bool oncePerCollision, bool useBulletPosition,
            bool syncLifetime, bool isSpawnSound)
        {
            Condition = condition;
            Sound = sound;
            Effect = effect;
            Color = color;
            Size = size;
            Lifetime = lifetime;
            Cooldown = cooldown;
            IsStatic = isStatic;
            OncePerCollision = oncePerCollision;
            UseBulletPosition = useBulletPosition;
            SyncLifetime = syncLifetime;
            IsSpawnSound = isSpawnSound;
        }

        // The sound of a SpawnBullet trigger (legacy SpawnBulletsAction): a bullet action, never stopped.
        public static BulletSfx SpawnSound(BulletSfxCondition condition, SoundClip sound, float cooldown) =>
            new BulletSfx(condition, sound, null, Color.white, 1f, 0f, cooldown, false, false, false, false, true);

        public BulletSfxCondition Condition { get; }

        // Empty for no sound; null for no effect.
        public SoundClip Sound { get; }
        public EffectRecord Effect { get; }

        public Color Color { get; }
        public float Size { get; }
        public float Lifetime { get; }
        public float Cooldown { get; }

        // SpawnStaticSfx: the effect stays where it starts and is Size x BodySize large.
        public bool IsStatic { get; }
        public bool OncePerCollision { get; }

        // PlaySfx only: a Hit effect on the bullet instead of the hit point; an effect that lives as long as the bullet.
        public bool UseBulletPosition { get; }
        public bool SyncLifetime { get; }
        public bool IsSpawnSound { get; }
    }
}
