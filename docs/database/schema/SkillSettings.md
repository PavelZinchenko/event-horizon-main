# SkillSettings

Skill settings turn the player's skill levels into bonuses: combat stats of player ships, experience, fuel, star-map travel, trade and crafting prices, and exploration loot. They also set the level cap of player ships and the factions that the "beat all enemies" skill needs. The skill tree itself is not in the database. The player skills service, the skill tree, the mother ship, shops, crafting, and the military base read skill settings.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `107` |
| Kind | settings (singleton) |
| File | [`Resources/Database/Settings/Skills.json`](../../../Assets/Modules/Database/Resources/Database/Settings/Skills.json) |
| Schema | [SkillSettings.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Settings/SkillSettings.xml) |
| Generated class | `GameDatabase.DataModel.SkillSettings` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/SkillSettings.cs)), read as `IDatabase.SkillSettings` |
| Runtime | [`PlayerSkills`](../../../Assets/Scripts/Domain/Player/PlayerSkills.cs) |

## Fields

The record has the `ItemType` [common field](index.md#common-fields) and no `Id`. The built-in catalog uses the schema defaults. Each expression takes the *skill level*: the level of one skill type (see [Skill levels](#skill-levels)). An expression can also use the numeric fields of this record as variables, for example `BaseFuelCapacity` ([SkillSettings.cs:154](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/SkillSettings.cs#L154)). The *Max* column shows the value at the highest skill level of the built-in tree with the defaults.

| Field | Type | Default / range | Max | Meaning |
| --- | --- | --- | --- | --- |
| `BeatAllEnemiesFactionList` | ref list [Faction](Faction.md) | | | Factions whose starbases the player must capture for the "beat all enemies" requirement node. Empty: every faction with territories ([Skills.cs:60](../../../Assets/Scripts/Domain/Skills/Skills.cs#L60)). |
| `DisableExceedTheLimits` | bool | | | `true`: hides the `ExceedTheLimits` node ([SkillTreeNode.cs:85](../../../Assets/Scripts/Legacy/GUI/ViewModel/Skills/SkillTreeNode.cs#L85)) and keeps the ship level cap at `MaxPlayerShipsLevel`. |
| `FuelTankCapacity` | expression `IntToInt(level)` | `BaseFuelCapacity + 50*level`, 0… | 600 | Fuel capacity of the mother ship. The fuel never goes above it ([PlayerResources.cs:52](../../../Assets/Scripts/Domain/Player/PlayerResources.cs#L52)). |
| `AttackBonus` | expression `IntToFloat(level)` | `0.1*level`, 0… | +100 % | Damage bonus of player ships in combat. |
| `DefenseBonus` | expression `IntToFloat(level)` | `0.1*level`, 0… | +100 % | Armor and shield bonus of player ships in combat. |
| `ShieldStrengthBonus` | expression `IntToFloat(level)` | `0.1*level`, 0… | +100 % | Extra shield bonus. It adds to `DefenseBonus`. |
| `ShieldRechargeBonus` | expression `IntToFloat(level)` | `0.1*level`, 0… | +100 % | Shield recharge bonus of player ships. |
| `ExperienceBonus` | expression `IntToFloat(level)` | `0.1*level`, 0… | +200 % | Bonus to the experience that player ships get after a battle ([CombatReward.cs:31](../../../Assets/Scripts/Combat/Domain/CombatReward.cs#L31)). |
| `FlightSpeed` | expression `IntToFloat(level)` | `BaseFlightSpeed + 0.4*level`, 1… | 5 | Star-map travel speed of the mother ship. It uses the `MainEnginePower` level. |
| `FlightRange` | expression `IntToFloat(level)` | `BaseFlightRange + 0.09*level`, 1.5… (not applied, [DB15.27](../../issues/database.md#db15-smaller-defects)) | 2.4 | Star-map jump range of the mother ship, in star distances ([GalaxyMap.cs:41](../../../Assets/Scripts/Legacy/Map/GalaxyMap.cs#L41)). It uses the `MainEnginePower` level too. |
| `ExplorationLootBonus` | expression `IntToFloat(level)` | `0.1*level`, 0… | +100 % | Bonus to the amount and chances of planet exploration loot ([LootGenerator.cs:107](../../../Assets/Scripts/Domain/Economy/LootGenerator.cs#L107)). |
| `HeatResistance` | expression `IntToFloat(level)` | `0.1*level`, 0…1 | 1 | Added to the heat resistance bonus of player ships. |
| `KineticResistance` | expression `IntToFloat(level)` | `0.1*level`, 0…1 | 1 | Same for kinetic resistance. |
| `EnergyResistance` | expression `IntToFloat(level)` | `0.1*level`, 0…1 | 1 | Same for energy resistance. |
| `MerchantPriceFactor` | expression `IntToFloat(level)` | `1 - 0.05*level`, 0… | 0.7 | Multiplier of the prices of goods in shops (merchants, faction shops, the black market, the arena) ([MerchantInventory.cs:36](../../../Assets/Scripts/Legacy/Game/Quests/Inventory/MerchantInventory.cs#L36)). |
| `CraftingPriceFactor` | expression `IntToFloat(level)` | `1 - 0.05*level`, 0… | 0.7 | Multiplier of crafting and ship-upgrade prices ([CraftPanel.cs:47](../../../Assets/Scripts/Gui/Craft/CraftPanel.cs#L47), [UnlockSlotPanel.cs:59](../../../Assets/Scripts/Gui/Craft/UnlockSlotPanel.cs#L59)). |
| `CraftingLevelReduction` | expression `IntToInt(level)` | `5*level`, 0… | 30 | Levels subtracted from the workshop level that crafting and ship upgrades need ([CraftPanel.cs:115](../../../Assets/Scripts/Gui/Craft/CraftPanel.cs#L115), [UnlockSlotPanel.cs:48](../../../Assets/Scripts/Gui/Craft/UnlockSlotPanel.cs#L48)). |
| `MaxPlayerShipsLevel` | int | 100, 0…500 | | Level cap of player ships. The military base also stops level-up purchases at it ([MilitaryBase.cs:56](../../../Assets/Scripts/Gui/StarMap/MilitaryBase/MilitaryBase.cs#L56)). |
| `IncreasedLevelLimit` | int | 200, 0…1000 | | Level cap of player ships after the player learns `ExceedTheLimits`. |
| `BaseFuelCapacity` | int | 100, 10… | | Fuel at the start of a new game ([StartingNewGameState.cs:53](../../../Assets/Scripts/GameStateMachine/States/StartingNewGameState.cs#L53)). A variable in `FuelTankCapacity`. |
| `BaseFlightRange` | float | 1.5, 1.5… | | A variable in `FlightRange`. The skill tree shows the range bonus relative to it. |
| `BaseFlightSpeed` | float | 1, 1… | | A variable in `FlightSpeed`. The skill tree shows the speed bonus relative to it. |

## Notes

### Skill levels

The skill tree is the file [`Resources/skills.json`](../../../Assets/Resources/skills.json), outside the database ([Skills.cs:21](../../../Assets/Scripts/Domain/Skills/Skills.cs#L21)). It has 167 nodes. Each node has a skill type and a multiplier. When the player learns a node, the level of its type grows by the multiplier ([PlayerSkills.cs:76](../../../Assets/Scripts/Domain/Player/PlayerSkills.cs#L76)). A mod cannot change the tree.

| Skill type | Field | Max level |
| --- | --- | ---: |
| `MainFuelCapacity` | `FuelTankCapacity` | 10 |
| `MainEnginePower` | `FlightSpeed`, `FlightRange` | 10 |
| `ShipAttack` | `AttackBonus` | 10 |
| `ShipDefense` | `DefenseBonus` | 10 |
| `ShipExperience` | `ExperienceBonus` | 20 |
| `ShieldStrength` | `ShieldStrengthBonus` | 10 |
| `ShieldRecharge` | `ShieldRechargeBonus` | 10 |
| `HeatDefense`, `EnergyDefense`, `KineticDefense` | the resistance fields | 10 each |
| `Trading` | `MerchantPriceFactor` | 6 |
| `CraftingPrice` | `CraftingPriceFactor` | 6 |
| `CraftingLevel` | `CraftingLevelReduction` | 6 |
| `PlanetaryScanner` | `ExplorationLootBonus` | 10 |
| `ExceedTheLimits` | `IncreasedLevelLimit` | 1 |

Other node types (rescue unit, master trader, space scanner, hangar slots) have fixed effects in code. Two requirement nodes are free when a condition holds ([Skills.cs:46](../../../Assets/Scripts/Domain/Skills/Skills.cs#L46)):

- `RequierementMaxLevel`: a ship of the player's fleet has reached `MaxPlayerShipsLevel`.
- `RequierementBeatAllEnemies`: the player has captured a starbase of every faction in `BeatAllEnemiesFactionList`.

The skill tree describes each node with the same expressions ([SkillType.cs:156](../../../Assets/Scripts/Legacy/Game/Skills/SkillType.cs#L156)).

### Combat bonuses

| Bonus | Formula | Code |
| --- | --- | --- |
| Damage | × (1 + `AttackBonus`) | [ShipBuilderExtensions.cs:10](../../../Assets/Scripts/Combat/Domain/ShipBuilderExtensions.cs#L10) |
| Armor | × (1 + `DefenseBonus`) | [ShipBuilderExtensions.cs:11](../../../Assets/Scripts/Combat/Domain/ShipBuilderExtensions.cs#L11) |
| Shield | × (1 + `DefenseBonus` + `ShieldStrengthBonus`) | [ShipBuilderExtensions.cs:12](../../../Assets/Scripts/Combat/Domain/ShipBuilderExtensions.cs#L12) |
| Shield recharge | × (1 + `ShieldRechargeBonus`) | [ShipBuilderExtensions.cs:13](../../../Assets/Scripts/Combat/Domain/ShipBuilderExtensions.cs#L13) |
| Resistances | The skill value is added to the resistance bonus, so the resistance grows by (armor + resistance) × value. | [ShipBuilderExtensions.cs:15](../../../Assets/Scripts/Combat/Domain/ShipBuilderExtensions.cs#L15), [ShipStatsCalculator.cs:152](../../../Assets/Modules/ShipConstructor/Scripts/Model/ShipStatsCalculator.cs#L152) |

The bonuses apply to player ships in battles and to the exploration ship ([FleetModel.cs:20](../../../Assets/Scripts/Combat/Domain/FleetModel.cs#L20), [ExplorationSceneManager.cs:169](../../../Assets/Scripts/Combat/Manager/ExplorationSceneManager.cs#L169)). [CombatRules](CombatRules.md) `DisableSkillBonuses` turns them off for one battle ([CombatModelBuilder.cs:52](../../../Assets/Scripts/Combat/Domain/CombatModelBuilder.cs#L52)). Resistance formulas: [ShipSettings: Stat formulas](ShipSettings.md#stat-formulas).

### Star-map travel and fuel

The mother ship uses `FlightRange` and `FlightSpeed` while it has fuel. Without fuel, it uses the fixed range 1.5 and speed 0.1 ([MotherShip.cs:17](../../../Assets/Scripts/Domain/Player/MotherShip.cs#L17)). A new game starts with `BaseFuelCapacity` fuel, not with `FuelTankCapacity(0)`. With the defaults both are 100.

### Ship level cap

`MaxShipExperience` is the experience of level `MaxPlayerShipsLevel`, or of `IncreasedLevelLimit` after `ExceedTheLimits` ([PlayerSkills.cs:125](../../../Assets/Scripts/Domain/Player/PlayerSkills.cs#L125)). Battle rewards cut ship experience at this cap ([IReward.cs:30](../../../Assets/Scripts/Legacy/Game/Quests/IReward.cs#L30)).

### Data rules

1. Keep `FlightRange` at 1.5 or more at level 0. The schema minimum does not apply ([DB15.27](../../issues/database.md#db15-smaller-defects)), and a lower range is shorter than the range without fuel.
2. Keep `MerchantPriceFactor` and `CraftingPriceFactor` above 0. At 0 goods and crafting are free.
3. Keep `IncreasedLevelLimit` above `MaxPlayerShipsLevel`, or `ExceedTheLimits` has no effect.
4. Write `FuelTankCapacity` so that level 0 gives `BaseFuelCapacity`. Otherwise the start fuel differs from the tank capacity.
5. A mod settings record replaces the whole record. Copy every value, not only the changed one ([DB7](../../issues/database.md#db7-settings-records-cannot-be-partly-overridden)).
