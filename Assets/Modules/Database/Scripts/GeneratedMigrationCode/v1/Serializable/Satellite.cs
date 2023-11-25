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
	public class SatelliteSerializable : SerializableItem
	{
		public SatelliteSerializable()
		{
			ItemType = ItemType.Satellite;
			FileName = $"{ItemType}.json";
		}

		public string Name;
		public string ModelImage;
		public float ModelScale;
		public SizeClass SizeClass;
		public string Layout;
		public BarrelSerializable[] Barrels;
	}
}
