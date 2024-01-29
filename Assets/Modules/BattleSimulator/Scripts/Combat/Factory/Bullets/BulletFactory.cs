using System;
using System.Collections.Generic;
using Combat.Collision.Behaviour;
using Combat.Collision.Behaviour.Action;
using Combat.Component.Body;
using Combat.Component.Bullet;
using Combat.Component.Bullet.Action;
using Combat.Component.Bullet.Cooldown;
using Combat.Component.Bullet.Lifetime;
using Combat.Component.Collider;
using Combat.Component.Controller;
using Combat.Component.DamageHandler;
using Combat.Component.Physics;
using Combat.Component.Platform;
using Combat.Component.Ship;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Component.View;
using Combat.Helpers;
using Combat.Scene;
using Constructor;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Extensions;
using GameDatabase.Model;
using Services.Audio;
using Services.ObjectPool;
using UnityEngine;

namespace Combat.Factory
{
    public class BulletFactory : IBulletFactory
    {
        public BulletFactory(Ammunition ammunition, WeaponStatModifier statModifier, IScene scene,
            ISoundPlayer soundPlayer, IObjectPool objectPool, PrefabCache prefabCache,
            SpaceObjectFactory spaceObjectFactory, EffectFactory effectFactory, IShip owner)
        {
            _ammunition = ammunition;
            _statModifier = statModifier;
            _scene = scene;
            _soundPlayer = soundPlayer;
            _objectPool = objectPool;
            _spaceObjectFactory = spaceObjectFactory;
            _effectFactory = effectFactory;
            _prefabCache = prefabCache;
            _owner = owner;

            _prefab = new Lazy<GameObject>(() => _prefabCache.GetBulletPrefab(_ammunition.Body.BulletPrefab));
            _stats = new BulletStats(ammunition, statModifier);
        }

        public IBulletStats Stats
        {
            get { return _stats; }
        }

        public IBullet Create(IWeaponPlatform parent, float spread, float rotation, Vector2 offset)
        {
            var bulletGameObject = new GameObjectHolder(_prefab, _objectPool);
            bulletGameObject.IsActive = true;

            var bulletSpeed = _stats.GetBulletSpeed();

            var body = ConfigureBody(bulletGameObject.GetComponent<IBodyComponent>(), parent, bulletSpeed, spread,
                rotation, offset);
            var view = ConfigureView(bulletGameObject.GetComponent<IView>(), _stats.Color);

            var bullet = CreateUnit(body, view, bulletGameObject);
            var collisionBehaviour = CreateCollisionBehaviour(bullet);
            bullet.Collider = ConfigureCollider(bulletGameObject.GetComponent<ICollider>(true), bullet);
            bullet.CollisionBehaviour = collisionBehaviour;
            bullet.Controller = CreateController(parent, bullet, bulletSpeed, spread, rotation);
            bullet.DamageHandler = CreateDamageHandler(bullet);
            bullet.CanBeDisarmed = _ammunition.Body.CanBeDisarmed;
            BulletTriggerBuilder.Build(this, bullet, collisionBehaviour);

            _scene.AddUnit(bullet);
            bullet.UpdateView(0);
            bullet.AddResource(bulletGameObject);
            return bullet;
        }

        private bool CanSiphonHitpoints()
        {
            for (int i = 0; i < _ammunition.Effects.Count; ++i)
                if (_ammunition.Effects[i].Type == ImpactEffectType.SiphonHitPoints)
                    return true;
            return false;
        }

