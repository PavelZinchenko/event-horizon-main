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
	public partial class Faction 
	{
		partial void OnDataDeserialized(FactionSerializable serializable, Database.Loader loader);

		public static Faction Create(FactionSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new Faction(serializable, loader);
		}

		private Faction(FactionSerializable serializable, Database.Loader loader)
		{
			Id = new ItemId<Faction>(serializable.Id);
			loader.AddFaction(serializable.Id, this);

			Name = serializable.Name;
			Color = new ColorData(serializable.Color);
			NoTerritories = serializable.NoTerritories;
			HomeStarDistance = UnityEngine.Mathf.Clamp(serializable.HomeStarDistance, 0, 5000);
			HomeStarDistanceMax = UnityEngine.Mathf.Clamp(serializable.HomeStarDistanceMax, 0, 5000);
			NoWanderingShips = serializable.NoWanderingShips;
			WanderingShipsDistance = UnityEngine.Mathf.Clamp(serializable.WanderingShipsDistance, 0, 5000);
			WanderingShipsDistanceMax = UnityEngine.Mathf.Clamp(serializable.WanderingShipsDistanceMax, 0, 5000);
			HideFromMerchants = serializable.HideFromMerchants;
			HideResearchTree = serializable.HideResearchTree;
			NoMissions = serializable.NoMissions;

			OnDataDeserialized(serializable, loader);
		}

		public readonly ItemId<Faction> Id;

		public string Name { get; private set; }
		public ColorData Color { get; private set; }
		public bool NoTerritories { get; private set; }
		public int HomeStarDistance { get; private set; }
		public int HomeStarDistanceMax { get; private set; }
		public bool NoWanderingShips { get; private set; }
		public int WanderingShipsDistance { get; private set; }
		public int WanderingShipsDistanceMax { get; private set; }
		public bool HideFromMerchants { get; private set; }
		public bool HideResearchTree { get; private set; }
		public bool NoMissions { get; private set; }

		public static Faction DefaultValue { get; private set; }
	}
}
