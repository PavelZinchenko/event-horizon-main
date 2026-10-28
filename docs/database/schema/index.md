# Database schema reference

This reference describes every table of the game database: what each field does, which code reads it, and the rules that the data must follow. For loading, mods, migration, and tooling, see the [Database module](../../database.md).

The source of truth is the XML schema in [`.Schema/v1/`](../../../Assets/Modules/Database/.Schema/v1/). The schema version is 1.7.

## How to read a table page

Each page has the same parts:

1. **Summary**: what the record is and which systems use it.
2. **Facts**: item type value, record kind, folder, record count in the built-in catalog, schema file, generated class.
3. **Fields**: one row per JSON field.
   - *Type* uses the schema type: `int`, `float`, `bool`, `string`, `char`, `color`, `vector`, `layout`, `image`, `audio`, `prefab`, `enum`, `flags`, `expression`, `ref`, `ref list`, `struct`, `struct list`.
   - *Default / range* shows the schema `default` and the clamp range. An empty cell means the C# default (`0`, `false`, empty) and no clamp.
   - *Meaning* describes the runtime effect, not only the name.
4. **Structures**: nested objects that only this table uses. Structures that several tables use are on [Shared structures](structures.md).
5. **Notes**: rules, pitfalls, and links to consumers and known issues.

