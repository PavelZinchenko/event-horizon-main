using Combat.Component.Bullet;
using Combat.Component.Platform;
using Combat.Component.Ship;
using Combat.Component.Triggers;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Combat.Unit;
using GameDatabase.DataModel;
using UnityEngine;

namespace Combat.Component.Systems.Weapons
{
    public sealed class AutoPointDefenseLaser : WeaponBase
    {
        public AutoPointDefenseLaser(IWeaponPlatform platform, WeaponStats weaponStats, Factory.IBulletFactory bulletFactory,
            int keyBinding, IScene scene, IShip owner)
            : base(platform, weaponStats, bulletFactory, keyBinding)
        {
            _scene = scene;
            _owner = owner;
            _energyConsumption = bulletFactory.Stats.EnergyCost;
        }

        public override bool CanBeActivated => false;
        public override float Cooldown => 0f;
        public override IBullet ActiveBullet => HasActiveBullet ? _activeBullet : null;

        protected override void OnUpdateView(float elapsedTime) {}

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            var target = FindTarget();
            if (!IsTargetInsideWeaponRange(target))
                target = null;
            SetTarget(target);

            // Recreate the live beam immediately when a missile preempts a
            // ship target. Some bound beam controllers retain their original
            // target until destruction, which delayed missile interception.
            if (target != _currentTarget && HasActiveBullet)
            {
                _activeBullet.Vanish();
                TimeFromLastUse = 0f;
                InvokeTriggers(ConditionType.OnDeactivate);
            }
            _currentTarget = target;

            if (!target.IsActive())
            {
                if (HasActiveBullet)
                {
                    _activeBullet.Vanish();
                    TimeFromLastUse = 0;
                    InvokeTriggers(ConditionType.OnDeactivate);
                }

                return;
            }

            if (HasActiveBullet)
            {
                Aim();
                if (TryConsumeEnergy(_energyConsumption * elapsedTime))
                {
                    _activeBullet.Lifetime.Restore();
                    InvokeTriggers(ConditionType.OnRemainActive);
                    return;
                }
            }
            else if (TryConsumeEnergy(ActivationCost))
            {
                Aim();
                _activeBullet = CreateBullet();
                _activeBullet.Lifetime.Restore();
                InvokeTriggers(ConditionType.OnActivate);
                return;
            }

            if (HasActiveBullet)
            {
                _activeBullet.Vanish();
                TimeFromLastUse = 0;
                InvokeTriggers(ConditionType.OnDeactivate);
            }
        }

        protected override void OnDispose()
        {
            if (BulletFactory.Stats.IsBoundToCannon)
                _activeBullet?.Vanish();
        }

        private IUnit FindTarget()
        {
            var position = Platform.Body.WorldPosition();
            var range = Info.Range;
            IUnit nearestMissile = null;
            var missileDistance = float.MaxValue;

            lock (_scene.Units.LockObject)
            {
                foreach (var unit in _scene.Units.Items)
                {
                    if (!unit.IsActive() || unit.Type.Class != UnitClass.Missile || !CombatRelations.AreEnemies(unit.Type, _owner.Type))
                        continue;

                    var distance = Vector2.SqrMagnitude(unit.Body.WorldPosition() - position);
                    if (distance > range * range || distance >= missileDistance)
                        continue;

                    nearestMissile = unit;
                    missileDistance = distance;
                }
            }

            if (nearestMissile != null)
                return nearestMissile;

            return _scene.Ships.GetEnemyForTurret(_owner, position, Platform.Body.WorldRotation(), Platform.AutoAimingAngle, range);
        }

        private void SetTarget(IUnit target)
        {
            if (Platform is IUnitTargetingPlatform unitTargetingPlatform)
                unitTargetingPlatform.ActiveUnitTarget = target;
            else
                Platform.ActiveTarget = target as IShip;
        }

        private bool IsTargetInsideWeaponRange(IUnit target)
        {
            if (!target.IsActive())
                return false;

            var position = Platform.Body.WorldPosition();
            var range = Info.Range;
            return Vector2.SqrMagnitude(target.Body.WorldPosition() - position) <= range * range;
        }

        private bool HasActiveBullet => _activeBullet.IsActive();

        private readonly IScene _scene;
        private readonly IShip _owner;
        private readonly float _energyConsumption;
        private IBullet _activeBullet;
        private IUnit _currentTarget;
    }
}
