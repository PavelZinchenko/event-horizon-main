using System.Collections.Generic;
using Combat.Component.Body;
using Combat.Component.Features;
using Combat.Component.Ship;
using Combat.Component.Platform;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Unit;
using UnityEngine;

namespace Combat.Scene
{
    public struct EnemyMatchingOptions
    {
        public float MaxDistance;

        public bool IgnoreDrones;
        public bool UsePriority;
		public bool UseDroneCamouflage;
		public bool UseMissileCamouflage;

		public int IgnoreDecoyChance;
        public int TakeDecoyChance;

        //public int Seed;

        public static EnemyMatchingOptions EnemyForDrone(float maxDistance/*, int seed*/)
        {
            return new EnemyMatchingOptions
            {
                IgnoreDecoyChance = 0,
                TakeDecoyChance = 30,

                IgnoreDrones = false,
                UsePriority = false,
				UseDroneCamouflage = true,

				MaxDistance = maxDistance,

                //Seed = seed,
            };
        }

        public static EnemyMatchingOptions EnemyForShip(float maxDistance/*, int seed*/)
        {
            return new EnemyMatchingOptions
            {
                IgnoreDecoyChance = 100,
                TakeDecoyChance = 0,

                IgnoreDrones = false,
                UsePriority = false,

                MaxDistance = maxDistance,

                //Seed = seed,
            };
        }

        public static EnemyMatchingOptions EnemyForSatellite()
        {
            return new EnemyMatchingOptions
            {
                IgnoreDecoyChance = 0,
                TakeDecoyChance = 0,
                IgnoreDrones = false,
                UsePriority = false,
            };
        }
    }

    public static class ShipListExtensions
    {
        public static IShip GetEnemy(this IUnitList<IShip> shipList, IUnit unit, EnemyMatchingOptions options)
        {
            IShip enemy = null;
			var random = new LazyRandom();
			float minRange = float.MaxValue;
			var ignoreDecoy = options.IgnoreDecoyChance >= 100 || options.IgnoreDecoyChance > 0 && random.Random.Percentage(options.IgnoreDecoyChance);
			var takeDecoy = options.TakeDecoyChance >= 100 || options.TakeDecoyChance > 0 && random.Random.Percentage(options.TakeDecoyChance);

			lock (shipList.LockObject)
            {
                foreach (var ship in shipList.Items)
                {
                    if (!ship.IsActive() || ship.Type.Side.IsAlly(unit.Type.Side) || ship.Features.TargetPriority == TargetPriority.None || ship.Type.Class == UnitClass.Limb)
                        continue;
                    if (options.IgnoreDrones && ship.Type.Class == UnitClass.Drone)
                        continue;
                    if (ignoreDecoy && ship.Type.Class == UnitClass.Decoy)
                        continue;
					if (options.UseDroneCamouflage && ship.Features.ChanceToAvoidDrone > 0f && random.Random.Percentage(ship.Features.ChanceToAvoidDrone))
						continue;

					var dir = unit.Body.Position.Direction(ship.Body.Position);
                    var range = dir.magnitude - unit.Body.Scale/2 - ship.Body.Scale/2;

                    if (options.MaxDistance > 0)
                    {
                        if (ship.Type.Class == UnitClass.Drone)
                        {
                            if (ship.Type.Owner != null && ship.Type.Owner.Body.Position.Distance(ship.Body.Position) > options.MaxDistance)
                                continue;
                        }
                        else if (range > options.MaxDistance)
                            continue;
                    }

                    if (enemy == null)
                    {
                        minRange = range;
                        enemy = ship;
                        continue;
                    }

                    if (takeDecoy && ship.Type.Class == UnitClass.Decoy && enemy.Type.Class != UnitClass.Decoy)
                    {
                        enemy = ship;
                        minRange = range;
                        continue;
                    }

                    if (options.UsePriority && ship.Features.TargetPriority < enemy.Features.TargetPriority)
                        continue;

                    if (range < minRange || (options.UsePriority && ship.Features.TargetPriority > enemy.Features.TargetPriority))
                    {
                        minRange = range;
                        enemy = ship;
                    }
                }
            }

            return enemy;
        }

        public static IShip GetEnemyForTurret(this IUnitList<IShip> shipList, IUnit unit, Vector2 turretPosition,
            float turretMountAngle, float turningRange, float maxDistance, bool ignoreUnreachable = false)
        {
            const float maxTrackingDistance = 2f;

            IShip enemy = null;
            float enemyDistance = float.MaxValue;
            bool validEnemyFound = false;

            lock (shipList.LockObject)
            {
                foreach (var ship in shipList.Items)
                {
                    if (!ship.IsActive() || ship.Type.Side.IsAlly(unit.Type.Side) || ship.Features.TargetPriority == TargetPriority.None || ship.Type.Class == UnitClass.Limb)
                        continue;

                    var direction = turretPosition.Direction(ship.Body.Position);
                    var distance = direction.magnitude - ship.Body.Scale/2;

                    var isValidTarget = true;
                    if (distance > maxDistance)
                    {
                        if (validEnemyFound || ignoreUnreachable || distance > maxDistance*maxTrackingDistance) continue;
                        isValidTarget = false;
                    }

                    if (turningRange < 180)
                    {
                        var targetAngle = RotationHelpers.Angle(direction);
                        var deltaAngle = Mathf.Abs(Mathf.DeltaAngle(targetAngle, turretMountAngle));
                        if (deltaAngle > turningRange)
                        {
                            if (validEnemyFound || ignoreUnreachable) continue;
                            isValidTarget = false;
                        }
                    }

                    if (isValidTarget == validEnemyFound)
                    {
                        var priority = enemy == null ? 0 : ship.Features.TargetPriority - enemy.Features.TargetPriority;
                        if (priority < 0 || priority == 0 && distance >= enemyDistance) continue;
                    }

                    enemy = ship;
                    enemyDistance = distance;
                    validEnemyFound |= isValidTarget;
                }
            }

            return enemy;
        }

