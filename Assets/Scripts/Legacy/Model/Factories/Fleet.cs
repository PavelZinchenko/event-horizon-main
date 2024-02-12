using System;
using System.Collections.Generic;
using System.Linq;
using Database.Legacy;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;
using GameDatabase.Query;
using Model.Military;

namespace Model
{
	namespace Factories
	{
		public static class Fleet
		{
			public static IFleet Common(int distance, int seed, IDatabase database)
			{
				var random = new Random(seed);
				var count = Maths.Distance.FleetSize(distance, random);

				var ships = ShipBuildQuery.EnemyShips(database).
					Common().
					FilterByStarDistance(distance).
					SelectRandom(count, random).
					Shuffle(random);

				return new CommonFleet(database, ships.All, distance, random.Next());
			}

			public static IFleet Boss(int distance, Faction faction, int seed, IDatabase database)
			{
				var random = new Random(seed);
				var count = Maths.Distance.FleetSize(distance, random) - 1;
				var bossClass = distance > 50 ? DifficultyClass.Class2 : DifficultyClass.Class1;

				var flagships = ShipBuildQuery.EnemyShips(database).Flagships();

				ShipBuild boss = null;
				if (faction != Faction.Empty)
					boss = flagships.BelongToFaction(faction).WithDifficulty(DifficultyClass.Class1, bossClass).Random(random);
				if (boss == null)
					boss = flagships.FilterByStarDistance(distance, ShipBuildQuery.FilterMode.Faction).
						WithDifficulty(DifficultyClass.Class1, bossClass).Random(random);

				var ships = ShipBuildQuery.EnemyShips(database).
					CommonAndRare().
					BelongToFaction(boss.Faction).
					FilterByStarDistance(distance, ShipBuildQuery.FilterMode.SizeAndDifficulty).
					SelectRandom(count, random).
					Shuffle(random).
					Prepend(boss);

				return new CommonFleet(database, ships.All, distance, random.Next());
			}

			public static IFleet FactionDefenders(GameModel.Region region, int seed, IDatabase database)
			{
				var random = new Random(seed);
				var distance = region.HomeStarLevel;
				var count = Maths.Distance.FleetSize(distance, random);
				var ships = ShipBuildQuery.EnemyShips(database).
					CommonAndRare().
					BelongToFaction(region.Faction).
					FilterByStarDistance(distance, ShipBuildQuery.FilterMode.SizeAndDifficulty).
					SelectRandom(count, random).
					Shuffle(random);

				return new CommonFleet(database, ships.All, distance, random.Next());
			}

			public static IFleet Capital(GameModel.Region region, IDatabase database)
			{
				var seed = region.HomeStar;
				var random = new Random(seed);

				var distance = region.HomeStarLevel; 

				var numberOfShips = region.BaseDefensePower/25;
				var numberOfBosses = region.BaseDefensePower/100;
				var bossClass = numberOfBosses >= 2 ? DifficultyClass.Class2 : DifficultyClass.Class1;

				var bosses = ShipBuildQuery.EnemyShips(database).
					Flagships().
					BelongToFaction(region.Faction).
					WithDifficulty(DifficultyClass.Class1, bossClass).
					SelectRandom(numberOfBosses, random);

				var ships = ShipBuildQuery.EnemyShips(database).
					CommonAndRare().
					BelongToFaction(region.Faction).
					FilterByStarDistance(distance, ShipBuildQuery.FilterMode.SizeAndDifficulty).
					SelectRandom(numberOfShips, random);

                var starbaseClass = region.HomeStarLevel < 40 ? DifficultyClass.Default : DifficultyClass.Class1;
			    var starbase = ShipBuildQuery.Starbases(database).
					BelongToFaction(region.Faction).
					WithDifficulty(starbaseClass, starbaseClass).
					Random(random);

				if (starbase == null) starbase = database.GalaxySettings.DefaultStarbaseBuild;

				var fleet = bosses.Concat(ships).Shuffle(random).Prepend(starbase);
				return new CommonFleet(database, fleet.All, distance, random.Next());
			}

            public static IFleet Ruins(int distance, int seed, IDatabase database)
            {
                var random = new Random(seed);
                var ships = ShipBuildQuery.EnemyShips(database).
					BelongToFaction(database.GalaxySettings.AbandonedStarbaseFaction).
					WithMinDifficulty(DifficultyClass.Class1).
					FilterByStarDistance(distance, ShipBuildQuery.FilterMode.Size).
                    SelectRandom(Maths.Distance.FleetSize(distance, random) * 2, random).
					Shuffle(random);

                return new CommonFleet(database, ships.All, distance, random.Next());
            }

            public static IFleet Xmas(int distance, int seed, IDatabase database)
            {
                var random = new Random(seed);

                var starbase = database.GetShipBuild(new ItemId<ShipBuild>(232));
                var hidden = ShipBuildQuery.EnemyShips(database).
					HiddenShips().
					BelongToFaction(Faction.Empty).
					WithMinDifficulty(DifficultyClass.Class1).
					FilterByStarDistance(distance*2, ShipBuildQuery.FilterMode.Size);

                var normal = ShipBuildQuery.EnemyShips(database).
					CommonAndRare().
					WithMinDifficulty(DifficultyClass.Class1).
					FilterByStarDistance(distance * 2, ShipBuildQuery.FilterMode.Size).
					SelectRandom(Maths.Distance.FleetSize(distance, random), random);

				var ships = hidden.Concat(normal).Shuffle(random).Prepend(starbase);
                return new CommonFleet(database, ships.All, distance, random.Next());
            }

            public static IFleet Arena(int distance, int seed, IDatabase database)
			{
				var random = new Random(seed);
				var ships = ShipBuildQuery.EnemyShips(database).
					FilterByStarDistance(distance, ShipBuildQuery.FilterMode.SizeAndDifficulty).
					SelectRandom(1, random);

				return new CommonFleet(database, ships.All, distance, random.Next());
			}

			public static IFleet Survival(int distance, Faction faction, int seed, IDatabase database)
			{
				const int fleetSize = 100;
				var random = new Random(seed);
				var numberOfRandomShips = fleetSize/10;
				var randomShips = ShipBuildQuery.EnemyShips(database).
					CommonAndRare().
					SelectRandom(numberOfRandomShips, random);
				var factionShips = ShipBuildQuery.EnemyShips(database).
					CommonAndRare().
					BelongToFaction(faction).
					SelectRandom(fleetSize - numberOfRandomShips, random);

				return new SurvivalFleet(database, factionShips.Concat(randomShips).All.OrderBy(item => item.Ship.Layout.CellCount + random.Next(20)), distance, random.Next());
			}

			public static IFleet Tutorial(IDatabase database)
			{
				var ships = new List<ShipBuild>();
				ships.Add(database.GetShipBuild(LegacyShipBuildNames.GetId("Invader3")));
				ships.Add(database.GetShipBuild(LegacyShipBuildNames.GetId("Invader3")));
				ships.Add(database.GetShipBuild(LegacyShipBuildNames.GetId("Invader3")));

				return new CommonFleet(database, ships, 0, 0);
			}

			public static IFleet Player(GameServices.Player.PlayerFleet fleet, IDatabase database)
			{
				return new PlayerFleet(database, fleet);
			}

			public static readonly IFleet Empty = new CommonFleet(null, Enumerable.Empty<ShipBuild>(), 0, 0);
		}
	}
}
