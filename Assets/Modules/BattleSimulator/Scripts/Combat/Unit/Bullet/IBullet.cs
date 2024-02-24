using Combat.Component.Body;
using Combat.Component.Bullet.Lifetime;
using Combat.Component.Unit;

namespace Combat.Component.Bullet
{
    public interface IBullet : IUnit
    {
        ILifetime Lifetime { get; }
        void Detonate();
        bool CanBeDisarmed { get; }

        /// <summary>
        /// Returns a body that is attached to this bullet, and has world size of exactly 1
        /// </summary>
        IBody GetUnitSizedBody();
    }
}
