using System;
using System.Collections.Generic;
using DatabaseMigration.v1.Enums;
using DatabaseMigration.v1.Serializable;

namespace DatabaseMigration.v1
{
    // Converts obsolete ammunition (AmmunitionObsolete) to the new format (battle simulator phase 4, step 4c, decision D2).
    // The rules follow TODO/battle_simulator_ammo_mapping.md; comments name its rows (G*, E*, V*, S1).
    // The class reads no files, so a later migration step can call it.
    //
    // One instance converts a whole database: call Convert in obsolete-ID order. A converted record keeps its obsolete ID
    // (D3). Child area bullets and the BulletPrefab and VisualEffect records take the next free ID of their table when they
    // are first needed, so the same input in the same order gives the same output.
    public sealed class AmmunitionConverter
    {
        public sealed class NewRecord
        {
            public NewRecord(SerializableItem item, string name)
            {
                Item = item;
                Name = name;
            }

            public SerializableItem Item { get; }
            // A file name stem. Ammunition: the source name plus a suffix. BulletPrefab and VisualEffect: a fixed name.
            public string Name { get; }
        }

        public AmmunitionConverter(int nextAmmunitionId, int nextBulletPrefabId, int nextVisualEffectId)
        {
            _nextAmmunitionId = nextAmmunitionId;
            _nextPrefabId = nextBulletPrefabId;
            _nextEffectId = nextVisualEffectId;
        }

        // Returns the converted record first, then its child records and the BulletPrefab and VisualEffect records that
        // this call created. 'coupled' is the record of CoupledAmmunitionId; only FragBomb reads it.
        public List<NewRecord> Convert(AmmunitionObsoleteSerializable item, AmmunitionObsoleteSerializable coupled, string name)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (item.AmmunitionClass == AmmunitionClassObsolete.FragBomb && coupled == null)
                throw new InvalidOperationException($"ammunition {item.Id}: FragBomb needs its coupled record ({item.CoupledAmmunitionId})");

            _output = new List<NewRecord>();
            ConvertRecord(item, item.Id, name, coupled);
            var output = _output;
            _output = null;
            return output;
        }

        private AmmunitionSerializable ConvertRecord(AmmunitionObsoleteSerializable item, int id, string name, AmmunitionObsoleteSerializable coupled)
        {
            var type = item.AmmunitionClass;
            if (!IsSupported(type))
                throw new InvalidOperationException($"ammunition {item.Id}: class {type} has no conversion rule");

            var prefab = GetPrefab(item);
            var ammunition = new AmmunitionSerializable { Id = id };
            _output.Add(new NewRecord(ammunition, name));

            ammunition.Body = CreateBody(item, prefab);
            ammunition.Controller = CreateController(type);
            ammunition.ImpactType = IsDot(type) ? BulletImpactType.DamageOverTime : BulletImpactType.HitFirstTarget;

            var effects = new List<ImpactEffectSerializable>();
            AddEffects(item, effects);
            ammunition.Effects = effects.ToArray();

            var triggers = new List<BulletTriggerSerializable>();
            AddTriggers(item, prefab, name, coupled, triggers);
            ammunition.Triggers = triggers.ToArray();

            return ammunition;
        }

        private BulletBodySerializable CreateBody(AmmunitionObsoleteSerializable item, LegacyPrefab prefab)
        {
            var type = item.AmmunitionClass;
            var attached = IsBoundToCannon(type);

            var body = new BulletBodySerializable
            {
                // AuraController grows the body to 2 × its size argument: legacy passes AreaOfEffect, new passes BodySize.
                Size = type == AmmunitionClassObsolete.Aura ? item.AreaOfEffect : Mul(item.Size, prefab.K),
                Velocity = item.Velocity,
                ParentVelocityEffect = item.IgnoresShipVelocity ? 0f : 1f,
                AttachedToParent = attached,
                Range = item.Range,
                Lifetime = GetLifetime(item),
                Weight = item.Impulse,
                // Legacy applies no recoil to attached bullets and to bullets that ignore the ship velocity.
                Recoil = attached || item.IgnoresShipVelocity ? 0f : item.Recoil,
                HitPoints = item.HitPoints,
                Color = item.Color,
                BulletPrefab = GetPrefabId(prefab),
                EnergyCost = item.EnergyCost,
                CanBeDisarmed = CanBeDisarmed(type) && item.Impulse < 0.5f,
                FriendlyFire = type == AmmunitionClassObsolete.RepairRay,
                AiBulletBehavior = GetBehaviorType(type),
            };

            return body;
        }

