//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Linq;
using EditorDatabase.Enums;
using EditorDatabase.Serializable;
using EditorDatabase.Model;

namespace EditorDatabase.DataModel
{
	public partial class Barrel
	{
		partial void OnDataDeserialized(BarrelSerializable serializable, Database database);
		partial void OnDataSerialized(ref BarrelSerializable serializable);

		public static Barrel Create(BarrelSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new Barrel(serializable, database);
		}

		public Barrel() {}

		private Barrel(BarrelSerializable serializable, Database database)
		{
			Position = serializable.Position;
			Rotation = new NumericValue<float>(serializable.Rotation, -360f, 360f);
			Offset = new NumericValue<float>(serializable.Offset, 0f, 1f);
			AutoAimingArc = new NumericValue<float>(serializable.AutoAimingArc, 0f, 360f);
			RotationSpeed = new NumericValue<float>(serializable.RotationSpeed, 0f, 1000f);
			WeaponClass = serializable.WeaponClass;
			Image = serializable.Image;
			Size = new NumericValue<float>(serializable.Size, 0f, 100f);
			OnDataDeserialized(serializable, database);
		}

		public BarrelSerializable Serialize()
		{
			var serializable = new BarrelSerializable();
			serializable.Position = Position;
			serializable.Rotation = Rotation.Value;
			serializable.Offset = Offset.Value;
			serializable.AutoAimingArc = AutoAimingArc.Value;
			serializable.RotationSpeed = RotationSpeed.Value;
			serializable.WeaponClass = WeaponClass;
			serializable.Image = Image;
			serializable.Size = Size.Value;
			OnDataSerialized(ref serializable);
			return serializable;
		}

		public Vector2 Position;
		public NumericValue<float> Rotation = new NumericValue<float>(0, -360f, 360f);
		public NumericValue<float> Offset = new NumericValue<float>(0, 0f, 1f);
		public NumericValue<float> AutoAimingArc = new NumericValue<float>(0, 0f, 360f);
		public NumericValue<float> RotationSpeed = new NumericValue<float>(0, 0f, 1000f);
		public string WeaponClass;
		public string Image;
		public NumericValue<float> Size = new NumericValue<float>(0, 0f, 100f);

		public static Barrel DefaultValue { get; private set; }
	}
}
