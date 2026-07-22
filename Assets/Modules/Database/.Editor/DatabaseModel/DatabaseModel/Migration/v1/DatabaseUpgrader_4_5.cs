using DatabaseMigration.v1.Serializable;

namespace DatabaseMigration.v1
{
	public partial class DatabaseUpgrader
	{
		partial void Migrate_4_5()
		{
			Console.WriteLine("Database migration: v1.4 -> v1.5");

            var combatRules = new CombatRulesSerializable { Id = 1, FileName = "DefaultCombatRules.json" };
            Content.CombatRulesList.Add(combatRules);

            Content.CreateCombatSettings().DefaultCombatRules = 1;
		}
	}
}
