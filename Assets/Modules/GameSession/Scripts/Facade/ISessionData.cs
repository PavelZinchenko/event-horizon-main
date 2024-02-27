using Session.Content;

namespace Session
{
	public interface ISessionDataContent {}

	public interface ISessionContent
	{
		IGameData Game { get; }
		IStarMapData StarMap { get; }
		IInventoryData Inventory { get; }
		IFleetData Fleet { get; }
		IShopData Shop { get; }
		IEventData Events { get; }
		IBossData Bosses { get; }
		IRegionData Regions { get; }
		IIapData Purchases { get; }
		IPvpData Pvp { get; }
		IWormholeData Wormholes { get; }
		IAchievementData Achievements { get; }
		ICommonObjectData CommonObjects { get; }
		IResearchData Research { get; }
		IStatisticsData Statistics { get; }
		IResourcesData Resources { get; }
		IUpgradesData Upgrades { get; }
		ISocialData Social { get; }
		IQuestData Quests { get; }
        IShipPresetsData ShipPresets { get; }
    }

	public interface ISessionData : ISessionContent, Services.Storage.IGameData { }

	public static class SessionDataExtensions
	{
		public static bool IsGameStarted(this ISessionContent session)
		{
			return session != null && session.Game != null && session.Game.GameStartTime > 0;
		}
	}
}
