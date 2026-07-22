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
	public class SatelliteSerializable : SerializableItem
	{
		[DefaultValue("")]
		public string Name;
		[DefaultValue("")]
		public string ModelImage;
		public float ModelScale;
		public SizeClass SizeClass;
		[DefaultValue("")]
		public string Layout;
		public BarrelSerializable[] Barrels;
	}
}
