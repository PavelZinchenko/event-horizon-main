using System;
using System.Linq;
using System.Collections.Generic;
using Session.Model;

namespace Session
{
	public class LegacyDataLoader
	{
		private readonly ContentFactoryObsolete _contentFactory;

		public LegacyDataLoader(ContentFactoryObsolete contentFactory) => _contentFactory = contentFactory;

		public bool TryDeserializeOldData(byte[] data, int startIndex, out SaveGameData content)
		{
			try
			{
				var obsoleteData = DatabaseContentObsolete.TryDeserialize(data, startIndex, _contentFactory);
				if (obsoleteData == null)
				{
					content = null;
					return false;
				}

				var temp = new v1.SaveGameData(null);
				TransferAchievementData(obsoleteData.Achievements, temp.Achievements);
				TransferGameData(obsoleteData.Game, temp.Game);
				TransferStarMapData(obsoleteData.Starmap, temp.StarMap);
				TransferInventoryData(obsoleteData.Inventory, temp.Inventory);
				TransferFleetData(obsoleteData.Fleet, temp.Fleet);
				TransferShopData(obsoleteData.Shop, temp.Shop);
				TransferEventsData(obsoleteData.Events, temp.Events);
				TransferBossData(obsoleteData.Bosses, temp.Bosses);
				TransferRegionData(obsoleteData.Regions, temp.Regions);
				TransferInAppPurchasesData(obsoleteData.Purchases, temp.Iap);
				TransferWormholeData(obsoleteData.Wormholes, temp.Wormholes);
				TransferCommonObjectData(obsoleteData.CommonObjects, temp.Common);
				TransferResearchData(obsoleteData.Research, temp.Research);
				TransferStatisticsData(obsoleteData.Statistics, temp.Statistics);
				TransferResourcesData(obsoleteData.Resources, temp.Resources);
				TransferUpgradesData(obsoleteData.Upgrades, temp.Upgrades);
				TransferPvpData(obsoleteData.Pvp, temp.Pvp);
				TransferSocialData(obsoleteData.Social, temp.Social);
				TransferQuestData(obsoleteData.Quests, temp.Quests);

				content = new SessionLoader().Convert(temp);
				return true;
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
				content = null;
				return false;
			}
		}

		private void TransferAchievementData(ContentObsolete.AchievementData oldData, Achievements newData)
		{
			foreach (var id in oldData._unlockedAchievements)
				newData.Gained.Add(id);
		}

		private void TransferGameData(ContentObsolete.GameData oldData, GameData newData)
		{
			newData.Counter = oldData.Counter;
			newData.Seed = oldData.Seed;
			newData.StartTime = oldData.GameStartTime;
			newData.SupplyShipStartTime = oldData.SupplyShipStartTime;
			newData.TotalPlayTime = oldData.TotalPlayTime;
		}

		private void TransferStarMapData(ContentObsolete.StarMapData oldData, v1.StarMapData newData)
		{
			newData.PlayerPosition = oldData.PlayerPosition;
			newData.MapModeZoom = oldData.MapScaleFactor;
			newData.StarModeZoom = oldData.StarScaleFactor;

			foreach (var item in oldData._bookmarks)
				newData.Bookmarks.Add(item.Key, item.Value);
			foreach (var item in oldData._planetdata)
				newData.PlanetData.Add(item.Key, item.Value);
			foreach (var item in oldData._stardata)
				newData.StarData.Add(item.Key, item.Value);
		}

		private void TransferInventoryData(ContentObsolete.InventoryData oldData, v1.Inventory newData)
		{
			foreach (var item in oldData._components.Items)
				newData.Components.Add(item.Key, item.Value);
			foreach (var item in oldData._satellites.Items)
				newData.Satellites.Add(item.Key, item.Value);
		}

		private void TransferFleetData(ContentObsolete.FleetData oldData, FleetData newData)
		{
			foreach (var item in oldData._hangarSlots)
				newData.HangarSlots.Add(new HangarSlotInfo(item.Index, item.ShipId));
			foreach (var oldShip in oldData._ships)
			{
				var components = TransferComponentsData(oldShip.Components);
				var satellite1 = new SatelliteInfo(oldShip.Satellite1.Id, TransferComponentsData(oldShip.Satellite1.Components), newData);
				var satellite2 = new SatelliteInfo(oldShip.Satellite2.Id, TransferComponentsData(oldShip.Satellite2.Components), newData);

				newData.Ships.Add(new ShipInfo(oldShip.Id, oldShip.Name, oldShip.ColorScheme, oldShip.Experience, components,
					oldShip.Modifications.Layout, oldShip.Modifications.Modifications, satellite1, satellite2, newData));
			}
		}

		private void TransferShopData(ContentObsolete.ShopData oldData, v1.ShopData newData)
		{
			foreach (var purchases in oldData._purchases)
			{
				var purchasesMap = new v1.PurchasesMap(newData);
				foreach (var item in purchases.Value)
					purchasesMap.Purchases.Add(item.Key, new v1.PurchaseInfo(item.Value.Quantity, item.Value.Time));

				newData.Purchases.Add(purchases.Key, purchasesMap);
			}
		}

