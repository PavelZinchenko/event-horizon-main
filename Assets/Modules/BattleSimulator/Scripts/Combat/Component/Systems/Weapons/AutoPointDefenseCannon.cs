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
    public sealed class AutoPointDefenseCannon : WeaponBase
    {
        public AutoPointDefenseCannon(IWeaponPlatform platform, WeaponStats weaponStats,
            Factory.IBulletFactory bulletFactory, int keyBinding, IScene scene, IShip owner)
            : base(platform, weaponStats, bulletFactory, keyBinding)
        {
            _scene = scene;
            _owner = owner;
            _energyCost = bulletFactory.Stats.EnergyCost;
            MaxCooldown = weaponStats.FireRate > 0f ? 1f / weaponStats.FireRate : 0f;
            _bullets = BulletCompositeDisposable.Create(BulletFactory.Stats);
        }

        public override bool CanBeActivated => false;
        public override float Cooldown => 0f;
        protected override void OnUpdateView(float elapsedTime) { }

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            var target = FindTarget();
            SetTarget(target);
            if (!target.IsActive() || !Enabled || TimeFromLastUse < MaxCooldown ||
                !Platform.IsReady || !TryConsumeEnergy(_energyCost))
                return;

            Aim();
            Platform.OnShot();
            _bullets.Add(CreateBullet());
            TimeFromLastUse = 0f;
            InvokeTriggers(ConditionType.OnActivate);
        }

        protected override void OnDispose() => _bullets.Dispose();

        private IUnit FindTarget()
        {
            var position = Platform.Body.WorldPosition();
            var rangeSquared = Info.Range * Info.Range;
            IUnit nearestMissile = null;
            var nearestDistance = float.MaxValue;
            lock (_scene.Units.LockObject)
            {
                foreach (var unit in _scene.Units.Items)
                {
                    if (!unit.IsActive() || unit.Type.Class != UnitClass.Missile ||
                        !CombatRelations.AreEnemies(unit.Type, _owner.Type))
                        continue;
                    var distance = Vector2.SqrMagnitude(unit.Body.WorldPosition() - position);
                    if (distance > rangeSquared || distance >= nearestDistance)
                        continue;
                    nearestMissile = unit;
                    nearestDistance = distance;
                }
            }

            return nearestMissile ?? _scene.Ships.GetEnemyForTurret(_owner, position,
                Platform.Body.WorldRotation(), 360f, Info.Range, true);
        }

        private void SetTarget(IUnit target)
        {
            if (Platform is IUnitTargetingPlatform unitTargetingPlatform)
                unitTargetingPlatform.ActiveUnitTarget = target;
            else
                Platform.ActiveTarget = target as IShip;
        }

        private readonly IScene _scene;
        private readonly IShip _owner;
        private readonly float _energyCost;
        private readonly IBulletCompositeDisposable _bullets;
    }
}
