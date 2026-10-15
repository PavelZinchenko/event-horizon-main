# Database Module Issues

This file records problems in `Assets/Modules/Database/` and in the code that reads database records. For a description of the current design, see [Database module](../database.md). Like the [BattleSimulator issues](battle_simulator.md), it lists local defects as well as structural issues.

The analysis was done by reading the code and the built-in data on 2026-10-11. No issue was reproduced in Play Mode. Each issue has a severity (High, Medium, or Low) and a status:

- **Confirmed** – the code path is certain. The failure follows directly from the code.
- **Confirmed (latent)** – the code path is certain, but built-in data does not trigger it. Mod data can.
- **Plausible** – the code is wrong, but the failure needs specific data or conditions.

Structural issues use Problem / Evidence / Effect / Direction. Defects use Where / Failure / Fix.

### DB1. The UnlimitedRespawn perk crashes ship creation

**Severity:** High. **Status:** Confirmed (latent).

**Where.** `ShipPerkType.UnlimitedRespawn` maps to `ModificationType.UnlimitedRespawn` ([ModificationType.cs:37](../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/ModificationType.cs#L37)). The matching case in `ModificationFactory.Create` is commented out ([ModificationFactory.cs:38](../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/ModificationFactory.cs#L38)), so the `default` branch throws `InvalidEnumArgumentException` ([line 49](../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/ModificationFactory.cs#L49)). `ShipModel(ShipBuild, IDatabase)` calls the factory for every perk ([ShipModel.cs:30](../../Assets/Modules/ShipConstructor/Scripts/Ships/ShipModel.cs#L30)).

**Failure.** A ship build with perk 7 throws when the game creates the ship. No built-in build uses this perk. A mod can.

**Fix.** Restore the case, or map the perk to `ModificationType.Empty` and mark the enum value obsolete in the schema.

### DB2. Migrations assume a complete database

**Severity:** High. **Status:** Confirmed.

**Where.** `FolderDatabaseStorage` assumes version 1.0 when the mod has no `DatabaseSettings` record ([FolderDatabaseStorage.cs:32](../../Assets/Modules/Database/Scripts/Storage/FolderDatabaseStorage.cs#L32)). Every migration step then runs on the mod content alone, without the built-in records.

- `Migrate_1_2` sets `Content.GalaxySettings.StartingInvenory` without a null check ([DatabaseUpgrader_1_2.cs:32](../../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_1_2.cs#L32)). It picks the new loot ID as `max(mod loot) + 1` ([line 16](../../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_1_2.cs#L16)).
- `Migrate_3_4` deletes all component mods of the mod ([DatabaseUpgrader_3_4.cs:9](../../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_3_4.cs#L9)).
- `Migrate_4_5` adds `CombatRules` 1 and creates a whole `CombatSettings` record without AI trees ([DatabaseUpgrader_4_5.cs:9](../../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_4_5.cs#L9)).
- `Migrate_6_7` numbers the new component group tags from 1 ([DatabaseUpgrader_6_7.cs:81](../../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_6_7.cs#L81)).

**Failure.**
1. A mod without `GalaxySettings` throws `NullReferenceException` in `Migrate_1_2`, and the mod does not load.
2. The new loot ID collides with built-in loot IDs. The first record wins (see DB15.4), so one of the two records is lost.
3. The new `CombatSettings` replaces the built-in record (see DB7). Enemies then fall back to the legacy AI.
4. The new group tags and `CombatRules` 1 override built-in records with the same IDs.

**Fix.** Write `DatabaseSettings` with the current version into every mod that the editor exports. In the migrations, skip steps that need records the mod does not contain, and allocate new IDs above the built-in maximum.

### DB3. The migration code is older than the schema

**Severity:** Medium. **Status:** Confirmed.

**Where.** `GeneratedMigrationCode/v1` has no classes for `WeaponSlots`, `WeaponSlot`, and `ShipVisualEffects`. The v1 `DatabaseContent.LoadJson` throws `Unknown file type` for item type 118. These v1 fields are missing: `BulletBody.DetonateWhenDestroyed`, `BulletController.Lifetime`, `Device.AmmunitionId`, `Ship.VisualEffects`, the explosion effects and sounds in `ShipSettings`, and `Technology.DoesnPreventUnlocking`.

**Failure.** When a mod goes through migration, these fields are dropped during export, and a `WeaponSlots` record breaks the load.

**Fix.** Run `generate_migration_code.cmd`. Add a check that fails when the migration code and the schema differ.

### DB4. Null references have two sentinels and Id 0 is not protected

**Severity:** Medium. **Status:** Confirmed.

**Problem.** The game and the editor disagree on which ID means "no reference", and nothing reserves ID 0.

**Evidence.**
- JSON uses `0` for an empty reference, because that is the Unity default. 38 built-in components use `Faction: -1`.
- The game tests `_id < 0` ([ItemId.cs:18](../../Assets/Modules/Database/Scripts/Types/ItemId.cs#L18)). The editor tests `Value <= 0` ([ItemId.cs:40](../../Assets/Modules/Database/.Editor/DatabaseModel/DatabaseModel/Model/ItemId.cs#L40)).
- `Faction.Empty` has ID 0.
- The built-in [`Component/Afterburner_buggy.json`](../../Assets/Modules/Database/Resources/Database/Component/Afterburner_buggy.json) has no `Id`, so it loads as component 0. A missing `ComponentId` in a build or a loot item resolves to it instead of failing.

**Effect.** A record with `Id` 0 becomes the target of every empty reference of its type in the game. The editor shows the same reference as empty.

**Direction.** Pick one sentinel. Reject `Id` 0 in the loader and the editor, or treat 0 as null in the game as the editor does.

### DB5. Error handling differs by storage

**Severity:** Medium. **Status:** Confirmed.

**Problem.** The same bad file has a different result in each storage backend and build type.

**Evidence.**
- `ResourceDatabaseStorage` catches JSON parse errors in the Editor ([ResourceDatabaseStorage.cs:34](../../Assets/Modules/Database/Scripts/Storage/ResourceDatabaseStorage.cs#L34)), but not in builds ([line 39](../../Assets/Modules/Database/Scripts/Storage/ResourceDatabaseStorage.cs#L39)).
- `FolderDatabaseStorage` does not catch them, so the whole load fails ([FolderDatabaseStorage.cs:81](../../Assets/Modules/Database/Scripts/Storage/FolderDatabaseStorage.cs#L81)).
- `FileDatabaseStorage` logs the error and skips the file ([FileDatabaseStorage.cs:58](../../Assets/Modules/Database/Scripts/Storage/FileDatabaseStorage.cs#L58)).
- `notnull` and `object_list` references throw and stop the whole load ([Database.cs:404](../../Assets/Modules/Database/Scripts/GeneratedGameCode/Database.cs#L404), [Component.cs:50](../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/Component.cs#L50) for `PossibleModifications`). Plain references to missing records silently become null.
- `LoadLocalization`, `LoadImage`, and `LoadAudioClip` use `Dictionary.Add` ([DatabaseContent.cs:577](../../Assets/Modules/Database/Scripts/GeneratedGameCode/Storage/DatabaseContent.cs#L577), [582](../../Assets/Modules/Database/Scripts/GeneratedGameCode/Storage/DatabaseContent.cs#L582), [587](../../Assets/Modules/Database/Scripts/GeneratedGameCode/Storage/DatabaseContent.cs#L587)).

**Effect.** A mod that loads in the Editor can fail in a build. Two images with the same file name in different mod subfolders, or `x.wav` next to `x.ogg`, throw "same key" and the mod fails. If `LoadDefault` fails, the game keeps an empty database with null settings.

**Direction.** Use one policy for all backends: collect errors per file, report them together, and fail the load only for errors that make the data unusable. Report duplicate media keys as errors with both file names.

### DB6. Expression errors appear late and become zero

**Severity:** Medium. **Status:** Confirmed.

**Problem.** Database expressions compile on first use, not on load ([IntToInt.cs:26](../../Assets/Modules/Database/Scripts/GeneratedGameCode/Expressions/IntToInt.cs#L26)).

**Evidence.** `ExpressionBuilder.Build` logs a compile error and returns an empty expression ([ExpressionBuilder.cs:39](../../Assets/Modules/Database/Scripts/Expression/ExpressionBuilder.cs#L39)). The function then returns 0, and the caller clamps it.

**Effect.** A bad formula, for example for enemy level or time limit, shows up only in play, as a silent 0.

**Direction.** Compile all expressions after load, and report errors with the record and field name. Run the same check in the analyzer.

### DB7. Settings records cannot be partly overridden

**Severity:** Low. **Status:** Confirmed (design).

**Problem.** A settings record in a mod replaces the whole built-in record. Fields that the mod omits get schema defaults, not the built-in values.

**Evidence.** Callers handle null settings references in different ways. Most fall back to `CombatSettings.DefaultCombatRules`. `XmasTree` assigns `SpecialEventSettings.XmasCombatRules` directly ([XmasTree.cs:48](../../Assets/Scripts/Domain/Galaxy/StarContent/XmasTree.cs#L48)). `CombatModelBuilder` then reads `Rules.DisableSkillBonuses` ([CombatModelBuilder.cs:52](../../Assets/Scripts/Combat/Domain/CombatModelBuilder.cs#L52)).

**Effect.** A mod that changes one setting must copy all others. A mod `SpecialEventSettings` without `XmasCombatRules` throws `NullReferenceException` at the Christmas tree.

**Direction.** Merge settings field by field, or document the replace rule in the editor. Route all combat rules lookups through one fallback helper.

### DB8. Quest node IDs above 1000 collide

**Severity:** Medium. **Status:** Confirmed (latent).

**Where.** `NodeId` clamps its value to 0–1000 ([NodeId.cs:11](../../Assets/Modules/Database/Scripts/Types/NodeId.cs#L11)). The schema allows `Node.Id` up to 999,999.

**Failure.** Two nodes with IDs above 1000 both become 1000, and `ToDictionary` in `NodeBuilder` throws. The built-in maximum node ID is 100.

**Fix.** Make the schema range match the clamp, or remove the clamp.

### DB9. Live fields are marked obsolete

**Severity:** Medium. **Status:** Confirmed.

**Problem.** The schema marks fields as obsolete although the game still depends on them.

**Evidence.**
- `Component.CellType` has `options="obsolete"` ([Component.xml:13](../../Assets/Modules/Database/.Schema/v1/Objects/Component.xml#L13)). It is still the placement rule for 260 of 264 components ([ShipLayoutModel.cs:174](../../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipLayoutModel.cs#L174), [ShipLayoutObsolete.cs:184](../../Assets/Modules/ShipConstructor/Scripts/ShipLayoutObsolete.cs#L184)). Hand-written partials keep it alive in the game ([Component.cs:14](../../Assets/Modules/Database/Scripts/DataModel/Component.cs#L14)) and in the editor ([Component.cs](../../Assets/Modules/Database/.Editor/DatabaseModel/DatabaseModel/DataModel/Component.cs)).
- `DroneBay.ImprovedAi` is obsolete, but `DroneBay` still reads it ([DroneBay.cs:32](../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/DroneBays/DroneBay.cs#L32)), and `ControllerFactory.CreateDroneController` uses it to pick the AI ([ControllerFactory.cs:69](../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ControllerFactory.cs#L69)). The editor cannot write it.

**Effect.** Readers of the schema remove or ignore fields that control gameplay. Mod authors cannot set `ImprovedAi`.

**Direction.** Remove the `obsolete` option from both fields, or replace them with new fields and a migration.

### DB10. Two schema members share one JSON field

**Severity:** Low. **Status:** Confirmed.

**Problem.** `Component.xml` declares the JSON field `AmmunitionId` twice, once for `Ammunition` and once for `AmmunitionObsolete` ([Component.xml:16](../../Assets/Modules/Database/.Schema/v1/Objects/Component.xml#L16)).

**Evidence.** The loader resolves the ID against both tables. This works only because the built-in IDs do not overlap (56 and 81 records). The generated editor `Save` writes the field twice. A hand-written `OnDataSerialized` fixes this ([Component.cs:14](../../Assets/Modules/Database/.Editor/DatabaseModel/DatabaseModel/DataModel/Component.cs#L14)).

**Effect.** A mod whose `Ammunition` and `AmmunitionObsolete` IDs overlap gets both on one component.

**Direction.** Convert the remaining `AmmunitionObsolete` records to `Ammunition` with a migration, then remove the second member.

### DB11. Schema fields without runtime effect

**Severity:** Low. **Status:** Confirmed.

**Problem.** These schema fields, tables, and types have no consumer in the game:

- `CombatRules.NextEnemyButton`. The button uses `CombatManager.CanCallNextEnemy` instead.
- `Character.Faction`, `Inventory`, `Fleet`, `Relations`, `IsUnique`. The game reads only `Name` and `AvatarIcon`. Session relations start at 0, not at `Character.Relations`.
- `DatabaseSettings.ModVersion`, `ModId`, and `ModName`. `ModName` only feeds `IDatabase.Name` ([Database.cs:26](../../Assets/Modules/Database/Scripts/Database.cs#L26)), which nothing reads. The mod list and the save key use the name and ID of the storage (the `id` file or the packed header).
- `SatelliteBuild.NotAvailableInGame`, `AmmunitionObsolete.InitialPosition`.
- `ShipModSettings.RemoveUnlimitedRespawnMod`. The code that reads it is commented out ([ModificationFactory.cs:64](../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/ModificationFactory.cs#L64)).
- Tables `StatUpgradeTemplate` and `ComponentStatUpgrade`, enum `SkillType`, structure `ShipToValue`.

**Effect.** Mod authors set values that do nothing.

**Direction.** Mark each one obsolete in the schema, or implement it. The schema pages document them as unused.

### DB12. The loader does not detect reference cycles

**Severity:** Low. **Status:** Plausible.

**Where.** The loader registers a record before it resolves its references, so cycles load. Some consumers then recurse without a visited set: `TechnologyExtension.GetWorkshopLevel` ([ITechnology.cs:104](../../Assets/Scripts/Domain/Technologies/ITechnology.cs#L104)), `Research.IsAvailabe` ([Research.cs:150](../../Assets/Scripts/Domain/Research/Research.cs#L150)), and behavior tree `SubTree` nodes ([NodeBuilder.cs:82](../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/NodeBuilder.cs#L82)).

**Failure.** A cycle in mod data causes a stack overflow. Built-in data has no cycles.

**Fix.** Detect cycles in technology dependencies and behavior tree subtrees after load, and reject the mod with a clear error.

### DB13. Mod discovery reads every mod at startup

**Severity:** Low. **Status:** Confirmed.

**Problem.** `LookForMods` constructs a storage for every mod at startup ([Database.cs:51](../../Assets/Modules/Database/Scripts/Database.cs#L51)).

**Evidence.**
- `FolderDatabaseStorage.TryFindDatabaseVersion` reads and parses every JSON file with Newtonsoft ([FolderDatabaseStorage.cs:35](../../Assets/Modules/Database/Scripts/Storage/FolderDatabaseStorage.cs#L35)).
- `FileDatabaseStorage` unwraps the RSA key and decrypts the whole file with AES-GCM to read the header ([FileDatabaseStorage.cs:171](../../Assets/Modules/Database/Scripts/Storage/FileDatabaseStorage.cs#L171)).
- Both repeat the work when the mod loads.
- `ResourceDatabaseStorage` calls `AssetDatabase.Refresh()` on every Editor load ([ResourceDatabaseStorage.cs:24](../../Assets/Modules/Database/Scripts/Storage/ResourceDatabaseStorage.cs#L24)).

**Effect.** Startup time grows with the number and size of installed mods.

**Direction.** Read only the name, ID, and version during discovery: a small header file for folder mods, and an unencrypted header for packed mods.

### DB14. Quest fleets filter by hull faction

**Severity:** Low. **Status:** Plausible.

**Where.** `FleetFactory` filters random ships by `item.Ship.Faction`, the hull faction ([FleetFactory.cs:66](../../Assets/Modules/Quests/Scripts/Factory/FleetFactory.cs#L66)). `ShipBuildQuery.BelongToFaction` uses `ShipBuild.Faction`, where the build faction overrides the hull faction ([ShipBuildQuery.cs:68](../../Assets/Modules/Database/Scripts/Query/ShipBuildQuery.cs#L68)).

**Failure.** Builds with `BuildFaction` appear in quest fleets of the hull faction, not of the build faction.

**Fix.** Use `ShipBuild.Faction` in `FleetFactory`.

### DB15. Smaller defects

| ID | Where | Problem | Status |
| --- | --- | --- | --- |
| DB15.1 | [ShipBuildQuery.cs:144](../../Assets/Modules/Database/Scripts/Query/ShipBuildQuery.cs#L144) | `ShipType != Common \|\| ShipType != Flagship` is always true, so `HiddenShips()` is empty and `Fleet.Xmas` gets no hidden ships. | Confirmed |
| DB15.2 | [ShipEquipmentStats.cs:100](../../Assets/Modules/ShipConstructor/Scripts/Model/ShipEquipmentStats.cs#L100) | `FromComponent` does not multiply `EngineEnergyConsumption` by the per-cell multiplier. | Confirmed |
| DB15.3 | [CommonComponent.cs:45](../../Assets/Modules/ShipConstructor/Scripts/Component/CommonComponent.cs#L45) | `PassiveEnergyConsumption` uses the unscaled drone bay value. `GetStats` scales it by ship size ([line 29](../../Assets/Modules/ShipConstructor/Scripts/Component/CommonComponent.cs#L29)). Only `RequiresEnergyToInstall` reads the property, and only its sign ([IComponent.cs:40](../../Assets/Modules/ShipConstructor/Scripts/Component/IComponent.cs#L40)). The sign can differ only for a drone bay component with a positive energy recharge. No built-in record has one. | Confirmed (latent) |
| DB15.4 | [DatabaseContent.cs:23](../../Assets/Modules/Database/Scripts/GeneratedGameCode/Storage/DatabaseContent.cs#L23) | `_allowDuplicates` is always true after construction. The first record with an ID wins, and later duplicates are skipped silently. | Confirmed |
| DB15.5 | [CompositeEffect.cs:223](../../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/CompositeEffect.cs#L223) | `UpdateEffectLife` and `UpdateLoopingEffectLife` ([line 213](../../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/CompositeEffect.cs#L213)) divide by `VisualEffectElement.Lifetime`. The schema allows 0. A non-looping element with 0 is visible only when the timeline time equals `StartTime` exactly, so it almost never shows. The `Shake` element of built-in effect 9 (`ShockWave`) has 0 and never shakes the camera. A looping element with 0 gets a NaN life. | Confirmed |
| DB15.6 | [ShipBuilder.cs:198](../../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L198) | Throws `companions should have one barrel`. The schema allows a list of barrels on `Satellite`. | Confirmed (latent) |
| DB15.7 | [InitializationState.cs:127](../../Assets/Scripts/GameStateMachine/States/InitializationState.cs#L127) | `tech.Dependencies.IndexOf(null)` can never find a null, because the loader throws on missing dependencies. Dead check. | Confirmed |
| DB15.8 | [ResourceLocator.cs:95](../../Assets/Modules/ResourceLocator/Scripts/ResourceLocator.cs#L95) | `GetAudioClip` has no null check on `_database`, unlike `GetSprite`. | Confirmed |
| DB15.9 | [FolderDatabaseStorage.cs:63](../../Assets/Modules/Database/Scripts/Storage/FolderDatabaseStorage.cs#L63) | Folder mods key images with the extension, but audio and localization without it. The editor keys images without the extension ([editor.md](../database/editor.md)). Which key the packed format and JSON sprite names expect is not verified. | Plausible |
| DB15.10 | Several files | Hard-coded record IDs (Device 18, QuestItem 5/6/7/11/12/13/14/16/25, Component 52/65/76/85/96/213/215/219/220, ShipBuild 232/226/218, Technology 264/277–282) break mods that use `UnloadOriginalDatabase`. See [issue 9](application_layer.md#9-game-rules-are-in-code-instead-of-the-database). | Confirmed |
| DB15.11 | [Faction.cs:10](../../Assets/Modules/Database/Scripts/DataModel/Faction.cs#L10) | `Faction.Empty` is built by a private constructor that does not run `OnDataDeserialized`. Its `HomeStarRange` and `WanderingShipsRange` stay `0…0`. A ship build with a neutral build faction and a neutral hull never passes `ShipBuildQuery.IsFactionValid` at a level above 0. No built-in enemy build is neutral. | Confirmed (latent) |
| DB15.12 | [Region.cs:113](../../Assets/Scripts/Legacy/Game/Galaxy/Region.cs#L113) | When no faction has territories at a region's home-star level, `RandomElement` returns `null` and the next line throws `NullReferenceException`. Built-in factions 1, 2, and 6 have no territory bounds. | Confirmed (latent) |
| DB15.13 | [TechTreePanelViewModel.cs:296](../../Assets/Scripts/Legacy/GUI/ViewModel/StarMap/TechTreePanelViewModel.cs#L296) | The tree layout only places technologies of one faction that are not `Special`. A technology that depends on another faction's technology or on a `Special` one is never placed. The loop runs 1000 times, logs `BuildTechTree: bad tech data`, and calls `Debug.Break` in the Editor. No validator checks this rule. Built-in data has no such dependency. | Confirmed (latent) |
| DB15.14 | [DroneBay.cs:37](../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/DroneBays/DroneBay.cs#L37) | `ShipBuilder(ShipBuild)` multiplies damage, armor, and shield by `1 + 0.5 × DifficultyClass` ([ShipBuilder.cs:22](../../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L22)). The only caller, `DroneBay`, then assigns all three multipliers, so the class has no effect on drone stats. Built-in drone builds 139, 374, and 375 have class 1. | Confirmed |
| DB15.15 | [LayoutModifications.cs:122](../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/LayoutModifications.cs#L122) | `FullyUpgrade` (used by `ShipBuild.ExtendedLayout`) converts cells without raising `_addedCellCount`. The cells add no per-cell armor or weight, and `Serialize` writes nothing, so a player ship loses them on reload. Built-in builds with the flag are enemy-only. | Confirmed (latent) |
| DB15.16 | [Fleet.cs:49](../../Assets/Scripts/Legacy/Model/Factories/Fleet.cs#L49) | `Fleet.Boss` reads `boss.Faction` without a null check. When no flagship build of class 1 (or 2 above level 50) fits the faction or the level, `Random` returns `null` and the factory throws. | Confirmed (latent) |
| DB15.17 | [ShipValidator.cs:20](../../Assets/Scripts/Domain/Player/ShipValidator.cs#L20) | `HasForbiddenComponents` rejects player ships with `Availability.None` components, unlocked `Special` components, and upgraded components. Nothing calls it. The only live check is the arena rule against `None` ([ShipValidator.cs:11](../../Assets/Modules/ShipConstructor/Scripts/Ships/ShipValidator.cs#L11)). A player can install `None` and `Special` components that loot or quests give. | Confirmed |
| DB15.18 | [ShipBuilder.cs:93](../../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L93) | `Build` counts the group tag and `MaxComponentAmount` before it checks compatibility ([line 112](../../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L112)). An incompatible component that is skipped still uses up the limit, so a later valid component of the group is skipped too. The ship editor checks in the right order, so only data-defined builds are affected. | Confirmed (latent) |
| DB15.19 | [PrefabCache.cs:153](../../Assets/Modules/BattleSimulator/Scripts/Combat/Services/PrefabCache.cs#L153) | `GetEffectPrefab` caches one element prefab per `Type` and `Image`. The initializers of `Spark`, `Smoke`, and `Lightning` elements also store `Size`, `TurnRate`, `ParticleSize`, `Color`, and `ColorMode` in the prefab ([SparkEffect.cs:24](../../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/SparkEffect.cs#L24), [SmokeEffect.cs:28](../../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/SmokeEffect.cs#L28), [LightningEffect.cs:41](../../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/LightningEffect.cs#L41)). A second element with the same type and image gets the values of the element that was created first after the database load. Built-in effects 15 (smoke `CloudWhite`), 12 and 19 (lightning `beam`), and 9, 13, and 14 (lightning strike `beam`) differ in these values. | Confirmed |
| DB15.20 | [NodeBuilder.cs:72](../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/NodeBuilder.cs#L72) | `IdentifiersMap` is a mutable struct ([Collections.cs:5](../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/Utils/Collections.cs#L5)) stored in `readonly` fields ([NodeBuilder.cs:19](../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/NodeBuilder.cs#L19), [MessageHub.cs:16](../../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/Utils/MessageHub.cs#L16)). C# calls `GetMessageId` on a copy, so the dictionary is never kept and every name gets ID 0. All behavior-tree variables share one value, all saved-target slots share one target, and all messages are one message. Built-in trees use one variable name and one message name. | Confirmed (latent) |
| DB15.21 | [XmasTree.cs:48](../../Assets/Scripts/Domain/Galaxy/StarContent/XmasTree.cs#L48) | Assigns `SpecialEventSettings.XmasCombatRules` without the `CombatSettings.DefaultCombatRules` fallback that every other battle type uses. With `0`, `CombatModelBuilder.Build` throws `NullReferenceException` ([CombatModelBuilder.cs:52](../../Assets/Scripts/Combat/Domain/CombatModelBuilder.cs#L52)). Built-in data sets 8. | Confirmed (latent) |
| DB15.22 | [QuestGiver.cs:46](../../Assets/Modules/Quests/Scripts/QuestGiver/QuestGiver.cs#L46) | `GetRandomFactionBase` builds the list of nearby regions on the first call and keeps it. Later calls ignore the new center star. The quest giver lives for the whole session ([QuestManager.cs:126](../../Assets/Modules/Quests/Scripts/Manager/QuestManager.cs#L126)), so a `RandomFactionBase` origin always searches around the star of the first check after the session starts. Built-in daily quest 200 uses this origin. | Confirmed |
| DB15.23 | [Fleet.xml:7](../../Assets/Modules/Database/.Schema/v1/Objects/Quests/Fleet.xml#L7) | The obsolete `Fleet` fields `LootCondition`, `ExpCondition`, `CombatTimeLimit`, `NoShipChanging`, and `PlayerHasOneShip` have no consumer and no migration to `CombatRules`. Built-in fleets 1 and 10 set `LootCondition` `Never` (fleet 1 also `ExpCondition` `Never`), have no `CombatRules`, and get the default rules, so their battles give loot and experience. | Confirmed |
| DB15.24 | [NodeBuilder.cs:101](../../Assets/Modules/Quests/Scripts/Factory/NodeBuilder.cs#L101) | `Create(Node_Condition)` calls `Transitions.First()` and throws on an empty list. `Create(Node_ChangeCharacterRelations)` and `Create(Node_SetCharacterRelations)` read `content.Character.Id` without a null check ([line 266](../../Assets/Modules/Quests/Scripts/Factory/NodeBuilder.cs#L266), [line 274](../../Assets/Modules/Quests/Scripts/Factory/NodeBuilder.cs#L274)). `CharacterRelationsRequirement.IsMet` does the same ([CharacterRelationsRequirement.cs:29](../../Assets/Modules/Quests/Scripts/Requirements/CharacterRelationsRequirement.cs#L29)). The exception leaves the quest manager's event handler. No built-in node has this data. | Confirmed (latent) |
| DB15.25 | [RequirementsFactory.cs:25](../../Assets/Modules/Quests/Scripts/Factory/RequirementsFactory.cs#L25) | `CreateForQuest` builds `Quest.Requirement` with `new QuestInfo(seed)`: star 0, quest ID 0. `FactionRelations`, `FactionStarbasePower`, and `StarbaseCaptured` then check the current star, `ComeToOrigin` and `RandomStarSystem` check star 0, and the time requirements read quest 0. Only `Faction` and `IsHostileFaction` check the candidate quest star, through `CanStart` ([FactionRequirement.cs:16](../../Assets/Modules/Quests/Scripts/Requirements/FactionRequirement.cs#L16)). No built-in quest uses these types at start. | Confirmed (latent) |
| DB15.26 | [RandomNode.cs:25](../../Assets/Modules/Quests/Scripts/Nodes/RandomNode.cs#L25) | `GetRequirementsText` returns `Message` without localization, unlike `SwitchNode` and `ConditionNode` ([SwitchNode.cs:24](../../Assets/Modules/Quests/Scripts/Nodes/SwitchNode.cs#L24)). A `$` key shows as raw text in the quest log. The built-in random node has no message. | Confirmed (latent) |
| DB15.27 | [WriteDeserializationCode.tt:94](../../Assets/Modules/Database/.CodeGenerator/DatabaseCodeGenerator/GameCode/Templates/WriteDeserializationCode.tt#L94) | The generator reads expression bounds with `MinMaxInt`, which uses `int.TryParse` ([Extensions.cs:7](../../Assets/Modules/Database/.CodeGenerator/DatabaseCodeGenerator/Schema/Extensions.cs#L7)). A fractional bound is dropped silently. `SkillSettings.FlightRange` has `minvalue="1.5"`, and the generated expression has no lower bound ([SkillSettings.cs:45](../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/SkillSettings.cs#L45)). The built-in formula never goes below 1.5. | Confirmed (latent) |
| DB15.28 | [MainMenu.cs:162](../../Assets/Scripts/Gui/MainMenu/MainMenu.cs#L162) | `OnDatabaseLoaded` hides the credits text when `UiSettings.NoCreditsText` is true and never shows it again. The main menu stays open across mod changes, so after the player switches from such a mod to a catalog without the flag, the credits stay hidden until the game restarts. | Confirmed |
| DB15.29 | [DatabaseContent.cs:23](../../Assets/Modules/Database/Scripts/GeneratedGameCode/Storage/DatabaseContent.cs#L23) | The constructor sets `_allowDuplicates = true` only after it loads its storage. [`Database.LoadContent`](../../Assets/Modules/Database/Scripts/Database.cs#L180) passes a current-version mod to the constructor, so a duplicate ID or a second settings record in that mod throws and the mod does not load. The same mod at an older version goes through the upgrader into a content built without a storage, so the duplicate is skipped silently. A mod that loads at 1.6 can stop loading after the author raises it to 1.7. | Confirmed (latent) |