        private BulletCollisionBehaviour CreateCollisionBehaviour(IBullet bullet)
        {
            var collisionBehaviour = new BulletCollisionBehaviour();
            var impactType = _ammunition.ImpactType;

            for (int i = 0; i < _ammunition.Effects.Count; ++i)
            {
                var effect = _ammunition.Effects[i];
                if (effect.Type == ImpactEffectType.Damage)
                    collisionBehaviour.AddAction(new DealDamageAction(effect.DamageType, effect.Power * _stats.DamageMultiplier, impactType));
                else if (effect.Type == ImpactEffectType.Push)
                    collisionBehaviour.AddAction(new PushAction(effect.Power, impactType));
                else if (effect.Type == ImpactEffectType.Pull)
                    collisionBehaviour.AddAction(new PushAction(-effect.Power, impactType));
                else if (effect.Type == ImpactEffectType.DrainEnergy)
                    collisionBehaviour.AddAction(new DrainEnergyAction(effect.Power, impactType));
                else if (effect.Type == ImpactEffectType.SiphonHitPoints)
                    collisionBehaviour.AddAction(new SiphonHitPointsAction(effect.DamageType, effect.Power * _stats.DamageMultiplier, effect.Factor, impactType));
                else if (effect.Type == ImpactEffectType.SlowDown)
                    collisionBehaviour.AddAction(new SlowDownAction(effect.Power, impactType));
                else if (effect.Type == ImpactEffectType.CaptureDrones)
                    collisionBehaviour.AddAction(new CaptureDroneAction(impactType));
                else if (effect.Type == ImpactEffectType.Repair)
                    collisionBehaviour.AddAction(new RepairAction(effect.Power*_stats.DamageMultiplier, impactType, effect.DamageType, effect.Factor, _owner.Type.Side));
                else if (effect.Type == ImpactEffectType.Teleport)
                    collisionBehaviour.AddAction(new TeleportAction(effect.Power));
                else if (effect.Type == ImpactEffectType.Devour)
                    collisionBehaviour.AddAction(new DevourAction(effect.DamageType, effect.Power * _stats.DamageMultiplier, impactType));
                else if (effect.Type == ImpactEffectType.RestoreLifetime)
                {
                    bullet.Lifetime.Take(bullet.Lifetime.Max * effect.Factor);
                    collisionBehaviour.AddAction(new RestoreLifeAction(bullet.Lifetime, effect.Power, impactType));
                }
            }

            if (impactType == BulletImpactType.HitFirstTarget)
            {
                if (_ammunition.Body.HitPoints > 0)
                    collisionBehaviour.AddAction(new DetonateAtTargetAction());
                else
                    collisionBehaviour.AddAction(new SelfDestructAction());
            }

            return collisionBehaviour;
        }

        private Bullet CreateUnit(IBody body, IView view, GameObjectHolder gameObject)
        {
            UnitClass unitClass;
            if (_ammunition.Body.HitPoints > 0)
                unitClass = UnitClass.Missile;
            else if (_ammunition.ImpactType == BulletImpactType.HitFirstTarget)
                unitClass = UnitClass.EnergyBolt;
            else
                unitClass = UnitClass.AreaOfEffect;

            var unitType = new UnitType(unitClass, UnitSide.Undefined, _ammunition.Body.FriendlyFire ? null : _owner);
            var bullet = new Bullet(body, view, new Lifetime(_stats.GetBulletLifetime()), unitType);

            bullet.Physics = gameObject.GetComponent<PhysicsManager>();
            return bullet;
        }

        private IBody ConfigureBody(IBodyComponent body, IWeaponPlatform parent, float bulletSpeed, float spread,
            float deltaAngle, Vector2 offset)
        {
            IBody parentBody = null;
            var position = Vector2.zero;
            var velocity = Vector2.zero;
            var rotation = deltaAngle;
            var angularVelocity = 0f;
            var weight = _stats.Weight;
            var scale = _stats.BodySize;

            if (_ammunition.Body.Type == BulletType.Continuous && !parent.IsTemporary)
            {
                parentBody = parent.Body;
                position = offset;
            }
            else
            {
                rotation = parent.Body.WorldRotation() + (UnityEngine.Random.value - 0.5f) * spread + deltaAngle;
                position = parent.Body.WorldPosition() + RotationHelpers.Transform(offset, rotation);
            }

            if (_ammunition.Body.Type != BulletType.Continuous)
            {
                velocity = RotationHelpers.Direction(rotation) * bulletSpeed;

                if (_ammunition.Body.Type == BulletType.Homing)
                    velocity *= 0.1f;

                if (_ammunition.Body.Type != BulletType.Static)
                    velocity += parent.Body.WorldVelocity();
            }

            body.Initialize(parentBody, position, rotation, scale, velocity, angularVelocity, weight);
            return body;
        }

