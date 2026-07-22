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
	public class StatUpgradeTemplateSerializable : SerializableItem
	{
		[DefaultValue(20)]
		public int MaxLevel = 20;
		[DefaultValue("")]
		public string Stars;
		[DefaultValue("")]
		public string Credits;
		[DefaultValue("")]
		public string Resources;
	}
}