        // Legacy flight time of a moving bullet is range / speed; the new factory does the same when Lifetime is 0 (G6).
        // Beams keep the value: the beam lifetime also sets the flash time and the start cost.
        private static float GetLifetime(AmmunitionObsoleteSerializable item)
        {
            if (IsBeam(item.AmmunitionClass)) return item.LifeTime;
            if (item.Velocity > 0 && item.Range > 0) return 0f;
            return item.LifeTime;
        }

        private static BulletControllerSerializable CreateController(AmmunitionClassObsolete type)
        {
            var controller = new BulletControllerSerializable();
            if (IsBeam(type))
            {
                controller.Type = BulletControllerType.Beam;
                if (StickToTarget(type)) controller.TrackingAngle = 60f; // G3
            }
            else if (IsHoming(type))
            {
                controller.Type = BulletControllerType.Homing;
            }
            else if (type == AmmunitionClassObsolete.UnguidedRocket)
            {
                controller.Type = BulletControllerType.Rocket; // G8
            }
            else if (type == AmmunitionClassObsolete.Aura)
            {
                controller.Type = BulletControllerType.AuraEmitter;
            }

            if (HasEngine(type))
                controller.StartingVelocityModifier = 0.1f;

            return controller;
        }

        // Follows BulletFactoryObsolete.CreateCollisionBehaviour. Effects with zero power are left out: legacy actions with
        // zero power do nothing, and a zero damage effect would give the weapon the AI capability DamageEnemy.
        private static void AddEffects(AmmunitionObsoleteSerializable item, List<ImpactEffectSerializable> effects)
        {
            var type = item.AmmunitionClass;

            if (type == AmmunitionClassObsolete.RepairRay)
                Add(effects, ImpactEffectType.Repair, item.DamageType, item.Damage, 1f);
            else if (type == AmmunitionClassObsolete.VampiricRay)
                Add(effects, ImpactEffectType.SiphonHitPoints, item.DamageType, item.Damage, 1f);
            else if (type == AmmunitionClassObsolete.SmallVampiricRay)
                Add(effects, ImpactEffectType.SiphonHitPoints, item.DamageType, item.Damage, 0.1f);
            else if (IsDot(type) || HasDirectDamage(type))
                Add(effects, ImpactEffectType.Damage, item.DamageType, item.Damage);

            if (type == AmmunitionClassObsolete.EnergySiphon)
                Add(effects, ImpactEffectType.DrainEnergy, DamageType.Impact, Mul(4m, item.EnergyCost));

            // StrikeAction: push = bullet velocity × weight, plus impact damage of the same size (G2, difference 1).
            if (HasDirectImpulse(type) && item.Impulse > 0)
            {
                var strike = Mul(item.Velocity, item.Impulse);
                var damage = effects.Find(effect => effect.Type == ImpactEffectType.Damage && effect.DamageType == DamageType.Impact);
                if (damage != null)
                    damage.Power = Add(damage.Power, strike);
                else
                    Add(effects, ImpactEffectType.Damage, DamageType.Impact, strike);

                Add(effects, ImpactEffectType.Push, DamageType.Impact, item.Impulse);
            }

            if (type == AmmunitionClassObsolete.DroneControl)
                effects.Add(new ImpactEffectSerializable { Type = ImpactEffectType.CaptureDrones });

            if (type == AmmunitionClassObsolete.Immobilizer || type == AmmunitionClassObsolete.HomingImmobilizer)
                Add(effects, ImpactEffectType.SlowDown, DamageType.Impact, item.LifeTime);

            if (type == AmmunitionClassObsolete.Singularity)
                Add(effects, ImpactEffectType.Roll, DamageType.Impact, item.LifeTime); // G5
        }

