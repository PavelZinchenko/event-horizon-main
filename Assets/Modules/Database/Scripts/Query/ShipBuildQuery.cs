using System.Linq;
using System.Collections.Generic;
using GameDatabase.Enums;
using GameDatabase.DataModel;

namespace GameDatabase.Query
{
	public struct ShipBuildQuery
	{
		public enum FilterMode
		{
			Size = 1,
			Faction = 2,
			Difficulty = 4,
			SizeAndDifficulty = Size|Difficulty,
			All = 0xffff,
		}

		private readonly IDatabase _database;
		private readonly IEnumerable<ShipBuild> _shipBuilds;

		public IEnumerable<ShipBuild> All => _shipBuilds;
		public ShipBuild Random(System.Random random) => _shipBuilds.RandomElement(random);

		public static ShipBuildQuery EnemyShips(IDatabase database)
		{
			return new ShipBuildQuery(database, database.ShipBuildList.Where(IsValidForEnemy));
		}

		public static ShipBuildQuery PlayerShips(IDatabase database)
		{
			return new ShipBuildQuery(database, database.ShipBuildList.Where(IsValidForPlayer));
		}

		public static ShipBuildQuery Starbases(IDatabase database)
		{
			return new ShipBuildQuery(database, database.ShipBuildList.Where(IsStarbase));
		}

		public static ShipBuildQuery Drones(IDatabase database)
		{
			return new ShipBuildQuery(database, database.ShipBuildList.Where(IsDrone));
		}

		public static ShipBuildQuery AllShips(IDatabase database)
		{
			return new ShipBuildQuery(database, database.ShipBuildList);
		}

		public ShipBuildQuery Append(ShipBuild build) => build == null ? this : new(_database, _shipBuilds.Append(build));
		public ShipBuildQuery Prepend(ShipBuild build) => build == null ? this : new(_database, _shipBuilds.Prepend(build));
		public ShipBuildQuery Shuffle(System.Random random) => new(_database, _shipBuilds.OrderBy(item => random.Next()));
		public ShipBuildQuery SelectRandom(int quantity, System.Random random) => new(_database, _shipBuilds.RandomElements(quantity, random));
		public ShipBuildQuery SelectUniqueRandom(int quantity, System.Random random) => new(_database, _shipBuilds.RandomUniqueElements(quantity, random));
		public ShipBuildQuery Concat(ShipBuildQuery other) => Concat(other._shipBuilds);
		public ShipBuildQuery Concat(IEnumerable<ShipBuild> other) => new(_database, _shipBuilds.Concat(other));
		public ShipBuildQuery Where(System.Func<ShipBuild, bool> predicate) => new(_database, _shipBuilds.Where(predicate));
		public ShipBuildQuery Take(int count) => new(_database, _shipBuilds.Take(count));
		public ShipBuildQuery TryApplyFilter(System.Func<ShipBuild, bool> predicate) => new(_database, TryApplyFilter(_shipBuilds, predicate));

		public ShipBuildQuery Common() => new(_database, _shipBuilds.Where(IsCommonShip));
		public ShipBuildQuery CommonAndRare() => new(_database, _shipBuilds.Where(IsCommonOrRareShip));
		public ShipBuildQuery HiddenShips() => new(_database, _shipBuilds.Where(IsHiddenShip));
		public ShipBuildQuery Flagships() => new(_database, _shipBuilds.Where(IsFlagship));
		public ShipBuildQuery WithSizeClass(SizeClass min, SizeClass max) =>
			new(_database, _shipBuilds.Where(build => IsValueInRange((int)build.Ship.SizeClass, (int)min, (int)max)));
		public ShipBuildQuery BelongToFaction(Faction faction) => 
			new(_database, _shipBuilds.Where(build => build.Faction == faction));
		public ShipBuildQuery WithDifficulty(DifficultyClass min, DifficultyClass max) =>
			new(_database, _shipBuilds.Where(build => IsValueInRange((int)build.DifficultyClass, (int)min, (int)max)));
		public ShipBuildQuery WithMinDifficulty(DifficultyClass min) =>
			new(_database, _shipBuilds.Where(build => build.DifficultyClass > min));
		public ShipBuildQuery WithBestAvailableClass(DifficultyClass shipClassMax) =>
			new ShipBuildQuery(_database, ShipsWithBestAvailableClass(_shipBuilds, shipClassMax));

