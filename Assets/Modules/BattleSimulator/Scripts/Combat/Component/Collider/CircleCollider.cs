using System.Collections.Generic;
using Combat.Collision.Manager;
using Combat.Component.Unit;
using UnityEngine;
using Zenject;

namespace Combat.Component.Collider
{
    public class CircleCollider : MonoBehaviour, ICollider
    {
        [Inject] private ICollisionManager _collisionManager;

        public bool Enabled { get { return _enabled; } set { _enabled = value; } }

		public IUnit Unit { get; set; }
		public IUnit Source { get; set; }

        public float MaxRange { get; set; }

        public IUnit ActiveCollision { get; private set; }
        public IUnit ActiveTrigger => ActiveCollision;
        public Vector2 LastContactPoint { get; private set; }
        public IUnit LastCollision { get; private set; }
        public bool OneHitOnly { get; set; }
        public float StuckTime => 0;

        public void Initialize(ICollisionManager collisionManager)
        {
            _collisionManager = collisionManager;
        }

        public void UpdatePhysics(float elapsedTime)
        {
            if (Unit == null || MaxRange <= 0 || !Enabled)
            {
                _activeCollisions.Clear();
                ActiveCollision = null;
                return;
            }

            Generic.Swap(ref _lastActiveCollisions, ref _activeCollisions);
            _activeCollisions.Clear();

            var position = Unit.Body.WorldPosition();
            var count = Physics2D.OverlapCircleNonAlloc(position, MaxRange, _buffer, Unit.Type.CollisionMask);
            if (count > _buffer.Length)
                count = _buffer.Length;

            for (var i = 0; i < count; ++i)
            {
                var collider = _buffer[i];
                var other = collider.GetComponent<ICollider>();
				if (Source != null && (other.Unit == Source || other.Unit.Type.Owner == Source))
					continue;

				var target = other.Unit;
                ActiveCollision = target;
                LastCollision = target;
                LastContactPoint = target.Body.WorldPosition();
                _activeCollisions.Add(target);
                var isNew = !_lastActiveCollisions.Contains(target);
                _collisionManager.OnCollision(Unit, target, CollisionData.FromObjects(Unit, target, LastContactPoint, isNew, elapsedTime));
            }
        }

        public void Dispose()
        {
            Unit = null;
			Source = null;
            ActiveCollision = null;
            LastCollision = null;
            OneHitOnly = false;
            _activeCollisions.Clear();
            MaxRange = 0;
            _enabled = true;
        }

        private bool _enabled = true;
        private readonly Collider2D[] _buffer = new Collider2D[32];
        private HashSet<IUnit> _activeCollisions = new HashSet<IUnit>();
        private HashSet<IUnit> _lastActiveCollisions = new HashSet<IUnit>();
    }
}