        private void AddTriggers(AmmunitionObsoleteSerializable item, LegacyPrefab prefab, string name,
            AmmunitionObsoleteSerializable coupled, List<BulletTriggerSerializable> triggers)
        {
            var type = item.AmmunitionClass;

            if (!string.IsNullOrEmpty(item.FireSound))
                triggers.Add(new BulletTriggerSerializable { Condition = BulletTriggerCondition.Created, EffectType = BulletEffectType.PlaySfx, AudioClip = item.FireSound });

            if (prefab.BodyEffect != null)
            {
                triggers.Add(new BulletTriggerSerializable
                {
                    Condition = BulletTriggerCondition.Created,
                    EffectType = BulletEffectType.PlaySfx,
                    VisualEffect = GetEffectId(prefab.BodyEffect),
                    SyncLifetimeWithBullet = true,
                });
            }

            // Both factories multiply the hit effect size by the bullet scale: legacy shows 1.5 × Size², so the trigger
            // size is 1.5 × Size / k for a bullet scale of Size × k.
            var hitEffect = GetHitEffect(item);
            if (hitEffect != null)
            {
                triggers.Add(new BulletTriggerSerializable
                {
                    Condition = BulletTriggerCondition.Hit,
                    EffectType = BulletEffectType.PlaySfx,
                    VisualEffect = GetEffectId(hitEffect),
                    Size = Div(Mul(1.5m, item.Size), prefab.K),
                    Lifetime = 0.2f,
                });
            }

            var conditions = DetonateIfExpired(type)
                ? new[] { BulletTriggerCondition.Detonated, BulletTriggerCondition.Expired }
                : new[] { BulletTriggerCondition.Detonated };

            if (!string.IsNullOrEmpty(item.HitSound))
                foreach (var condition in conditions)
                    triggers.Add(new BulletTriggerSerializable { Condition = condition, EffectType = BulletEffectType.PlaySfx, AudioClip = item.HitSound });

            if (type == AmmunitionClassObsolete.Explosion)
            {
                var child = CreateArea(item, AreaKind.StrongExplosion, name);
                triggers.Add(Spawn(BulletTriggerCondition.Created, child));
                return;
            }

            if (type == AmmunitionClassObsolete.FragBomb)
            {
                var child = CreateFragment(item, coupled, name);
                foreach (var condition in conditions)
                {
                    var trigger = Spawn(condition, child);
                    trigger.Quantity = 20;
                    trigger.RandomFactor = 0.75f;
                    // Legacy fragments keep the colour of their own record.
                    trigger.Color = string.IsNullOrEmpty(coupled.Color) ? "#FFFFFF" : coupled.Color;
                    trigger.ColorMode = ColorMode.UseMyOwn;
                    triggers.Add(trigger);
                }
                return;
            }

            var kind = GetAreaKind(item);
            if (kind == AreaKind.None) return;

            var area = CreateArea(item, kind, name);
            foreach (var condition in conditions)
                triggers.Add(Spawn(condition, area));
        }

        private static BulletTriggerSerializable Spawn(BulletTriggerCondition condition, AmmunitionSerializable child)
        {
            return new BulletTriggerSerializable { Condition = condition, EffectType = BulletEffectType.SpawnBullet, Ammunition = child.Id };
        }

        // Legacy overrides the damage, damage type and range of the coupled record (BulletFactoryObsolete.CreateTriggers),
        // so every parent gets its own copy.
        private AmmunitionSerializable CreateFragment(AmmunitionObsoleteSerializable parent, AmmunitionObsoleteSerializable coupled, string name)
        {
            if (coupled.AmmunitionClass == AmmunitionClassObsolete.FragBomb)
                throw new InvalidOperationException($"ammunition {parent.Id}: the coupled record {coupled.Id} is a FragBomb too");

            var fragment = Copy(coupled);
            fragment.Damage = parent.Damage;
            fragment.DamageType = parent.DamageType;
            fragment.Range = parent.AreaOfEffect;
            fragment.CoupledAmmunitionId = 0;
            return ConvertRecord(fragment, _nextAmmunitionId++, name + "_Fragment", null);
        }

