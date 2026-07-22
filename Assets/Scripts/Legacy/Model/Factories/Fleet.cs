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
                // 星舰地球 has a dedicated class-2 station. The generic query
                // filters stations to class 0/1 and therefore silently fell back
                // to the original default station. Bind its faction explicitly.
			    var starbase = region.Faction.Id.Value == 21
                    ? database.GetShipBuild(new ItemId<ShipBuild>(94000))
                    : ShipBuildQuery.Starbases(database).
					    BelongToFaction(region.Faction).
					    WithDifficulty(starbaseClass, starbaseClass).
					    Random(random);

				if (starbase == null) starbase = database.GalaxySettings.DefaultStarbaseBuild;

				// The two SizeClass-6 Titans are capital-defense ships.  A single
				// one joins only an assault on its own faction's starbase; ordinary
				// fleets, roaming encounters, quick battles, and defense missions
				// must not add them.
				var titan = GetStationAssaultTitan(region.Faction, random, database);
				var defenders = titan == null
					? bosses.Concat(ships)
					: bosses.Concat(ships).Append(titan);
				var fleet = defenders.Shuffle(random).Prepend(starbase);
				return new CommonFleet(database, fleet.All, distance, random.Next());
			}

            public static IFleet StarbaseDefenseAllies(GameModel.Region region, int seed, IDatabase database)
            {
                var random = new Random(seed);
                var stationLevel = UnityEngine.Mathf.Max(1, region.BaseDefendersLevel);
                var builds = new List<ShipBuild>();
                AddDefenseClass(builds, region.Faction, SizeClass.Battleship, 3, stationLevel, random, database);
                AddDefenseClass(builds, region.Faction, SizeClass.Cruiser, 5, stationLevel, random, database);
                AddDefenseClass(builds, region.Faction, SizeClass.Destroyer, 10, stationLevel, random, database);
                AddDefenseClass(builds, region.Faction, SizeClass.Frigate, 20, stationLevel, random, database);
                return new CommonFleet(database, builds.OrderBy(_ => random.Next()), stationLevel, random.Next(),
                    Maths.Distance.AiLevel(stationLevel));
            }

            public static IFleet StarbaseDefenseEnemies(GameModel.Region region, int seed, int excludedFactionId,
                IDatabase database, out int selectedFactionId)
            {
                var random = new Random(seed);
                var stationLevel = UnityEngine.Mathf.Max(1, region.BaseDefendersLevel);
                var level = UnityEngine.Mathf.Max(1, UnityEngine.Mathf.RoundToInt(stationLevel * 1.5f));
                var allAvailable = ShipBuildQuery.EnemyShips(database)
                    .Where(item => item.Faction != region.Faction)
                    .Where(item => item.Ship.ModelImage.Id != "worm_head" && item.Ship.ModelImage.Id != "worm_head2")
                    .All.ToList();
                var factions = allAvailable.Select(item => item.Faction).Distinct().ToList();
                var differentFactions = factions.Where(item => item.Id.Value != excludedFactionId).ToList();
                if (differentFactions.Count > 0)
                    factions = differentFactions;

                selectedFactionId = -1;
                var available = new List<ShipBuild>();
                if (factions.Count > 0)
                {
                    var faction = factions[random.Next(factions.Count)];
                    selectedFactionId = faction.Id.Value;
                    available = ShipBuildQuery.EnemyShips(database)
                        .BelongToFaction(faction)
                        .FilterByStarDistance(level, ShipBuildQuery.FilterMode.SizeAndDifficulty)
                        .Where(item => item.Ship.ModelImage.Id != "worm_head" && item.Ship.ModelImage.Id != "worm_head2")
                        .All.ToList();
                    if (available.Count == 0)
                        available = allAvailable.Where(item => item.Faction == faction).ToList();
                }

                var builds = new List<ShipBuild>();
                for (var i = 0; i < 100 && available.Count > 0; i++)
                    builds.Add(available[random.Next(available.Count)]);
                return new CommonFleet(database, builds.OrderBy(_ => random.Next()), level, random.Next(), Maths.Distance.AiLevel(level));
            }

            public static ShipBuild StarbaseForFaction(GameModel.Region region, IDatabase database)
            {
                if (region.Faction.Id.Value == 21)
                    return database.GetShipBuild(new ItemId<ShipBuild>(94000));

                var starbaseClass = region.HomeStarLevel < 40 ? DifficultyClass.Default : DifficultyClass.Class1;
                return ShipBuildQuery.Starbases(database)
                           .BelongToFaction(region.Faction)
                           .WithDifficulty(starbaseClass, starbaseClass)
                           .Random(new Random(region.HomeStar))
                       ?? database.GalaxySettings.DefaultStarbaseBuild;
            }

            private static void AddDefenseClass(List<ShipBuild> output, Faction faction, SizeClass sizeClass,
                int count, int level, Random random, IDatabase database)
            {
                var candidates = GetDefenseCandidates(faction, sizeClass, level, database).ToList();
                if (candidates.Count == 0)
                    candidates = GetDefenseCandidates(faction, sizeClass, 0, database, false).ToList();

                for (var i = 0; i < count && candidates.Count > 0; i++)
                    output.Add(candidates[random.Next(candidates.Count)]);
            }

            private static IEnumerable<ShipBuild> GetDefenseCandidates(Faction faction, SizeClass sizeClass, int level,
                IDatabase database, bool filterByDistance = true)
            {
                IEnumerable<ShipBuild> builds = ShipBuildQuery.EnemyShips(database)
                    .BelongToFaction(faction)
                    .WithSizeClass(sizeClass, sizeClass)
                    .All;

                if (filterByDistance)
                {
                    var max = ShipBuildQuery.StarLevelToMaxDifficulty(level);
                    var min = ShipBuildQuery.StarLevelToMinDifficulty(level);
                    builds = builds.Where(item => item.DifficultyClass >= min && item.DifficultyClass <= max);
                }

                return builds;
            }

            private static ShipBuild GetStationAssaultTitan(Faction faction, Random random, IDatabase database)
            {
                if (faction.Id.Value != 21 && faction.Id.Value != 22)
                    return null;

                var candidates = database.ShipBuildList
                    .Where(item => item.AvailableForEnemy &&
                                   item.Ship.ShipType == ShipType.Flagship &&
                                   item.Ship.SizeClass == SizeClass.TitanP &&
                                   item.Faction == faction)
                    .ToList();
                return candidates.Count > 0 ? candidates[random.Next(candidates.Count)] : null;
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

			public static IFleet StarshipEarthAllies(int distance, int seed, IDatabase database)
			{
				var faction = database.GetFaction(new ItemId<Faction>(21));
				var ships = ShipBuildQuery.EnemyShips(database).
					CommonAndRare().
					BelongToFaction(faction).
					SelectRandom(5, new Random(seed));
				return new CommonFleet(database, ships.All, distance, seed, Maths.Distance.AiLevel(distance));
			}

			public static readonly IFleet Empty = new CommonFleet(null, Enumerable.Empty<ShipBuild>(), 0, 0);
		}
	}
}
