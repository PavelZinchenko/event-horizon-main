using GameDatabase.Enums;

namespace GameDatabase.DataModel
{
    public partial class SatelliteMountPoint
    {
        public void SetSizeClass(SizeClass sizeClass)
        {
            if (!_thisIsCopy)
                throw new System.InvalidOperationException();

            SizeClassModified = true;
            MaxSizeClass = sizeClass;
        }

        public bool SizeClassModified { get; private set; }

        public static SatelliteMountPoint Clone(SatelliteMountPoint mountPoint)
        {
            return new SatelliteMountPoint()
            {

                Position = mountPoint.Position,
                Direction = mountPoint.Direction,
		        RotationLimit = mountPoint.RotationLimit,
                MinSizeClass = mountPoint.MinSizeClass,
		        MaxSizeClass  = mountPoint.MaxSizeClass,
            };
        }

        private readonly bool _thisIsCopy;

        private SatelliteMountPoint() => _thisIsCopy = true;
    }
}
