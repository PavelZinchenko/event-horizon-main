# Activities

[Game design index](index.md)

## Purpose

Activities apply the core construction and combat systems to different objectives, restrictions, and reward opportunities.
Their repeat rules give the player reasons to revisit known locations.

## Player actions

1. Select an activity from the map, a local service, or the main menu.
2. Inspect its opponents, entry conditions, and available rewards.
3. Prepare the fleet or use the supplied challenge ship.
4. Complete the encounter and collect eligible rewards.
5. Continue to another activity or return when its repeat condition permits it.

## Current rules

### Campaign activities

| Activity | Encounter and reward role | Repeat behavior |
| --- | --- | --- |
| Star occupants | Ordinary or faction fleets; can include research rewards | Clearing removes those occupants; quest actions can also suppress them |
| Faction starbase | Regional defense battle linked to the capture quest | Capture persists; later quest changes can alter regional state |
| Boss | Flagship encounter with faction research reward | Returns after a seeded real-time delay; gains five encounter levels per recorded defeat |
| Ruins | Abandoned-base faction encounter with special rewards | Completion is recorded |
| Survival | Large enemy fleet with rewards and experience allowed without victory | Four-hour cooldown begins when the attempt starts |
| Challenge | Five stages using a supplied ship against selected opponents | Victory advances one stage; all five victories complete the location |
| Local event | Beacon starts a selected quest | Player builds consume the beacon on activation, then apply a long cooldown |
| Hive | Infected planet with hostile objectives and guardians | Defeated when all planet objectives are complete |
| Christmas tree | Seasonal fleet encounter with holiday rewards | Completion is recorded for that location |

Wormholes and markets are also special star objects, but function as travel or service access.
Their rules belong to [Galaxy](galaxy.md) and [Economy](economy.md).

The star-object selector restricts certain objects to neutral or faction regions.
A faction home star takes precedence over the ordinary special-object roll.
Introductory locations use a curated shuffled set instead of the normal distribution.
The exact selection ranges remain in [StarData](../../Assets/Scripts/Domain/Galaxy/StarData.cs).

### Challenges

The player uses a supplied build rather than the owned active fleet.
The location selects five enemy builds and orders them by hull size.
Each attempt fights the current stage; only victory advances progress.

The built-in challenge rules disable skill bonuses, random combat loot, and experience.
They use a 60-second limit followed by health drain and deploy the supplied ship by order.
Each stage attempts to award a component through the loot generator.
The final stage also awards one to three premium price units.
The build's premium conversion can change the resulting currency.

### Arena

The arena is asynchronous competition against other players' uploaded fleets.
The opponent is controlled locally by AI. There is no live opposing player controlling the ships during that battle.
The client requires a connected account and an eligible fleet.

The player searches for an opponent, downloads the fleet, fights, and submits the result.
The documented server first searches within rating windows, then broadens its search when needed.
Its behavior is described in the [server reference](../server.md#arena); this review does not verify the deployed service.

The enemy receives a rating-based power multiplier:
`1 + min(5, fleet rating / 20000)`, using fractional division.
Victory awards `1 + floor(fleet rating / 1000)` tokens.
The built-in arena rules disable experience, random loot, skill bonuses, and asteroids.
They use ordered deployment and a 90-second reinforcement-or-draw timer.
Both fleets are shuffled before that deployment order is applied.

Purchase-enabled builds allow a pool of ten battles, recovering one every ten minutes.
A completed rewarded advertisement can add an available battle.
Under `IAP_DISABLED`, the availability check always returns the maximum instead of enforcing depletion.
This exception does not remove the account and server requirements.

Arena ship reconstruction currently drops stored ship experience and appearance.
The repository records this as an [unresolved behavior](../issues/ship_constructor.md#sc4-arena-ships-drop-experience-and-colour), not an approved competitive-balance rule.

### Quick combat

Quick combat starts from the main menu with a selected difficulty and separate encounter setup.
It uses the configured quick-combat rules rather than entering through campaign travel.
Editor-only test configuration can replace normal fleets.
Treat those developer overrides separately from the player-facing mode.

### Holidays and daily rewards

The holiday manager checks configured date windows for Christmas, Halloween, and Easter.
It uses the internet-time service and starts the corresponding configured quests on session creation.
Before that service receives a network value, holiday checks use its local-time fallback.
Christmas also affects star objects, loot, and the snowflake shop.

Daily rewards use received internet time and the last claimed date.
Consecutive-day claims increase reward size; a break restarts the sequence.
The reward also considers campaign exploration progress.
The main-menu activation path checks daily rewards on non-standalone platforms; event notifications also exist.
Daily rewards are distinct from timer-driven Daily quests.

### Time and repeat behavior

| System | Clock or completion condition |
| --- | --- |
| Boss | Local UTC time; seeded respawn delay in the 24-to-72-hour range after victory |
| Survival | Local UTC time; four hours from attempt start |
| Local event | Local UTC time; 5 to 29 days after recorded activation |
| Arena | Client timer corrected using server fight-time information |
| Holidays | Internet-time service, with local-time fallback before receipt |
| Daily rewards | Received internet time |
| Daily quest eligibility | Active campaign play time |
| Challenge and planet objectives | Saved progress, rather than a refresh timer |

Editor local events use fresh random seeds and do not record the normal activation cooldown.
Editor behavior therefore cannot establish player-build repeat timing.

## System connections

[Combat](combat.md) defines shared battle behavior.
[Galaxy](galaxy.md) places activities and owns planet exploration.
[Factions and quests](factions_and_quests.md) provides event and capture flows.
[Platforms and mods](platforms_and_mods.md) determines service availability and build exceptions.

## Design intent and open questions

Activities should test different skills and support different goals.
Supplied-ship challenges should make success depend on understanding the given build.
Repeat timers and entry limits should be visible before the player invests travel or preparation time.

Open questions:

1. Should the arena's experience handling become an explicit normalization rule or preserve uploaded ship progression?
2. Should activity timers use a consistent clock and refresh explanation?
3. Does consuming a local beacon on activation match the intended treatment of failed or cancelled quests?

## Sources

- [Star-content services](../../Assets/Scripts/Domain/Galaxy/StarContent/): occupants, bosses, ruins, survival, challenges, local events, hives, and Christmas encounters.
- [Built-in combat rules](../../Assets/Modules/Database/Resources/Database/Quests/CombatRules/): activity-specific settings.
- [OfflineMultiplayer](../../Assets/Scripts/GameServices/OfflineMultiplayer/OfflineMultiplayer.cs), [EnemyPlayerInfo](../../Assets/Scripts/GameServices/OfflineMultiplayer/EnemyPlayerInfo.cs), and [RatingCalculator](../../Assets/Scripts/GameServices/OfflineMultiplayer/RatingCalculator.cs): fleet setup, power scaling, and rewards.
- [ArenaTimeManager](../../Assets/Scripts/GameServices/OfflineMultiplayer/ArenaTimeManager.cs) and [Server: arena](../server.md#arena): competition and limits.
- [QuickCombatState](../../Assets/Scripts/GameStateMachine/States/QuickCombatState.cs): quick-combat entry and test overrides.
- [HolidayManager](../../Assets/Scripts/GameServices/HolidayManager.cs), [DailyReward](../../Assets/Scripts/Domain/Player/DailyReward.cs), and [MainMenuState](../../Assets/Scripts/GameStateMachine/States/MainMenuState.cs): timed events and claims.
- [InternetTimeService](../../Assets/Scripts/Services/InternetTime/InternetTimeService.cs): network time and local fallback.
