


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
	public partial class ShipBuild 
	{
		partial void OnDataDeserialized(ShipBuildSerializable serializable, Database.Loader loader);

		public static ShipBuild Create(ShipBuildSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new ShipBuild(serializable, loader);
		}

		private ShipBuild(ShipBuildSerializable serializable, Database.Loader loader)
		{
			Id = new ItemId<ShipBuild>(serializable.Id);
			loader.AddShipBuild(serializable.Id, this);

			Ship = loader?.GetShip(new ItemId<Ship>(serializable.ShipId)) ?? Ship.DefaultValue;
			if (loader != null && Ship == null)
			    throw new DatabaseException("ObjectTemplate.Ship cannot be null - " + serializable.ShipId);
			AvailableForPlayer = serializable.AvailableForPlayer;
			AvailableForEnemy = serializable.AvailableForEnemy;
			DifficultyClass = serializable.DifficultyClass;
			BuildFaction = loader?.GetFaction(new ItemId<Faction>(serializable.BuildFaction)) ?? Faction.DefaultValue;
			CustomAI = loader?.GetBehaviorTree(new ItemId<BehaviorTreeModel>(serializable.CustomAI)) ?? BehaviorTreeModel.DefaultValue;
			Components = new ImmutableCollection<InstalledComponent>(serializable.Components?.Select(item => InstalledComponent.Create(item, loader)));

			OnDataDeserialized(serializable, loader);
		}

		public readonly ItemId<ShipBuild> Id;

		public Ship Ship { get; private set; }
		public bool AvailableForPlayer { get; private set; }
		public bool AvailableForEnemy { get; private set; }
		public DifficultyClass DifficultyClass { get; private set; }
		public Faction BuildFaction { get; private set; }
		public BehaviorTreeModel CustomAI { get; private set; }
		public ImmutableCollection<InstalledComponent> Components { get; private set; }

		public static ShipBuild DefaultValue { get; private set; }
	}
}