Every collection record also has the three [common fields](#common-fields).

## Common fields

| Field | Type | Meaning |
| --- | --- | --- |
| `ItemType` | enum [ItemType](enums.md#itemtype) | Selects the table. Required in every record, including settings. |
| `Id` | int | Record ID, unique within the table. Collection records only. Use a positive value. `0` is the "no reference" value. |
| `Disabled` | bool | When `true`, the loader does not create the record. A mod uses it to hide a built-in record with the same ID. |

## Conventions

- **References** are integer IDs. `0` (and, in old data, `-1`) means "none". A missing target becomes `null`, except `Faction` (becomes the neutral [`Faction.Empty`](Faction.md)) and `ComponentMod` (becomes the empty modification). See [Records, IDs, and references](../../database.md#records-ids-and-references).
- **Localized text**: a string that starts with `$` is a localization key (for example `$GroupWeaponC`). Other strings are shown as they are.
- **Bonus fields** named `...Bonus` or `...Modifier` are usually *added* to a multiplier of 1. A value of `0.2` means +20 %, and `-0.5` means −50 %. Each page says when a field is a direct multiplier instead.
- **Distances and levels**: a star's level equals its distance from the center of the galaxy in star-map units. "Level" and "distance" mean the same number in most formulas.
- **Sizes** in combat are Unity world units. A ship with `ModelScale` 1 is about one unit wide.
- **Switch structures** (marked *switch*) show different fields for different values of their `Type` field. The *Case* column lists the values for which a field applies.

## Tables by domain

### Ships and builds

| Value | Table | Kind | Summary |
| ---: | --- | --- | --- |
| 6 | [Ship](Ship.md) | collection | A hull: image, layout, barrels, engines, base features. |
| 8 | [ShipBuild](ShipBuild.md) | collection | A hull plus installed components, perks, AI, and satellites. Used for enemies, starting ships, drones, starbases. |
| 7 | [Satellite](Satellite.md) | collection | A satellite (companion) hull. |
| 9 | [SatelliteBuild](SatelliteBuild.md) | collection | A satellite plus installed components. |

### Components and ship systems

| Value | Table | Kind | Summary |
| ---: | --- | --- | --- |
| 1 | [Component](Component.md) | collection | An installable module: layout, stats, and an optional device, weapon, or drone bay. |
| 11 | [ComponentStats](ComponentStats.md) | collection | Passive stat contribution of a component. |
| 12 | [ComponentMod](ComponentMod.md) | collection | A random modification ("quality") that scales component stats. |
| 35 | [ComponentGroupTag](ComponentGroupTag.md) | collection | A limit on how many components of one group a ship may install. |
| 2 | [Device](Device.md) | collection, legacy | An active or passive ship system (shield, teleporter, cloak, and so on). |
| 3 | [Weapon](Weapon.md) | collection, legacy | Firing behavior of a weapon component. |
| 5 | [DroneBay](DroneBay.md) | collection, legacy | Drone launch behavior. |

### Ammunition and visual effects

| Value | Table | Kind | Summary |
| ---: | --- | --- | --- |
| 25 | [Ammunition](Ammunition.md) | collection | A projectile: body, movement, impact effects, triggers. |
| 4 | [AmmunitionObsolete](AmmunitionObsolete.md) | collection, legacy | The old projectile format with fixed behavior classes. |
| 27 | [BulletPrefab](BulletPrefab.md) | collection | The look of a projectile. |
| 26 | [VisualEffect](VisualEffect.md) | collection | A composite effect made of timed elements. |
| 29 | [GameObjectPrefab](GameObjectPrefab.md) | collection, switch | A code-built combat object (worm-tail segment, circle). |

### Progression and world

| Value | Table | Kind | Summary |
| ---: | --- | --- | --- |
| 14 | [Faction](Faction.md) | collection | A faction: color, territory, wandering ships, merchant and research rules. |
| 10 | [Technology](Technology.md) | collection, switch | A research-tree entry that unlocks crafting of a component, ship, or satellite. |

### Quests and encounters

| Value | Table | Kind | Summary |
| ---: | --- | --- | --- |
| 15 | [Quest](Quest.md) | collection | A quest graph with start rules. |
| 16 | [Loot](Loot.md) | collection | A reusable reward or item list. |
| 18 | [Fleet](Fleet.md) | collection | An enemy fleet for quest battles. |
| 19 | [Character](Character.md) | collection | A dialog speaker in quests. |
| 20 | [QuestItem](QuestItem.md) | collection | An artifact item in the cargo hold. |
| 30 | [CombatRules](CombatRules.md) | collection | Rules of one battle type. |

### AI

| Value | Table | Kind | Summary |
| ---: | --- | --- | --- |
| 28 | [BehaviorTree](BehaviorTree.md) | collection | A combat AI behavior tree. |

### Settings (singletons)

| Value | Table | Summary |
| ---: | --- | --- |
| 100 | [ShipSettings](ShipSettings.md) | Ship stat base values, movement limits, default explosions. |
| 101 | [GalaxySettings](GalaxySettings.md) | New-game setup, enemy level curve, combat rules per battle type. |
| 102 | [DatabaseSettings](DatabaseSettings.md) | Data version and whether a mod replaces the built-in catalog. |
| 103 | [ExplorationSettings](ExplorationSettings.md) | Planet exploration actors and hazards. |
| 105 | [ShipModSettings](ShipModSettings.md) | Ship perk values and the pool of random perks. |
| 106 | [SpecialEventSettings](SpecialEventSettings.md) | Christmas, Easter, and Halloween events. |
| 107 | [SkillSettings](SkillSettings.md) | Skill-tree bonus formulas and level caps. |
| 108 | [DebugSettings](DebugSettings.md) | Debug console and cheat codes. |
| 109 | [CombatSettings](CombatSettings.md) | Default AI trees and default combat rules. |
| 110 | [UiSettings](UiSettings.md) | UI color theme and main-menu background. |
| 111 | [FactionsSettings](FactionsSettings.md) | Starbase defense power. |
| 112 | [MusicPlaylist](MusicPlaylist.md) | Music per game screen. |
| 117 | [LocalizationSettings](LocalizationSettings.md) | Labels for corrosive damage. |
| 118 | [WeaponSlots](WeaponSlots.md) | Names and icons of weapon-slot letters. |
| 119 | [BlackMarketSettings](BlackMarketSettings.md) | Satellites excluded from the smuggler market. |
| 120 | [RuinsSettings](RuinsSettings.md) | Satellites excluded from ruins loot and the loot cell limit. |

### Unused and reserved

| Value | Table | Status |
| ---: | --- | --- |
| 31 | [ComponentStatUpgrade](ComponentStatUpgrade.md) | Schema exists, no fields, no data, no runtime use. |
| 32 | [StatUpgradeTemplate](StatUpgradeTemplate.md) | Schema exists, no data, no runtime use. |
| 0 | [Undefined](Undefined.md) | Default enum value. Not a table. |
| 13 | [Skill](Skill.md) | Reserved. No schema. |
| 33, 34 | [FrontierLevel](FrontierLevel.md), [FrontierCommonLevel](FrontierCommonLevel.md) | Reserved. No schema. |
| 104, 115, 116 | [FrontierSettings](FrontierSettings.md), [FrontierNpcSettings](FrontierNpcSettings.md), [FrontierLevelSettings](FrontierLevelSettings.md) | Reserved. No schema. |
| 113 | [ResearchSetting](ResearchSetting.md) | Reserved. No schema. |
| 114 | [PvpSettings](PvpSettings.md) | Reserved. No schema. |

A record with a reserved item type, or with any value that has no schema, makes the loader throw `Unknown file type`.

## Other pages

- [Shared structures](structures.md): `Barrel`, `Engine`, `InstalledComponent`, `StatModification`, `FactionFilter`, `LootContent`, `LootItem`, `Requirement`, `SoundTrack`.
- [Enums](enums.md): every enum with its values and meaning.