		private IView ConfigureView(IView view, Color color)
        {
            view.Life = 0;
            view.Color = color;

			if (BulletShape.IsBeam())
				view.Size = 0;

            view.UpdateView(0);
            return view;
        }

        private ICollider ConfigureCollider(ICollider collider, IUnit unit)
        {
            collider.Unit = unit;
			collider.Source = _owner;

            if (_ammunition.Body.Type == BulletType.Continuous)
                collider.MaxRange = _stats.Range;

			return collider;
        }

        private IDamageHandler CreateDamageHandler(Bullet bullet)
        {
            var hitPoints = _stats.HitPoints;
            if (hitPoints > 0)
            {
                //UnityEngine.Debug.LogError("HP:" + _stats.HitPoints);
                return new MissileDamageHandler(bullet, hitPoints);
            }
            else if (_ammunition.Body.Type == BulletType.Continuous)
                return new BeamDamageHandler(bullet, CanSiphonHitpoints());
            else
                return new DefaultDamageHandler(bullet, CanSiphonHitpoints());
        }

        private static float WeightToAcceleration(float weight)
        {
            return 1f / (0.2f + weight * 2f);
        }

        private IController CreateController(IWeaponPlatform parent, Bullet bullet, float bulletSpeed, float spread,
            float rotationOffset)
        {
            var range = _stats.Range;
            var weight = _stats.Weight;

			IController controller = null;
            if (_ammunition.Body.Type == BulletType.Homing)
                controller = new HomingController(bullet, bulletSpeed, 120f * WeightToAcceleration(weight),
                    0.5f * bulletSpeed / (0.2f + weight * 2), range, _scene);
			else if (_ammunition.Body.Type == BulletType.Magnetic)
                controller = new MagneticController(bullet, bulletSpeed, bulletSpeed * WeightToAcceleration(weight), range, 
					BulletShape.HasDirection(), _scene);            
			else if (_ammunition.Body.Type == BulletType.Continuous && !parent.IsTemporary)
                controller = new BeamController(bullet, spread, rotationOffset);

			if (_ammunition.Body.Type != BulletType.Continuous && BulletShape.IsBeam())
			{
				var length = _ammunition.Body.Length > 0 ? _stats.Length : _stats.BodySize;
				var velocity = _ammunition.Body.Type == BulletType.Static ? Vector2.zero : parent.Body.WorldVelocity();
				controller = new MovingBeamController(bullet, length, bulletSpeed, velocity, controller);
			}

            return controller;
        }

        private BulletFactory CreateFactory(Ammunition ammunition, WeaponStatModifier stats)
        {
            var factory = new BulletFactory(ammunition, stats, _scene, _soundPlayer, _objectPool, _prefabCache,
                _spaceObjectFactory, _effectFactory, _owner);

            factory.Stats.PowerLevel = _stats.PowerLevel;
            factory.Stats.HitPointsMultiplier = _stats.HitPointsMultiplier;
            factory.Stats.RandomFactor = _stats.RandomFactor;
            factory._nestingLevel = _nestingLevel + 1;

            return factory;
        }

		private Cooldown GetSpawnBulletCooldown(BulletTrigger_SpawnBullet trigget)
        {
            // When the Cooldown condition is used, the user likely wants for trigger to activate
            // consistently every `Cooldown` seconds, and the shared SpawnBullet cooldown will likely mess
            // with that. Additionally, there are possible floating point issues when trying to synchronize
            // two independent timers 
            if (trigget.Condition == BulletTriggerCondition.Cooldown) return null;
			if (_cooldownMap == null) _cooldownMap = new();

			if (_cooldownMap.TryGetValue(trigget, out var cooldown))
				return cooldown;

			cooldown = new Cooldown(trigget.Cooldown);
			_cooldownMap.Add(trigget, cooldown);
			return cooldown;
		}

		private BulletShape BulletShape => _ammunition.Body.BulletPrefab == null ? BulletShape.Mine : _ammunition.Body.BulletPrefab.Shape;

