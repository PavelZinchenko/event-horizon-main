using CombatSimulation.Core;
using GameDatabase.Enums;
using GameDatabase.Model;
using UnityEngine;

namespace Combat.Integration.Core
{
    // Reads one core ship through IShipView. The adapter copies the ship's state after every step,
    // so the view never holds a snapshot span (spans are valid only until the next step).
    public sealed class CoreShipView : IShipView
    {
        public CoreShipView(EntityId id, CombatSide side, Constructor.IShipSpecification specification, in ShipState state)
        {
            Id = id;
            Side = side;
            _specification = specification;
            _state = state;
            IsActive = true;
        }

        public EntityId Id { get; }
        public CombatSide Side { get; }
        public bool IsActive { get; private set; }

        public ResourceValue Armor => ToValue(_state.Armor);
        public ResourceValue Shield => ToValue(_state.Shield);
        public ResourceValue Energy => ToValue(_state.Energy);

        // The core has no resistances; legacy values would describe damage the core does not apply.
        public ShipResistance Resistance => default;

        public SpriteId IconImage => _specification.Stats.ShipModel.IconImage;
        public SpriteId ModelImage => _specification.Stats.ShipModel.ModelImage;
        public SizeClass SizeClass => _specification.Stats.ShipModel.SizeClass;
        public DifficultyClass Class => _specification.Info.Class;
        public int Level => _specification.Info.Level;

        public float Rotation => _state.Angle * Mathf.Rad2Deg;

        // The raw pose of the last step. Step 4 adds interpolation and the torus image nearest to the camera.
        public Vector2 VisualPosition => new Vector2(_state.Position.X, _state.Position.Y);
        public float VisualRotation => Rotation;

        public ShipState State => _state;

        public void Update(in ShipState state) => _state = state;
        public void Deactivate() => IsActive = false;

        private static ResourceValue ToValue(ResourceState resource)
        {
            var fraction = resource.Exists ? Mathf.Clamp01(resource.Value / resource.Max) : 0f;
            return new ResourceValue(resource.Exists, resource.Value, resource.Max, fraction);
        }

        private readonly Constructor.IShipSpecification _specification;
        private ShipState _state;
    }
}
