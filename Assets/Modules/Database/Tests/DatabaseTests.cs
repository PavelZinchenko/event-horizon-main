using System.Linq;
using NUnit.Framework;
using GameDatabase.Enums;
using GameDatabase.Query;
using GameDatabase.DataModel;

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

		[Test]
		public void CheckFactionIcons()
		{
			foreach (var faction in Database.FactionList.Where(item => !string.IsNullOrEmpty(item.Icon)))
			{
				var texture = UnityEngine.Resources.Load<UnityEngine.Texture2D>(
					"Textures/Factions/" + faction.Icon);
				Assert.IsNotNull(texture, $"Missing faction icon: {faction.Id.Value} ({faction.Icon})");
				Assert.AreEqual(512, texture.width, $"Unexpected faction icon width: {faction.Icon}");
				Assert.AreEqual(512, texture.height, $"Unexpected faction icon height: {faction.Icon}");
			}
		}

		[Test]
		public void CheckThreeBodyContent()
		{
			var factions = Database.FactionList.Where(item => item.Id.Value >= 21 && item.Id.Value <= 27).ToArray();
			Assert.AreEqual(7, factions.Length);
			var starshipEarth = factions.Single(item => item.Id.Value == 21);
			Assert.IsFalse(starshipEarth.NoTerritories);
			Assert.IsFalse(starshipEarth.NoWanderingShips);
			Assert.IsFalse(starshipEarth.HideResearchTree);

			Assert.IsTrue(factions.Where(item => item.Id.Value != 21).All(item =>
				item.NoTerritories &&
				item.NoWanderingShips &&
				item.HideFromMerchants &&
				item.NoMissions &&
				!item.HideResearchTree));

			var starshipEarthComponents = Database.ComponentList
				.Where(item => item.Id.Value >= 300 && item.Id.Value <= 306)
				.ToArray();
			Assert.AreEqual(7, starshipEarthComponents.Length);
			Assert.IsTrue(starshipEarthComponents.All(item =>
				item.ContentSource == ContentSource.ThreeBody &&
				item.Faction.Id.Value == 21));
			Assert.AreEqual(5, starshipEarthComponents.Select(item => item.DisplayCategory).Distinct().Count());

			var componentTechnologies = Database.TechnologyList.OfType<Technology_Component>()
				.Where(item => item.Faction.Id.Value == 21 &&
					item.Component.Id.Value >= 300 &&
					item.Component.Id.Value <= 306)
				.ToArray();
			Assert.AreEqual(7, componentTechnologies.Length);
			Assert.IsTrue(componentTechnologies.All(item => !item.Hidden));

			var ships = Database.ShipList
				.Where(item => item.Id.Value >= 161 && item.Id.Value <= 165)
				.OrderBy(item => item.SizeClass)
				.ToArray();
			Assert.AreEqual(5, ships.Length);
			Assert.IsTrue(ships.All(item => item.Faction.Id.Value == 21));
			CollectionAssert.AreEqual(
				new[] { SizeClass.Frigate, SizeClass.Destroyer, SizeClass.Cruiser, SizeClass.Battleship, SizeClass.Titan },
				ships.Select(item => item.SizeClass).ToArray());

			var builds = Database.ShipBuildList
				.Where(item => item.Id.Value >= 412 && item.Id.Value <= 416)
				.ToArray();
			Assert.AreEqual(5, builds.Length);
			Assert.IsTrue(builds.All(item => item.AvailableForPlayer && item.AvailableForEnemy));

			var shipTechnologies = Database.TechnologyList.OfType<Technology_Ship>()
				.Where(item => item.Ship.Id.Value >= 161 && item.Ship.Id.Value <= 165)
				.ToArray();
			Assert.AreEqual(5, shipTechnologies.Length);
			Assert.IsTrue(shipTechnologies.All(item => !item.Hidden));
		}
	}
}
