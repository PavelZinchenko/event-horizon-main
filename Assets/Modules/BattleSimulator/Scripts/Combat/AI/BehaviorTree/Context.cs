using System.Collections.Generic;
using Combat.Component.Ship;
using Combat.Component.Unit;
using Combat.Scene;
using Combat.Ai.BehaviorTree.Utils;

namespace Combat.Ai.BehaviorTree
{
	public class Context
	{
		private const float _initialTime = -60;

		private readonly IScene _scene;
		private readonly IShip _ship;
		private readonly ShipWeaponList _allWeapons;

		private IShip _targetShip;
		private float _elapsedTime;
		private ShipWeaponList _selectedWeapons;
		private TargetList _targetList;
		private ThreatList _threatList;
		private HashSet<int> _variables;
		private float _targetListUpdateTime = _initialTime;
		private float _threatListUpdateTime = _initialTime;

		public IScene Scene => _scene;
		public IShip Ship => _ship;
		public ShipControls Controls { get; } = new();

		public IShip TargetShip { get; set; }
		public IShip LastMessageSender { get; set; }
		public float LastTargetUpdateTime { get; set; } = _initialTime;

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
		public float EnergyLevelPercentage => UnityEngine.Mathf.Clamp01((_ship.Stats.Energy.Value - LockedEnergy) / _ship.Stats.Energy.MaxValue);
		public float EnergyLevel => UnityEngine.Mathf.Max(0, _ship.Stats.Energy.Value - LockedEnergy);

		public bool RestoringEnergy { get; set; }
		public ShipWeaponList SelectedWeapons { get => _selectedWeapons ?? _allWeapons; set => _selectedWeapons = value; }

		public Context(IShip ship, IScene scene)
		{
			_scene = scene;
			_ship = ship;
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
				_targetList = new(_scene);

			if (_elapsedTime - _targetListUpdateTime < cooldown) return;
			_targetListUpdateTime = _elapsedTime;
			_targetList.Update(_ship, _targetShip);
		}

		public void UpdateThreatList(IThreatAnalyzer threatAnalyzer, float cooldown)
		{
			if (_threatList == null)
				_threatList = new(_scene);

			if (_elapsedTime - _threatListUpdateTime < cooldown) return;
			_threatListUpdateTime = _elapsedTime;
			_threatList.Update(_ship, threatAnalyzer);
		}

		public bool TrySetValue(int id, bool value)
		{
			if (value)
			{
				if (_variables == null)
					_variables = new();

				return _variables.Add(id);
			}
			else
			{
				if (_variables == null)
					return false;

				return _variables.Remove(id);
			}
		}

		public bool GetValue(int id)
		{
			return _variables != null && _variables.Contains(id);
		}
	}
}
