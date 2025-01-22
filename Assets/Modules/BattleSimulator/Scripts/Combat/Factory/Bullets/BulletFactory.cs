﻿using System;
using System.Collections.Generic;
using Combat.Collision.Behaviour;
using Combat.Collision.Behaviour.Action;
using Combat.Component.Body;
using Combat.Component.Bullet;
using Combat.Component.Bullet.Action;
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
using Combat.Services;
using Combat.Unit;
using Constructor;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Extensions;
using GameDatabase.Model;
using UnityEngine;

namespace Combat.Factory
{
    public class BulletFactory : IBulletFactory
    {
        public BulletFactory(Ammunition ammunition, WeaponStatModifier statModifier, IScene scene, IGameServicesProvider services,
            SpaceObjectFactory spaceObjectFactory, EffectFactory effectFactory, IShip owner, int nestingLevel = 0)
        {
            _services = services;
            _ammunition = ammunition;
            _statModifier = statModifier;
            _scene = scene;
            _spaceObjectFactory = spaceObjectFactory;
            _effectFactory = effectFactory;
            _nestingLevel = nestingLevel;
            _owner = owner;

            _prefab = new Lazy<GameObject>(() => _services.PrefabCache.GetBulletPrefab(_ammunition.Body.BulletPrefab));
            _stats = new BulletStats(ammunition, statModifier, nestingLevel > 0);
            _triggerBuilder = new(this);
        }

        public IBulletStats Stats
        {
            get { return _stats; }
        }

