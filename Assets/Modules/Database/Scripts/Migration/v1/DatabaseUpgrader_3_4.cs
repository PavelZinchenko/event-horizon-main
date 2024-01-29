namespace DatabaseMigration.v1
{
    public partial class DatabaseUpgrader
    {
		partial void Migrate_3_4()
		{
			UnityEngine.Debug.LogWarning("Database migration: v1.3 -> v1.4");
			Content.ComponentModList.Clear();
			var storage = new GameDatabase.Storage.ResourceDatabaseStorage("DatabaseMigration/v1.3");
			storage.LoadContent(Content);
		}
	}
}
