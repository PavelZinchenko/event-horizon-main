using Combat.Collision.Manager;
using Combat.Component.Ship;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Factory;
using UnityEngine;

namespace Combat.Collision.Behaviour.Action
{
    public class TeleportAction : ICollisionAction
    {
        private const string _effectName = "FlashAdditive";
        private const float _effectLifetime = 0.5f;
        private const float _effectScale = 2f;
        private const float _effectAlphaScale = 2f;
        private readonly EffectFactory _effectFactory;
        private readonly Color _effectColor;
        private readonly float _power;

        public TeleportAction(EffectFactory effectFactory, Color color, float power)
        {
            _power = power;
            _effectColor = color;
            _effectColor.a *= _effectAlphaScale;
            _effectFactory = effectFactory;
        }

        public void Invoke(IUnit self, IUnit target, CollisionData collisionData, ref Impact selfImpact, ref Impact targetImpact)
        {
            var range = _power;// / Mathf.Sqrt(Mathf.Sqrt(target.Body.Weight));

            if (!collisionData.IsNew) return;
            if (TryInvokeSpecialAction(target, ref targetImpact)) return;
            if (!IsValidTarget(target)) return;

            var body = target.Body.Parent ?? target.Body;
            var newPosition = body.Position + RotationHelpers.Direction(body.Rotation) * range;
            SpawnEffect(body.Position, body.Scale);
            SpawnEffect(newPosition, body.Scale);
            body.Move(newPosition);
            body.ApplyAcceleration(-body.Velocity);
        }

        private bool TryInvokeSpecialAction(IUnit target, ref Impact targetImpact)
        {
            if (target.Type.Class == UnitClass.Limb)
            {
                targetImpact.Effects |= CollisionEffect.Destroy;
                return true;
            }

            return false;
        }

        private bool IsValidTarget(IUnit target)
        {
            if (target.Type.Class == UnitClass.Shield)
                target = target.Type.Owner;

            if (target.Type.Class != UnitClass.Ship && target.Type.Class != UnitClass.Drone) return false;
            if (target is not IShip ship) return false;

            var shipType = ship.Specification.Stats.ShipModel.ShipType;
            if (shipType == GameDatabase.Enums.ShipType.Starbase || shipType == GameDatabase.Enums.ShipType.Special) return false;

            return true;
        }

        private void SpawnEffect(Vector2 position, float size)
        {
            var effect = _effectFactory.CreateEffect(_effectName);
            effect.Color = _effectColor;
            effect.Position = position;
            effect.Size = size * _effectScale;
            effect.Run(_effectLifetime, Vector2.zero, 0);
        }

        public void Dispose() { }
    }
}
