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
	public partial class QuestModel
	{
		partial void OnDataDeserialized(QuestSerializable serializable, Database database);
		partial void OnDataSerialized(ref QuestSerializable serializable);

		public static QuestModel Create(QuestSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new QuestModel(serializable, database);
		}

		private QuestModel(QuestSerializable serializable, Database database)
		{
			try
			{
				Id = new ItemId<QuestModel>(serializable.Id, serializable.FileName);
				Name = serializable.Name;
				QuestType = serializable.QuestType;
				StartCondition = serializable.StartCondition;
				Weight = new NumericValue<float>(serializable.Weight, 0f, 1000f);
				Origin.Value = DataModel.QuestOrigin.Create(serializable.Origin, database);
				Requirement.Value = DataModel.Requirement.Create(serializable.Requirement, database);
				Level = new NumericValue<int>(serializable.Level, 0, 1000);
				UseRandomSeed = serializable.UseRandomSeed;
				Nodes = serializable.Nodes?.Select(item => Node.Create(item, database)).ToArray();
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(this.GetType() + ": deserialization failed. " + serializable.FileName + " (" + serializable.Id + ")", e);
			}
			OnDataDeserialized(serializable, database);
		}

		public void Save(QuestSerializable serializable)
		{
			serializable.Name = Name;
			serializable.QuestType = QuestType;
			serializable.StartCondition = StartCondition;
			serializable.Weight = Weight.Value;
			serializable.Origin = Origin.Value?.Serialize();
			serializable.Requirement = Requirement.Value?.Serialize();
			serializable.Level = Level.Value;
			serializable.UseRandomSeed = UseRandomSeed;
			if (Nodes == null || Nodes.Length == 0)
			    serializable.Nodes = null;
			else
			    serializable.Nodes = Nodes.Select(item => item.Serialize()).ToArray();
			OnDataSerialized(ref serializable);
		}

		public readonly ItemId<QuestModel> Id;

		public string Name;
		public QuestType QuestType;
		public StartCondition StartCondition;
		public NumericValue<float> Weight = new NumericValue<float>(0, 0f, 1000f);
		public ObjectWrapper<QuestOrigin> Origin = new(DataModel.QuestOrigin.DefaultValue);
		public ObjectWrapper<Requirement> Requirement = new(DataModel.Requirement.DefaultValue);
		public NumericValue<int> Level = new NumericValue<int>(0, 0, 1000);
		public bool UseRandomSeed;
		public Node[] Nodes;

		public static QuestModel DefaultValue { get; private set; }
	}
}
