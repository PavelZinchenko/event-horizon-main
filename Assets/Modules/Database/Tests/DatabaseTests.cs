using System.Linq;
using NUnit.Framework;
using GameDatabase.Enums;
using GameDatabase.Query;

namespace GameDatabase.Tests
{
	[TestFixture]
	public class DatabaseTests
    {
		private IDatabase Database { get; } = new Database();

		[OneTimeSetUp]
		public void RunBeforeAnyTests()
		{
			Database.LoadDefault();
		}

		[OneTimeTearDown]
		public void RunAfterAnyTests()
		{
		}

		[Test]
		public void TestShipMinSpawnDistance()
		{
			var expression = Database.GalaxySettings.ShipMinSpawnDistance;
			Assert.Greater(expression(SizeClass.Destroyer), expression(SizeClass.Frigate));
			Assert.Greater(expression(SizeClass.Cruiser), expression(SizeClass.Destroyer));
			Assert.Greater(expression(SizeClass.Battleship), expression(SizeClass.Cruiser));
		}

		[Test]
		public void FindPlayerShips()
		{
			var playerShips = ShipBuildQuery.PlayerShips(Database);
			Assert.IsTrue(playerShips.All.Any());
		}

		[Test]
		[TestCase(0)]
		[TestCase(25)]
		[TestCase(50)]
		[TestCase(100)]
		[TestCase(300)]
		[TestCase(500)]
		[TestCase(1000)]
		public void FindEnemyShips(int distance)
		{
			var count = ShipBuildQuery.EnemyShips(Database).CommonAndRare().FilterByStarDistance(distance).All.Count();
			UnityEngine.Debug.Log($"Found {count} valid enemy ship builds on distance {distance}");
			Assert.Greater(count, 0);
		}

		[Test]
		public void CheckFactions()
		{
			foreach (var faction in Database.FactionList)
			{
				var factionShips = ShipBuildQuery.EnemyShips(Database).BelongToFaction(faction);
				var ships = factionShips.CommonAndRare().All.Count();
				var flagships = factionShips.Flagships().All.Count();
				UnityEngine.Debug.Log($"Faction {faction.Name} has {ships} ships and {flagships} flagships");
				Assert.IsTrue(ships + flagships > 0 || faction.NoTerritories && faction.NoWanderingShips);
			}
		}
	}
}
