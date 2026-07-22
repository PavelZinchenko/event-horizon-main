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
	public partial class Fleet
	{
		partial void OnDataDeserialized(FleetSerializable serializable, Database database);
		partial void OnDataSerialized(ref FleetSerializable serializable);

		public static Fleet Create(FleetSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new Fleet(serializable, database);
		}

		private Fleet(FleetSerializable serializable, Database database)
		{
			try
			{
				Id = new ItemId<Fleet>(serializable.Id, serializable.FileName);
				Factions.Value = DataModel.RequiredFactions.Create(serializable.Factions, database);
				LevelBonus = new NumericValue<int>(serializable.LevelBonus, -10000, 10000);
				NoRandomShips = serializable.NoRandomShips;
				SpecificShips = serializable.SpecificShips?.Select(id => new Wrapper<ShipBuild> { Item = database.GetShipBuildId(id) }).ToArray();
				CombatRules = database.GetCombatRulesId(serializable.CombatRules);
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(this.GetType() + ": deserialization failed. " + serializable.FileName + " (" + serializable.Id + ")", e);
			}
			OnDataDeserialized(serializable, database);
		}

		public void Save(FleetSerializable serializable)
		{
			serializable.Factions = Factions.Value?.Serialize();
			serializable.LevelBonus = LevelBonus.Value;
			serializable.NoRandomShips = NoRandomShips;
			if (SpecificShips == null || SpecificShips.Length == 0)
			    serializable.SpecificShips = null;
			else
			    serializable.SpecificShips = SpecificShips.Select(wrapper => wrapper.Item.Value).ToArray();
			serializable.CombatRules = CombatRules.Value;
			OnDataSerialized(ref serializable);
		}

		public readonly ItemId<Fleet> Id;

		public ObjectWrapper<RequiredFactions> Factions = new(DataModel.RequiredFactions.DefaultValue);
		public NumericValue<int> LevelBonus = new NumericValue<int>(0, -10000, 10000);
		public bool NoRandomShips;
		public Wrapper<ShipBuild>[] SpecificShips;
		public ItemId<CombatRules> CombatRules = ItemId<CombatRules>.Empty;

		public static Fleet DefaultValue { get; private set; }
	}
}
