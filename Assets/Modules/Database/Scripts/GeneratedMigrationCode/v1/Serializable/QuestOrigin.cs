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
	public struct QuestOriginSerializable
	{
		public QuestOriginType Type;
		public FactionFilterSerializable Factions;
		public int MinDistance;
		public int MaxDistance;
		public int MinRelations;
		public int MaxRelations;
	}
}
