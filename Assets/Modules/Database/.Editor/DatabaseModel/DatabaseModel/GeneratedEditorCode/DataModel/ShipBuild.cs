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
	public partial class ShipBuild
	{
		partial void OnDataDeserialized(ShipBuildSerializable serializable, Database database);
		partial void OnDataSerialized(ref ShipBuildSerializable serializable);

		public static ShipBuild Create(ShipBuildSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new ShipBuild(serializable, database);
		}

		private ShipBuild(ShipBuildSerializable serializable, Database database)
		{
			try
			{
				Id = new ItemId<ShipBuild>(serializable.Id, serializable.FileName);
				Ship = database.GetShipId(serializable.ShipId);
				if (Ship.IsNull)
				    throw new DatabaseException(this.GetType().Name + " (" + serializable.Id + "): Ship cannot be null");
				AvailableForPlayer = serializable.AvailableForPlayer;
				AvailableForEnemy = serializable.AvailableForEnemy;
				DifficultyClass = serializable.DifficultyClass;
				BuildFaction = database.GetFactionId(serializable.BuildFaction);
				CustomAI = database.GetBehaviorTreeId(serializable.CustomAI);
				Components = serializable.Components?.Select(item => InstalledComponent.Create(item, database)).ToArray();
				Perks.Value = DataModel.ShipBuildPerks.Create(serializable.Perks, database);
				ExtendedLayout = serializable.ExtendedLayout;
				RandomColor = serializable.RandomColor;
				LeftSatelliteBuild = database.GetSatelliteBuildId(serializable.LeftSatelliteBuild);
				RightSatelliteBuild = database.GetSatelliteBuildId(serializable.RightSatelliteBuild);
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(this.GetType() + ": deserialization failed. " + serializable.FileName + " (" + serializable.Id + ")", e);
			}
			OnDataDeserialized(serializable, database);
		}

		public void Save(ShipBuildSerializable serializable)
		{
			serializable.ShipId = Ship.Value;
			serializable.AvailableForPlayer = AvailableForPlayer;
			serializable.AvailableForEnemy = AvailableForEnemy;
			serializable.DifficultyClass = DifficultyClass;
			serializable.BuildFaction = BuildFaction.Value;
			serializable.CustomAI = CustomAI.Value;
			if (Components == null || Components.Length == 0)
			    serializable.Components = null;
			else
			    serializable.Components = Components.Select(item => item.Serialize()).ToArray();
			serializable.Perks = Perks.Value?.Serialize();
			serializable.ExtendedLayout = ExtendedLayout;
			serializable.RandomColor = RandomColor;
			serializable.LeftSatelliteBuild = LeftSatelliteBuild.Value;
			serializable.RightSatelliteBuild = RightSatelliteBuild.Value;
			OnDataSerialized(ref serializable);
		}

		public readonly ItemId<ShipBuild> Id;

		public ItemId<Ship> Ship = ItemId<Ship>.Empty;
		public bool AvailableForPlayer;
		public bool AvailableForEnemy;
		public DifficultyClass DifficultyClass;
		public ItemId<Faction> BuildFaction = ItemId<Faction>.Empty;
		public ItemId<BehaviorTreeModel> CustomAI = ItemId<BehaviorTreeModel>.Empty;
		public InstalledComponent[] Components;
		public ObjectWrapper<ShipBuildPerks> Perks = new(DataModel.ShipBuildPerks.DefaultValue);
		public bool ExtendedLayout;
		public bool RandomColor;
		public ItemId<SatelliteBuild> LeftSatelliteBuild = ItemId<SatelliteBuild>.Empty;
		public ItemId<SatelliteBuild> RightSatelliteBuild = ItemId<SatelliteBuild>.Empty;

		public static ShipBuild DefaultValue { get; private set; }
	}
}
