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
	public partial class Character
	{
		partial void OnDataDeserialized(CharacterSerializable serializable, Database database);
		partial void OnDataSerialized(ref CharacterSerializable serializable);

		public static Character Create(CharacterSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new Character(serializable, database);
		}

		private Character(CharacterSerializable serializable, Database database)
		{
			try
			{
				Id = new ItemId<Character>(serializable.Id, serializable.FileName);
				Name = serializable.Name;
				AvatarIcon = serializable.AvatarIcon;
				Faction = database.GetFactionId(serializable.Faction);
				Inventory = database.GetLootId(serializable.Inventory);
				Fleet = database.GetFleetId(serializable.Fleet);
				Relations = new NumericValue<int>(serializable.Relations, -100, 100);
				IsUnique = serializable.IsUnique;
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(this.GetType() + ": deserialization failed. " + serializable.FileName + " (" + serializable.Id + ")", e);
			}
			OnDataDeserialized(serializable, database);
		}

		public void Save(CharacterSerializable serializable)
		{
			serializable.Name = Name;
			serializable.AvatarIcon = AvatarIcon;
			serializable.Faction = Faction.Value;
			serializable.Inventory = Inventory.Value;
			serializable.Fleet = Fleet.Value;
			serializable.Relations = Relations.Value;
			serializable.IsUnique = IsUnique;
			OnDataSerialized(ref serializable);
		}

		public readonly ItemId<Character> Id;

		public string Name;
		public string AvatarIcon;
		public ItemId<Faction> Faction = ItemId<Faction>.Empty;
		public ItemId<LootModel> Inventory = ItemId<LootModel>.Empty;
		public ItemId<Fleet> Fleet = ItemId<Fleet>.Empty;
		public NumericValue<int> Relations = new NumericValue<int>(0, -100, 100);
		public bool IsUnique;

		public static Character DefaultValue { get; private set; }
	}
}
