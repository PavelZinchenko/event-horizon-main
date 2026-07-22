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
	public partial class Ship
	{
		partial void OnDataDeserialized(ShipSerializable serializable, Database database);
		partial void OnDataSerialized(ref ShipSerializable serializable);

		public static Ship Create(ShipSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new Ship(serializable, database);
		}

		private Ship(ShipSerializable serializable, Database database)
		{
			try
			{
				Id = new ItemId<Ship>(serializable.Id, serializable.FileName);
				ShipType = serializable.ShipType;
				ShipRarity = serializable.ShipRarity;
				SizeClass = serializable.SizeClass;
				Name = serializable.Name;
				Description = serializable.Description;
				Faction = database.GetFactionId(serializable.Faction);
				IconImage = serializable.IconImage;
				IconScale = new NumericValue<float>(serializable.IconScale, 0.1f, 100f);
				ModelImage = serializable.ModelImage;
				ModelScale = new NumericValue<float>(serializable.ModelScale, 0.1f, 100f);
				EngineColor = Helpers.ColorFromString(serializable.EngineColor);
				Engines = serializable.Engines?.Select(item => Engine.Create(item, database)).ToArray();
				Layout = new Layout(serializable.Layout);
				Barrels = serializable.Barrels?.Select(item => Barrel.Create(item, database)).ToArray();
				Features.Value = DataModel.ShipFeatures.Create(serializable.Features, database);
				VisualEffects.Value = DataModel.ShipVisualEffects.Create(serializable.VisualEffects, database);
				CellsExpansions = serializable.CellsExpansions;
				ColliderTolerance = new NumericValue<float>(serializable.ColliderTolerance, 0f, 1f);
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(this.GetType() + ": deserialization failed. " + serializable.FileName + " (" + serializable.Id + ")", e);
			}
			OnDataDeserialized(serializable, database);
		}

		public void Save(ShipSerializable serializable)
		{
			serializable.ShipType = ShipType;
			serializable.ShipRarity = ShipRarity;
			serializable.SizeClass = SizeClass;
			serializable.Name = Name;
			serializable.Description = Description;
			serializable.Faction = Faction.Value;
			serializable.IconImage = IconImage;
			serializable.IconScale = IconScale.Value;
			serializable.ModelImage = ModelImage;
			serializable.ModelScale = ModelScale.Value;
			serializable.EngineColor = Helpers.ColorToString(EngineColor);
			if (Engines == null || Engines.Length == 0)
			    serializable.Engines = null;
			else
			    serializable.Engines = Engines.Select(item => item.Serialize()).ToArray();
			serializable.Layout = Layout.Data;
			if (Barrels == null || Barrels.Length == 0)
			    serializable.Barrels = null;
			else
			    serializable.Barrels = Barrels.Select(item => item.Serialize()).ToArray();
			serializable.Features = Features.Value?.Serialize();
			serializable.VisualEffects = VisualEffects.Value?.Serialize();
			serializable.CellsExpansions = CellsExpansions;
			serializable.ColliderTolerance = ColliderTolerance.Value;
			OnDataSerialized(ref serializable);
		}

		public readonly ItemId<Ship> Id;

		public ShipType ShipType;
		public ShipRarity ShipRarity;
		public SizeClass SizeClass;
		public string Name;
		public string Description;
		public ItemId<Faction> Faction = ItemId<Faction>.Empty;
		public string IconImage;
		public NumericValue<float> IconScale = new NumericValue<float>(0, 0.1f, 100f);
		public string ModelImage;
		public NumericValue<float> ModelScale = new NumericValue<float>(0, 0.1f, 100f);
		public System.Drawing.Color EngineColor;
		public Engine[] Engines;
		public Layout Layout;
		public Barrel[] Barrels;
		public ObjectWrapper<ShipFeatures> Features = new(DataModel.ShipFeatures.DefaultValue);
		public ObjectWrapper<ShipVisualEffects> VisualEffects = new(DataModel.ShipVisualEffects.DefaultValue);
		public ToggleState CellsExpansions;
		public NumericValue<float> ColliderTolerance = new NumericValue<float>(0, 0f, 1f);

		public static Ship DefaultValue { get; private set; }
	}
}
