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
	public partial class Faction
	{
		partial void OnDataDeserialized(FactionSerializable serializable, Database database);
		partial void OnDataSerialized(ref FactionSerializable serializable);

		public static Faction Create(FactionSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new Faction(serializable, database);
		}

		private Faction(FactionSerializable serializable, Database database)
		{
			try
			{
				Id = new ItemId<Faction>(serializable.Id, serializable.FileName);
				Name = serializable.Name;
				Color = Helpers.ColorFromString(serializable.Color);
				NoTerritories = serializable.NoTerritories;
				HomeStarDistance = new NumericValue<int>(serializable.HomeStarDistance, 0, 5000);
				HomeStarDistanceMax = new NumericValue<int>(serializable.HomeStarDistanceMax, 0, 5000);
				NoWanderingShips = serializable.NoWanderingShips;
				WanderingShipsDistance = new NumericValue<int>(serializable.WanderingShipsDistance, 0, 5000);
				WanderingShipsDistanceMax = new NumericValue<int>(serializable.WanderingShipsDistanceMax, 0, 5000);
				HideFromMerchants = serializable.HideFromMerchants;
				HideResearchTree = serializable.HideResearchTree;
				NoMissions = serializable.NoMissions;
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(this.GetType() + ": deserialization failed. " + serializable.FileName + " (" + serializable.Id + ")", e);
			}
			OnDataDeserialized(serializable, database);
		}

		public void Save(FactionSerializable serializable)
		{
			serializable.Name = Name;
			serializable.Color = Helpers.ColorToString(Color);
			serializable.NoTerritories = NoTerritories;
			serializable.HomeStarDistance = HomeStarDistance.Value;
			serializable.HomeStarDistanceMax = HomeStarDistanceMax.Value;
			serializable.NoWanderingShips = NoWanderingShips;
			serializable.WanderingShipsDistance = WanderingShipsDistance.Value;
			serializable.WanderingShipsDistanceMax = WanderingShipsDistanceMax.Value;
			serializable.HideFromMerchants = HideFromMerchants;
			serializable.HideResearchTree = HideResearchTree;
			serializable.NoMissions = NoMissions;
			OnDataSerialized(ref serializable);
		}

		public readonly ItemId<Faction> Id;

		public string Name;
		public System.Drawing.Color Color;
		public bool NoTerritories;
		public NumericValue<int> HomeStarDistance = new NumericValue<int>(0, 0, 5000);
		public NumericValue<int> HomeStarDistanceMax = new NumericValue<int>(0, 0, 5000);
		public bool NoWanderingShips;
		public NumericValue<int> WanderingShipsDistance = new NumericValue<int>(0, 0, 5000);
		public NumericValue<int> WanderingShipsDistanceMax = new NumericValue<int>(0, 0, 5000);
		public bool HideFromMerchants;
		public bool HideResearchTree;
		public bool NoMissions;

		public static Faction DefaultValue { get; private set; }
	}
}
