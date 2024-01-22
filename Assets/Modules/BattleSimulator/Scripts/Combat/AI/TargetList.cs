using System;
using System.Collections.Generic;
using Combat.Component.Ship;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Combat.Unit;

namespace Combat.Ai
{
    public class TargetList
    {
		private float _cooldown;
		private IShip _primaryTarget;
		private const float UpdateInterval = 1.0f;
		private readonly List<IShip> _targets;
		private readonly IReadOnlyList<IShip> _targetsReadOnly;
		//private readonly List<IUnit> _loot;
		private readonly IScene _scene;
		
		public TargetList(IScene scene, bool lookForLoot = false)
        {
            _scene = scene;
			_targets = new List<IShip>();
			_targetsReadOnly = _targets.AsReadOnly();

			//if (lookForLoot)
			//    _loot = new List<IUnit>();
		}

		public IReadOnlyList<IShip> Items => _targetsReadOnly;
		//public System.Collections.ObjectModel.ReadOnlyCollection<IUnit> Loot { get { return _loot != null ? _loot.AsReadOnly() : null; } }

		[Obsolete] public void Update(float elapsedTime, IShip ship, IShip enemy)
		{
			_cooldown -= elapsedTime;
			if (_cooldown > 0)
				return;

			_cooldown = UpdateInterval;
			Update(ship, enemy);
		}

		public void Update(IShip ship, IShip enemy)
		{
			if (!ship.IsActive())
	            return;

            _scene.Ships.GetEnemyShips(_targets, ship.Type.Side);
            _primaryTarget = enemy;

            _targets.RemoveAll(IsBadTarget);

            //if (_loot != null && _targets.Count == 0)
            //    LookForLoot();

            if (_targets.Count < 5)
                return;

            var range1 = Distance(ship, _targets[0]);
            var range2 = Distance(ship, _targets[1]);
            var range3 = Distance(ship, _targets[2]);
            var range4 = Distance(ship, _targets[3]);
            var range5 = Distance(ship, _targets[4]);

            for (var i = 5; i < _targets.Count; ++i)
            {
                var range = Distance(ship, _targets[i]);
                CompareAndSwapIfBetter(0, i, ref range1, ref range);
                CompareAndSwapIfBetter(1, i, ref range2, ref range);
                CompareAndSwapIfBetter(2, i, ref range3, ref range);
                CompareAndSwapIfBetter(3, i, ref range4, ref range);
                CompareAndSwapIfBetter(4, i, ref range5, ref range);
            }

            _targets.RemoveRange(5, _targets.Count - 5);
        }

        private bool IsBadTarget(IShip ship)
        {
            return ship == _primaryTarget || !ship.IsActive() || ship.Type.Class == UnitClass.Decoy;
        }

        private void CompareAndSwapIfBetter(int index1, int index2, ref float range1, ref float range2)
        {
            if (range1 <= range2)
                return;

            var temp = range1;
            range1 = range2;
            range2 = temp;

            var ship = _targets[index1];
            _targets[index1] = _targets[index2];
            _targets[index2] = ship;
        }

        //private void LookForLoot()
        //{
        //    _loot.Clear();
        //    lock (_scene.Units.LockObject)
        //    {
        //        foreach (var item in _scene.Units.Items)
        //        {
        //            if (item.Type.Class != UnitClass.Loot)
        //                continue;
        //            _loot.Add(item);
        //        }
        //    }
        //}

        private static float Distance(IShip ship1, IShip ship2)
        {
            return ship1.Body.Position.Direction(ship2.Body.Position).sqrMagnitude;
        }
    }
}
