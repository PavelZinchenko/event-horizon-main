using System;
using System.Collections.Generic;
using Combat.Component.Collider;
using Combat.Component.Ship;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Combat.Unit;
using UnityEngine;

namespace Combat.Ai
{
    public class ThreatList
    {
		private float _timeToHit = 1000f;
        private float _cooldown;
        private const float UpdateInterval = 0.1f;
        private readonly IScene _scene;
        private readonly List<IUnit> _threats;
        
        public ThreatList(IScene scene)
        {
            _scene = scene;
			_threats = new List<IUnit>();
        }

        public IReadOnlyList<IUnit> Units => _threats;
        public float TimeToHit => _timeToHit;

        [Obsolete]
        public void Update(float elapsedTime, IShip ship, IThreatAnalyzer analyzer)
        {
            _cooldown -= elapsedTime;
            if (_cooldown > 0)
                return;

            _cooldown = UpdateInterval;
            Update(ship, analyzer);
        }

        public void Update(IShip ship, IThreatAnalyzer analyzer)
        {
            _threats.Clear();
            _timeToHit = 1000f;

            if (!ship.IsActive() || analyzer == null)
                return;

            var parentedObjects = new List<IUnit>();

            _scene.Units.GetObjectsInRange(_threats, parentedObjects, ship.Body.Position, 20, 1000);

            _timeToHit = 1000f;
            var index = 0;
            for (var i = 0; i < _threats.Count; ++i)
            {
                var item = _threats[i];
                if (!analyzer.IsThreat(ship, item)) continue;

                var itemPosition = ship.Body.Position.Direction(item.Body.Position);
                var velocity = ship.Body.Velocity - item.Body.Velocity;
                var dir = velocity.normalized;
                var len = Mathf.Max(0, Vector2.Dot(itemPosition, dir));

                var distance = Vector2.Distance(len * dir, itemPosition);
                var threatTime = len / velocity.magnitude;
                if (2 * distance <= item.Body.Scale + ship.Body.Scale && threatTime < 5f)
                {
                    _threats[index++] = item;

                    if (threatTime < _timeToHit)
                        _timeToHit = threatTime;
                }
            }

            if (parentedObjects.Count > 0)
            {
                var requestedCount = parentedObjects.Count + index;
				for (int i = _threats.Count; i < requestedCount; ++i)
					_threats.Add(null);
            }
            // For objects with parent, we don't consider them moving, so only check for overlap
            for (var i = 0; i < parentedObjects.Count; i++)
            {
                var item = parentedObjects[i];
                if (item.Type.Class != UnitClass.AreaOfEffect) continue;

                if (!analyzer.IsThreat(ship, item))
                    continue;

                var collider = item.Collider;

                var sqrDistance = item.Body.WorldPosition().SqrDistance(ship.Body.Position);
                if (collider is RayCastCollider)
                {
                    if (sqrDistance > collider.MaxRange * collider.MaxRange) continue;
                    var vector = RotationHelpers.Direction(item.Body.Rotation) * collider.MaxRange;
                    var distance =
                        Geometry.Point2VectorDistance(item.Body.WorldPosition().Direction(ship.Body.Position), vector);
                    if (distance <= ship.Body.Scale / 2)
                    {
						if (index < 0 || index >= _threats.Count)
						{
							Debug.LogError($"Index out of range: {index} / {_threats.Count}");
							Debug.Break();
						}

                        _threats[index++] = item;
                        _timeToHit = 0;
                    }
                }
                else if (collider is CommonCollider)
                {
                    if (sqrDistance <= item.Body.Scale * item.Body.Scale / 4)
                    {
                        _threats[index++] = item;
                        _timeToHit = 0;
                    }
                } else if (collider is CircleCollider)
                {
                    if (sqrDistance <= collider.MaxRange * collider.MaxRange)
                    {
                        _threats[index++] = item;
                        _timeToHit = 0;
                    }
                }
            }

            var count = _threats.Count - index;
            if (count > 0)
                _threats.RemoveRange(index, count);
        }
    }
}
