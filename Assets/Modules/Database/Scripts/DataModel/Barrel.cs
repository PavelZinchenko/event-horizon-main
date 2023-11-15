using GameDatabase.Serializable;

namespace GameDatabase.DataModel
{
    public partial class Barrel
    {
        public Barrel(BarrelSerializable serializable, Database.Loader loader, int positionInLayout)
            : this(serializable, loader)
        {
            PositionInLayout = positionInLayout;
        }

        partial void OnDataDeserialized(BarrelSerializable serializable, Database.Loader loader)
        {
            if (serializable.PlatformType > 0)
                AutoAimingArc = PlatformTypeToAngle(serializable.PlatformType);
        }

        public static float PlatformTypeToAngle(int platformType)
        {
            switch (platformType)
            {
                case 1: //AutoTarget
                    return 360;
                case 2:// AutoTargetFrontal
                    return 80;
                case 3:// TargetingUnit
                    return 20;
                default:
                    return 0;
            }
        }

        public readonly int PositionInLayout;

        public static readonly Barrel Empty = new Barrel();
        private Barrel() { }
    }
  
}
