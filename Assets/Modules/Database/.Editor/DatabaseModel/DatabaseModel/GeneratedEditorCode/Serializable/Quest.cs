//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using EditorDatabase.Enums;
using EditorDatabase.Model;

namespace EditorDatabase.Serializable
{
	[Serializable]
	public class QuestSerializable : SerializableItem
	{
		[DefaultValue("")]
		public string Name;
		public QuestType QuestType;
		public StartCondition StartCondition;
		public float Weight;
		public QuestOriginSerializable Origin;
		public RequirementSerializable Requirement;
		public int Level;
		public bool UseRandomSeed;
		public NodeSerializable[] Nodes;
	}
}
