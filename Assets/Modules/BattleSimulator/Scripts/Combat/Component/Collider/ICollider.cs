using System;
using Combat.Collision.Manager;
using Combat.Component.Unit;
using UnityEngine;

namespace Combat.Component.Collider
{
    public interface ICollider : IDisposable
    {
        bool Enabled { get; set; }

		IUnit Source { get; set; }
        IUnit Unit { get; set; }
        float MaxRange { get; set; }
        bool OneHitOnly { get; set; }
        float StuckTime { get; }

        IUnit ActiveCollision { get; }
        IUnit ActiveTrigger { get; }
        Vector2 LastContactPoint { get; }
        IUnit LastCollision { get; }

        void Initialize(ICollisionManager collisionManager);
        void UpdatePhysics(float elapsedTime);
    }
}
