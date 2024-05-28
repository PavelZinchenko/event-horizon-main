using System;
using Combat.Component.Collider;
using Combat.Component.Ship;
using UnityEngine;

namespace Combat.Component.Unit.Classification
{
    public class UnitType
    {
        public UnitType(UnitClass type, UnitSide side, IShip owner, bool ignoreOwnerSide = false)
        {
            Class = type;
            _side = side;
            Owner = owner;
            _ignoreOwnerSide = ignoreOwnerSide;
        }

        public readonly UnitClass Class;
        public UnitSide Side => _ignoreOwnerSide || Owner == null ? _side : Owner.Type.Side;
        public IShip Owner;

        public Layer CollisionLayer => GetCollisionLayer(Class, Side);

        public static Layer GetCollisionLayer(UnitClass unitClass, UnitSide unitSide)
        {
            Layer layer;

            switch (unitClass)
            {
                case UnitClass.SpaceObject:
                    return Layer.Default;
                case UnitClass.Ship:
                case UnitClass.Shield:
                    layer = Layer.Ship1;
                    break;
                case UnitClass.Missile:
                case UnitClass.Limb:
                    layer = Layer.Missile1;
                    break;
                case UnitClass.EnergyBolt:
                case UnitClass.AreaOfEffect:
                case UnitClass.Camera:
                case UnitClass.Loot:
                case UnitClass.BackgroundObject:
                    layer = Layer.Energy1;
                    break;
                case UnitClass.Decoy:
                case UnitClass.Drone:
                    layer = Layer.Drone1;
                    break;
                case UnitClass.Platform:
                    layer = Layer.Platform1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (unitSide)
            {
                case UnitSide.Player:
                case UnitSide.Ally:
                    break;
                case UnitSide.Enemy:
                    layer += 1;
                    break;
                case UnitSide.Neutral:
                    layer += 2;
                    break;
                case UnitSide.Undefined:
                    layer += 3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return layer;
        }

        public int CollisionMask { get { return Physics2D.GetLayerCollisionMask((int)CollisionLayer); } }

        private readonly bool _ignoreOwnerSide;
        private readonly UnitSide _side;

        public static readonly UnitType Default = new UnitType(UnitClass.SpaceObject, UnitSide.Undefined, null);
    }
}
