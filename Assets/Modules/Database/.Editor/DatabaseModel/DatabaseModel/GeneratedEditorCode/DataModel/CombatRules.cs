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
	public partial class CombatRules
	{
		partial void OnDataDeserialized(CombatRulesSerializable serializable, Database database);
		partial void OnDataSerialized(ref CombatRulesSerializable serializable);

		public static CombatRules Create(CombatRulesSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new CombatRules(serializable, database);
		}

		private CombatRules(CombatRulesSerializable serializable, Database database)
		{
			try
			{
				Id = new ItemId<CombatRules>(serializable.Id, serializable.FileName);
				InitialEnemyShips = serializable.InitialEnemyShips;
				MaxEnemyShips = serializable.MaxEnemyShips;
				BattleMapSize = new NumericValue<int>(serializable.BattleMapSize, 50, 2147483647);
				TimeLimit = serializable.TimeLimit;
				TimeOutMode = serializable.TimeOutMode;
				LootCondition = serializable.LootCondition;
				ExpCondition = serializable.ExpCondition;
				ShipSelection = serializable.ShipSelection;
				DisableSkillBonuses = serializable.DisableSkillBonuses;
				DisableRandomLoot = serializable.DisableRandomLoot;
				DisableAsteroids = serializable.DisableAsteroids;
				DisablePlanet = serializable.DisablePlanet;
				NextEnemyButton = serializable.NextEnemyButton;
				KillThemAllButton = serializable.KillThemAllButton;
				CustomSoundtrack = serializable.CustomSoundtrack?.Select(item => SoundTrack.Create(item, database)).ToArray();
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(this.GetType() + ": deserialization failed. " + serializable.FileName + " (" + serializable.Id + ")", e);
			}
			OnDataDeserialized(serializable, database);
		}

		public void Save(CombatRulesSerializable serializable)
		{
			serializable.InitialEnemyShips = InitialEnemyShips;
			serializable.MaxEnemyShips = MaxEnemyShips;
			serializable.BattleMapSize = BattleMapSize.Value;
			serializable.TimeLimit = TimeLimit;
			serializable.TimeOutMode = TimeOutMode;
			serializable.LootCondition = LootCondition;
			serializable.ExpCondition = ExpCondition;
			serializable.ShipSelection = ShipSelection;
			serializable.DisableSkillBonuses = DisableSkillBonuses;
			serializable.DisableRandomLoot = DisableRandomLoot;
			serializable.DisableAsteroids = DisableAsteroids;
			serializable.DisablePlanet = DisablePlanet;
			serializable.NextEnemyButton = NextEnemyButton;
			serializable.KillThemAllButton = KillThemAllButton;
			if (CustomSoundtrack == null || CustomSoundtrack.Length == 0)
			    serializable.CustomSoundtrack = null;
			else
			    serializable.CustomSoundtrack = CustomSoundtrack.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public readonly ItemId<CombatRules> Id;

		public string InitialEnemyShips;
		public string MaxEnemyShips;
		public NumericValue<int> BattleMapSize = new NumericValue<int>(0, 50, 2147483647);
		public string TimeLimit;
		public TimeOutMode TimeOutMode;
		public RewardCondition LootCondition;
		public RewardCondition ExpCondition;
		public PlayerShipSelectionMode ShipSelection;
		public bool DisableSkillBonuses;
		public bool DisableRandomLoot;
		public bool DisableAsteroids;
		public bool DisablePlanet;
		public bool NextEnemyButton;
		[TooltipText("For debug purposes")]
		public bool KillThemAllButton;
		public SoundTrack[] CustomSoundtrack;

		public static CombatRules DefaultValue { get; private set; }
	}
}
