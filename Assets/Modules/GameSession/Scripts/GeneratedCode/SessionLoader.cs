//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using Session.Utils;

namespace Session
{
	public partial class SessionLoader
	{
		/// <summary>
		/// These items must be transferred manually:
		/// copy oldData.Bosses.Bosses to newData.Bosses.Bosses;
		/// type was changed - oldData.Events.CompletedTime;
		/// type was changed - oldData.Inventory.Components;
		/// property was removed - oldData.Regions.DefeatedFleetCount;
		/// property was added - newData.Regions.MilitaryPower;
		/// property was added - newData.Regions.CapturedBases;
		/// copy oldData.Shop.Purchases to newData.Shop.Purchases;
		/// property was removed - oldData.StarMap.StarData;
		/// property was added - newData.StarMap.DiscoveredStars;
		/// property was added - newData.StarMap.SecuredStars;
		/// property was added - newData.StarMap.EnemiesOnStars;
		/// </summary>
		partial void Upgrage_v1_0_to_v2_0(v1.SaveGameData oldData, v2.SaveGameData newData);
		/// <summary>
		/// These items must be transferred manually:
		/// property was added - newData.ShipPresets;
		/// </summary>
		partial void Upgrage_v2_0_to_v3_0(v2.SaveGameData oldData, Model.SaveGameData newData);

		public Model.SaveGameData Convert(v1.SaveGameData data)
		{
			var data2 = Convert_1_2(data);
			var data3 = Convert_2_3(data2);
			return data3;
		}

		public Model.SaveGameData Load(SessionDataReader reader, int versionMajor, int versionMinor)
		{
			v1.SaveGameData data1 = null;
			if (versionMajor == 1)
				data1 = new v1.SaveGameData(reader, null);

			v2.SaveGameData data2 = null;
			if (versionMajor == 2)
				data2 = new v2.SaveGameData(reader, null);
			else if (versionMajor == 1)
			{
				data2 = Convert_1_2(data1);
				versionMajor = 2;
				versionMinor = 0;
			}

			Model.SaveGameData data3 = null;
			if (versionMajor == 3)
				data3 = new Model.SaveGameData(reader, null);
			else if (versionMajor == 2)
			{
				data3 = Convert_2_3(data2);
				versionMajor = 3;
				versionMinor = 0;
			}

			return data3;
		}


		private v2.SaveGameData Convert_1_2(v1.SaveGameData oldData)
		{
			var newData = new v2.SaveGameData(null);
			newData.Game = oldData.Game;
			newData.Achievements = oldData.Achievements;
			newData.Common = oldData.Common;
			newData.Fleet = oldData.Fleet;
			newData.Iap = oldData.Iap;
			newData.Pvp = oldData.Pvp;
			newData.Inventory.Satellites.Assign(oldData.Inventory.Satellites);
			newData.Quests = oldData.Quests;
			newData.Regions.Factions.Assign(oldData.Regions.Factions);
			newData.Research = oldData.Research;
			newData.Resources = oldData.Resources;
			newData.Social = oldData.Social;
			newData.StarMap.PlayerPosition = oldData.StarMap.PlayerPosition;
			newData.StarMap.MapModeZoom = oldData.StarMap.MapModeZoom;
			newData.StarMap.StarModeZoom = oldData.StarMap.StarModeZoom;
			newData.StarMap.PlanetData.Assign(oldData.StarMap.PlanetData);
			newData.StarMap.Bookmarks.Assign(oldData.StarMap.Bookmarks);
			newData.Statistics = oldData.Statistics;
			newData.Upgrades = oldData.Upgrades;
			newData.Wormholes = oldData.Wormholes;
			Upgrage_v1_0_to_v2_0(oldData, newData);
			return newData;
		}

		private Model.SaveGameData Convert_2_3(v2.SaveGameData oldData)
		{
			var newData = new Model.SaveGameData(null);
			newData.Game = oldData.Game;
			newData.Achievements = oldData.Achievements;
			newData.Bosses = oldData.Bosses;
			newData.Common = oldData.Common;
			newData.Events = oldData.Events;
			newData.Fleet = oldData.Fleet;
			newData.Iap = oldData.Iap;
			newData.Pvp = oldData.Pvp;
			newData.Inventory = oldData.Inventory;
			newData.Quests = oldData.Quests;
			newData.Regions = oldData.Regions;
			newData.Research = oldData.Research;
			newData.Resources = oldData.Resources;
			newData.Shop = oldData.Shop;
			newData.Social = oldData.Social;
			newData.StarMap = oldData.StarMap;
			newData.Statistics = oldData.Statistics;
			newData.Upgrades = oldData.Upgrades;
			newData.Wormholes = oldData.Wormholes;
			Upgrage_v2_0_to_v3_0(oldData, newData);
			return newData;
		}
	}
}
