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
	public partial class Satellite
	{
		partial void OnDataDeserialized(SatelliteSerializable serializable, Database database);
		partial void OnDataSerialized(ref SatelliteSerializable serializable);

		public static Satellite Create(SatelliteSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new Satellite(serializable, database);
		}

		private Satellite(SatelliteSerializable serializable, Database database)
		{
			try
			{
				Id = new ItemId<Satellite>(serializable.Id, serializable.FileName);
				Name = serializable.Name;
				ModelImage = serializable.ModelImage;
				ModelScale = new NumericValue<float>(serializable.ModelScale, 0.1f, 100f);
				SizeClass = serializable.SizeClass;
				Layout = new Layout(serializable.Layout);
				Barrels = serializable.Barrels?.Select(item => Barrel.Create(item, database)).ToArray();
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(this.GetType() + ": deserialization failed. " + serializable.FileName + " (" + serializable.Id + ")", e);
			}
			OnDataDeserialized(serializable, database);
		}

		public void Save(SatelliteSerializable serializable)
		{
			serializable.Name = Name;
			serializable.ModelImage = ModelImage;
			serializable.ModelScale = ModelScale.Value;
			serializable.SizeClass = SizeClass;
			serializable.Layout = Layout.Data;
			if (Barrels == null || Barrels.Length == 0)
			    serializable.Barrels = null;
			else
			    serializable.Barrels = Barrels.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public readonly ItemId<Satellite> Id;

		public string Name;
		public string ModelImage;
		public NumericValue<float> ModelScale = new NumericValue<float>(0, 0.1f, 100f);
		public SizeClass SizeClass;
		public Layout Layout;
		public Barrel[] Barrels;

		public static Satellite DefaultValue { get; private set; }
	}
}
