# Project Tasks

## Active / Open (database editor — `docs/database/editor.md`)
- [ ] Port `Migrate_3_4` (component-mod replacement from `Resources/DatabaseMigration/v1.3`) to `.Editor/DatabaseModel/DatabaseModel/Migration/v1/`
- [ ] `rebuild_editor.cmd`: replace the hard-coded `MSBuild.exe` path (vswhere or `dotnet build`)

## Active / Open (code generator — `docs/database/code_generator.md`)
- [ ] `DatabaseCodeGenerator` returns a non-zero exit code on schema or generation errors
- [ ] Fix `StringExtensions.ContainsKey` word-boundary check (indexes `key` instead of `source`, inverted condition)
- [ ] Emit enum `default` values as `Enum.Value`, not a quoted string, in game serializable DTOs
- [ ] Parse expression `minvalue`/`maxvalue` as floats for `float` results

## Open (quests — `docs/quests.md`)
- [ ] Investigate: a quest beacon stays after its quest completes and lets the player start the quest again

## Planned (battle simulator — `docs/issues/battle_simulator.md`)
- [ ] Performance gate: set a snapshot budget or close the risk; R1 measured 0.24 ms per tick at R2 (`TODO/battle_simulator_core.md`, risk 3)
- [ ] Phase 7: run the network session on its own thread with short loop gaps (`TODO/battle_simulator_network.md`, "Open after the gate")
- [ ] Phase 7: repeat the network bandwidth and drift rows with real ships, with 8-bit resource precision (`TODO/battle_simulator_network.md`, "Open after the gate")
- [ ] Set the missile correction budget (300-byte placeholder) from a playable scene (`TODO/battle_simulator_network.md`, "Open after the gate")
- [ ] Review: satellite network gate covers only the engine-free test; decide on a `Satellites` scene in the local harness (`TODO/battle_simulator_phase5.md`, 12c finding 7)
- [ ] Frontier satellite features: custom mount points, steadiness, destroyable satellites with respawn (`TODO/battle_simulator_phase5.md`, "Frontier satellites (later)")
- [ ] Phase 7: homing copies that orbit a drone explode about 1 s early (`TODO/battle_simulator_phase5.md`, H14)
- [ ] Core combat scene: device cooldowns in the snapshot, sounds, legacy-quality effects, beacons (`TODO/battle_simulator_scene.md`, out of scope)
- [ ] Core combat scene: decide the camera whip toward a new player ship (`TODO/battle_simulator_scene.md`, step 4)
- [ ] Cost test of the AI threat cut-off (more than 30 pilots) on the new simulator; remove it if not needed (`TODO/battle_simulator_phase4.md`, Targets (7c))
- [ ] Database editor: collider editor for `Ship.Collider`, after migration phase 4 (`TODO/battle_simulator_phase4.md`, D5)
- [ ] Charged beam lifetime × (0.5 + 0.5 × charge) in legacy `BulletStats` (deferred E2, `TODO/battle_simulator_ammo_mapping.md`, G9; the core does it since phase 5, step 4f)
- [ ] Capsule hit shape for long bullets (deferred E3, `TODO/battle_simulator_ammo_mapping.md`, G10)
- [In Progress] Migration phase 5: built-in device coverage, data-model gate and explicit deferrals (`TODO/battle_simulator_migration.md`)
- [In Progress] Migration phase 5: asteroids as core bodies, with the AI obstacle and asteroid-threat port (`TODO/battle_simulator_phase4.md`, D8)
- [ ] Migration phase 6: exploration and controls configuration (`TODO/battle_simulator_migration.md`)
- [ ] Migration phase 7: live duel, release gates and GitHub alpha (`TODO/battle_simulator_migration.md`)
- [ ] Verify the tractor beam rope anchor (`autoConfigureConnectedAnchor`) in Unity (`TODO/battle_simulator_physics.md`, P4 risk 3)
- [ ] CombatSimulation.Physics P3 follow-ups: measure target-spin misses, count buffer growth events (`TODO/battle_simulator_physics.md`, "P3 closure")
- [ ] Core tractor beam redesign: a breakable rope that reels the target in; old simulator keeps the pull beam (needs owner decisions: physical or one-sided pull, break rule)
- [ ] After the new combat simulator is done: convert mod obsolete ammunition with `AmmunitionConverter` as a migration step (`TODO/battle_simulator_ammo_mapping.md`, E5)
- [ ] After PR #599 merges: give the gravity bomb its own `UnitClass.GravityField` (D14)
