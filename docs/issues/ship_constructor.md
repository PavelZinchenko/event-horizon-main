# ShipConstructor Module Issues

This file records problems in `Assets/Modules/ShipConstructor/`. For the design, see [ShipConstructor module](../ship_constructor.md). Related issues in other files are not repeated here:

| ID | Topic |
| --- | --- |
| [DB1](database.md#db1-the-unlimitedrespawn-perk-crashes-ship-creation) | A ship build with the `UnlimitedRespawn` perk throws in `ModificationFactory.Create`. |
| [DB15.2](database.md#db15-smaller-defects) | `FromComponent` does not scale `EngineEnergyConsumption` per cell. |
| [DB15.3](database.md#db15-smaller-defects) | `PassiveEnergyConsumption` uses the unscaled drone bay value. |
| [DB15.6](database.md#db15-smaller-defects) | `ShipBuilder` throws for a satellite with more than one barrel. |
| [DB15.10](database.md#db15-smaller-defects) | Hard-coded IDs, including device 18 of the `Infected` perk. |
| [DB15.14](database.md#db15-smaller-defects) | The drone bay overwrites the difficulty bonus of `ShipBuilder(ShipBuild)`. |
| [DB15.15](database.md#db15-smaller-defects) | `FullyUpgrade` does not count the added cells. |
| [DB15.18](database.md#db15-smaller-defects) | `ShipBuilder` counts limits before the compatibility check. |
| [SE10](ship_editor.md#se10-placement-rules-live-in-the-ui-module-and-have-copies) | Placement rules live in ShipEditor; `ShipLayoutObsolete` has a copy. |
| [SE11](ship_editor.md#se11-statistics-duplicate-shipbuilder-and-omit-combat-bonuses) | The editor statistics omit combat bonuses. |
| [GS5.14](game_session.md#gs5-smaller-defects) | Ship components are saved with upgrade level 0. |

The analysis was done by reading the code on 2026-10-12. No issue was reproduced in Play Mode. Each issue has a severity (High, Medium, or Low) and a status:

- **Confirmed** – the code path is certain. The failure follows directly from the code.
- **Confirmed (latent)** – the code path is certain, but current data or current UI does not trigger it.
- **Plausible** – the code is wrong, but the failure needs specific data or an old save.

Defects use Where / Failure / Fix. Structural issues use Problem / Evidence / Direction.

| ID | Severity | Status | Summary |
| --- | --- | --- | --- |
| [SC1](#sc1-a-saved-perk-without-a-factory-case-deletes-the-ship) | Medium | Plausible | An unknown saved perk type removes the ship from the fleet. |
| [SC2](#sc2-changing-an-expanded-cell-counts-it-again) | Low | Confirmed (latent) | Changing an expanded cell counts it twice. |
| [SC3](#sc3-random-build-colours-use-the-global-unity-generator) | Low | Confirmed | Random build colours ignore the fleet seed. |
| [SC4](#sc4-arena-ships-drop-experience-and-colour) | Low | Confirmed | Arena ships fight at level 0 with the default colour. |
| [SC5](#sc5-smaller-defects) | Low | Mixed | Smaller defects. |
| [SC6](#sc6-component-upgrades-are-wired-but-inert) | — | Structural | Component upgrades are wired but change nothing. |
| [SC7](#sc7-dead-and-duplicate-code) | — | Structural | Dead fields, unused methods, duplicate rules. |
| [SC8](#sc8-names-collide-across-namespaces) | — | Structural | Duplicate class names and typos. |
| [SC9](#sc9-the-module-holds-unrelated-game-formulas) | — | Structural | Economy and galaxy formulas live here. |

## Defects

### SC1. A saved perk without a factory case deletes the ship

**Severity:** Medium. **Status:** Plausible.

**Where.** `ShipInfo.ToShip` deserializes every saved perk with `ShipModificationExtensions.Deserialize` ([ShipInfo.cs:58](../../Assets/Modules/GameSession/Scripts/Model/ShipInfo.cs#L58), [ShipModificationExtensions.cs:17](../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/ShipModificationExtensions.cs#L17)). `ModificationFactory.Create` throws `InvalidEnumArgumentException` for `UnlimitedRespawn` (7) and for any value above 11 ([ModificationFactory.cs:48](../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/ModificationFactory.cs#L48)). `PlayerFleet.Load` catches the exception, logs it, and drops the ship ([PlayerFleet.cs:118](../../Assets/Scripts/Domain/Player/PlayerFleet.cs#L118)).

**Failure.**
1. A save holds a ship with perk type 7. The `UnlimitedRespawn` perk existed in earlier versions (the type, the icon, and `ShipModSettings.RemoveUnlimitedRespawnMod` remain). A save from a newer version with a new perk type has the same effect.
2. On load, the ship, its components, and its satellites are not added to the fleet. A hangar slot that pointed to it becomes empty.
3. The next save writes the fleet without the ship. The loss is permanent.

[DB1](database.md#db1-the-unlimitedrespawn-perk-crashes-ship-creation) covers the same factory gap for database builds. This issue is the save path.

**Fix.** In `Deserialize`, return an empty perk slot (`EmptyModification`) for a type that the factory does not know, and log a warning. Keep the slot so the ship keeps its quality.

### SC2. Changing an expanded cell counts it again

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** `CustomLayout.IsCustomizable` is true for any cell whose stock cell is empty and whose current cell is not empty ([LayoutModifications.cs:283](../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/LayoutModifications.cs#L283)). An expanded cell therefore passes `IsValidModification` again. `TryModifyCell` increments `_addedCellCount` on every change ([line 175](../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/LayoutModifications.cs#L175)).

**Failure.** A second `TryAddCell` on the same cell raises `CellCount` and `ExtraCells()` by one more. The ship gains per-cell armor and weight for a cell that does not exist, and the upgrade panel prices the next cell too high. A reload recounts correctly, so the stats change after the reload.

The ship service panel lets the player select only `*` cells ([ShipLayoutPanel.cs:63](../../Assets/Scripts/Gui/ShipService/ShipLayoutPanel.cs#L63)), so the UI does not trigger it.

**Fix.** Increment the count only when the previous cell was `*`. Or reject cells that are already expanded.

### SC3. Random build colours use the global Unity generator

**Severity:** Low. **Status:** Confirmed.

**Where.** `CommonShip(ShipBuild)` sets a random hue and saturation with `UnityEngine.Random.value` when `RandomColor` is set ([CommonShip.cs:38](../../Assets/Modules/ShipConstructor/Scripts/Ships/CommonShip.cs#L38)). Enemy fleets create their ships with a seeded `System.Random` ([CommonFleet.cs:28](../../Assets/Scripts/Legacy/Model/Military/CommonFleet.cs#L28)), but this colour does not use it.

**Failure.** The same enemy fleet gets different colours each time it is created. The call also advances the global generator, which star drawing reseeds ([CC3](common_components.md#cc3-getstarposition-reseeds-the-global-unity-random-generator)). The effect is cosmetic; stats do not change.

`ModificationFactory.Create` also draws perk seeds from an unseeded `System.Random` ([ModificationFactory.cs:16](../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/ModificationFactory.cs#L16)). No perk reads its seed, so this has no effect today.

**Fix.** Pass the fleet's `System.Random` into the build constructor. This changes the colours of existing seeded fleets; ask the owner before the change.

### SC4. Arena ships drop experience and colour

**Severity:** Low. **Status:** Confirmed. Blocked by [SCQ1](#open-questions).

**Where.** `ArenaShip` copies the name, components, and satellites of the source ship ([ArenaShip.cs:8](../../Assets/Modules/ShipConstructor/Scripts/Ships/ArenaShip.cs#L8)). It does not copy `Experience` or `ColorScheme`.

**Failure.**
1. `BaseShip.CreateBuilder` multiplies armor, shield, damage, and ramming by `Experience.PowerMultiplier`. For an arena ship the level is 0, so the multiplier is 1. A level-100 ship loses its ×11 bonus.
2. `ArenaFleet.Power` uses `Threat.GetShipPower`, which also reads the level. Arena threat values ignore levels.
3. Every arena ship shows the default colour.

If arena equalization is intended, only the colour is a defect.

**Fix.** After the question is answered: copy `ColorScheme.Value` always; copy `Experience` if levels should count.

### SC5. Smaller defects

| ID | Where | Defect | Status |
| --- | --- | --- | --- |
| SC5.1 | [IntegratedComponent.cs:23](../../Assets/Modules/ShipConstructor/Scripts/IntegratedComponent.cs#L23) | `ToString` throws `NotSupportedException`. String interpolation, logging, and the debugger display fail on an installed component. | Confirmed |
| SC5.2 | [ShipBuilder.cs:290](../../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L290) | The `WeaponPlatform` constructor sets damage, fire rate, and energy multipliers to 1 but leaves `RangeMultiplier` at 0. `Build` always calls `ApplyStats`, which sets it. Any other use of a new platform gets zero range. | Confirmed (latent) |
| SC5.3 | [ShipStatsCalculator.cs:166](../../Assets/Modules/ShipConstructor/Scripts/Model/ShipStatsCalculator.cs#L166) | The four resistance percentages divide by `ArmorPoints + R`. With armor 0 and resistance 0, the result is `NaN`. `ArmorPoints` is 0 when the base armor sum is below 1. | Plausible |
| SC5.4 | [ShipStatsCalculator.cs:229](../../Assets/Modules/ShipConstructor/Scripts/Model/ShipStatsCalculator.cs#L229) | `DroneBuildSpeed` divides by the total build-time multiplier. A total build-time bonus of −100 % gives infinity. | Plausible |
| SC5.5 | [ShipStatsCalculator.cs:203](../../Assets/Modules/ShipConstructor/Scripts/Model/ShipStatsCalculator.cs#L203) | `EnergyAbsorption` uses only the armor bonus from `ShipBonuses`. Resistances also use the hull and perk armor multiplier. An `Infected` hull therefore keeps full absorption but loses resistance. | Plausible |
| SC5.6 | [Threat.cs:32](../../Assets/Modules/ShipConstructor/Scripts/Maths/Threat.cs#L32) | `GetLevel(0, 0)` computes `0 / 0`. `NaN` fails every comparison, so the result is `VeryHard`. | Confirmed (latent) |
| SC5.7 | [ShipExtensions.cs:115](../../Assets/Modules/ShipConstructor/Scripts/Extensions/ShipExtensions.cs#L115) | Enemy satellites are chosen with `SuitableFor(data.Ship)`, the database hull. A build with the `SatelliteSize` perk never gets the larger satellites that the perk allows. | Confirmed |
| SC5.8 | [ArenaShip.cs:21](../../Assets/Modules/ShipConstructor/Scripts/Ships/ArenaShip.cs#L21) | The `Name` setter calls `BaseShip.Name`, which throws `InvalidOperationException`. No caller sets it today. | Confirmed (latent) |
| SC5.9 | [ShipLayoutObsolete.cs:139](../../Assets/Modules/ShipConstructor/Scripts/ShipLayoutObsolete.cs#L139) | `GetComponent` checks `id > Count` instead of `id >= Count`. `id == Count` throws. No caller. | Confirmed (latent) |
| SC5.10 | [ShipLayoutObsolete.cs:197](../../Assets/Modules/ShipConstructor/Scripts/ShipLayoutObsolete.cs#L197) | `LayoutIndices` yields −1 for a cell outside the layout, then also yields the computed index (no `continue`). Callers stop at −1, so the second value is never used. | Confirmed (latent) |
| SC5.11 | [ShipLayoutObsolete.cs:210](../../Assets/Modules/ShipConstructor/Scripts/ShipLayoutObsolete.cs#L210) | `UpdateBarrelData` indexes `_layout[barrel.PositionInLayout]` without a bounds check. A mod satellite with a barrel position outside the layout throws in `InitializationState` when the database is editable. | Plausible |
| SC5.12 | [ComponentInfo.cs:304](../../Assets/Modules/ShipConstructor/Scripts/ComponentInfo.cs#L304) | `ComponentInfo.Empty` is a public static field, not `readonly`. Any code can replace it. | Confirmed (latent) |
| SC5.13 | [SatelliteExtensions.cs:15](../../Assets/Modules/ShipConstructor/Scripts/Satellites/SatelliteExtensions.cs#L15) | `CreateCopy` shares the `IntegratedComponent` objects with the source. `ArenaShip` also shares the hull components. A change of key, behaviour, or lock on the copy changes the player ship. Combat does not change them today. | Confirmed (latent) |

## Structural issues

### SC6. Component upgrades are wired but inert

**Problem.** The module declares component upgrades, but no code applies them. An upgrade level changes the component's name and identity only.

**Evidence.**
- `IComponent.Upgrades` is set by the ShipEditor component panel ([ComponentItem.cs:87](../../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentItem.cs#L87)). `CommonComponent` stores it and never reads it ([CommonComponent.cs:149](../../Assets/Modules/ShipConstructor/Scripts/Component/CommonComponent.cs#L149)). No class implements `IComponentUpgrades`.
- Both upgrade providers return `null` ([ShipEditorState.cs:101](../../Assets/Scripts/GameStateMachine/States/ShipEditorState.cs#L101), [DatabaseEditorContext.cs:127](../../Assets/ModulesShared/ShipEditor/Scripts/Context/DatabaseEditorContext.cs#L127)).
- `ComponentInfo.Level` (0–20) adds `+N` to the name ([ComponentInfo.cs:113](../../Assets/Modules/ShipConstructor/Scripts/ComponentInfo.cs#L113)) and takes part in `Equals`. Two items that differ only by level do not stack in the inventory. Ship components are saved with level 0 ([GS5.14](game_session.md#gs5-smaller-defects)).
- The schema has `ComponentStatUpgrade` ([schema](../database/schema/ComponentStatUpgrade.md)), and `ComponentUpgradeLevel` refers to it.

**Direction.** Decide ([SCQ2](#open-questions)). To remove: delete `IComponent.Upgrades`, the provider interface, and the level from equality. To finish: implement `IComponentUpgrades` from `ComponentStatUpgrade`, call it in every `CommonComponent` stat getter, and save the level for ship components.

### SC7. Dead and duplicate code

**Problem.** Several fields and methods have no effect, and some rules exist twice. Readers cannot tell which copy is live.

**Evidence.**
- `ShipBuilder.SizeMultiplier` is copied to `ShipStatsCalculator.SizeMultiplier` ([ShipBuilder.cs:75](../../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L75)). No code sets the builder value, and no code reads the stats value.
- `ShipEquipmentStats.ArmorRepairBaseCooldown`, `EnergyRechargeBaseCooldown`, and `ShieldRechargeBaseCooldown` are never assigned, and `HullRepairBaseCooldown` is never read ([ShipEquipmentStats.cs:22](../../Assets/Modules/ShipConstructor/Scripts/Model/ShipEquipmentStats.cs#L22)). The cooldown formulas add them as 0 ([ShipStatsCalculator.cs:162](../../Assets/Modules/ShipConstructor/Scripts/Model/ShipStatsCalculator.cs#L162)), so components change a cooldown only through its multiplier.
- `DeviceClassExtension.IsSuitable` ([DeviceClass.cs:8](../../Assets/Modules/ShipConstructor/Scripts/DeviceClass.cs#L8)) duplicates `CompatibilityChecker.IsCompatibleDevice` and has no caller. `IsCompatibleWeapon` always returns true ([CompatibilityChecker.cs:44](../../Assets/Modules/ShipConstructor/Scripts/Component/CompatibilityChecker.cs#L44)).
- `EnergyRechargeCooldownMod.IsSuitable` and `ShieldRechargeCooldownMod.IsSuitable` copy the weapon-class rule of `WeaponClassMod` and are never called ([EnergyRechargeCooldownMod.cs:15](../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/EnergyRechargeCooldownMod.cs#L15), [ShieldRechargeCooldownMod.cs:15](../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/ShieldRechargeCooldownMod.cs#L15)).
- `ModificationFactory.Create(ExtraBlueCells)` returns an empty slot ([ModificationFactory.cs:31](../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/ModificationFactory.cs#L31)). No perk ever has type `ExtraBlueCells`, so the arena check for it is dead ([ShipValidator.cs:71](../../Assets/Modules/ShipConstructor/Scripts/Ships/ShipValidator.cs#L71)). `IsForbiddenOnArena` always returns false ([line 85](../../Assets/Modules/ShipConstructor/Scripts/Ships/ShipValidator.cs#L85)).
- `ShipModificationExtensions.CreateRandomModifications` has no caller. It would include `Empty`, `ExtraBlueCells`, and `UnlimitedRespawn`, and throw on the last ([ShipModificationExtensions.cs:25](../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/ShipModificationExtensions.cs#L25)).
- Unused: `ComponentInfo.FromItemQuality`, `ComponentExtensions._separator`, `WeaponDamageCalculator.MinCooldown`, `ShipExtensions.RandomizeColor`, `Unlocked`, `OfLevel`, `CreateCopy` (only commented-out callers).
- `ShipLayoutObsolete` is a third copy of the placement rules ([SE10](ship_editor.md#se10-placement-rules-live-in-the-ui-module-and-have-copies)). Its only caller checks satellite builds in `InitializationState`. It ignores expanded cells and maps barrels with its own algorithm.
- Commented-out code: the old `ActivationType` and `SizeClass` enums, `GetWeaponData`, `IsHoming`, and starbase checks.

**Direction.** Delete the dead members. Replace `ShipLayoutObsolete` in `InitializationState` with ShipEditor's `ShipLayoutModel` (which `ShipValidator` in the application already uses).

### SC8. Names collide across namespaces

**Problem.** Several classes share a name with another class. Code search and `using` lists pick the wrong one easily.

**Evidence.**

| Name | Classes |
| --- | --- |
| `ShipValidator` | `Constructor.Ships.ShipValidator` (arena, viability); `Domain.Shipyard.ShipValidator` (layout, [ShipValidator.cs](../../Assets/Scripts/Domain/Player/ShipValidator.cs)) |
| `EmptyModification` | `Constructor.Modification.EmptyModification` (component); `Constructor.Ships.Modification.EmptyModification` (perk slot) |
| `ComponentExtensions` | `Constructor.ComponentExtensions` (`FromDatabase`, obsolete packing); `Constructor.Component.ComponentExtensions` (`RequiresEnergyToInstall`) |
| `ShipInfo` | `Constructor.ShipInfo` (builder output); GameSession `ShipInfo` (save struct); `Combat.Domain.ShipInfo` |
| `ModificationFactory` | Global namespace, unlike every other class |

Misspelled public names: `ClonningCenters`, `EnginePowerWihoutEnergy`, `TurnRateWihoutEnergy`, `ShipLayoutExtensison`, `CellTypeConveter`, `RemoveAllComopnents` (ShipEditor).

**Direction.** Rename in one pass with the IDE: `ArenaShipValidator`, `EmptyComponentModification` / `EmptyShipPerk`, `ShipSpecInfo`. Move `ModificationFactory` into `Constructor.Ships.Modification`. Fix the spellings. None of these names is serialized.

### SC9. The module holds unrelated game formulas

**Problem.** The module mixes ship construction with economy and galaxy rules.

**Evidence.** `Maths.Distance` maps star level to fleet size, credits, AI level, and combat time. `Maths.Threat` rates fleets. `ShipExtensions` holds ship price, scrap value, sale rule, and enemy fleet generation. `Maths.Experience` is ship progression. `Maths.Format` formats UI numbers. They live here only because they read `IShip`.

**Direction.** Low priority. When the economy or galaxy code moves to its own module, move `Distance`, `Threat`, price, scrap, and `Create` with it. Keep `Experience` next to `IShip`.

## Open questions

These questions wait for a design decision. Do not fix the linked issues until the owner answers.

| ID | Question | Blocks | Status |
| --- | --- | --- | --- |
| SCQ1 | Should arena ships fight at their real level, or at level 0 for fairness? | [SC4](#sc4-arena-ships-drop-experience-and-colour) | Open |
| SCQ2 | Should component upgrades be implemented or removed? | [SC6](#sc6-component-upgrades-are-wired-but-inert) | Open |
