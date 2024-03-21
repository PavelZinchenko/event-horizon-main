using Combat.Component.Collider;
using Combat.Component.Unit;
using GameDatabase.DataModel;
using UnityEngine;

namespace Combat.Component.Controller
{
    /// <summary>
    /// Controller that directly manipulates object body via parametric functions
    /// </summary>
    public class ParametricController : IController
    {
        public ParametricController(IUnit unit, BulletController_Parametric controller)
        {
            _unit = unit;
            _controller = controller;
            _rayCollider = _unit.Collider as RayCastCollider;
            _unit.Body.ApplyAcceleration(-_unit.Body.Velocity);
            _anchored = false;
        }

        public void Dispose() { }

        public void UpdatePhysics(float elapsedTime)
        {
            _lifetime += elapsedTime;
            var body = _unit.Body;
            // Calculate initial stats on the first tick instead of during
            // construction to interact nicely when wrapped in another
            // controller (like MovingBeamController)
            if (!_anchored)
            {
                _anchorPosition = body.Position;
                _anchorRotation = body.Rotation;
                _anchorScale = body.Scale;
                _anchored = true;
                _lifetime = 0;
            }
            
            var t = _lifetime;

            var angle =  _controller.Rotation(t);
            var offset = new Vector2(_controller.X(t), _controller.Y(t));
            body.Move(_anchorPosition + RotationHelpers.Transform(offset, _anchorRotation));
            body.Turn(_anchorRotation + angle);
            body.SetSize(_anchorScale * _controller.Size(t));
            
            if (_rayCollider != null)
            {
                _rayCollider.MaxRange = _controller.Length(t);
            }
        }

        private float _lifetime;
        private Vector2 _anchorPosition;
        private float _anchorRotation;
        private float _anchorScale;
        private bool _anchored;
        private readonly BulletController_Parametric _controller;
        private readonly IUnit _unit;
        private readonly RayCastCollider _rayCollider;
    }
}
