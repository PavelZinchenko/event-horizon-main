using System;
using Combat.Collision;
using Combat.Component.Unit;

namespace Combat.Component.DamageHandler
{
    public interface IDamageHandler : IDisposable
    {
        CollisionEffect ApplyDamage(Impact impact, IUnit source);
    }
}
