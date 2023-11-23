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
	public class SkillSerializable : SerializableItem
	{
		public SkillSerializable()
		{
			ItemType = ItemType.Skill;
		}

		public string Name;
		public string Icon;
		public string Description;
		public float BaseRequirement;
		public float RequirementPerLevel;
		public float BasePrice;
		public float PricePerLevel;
		public int MaxLevel;
	}
}
