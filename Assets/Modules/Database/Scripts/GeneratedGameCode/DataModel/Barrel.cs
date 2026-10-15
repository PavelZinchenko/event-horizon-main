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
	public partial class Barrel 
	{
		partial void OnDataDeserialized(BarrelSerializable serializable, Database.Loader loader);

		public static Barrel Create(BarrelSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new Barrel(serializable, loader);
		}

		private Barrel(BarrelSerializable serializable, Database.Loader loader)
		{
			Position = serializable.Position;
			Rotation = Utilites.MathEx.Clamp(serializable.Rotation, -360f, 360f);
			Offset = Utilites.MathEx.Clamp(serializable.Offset, 0f, 1f);
			AutoAimingArc = Utilites.MathEx.Clamp(serializable.AutoAimingArc, 0f, 360f);
			RotationSpeed = Utilites.MathEx.Clamp(serializable.RotationSpeed, 0f, 1000f);
			WeaponClass = serializable.WeaponClass;
			Image = new SpriteId(serializable.Image, SpriteId.Type.Satellite);
			Size = Utilites.MathEx.Clamp(serializable.Size, 0f, 100f);

			OnDataDeserialized(serializable, loader);
		}

		public GameDatabase.Model.Vector2Data Position { get; private set; }
		public float Rotation { get; private set; }
		public float Offset { get; private set; }
		public float AutoAimingArc { get; private set; }
		public float RotationSpeed { get; private set; }
		public string WeaponClass { get; private set; }
		public SpriteId Image { get; private set; }
		public float Size { get; private set; }

		public static Barrel DefaultValue { get; private set; }= new(new(), null);
	}
}
