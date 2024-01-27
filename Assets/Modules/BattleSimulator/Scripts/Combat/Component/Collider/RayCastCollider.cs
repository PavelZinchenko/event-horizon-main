using Combat.Collision.Manager;
using Combat.Component.Body;
using Combat.Component.Unit;
using Combat.Component.View;
using UnityEngine;
using Zenject;

namespace Combat.Component.Collider
{
    public class RayCastCollider : MonoBehaviour, ICollider
    {
        [Inject] private readonly ICollisionManager _collisionManager;

        [SerializeField] private BaseView _view;
        [SerializeField] private GameObjectBody _body;

        public bool Enabled { get { return _enabled; } set { _enabled = value; } }

		public IUnit Source { get; set; }
		public IUnit Unit { get; set; }

		public float MaxRange
        {
            get { return _maxRange; }
            set
            {
                _maxRange = value;
                _needUpdateView = true;
            }
        }

        //public bool IsTrigger { get; set; }

        public IUnit ActiveCollision { get; private set; }
        public Vector2 LastContactPoint { get; private set; }

        public void UpdatePhysics(float elapsedTime)
        {
            if (Unit == null || MaxRange <= 0 || !Enabled)
            {
                ActiveCollision = null;
                return;
            }

            var position = Unit.Body.WorldPositionNoOffset();
            var direction = RotationHelpers.Direction(Unit.Body.WorldRotation());

            //if (IsTrigger)
            //{
            //    if (_hitsBuffer == null)
            //        _hitsBuffer = new RaycastHit2D[32];

            //    var hits = Physics2D.RaycastNonAlloc(position, direction, _hitsBuffer, MaxRange, Unit.Type.CollisionMask);
            //    for (var i = 0; i < hits; ++i)
            //    {
            //        var hit = _hitsBuffer[i];
            //        var other = hit.collider.GetComponent<ICollider>();
            //        ActiveCollision = other.Unit;
            //        _collisionManager.OnCollisionEnter(Unit, other.Unit, CollisionData.FromRaycastHit2D(hit));
            //        LastContactPoint = hit.point;
            //    }

            //    _view.Size = MaxRange;
            //}
            //else
            {
                var hits = Physics2D.RaycastNonAlloc(position, direction, _buffer, MaxRange, Unit.Type.CollisionMask);

				var point = Vector2.zero;
				ICollider other = null;
				for (int i = 0; i < hits; ++i)
				{
					var collider = _buffer[i].collider;
					if (collider == null) continue;
					var target = collider.GetComponent<ICollider>();
					if (target.Unit == Source || target.Unit.Type.Owner == Source)
						continue;

					other = target;
					point = _buffer[i].point;
					break;
				}

                if (other != null)
                {
                    var distance = Vector2.Distance(position, point);

                    if (other.Unit == Unit.Type.Owner)
                    {
                        ActiveCollision = null;
                        _view.Size = distance;
                        if (_body != null) _body.Offset = distance;
                        return;
                    }

                    var isNew = ActiveCollision != other.Unit;
                    ActiveCollision = other.Unit;
                    LastContactPoint = point;
                    _view.Size = distance;
                    if (_body != null) _body.Offset = distance;
                    _collisionManager.OnCollision(Unit, other.Unit, CollisionData.FromRaycastHit2D(point, isNew, elapsedTime));
                }
                else if (ActiveCollision != null || _needUpdateView)
                {
                    _view.Size = MaxRange;
                    if (_body != null) _body.Offset = MaxRange;

                    ActiveCollision = null;
                }
            }
        }

        public void Dispose()
        {
            Unit = null;
			Source = null;
            ActiveCollision = null;
            MaxRange = 0;
            _enabled = true;
        }

        private RaycastHit2D[] _buffer = new RaycastHit2D[4];
        private float _maxRange;
        private bool _needUpdateView;
        private bool _enabled = true;
    }
}
