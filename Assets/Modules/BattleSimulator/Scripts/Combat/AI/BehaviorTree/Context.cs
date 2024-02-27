using System.Collections.Generic;
using Combat.Component.Ship;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Combat.Ai.BehaviorTree.Utils;

namespace Combat.Ai.BehaviorTree
{
	public class Context
	{
		private const float _initialTime = -60;

		private readonly ShipWeaponList _allWeapons;

        private IShip _mothership;
        private float _elapsedTime;
		private ShipWeaponList _selectedWeapons;
		private TargetList _targetList;
		private ThreatList _threatList;
		private float _targetListUpdateTime = _initialTime;
		private float _threatListUpdateTime = _initialTime;

		private AutoExpandingList<IShip> _savedTargets;
		private BitArray64 _savedFlags;

        public bool IsDrone => Ship.Type.Class == UnitClass.Drone;
		public IScene Scene { get; }
		public IShip Ship { get; }
		public ShipControls Controls { get; } = new();

        public IShip TargetShip { get; set; }

        public IShip Mothership 
        {
            get => IsDrone ? Ship.Type.Owner : _mothership;
            set 
            {
                if (IsDrone)
                    Ship.Type.Owner = value;
                else 
                    _mothership = value;
            } 
        }

        public IShip LastMessageSender { get; set; }
		public float LastTargetUpdateTime { get; set; } = _initialTime;
		public float LastTextMessageTime { get; set; } = _initialTime;

		public IReadOnlyList<IShip> SecondaryTargets => _targetList?.Items ?? EmptyList<IShip>.Instance;

		public IReadOnlyList<IUnit> Threats => _threatList?.Units ?? EmptyList<IUnit>.Instance;
		public float TimeToCollision => _threatList != null ? _threatList.TimeToHit : float.MaxValue;

		public bool HaveWeapons => _allWeapons.Count > 0;
		public float AttackRangeMax => _allWeapons.RangeMax;
		public float AttackRangeMin => _allWeapons.RangeMin;

		public int FrameId { get; private set; }
		public float DeltaTime { get; private set; }
		public float LockedEnergy { get; set; }
		public float Time => _elapsedTime;
		public float EnergyLevelPercentage => UnityEngine.Mathf.Clamp01((Ship.Stats.Energy.Value - LockedEnergy) / Ship.Stats.Energy.MaxValue);
		public float EnergyLevel => UnityEngine.Mathf.Max(0, Ship.Stats.Energy.Value - LockedEnergy);

		public bool RestoringEnergy { get; set; }
		public ShipWeaponList SelectedWeapons { get => _selectedWeapons ?? _allWeapons; set => _selectedWeapons = value; }

		public Context(IShip ship, IScene scene)
		{
			Scene = scene;
			Ship = ship;
			_allWeapons = new ShipWeaponList(ship);
		}

		public void Update(float deltaTime) 
		{
			_elapsedTime += deltaTime;
			DeltaTime = deltaTime;
			FrameId++;
			LockedEnergy = 0;
		}

		public void UpdateTargetList(float cooldown)
		{
			if (_targetList == null) 
				_targetList = new(Scene);

			if (_elapsedTime - _targetListUpdateTime < cooldown) return;
			_targetListUpdateTime = _elapsedTime;
			_targetList.Update(Ship, TargetShip);
		}

		public void UpdateThreatList(IThreatAnalyzer threatAnalyzer, float cooldown)
		{
			if (_threatList == null)
				_threatList = new(Scene);

			if (_elapsedTime - _threatListUpdateTime < cooldown) return;
			_threatListUpdateTime = _elapsedTime;
			_threatList.Update(Ship, threatAnalyzer);
		}

		public bool TrySetValue(int id, bool value)
		{
			var currentValue = _savedFlags[id];
			if (currentValue == value)
				return false;

			_savedFlags[id] = value;
			return true;
		}

		public bool GetValue(int id) => _savedFlags[id];

		public bool TrySaveTarget(int id, IShip target)
		{
			if (_savedTargets[id] == target)
				return false;

			_savedTargets[id] = target;
			return true;
		}

		public IShip LoadTarget(int id)
		{
			var target = _savedTargets[id];
			return target != null && target.State == Unit.UnitState.Active ? target : null;
		}
	}
}
