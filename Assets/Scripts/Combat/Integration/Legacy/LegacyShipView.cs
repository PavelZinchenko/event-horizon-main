using System.Runtime.CompilerServices;
using Combat.Component.Ship;
using Combat.Component.Unit.Classification;
using Combat.Unit;
using Combat.Unit.HitPoints;
using GameDatabase.Enums;
using GameDatabase.Model;
using UnityEngine;

namespace Combat.Integration.Legacy
{
    // Reads a legacy ship through IShipView. Values come from the ship on every read.
    public sealed class LegacyShipView : IShipView
    {
        public LegacyShipView(IShip ship, EntityId id)
        {
            _ship = ship;
            Id = id;
        }

        // Temporary entry for scenes without a combat adapter (exploration) until phase 6.
        // Returns the same view for the same ship, so panels can detect an unchanged target.
        public static LegacyShipView Of(IShip ship)
        {
            return ship == null ? null : Detached.GetValue(ship, item => new LegacyShipView(item, EntityId.None));
        }

        public EntityId Id { get; }
        public CombatSide Side => _ship.Type.Side.IsAlly(UnitSide.Player) ? CombatSide.Player : CombatSide.Enemy;
        public bool IsActive => _ship.IsActive();

        public ResourceValue Armor => ToValue(_ship.Stats.Armor);
        public ResourceValue Shield => ToValue(_ship.Stats.Shield);
        public ResourceValue Energy => ToValue(_ship.Stats.Energy);

        public ShipResistance Resistance
        {
            get
            {
                var resistance = _ship.Stats.Resistance;
                return new ShipResistance(resistance.Heat, resistance.Energy, resistance.Kinetic);
            }
        }

        public SpriteId IconImage => _ship.Specification.Stats.ShipModel.IconImage;
        public SpriteId ModelImage => _ship.Specification.Stats.ShipModel.ModelImage;
        public SizeClass SizeClass => _ship.Specification.Stats.ShipModel.SizeClass;
        public DifficultyClass Class => _ship.Specification.Info.Class;
        public int Level => _ship.Specification.Info.Level;

        public float Rotation => _ship.Body.Rotation;

        public Vector2 VisualPosition => _ship.Body.VisualPosition;
        public float VisualRotation => _ship.Body.VisualRotation;

        private static ResourceValue ToValue(IResourcePoints points)
        {
            return new ResourceValue(points.Exists, points.Value, points.MaxValue, points.Percentage);
        }

        private readonly IShip _ship;

        private static readonly ConditionalWeakTable<IShip, LegacyShipView> Detached = new ConditionalWeakTable<IShip, LegacyShipView>();
    }
}
