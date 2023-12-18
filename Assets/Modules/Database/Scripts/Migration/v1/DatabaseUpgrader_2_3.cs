using System.Collections.Generic;

namespace DatabaseMigration.v1
{
    public partial class DatabaseUpgrader
    {
		partial void Migrate_2_3()
		{
			UnityEngine.Debug.LogWarning("Database migration: v1.2 -> v1.3");

			Content.GalaxySettings.StartingInventory = Content.GalaxySettings.StartingInvenory;
			Content.GalaxySettings.StartingInvenory = 0;

			var ships = new Dictionary<int, Serializable.ShipSerializable>();
			foreach (var item in Content.ShipList)
				ships.Add(item.Id, item);

			foreach (var build in Content.ShipBuildList)
			{
				build.AvailableForEnemy = !build.NotAvailableInGame && ships.TryGetValue(build.ShipId, out var ship) && ship.ShipRarity != Enums.ShipRarity.Unique;
				build.AvailableForPlayer = !build.NotAvailableInGame && build.DifficultyClass == Enums.DifficultyClass.Default;
			}
		}
    }
}