        // G7: a legacy area object becomes a child area bullet without a prefab. Values follow SpaceObjectFactory; the
        // falloff effects follow ApplyExplosionDamageAction (E6). Push = radius × the parent bullet's weight.
        private AmmunitionSerializable CreateArea(AmmunitionObsoleteSerializable parent, AreaKind kind, string name)
        {
            var radius = parent.AreaOfEffect;
            var area = new AmmunitionSerializable
            {
                Id = _nextAmmunitionId++,
                Body = new BulletBodySerializable
                {
                    Size = Mul(2m, radius),
                    ParentVelocityEffect = 0f,
                    AiBulletBehavior = AiBulletBehavior.AreaOfEffect,
                },
                Controller = new BulletControllerSerializable(),
                ImpactType = BulletImpactType.HitAllTargets,
            };

            _output.Add(new NewRecord(area, name + "_" + kind));

            var effects = new List<ImpactEffectSerializable>();
            var triggers = new List<BulletTriggerSerializable>();

            switch (kind)
            {
                case AreaKind.Explosion:
                case AreaKind.SmokeExplosion:
                case AreaKind.StrongExplosion:
                case AreaKind.Flash:
                    area.Body.Lifetime = 0.3f;
                    Add(effects, ImpactEffectType.DamageWithFalloff, parent.DamageType, parent.Damage);
                    Add(effects, ImpactEffectType.PushFromCenterWithFalloff, DamageType.Impact, Mul(radius, parent.Impulse));
                    triggers.Add(AreaEffect(GetAreaEffect(kind), kind == AreaKind.Flash ? 0.3f : 0.5f));
                    break;

                case AreaKind.Emp:
                    // Legacy EMP pushes nothing; its drain and shield damage have no falloff.
                    area.Body.Lifetime = 0.5f;
                    Add(effects, ImpactEffectType.DamageWithFalloff, parent.DamageType, parent.Damage);
                    Add(effects, ImpactEffectType.DrainEnergy, DamageType.Impact, Mul(10m, parent.EnergyCost));
                    Add(effects, ImpactEffectType.DrainShield, DamageType.Impact, Mul(25m, parent.Damage));
                    triggers.Add(AreaEffect(EmpFlash, 0.5f));
                    break;

                case AreaKind.Cloud:
                    // The cloud drifts with 1 % of the rocket velocity (CreateCloudAction).
                    area.Body.Lifetime = parent.LifeTime;
                    area.Body.ParentVelocityEffect = 0.01f;
                    area.ImpactType = BulletImpactType.DamageOverTime;
                    Add(effects, ImpactEffectType.Damage, parent.DamageType, parent.Damage);
                    var cloud = AreaEffect(AcidCloud, 0f);
                    cloud.SyncLifetimeWithBullet = true;
                    triggers.Add(cloud);
                    // Legacy halves the alpha of both cloud layers.
                    triggers.Add(new BulletTriggerSerializable
                    {
                        Condition = BulletTriggerCondition.Created,
                        EffectType = BulletEffectType.PlaySfx,
                        VisualEffect = GetEffectId(Orb),
                        Color = HalfAlpha,
                        ColorMode = ColorMode.Multiply,
                        Size = 1f,
                        Lifetime = 0.3f,
                    });
                    break;

                case AreaKind.Web:
                    // Legacy slows with a Unity drag field of 0.5 × radius. SlowDown divides by √(target weight), so the
                    // owner doubled it (2026-10-31): equal to legacy at weight 4, the median build weighs 2.9.
                    area.Body.Lifetime = parent.LifeTime;
                    area.ImpactType = BulletImpactType.DamageOverTime;
                    Add(effects, ImpactEffectType.Damage, parent.DamageType, parent.Damage);
                    Add(effects, ImpactEffectType.SlowDown, DamageType.Impact, radius);
                    var web = AreaEffect(Web, 0f);
                    web.SyncLifetimeWithBullet = true;
                    triggers.Add(web);
                    break;

                default:
                    throw new InvalidOperationException($"unknown area kind {kind}");
            }

            area.Effects = effects.ToArray();
            area.Triggers = triggers.ToArray();
            return area;
        }

        // A composite on a bullet of scale 2R with trigger size 0.5 has size R, so element sizes are the legacy multipliers of R.
        private BulletTriggerSerializable AreaEffect(EffectSpec effect, float lifetime)
        {
            return new BulletTriggerSerializable
            {
                Condition = BulletTriggerCondition.Created,
                EffectType = BulletEffectType.PlaySfx,
                VisualEffect = GetEffectId(effect),
                Size = 0.5f,
                Lifetime = lifetime,
            };
        }

        // Legacy CreateTriggers.
        private static AreaKind GetAreaKind(AmmunitionObsoleteSerializable item)
        {
            switch (item.AmmunitionClass)
            {
                case AmmunitionClassObsolete.Bomb:
                case AmmunitionClassObsolete.Rocket:
                case AmmunitionClassObsolete.UnguidedRocket:
                    if (item.DamageType == DamageType.Impact) return AreaKind.SmokeExplosion;
                    return item.AreaOfEffect > 2f ? AreaKind.Explosion : AreaKind.Flash;
                case AmmunitionClassObsolete.Fireworks:
                    return AreaKind.Flash;
                case AmmunitionClassObsolete.Emp:
                    return AreaKind.Emp;
                case AmmunitionClassObsolete.AcidRocket:
                    return AreaKind.Cloud;
                case AmmunitionClassObsolete.PlasmaWeb:
                    return AreaKind.Web;
                default:
                    return AreaKind.None;
            }
        }

        private static EffectSpec GetAreaEffect(AreaKind kind)
        {
            switch (kind)
            {
                case AreaKind.Explosion: return Explosion1;
                case AreaKind.SmokeExplosion: return SmokeExplosion;
                case AreaKind.StrongExplosion: return StrongExplosion;
                case AreaKind.Flash: return FlashExplosion;
                default: throw new InvalidOperationException($"no effect for {kind}");
            }
        }

