using Combat.Component.Engine;
using Combat.Component.Body;
using Combat.Component.Ship;
using Combat.Component.Triggers;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Combat.Unit;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using System.Linq;
using UnityEngine;

namespace Combat.Component.Systems.Devices
{
    // The droplet's guidance system is an energy-free click toggle.  It keeps
    // the craft at maximum speed and snaps its nose to the current target on
    // every physics frame; there is deliberately no lead calculation.
    public sealed class SophonGuidanceDevice : SystemBase, IDevice, IEngineModification
    {
        public SophonGuidanceDevice(IShip ship, DeviceStats stats, int keyBinding, IScene scene)
            : base(keyBinding, stats.ControlButtonIcon)
        {
            _ship = ship;
            _scene = scene;
            DeviceClass = stats.DeviceClass;
            MaxCooldown = stats.Cooldown;
        }

        public DeviceClass DeviceClass { get; }
        public override IEngineModification EngineModification => this;
        public override bool CanBeActivated => base.CanBeActivated;

        public bool TryApplyModification(ref EngineData data)
        {
            if (!_enabled)
                return true;

            data.Throttle = 1f;
            data.Deceleration = 0f;
            return true;
        }

        public void Deactivate()
        {
            if (!_enabled)
                return;
            _enabled = false;
            TimeFromLastUse = 0f;
            InvokeTriggers(ConditionType.OnDeactivate);
        }

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            // AI controllers do not consistently hold a manually bound device
            // key.  A non-player droplet always runs its guidance system while
            // the installed device is available; the player still controls the
            // normal click-toggle path.
            var shouldRun = Active || _ship.Type.Side != UnitSide.Player;
            if (!shouldRun || !CanBeActivated)
            {
                Deactivate();
                return;
            }

            if (!_enabled)
            {
                _enabled = true;
                InvokeTriggers(ConditionType.OnActivate);
            }
            else
            {
                InvokeTriggers(ConditionType.OnRemainActive);
            }

            var target = _ship.Type.Side == UnitSide.Player ? _scene.LockedEnemyShip : null;
            if (_ship.Type.Side != UnitSide.Player &&
                (!target.IsActive() || !CombatRelations.AreEnemies(_ship.Type, target.Type)))
            {
                lock (_scene.Ships.LockObject)
                {
                    target = _scene.Ships.Items
                        .Where(item => item.IsActive() && CombatRelations.AreEnemies(_ship.Type, item.Type))
                        .OrderBy(item => Vector2.SqrMagnitude(item.Body.WorldPosition() - _ship.Body.WorldPosition()))
                        .FirstOrDefault();
                }
            }
            if (!target.IsActive() || !CombatRelations.AreEnemies(_ship.Type, target.Type))
                return;

            var delta = target.Body.WorldPosition() - _ship.Body.WorldPosition();
            var rotation = RotationHelpers.Angle(delta);
            _ship.Body.Turn(rotation);
            _ship.Body.ApplyAngularAcceleration(-_ship.Body.AngularVelocity);

            // Reorient the existing motion immediately and top it up to the
            // engine's current maximum.  This prevents a forced snap-turn from
            // bleeding speed or leaving the craft sliding sideways.
            var maximumSpeed = Mathf.Max(0f, _ship.Engine.MaxVelocity);
            var desiredVelocity = RotationHelpers.Direction(rotation) * maximumSpeed;
            if (_ship.Body is RigidBodyAdapter rigidBody)
            {
                // A force-based correction could be delayed by the physics
                // step (and by a very light body), leaving the visible nose
                // pointing at the target while momentum continued sideways.
                // Guidance is defined as an immediate course snap, so update
                // the rigid body state atomically every physics frame.
                rigidBody.Rotation = rotation;
                rigidBody.AngularVelocity = 0f;
                rigidBody.Velocity = desiredVelocity;
            }
            else
            {
                _ship.Body.ApplyAcceleration(desiredVelocity - _ship.Body.Velocity);
            }
            _ship.Controls.Course = rotation;
            _ship.Controls.Throttle = 1f;
        }

        protected override void OnUpdateView(float elapsedTime) { }
        protected override void OnDispose() { }

        private readonly IShip _ship;
        private readonly IScene _scene;
        private bool _enabled;
    }
}
