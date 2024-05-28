using System.Collections.Generic;
using Combat.Component.Ship;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Combat.Unit;

namespace Combat.Ai
{
    public class TargetListBase : ITargetList
    {
        public TargetListBase(IScene scene)
        {
            _scene = scene;
        }

        public IReadOnlyList<IShip> Enemies { get { return _enemies; } }
        public IReadOnlyList<IShip> Allies { get { return _allies; } }

        protected void Update(IShip ship, IShip enemy, bool allies, bool enemies)
        {
            if (!ship.IsActive())
                return;
            
            if (enemies || allies)
                GetTargets(ship, enemies, allies, enemy);

            if (enemies)
                SelectBestTargets(ship, _enemies);
            if (allies)
                SelectBestTargets(ship, _allies);
        }

        private void SelectBestTargets(IShip ship, List<IShip> targets)
        {
            if (targets.Count < 5)
                return;

            var range1 = Distance(ship, targets[0]);
            var range2 = Distance(ship, targets[1]);
            var range3 = Distance(ship, targets[2]);
            var range4 = Distance(ship, targets[3]);
            var range5 = Distance(ship, targets[4]);

            for (var i = 5; i < targets.Count; ++i)
            {
                var range = Distance(ship, targets[i]);
                CompareAndSwapIfBetter(targets, 0, i, ref range1, ref range);
                CompareAndSwapIfBetter(targets, 1, i, ref range2, ref range);
                CompareAndSwapIfBetter(targets, 2, i, ref range3, ref range);
                CompareAndSwapIfBetter(targets, 3, i, ref range4, ref range);
                CompareAndSwapIfBetter(targets, 4, i, ref range5, ref range);
            }

            targets.RemoveRange(5, targets.Count - 5);
        }

        private void GetTargets(IShip ship, bool enemies, bool allies, IShip primaryTarget)
        {
            _enemies.Clear();
            _allies.Clear();
            var side = ship.Type.Side;

            lock (_scene.Ships.LockObject)
            {
                var ships = _scene.Ships.Items;

                for (int i = 0; i < ships.Count; ++i)
                {
                    var target = ships[i];
                    if (target == ship || target == primaryTarget || !target.IsActive())
                        continue;

                    var type = target.Type.Class;
                    if (type == UnitClass.Decoy)
                        continue;

                    if (target.Type.Side.IsAlly(side) && allies)
                    {
                        if (type == UnitClass.Drone)
                            continue;

                        _allies.Add(target);
                    }

                    if (target.Type.Side.IsEnemy(side) && enemies)
                    {
                        _enemies.Add(target);
                    }
                }
            }
        }

        private void CompareAndSwapIfBetter(IList<IShip> ships, int index1, int index2, ref float range1, ref float range2)
        {
            if (range1 <= range2)
                return;

            var temp = range1;
            range1 = range2;
            range2 = temp;

            var ship = ships[index1];
            ships[index1] = ships[index2];
            ships[index2] = ship;
        }

        private static float Distance(IShip ship1, IShip ship2)
        {
            return ship1.Body.Position.Direction(ship2.Body.Position).sqrMagnitude;
        }

        private readonly List<IShip> _enemies = new List<IShip>();
        private readonly List<IShip> _allies = new List<IShip>();
        private readonly IScene _scene;
    }
}
