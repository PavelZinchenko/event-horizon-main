//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Linq;
using GameDatabase.Enums;
using GameDatabase.Serializable;
using GameDatabase.Model;

namespace GameDatabase.DataModel
{
	public partial class SatelliteMountPoint 
	{
		partial void OnDataDeserialized(SatelliteMountPointSerializable serializable, Database.Loader loader);

		public static SatelliteMountPoint Create(SatelliteMountPointSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new SatelliteMountPoint(serializable, loader);
		}

		private SatelliteMountPoint(SatelliteMountPointSerializable serializable, Database.Loader loader)
		{
			Position = serializable.Position;
			Direction = Utilites.MathEx.Clamp(serializable.Direction, -180f, 180f);
			RotationLimit = Utilites.MathEx.Clamp(serializable.RotationLimit, 0f, 180f);
			MinSizeClass = serializable.MinSizeClass;
			MaxSizeClass = serializable.MaxSizeClass;

			OnDataDeserialized(serializable, loader);
		}

		public GameDatabase.Model.Vector2Data Position { get; private set; }
		public float Direction { get; private set; }
		public float RotationLimit { get; private set; }
		public SizeClass MinSizeClass { get; private set; }
		public SizeClass MaxSizeClass { get; private set; }

		public static SatelliteMountPoint DefaultValue { get; private set; }= new(new(), null);
	}
}