		private int _nestingLevel;
		private Dictionary<BulletTrigger_SpawnBullet, Cooldown> _cooldownMap;
		private readonly Lazy<GameObject> _prefab;
        private readonly BulletStats _stats;
        private readonly Ammunition _ammunition;
        private readonly WeaponStatModifier _statModifier;
        private readonly IScene _scene;
        private readonly ISoundPlayer _soundPlayer;
        private readonly IObjectPool _objectPool;
        private readonly SpaceObjectFactory _spaceObjectFactory;
        private readonly EffectFactory _effectFactory;
        private readonly PrefabCache _prefabCache;
        private readonly IShip _owner;

        private const int MaxNestingLevel = 100;

        private class BulletTriggerBuilder : IBulletTriggerFactory<BulletTriggerBuilder.Result>
        {
            public enum Result
            {
                Ok = 0,
                Error = 1,
                OutOfAmmo = 2,
            }

            public static void Build(BulletFactory factory, Bullet bullet, BulletCollisionBehaviour collisionBehaviour)
            {
                new BulletTriggerBuilder(factory, bullet, collisionBehaviour).Build();
            }

            private BulletTriggerBuilder(BulletFactory factory, Bullet bullet, BulletCollisionBehaviour collisionBehaviour)
            {
                _factory = factory;
                _bullet = bullet;
                _collisionBehaviour = collisionBehaviour;
            }

            private void Build()
            {
                var triggers = _factory._ammunition.Triggers;
                var count = triggers.Count;

                var hasOutOfAmmoTriggers = false;
                var outOfAmmoCondition = ConditionType.None;

                for (var i = 0; i < count; ++i)
                {
                    var trigger = triggers[i];
                    if (trigger.Condition == BulletTriggerCondition.Undefined) continue;
                    if (trigger.Condition == BulletTriggerCondition.OutOfAmmo)
                    {
                        hasOutOfAmmoTriggers = true;
                        continue;
                    }

                    _condition = FromTriggerCondition(trigger.Condition);
                    var result = trigger.Create(this);

                    if (result == Result.OutOfAmmo) 
                        outOfAmmoCondition |= _condition;
                }

                if (hasOutOfAmmoTriggers && outOfAmmoCondition != ConditionType.None)
                {
                    _condition = outOfAmmoCondition;
                    for (var i = 0; i < count; ++i)
                    {
                        var trigger = triggers[i];
                        if (trigger.Condition == BulletTriggerCondition.OutOfAmmo) 
                            trigger.Create(this);
                    }
                }
            }

            private static ConditionType FromTriggerCondition(BulletTriggerCondition condition)
            {
                switch (condition)
                {
                    case BulletTriggerCondition.Created:
                        return ConditionType.None;
                    case BulletTriggerCondition.Destroyed:
                        return ConditionType.OnDestroy;
                    case BulletTriggerCondition.Hit:
                        return ConditionType.OnCollide;
                    case BulletTriggerCondition.Disarmed:
                        return ConditionType.OnDisarm;
                    case BulletTriggerCondition.Expired:
                        return ConditionType.OnExpire;
                    case BulletTriggerCondition.Detonated:
                        return ConditionType.OnDetonate;
                    case BulletTriggerCondition.Cooldown:
                        return ConditionType.OnCooldown;
                    default:
                        return ConditionType.None;
                }
            }

            public Result Create(BulletTrigger_None trigger) { return Result.Ok; }

            public Result Create(BulletTrigger_PlaySfx trigger)
            {
                var condition = FromTriggerCondition(trigger.Condition);
                CreateSoundEffect(_bullet, trigger.AudioClip, condition, trigger);
                CreateVisualEffect(_bullet, _collisionBehaviour, condition, trigger);
                return Result.Ok;
            }

            public Result Create(BulletTrigger_SpawnStaticSfx trigger)
            {
                var condition = FromTriggerCondition(trigger.Condition);
                CreateSoundEffect(_bullet, trigger.AudioClip, condition, trigger);
                CreateStaticVisualEffect(_bullet, condition, trigger);
                return Result.Ok;
            }

