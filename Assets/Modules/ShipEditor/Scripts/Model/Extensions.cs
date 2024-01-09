namespace ShipEditor.Model
{
	public static class CellIndex
	{
		public static ulong FromXY(int x, int y) => ((ulong)y << 32) + (uint)x;
		
		public static void GetXY(ulong index, out int x, out int y)
		{
			y = (int)(index >> 32);
			x = (int)(index & 0xFFFFFFFF);
		}
	}

	public static class ShipElementTypeExtensions
	{
		public static SatelliteLocation ToSatelliteLocation(this ShipElementType type)
		{
			switch (type)
			{
				case ShipElementType.SatelliteL:
					return SatelliteLocation.Left;
				case ShipElementType.SatelliteR:
					return SatelliteLocation.Right;
				default:
					throw new System.InvalidOperationException();
			}
		}

		public static ShipElementType ToShipElement(this SatelliteLocation type)
		{
			switch (type)
			{
				case SatelliteLocation.Left: 
					return ShipElementType.SatelliteL;
				case SatelliteLocation.Right: 
					return ShipElementType.SatelliteR;
				default:
					throw new System.InvalidOperationException();
			}
		}
	}
}