        public static IShip GetEnemy(this IUnitList<IShip> shipList, IUnit unit, float rotation, float maxRange, float maxDeviation, bool trueVision, bool ignoreDrones)
        {
            IShip enemy = null;
            float minRange = float.MaxValue;
            float minDeviation = 360f;
            bool isMatch = false;

            lock (shipList.LockObject)
            {
                foreach (var ship in shipList.Items)
                {
                    if (!ship.IsActive() || ship.Type.Side.IsAlly(unit.Type.Side) || ship.Type.Class == UnitClass.Limb)
                        continue;

                    if (trueVision && ship.Type.Class == UnitClass.Decoy)
                        continue;

                    var targetPriority = ship.Features.TargetPriority;
                    if (targetPriority == TargetPriority.None && !trueVision)
                        continue;

					var chanceToAvoidMissile = ship.Features.ChanceToAvoidMissile;
					if (chanceToAvoidMissile > 0f && !trueVision && new System.Random().Percentage(chanceToAvoidMissile))
						continue;

					if (ignoreDrones && ship.Type.Class == UnitClass.Drone)
                        continue;

                    var dir = unit.Body.Position.Direction(ship.Body.Position);
                    var range = dir.magnitude - unit.Body.Scale / 2 - ship.Body.Scale / 2;
                    var deviation =
                        Mathf.Abs(Mathf.DeltaAngle(RotationHelpers.Angle(dir), unit.Body.Rotation + rotation));

                    bool betterTarget = false;
                    bool isNearer = deviation <= maxDeviation ? range < minRange : deviation < minDeviation;

                    if (range <= maxRange && deviation <= maxDeviation)
                    {
                        if (!isMatch)
                        {
                            betterTarget = true;
                            isMatch = true;
                        }
                        else if (enemy.Features.TargetPriority < targetPriority)
                        {
                            betterTarget = true;
                        }
                        else if (enemy.Features.TargetPriority == targetPriority)
                        {
                            betterTarget = isNearer;
                        }
                    }
                    else
                    {
                        betterTarget = isNearer;
                    }

                    if (betterTarget)
                    {
                        minRange = range;
                        minDeviation = deviation;
                        enemy = ship;
                    }
                }
            }

            return enemy;
        }

        public static void GetEnemyShips(this IUnitList<IShip> shipList, IList<IShip> targetList, UnitSide side)
        {
            lock (shipList.LockObject)
            {
                var ships = shipList.Items;
                var count = ships.Count;
                targetList.Clear();

                for (var i = 0; i < count; ++i)
                {
                    var ship = ships[i];
                    if (ship.Type.Class == UnitClass.Limb)
                        continue;
                    if (ship.Type.Side.IsEnemy(side))
                        targetList.Add(ship);
                }
            }
        }

        /// <summary>
        /// Returns list of all objects WITHOUT PARENTS within a specified radius around the center point
        /// </summary>
        /// <param name="unitList">list to fetch units from</param>
        /// <param name="targetList">list to write targets to</param>
        /// <param name="center">center point</param>
        /// <param name="radius">max radius around the center point</param>
        public static void GetObjectsInRange(this IUnitList<IUnit> unitList, IList<IUnit> targetList, Vector2 center, float radius)
        {
            lock (unitList.LockObject)
            {
                var units = unitList.Items;
                var count = units.Count;
                targetList.Clear();
                var sqrRadius = radius*radius;

                for (var i = 0; i < count; ++i)
                {
                    var unit = units[i];
                    if (unit.Body.Parent != null)
                        continue;
                    if (unit.Body.Position.SqrDistance(center) < sqrRadius)
                        targetList.Add(unit);
                }
            }
        }
        

        /// <summary>
        /// Functionally identical to GetObjectsInRange, but also returns objects with parents as a separate list and
        /// with its separate max tracking range
        /// </summary>
        /// <param name="unitList">list to fetch units from</param>
        /// <param name="targetList">list to write targets without parents to</param>
        /// <param name="parentedTargetsList">list to write targets with parents to</param>
        /// <param name="center">center point</param>
        /// <param name="radius">max radius around the center point</param>
        /// <param name="parentedRadius">max radius around the center point for objects with parents</param>
        public static void GetObjectsInRange(this IUnitList<IUnit> unitList, IList<IUnit> targetList, IList<IUnit> parentedTargetsList, Vector2 center, float radius, float parentedRadius)
        {
            lock (unitList.LockObject)
            {
                var units = unitList.Items;
                var count = units.Count;
                targetList.Clear();
                var sqrRadius = radius*radius;
                var sqrParRadius = parentedRadius*parentedRadius;

                for (var i = 0; i < count; ++i)
                {
                    var unit = units[i];
                    if (unit.Body.Parent != null)
                    {
                        if (unit.Body.Position.SqrDistance(center) < sqrParRadius)
                            parentedTargetsList?.Add(unit);
                        continue;
                    }
                    if (unit.Body.Position.SqrDistance(center) < sqrRadius)
                        targetList.Add(unit);
                }
            }
        }

        private struct LazyRandom
        {
            private System.Random _random;

            public System.Random Random => _random ??= new();
        }
    }
}
