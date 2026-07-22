# EventHorizon Editor

## How to use:

1. Download and extract Release.zip.
2. Run GameDatabase.exe and open the database.
3. Click Create Mod and enter the name.
4. Copy the Mod file to the folder "\<Game Installation Folder\>/Mods" for PC or "Android/data/com.ZipasGames.EventHorizon/files/Mods" for Android.
5. Open the game settings and select your Mod.

Since 0.15.3 you can also put the whole database into the Mods folder. This allows you to edit your ship builds in game editor.

## Images

Images can be placed anywere in the /Database/ folder. They must be a square size - 128x128, 256x256, 512x512, 1024x1024 etc. Only PNG and JPEG formats are supported.

## Localization

Localization files should be named according to the language - \<language\>.xml (e.g. English.xml, Russian.xml)

```
<?xml version="1.0" encoding="UTF-8"?>
<resources>
<string name="Module1">Small bomb</string>
<string name="Module2">Big bomb</string>
</resources>
```

## Quests

You can email me any interesting quests you made. If I like it, I'll add it to the game.

#### Quest Types
* Common - can be started multiple times
* Singleton - like common, but only one can be active at a time
* Storyline - can be completed only once
* Temporary - like common, but doesn't save it's state

#### Start Conditions
* Manual - can only be started manually
* Beacon - starts at beacons (more `Weight` = more chances to start)
* LocalEncounter - starts when you are attacked (or attack) by local occupants (more `Weight` = more chances to start)
* FactionMission - starts when you take a mission (more `Weight` = more chances to start)
* GameStart - starts when you start (or load) the game
* NewStarExplored - starts when you explore a new star and defeat its defenders (`Weight*100%` = chances to start)
* ArrivedAtStar - starts when you arrive at any star stystem (`Weight*100%` = chances to start)

#### Quest node types
* ComingSoon - does nothing, shows 'Coming Soon' message in quest log
* ShowDialog - shows the dialog window
* Switch - waits until any transition in `Transitions` or `DefaultTransition` is available, then use it.
* Random - selects random transition it `Transitions` or `DefaultTransition` in none is available.
* AttackFleet - attacks the fleet then jumps to `VictoryTransition` or `DefeatTransition`
* AttackOccupants - same as AttackFleet but attacks current star defenders
* DestroyOccupants - removes current star defenders
* SuppressOccupants - makes current star defenders non agressive
* Retreat - moves to the nearest safe star system
* ReceiveItem - gives `Loot` to player
* RemoveItem - removes `Loot` from player
* Trade - opens trade window. `Loot` = items in the store.
* CompleteQuest - completes this quest
* FailQuest - fails this quest
* CancelQuest - cancels this quest (like it has never started)
* StartQuest - starts the new `Quest` (it should be `Manual`)
* SetCharacterRelations - sets the relations between `Character` and player to `Value`
* SetFactionRelations - sets the relations between current faction and player to `Value`
* ChangeCharacterRelations - modifies the relations between `Character` and player by `Value`
* ChangeFactionRelations - modifies the relations between current faction and player by `Value`

#### Requirement types
* Any - any of `Requirements` should be true
* All - all `Requirements` should be true
* None - all `Requirements` should be false
* PlayerPosition - player should be between `MinDistance` and `MaxDistance` l.y. from home
* RandomStarSystem - player should be at random star system between `MinDistance` and `MaxDistance` l.y. from home
* AggressiveOccupants - enemies in current star system want attack player
* QuestCompleted - quest `QuestId` has been completed
* QuestActive - quest `QuestId` is in progress
* CharacterRelations - relations between player and `Character` should be between `MinValue` and `MaxValue`
* FactionRelations - relations between player and current faction should be between `MinValue` and `MaxValue`
* Faction - player should be on the territory of `Faction`
* HaveQuestItem - player has an `Amount` of `Item`s
* HaveItem, HaveItemById - player has every item in `Loot` (or `LootId`)
* ComeBack - player should go to the star system where this quest has been taken

#### Loot types
* SomeMoney - an amount of money according to current distance from homestar multiplied by `ValueRatio`.
* Fuel - random amount of fuel between `MinAmount` and `MaxAmount`
* Money - random amount of money between `MinAmount` and `MaxAmount`
* Stars - random amount of stars between `MinAmount` and `MaxAmount` (mobile version only)
* StarMap - a star map which explores adjacent star systems
* RandomComponents - random amount between `MinAmount` and `MaxAmount` of random compmonents of level according to the distance from homestar multiplied by `ValueRatio` and filtered by `Factions`
* RandomItems - random amount of `Items` between `MinAmount` and `MaxAmount` (more `Weight` = more chances to pick)
* AllItems - every item in `Items`
* ItemsWithChance - random amount of `Items` (chance to pick = `Weight`*100%)
* QuestItem - random amount of `Item`s between `MinAmount` and `MaxAmount`
* Ship - selected ship `Build`
* EmptyShip - selected `Ship` without weapons and modules
* Component - random amount of selected `Component`s between `MinAmount` and `MaxAmount`
