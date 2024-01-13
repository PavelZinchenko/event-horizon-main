using System;
using System.Linq;
using System.Collections.Generic;
using Session.Utils;
using Session.Model;

namespace Session
{
	public class SessionSerializer
	{
		private const uint _header = 0x5AFE5AFE;
		private const int _format = 1;

		private readonly ContentFactoryObsolete _contentFactoryObsolete;
		private readonly IDataChangedCallback _callback;

		public SessionSerializer(ContentFactoryObsolete contentFactoryObsolete, IDataChangedCallback callback)
		{
			_callback = callback;
			_contentFactoryObsolete = contentFactoryObsolete;
		}

		public bool TryDeserialize(byte[] data, int startIndex, out SaveGameData content)
		{
			try
			{
				var reader = new MemoryReaderStream(data, startIndex);
				var header = reader.ReadUint();
				if (header != _header)
					return TryDeserializeOldData(data, startIndex, out content);

				var format = reader.ReadInt();
				if (format != _format)
				{
					content = null;
					return false;
				}

				var version = reader.ReadInt();

				content = new SaveGameData(new SessionDataReader(reader), _callback);
				return true;
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
				content = null;
				return false;
			}
		}

		public void Serialize(SaveGameData data, IWriterStream writer)
		{
			writer.WriteUint(_header);
			writer.WriteInt(_format);
			var version = SaveGameData.VersionMajor << 16 + SaveGameData.VersionMinor;
			writer.WriteInt(version);

			using (var sessionDataWriter = new SessionDataWriter(writer))
				data.Serialize(sessionDataWriter);
		}

		public byte[] Serialize(SaveGameData data)
		{
			using (var stream = new System.IO.MemoryStream())
			{
				Serialize(data, new WriterStream(stream));
				return stream.ToArray();
			}
		}

		private bool TryDeserializeOldData(byte[] data, int startIndex, out SaveGameData content)
		{
			try
			{
				var obsoleteData = DatabaseContentObsolete.TryDeserialize(data, startIndex, _contentFactoryObsolete);
				if (obsoleteData == null)
				{
					content = null;
					return false;
				}

				content = new SaveGameData(_callback);

				TransferAchievementData(obsoleteData.Achievements, content.Achievements);
				TransferGameData(obsoleteData.Game, content.Game);
				TransferStarMapData(obsoleteData.Starmap, content.StarMap);
				TransferInventoryData(obsoleteData.Inventory, content.Inventory);
				TransferFleetData(obsoleteData.Fleet, content.Fleet);
				TransferShopData(obsoleteData.Shop, content.Shop);
				TransferEventsData(obsoleteData.Events, content.Events);
				TransferBossData(obsoleteData.Bosses, content.Bosses);
				TransferRegionData(obsoleteData.Regions, content.Regions);
				TransferInAppPurchasesData(obsoleteData.Purchases, content.Iap);
				TransferWormholeData(obsoleteData.Wormholes, content.Wormholes);
				TransferCommonObjectData(obsoleteData.CommonObjects, content.Common);
				TransferResearchData(obsoleteData.Research, content.Research);
				TransferStatisticsData(obsoleteData.Statistics, content.Statistics);
				TransferResourcesData(obsoleteData.Resources, content.Resources);
				TransferUpgradesData(obsoleteData.Upgrades, content.Upgrades);
				TransferPvpData(obsoleteData.Pvp, content.Pvp);
				TransferSocialData(obsoleteData.Social, content.Social);
				TransferQuestData(obsoleteData.Quests, content.Quests);

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

		private void TransferStarMapData(ContentObsolete.StarMapData oldData, StarMapData newData)
		{
			newData.PlayerPosition = (uint)oldData.PlayerPosition;
			newData.MapModeZoom = oldData.MapScaleFactor;
			newData.StarModeZoom = oldData.StarScaleFactor;

			foreach (var item in oldData._bookmarks)
				newData.Bookmarks.Add((uint)item.Key, item.Value);
			foreach (var item in oldData._planetdata)
				newData.PlanetData.Add((ulong)item.Key, (uint)item.Value);
			foreach (var item in oldData._stardata)
				newData.StarData.Add((uint)item.Key, (uint)item.Value);
		}

		private void TransferInventoryData(ContentObsolete.InventoryData oldData, Inventory newData)
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

		private void TransferShopData(ContentObsolete.ShopData oldData, ShopData newData)
		{
			foreach (var purchases in oldData._purchases)
			{
				var purchasesMap = new PurchasesMap(newData);
				foreach (var item in purchases.Value)
					purchasesMap.Purchases.Add(item.Key, new PurchaseInfo(item.Value.Quantity, item.Value.Time));

				newData.Purchases.Add(purchases.Key, purchasesMap);
			}
		}

		private void TransferEventsData(ContentObsolete.EventData oldData, EventData newData)
		{
			foreach (var item in oldData._completedTime)
				newData.CompletedTime.Add(item.Key, item.Value);
		}

		private void TransferBossData(ContentObsolete.BossData oldData, BossData newData)
		{
			foreach (var item in oldData._completedTime)
				newData.Bosses.Add(item.Key, new BossInfo(item.Value.DefeatCount, item.Value.LastDefeatTime));
		}

		private void TransferRegionData(ContentObsolete.RegionData oldData, RegionData newData)
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