		public ShipBuildQuery FilterByStarDistance(int distance, FilterMode filterMode = FilterMode.All)
		{
			var filterByFaction = HasFlag(filterMode, FilterMode.Faction);
			var filterByDifficultyClass = HasFlag(filterMode, FilterMode.Difficulty);
			var filterBySizeClass = HasFlag(filterMode, FilterMode.Size);
			var maxShipClass = Utils.Helpers.StarLevelToMaxDifficulty(distance);
			var minShipClass = Utils.Helpers.StarLevelToMinDifficulty(distance);
			var minSpawnDistance = _database.GalaxySettings.ShipMinSpawnDistance;

			return new ShipBuildQuery(_database, _shipBuilds.Where(build => {
				if (filterByFaction && !IsFactionValid(build, distance)) return false;
				if (filterByDifficultyClass && !IsValueInRange((int)build.DifficultyClass, (int)minShipClass, (int)maxShipClass)) return false;
				if (filterBySizeClass && minSpawnDistance(build.Ship.SizeClass) > distance) return false;
				return true;
			}));
		}

		private ShipBuildQuery(IDatabase database, IEnumerable<ShipBuild> shipBuilds)
		{
			_database = database;
			_shipBuilds = shipBuilds;
		}

		private static bool IsValidForEnemy(ShipBuild build)
		{
			if (build.NotAvailableInGame) return false;
			if (build.Ship.ShipType != ShipType.Common && build.Ship.ShipType != ShipType.Flagship) return false;
			if (build.Ship.ShipRarity == ShipRarity.Unique) return false;
			return true;
		}

		private static bool IsValidForPlayer(ShipBuild build)
		{
			if (build.NotAvailableInGame) return false;
			if (build.DifficultyClass != DifficultyClass.Default) return false;
			if (build.Ship.ShipType != ShipType.Common && build.Ship.ShipType != ShipType.Flagship) return false;
			return true;
		}

		private static bool IsStarbase(ShipBuild build)
		{
			if (build.NotAvailableInGame) return false;
			if (build.Ship.ShipType != ShipType.Starbase) return false;
			return true;
		}

		public static bool IsDrone(ShipBuild build)
		{
			if (build.NotAvailableInGame) return false;
			if (build.Ship.ShipType != ShipType.Drone) return false;
			return true;
		}

		private static bool IsFlagship(ShipBuild build) => build.Ship.ShipType == ShipType.Flagship;

		private static bool IsCommonShip(ShipBuild build)
		{
			if (build.Ship.ShipType != ShipType.Common) return false;
			return build.Ship.ShipRarity == ShipRarity.Normal;
		}

		private static bool IsCommonOrRareShip(ShipBuild build)
		{
			if (build.Ship.ShipType != ShipType.Common) return false;
			return build.Ship.ShipRarity == ShipRarity.Normal || build.Ship.ShipRarity == ShipRarity.Rare;
		}

		private static bool IsHiddenShip(ShipBuild build)
		{
			if (build.Ship.ShipType != ShipType.Common || build.Ship.ShipType != ShipType.Flagship) return false;
			return build.Ship.ShipRarity == ShipRarity.Hidden;
		}

		private static bool IsFactionValid(ShipBuild build, int distance)
		{
			if (build.Faction.NoWanderingShips) return false;
			return build.Faction.WanderingShipsRange.Contains(distance);
		}

		private static bool IsValueInRange(int value, int min, int max)
		{
			return value >= min && value <= max;
		}

		private static bool HasFlag(FilterMode filterMode, FilterMode flag) => (filterMode & flag) == flag;

		private static IEnumerable<ShipBuild> ShipsWithBestAvailableClass(IEnumerable<ShipBuild> shipBuilds, DifficultyClass shipClassMax)
		{
			var bestClass = DifficultyClass.Default;
			foreach (var build in shipBuilds)
			{
				if (build.DifficultyClass > bestClass && build.DifficultyClass <= shipClassMax)
					bestClass = build.DifficultyClass;

				if (build.DifficultyClass == shipClassMax)
					yield return build;
			}

			if (bestClass < shipClassMax)
				foreach (var build in shipBuilds)
					if (build.DifficultyClass == bestClass)
						yield return build;
		}

		private static IEnumerable<ShipBuild> TryApplyFilter(IEnumerable<ShipBuild> shipBuilds, System.Func<ShipBuild, bool> predicate)
		{
			int count = 0;
			foreach (var build in shipBuilds)
			{
				if (!predicate(build)) continue;

				count++;
				yield return build;
			}

			if (count == 0)
				foreach (var build in shipBuilds)
					yield return build;
		}

		// TODO: move to database
		public static DifficultyClass StarLevelToMaxDifficulty(int level) => (DifficultyClass)(level / 25);
		public static DifficultyClass StarLevelToMinDifficulty(int level) => level < 50 ? DifficultyClass.Default : DifficultyClass.Class1;
	}
}
