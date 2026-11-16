using DatabaseMigration.v1.Enums;
using DatabaseMigration.v1.Serializable;

namespace DatabaseMigration.v1
{
      public partial class DatabaseUpgrader
      {
            partial void Migrate_7_8()
            {
                  GameDiagnostics.Trace.LogWarning("Database migration: v1.7 -> v1.8");

                  // Replace the 1.7 Titan code rule with two record flags. We(Modder community on Discord) plan to move
                  // more hard-coded game rules into database settings the same way.
                  // Please come back online on Discord; we need you, Pavel.
                  foreach (var satellite in Content.SatelliteList)
                  {
                        if (satellite.SizeClass != SizeClass.Titan) continue;

                        satellite.NotAvailableInBlackMarket = true;
                        satellite.NotAvailableInRuin = true;
                  }
            }
      }
}
