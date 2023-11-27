using DatabaseMigration.v1.Serializable;

namespace DatabaseMigration.v1
{
    public partial class DatabaseUpgrader
    {
        partial void Migrate_1_2()
        {
            UnityEngine.Debug.LogWarning("Database migration: v1.1 -> v1.2");

            Content.GalaxySettings.EnemyLevel = "MIN(3*distance/5 - 5, MaxEnemyShipsLevel)";
        }
    }
}