        private static EffectSpec GetHitEffect(AmmunitionObsoleteSerializable item)
        {
            switch (item.HitEffectPrefab ?? string.Empty)
            {
                case "": return null;
                case "FlashAdditive": return Flash;
                case "OrbAdditive": return Orb;
                // No such prefab in Combat/Effects: legacy shows nothing.
                case "Cloud":
                case "Energy2":
                    return null;
                default:
                    throw new InvalidOperationException($"ammunition {item.Id}: unknown hit effect '{item.HitEffectPrefab}'");
            }
        }

        private static LegacyPrefab GetPrefab(AmmunitionObsoleteSerializable item)
        {
            var name = string.IsNullOrEmpty(item.BulletPrefab) ? (IsBeam(item.AmmunitionClass) ? "Laser" : "Plasma2") : item.BulletPrefab;
            if (!Prefabs.TryGetValue(name, out var prefab))
                throw new InvalidOperationException($"ammunition {item.Id}: unknown bullet prefab '{item.BulletPrefab}'");
            return prefab;
        }

        private int GetPrefabId(LegacyPrefab prefab)
        {
            if (prefab.ExistingId > 0) return prefab.ExistingId;
            if (prefab.New == null) return 0;

            if (_prefabIds.TryGetValue(prefab.New.Name, out var id)) return id;

            id = _nextPrefabId++;
            _prefabIds.Add(prefab.New.Name, id);
            var spec = prefab.New;
            _output.Add(new NewRecord(new BulletPrefabSerializable
            {
                Id = id,
                Shape = spec.Shape,
                Image = spec.Image,
                Size = spec.Size,
                Margins = spec.Margins,
                SecondColor = spec.Shape == BulletShape.Rocket ? RocketEngineColor : null,
                SecondColorMode = spec.Shape == BulletShape.Rocket ? ColorMode.Blend : ColorMode.TakeFromOwner,
            }, spec.Name));

            return id;
        }

        private int GetEffectId(EffectSpec effect)
        {
            if (effect.ExistingId > 0) return effect.ExistingId;
            if (_effectIds.TryGetValue(effect.Name, out var id)) return id;

            id = _nextEffectId++;
            _effectIds.Add(effect.Name, id);
            _output.Add(new NewRecord(new VisualEffectSerializable { Id = id, Elements = effect.Elements() }, effect.Name));
            return id;
        }

        private static void Add(List<ImpactEffectSerializable> effects, ImpactEffectType type, DamageType damageType, float power, float factor = 0f)
        {
            if (power <= 0) return;
            effects.Add(new ImpactEffectSerializable { Type = type, DamageType = damageType, Power = power, Factor = factor });
        }

        private static AmmunitionObsoleteSerializable Copy(AmmunitionObsoleteSerializable item)
        {
            return new AmmunitionObsoleteSerializable
            {
                Id = item.Id,
                AmmunitionClass = item.AmmunitionClass,
                DamageType = item.DamageType,
                Impulse = item.Impulse,
                Recoil = item.Recoil,
                Size = item.Size,
                InitialPosition = item.InitialPosition,
                AreaOfEffect = item.AreaOfEffect,
                Damage = item.Damage,
                Range = item.Range,
                Velocity = item.Velocity,
                LifeTime = item.LifeTime,
                HitPoints = item.HitPoints,
                IgnoresShipVelocity = item.IgnoresShipVelocity,
                EnergyCost = item.EnergyCost,
                CoupledAmmunitionId = item.CoupledAmmunitionId,
                Color = item.Color,
                FireSound = item.FireSound,
                HitSound = item.HitSound,
                HitEffectPrefab = item.HitEffectPrefab,
                BulletPrefab = item.BulletPrefab,
            };
        }

        // Decimal arithmetic on the values as written, so 1.5 × 0.6 gives the float nearest to 0.9, not 0.90000004.
        private static float Mul(float a, float b) => (float)((decimal)a * (decimal)b);
        private static float Mul(decimal a, float b) => (float)(a * (decimal)b);
        private static float Mul(float a, decimal b) => (float)((decimal)a * b);
        private static float Div(float a, decimal b) => (float)Math.Round((decimal)a / b, 6);
        private static float Add(float a, float b) => (float)((decimal)a + (decimal)b);

        #region Legacy class rules (AmmunitionClassExtensions)

        private static bool IsSupported(AmmunitionClassObsolete type)
        {
            switch (type)
            {
                // No built-in record uses these; mods keep the old simulator (mapping table, Scope).
                case AmmunitionClassObsolete.Carrier:
                case AmmunitionClassObsolete.HomingCarrier:
                case AmmunitionClassObsolete.HomingTorpedo:
                case AmmunitionClassObsolete.EmpMissile:
                case AmmunitionClassObsolete.IonBeam:
                case AmmunitionClassObsolete.BlackHole:
                case AmmunitionClassObsolete.ClusterMissile:
                    return false;
                default:
                    return true;
            }
        }

