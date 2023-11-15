//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Linq;
using GameDatabase.Enums;
using GameDatabase.Serializable;
using GameDatabase.Model;

namespace GameDatabase.DataModel
{
	public partial class FrontierSettings
	{
		partial void OnDataDeserialized(FrontierSettingsSerializable serializable, Database.Loader loader);

		public static FrontierSettings Create(FrontierSettingsSerializable serializable, Database.Loader loader)
		{
			return new FrontierSettings(serializable, loader);
		}

		private FrontierSettings(FrontierSettingsSerializable serializable, Database.Loader loader)
		{
			BaseCommandPoints = UnityEngine.Mathf.Clamp(serializable.BaseCommandPoints, 0, 2147483647);
			MaxExtraCommandPoints = UnityEngine.Mathf.Clamp(serializable.MaxExtraCommandPoints, 0, 2147483647);
			SupporterPackShip = loader.GetShip(new ItemId<Ship>(serializable.SupporterPackShip));
			FalconPackShip = loader.GetShip(new ItemId<Ship>(serializable.FalconPackShip));
			BigBossEasyBuild = loader.GetShipBuild(new ItemId<ShipBuild>(serializable.BigBossEasyBuild));
			BigBossNormalBuild = loader.GetShipBuild(new ItemId<ShipBuild>(serializable.BigBossNormalBuild));
			BigBossHardBuild = loader.GetShipBuild(new ItemId<ShipBuild>(serializable.BigBossHardBuild));
			DemoSceneStarbaseBuild = loader.GetShipBuild(new ItemId<ShipBuild>(serializable.DemoSceneStarbaseBuild));
			TutorialStarbaseBuild = loader.GetShipBuild(new ItemId<ShipBuild>(serializable.TutorialStarbaseBuild));
			DefaultStarbaseBuild = loader.GetShipBuild(new ItemId<ShipBuild>(serializable.DefaultStarbaseBuild));
			ExplorationStarbase = loader.GetShip(new ItemId<Ship>(serializable.ExplorationStarbase));
			MerchantShipBuild = loader.GetShipBuild(new ItemId<ShipBuild>(serializable.MerchantShipBuild));
			SmugglerShipBuild = loader.GetShipBuild(new ItemId<ShipBuild>(serializable.SmugglerShipBuild));
			EngineerShipBuild = loader.GetShipBuild(new ItemId<ShipBuild>(serializable.EngineerShipBuild));
			MercenaryShipBuild = loader.GetShipBuild(new ItemId<ShipBuild>(serializable.MercenaryShipBuild));
			ShipyardShipBuild = loader.GetShipBuild(new ItemId<ShipBuild>(serializable.ShipyardShipBuild));
			SantaShipBuild = loader.GetShipBuild(new ItemId<ShipBuild>(serializable.SantaShipBuild));
			SalvageDroneBuild = loader.GetShipBuild(new ItemId<ShipBuild>(serializable.SalvageDroneBuild));
			CustomShipLevels = new ImmutableCollection<ShipToValue>(serializable.CustomShipLevels?.Select(item => ShipToValue.Create(item, loader)));
			CustomShipPrices = new ImmutableCollection<ShipToValue>(serializable.CustomShipPrices?.Select(item => ShipToValue.Create(item, loader)));
			ExplorationShips = new ImmutableCollection<Ship>(serializable.ExplorationShips?.Select(item => loader.GetShip(new ItemId<Ship>(item), true)));

			OnDataDeserialized(serializable, loader);
		}

		public int BaseCommandPoints { get; private set; }
		public int MaxExtraCommandPoints { get; private set; }
		public Ship SupporterPackShip { get; private set; }
		public Ship FalconPackShip { get; private set; }
		public ShipBuild BigBossEasyBuild { get; private set; }
		public ShipBuild BigBossNormalBuild { get; private set; }
		public ShipBuild BigBossHardBuild { get; private set; }
		public ShipBuild DemoSceneStarbaseBuild { get; private set; }
		public ShipBuild TutorialStarbaseBuild { get; private set; }
		public ShipBuild DefaultStarbaseBuild { get; private set; }
		public Ship ExplorationStarbase { get; private set; }
		public ShipBuild MerchantShipBuild { get; private set; }
		public ShipBuild SmugglerShipBuild { get; private set; }
		public ShipBuild EngineerShipBuild { get; private set; }
		public ShipBuild MercenaryShipBuild { get; private set; }
		public ShipBuild ShipyardShipBuild { get; private set; }
		public ShipBuild SantaShipBuild { get; private set; }
		public ShipBuild SalvageDroneBuild { get; private set; }
		public ImmutableCollection<ShipToValue> CustomShipLevels { get; private set; }
		public ImmutableCollection<ShipToValue> CustomShipPrices { get; private set; }
		public ImmutableCollection<Ship> ExplorationShips { get; private set; }

		public static FrontierSettings DefaultValue { get; private set; }
	}
}
