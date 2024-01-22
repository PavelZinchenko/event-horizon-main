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
			return serializable == null ? DefaultValue : new FrontierSettings(serializable, loader);
		}

		private FrontierSettings(FrontierSettingsSerializable serializable, Database.Loader loader)
		{
			BaseCommandPoints = UnityEngine.Mathf.Clamp(serializable.BaseCommandPoints, 0, 2147483647);
			MaxExtraCommandPoints = UnityEngine.Mathf.Clamp(serializable.MaxExtraCommandPoints, 0, 2147483647);
			SupporterPackShip = loader?.GetShip(new ItemId<Ship>(serializable.SupporterPackShip)) ?? Ship.DefaultValue;
			FalconPackShip = loader?.GetShip(new ItemId<Ship>(serializable.FalconPackShip)) ?? Ship.DefaultValue;
			BigBossEasyBuild = loader?.GetShipBuild(new ItemId<ShipBuild>(serializable.BigBossEasyBuild)) ?? ShipBuild.DefaultValue;
			BigBossNormalBuild = loader?.GetShipBuild(new ItemId<ShipBuild>(serializable.BigBossNormalBuild)) ?? ShipBuild.DefaultValue;
			BigBossHardBuild = loader?.GetShipBuild(new ItemId<ShipBuild>(serializable.BigBossHardBuild)) ?? ShipBuild.DefaultValue;
			DemoSceneStarbaseBuild = loader?.GetShipBuild(new ItemId<ShipBuild>(serializable.DemoSceneStarbaseBuild)) ?? ShipBuild.DefaultValue;
			TutorialStarbaseBuild = loader?.GetShipBuild(new ItemId<ShipBuild>(serializable.TutorialStarbaseBuild)) ?? ShipBuild.DefaultValue;
			DefaultStarbaseBuild = loader?.GetShipBuild(new ItemId<ShipBuild>(serializable.DefaultStarbaseBuild)) ?? ShipBuild.DefaultValue;
			ExplorationStarbase = loader?.GetShip(new ItemId<Ship>(serializable.ExplorationStarbase)) ?? Ship.DefaultValue;
			MerchantShipBuild = loader?.GetShipBuild(new ItemId<ShipBuild>(serializable.MerchantShipBuild)) ?? ShipBuild.DefaultValue;
			SmugglerShipBuild = loader?.GetShipBuild(new ItemId<ShipBuild>(serializable.SmugglerShipBuild)) ?? ShipBuild.DefaultValue;
			EngineerShipBuild = loader?.GetShipBuild(new ItemId<ShipBuild>(serializable.EngineerShipBuild)) ?? ShipBuild.DefaultValue;
			MercenaryShipBuild = loader?.GetShipBuild(new ItemId<ShipBuild>(serializable.MercenaryShipBuild)) ?? ShipBuild.DefaultValue;
			ShipyardShipBuild = loader?.GetShipBuild(new ItemId<ShipBuild>(serializable.ShipyardShipBuild)) ?? ShipBuild.DefaultValue;
			SantaShipBuild = loader?.GetShipBuild(new ItemId<ShipBuild>(serializable.SantaShipBuild)) ?? ShipBuild.DefaultValue;
			SalvageDroneBuild = loader?.GetShipBuild(new ItemId<ShipBuild>(serializable.SalvageDroneBuild)) ?? ShipBuild.DefaultValue;
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
