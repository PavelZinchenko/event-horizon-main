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
	public class ComponentRestrictionsSerializable
	{
		public SizeClass[] ShipSizes;
		public bool NotForOrganicShips;
		public bool NotForMechanicShips;
		public int MaxComponentAmount;
		public int ComponentGroupTag;
		[DefaultValue("")]
		public string UniqueComponentTag;
	}
}
