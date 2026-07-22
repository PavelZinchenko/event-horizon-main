using Constructor.Ships;

namespace ShipEditor.Context
{
    public interface IShipDataProvider
    {
        bool TryGet(IShip ship, out ShipGameObjectData position);
        ShipGameObjectData Default { get; }
    }

    public readonly struct ShipGameObjectData
    {
        public ShipGameObjectData(UnityEngine.Vector2 position, float rotation, float size, bool hasImage)
        {
            Position = position;
            Rotation = rotation;
            Size = size;
            HasImage = hasImage;
        }

        public readonly bool HasImage;
        public readonly float Size;
        public readonly float Rotation;
        public readonly UnityEngine.Vector2 Position;
    }

    public class EmptyDataProvider : IShipDataProvider
    {
        public bool TryGet(IShip ship, out ShipGameObjectData position)
        {
            position = Default;
            return false; 
        }

        public ShipGameObjectData Default { get; }
    }
}
