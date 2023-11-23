namespace DatabaseMigration.v1
{
    public partial class DatabaseUpgrader
    {
        partial void Migrate_0_1()
        {
            UnityEngine.Debug.LogError("Database migration: v1.0 -> v1.1");
        }
    }
}
