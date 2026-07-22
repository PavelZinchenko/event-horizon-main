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
	public partial class NodeAction
	{
		partial void OnDataDeserialized(NodeActionSerializable serializable, Database database);
		partial void OnDataSerialized(ref NodeActionSerializable serializable);

		public static NodeAction Create(NodeActionSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new NodeAction(serializable, database);
		}

		public NodeAction() {}

		private NodeAction(NodeActionSerializable serializable, Database database)
		{
			TargetNode = new NumericValue<int>(serializable.TargetNode, 1, 999999);
			Requirement.Value = DataModel.Requirement.Create(serializable.Requirement, database);
			ButtonText = serializable.ButtonText;
			OnDataDeserialized(serializable, database);
		}

		public NodeActionSerializable Serialize()
		{
			var serializable = new NodeActionSerializable();
			serializable.TargetNode = TargetNode.Value;
			serializable.Requirement = Requirement.Value?.Serialize();
			serializable.ButtonText = ButtonText;
			OnDataSerialized(ref serializable);
			return serializable;
		}

		public NumericValue<int> TargetNode = new NumericValue<int>(0, 1, 999999);
		public ObjectWrapper<Requirement> Requirement = new(DataModel.Requirement.DefaultValue);
		public string ButtonText;

		public static NodeAction DefaultValue { get; private set; }
	}
}