		private void TransferEventsData(ContentObsolete.EventData oldData, v1.EventData newData)
		{
			foreach (var item in oldData._completedTime)
				newData.CompletedTime.Add(item.Key, item.Value);
		}

		private void TransferBossData(ContentObsolete.BossData oldData, v1.BossData newData)
		{
			foreach (var item in oldData._completedTime)
				newData.Bosses.Add(item.Key, new v1.BossInfo(item.Value.DefeatCount, item.Value.LastDefeatTime));
		}

		private void TransferRegionData(ContentObsolete.RegionData oldData, v1.RegionData newData)
		{
			foreach (var item in oldData._defeatedFleetCount)
				newData.DefeatedFleetCount.Add(item.Key, item.Value);
			foreach (var item in oldData._factions)
				newData.Factions.Add(item.Key, item.Value);
		}

		private void TransferInAppPurchasesData(ContentObsolete.InAppPurchasesData oldData, IapData newData)
		{
			newData.RemoveAds = oldData.RemoveAds;
			newData.SupporterPack = oldData.SupporterPack;
			newData.Stars = oldData.PurchasedStars;
		}

		private void TransferWormholeData(ContentObsolete.WormholeData oldData, WormholeData newData)
		{
			foreach (var item in oldData._routes)
				newData.Routes.Add(item.Key, item.Value);
		}

		private void TransferCommonObjectData(ContentObsolete.CommonObjectData oldData, CommonObjectData newData)
		{
			foreach (var item in oldData._intValues)
				newData.IntValues.Add(item.Key, item.Value);
			foreach (var item in oldData._usedTime)
				newData.LongValues.Add(item.Key, item.Value);
		}

		private void TransferResearchData(ContentObsolete.ResearchData oldData, ResearchData newData)
		{
			foreach (var item in oldData._technologies)
				newData.Technologies.Add(item);
			foreach (var item in oldData._researchPoints)
				newData.ResearchPoints.Add(item.Key, item.Value);
		}

		private void TransferStatisticsData(ContentObsolete.StatisticsData oldData, Statistics newData)
		{
			newData.DefeatedEnemies = oldData.DefeatedEnemies;
			foreach (var item in oldData._unlockedShips)
				newData.UnlockedShips.Add(item);
		}

		private void TransferResourcesData(ContentObsolete.ResourcesData oldData, ResourcesData newData)
		{
			newData.Money = oldData.Money;
			newData.Stars = oldData.Stars;
			newData.Tokens = oldData.Tokens;
			newData.Fuel = oldData.Fuel;

			foreach (var item in oldData._resources.Items)
				newData.Resources.Add(item.Key, item.Value);
		}

		private void TransferUpgradesData(ContentObsolete.UpgradesData oldData, UpgradesData newData)
		{
			newData.ResetCounter = oldData.ResetCounter;
			newData.PlayerExperience = oldData.PlayerExperience;

			foreach (var item in oldData.Skills)
				newData.Skills.Add(item);
		}

		private void TransferPvpData(ContentObsolete.PvpData oldData, PvpData newData)
		{
			newData.ArenaFightsFromTimerStart = oldData.FightsFromTimerStart;
			newData.ArenaLastFightTime = oldData.LastFightTime;
			newData.ArenaTimerStartTime = oldData.TimerStartTime;
		}

		private void TransferSocialData(ContentObsolete.SocialData oldData, SocialData newData)
		{
			newData.FirstDailyRewardDate = oldData.FirstDailyRewardDate;
			newData.LastDailyRewardDate = oldData.LastDailyRewardDate;
		}

		private void TransferQuestData(ContentObsolete.QuestData oldData, QuestData newData)
		{
			foreach (var item in oldData._factionRelations)
				newData.FactionRelations.Add(item.Key, item.Value);
			foreach (var item in oldData._characterRelations)
				newData.CharacterRelations.Add(item.Key, item.Value);

			foreach (var item in oldData._statistics)
				newData.Statistics.Add(item.Key, new QuestStatistics(
					item.Value.CompletionCount, item.Value.FailureCount, item.Value.LastStartTime, item.Value.CompletionCount));

			foreach (var progress in oldData._progress)
			{
				var progressMap = new StarQuestMap(newData);
				foreach (var item in progress.Value)
					progressMap.StarQuestsMap.Add(item.Key, new QuestProgress(item.Value.Seed, item.Value.ActiveNode, item.Value.StartTime));

				newData.Progress.Add(progress.Key, progressMap);
			}
		}

		private static IEnumerable<ShipComponentInfo> TransferComponentsData(ContentObsolete.ShipComponentsData oldData)
		{
			if (oldData.Components == null)
				return Enumerable.Empty<ShipComponentInfo>();

			return oldData.Components.Select(item => new ShipComponentInfo(
				item.Id, item.Quality, item.Modification, item.UpgradeLevel, item.X, item.Y,
				item.BarrelId, item.KeyBinding, item.Behaviour, item.Locked));
		}
	}
}
