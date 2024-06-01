using Combat.Collision;
using Combat.Collision.Behaviour;
using Combat.Collision.Manager;
using Combat.Component.Body;
using Combat.Component.Collider;
using Combat.Component.Controls;
using Combat.Component.Engine;
using Combat.Component.Features;
using Combat.Component.Physics;
using Combat.Component.Physics.Joint;
using Combat.Component.Platform;
using Combat.Component.Ship;
using Combat.Component.Ship.Effects;
using Combat.Component.Stats;
using Combat.Component.Systems;
using Combat.Component.Triggers;
using Combat.Component.Unit.Classification;
using Combat.Component.View;
using Combat.Unit;
using Constructor;
using UnityEngine;

namespace Combat.Component.Unit
{
    public interface IWormSegment : IUnit
    {
        IDamageIndicator DamageIndicator { get; set; }
        int MaxLength { get; }
        int Length { get; }
        void InitiateSelfDestruct();
        void Detach();
        void Destroy();
        bool TryDealDamage(ref float damage);
    }

    public abstract class WormSegmentBase : UnitBase, IWormSegment
    {
        public WormSegmentBase(IShip ship, IBody body, IView view, ICollider collider, PhysicsManager physics,
            float initialCooldown, float respawnCooldown, IWormSegmentFactory factory)
            : base(new UnitType(UnitClass.Limb, ship.Type.Side, ship), body, view, collider, physics)
        {
            _factory = factory;
            _state = UnitState.Active;
            _features = new Features.Features(TargetPriority.High, Color.white);
            _nextSegmentSpawnCooldown = respawnCooldown;
            _timeToSpawnNextSegment = initialCooldown;
        }

        public virtual int Length => _nextSegment == null ? 1 : 1 + _nextSegment.Length;
        public int MaxLength => _factory.MaxLength;

        public void Attach(IUnit parent, float parentOffset, float offset, float maxAngle)
        {
            Body.Move(parent.Body.Position - RotationHelpers.Direction(parent.Body.Rotation) * parent.Body.Scale);
            Body.Turn(parent.Body.Rotation);

            _parent = parent;
            _offset = offset;
            _maxAngle = maxAngle;
            _parentOffset = parentOffset;
            _joint = parent.Physics.CreateHingeJoint(Physics, parentOffset, offset, -maxAngle, maxAngle);
        }

        public void Detach()
        {
            _joint?.Dispose();
            _parent = null;
            _joint = null;
            InitiateSelfDestruct();
        }

        public void InitiateSelfDestruct()
        {
            if (_selfDestructTimer > 0) return;
            _selfDestructTimer = _selfDestructCooldown;
            _nextSegment?.InitiateSelfDestruct();
        }

        public bool Indestructable { get; set; }
        public IDamageIndicator DamageIndicator { get; set; }
        public override ICollisionBehaviour CollisionBehaviour { get { return null; } }
        public override UnitState State { get { return _state; } }

        protected IWormSegment NextSegment => _nextSegment;
        protected void ResetSpawnCooldown() => _timeToSpawnNextSegment = _nextSegmentSpawnCooldown;
        protected bool SelfDestructPenging => _selfDestructTimer > 0;
        protected bool Attached => _parent != null && _parent.State == UnitState.Active;

        public override void OnCollision(Impact impact, IUnit target, CollisionData collisionData)
        {
            Affect(impact, target);
        }

        protected override void OnUpdateView(float elapsedTime) { }

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            if (_selfDestructTimer > 0)
            {
                _selfDestructTimer -= elapsedTime;
                if (_selfDestructTimer <= 0)
                    Destroy();

                return;
            }

            UpdateChildren(elapsedTime);

            if (Type.Owner.State == UnitState.Inactive)
            {
                Vanish();
                return;
            }
            if (Type.Owner.State == UnitState.Destroyed)
            {
                Destroy();
                return;
            }

            if (!EnsurePositionValid())
                return;

            //if (_ownDamageIndicator)
            //    _damageIndicator.Update(elapsedTime);

            var velocity = Body.Velocity;
            var direction = RotationHelpers.Direction(Body.Rotation);
            var sideDirection = new Vector2(direction.y, -direction.x);
            var sideVelocity = Vector2.Dot(velocity, sideDirection);
            Body.ApplyAcceleration(-5f * sideVelocity * elapsedTime * sideDirection);
        }

        private bool EnsurePositionValid()
        {
            if (_parent == null) return true;

            if (_parent.Body.Position.SqrDistance(Body.Position) < Body.Scale * 5 && Mathf.Abs(Mathf.DeltaAngle(_parent.Body.Rotation, Body.Rotation)) < 90f)
                return true;

            _joint.Dispose();
            Attach(_parent, _parentOffset, _offset, _maxAngle);

            Body.ApplyAcceleration(_parent.Body.Velocity - Body.Velocity);
            Body.ApplyAngularAcceleration(-Body.AngularVelocity);

            return false;
        }

        protected override void OnDispose()
        {
            _features.Dispose();
        }

        public IControls Controls { get; set; }
        public IStats Stats { get { return null; } }
        public IEngine Engine { get { return null; } }
        public IFeatures Features { get { return _features; } }
        public IShipSystems Systems { get { return null; } }
        public IShipEffects Effects { get { return null; } }
        public IShipSpecification Specification { get { return null; } }

        public int SpawnerId => 0;

        public void AddPlatform(IWeaponPlatform platform) { }
        public void AddSystem(ISystem system) { }
        public void AddEffect(IShipEffect shipEffect) { }

        public abstract void Affect(Impact impact, IUnit source);
        public abstract bool TryDealDamage(ref float damage);

        public override void Vanish()
        {
            _state = UnitState.Inactive;
        }

        public void Destroy()
        {
            _nextSegment?.Detach();
            _nextSegment = null;
            _state = UnitState.Destroyed;
            InvokeTriggers(ConditionType.OnDestroy);
        }

        protected virtual void UpdateChildren(float elapsedTime)
        {
            if (_nextSegment == null)
            {
                if (_factory == null) return;

                _timeToSpawnNextSegment -= elapsedTime;
                if (_timeToSpawnNextSegment <= 0)
                    _nextSegment = _factory.Create(Type.Owner, this);

                return;
            }

            if (_nextSegment.State == UnitState.Destroyed)
            {
                _nextSegment = null;
                _timeToSpawnNextSegment = _nextSegmentSpawnCooldown;
            }
        }

        public void Broadcast(string message, Color color) { }
        public void InvokeEvent(SystemEventType eventType, IUnit source) { }

        private float _selfDestructTimer;
        private float _timeToSpawnNextSegment;
        private IWormSegment _nextSegment;
        private IJoint _joint;
        private UnitState _state;
        private float _maxAngle;
        private float _offset;
        private float _parentOffset;
        private IUnit _parent;
        private readonly IFeatures _features;
        private readonly IWormSegmentFactory _factory;
        private readonly float _nextSegmentSpawnCooldown;

        private const float _selfDestructCooldown = 5f;
    }
}
