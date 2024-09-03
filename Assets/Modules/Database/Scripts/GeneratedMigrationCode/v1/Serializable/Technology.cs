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
	public class TechnologySerializable : SerializableItem
	{
		public TechnologySerializable()
		{
			ItemType = ItemType.Technology;
			FileName = "Technology.json";
		}

		public TechType Type;
		public int ItemId;
		public int Faction;
		public int Price;
		public bool Hidden;
		public bool Special;
		public int[] Dependencies;
		public int CustomCraftingLevel;
	}
}
