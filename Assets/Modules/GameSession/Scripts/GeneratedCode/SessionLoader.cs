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
		/// property was removed - oldData.StarMap.StarData;
		/// property was added - newData.StarMap.DiscoveredStars;
		/// property was added - newData.StarMap.SecuredStars;
		/// property was added - newData.StarMap.EnemiesOnStars;
		/// </summary>
		partial void Upgrage_v1_0_to_v2_0(v1.SaveGameData oldData, Model.SaveGameData newData);

		public Model.SaveGameData Convert(v1.SaveGameData data)
		{
			var data2 = Convert_1_2(data);
			return data2;
		}

		public Model.SaveGameData Load(SessionDataReader reader, int versionMajor, int versionMinor)
		{
			v1.SaveGameData data1 = null;
			if (versionMajor == 1)
				data1 = new v1.SaveGameData(reader, null);

			Model.SaveGameData data2 = null;
			if (versionMajor == 2)
				data2 = new Model.SaveGameData(reader, null);
			else if (versionMajor == 1)
			{
				data2 = Convert_1_2(data1);
				versionMajor = 2;
				versionMinor = 0;
			}

			return data2;
		}


		private Model.SaveGameData Convert_1_2(v1.SaveGameData oldData)
		{
			var newData = new Model.SaveGameData(null);
			newData.Game = oldData.Game;
			newData.Achievements = oldData.Achievements;
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
	}
}
