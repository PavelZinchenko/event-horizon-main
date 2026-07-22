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
	public partial class BulletPrefab
	{
		partial void OnDataDeserialized(BulletPrefabSerializable serializable, Database database);
		partial void OnDataSerialized(ref BulletPrefabSerializable serializable);

		public static BulletPrefab Create(BulletPrefabSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new BulletPrefab(serializable, database);
		}

		private BulletPrefab(BulletPrefabSerializable serializable, Database database)
		{
			try
			{
				Id = new ItemId<BulletPrefab>(serializable.Id, serializable.FileName);
				Shape = serializable.Shape;
				Image = serializable.Image;
				Size = new NumericValue<float>(serializable.Size, 0.01f, 100f);
				Margins = new NumericValue<float>(serializable.Margins, 0f, 1f);
				Deformation = new NumericValue<float>(serializable.Deformation, -100f, 100f);
				MainColor = Helpers.ColorFromString(serializable.MainColor);
				MainColorMode = serializable.MainColorMode;
				SecondColor = Helpers.ColorFromString(serializable.SecondColor);
				SecondColorMode = serializable.SecondColorMode;
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(this.GetType() + ": deserialization failed. " + serializable.FileName + " (" + serializable.Id + ")", e);
			}
			OnDataDeserialized(serializable, database);
		}

		public void Save(BulletPrefabSerializable serializable)
		{
			serializable.Shape = Shape;
			serializable.Image = Image;
			serializable.Size = Size.Value;
			serializable.Margins = Margins.Value;
			serializable.Deformation = Deformation.Value;
			serializable.MainColor = Helpers.ColorToString(MainColor);
			serializable.MainColorMode = MainColorMode;
			serializable.SecondColor = Helpers.ColorToString(SecondColor);
			serializable.SecondColorMode = SecondColorMode;
			OnDataSerialized(ref serializable);
		}

		public readonly ItemId<BulletPrefab> Id;

		public BulletShape Shape;
		public string Image;
		public NumericValue<float> Size = new NumericValue<float>(0, 0.01f, 100f);
		public NumericValue<float> Margins = new NumericValue<float>(0, 0f, 1f);
		public NumericValue<float> Deformation = new NumericValue<float>(0, -100f, 100f);
		public System.Drawing.Color MainColor;
		public ColorMode MainColorMode;
		public System.Drawing.Color SecondColor;
		public ColorMode SecondColorMode;

		public static BulletPrefab DefaultValue { get; private set; }
	}
}
