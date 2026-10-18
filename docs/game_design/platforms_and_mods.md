# Platforms and Mods

[Game design index](index.md)

## Purpose

Platform integration makes the campaign available through different controls, accounts, and storage services.
Mods let authors change the game's content and rules through an alternative database.

## Player actions

1. Use the input and settings supported by the current build.
2. Connect a platform account when a feature requires it.
3. Use available local, cloud, export, or import save options.
4. Buy offered products or choose a rewarded advertisement where supported.
5. Select an installed mod through the database settings.

## Current rules

### Platform services

The checked-in platform definitions and service selection describe the following commercial configurations.
This is a source snapshot, not a claim about currently published store builds or live service availability.

| Platform | Account integration | Storage and commercial services |
| --- | --- | --- |
| Android | Google Play Games | Local and Google cloud storage; configured purchases and advertisements |
| iOS | Game Center | Local and Apple cloud storage; configured purchases and advertisements |
| Standalone PC | Steam on supported desktop targets | Local storage; purchases and advertisements disabled in current definitions |
| WebGL | No platform account in the documented selection | Browser-backed local storage; purchases and advertisements disabled |

The public project can build with replacement services under the open-source license configuration.
Those replacements do not establish working accounts, cloud saves, purchases, advertisements, or arena requests.
The private implementations and build definitions determine the actual service set.

A cloud-save implementation on two platforms does not establish account linking or cross-platform save transfer.
That behavior needs its own supported flow.

### Purchases and economic variants

The purchase implementation offers six consumable star packs and a non-consumable supporter pack.
The supporter-pack flow can add its configured ship if the player does not already own that hull.
Store product details and localized prices come from the store integration.

`IAP_DISABLED` changes gameplay as well as hiding store purchases.
Military-base stars are excluded, arena battle depletion is bypassed, and premium price helpers convert to credits.
The [economy chapter](economy.md#price-types-and-build-differences) owns the conversion rule.
Balance comparisons must therefore identify the build configuration.

### Advertisements

A completed rewarded video can grant store loot or restore an arena battle through the relevant active window.
The reward depends on completion, not merely starting the advertisement.
The startup flow also requests an interstitial unless advertisements have been removed.

The technical review records a broken interstitial path in the Unity Ads implementation.
Document that as attempted behavior with a known defect, rather than promising that the advertisement appears.
See [advertisement implementation and issue](../modules_private.md#advertisements).

### Mod scope

A mod is a full or partial game database.
It can supply ships, components, factions, quests, settings, rules, media, and localized text.
The external database editor is the main authoring tool.
Database changes do not automatically extend rules that remain fixed in application code.

Mods can come from folders or packed files.
Android and standalone builds scan designated mod folders.
iOS and WebGL do not use that folder scan; packed-mod access depends on the available platform interface.

| Mod content | Merge behavior |
| --- | --- |
| Collection record with an existing type and ID | Replaces the corresponding built-in record |
| New collection record | Adds content |
| Disabled record | Hides the corresponding built-in record |
| Settings record | Replaces the whole settings record, with omitted fields using defaults |
| Full replacement database | Can exclude the original catalog entirely |
| Image or audio with a built-in name | Built-in asset wins; mod lookup is a fallback |
| Language XML | Can add or replace localized text |

Settings replacement is not a field-by-field patch.
A mod author must account for omitted values and references to other records.
Full replacement also has to satisfy runtime assumptions about required content.

Changing content can affect existing ships, quests, and saves.
For example, removed quest steps can force reconstruction at another step, and fleet loading can discard invalid parts.
Compatibility is therefore part of mod design, not just file loading.

## System connections

[Economy](economy.md) defines currency behavior.
[Activities](activities.md) defines arena and rewarded activity rules.
[Player experience](player_experience.md) defines control and save expectations.
[Factions and quests](factions_and_quests.md) describes quest reconstruction after content changes.

## Design intent and open questions

Platform services should preserve the core exploration, construction, and combat loop.
Purchase and advertisement availability should not become a prerequisite for understanding that loop.
Mods should make content variation practical while keeping compatibility limits visible.

Open questions:

1. Which services are required for each supported release, and which should fail without blocking the campaign?
2. What save compatibility should be promised when switching databases or updating a mod?
3. Which cross-platform account or save-transfer flows are intended beyond the current platform-specific services?

## Sources

- [ProjectSettings](../../ProjectSettings/ProjectSettings.asset): platform compilation definitions at review time.
- [Services](../services.md) and [ModulesPrivate](../modules_private.md): selected implementations, replacement services, and commercial flows.
- [PlayerFleet](../../Assets/Scripts/Domain/Player/PlayerFleet.cs): supporter ship and fleet repair during load.
- [ArenaTimeManager](../../Assets/Scripts/GameServices/OfflineMultiplayer/ArenaTimeManager.cs): purchase-disabled battle-limit exception.
- [Database: mods and merge rules](../database.md#mods-and-merge-rules) and [Database editor](../database/editor.md): mod loading and authoring.
- [Localization](../localization.md#database-and-mod-localization) and [ResourceLocator](../resource_locator.md): text and media precedence.
