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
	public partial class BehaviorTreeModel 
	{
		partial void OnDataDeserialized(BehaviorTreeSerializable serializable, Database.Loader loader);

		public static BehaviorTreeModel Create(BehaviorTreeSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new BehaviorTreeModel(serializable, loader);
		}

		private BehaviorTreeModel(BehaviorTreeSerializable serializable, Database.Loader loader)
		{
			Id = new ItemId<BehaviorTreeModel>(serializable.Id);
			loader.AddBehaviorTree(serializable.Id, this);

			RootNode = BehaviorTreeNode.Create(serializable.RootNode, loader);

			OnDataDeserialized(serializable, loader);
		}

		public readonly ItemId<BehaviorTreeModel> Id;

		public BehaviorTreeNode RootNode { get; private set; }

		public static BehaviorTreeModel DefaultValue { get; private set; }
	}
}
