using Combat.Component.Bullet;
using Combat.Component.Platform;
using UnityEngine;

namespace Combat.Factory
{
    public interface IBulletFactory
    {
        IBullet Create(IWeaponPlatform parent, float spread, float rotation, Vector2 offset);
        IBulletStats Stats { get; }
    }
}
