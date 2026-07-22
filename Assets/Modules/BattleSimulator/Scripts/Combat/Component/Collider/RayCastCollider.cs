using Combat.Collision.Manager;
using Combat.Component.Body;
using Combat.Component.Unit;
using Combat.Component.View;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Combat.Component.Systems.Devices;

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
            var effectiveRange = MaxRange;
            var blockedByWarpTrail = WarpTrailEffect.TryBlockRay(position, direction, MaxRange, out var trailRange);
            if (blockedByWarpTrail)
                effectiveRange = trailRange;
            if (StrategicFieldEffect.TryBlockRay(position, direction, effectiveRange, out var strategicRange))
                effectiveRange = Mathf.Min(effectiveRange, strategicRange);

            // A player-owned macro-electron intentionally lives on a neutral
            // missile layer so both sides can target it. That layer is not in
            // an allied laser's normal collision mask, therefore the beam
            // never reached CollisionManager even though friendly charging is
            // allowed there. Scan all physics layers, then retain the original
            // mask for every ordinary target and make only ball lightning the
            // explicit exception.
            // Macro-electrons use trigger colliders. Query triggers explicitly
            // instead of inheriting Physics2D.queriesHitTriggers, otherwise
            // lasers can pass straight through them on battle configurations
            // where global trigger queries are disabled.
            var contactFilter = new ContactFilter2D();
            contactFilter.NoFilter();
            contactFilter.SetLayerMask(Physics2D.AllLayers);
            contactFilter.useTriggers = true;
            var hits = Physics2D.Raycast(position, direction, contactFilter, _buffer, effectiveRange);
            // RaycastNonAlloc does not guarantee hit ordering.  The old loop
            // could therefore stop at a farther collider before reaching a
            // macro-electron that was visibly in front of it.  Keep beam
            // collision deterministic by sorting the populated range.
            for (var i = 1; i < hits; ++i)
            {
                var value = _buffer[i];
                var j = i - 1;
                while (j >= 0 && _buffer[j].distance > value.distance)
                {
                    _buffer[j + 1] = _buffer[j];
                    --j;
                }
                _buffer[j + 1] = value;
            }
            bool collisionFound = false;
			for (int i = 0; i < hits; ++i)
			{
                ref var hit = ref _buffer[i];
				var collider = hit.collider;
				if (collider == null) continue;
                // Several projectile prefabs keep the Unity Collider2D on a
                // child while their combat collider is attached to the root.
                // GetComponent alone made those targets (notably ball
                // lightning) invisible to laser beams.
                var target = collider.GetComponent<ICollider>() ?? collider.GetComponentInParent<ICollider>();

                if (target == null || target.Unit == null)
                    continue;
                var nativeLayer = (Unit.Type.CollisionMask & (1 << collider.gameObject.layer)) != 0;
                if (!nativeLayer && !IsBallLightning(target.Unit))
                    continue;
				if (Source != null && (target.Unit == Source ||
                    target.Unit.Type.Owner == Source && !IsBallLightning(target.Unit)))
					continue;

                ProcessCollision(target, position, hit.point, elapsedTime, !collisionFound);
                collisionFound = true;
                if (!_passThrough) break;
            }

            if (!collisionFound)
            {
                ActiveCollision = null;
                UpdateLength(effectiveRange);
            }

            if (_needUpdateView)
                UpdateLength(effectiveRange);
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
        private RaycastHit2D[] _buffer = new RaycastHit2D[64];
        private float _maxRange;
        private bool _needUpdateView;
        private bool _enabled = true;

        private static bool IsBallLightning(IUnit unit)
        {
            return unit is Combat.Component.Bullet.Bullet bullet &&
                   bullet.Controller is Combat.Component.Controller.BallLightningController;
        }
    }
}
