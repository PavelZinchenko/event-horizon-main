//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using GameDatabase.Model;
using DatabaseMigration.v1.Enums;

namespace DatabaseMigration.v1.Serializable
{
	[Serializable]
	public class CharacterSerializable : SerializableItem
	{
		public CharacterSerializable()
		{
			ItemType = ItemType.Character;
			FileName = $"{ItemType}.json";
		}

		public string Name;
		public string AvatarIcon;
		public int Faction;
		public int Inventory;
		public int Fleet;
		public int Relations;
		public bool IsUnique;
	}
}
