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
- [ ] Network session: default correction interval 150 ms, the gate's choice (`NetworkSessionOptions`; `TODO/battle_simulator_network.md`, "Gate result")
- [ ] Phase 7: run the network session on its own thread with short loop gaps (`TODO/battle_simulator_network.md`, "Open after the gate")
- [ ] Phase 7: repeat the network bandwidth and drift rows with real ships, with 8-bit resource precision (`TODO/battle_simulator_network.md`, "Open after the gate")
- [ ] Set the missile correction budget (300-byte placeholder) from a playable scene (`TODO/battle_simulator_network.md`, "Open after the gate")
- [ ] Network target rule and gate scene for auto-aiming barrels and satellites when phase 4 or 5 adds them (`TODO/battle_simulator_network_plan.md`, open items)
- [ ] Phase 5 homing rework: homing copies that orbit a drone explode about 1 s early (`TODO/battle_simulator_network.md`, accepted defects)
- [ ] Core combat scene: device cooldowns in the snapshot, sounds, legacy-quality effects, beacons (`TODO/battle_simulator_scene.md`, out of scope)
- [ ] Core combat scene: decide the camera whip toward a new player ship (`TODO/battle_simulator_scene.md`, step 4)
- [ ] Migration phase 4: original ships/content and campaign result integration (`TODO/battle_simulator_migration.md`)
- [ ] Migration phase 5: built-in device coverage, data-model gate and explicit deferrals (`TODO/battle_simulator_migration.md`)
- [ ] Migration phase 6: exploration and controls configuration (`TODO/battle_simulator_migration.md`)
- [ ] Migration phase 7: live duel, release gates and GitHub alpha (`TODO/battle_simulator_migration.md`)
- [ ] Battle core rules from P4: worm turn-rate cap, free space before tail spawn or regrowth, harpoon hook stops at rope length (`TODO/battle_simulator_physics.md`, "P4 closure")
- [ ] Verify the tractor beam rope anchor (`autoConfigureConnectedAnchor`) in Unity (`TODO/battle_simulator_physics.md`, P4 risk 3)
- [ ] CombatSimulation.Physics P3 follow-ups: measure target-spin misses, count buffer growth events (`TODO/battle_simulator_physics.md`, "P3 closure")
- [ ] Tractor beam redesign: a breakable rope that reels the target in (needs owner decisions: physical or one-sided pull, break rule)
- [ ] After PR #599 merges: give the gravity bomb its own `UnitClass.GravityField` (D14)
