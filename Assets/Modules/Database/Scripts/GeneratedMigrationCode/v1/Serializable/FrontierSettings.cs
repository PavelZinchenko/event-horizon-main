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
	public class FrontierSettingsSerializable : SerializableItem
	{
		public FrontierSettingsSerializable()
		{
			ItemType = ItemType.FrontierSettings;
			FileName = $"{ItemType}.json";
		}

		public int BaseCommandPoints;
		public int MaxExtraCommandPoints;
		public int SupporterPackShip;
		public int FalconPackShip;
		public int BigBossEasyBuild;
		public int BigBossNormalBuild;
		public int BigBossHardBuild;
		public int DemoSceneStarbaseBuild;
		public int TutorialStarbaseBuild;
		public int DefaultStarbaseBuild;
		public int ExplorationStarbase;
		public int MerchantShipBuild;
		public int SmugglerShipBuild;
		public int EngineerShipBuild;
		public int MercenaryShipBuild;
		public int ShipyardShipBuild;
		public int SantaShipBuild;
		public int SalvageDroneBuild;
		public ShipToValueSerializable[] CustomShipLevels;
		public ShipToValueSerializable[] CustomShipPrices;
		public int[] ExplorationShips;
	}
}
