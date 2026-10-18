# Project Tasks

## Active / Open (database editor — `docs/database/editor.md`)
- [ ] Port `Migrate_3_4` (component-mod replacement from `Resources/DatabaseMigration/v1.3`) to `.Editor/DatabaseModel/DatabaseModel/Migration/v1/`
- [ ] `rebuild_editor.cmd`: replace the hard-coded `MSBuild.exe` path (vswhere or `dotnet build`)

## Active / Open (code generator — `docs/database/code_generator.md`)
- [ ] `DatabaseCodeGenerator` returns a non-zero exit code on schema or generation errors
- [ ] Fix `StringExtensions.ContainsKey` word-boundary check (indexes `key` instead of `source`, inverted condition)
- [ ] Emit enum `default` values as `Enum.Value`, not a quoted string, in game serializable DTOs
- [ ] Parse expression `minvalue`/`maxvalue` as floats for `float` results

## Planned (quests — `docs/issues/quests.md`)
- [ ] After PR #547 merges: record in `docs/issues/quests.md` that loot requirements are re-evaluated on every load, so state-dependent requirements can change quest loot after a reload

## Planned (battle simulator — `docs/issues/battle_simulator.md`)
- [ ] After PR #599 merges: give the gravity bomb its own `UnitClass.GravityField` (D14)
- [ ] Isolate the R1 step-cost rise of the physics library between P2-1 and P2-4 (0.41 → 0.72 ms; `TODO/battle_simulator_implementation.md`)
