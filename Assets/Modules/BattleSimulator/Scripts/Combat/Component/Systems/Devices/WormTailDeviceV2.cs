using Combat.Collision;
using Combat.Component.Ship;
using Combat.Component.Stats;
using Combat.Component.Unit;
using Combat.Factory;
using GameDatabase.DataModel;
using GameDatabase.Model;
using UnityEngine;

namespace Combat.Component.Systems.Devices
{
    public class WormTailDeviceV2 : SystemBase, IDevice, IStatsModification
    {
        private readonly float _respawnCooldown;
        private readonly IShip _ship;
        private readonly GameObject _prefab;
        private readonly SpaceObjectFactory _objectFactory;
        private readonly DeviceStats _stats;

        private IWormSegment _tail;
        private float _timeToRespawn = 0;
        private int _respawnCounter;

        public WormTailDeviceV2(DeviceStats stats, GameObject prefab, IShip ship, SpaceObjectFactory objectFactory)
            : base(-1, SpriteId.Empty)
        {
            _ship = ship;
            _prefab = prefab;
            _objectFactory = objectFactory;
            _stats = stats;
            _respawnCooldown = _stats.Cooldown * 1.5f;
        }

        public GameDatabase.Enums.DeviceClass DeviceClass => _stats.DeviceClass;
		public override float ActivationCost { get { return 0f; } }
        public override bool CanBeActivated { get { return false; } }

        public override IStatsModification StatsModification { get { return this; } }
        public bool TryApplyModification(ref Resistance data)
        {
            if (_tail == null) return true;

            var power = (float)_tail.Length / _tail.MaxLength;

            data.Heat = power + (1f - power) * data.Heat;
            data.Energy = power + (1f - power) * data.Energy;
            data.Kinetic = power + (1f - power) * data.Kinetic;

            return true;
        }

        public void Deactivate() {}

        protected override void OnUpdatePhysics(float elapsedTime) 
        {
            if (_tail == null)
            {
                _timeToRespawn -= elapsedTime;
                if (_timeToRespawn <= 0)
                    _tail = CreateTail();
            }            
            else if (_tail.State == Combat.Unit.UnitState.Destroyed)
            {
                _tail = null;
                _timeToRespawn = _respawnCooldown;
            }
        }

        protected override void OnUpdateView(float elapsedTime) {}

        protected override void OnDispose() { }

        public IWormSegment CreateTail()
        {
            var weight = _ship.Body.Weight / _stats.Size;
            var hitPoints = _ship.Stats.Armor.MaxValue * _stats.Power;
            var length = Mathf.RoundToInt(_stats.Size);
            var withCooldown = _respawnCounter > 0;
            _respawnCounter++;

            return WormSegmentFactory.Create(_objectFactory, _prefab, _ship, length, weight, hitPoints, _stats.Offset.x, _stats.Offset.y, 0.15f, 
                _stats.Cooldown, withCooldown, _ship.Specification.Stats.ShipColor, null);
        }
    }
}
