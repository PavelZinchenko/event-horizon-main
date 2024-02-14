namespace DatabaseMigration.v1
{
    public partial class DatabaseUpgrader
    {
		partial void Migrate_4_5()
		{
            GameDiagnostics.Trace.LogWarning("Database migration: v1.4 -> v1.5");

            Content.CombatRulesList.Add(new Serializable.CombatRulesSerializable { Id = 1 });
            Content.CreateCombatSettings().DefaultCombatRules = 1;
		}
	}
}
