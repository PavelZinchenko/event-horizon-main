# Project Tasks

## Active / Open (database editor — `docs/database/editor.md`)
- [ ] Port `Migrate_3_4` (component-mod replacement from `Resources/DatabaseMigration/v1.3`) to `.Editor/DatabaseModel/DatabaseModel/Migration/v1/`
- [ ] `rebuild_editor.cmd`: replace the hard-coded `MSBuild.exe` path (vswhere or `dotnet build`)

## Active / Open (code generator — `docs/database/code_generator.md`)
- [ ] `DatabaseCodeGenerator` returns a non-zero exit code on schema or generation errors
- [ ] Fix `StringExtensions.ContainsKey` word-boundary check (indexes `key` instead of `source`, inverted condition)
- [ ] Emit enum `default` values as `Enum.Value`, not a quoted string, in game serializable DTOs
- [ ] Parse expression `minvalue`/`maxvalue` as floats for `float` results

## Planned (battle simulator — `docs/issues/battle_simulator.md`)
- [ ] Performance gate: snapshot copy is 188 KiB per tick at R2, 3.2× the estimate; decide on changed ranges or smaller states (`TODO/battle_simulator_core.md`, risk 3)
- [ ] Migration phase 3a step 0: owner decision B3 (campaign abort) and its acceptance examples (`TODO/battle_simulator_migration.md`)
- [ ] Migration phase 3b: relay simulator, network gate and roster agreement checks (`TODO/battle_simulator_network.md`)
- [ ] Migration phase 3c: new combat scene, adapter, input and presentation (`TODO/battle_simulator_scene.md`)
- [ ] Before migration phase 4: decide save rollback, active-mod admission and campaign abort policies (`TODO/battle_simulator_migration.md`)
- [ ] Migration phase 4: original ships/content and campaign result integration (`TODO/battle_simulator_migration.md`)
- [ ] Migration phase 5: built-in device coverage, data-model gate and explicit deferrals (`TODO/battle_simulator_migration.md`)
- [ ] Migration phase 6: exploration and controls configuration (`TODO/battle_simulator_migration.md`)
- [ ] Migration phase 7: live duel, release gates and GitHub alpha (`TODO/battle_simulator_migration.md`)
- [ ] Battle core rules from P4: worm turn-rate cap, free space before tail spawn or regrowth, harpoon hook stops at rope length (`TODO/battle_simulator_physics.md`, "P4 closure")
- [ ] Verify the tractor beam rope anchor (`autoConfigureConnectedAnchor`) in Unity (`TODO/battle_simulator_physics.md`, P4 risk 3)
- [ ] CombatSimulation.Physics P3 follow-ups: measure target-spin misses, count buffer growth events (`TODO/battle_simulator_physics.md`, "P3 closure")
- [ ] Tractor beam redesign: a breakable rope that reels the target in (needs owner decisions: physical or one-sided pull, break rule)
- [ ] After PR #599 merges: give the gravity bomb its own `UnitClass.GravityField` (D14)