        private static bool IsBeam(AmmunitionClassObsolete type)
        {
            switch (type)
            {
                case AmmunitionClassObsolete.EnergyBeam:
                case AmmunitionClassObsolete.EnergySiphon:
                case AmmunitionClassObsolete.VampiricRay:
                case AmmunitionClassObsolete.LaserBeam:
                case AmmunitionClassObsolete.RepairRay:
                case AmmunitionClassObsolete.SmallVampiricRay:
                case AmmunitionClassObsolete.TractorBeam:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsDot(AmmunitionClassObsolete type)
        {
            return IsBeam(type) || type == AmmunitionClassObsolete.Acid || type == AmmunitionClassObsolete.Aura ||
                type == AmmunitionClassObsolete.DamageOverTime;
        }

        private static bool IsBoundToCannon(AmmunitionClassObsolete type)
        {
            return IsBeam(type) || type == AmmunitionClassObsolete.Aura || type == AmmunitionClassObsolete.Explosion;
        }

        private static bool StickToTarget(AmmunitionClassObsolete type)
        {
            return type == AmmunitionClassObsolete.EnergyBeam || type == AmmunitionClassObsolete.EnergySiphon ||
                type == AmmunitionClassObsolete.VampiricRay || type == AmmunitionClassObsolete.SmallVampiricRay;
        }

        private static bool IsHoming(AmmunitionClassObsolete type)
        {
            return type == AmmunitionClassObsolete.Rocket || type == AmmunitionClassObsolete.AcidRocket ||
                type == AmmunitionClassObsolete.HomingImmobilizer;
        }

        private static bool HasEngine(AmmunitionClassObsolete type)
        {
            return IsHoming(type) || type == AmmunitionClassObsolete.UnguidedRocket;
        }

        private static bool HasDirectDamage(AmmunitionClassObsolete type)
        {
            switch (type)
            {
                case AmmunitionClassObsolete.Bomb:
                case AmmunitionClassObsolete.Acid:
                case AmmunitionClassObsolete.AcidRocket:
                case AmmunitionClassObsolete.Emp:
                case AmmunitionClassObsolete.Fireworks:
                case AmmunitionClassObsolete.UnguidedRocket:
                case AmmunitionClassObsolete.Rocket:
                case AmmunitionClassObsolete.FragBomb:
                // Legacy adds the damage, but the Empty prefab has no collider, so it never hits.
                case AmmunitionClassObsolete.Explosion:
                    return false;
                default:
                    return true;
            }
        }

        private static bool HasDirectImpulse(AmmunitionClassObsolete type)
        {
            return type == AmmunitionClassObsolete.Common || type == AmmunitionClassObsolete.Fragment ||
                type == AmmunitionClassObsolete.Singularity || type == AmmunitionClassObsolete.AcidRocket;
        }

        private static bool CanBeDisarmed(AmmunitionClassObsolete type)
        {
            switch (type)
            {
                case AmmunitionClassObsolete.Acid:
                case AmmunitionClassObsolete.Aura:
                case AmmunitionClassObsolete.DamageOverTime:
                case AmmunitionClassObsolete.Explosion:
                    return false;
                default:
                    return !IsBeam(type);
            }
        }

        private static bool DetonateIfExpired(AmmunitionClassObsolete type)
        {
            switch (type)
            {
                case AmmunitionClassObsolete.Bomb:
                case AmmunitionClassObsolete.Rocket:
                case AmmunitionClassObsolete.AcidRocket:
                case AmmunitionClassObsolete.UnguidedRocket:
                case AmmunitionClassObsolete.Emp:
                case AmmunitionClassObsolete.FragBomb:
                case AmmunitionClassObsolete.Fireworks:
                case AmmunitionClassObsolete.PlasmaWeb:
                    return true;
                default:
                    return false;
            }
        }

        private static AiBulletBehavior GetBehaviorType(AmmunitionClassObsolete type)
        {
            if (IsBeam(type)) return AiBulletBehavior.Beam;
            if (IsHoming(type)) return AiBulletBehavior.Homing;
            if (type == AmmunitionClassObsolete.Aura || type == AmmunitionClassObsolete.Explosion) return AiBulletBehavior.AreaOfEffect;
            return AiBulletBehavior.Projectile;
        }

        #endregion

        #region New records (mapping table, "New records")

        private enum AreaKind
        {
            None,
            Explosion,
            SmokeExplosion,
            StrongExplosion,
            Flash,
            Emp,
            Cloud,
            Web,
        }

        private sealed class PrefabSpec
        {
            public string Name;
            public BulletShape Shape;
            public string Image;
            public float Size;
            public float Margins;
        }

        // K = legacy hit radius / new hit radius at bullet size 1; Body.Size = Size × K.
        private sealed class LegacyPrefab
        {
            public decimal K = 1m;
            public int ExistingId;
            public PrefabSpec New;
            public EffectSpec BodyEffect;
        }

        private sealed class EffectSpec
        {
            public string Name;
            public int ExistingId;
            public Func<VisualEffectElementSerializable[]> Elements;
        }

        private const string RocketEngineColor = "#C9C5FF"; // BulletPrefab Rocket (3)
        private const string HalfAlpha = "#80FFFFFF";

        private static readonly EffectSpec Flash = new EffectSpec { Name = "Flash", ExistingId = 1 };
        private static readonly EffectSpec Explosion1 = new EffectSpec { Name = "Explosion1", ExistingId = 2 };

        private static readonly EffectSpec Orb = new EffectSpec
        {
            Name = "Orb",
            Elements = () => new[] { Element(VisualEffectType.FlashAdditive, "cloud", 1f, lifetime: 0.2f) },
        };

        private static readonly EffectSpec SmokeExplosion = new EffectSpec
        {
            Name = "SmokeExplosion",
            Elements = () => new[]
            {
                Element(VisualEffectType.FlashAdditive, "star", 2f),
                Element(VisualEffectType.Smoke, "CloudWhite", 1.5f, quantity: 3, color: "#000000", colorMode: ColorMode.UseMyOwn),
                Element(VisualEffectType.SmokeAdditive, "CloudWhite", 1f, quantity: 3),
                Element(VisualEffectType.Shake, null, 1f),
            },
        };

        private static readonly EffectSpec StrongExplosion = new EffectSpec
        {
            Name = "StrongExplosion",
            Elements = () => new[]
            {
                Element(VisualEffectType.Shockwave, "wave", 1.1f),
                Element(VisualEffectType.FlashAdditive, "star", 2f),
                Element(VisualEffectType.FlashAdditive, "cloud", 2f),
                Element(VisualEffectType.Shake, null, 2f),
            },
        };

        private static readonly EffectSpec FlashExplosion = new EffectSpec
        {
            Name = "FlashExplosion",
            Elements = () => new[]
            {
                Element(VisualEffectType.Flash, "star", 2f),
                Element(VisualEffectType.FlashAdditive, "star", 1f, color: "#808080", colorMode: ColorMode.UseMyOwn),
                Element(VisualEffectType.Shake, null, 0.5f),
            },
        };

        private static readonly EffectSpec EmpFlash = new EffectSpec
        {
            Name = "EmpFlash",
            Elements = () => new[]
            {
                Element(VisualEffectType.FlashAdditive, "star", 4f),
                Element(VisualEffectType.FlashAdditive, "star", 2f, colorMode: ColorMode.UseMyOwn),
            },
        };

        private static readonly EffectSpec AcidCloud = new EffectSpec
        {
            Name = "AcidCloud",
            Elements = () => new[]
            {
                Element(VisualEffectType.SmokeAdditive, "CloudWhite", 2.6f, quantity: 3, color: HalfAlpha, colorMode: ColorMode.Multiply),
            },
        };

        private static readonly EffectSpec Web = new EffectSpec
        {
            Name = "Web",
            Elements = () => new[] { Element(VisualEffectType.Sprite, "hotspot1", 2.5f) },
        };

        // Legacy RollingSpriteView: 0.2 and 0.5 turns per second over the 3 s flight, ÷ k 0.6.
        private static readonly EffectSpec RollingCloud = new EffectSpec
        {
            Name = "RollingCloud",
            Elements = () => new[]
            {
                Element(VisualEffectType.FlashAdditive, "CloudWhite", 2f, turnRate: 108f),
                Element(VisualEffectType.FlashAdditive, "CloudWhite", 2.5f, turnRate: -270f),
            },
        };

        private static readonly EffectSpec RollingFlame = new EffectSpec
        {
            Name = "RollingFlame",
            Elements = () => new[]
            {
                Element(VisualEffectType.FlashAdditive, "CloudWhite", 2f, turnRate: 135f),
                Element(VisualEffectType.FlashAdditive, "CloudWhite", 2.5f, turnRate: -135f),
            },
        };

        private static readonly EffectSpec Aura1 = new EffectSpec
        {
            Name = "Aura1",
            Elements = () => new[] { Element(VisualEffectType.Sprite, "atmosphere", 1f) },
        };

        private static readonly EffectSpec Aura2 = new EffectSpec
        {
            Name = "Aura2",
            Elements = () => new[] { Element(VisualEffectType.Sprite, "hotspot1", 1.3f) },
        };

        private static VisualEffectElementSerializable Element(VisualEffectType type, string image, float size, int quantity = 1,
            string color = null, ColorMode colorMode = ColorMode.TakeFromOwner, float turnRate = 0f, float lifetime = 1f)
        {
            return new VisualEffectElementSerializable
            {
                Type = type,
                Image = image,
                Size = size,
                Quantity = quantity,
                Color = color,
                ColorMode = colorMode,
                TurnRate = turnRate,
                Lifetime = lifetime,
            };
        }

        private static PrefabSpec Projectile(string name, string image, float size, BulletShape shape = BulletShape.Projectile, float margins = 0f)
        {
            return new PrefabSpec { Name = name, Shape = shape, Image = image, Size = size, Margins = margins };
        }

        // "Hit radius" table. New radius: 0.25 for Projectile and Rocket, 0.5 for Mine and the area prefab. A box hit
        // shape becomes a circle with half the box height (V2). Prefab size = legacy view scale / k.
        private static readonly Dictionary<string, LegacyPrefab> Prefabs = new Dictionary<string, LegacyPrefab>
        {
            ["Plasma2"] = new LegacyPrefab { K = 0.4m, New = Projectile("LegacyPlasma2", "plasma2", 2.5f) },
            ["Energy1"] = new LegacyPrefab { K = 1.2m, New = Projectile("LegacyEnergy1", "energy1", 2.5f) },
            ["Bomb"] = new LegacyPrefab { K = 1.2m, New = Projectile("LegacyEnergy1", "energy1", 2.5f) },
            ["Bomb2"] = new LegacyPrefab { K = 1m, New = Projectile("LegacyBomb2", "energy1", 2f) },
            ["Energy2"] = new LegacyPrefab { K = 1.2m, New = Projectile("LegacyEnergy2", "hotspot1", 1.25f) },
            ["Plasma1"] = new LegacyPrefab { K = 0.92m, New = Projectile("LegacyPlasma1", "plasma", 1.087f) },
            ["Bullet"] = new LegacyPrefab { K = 1.6m, New = Projectile("LegacyBullet", "ball_soft", 0.625f) },
            ["Fragment"] = new LegacyPrefab { K = 2m, New = Projectile("LegacyFragment", "fragment", 0.5f) },
            ["FragBomb"] = new LegacyPrefab { K = 2m, New = Projectile("LegacyFragBomb", "bomb", 0.5f) },
            ["EnergyBall1"] = new LegacyPrefab { K = 0.4m, New = Projectile("LegacyEnergyBall1", "twister", 3.75f, BulletShape.Mine) },
            ["TutorialBullet"] = new LegacyPrefab { K = 0.5m, New = Projectile("LegacyTutorialBullet", "tutorial_bullet", 2f) },
            ["Rocket"] = new LegacyPrefab { K = 1.4m, New = Projectile("LegacyRocket", "rocket", 0.714f, BulletShape.Rocket, 0.358f) },
            ["SatelliteRocket"] = new LegacyPrefab { K = 0.5m, New = Projectile("LegacySatelliteRocket", "rocket2", 2f, BulletShape.Rocket, 0.41f) },
            ["TutorialRocket"] = new LegacyPrefab { K = 0.5m, New = Projectile("LegacyTutorialRocket", "tutorial_bullet", 2f, BulletShape.Rocket, 0.326f) },
            ["Laser"] = new LegacyPrefab { ExistingId = 2 },
            ["Lightning"] = new LegacyPrefab { ExistingId = 5 },
            ["EnergyBeam"] = new LegacyPrefab { ExistingId = 6 },
            ["TractorBeam"] = new LegacyPrefab { New = Projectile("LegacyTractorBeam", "laser", 5f, BulletShape.LaserBeam, 1f) },
            ["Cloud"] = new LegacyPrefab { K = 0.6m, BodyEffect = RollingCloud },
            ["Flame"] = new LegacyPrefab { K = 0.6m, BodyEffect = RollingFlame },
            ["EnergyField1"] = new LegacyPrefab { BodyEffect = Aura1 },
            ["EnergyField2"] = new LegacyPrefab { BodyEffect = Aura2 },
            ["Empty"] = new LegacyPrefab(),
        };

        #endregion

        private List<NewRecord> _output;
        private int _nextAmmunitionId;
        private int _nextPrefabId;
        private int _nextEffectId;
        private readonly Dictionary<string, int> _prefabIds = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _effectIds = new Dictionary<string, int>();
    }
}
