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

		public void SetWeaponClass(string value)
		{
			if (!_thisIsCopy) 
				throw new System.InvalidOperationException();

			WeaponClass = value;
		}

		public void SetAutoAimingArc(float value)
		{
			if (!_thisIsCopy)
				throw new System.InvalidOperationException();

			AutoAimingArc = value;
		}

		public static Barrel Clone(Barrel barrel)
		{
			return new Barrel()
			{

				Position = barrel.Position,
				Rotation = barrel.Rotation,
				Offset = barrel.Offset,
				AutoAimingArc = barrel.AutoAimingArc,
				RotationSpeed = barrel.RotationSpeed,
				WeaponClass = barrel.WeaponClass,
				Image = barrel.Image,
				Size = barrel.Size,
			};
		}

		private readonly bool _thisIsCopy;
		public readonly int PositionInLayout;

        public static readonly Barrel Empty = new Barrel();
        private Barrel() => _thisIsCopy = true;
    }
}