        public IBullet Create(IWeaponPlatform parent, float spread, float rotation, Vector2 offset)
        {
            var bulletGameObject = new GameObjectHolder(_prefab, _services);
            bulletGameObject.IsActive = true;

            var bulletSpeed = _stats.GetBulletSpeed();

            var body = ConfigureBody(bulletGameObject.GetComponent<IBodyComponent>(), parent, bulletSpeed, spread,
                rotation, offset);
            var view = ConfigureView(bulletGameObject.GetComponent<IView>(), _stats.Color);

            var options = new Bullet.Options { CanBeDisarmed = _ammunition.Body.CanBeDisarmed };

            var bullet = CreateUnit(body, view, bulletGameObject, options);
            var collisionBehaviour = CreateCollisionBehaviour(bullet);
            bullet.Collider = ConfigureCollider(bulletGameObject.GetComponent<ICollider>(true), bullet, parent);
            bullet.CollisionBehaviour = collisionBehaviour;
            bullet.Controller = CreateController(parent, bullet, bulletSpeed, spread, rotation);
            bullet.DamageHandler = CreateDamageHandler(bullet);
            _triggerBuilder.Build(bullet, collisionBehaviour);
            _scene.AddUnit(bullet);
            bullet.UpdateView(0);
            bullet.AddResource(bulletGameObject);
            if (bullet.Body.Parent != null)
                parent.Bullets?.Add(bullet);

            bullet.UpdatePhysics(0);
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
                if (effect.Type == ImpactEffectType.ProgressiveDamage)
                    collisionBehaviour.AddAction(new ProgressiveDamageAction(effect.DamageType, effect.Power * _stats.DamageMultiplier, effect.Factor, impactType));
                else if (effect.Type == ImpactEffectType.Push)
                    collisionBehaviour.AddAction(new PushAction(effect.Power * _stats.EffectPowerMultiplier, impactType));
                else if (effect.Type == ImpactEffectType.Pull)
                    collisionBehaviour.AddAction(new PushAction(-effect.Power * _stats.EffectPowerMultiplier, impactType));
                else if (effect.Type == ImpactEffectType.PushFromCenter)
                    collisionBehaviour.AddAction(new RadialPushAction(effect.Power * _stats.EffectPowerMultiplier, impactType));
                else if (effect.Type == ImpactEffectType.PullToCenter)
                    collisionBehaviour.AddAction(new RadialPushAction(-effect.Power * _stats.EffectPowerMultiplier, impactType));
                else if (effect.Type == ImpactEffectType.DrainEnergy)
                    collisionBehaviour.AddAction(new DrainEnergyAction(effect.Power * _stats.EffectPowerMultiplier, impactType));
                else if (effect.Type == ImpactEffectType.SiphonHitPoints)
                    collisionBehaviour.AddAction(new SiphonHitPointsAction(effect.DamageType, effect.Power * _stats.DamageMultiplier, effect.Factor, impactType));
                else if (effect.Type == ImpactEffectType.SlowDown)
                    collisionBehaviour.AddAction(new SlowDownAction(effect.Power * _stats.EffectPowerMultiplier, impactType));
                else if (effect.Type == ImpactEffectType.CaptureDrones)
                    collisionBehaviour.AddAction(new AffectDroneAction(impactType, AffectDroneAction.EffectType.Capture));
                else if (effect.Type == ImpactEffectType.DriveDronesCrazy)
                    collisionBehaviour.AddAction(new AffectDroneAction(impactType, AffectDroneAction.EffectType.DriveCrazy));
                else if (effect.Type == ImpactEffectType.Repair)
                    collisionBehaviour.AddAction(new RepairAction(effect.Power * _stats.DamageMultiplier, impactType, effect.DamageType, effect.Factor, _owner.Type.Side));
                else if (effect.Type == ImpactEffectType.RechargeShield)
                    collisionBehaviour.AddAction(new RechargeShieldAction(effect.Power * _stats.DamageMultiplier, impactType, effect.Factor, _owner.Type.Side));
                else if (effect.Type == ImpactEffectType.RechargeEnergy)
                    collisionBehaviour.AddAction(new RechargeEnergyAction(effect.Power * _stats.EffectPowerMultiplier * _statModifier.EnergyCostMultiplier.Value, impactType, effect.Factor, _owner.Type.Side));
                else if (effect.Type == ImpactEffectType.Teleport)
                    collisionBehaviour.AddAction(new TeleportAction(_effectFactory, _stats.Color, effect.Power));
                else if (effect.Type == ImpactEffectType.DrainShield)
                    collisionBehaviour.AddAction(new DamageShieldAction(impactType, effect.Power * _stats.DamageMultiplier));
                else if (effect.Type == ImpactEffectType.Devour)
                    collisionBehaviour.AddAction(new DevourAction(effect.DamageType, effect.Power * _stats.DamageMultiplier, impactType));
                else if (effect.Type == ImpactEffectType.IgnoreShield)
                    collisionBehaviour.AddAction(new IgnoreShieldAction());
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

        private Bullet CreateUnit(IBody body, IView view, GameObjectHolder gameObject, in Bullet.Options options)
        {
            UnitClass unitClass;
            if (_ammunition.Body.HitPoints > 0)
                unitClass = UnitClass.Missile;
            else if (_ammunition.ImpactType == BulletImpactType.HitFirstTarget)
                unitClass = UnitClass.EnergyBolt;
            else
                unitClass = UnitClass.AreaOfEffect;

            var unitType = new UnitType(unitClass, UnitSide.Neutral, _owner, _ammunition.Body.FriendlyFire);
            var bullet = new Bullet(body, view, new Lifetime(_stats.GetBulletLifetime()), unitType, options);

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

            if (_ammunition.Body.AttachedToParent && parent.Owner.IsActive())
            {
                parentBody = parent.Body;
                rotation = deltaAngle;
                position = RotationHelpers.Transform(offset, rotation);
            }
            else
            {
                rotation = parent.Body.WorldRotation() + (UnityEngine.Random.value - 0.5f) * spread + deltaAngle;
                position = parent.Body.WorldPosition() + RotationHelpers.Transform(offset, rotation);
            }

            if (!_ammunition.Controller.Continuous)
            {
                velocity = RotationHelpers.Direction(rotation) * bulletSpeed;
                velocity *= _ammunition.Controller.StartingVelocityMultiplier;
                velocity += parent.Body.WorldVelocity() * _ammunition.Body.ParentVelocityEffect;
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

        private ICollider ConfigureCollider(ICollider collider, IUnit unit, IWeaponPlatform weaponPlatform)
        {
            collider.Unit = unit;
			collider.Source = weaponPlatform.Owner;
            collider.OneHitOnly = _ammunition.ImpactType != BulletImpactType.DamageOverTime;

            if (_ammunition.Controller.Continuous)
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
            else if (_ammunition.Controller.Continuous)
                return new BeamDamageHandler(bullet, CanSiphonHitpoints());
            else
                return new BulletDamageHandler(bullet, CanSiphonHitpoints());
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
            switch (_ammunition.Controller)
            {
                case BulletController_Projectile:
                    break;
                case BulletController_Harpoon:
                    controller = new HarpoonController(bullet, parent, range);
                    break;
                case BulletController_Homing homing:
                    if (homing.IgnoreRotation)
                    {
                        controller = new MagneticController(bullet, bulletSpeed, bulletSpeed * WeightToAcceleration(weight), range,
                            BulletShape.HasDirection(), homing.SmartAim, _scene);
                    }
                    else
                    {
                        controller = new HomingController(bullet, bulletSpeed, 120f * WeightToAcceleration(weight),
                            0.5f * bulletSpeed / (0.2f + weight * 2), range, homing.SmartAim, _scene);
                    }
                    break;
                case BulletController_Beam:
                    controller = new BeamController(bullet, spread, rotationOffset + bullet.Body.Rotation);
                    break;
                case BulletController_AuraEmitter:
                    controller = new AuraController(bullet, _stats.BodySize, _ammunition.Body.Lifetime);
                    break;
                case BulletController_Parametric controllerParametric:
                    controller = new ParametricController(bullet, controllerParametric);
                    break;
                case BulletController_StickyMine stickyMine:
                    controller = new StickyController(bullet, stickyMine.Lifetime);
                    break;
                default:
                    Debug.LogError($"Unknown controller: {_ammunition.Controller.GetType().Name}");
                    break;
            }

            if (BulletShape.IsBeam() && !_ammunition.Controller.Continuous && bulletSpeed > 0)
			{
				var length = _ammunition.Body.Length > 0 ? _stats.Length : _stats.BodySize;
                var velocity = parent.Body.WorldVelocity() * _ammunition.Body.ParentVelocityEffect;
                controller = new MovingBeamController(bullet, length, bulletSpeed, range, velocity, controller);
			}

            return controller;
        }

        private BulletFactory CreateFactory(Ammunition ammunition, WeaponStatModifier stats)
        {
            var factory = new BulletFactory(ammunition, stats, _scene, _services,
                _spaceObjectFactory, _effectFactory, _owner, _nestingLevel + 1);

            factory.Stats.PowerLevel = _stats.PowerLevel;
            factory.Stats.HitPointsMultiplier = _stats.HitPointsMultiplier;
            factory.Stats.RandomFactor = _stats.RandomFactor;

            return factory;
        }

		private BulletShape BulletShape => _ammunition.Body.BulletPrefab == null ? BulletShape.Mine : _ammunition.Body.BulletPrefab.Shape;

		private int _nestingLevel;
        private readonly IShip _owner;
        private readonly Lazy<GameObject> _prefab;
        private readonly BulletStats _stats;
        private readonly Ammunition _ammunition;
        private readonly WeaponStatModifier _statModifier;
        private readonly IScene _scene;
        private readonly IGameServicesProvider _services;
        private readonly SpaceObjectFactory _spaceObjectFactory;
        private readonly EffectFactory _effectFactory;
        private readonly PrefabCache _prefabCache;
        private readonly BulletTriggerBuilder _triggerBuilder;

        private const int MaxNestingLevel = 100;

        private class BulletTriggerBuilder : IBulletTriggerFactory<BulletTriggerBuilder.Result>
        {
            public enum Result
            {
                Ok = 0,
                Error = 1,
                OutOfAmmo = 2,
            }

            private ConditionType _condition;
            private Dictionary<BulletTrigger_SpawnBullet, BulletFactory> _factoryCache;
            private readonly BulletFactory _factory;
            private Bullet _bullet;
            private BulletCollisionBehaviour _collisionBehaviour;

            public BulletTriggerBuilder(BulletFactory factory)
            {
                _factory = factory;
            }

            public void Build(Bullet bullet, BulletCollisionBehaviour collisionBehaviour)
            {
                _bullet = bullet;
                _collisionBehaviour = collisionBehaviour;

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
                CreateSoundEffect(_bullet, _collisionBehaviour, trigger.AudioClip, condition, trigger, trigger.OncePerCollision);
                CreateVisualEffect(_bullet, _collisionBehaviour, condition, trigger);
                return Result.Ok;
            }

            public Result Create(BulletTrigger_SpawnStaticSfx trigger)
            {
                var condition = FromTriggerCondition(trigger.Condition);
                CreateSoundEffect(_bullet, _collisionBehaviour, trigger.AudioClip, condition, trigger, trigger.OncePerCollision);
                CreateStaticVisualEffect(_bullet, _collisionBehaviour, condition, trigger);
                return Result.Ok;
            }

            public Result Create(BulletTrigger_SpawnBullet trigger)
            {
                if (trigger.Ammunition == null) return Result.Error;

                var maxNestingLevel = trigger.MaxNestingLevel > 0 ? trigger.MaxNestingLevel : MaxNestingLevel;
                if (_factory._nestingLevel >= maxNestingLevel) return Result.OutOfAmmo;

                var factory = CreateFactory(trigger);
                var magazine = Math.Max(trigger.Quantity, 1);
                // TODO: previously, there was `factory._stats.BodySize / 2` bonus for "cluster"-type ammo (more than 1 magazine)
                // This is currently not replicable with variables exposed to expressions
                // May be resolved once member access is added, and expressions can do something like
                // IF(quantity == 1, 0, Ammunition.Body.Size * Size / 2)
                AddAction(_bullet, trigger, new SpawnBulletsAction(factory, magazine, trigger, _bullet,
                        factory._services.SoundPlayer, trigger.AudioClip, _condition).WithCooldown(trigger.Cooldown));

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
                var force = content.PowerMultiplier * _factory._statModifier.AoeRadiusMultiplier.Value;
                var size = content.Size * _factory._statModifier.AoeRadiusMultiplier.Value;

                var condition = FromTriggerCondition(content.Condition);
                AddAction(_bullet, content, new CreateGravitationAction(_bullet, _factory._spaceObjectFactory, size, force,
                    _factory._ammunition.Body.FriendlyFire, condition));

                return Result.Ok;
            }

            private void CreateSoundEffect(Bullet bullet, BulletCollisionBehaviour collisionBehaviour, 
                AudioClipId audioClip, ConditionType condition, BulletTrigger trigger, bool oncePerCollision)
            {
                if (!audioClip) return;

                if (condition == ConditionType.OnCollide)
                    collisionBehaviour.AddAction(new PlayHitSoundAction(_factory._services.SoundPlayer, audioClip, trigger.Cooldown, oncePerCollision));
                else if (condition == ConditionType.None && !audioClip.Loop)
                    _factory._services.SoundPlayer.Play(audioClip);
                else
                    AddAction(bullet, trigger, new PlaySoundAction(_factory._services.SoundPlayer, audioClip, condition).WithCooldown(trigger.Cooldown));
            }

            private void CreateVisualEffect(Bullet bullet, BulletCollisionBehaviour collisionBehaviour,
                ConditionType condition, BulletTrigger_PlaySfx trigger)
            {
                if (trigger.VisualEffect == null) return;

                var color = trigger.ColorMode.Apply(trigger.Color, _factory._stats.Color);
                var size = trigger.Size > 0 ? trigger.Size : 1.0f;

                if (condition == ConditionType.OnCollide && !trigger.UseBulletPosition)
                {
                    ICollisionAction action;
                    if (trigger.OncePerCollision)
                        action = new ShowMultipleHitEffectsAction(_factory._effectFactory, trigger.VisualEffect, color,
                            size, trigger.Lifetime);
                    else
                        action = new ShowHitEffectAction(_factory._effectFactory, trigger.VisualEffect, color,
                            size, trigger.Lifetime);

                    collisionBehaviour.AddAction(action);
                }
                else if (trigger.SyncLifetimeWithBullet)
                    AddAction(bullet, trigger, new AttachEffectAction(bullet, _factory._effectFactory, trigger.VisualEffect, color, size,
                        condition).WithCooldown(trigger.Cooldown));
                else
                    AddAction(bullet, trigger, new PlayEffectAction(bullet, _factory._effectFactory, trigger.VisualEffect, color, size,
                            trigger.Lifetime, condition).WithCooldown(trigger.Cooldown));
            }

            private void CreateStaticVisualEffect(Bullet bullet, BulletCollisionBehaviour collisionBehaviour,
                ConditionType condition, BulletTrigger_SpawnStaticSfx trigger)
            {
                if (trigger.VisualEffect == null) return;

                var color = trigger.ColorMode.Apply(trigger.Color, _factory._stats.Color);
                var size = trigger.Size > 0 ? trigger.Size : 1.0f;

                if (condition == ConditionType.OnCollide)
                    collisionBehaviour.AddAction(new SpawnHitEffectAction(_factory._effectFactory, trigger.VisualEffect, color,
                        size * _factory._stats.BodySize, trigger.Lifetime, trigger.Cooldown, trigger.OncePerCollision));
                else
                    AddAction(bullet, trigger, new SpawnEffectAction(bullet, _factory._effectFactory, trigger.VisualEffect, color, 
                        size * _factory._stats.BodySize, trigger.Lifetime, condition).WithCooldown(trigger.Cooldown));
            }

            private BulletFactory CreateFactory(BulletTrigger_SpawnBullet trigger)
            {
                BulletFactory factory;

                if (_factoryCache == null)
                    _factoryCache = new();
                else if (_factoryCache.TryGetValue(trigger, out factory))
                    return factory;

                var stats = _factory._statModifier;
                stats.Color = trigger.ColorMode.Apply(trigger.Color, _factory._stats.Color);
                if (trigger.PowerMultiplier > 0)
                {
                    stats.DamageMultiplier *= trigger.PowerMultiplier;
                    stats.EffectPowerMultiplier *= trigger.PowerMultiplier;
                }

                if (trigger.Size > 0)
                {
                    stats.AoeRadiusMultiplier *= trigger.Size;
                    stats.WeightMultiplier *= trigger.Size;
                    stats.RangeMultiplier *= trigger.Size;
                }

                factory = _factory.CreateFactory(trigger.Ammunition, stats);
                factory.Stats.RandomFactor = trigger.RandomFactor;
                _factoryCache.Add(trigger, factory);
                return factory;
            }

            private static void AddAction(Bullet bullet, BulletTrigger trigger, IAction action)
            {
                bullet.AddAction(action.Condition == ConditionType.OnCooldown
                    ? action.WithCooldown(trigger.Cooldown)
                    : action);
            }
        }
    }
}