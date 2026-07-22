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
	public partial class QuestItem
	{
		partial void OnDataDeserialized(QuestItemSerializable serializable, Database database);
		partial void OnDataSerialized(ref QuestItemSerializable serializable);

		public static QuestItem Create(QuestItemSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new QuestItem(serializable, database);
		}

		private QuestItem(QuestItemSerializable serializable, Database database)
		{
			try
			{
				Id = new ItemId<QuestItem>(serializable.Id, serializable.FileName);
				Name = serializable.Name;
				Description = serializable.Description;
				Icon = serializable.Icon;
				Color = Helpers.ColorFromString(serializable.Color);
				Price = new NumericValue<int>(serializable.Price, 0, 999999999);
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(this.GetType() + ": deserialization failed. " + serializable.FileName + " (" + serializable.Id + ")", e);
			}
			OnDataDeserialized(serializable, database);
		}

		public void Save(QuestItemSerializable serializable)
		{
			serializable.Name = Name;
			serializable.Description = Description;
			serializable.Icon = Icon;
			serializable.Color = Helpers.ColorToString(Color);
			serializable.Price = Price.Value;
			OnDataSerialized(ref serializable);
		}

		public readonly ItemId<QuestItem> Id;

		public string Name;
		public string Description;
		public string Icon;
		public System.Drawing.Color Color;
		public NumericValue<int> Price = new NumericValue<int>(0, 0, 999999999);

		public static QuestItem DefaultValue { get; private set; }
	}
}