            public Result Create(BulletTrigger_SpawnBullet trigger)
            {
                if (trigger.Ammunition == null) return Result.Error;

                var maxNestingLevel = trigger.MaxNestingLevel > 0 ? trigger.MaxNestingLevel : MaxNestingLevel;
                if (_factory._nestingLevel >= maxNestingLevel) return Result.OutOfAmmo;

                var factory = CreateFactory(trigger.Ammunition, trigger);
                var magazine = Math.Max(trigger.Quantity, 1);
                AddAction(_bullet, trigger, new SpawnBulletsAction(factory, magazine, factory._stats.BodySize / 2,
                    _bullet, factory._soundPlayer, trigger.AudioClip, _condition).WithCooldown(_factory.GetSpawnBulletCooldown(trigger)));

                return Result.Ok;
            }

            public Result Create(BulletTrigger_Detonate content)
            {
                var condition = FromTriggerCondition(content.Condition);
                AddAction(_bullet, content, new DetonateAction(condition));
                return Result.Ok;
            }

            public Result Create(BulletTrigger_GravityField content)
            {
                var condition = FromTriggerCondition(content.Condition);
                AddAction(_bullet, content, new CreateGravitationAction(_bullet, _factory._spaceObjectFactory, content.Size, content.PowerMultiplier, condition));
                return Result.Ok;
            }

            private void CreateSoundEffect(Bullet bullet, AudioClipId audioClip, ConditionType condition, BulletTrigger trigger)
            {
                if (!audioClip) return;

				if (condition == ConditionType.None && !audioClip.Loop)
                    _factory._soundPlayer.Play(audioClip);
                else
                    AddAction(bullet, trigger, new PlaySoundAction(_factory._soundPlayer, audioClip, condition));
            }

            private void CreateVisualEffect(Bullet bullet, BulletCollisionBehaviour collisionBehaviour,
                ConditionType condition, BulletTrigger_PlaySfx trigger)
            {
                if (trigger.VisualEffect == null) return;

                var color = trigger.ColorMode.Apply(trigger.Color, _factory._stats.Color);
                var size = trigger.Size > 0 ? trigger.Size : 1.0f;

                if (condition == ConditionType.OnCollide)
                    collisionBehaviour.AddAction(new ShowHitEffectAction(_factory._effectFactory, trigger.VisualEffect, color,
                        size * _factory._stats.BodySize, trigger.Lifetime));
                else
                    AddAction(bullet, trigger, new PlayEffectAction(bullet, _factory._effectFactory, trigger.VisualEffect, color, size,
                        trigger.Lifetime, condition));
            }

            private void CreateStaticVisualEffect(Bullet bullet, ConditionType condition, BulletTrigger_SpawnStaticSfx trigger)
            {
                if (trigger.VisualEffect == null) return;

                var color = trigger.ColorMode.Apply(trigger.Color, _factory._stats.Color);

                AddAction(bullet, trigger, new SpawnEffectAction(bullet, _factory._effectFactory, trigger.VisualEffect, color, trigger.Size,
                    trigger.Lifetime, condition));
            }

            private BulletFactory CreateFactory(Ammunition ammunition, BulletTrigger_SpawnBullet trigger)
            {
                var stats = _factory._statModifier;
                stats.Color = trigger.ColorMode.Apply(trigger.Color, _factory._stats.Color);
                if (trigger.PowerMultiplier > 0) stats.DamageMultiplier *= trigger.PowerMultiplier;
                if (trigger.Size > 0)
                {
                    stats.AoeRadiusMultiplier *= trigger.Size;
                    stats.WeightMultiplier *= trigger.Size;
                    stats.RangeMultiplier *= trigger.Size;
                }

                var factory = _factory.CreateFactory(trigger.Ammunition, stats);
                factory.Stats.RandomFactor = trigger.RandomFactor;
                return factory;
            }

            private static void AddAction(Bullet bullet, BulletTrigger trigger, IAction action)
            {
                bullet.AddAction(action.Condition == ConditionType.OnCooldown
                    ? action.WithCooldown(trigger.Cooldown)
                    : action);
            }

            private ConditionType _condition;

            private readonly BulletFactory _factory;
            private readonly Bullet _bullet;
            private readonly BulletCollisionBehaviour _collisionBehaviour;
        }
    }
}