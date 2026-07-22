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
	public partial class LootModel
	{
		partial void OnDataDeserialized(LootSerializable serializable, Database database);
		partial void OnDataSerialized(ref LootSerializable serializable);

		public static LootModel Create(LootSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new LootModel(serializable, database);
		}

		private LootModel(LootSerializable serializable, Database database)
		{
			try
			{
				Id = new ItemId<LootModel>(serializable.Id, serializable.FileName);
				Loot.Value = DataModel.LootContent.Create(serializable.Loot, database);
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(this.GetType() + ": deserialization failed. " + serializable.FileName + " (" + serializable.Id + ")", e);
			}
			OnDataDeserialized(serializable, database);
		}

		public void Save(LootSerializable serializable)
		{
			serializable.Loot = Loot.Value?.Serialize();
			OnDataSerialized(ref serializable);
		}

		public readonly ItemId<LootModel> Id;

		public ObjectWrapper<LootContent> Loot = new(DataModel.LootContent.DefaultValue);

		public static LootModel DefaultValue { get; private set; }
	}
}
