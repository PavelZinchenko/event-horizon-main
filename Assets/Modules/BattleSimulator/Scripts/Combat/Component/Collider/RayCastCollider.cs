using Combat.Collision.Manager;
using Combat.Component.Body;
using Combat.Component.Unit;
using Combat.Component.View;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Combat.Component.Collider
{
    public class RayCastCollider : MonoBehaviour, ICollider
    {
        [Inject] private ICollisionManager _collisionManager;

        [SerializeField] private BaseView _view;
        [SerializeField] private GameObjectBody _body;
        [SerializeField] private bool _passThrough;

        public bool Enabled { get { return _enabled; } set { _enabled = value; } }

		public IUnit Source { get; set; }
		public IUnit Unit { get; set; }

        public void Initialize(ICollisionManager collisionManager)
        {
            _collisionManager = collisionManager;
        }

        public float MaxRange
        {
            get { return _maxRange; }
            set
            {
                _maxRange = value;
                _needUpdateView = true;
            }
        }

        public IUnit ActiveCollision { get; private set; }
        public IUnit ActiveTrigger => ActiveCollision;
        public Vector2 LastContactPoint { get; private set; }
        public IUnit LastCollision { get; private set; }
        public bool OneHitOnly { get; set; }
        public float StuckTime => 0;

        public void UpdatePhysics(float elapsedTime)
        {
            if (Unit == null || MaxRange <= 0 || !Enabled)
            {
                ActiveCollision = null;
                return;
            }

            var position = Unit.Body.WorldPositionNoOffset();
            var direction = RotationHelpers.Direction(Unit.Body.WorldRotation());

            var hits = Physics2D.RaycastNonAlloc(position, direction, _buffer, MaxRange, Unit.Type.CollisionMask);
            bool collisionFound = false;
			for (int i = 0; i < hits; ++i)
			{
                ref var hit = ref _buffer[_passThrough ? hits - i - 1 : i];
				var collider = hit.collider;
				if (collider == null) continue;
				var target = collider.GetComponent<ICollider>();

                if (target == null) 
                    continue;
				if (Source != null && (target.Unit == Source || target.Unit.Type.Owner == Source))
					continue;

                ProcessCollision(target, position, hit.point, elapsedTime, !collisionFound);
                collisionFound = true;
                if (!_passThrough) break;
            }

            if (!collisionFound && ActiveCollision != null)
            {
                ActiveCollision = null;
                UpdateLength(MaxRange);
            }

            if (_needUpdateView)
                UpdateLength(MaxRange);
        }

        private void ProcessCollision(ICollider target, Vector2 position, Vector2 hitPoint, float elapsedTime, bool isFirst)
        {
            var distance = Vector2.Distance(position, hitPoint);

            if (!_passThrough && target.Unit == Unit.Type.Owner)
            {
                ActiveCollision = null;
                UpdateLength(distance);
                return;
            }

            var isNew = isFirst && ActiveCollision != target.Unit;
            if (isFirst)
            {
                ActiveCollision = target.Unit;
                LastCollision = target.Unit;
                LastContactPoint = hitPoint;
            }

            if (!_passThrough) UpdateLength(distance);

            if (isNew || !OneHitOnly)
                _collisionManager.OnCollision(Unit, target.Unit, CollisionData.FromRaycastHit2D(hitPoint, isNew, elapsedTime));
        }

        private void UpdateLength(float length)
        {
            _view.Size = length;
            if (_body != null) _body.Offset = length;
            _needUpdateView = false;
        }

        public void Dispose()
        {
            Unit = null;
			Source = null;
            ActiveCollision = null;
            LastCollision = null;
            OneHitOnly = false;
            MaxRange = 0;
            _enabled = true;
            _collisions.Clear();
        }

        private HashSet<IUnit> _collisions = new();
        private RaycastHit2D[] _buffer = new RaycastHit2D[8];
        private float _maxRange;
        private bool _needUpdateView;
        private bool _enabled = true;
    }
}
