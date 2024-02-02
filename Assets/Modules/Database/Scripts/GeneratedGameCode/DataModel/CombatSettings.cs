


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
	public partial class CombatSettings 
	{
		partial void OnDataDeserialized(CombatSettingsSerializable serializable, Database.Loader loader);

		public static CombatSettings Create(CombatSettingsSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new CombatSettings(serializable, loader);
		}

		private CombatSettings(CombatSettingsSerializable serializable, Database.Loader loader)
		{
			EnemyAI = loader?.GetBehaviorTree(new ItemId<BehaviorTreeModel>(serializable.EnemyAI)) ?? BehaviorTreeModel.DefaultValue;
			AutopilotAI = loader?.GetBehaviorTree(new ItemId<BehaviorTreeModel>(serializable.AutopilotAI)) ?? BehaviorTreeModel.DefaultValue;
			CloneAI = loader?.GetBehaviorTree(new ItemId<BehaviorTreeModel>(serializable.CloneAI)) ?? BehaviorTreeModel.DefaultValue;
			DefensiveDroneAI = loader?.GetBehaviorTree(new ItemId<BehaviorTreeModel>(serializable.DefensiveDroneAI)) ?? BehaviorTreeModel.DefaultValue;
			OffensiveDroneAI = loader?.GetBehaviorTree(new ItemId<BehaviorTreeModel>(serializable.OffensiveDroneAI)) ?? BehaviorTreeModel.DefaultValue;
			StarbaseAI = loader?.GetBehaviorTree(new ItemId<BehaviorTreeModel>(serializable.StarbaseAI)) ?? BehaviorTreeModel.DefaultValue;

			OnDataDeserialized(serializable, loader);
		}

		public BehaviorTreeModel EnemyAI { get; private set; }
		public BehaviorTreeModel AutopilotAI { get; private set; }
		public BehaviorTreeModel CloneAI { get; private set; }
		public BehaviorTreeModel DefensiveDroneAI { get; private set; }
		public BehaviorTreeModel OffensiveDroneAI { get; private set; }
		public BehaviorTreeModel StarbaseAI { get; private set; }

		public static CombatSettings DefaultValue { get; private set; }
	}
}
