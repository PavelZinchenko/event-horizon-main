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
	public class QuestOriginSerializable
	{
		public QuestOriginType Type;
		public FactionFilterSerializable Factions;
		public int MinDistance;
		public int MaxDistance;
		public int MinRelations;
		public int MaxRelations;
	}
}
