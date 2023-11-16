using System;
using System.Collections.Generic;
using System.Linq;
using Database.Legacy;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Extensions;
using GameDatabase.Model;
using Model.Military;

namespace Model
{
	namespace Factories
	{
		public static class Fleet
		{
			public static IFleet Common(int distance, Faction faction, int seed, IDatabase database)
			{
				var random = new Random(seed);
				var count = Maths.Distance.FleetSize(distance, random);
				var ships = database.ShipBuildList.
					ValidForEnemy().
					CommonShips().
					LimitByStarLevel(distance, faction).
					RandomElements(count, random).OrderBy(item => random.Next());

				return new CommonFleet(database, ships, distance, random.Next());
			}

			public static IFleet Boss(int distance, Faction faction, int seed, IDatabase database)
			{
				var random = new Random(seed);
				var count = Maths.Distance.FleetSize(distance, random) - 1;
				var bossClass = distance > 50 ? DifficultyClass.Class2 : DifficultyClass.Class1;

				var boss = database.ShipBuildList.
					Flagships().
					LimitByFactionOrStarLevel(faction, distance).
					WithDifficultyClass(DifficultyClass.Class1, bossClass).
					RandomElement(random);
				
				if (boss == null)
					boss = database.ShipBuildList.
					Flagships().
					WithDifficultyClass(DifficultyClass.Class1, bossClass).
					RandomElement(random);

				var ships = database.ShipBuildList.
					ValidForEnemy().
					CommonAndRareShips().
					LimitByStarLevel(distance, faction).
					RandomElements(count, random).OrderBy(item => random.Next());

				return new CommonFleet(database, ships.Prepend(boss), distance, random.Next());
			}

		    public static IFleet SingleBoss(int distance, Faction faction, int seed, IDatabase database)
		    {
		        var random = new Random(seed);
		        var bossClass = distance < 50 ? DifficultyClass.Default : distance < 150 ? DifficultyClass.Class1 : DifficultyClass.Class2;
				var boss = database.ShipBuildList.
					Flagships().
					LimitByFactionOrStarLevel(faction, distance).
					WithDifficultyClass(bossClass, bossClass).
					RandomElements(1, random);

		        return new CommonFleet(database, boss, distance, random.Next());
		    }

			public static IFleet Faction(GameModel.Region region, int seed, IDatabase database)
			{
				var random = new Random(seed);
				var distance = region.MilitaryPower;
				var count = Maths.Distance.FleetSize(distance, random);
				var ships = database.ShipBuildList.
					ValidForEnemy().
					CommonAndRareShips().
					LimitByStarLevel(distance, region.Faction).
					RandomElements(count, random);

				return new CommonFleet(database, ships, distance, random.Next());
			}

			public static IFleet Capital(GameModel.Region region, IDatabase database)
			{
				var seed = region.HomeStar;
				var random = new Random(seed);

				var distance = region.MilitaryPower; 

				var numberOfShips = (int)Math.Round(4*region.BaseDefensePower);
				var numberOfBosses = (int)Math.Floor(region.BaseDefensePower);
				var bossClass = numberOfBosses >= 2 ? DifficultyClass.Class2 : DifficultyClass.Class1;

				var bosses = database.ShipBuildList.
					Flagships().
					BelongToFaction(region.Faction).
					WithDifficultyClass(DifficultyClass.Class1, bossClass).
					RandomElements(numberOfBosses, random);

				var ships = database.ShipBuildList.
					ValidForEnemy().
					CommonAndRareShips().
					BelongToFaction(region.Faction).
					LimitClassByStarLevel(distance).
					LimitSizeByStarLevel(distance).
					RandomElements(numberOfShips, random);

                var starbaseClass = region.MilitaryPower < 40 ? DifficultyClass.Default : DifficultyClass.Class1;
			    var starbase = database.ShipBuildList.
					Starbases().
					BelongToFaction(region.Faction).
					WithBestAvailableClass(starbaseClass).
					FirstOrDefault();

				if (starbase == null) starbase = database.GalaxySettings.DefaultStarbaseBuild;

                var fleet = (starbase == null ? Enumerable.Empty<ShipBuild>() : Enumerable.Repeat(starbase, 1)).Concat((bosses.Concat(ships).OrderBy(item => random.Next())));
				return new CommonFleet(database, fleet, distance, random.Next());
			}

            public static IFleet Ruins(int distance, int seed, IDatabase database)
            {
                var random = new Random(seed);
                var ships = database.ShipBuildList.
					ValidForEnemy().
					BelongToFaction(database.GalaxySettings.AbandonedStarbaseFaction).
					Hardcore().
					LimitSizeByStarLevel(distance).
                    RandomElements(Maths.Distance.FleetSize(distance, random) * 2, random);

                return new CommonFleet(database, ships, distance, random.Next());
            }

            public static IFleet Xmas(int distance, int seed, IDatabase database)
            {
                var random = new Random(seed);

                var starbase = database.GetShipBuild(new ItemId<ShipBuild>(232));
                var hidden = database.ShipBuildList.
					ValidForEnemy().
					HiddenShips().
					BelongToFaction(GameDatabase.DataModel.Faction.Neutral).
					Hardcore().
					LimitSizeByStarLevel(distance*2);

                var normal = database.ShipBuildList.
					ValidForEnemy().
					CommonAndRareShips().
					Hardcore().
					LimitSizeByStarLevel(distance);

                var ships = starbase.ToEnumerable().Concat(hidden.Concat(normal.RandomElements(Maths.Distance.FleetSize(distance, random), random)).OrderBy(item => random.Next()));
                return new CommonFleet(database, ships, distance, random.Next());
            }

            public static IFleet Arena(int distance, int seed, IDatabase database)
			{
				var random = new Random(seed);
				var ships = database.ShipBuildList.
					ValidForEnemy().
					LimitClassByStarLevel(distance).
					LimitSizeByStarLevel(distance).
					RandomUniqueElements(1, random);

				return new CommonFleet(database, ships, distance, random.Next());
			}

			public static IFleet Survival(int distance, Faction faction, int seed, IDatabase database)
			{
				const int fleetSize = 100;
				var random = new Random(seed);
				var numberOfRandomShips = fleetSize/10;
				var randomShips = database.ShipBuildList.
					ValidForEnemy().
					CommonAndRareShips().
					RandomElements(numberOfRandomShips, random);
				var factionShips = database.ShipBuildList.
					ValidForEnemy().
					CommonAndRareShips().
					LimitByFactionOrStarLevel(faction,distance).
					RandomElements(fleetSize - numberOfRandomShips, random);
				return new SurvivalFleet(database, factionShips.Concat(randomShips).OrderBy(item => item.Ship.Layout.CellCount + random.Next(20)), distance, random.Next());
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
