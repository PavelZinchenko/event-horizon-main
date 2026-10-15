# Server

The game server is a set of PHP scripts on shared web hosting, with MySQL behind them. It stores the arena (asynchronous PvP: players fight uploaded fleets of other players), the purchase backup, and developer item transfers. It also serves the music asset bundles, the project web pages, the privacy policy, and a Unity Cloud Build webhook. The same server and the same first database also serve the sibling game *Event Horizon – Frontier*.

A copy of the web root is in `.server/`. Git ignores that folder ([.gitignore:50](../.gitignore#L50)), so this page refers to its files by path and does not link them. This page does not list host names, database names, database users, passwords, or tokens.

| | Value |
| --- | --- |
| Location | `.server/` (local copy of the web root, not in Git) |
| Stack | PHP with `mysqli` (old scripts: the removed `mysql_*` API), MySQL. No framework, no shared code between scripts. |
| Clients in this repository | [`Security.RequestFactory`](../Assets/ModulesPrivate/Security/Scripts/RequestFactory.cs) (arena, purchase backup, item transfer), [`AssetLoader`](../Assets/ModulesPrivate/Services/Scripts/Assets/AssetLoader.cs) (music bundle), [`MainMenu`](../Assets/Scripts/Gui/MainMenu/MainMenu.cs#L108) (privacy policy) |
| Protocol | HTTP form POST (arena) or GET query string (other scripts). Plain-text replies, one value per line. |
| Authentication | None. The server trusts every field ([MP15](issues/modules_private.md#mp15-server-requests-are-not-authenticated)). |
| Schema | Not in source control. Tables are known only from the queries ([Database tables](#database-tables)). |
| Known issues | [Server issues](issues/server.md) |

## Contents

1. [Terms](#terms)
2. [File map](#file-map)
3. [Arena](#arena)
4. [Frontier arena](#frontier-arena)
5. [IAP backup](#iap-backup)
6. [Developer item transfer](#developer-item-transfer)
7. [Asset bundles](#asset-bundles)
8. [Cloud build webhook](#cloud-build-webhook)
9. [Web pages](#web-pages)
10. [Database tables](#database-tables)
11. [Working on the server](#working-on-the-server)

## Terms

| Term | Meaning |
| --- | --- |
| Account key | The platform account ID that the client sends as `usr` or `id`: `G:` + Google Play player ID, `i:` + Game Center player ID, `s:` + Steam ID. The editor account sends `Cheater`. See [ModulesPrivate: Accounts](modules_private.md#accounts). |
| User ID | The integer that the server assigns to an account key in table `UserIds`. Opponents are addressed by user ID. |
| Player rating | `Rating`. It changes when the player attacks another fleet. The arena window shows it. |
| Fleet rating | `AiRating` (Frontier: `FleetRating`). It changes when another player attacks this player's uploaded fleet. Matchmaking compares the attacker's player rating with the defenders' fleet ratings. |
| Fleet blob | The serialized fleet: binary ship data, zlib-compressed, URL-safe Base64 without padding. [`FleetWebSerializer`](../Assets/Scripts/GameServices/OfflineMultiplayer/FleetWebSerializer.cs) writes format version 4 and reads versions 1–4. The client refuses blobs above 16,384 characters. |
| Operation flag | A form or query field whose presence selects the operation, for example `find`. Its value is ignored. |
| Frontier | *Event Horizon – Frontier*, a separate game. Its client is not in this repository. |

## File map

| Path in `.server/` | Purpose | Caller | State |
| --- | --- | --- | --- |
| `arena_v2.php` | Arena for this game. | `RequestFactory` | Live |
| `iap.php` | Purchase backup for this game. | `RequestFactory` via `WebStorage` | Live, no writes ([MP1](issues/modules_private.md#mp1-the-purchase-backup-is-never-written)) |
| `eh_support.php` | Developer item transfer. | `RequestFactory` via `Cheats` | Live, see [SV8](issues/server.md#sv8-several-scripts-fail-on-php-8) |
| `bundle.php`, `assetbundles/` | Music asset bundles. | `AssetLoader` | Live |
| `policy.html` | Privacy policy. | Main menu link | Live |
| `index.html`, `css/`, `js/`, `images/`, `icons/` | Project landing page (Bootstrap 4.3.1, jQuery 3.3.1). | Browsers | Live |
| `app-ads.txt` | Authorized ad sellers file. | Ad networks | Live |
| `arena_frontier.php` | Frontier arena, first version. | Frontier client | Live for Frontier (the "old database") |
| `arena-frontier-2966.php`, `arena/` | Frontier arena, current version, two arena modes. | Frontier client | Live for Frontier |
| `iap_frontier.php` | Frontier purchase backup. | Frontier client | Live for Frontier |
| `cloud_build_webhook.php` | Posts Unity Cloud Build results to a VK community board. | Unity Cloud Build | Probably broken ([SV9](issues/server.md#sv9-the-cloud-build-webhook-is-unauthenticated-and-probably-broken)) |
| `arena.php`, `rating_builder.php`, `rating.html` | First arena and its static leaderboard. | None | Dead ([SV8](issues/server.md#sv8-several-scripts-fail-on-php-8)) |
| `arena_frontier_test.php`, `github_test.php`, `policy_old.html`, `spoiler.php`, `request.txt`, `github.txt` | Test copies, a test page, an old policy, an unrelated image script, webhook output. | None | Dead ([SV14](issues/server.md#sv14-dead-files-and-archives-sit-in-the-web-root)) |
| `archive/`, `arena/arena-database-29*.php`, `arena/*test*.php`, `arena/arena_database*.php` | Earlier Frontier arena versions and two ZIP backups of them. | None (except the two files that `arena-frontier-2966.php` requires) | Dead ([SV14](issues/server.md#sv14-dead-files-and-archives-sit-in-the-web-root)) |

## Arena

`arena_v2.php` is the only arena endpoint that this game calls. The client is [`OfflineMultiplayer`](../Assets/Scripts/GameServices/OfflineMultiplayer/OfflineMultiplayer.cs) with [`PlayerInfo`](../Assets/Scripts/GameServices/OfflineMultiplayer/PlayerInfo.cs) and [`EnemyPlayerInfo`](../Assets/Scripts/GameServices/OfflineMultiplayer/EnemyPlayerInfo.cs). The local battle limit (10 battles, one back every 10 minutes) is client-side; see [Session: PvpData](game_session/schema/social.md#pvpdata).

### Request format

Every request is an HTTP POST with a `WWWForm` body. The server runs `urldecode` on every field a second time, after PHP has decoded the form ([MP12](issues/modules_private.md#mp12-request-fields-are-encoded-inconsistently)). The server checks the operation flags in the order of the table below and runs the first one present. Without a known flag it replies `Bad request`.

| Flag | Fields | Server action | Reply | Client |
| --- | --- | --- | --- | --- |
| `find` | `usr` | Read the player's record. | Four lines: player rating, fleet rating, seconds since the last fleet upload, seconds since the last fight. Empty if the account key is unknown. | [`CreateGetPlayerInfoRequest`](../Assets/ModulesPrivate/Security/Scripts/RequestFactory.cs#L10) → [`PlayerInfo.TryUpdate`](../Assets/Scripts/GameServices/OfflineMultiplayer/PlayerInfo.cs#L170) |
| `time` | `usr` | Read the time of the last fight. | Seconds since the last fight. | [`CreateGetTimeFromLastFightRequest`](../Assets/ModulesPrivate/Security/Scripts/RequestFactory.cs#L19) → [`CheckAvailableBattles`](../Assets/Scripts/GameServices/OfflineMultiplayer/PlayerInfo.cs#L100) |
| `upd` | `usr`, `n` (name), `r` (player rating), `air` (fleet power), `v` (`AppConfig.versionCode`), `f` (fleet blob) | Register the account key if it is new, then store the name, version, and fleet. `r` and `air` are used only when the record is created ([SV5](issues/server.md#sv5-the-fleet-rating-ignores-fleet-changes)). | Same four lines as `find`. | [`CreateRegisterPlayerRequest`](../Assets/ModulesPrivate/Security/Scripts/RequestFactory.cs#L28) → `TryUpdate` |
| `match` | `usr`, `r`, `v` | Pick a random opponent ([Matchmaking](#matchmaking)). | Three lines: user ID, fleet rating, name. | [`CreateFindOpponentRequest`](../Assets/ModulesPrivate/Security/Scripts/RequestFactory.cs#L45) → [`CreateOpponent`](../Assets/Scripts/GameServices/OfflineMultiplayer/PlayerInfo.cs#L134) |
| `fleet` | `id` (user ID) | Read a stored fleet. | The fleet blob. | [`CreateGetFleetRequest`](../Assets/ModulesPrivate/Security/Scripts/RequestFactory.cs#L55) → [`EnemyPlayerInfo`](../Assets/Scripts/GameServices/OfflineMultiplayer/EnemyPlayerInfo.cs#L26) |
| `result` | `usr`, `id` (opponent user ID), `w` (1 victory, −1 defeat, 0 no result) | Update both ratings and the fight time ([Rating rules](#rating-rules)). | The new player rating. Empty for `w=0`. | [`CreateUpdateStatsRequest`](../Assets/ModulesPrivate/Security/Scripts/RequestFactory.cs#L63) → [`TryUpdateRating`](../Assets/Scripts/GameServices/OfflineMultiplayer/PlayerInfo.cs#L154) |

`upd` rejects a negative `r` or `air` and an empty fleet. All errors are plain text with HTTP status 200, for example `Could not read data` followed by the MySQL error ([SV7](issues/server.md#sv7-errors-are-http-200-text-with-database-messages)). The client sees them as unparsable data and sets `Status.ConnectionError`.

### Client flow

1. The arena window calls `UpdateStatus`. If the account is not connected, the status is `NotLoggedIn`. Outside the editor, a fleet with a ship that fails `ShipValidator.IsAllowedOnArena` gives `ShipNotAllowed`.
2. The client sends `find` and stores the player rating and both times. An unknown player gets rating 100.
3. The player presses *Find opponent*. Outside the editor, if the last upload is more than one hour old, the client first sends `upd` with the active fleet and its power from [`RatingCalculator.CalculateFleetPower`](../Assets/Scripts/GameServices/OfflineMultiplayer/RatingCalculator.cs#L28). After a session load the client treats the last upload as one day old, so the first search of a session always uploads.
4. The client sends `time`, recomputes the available battles, and stops if none are left.
5. The client sends `match` and fires `EnemyFoundSignal` with an `EnemyPlayerInfo`.
6. On *Fight*, the client sends `fleet`, deserializes the blob, and drops ships that fail `IsAllowedOnArena`. User ID 1 keeps all ships ([EnemyPlayerInfo.cs:46](../Assets/Scripts/GameServices/OfflineMultiplayer/EnemyPlayerInfo.cs#L46)). One battle is used.
7. The battle uses `GalaxySettings.ArenaCombatRules`. The enemy fleet gets a power multiplier of 1 + min(5, fleet rating / 20,000). Victory pays 1 + fleet rating / 1,000 tokens ([OfflineMultiplayer.cs:143](../Assets/Scripts/GameServices/OfflineMultiplayer/OfflineMultiplayer.cs#L143)).
8. After the battle the client sends `result` and shows the returned rating.

### Matchmaking

`match` reads `r` from the client and searches the fleet ratings of other players with `GameVersion` ≤ `v`. The version filter makes sure the client can read the opponent's fleet blob. The server tries three windows and takes a random row from the first window that has one:

1. From `r − max(r/10, 500)` to `r + max(r/4, 500)`.
2. From `r − max(r/2, 5,000)` to `r + max(r/2, 5,000)`.
3. Any player.

The query does not exclude the editor account or players without a fleet.

### Rating rules

`result` reads the player rating `R` of the attacker and the fleet rating `A` of the defender.

| Result | Attacker | Defender |
| --- | --- | --- |
| Victory (`w > 0`) | `R + max(100, min(R, A)/10)` | `A − max(50, A/20)` |
| Defeat (`w < 0`) | `R − max(100, R/10)` | `A + max(100, min(R_new, A)/10)`, with the already reduced `R_new` ([SV4](issues/server.md#sv4-defeat-uses-the-reduced-rating-and-old-records-are-cut-to-the-cap)) |
| `w = 0` | Only `LastFightTime` changes. | No change. |

Both ratings are clamped to 0–50,000. PHP divides in floating point, so the reply can be a fraction ([SV1](issues/server.md#sv1-a-fractional-rating-reply-shows-a-connection-error)). [`RatingCalculator`](../Assets/Scripts/GameServices/OfflineMultiplayer/RatingCalculator.cs) repeats the attacker formulas, but no code calls `GetRatingChange`.

## Frontier arena

Frontier uses the same host. This game does not call these scripts, but they share the first database and table `UserIds`.

- `arena_frontier.php` has the same six flags as `arena_v2.php`. Differences: names live in `UserNames`, ratings in `UserInfoFrontier`, fleets in `UserFleetFrontier`; strings are escaped; `upd` rejects `v` < 2302; the cap is 20,000; a defeated fleet loses `max(20, A/50)`. The current Frontier script calls these tables "the old database".
- `arena-frontier-2966.php` (the number is a Frontier version code) uses a second database and the classes in `arena/`: `ArenaDatabase` (`arena-database-2966.php`), `ArenaDatabaseOld` (`arena-database-old-test.php`, reads the old tables with a rating scale of 5), and two arena modes, `StarbaseLevel` (`lvl=sb`) and `DuelLevel` (`lvl=d`). Account keys are 32-character hexadecimal GUIDs. Numeric fields are validated and strings are escaped.

| Flag | Fields | Action |
| --- | --- | --- |
| `player-id` | `guid` | User ID for a GUID. |
| `reg` | `guid`, `name` | Register; returns the new user ID. |
| `rename`, `getname` | `user`, `name` | Change or read the name. |
| `upload` | `lvl`, `user`, `ver`, `data`, `rating` | Store the fleet for the mode. `rating` is the start fleet rating. |
| `stats` | `lvl`, `user` | Player rating, fleet rating, seconds since upload. |
| `time` | `user` | Seconds since the last fight. |
| `opponent` | `lvl`, `user`, `min`, `max`, `ver` | Random opponent in a fleet-rating range. Replies empty for 50 % of requests, so the client falls back to `legacy-opponent`. |
| `bestmatch` | `lvl`, `user`, `rating`, `ver` | Random one of the ten closest fleets at or above `rating`, else below. Same 50 % rule. |
| `top-opponent` | `lvl`, `user`, `pos`, `ver` | The player at rank `pos`, skipping fleets with 50 or more bug reports. |
| `opponentid` | `lvl`, `user` | One opponent by user ID. |
| `legacy-opponent`, `legacy-fleet` | `rating` or `min`/`max`, `ver`; `user` | Opponent and fleet from the old database. |
| `fleet` | `lvl`, `user` | Fleet blob. |
| `start`, `last` | `lvl`, `user`, `enemy` | Store or read the last opponent. |
| `completed-player` | `lvl`, `user`, `rating` (enemy), `result` | Update the attacker's player rating and fight time. |
| `completed-ai` | `lvl`, `user`, `rating` (attacker), `result` | Update the defender's fleet rating. |
| `bug` | `lvl`, `user`, `enemy` | Add a bug report to the enemy fleet and a cheat report to the reporter. |
| `leaderboard`, `cheaters` | `lvl` | Rows of user ID, name, player rating, fleet rating, bug reports, five lines per row. |

The Frontier rating is linear in the rating difference with a cap of 100,000. A win adds up to `max(20, (100,000 − R) × 1,000 / 100,000)`; a loss takes up to 1,000. A fleet changes by 1,000 plus the rating difference (at most 10,000). Two defects in this code are [SV6](issues/server.md#sv6-the-frontier-clamp-returns-an-undefined-constant).

## IAP backup

`iap.php` stores one purchase-backup record per account key. It does not validate purchases. Receipt validation does not exist on the server or in the client ([MP16](issues/modules_private.md#mp16-purchases-are-not-validated)). The client is the private [`WebStorage`](../Assets/ModulesPrivate/Services/Scripts/IapStorage/WebStorage.cs), bound on Android ([ModulesPrivate: Purchase backup storage](modules_private.md#purchase-backup-storage)). iOS keeps the record in the keychain instead.

| Query | Server action | Reply | Client |
| --- | --- | --- | --- |
| `?get&id=<account key>` | Read `IapData` for the user ID. | The record. Empty if the account key is unknown. `Could not read data…` if the key exists but has no record. | [`CreateGetIapDataRequest`](../Assets/ModulesPrivate/Security/Scripts/RequestFactory.cs#L83) |
| `?set&id=<account key>&data=<record>` | Register the account key if it is new, then insert or replace the record. | Empty on success. | [`CreateSetIapDataRequest`](../Assets/ModulesPrivate/Security/Scripts/RequestFactory.cs#L88) |

The record is `IapData` as JSON, UTF-8, URL-safe Base64. `WebStorage.Write` reads the current record first and writes the merge of both. No code calls `Write` today ([MP1](issues/modules_private.md#mp1-the-purchase-backup-is-never-written)). `iap_frontier.php` is the same script for Frontier with table `InAppPurchases_Frontier`; it replies empty instead of an error when the record is missing.

## Developer item transfer

`eh_support.php` moves items from a developer's editor to a player's device. It is a one-time mailbox keyed by a five-character code.

1. In the editor, the developer enters a five-character code in the cheat console. [`Cheats`](../Assets/Scripts/Legacy/Debug/Cheats.cs#L148) serializes a hard-coded item list and sends `?set&id=<code>&data=<items>`.
2. On the device, the player enters the same code. `Cheats` sends `?get&id=<code>`.
3. The server returns the items and deletes the row. The client adds the items and opens the loot window.

The items are binary product data in URL-safe Base64. Table: `GameSupportTable (Id, Items)`.

## Asset bundles

`bundle.php` serves the music asset bundle. The private [`AssetLoader`](../Assets/ModulesPrivate/Services/Scripts/Assets/AssetLoader.cs#L82) requests `bundle.php?id=music2&p=<platform>` on WebGL, Android, and iOS. Standalone builds and open-source builds load `StreamingAssets/musicbundle` instead ([Services: Asset loader](services.md#asset-loader)).

`bundle.php` maps the query to a file in `assetbundles/` and returns it as `application/octet-stream` with `Access-Control-Allow-Origin: *` and an `ETag` from the file time. An unknown `id` or `p`, or a missing file, gives an empty reply.

| `p` | `id=music2` (this game) | `id=music` (Frontier) |
| --- | --- | --- |
| `web` | `musicbundle_eh_web2` (Unity 2022.3) | `musicbundle_web2` |
| `android` | `musicbundle_eh_android` (Unity 2021.3) | `musicbundle_android` |
| `android_v2` | missing | `musicbundle_android_v2` |
| `ios` | `musicbundle_eh_ios` (Unity 2021.3) | `musicbundle_ios` |
| `win` | `musicbundle_eh_win` (Unity 2021.3), requested only by the editor | `musicbundle_win` |

`musicbundle_web` has no mapping. The Unity version is the one in the bundle header. The project now uses Unity 6 ([SV10](issues/server.md#sv10-the-music-bundles-were-built-with-older-unity-versions)).

To update the bundle:

1. Select the target platform in Build Settings.
2. Run **Assets → Build AssetBundles** ([CreateAssetBundles.cs](../Assets/Editor/CreateAssetBundles.cs)). It writes `Assets/StreamingAssets/musicbundle`. The assets in `Assets/BundledAssets/MusicBundle/` carry the bundle name.
3. Upload the file as `assetbundles/musicbundle_eh_<suffix>` with the suffix from the table.
4. Increase the cache version in `AssetLoader`. Otherwise installed games keep the cached copy ([MP9](issues/modules_private.md#mp9-assetloader-never-reports-a-failure-and-never-refreshes-the-cache)).

## Cloud build webhook

`cloud_build_webhook.php` receives the JSON build notification from Unity Cloud Build. It does these steps:

1. Writes the decoded payload to `request.txt` in the web root.
2. Looks up the commit message of `lastBuiltRevision` through the GitHub API.
3. Opens a topic on the VK community board, posts "project (target #number) – status", the commit message, and the share link, and closes the topic.

It checks only that `projectName` exists. Problems are in [SV9](issues/server.md#sv9-the-cloud-build-webhook-is-unauthenticated-and-probably-broken). `github_test.php` is a manual test of step 2.

## Web pages

| File | Content |
| --- | --- |
| `index.html` | Landing page for Event Horizon and Frontier: store links, community links, GitHub links, support e-mail. |
| `policy.html` | Privacy policy, last updated 23 June 2021. The main menu opens it. It does not mention the data in this page's tables ([SV12](issues/server.md#sv12-the-privacy-policy-omits-the-server-data)). |
| `policy_old.html` | The 2017 policy. Nothing links it. |
| `rating.html` | A static top-20 table that `rating_builder.php` wrote from the first arena. It contains player names. |
| `app-ads.txt` | One AdMob seller line. |

## Database tables

The scripts use two MySQL databases on the same host. Column types, keys, and defaults are not in source control ([SV13](issues/server.md#sv13-the-schema-is-not-in-source-control)). The columns below are the ones that the queries use.

**First database** (this game, first Frontier arena, purchase backups, item transfer):

| Table | Columns | Written by |
| --- | --- | --- |
| `UserIds` | `UserId` (auto-increment), `UserKey` (account key) | `arena_v2`, `arena_frontier`, `iap`, `iap_frontier` |
| `UserInfo` | `UserId`, `UserName`, `Rating`, `AiRating`, `GameVersion`, `LastUpdateTime`, `LastFightTime` | `arena_v2` |
| `UserFleet` | `UserId`, `Fleet` | `arena_v2` |
| `UserNames` | `UserId`, `UserName` | `arena_frontier` |
| `UserInfoFrontier` | `UserId`, `Rating`, `AiRating`, `GameVersion`, `LastUpdateTime`, `LastFightTime` | `arena_frontier` |
| `UserFleetFrontier` | `UserId`, `Fleet` | `arena_frontier` |
| `InAppPurchases`, `InAppPurchases_Frontier` | `UserId` (unique), `IapData` | `iap`, `iap_frontier` |
| `GameSupportTable` | `Id`, `Items` | `eh_support` |

`UserIds` is shared, so one account key has one user ID in this game, in Frontier, and in both purchase backups. `LastUpdateTime` and `LastFightTime` are timestamps; the scripts set them with `CURRENT_TIMESTAMP`.

**Second database** (current Frontier arena):

| Table | Columns |
| --- | --- |
| `UserIds` | `Id` (auto-increment), `Guid` |
| `UserInfo` | `Id`, `Name`, `LastFightTime`, `CheatReports` |
| `FleetData` | `Id` (auto-increment), `Data` |
| `FleetStarbase`, `FleetDuel` | `UserId`, `FleetId`, `GameVersion`, `PlayerRating`, `FleetRating`, `UpdateTime`, `BugReports`, `LastOpponent` |
| `LeaderboardStarbase`, `LeaderboardDuel`, `CheaterboardStarbase`, `CheaterboardDuel` | `UserId`. No script writes them; they are probably views or are filled outside these scripts. |
| `CheaterList` | `UserId`, `Reports`. Only the archived version 2964 reads it. |

## Working on the server

### Deploying

There is no deployment script. The scripts are copied to the web root by hand. Keep `.server/` equal to the live web root, and do not deploy `archive/`, test scripts, or ZIP files ([SV14](issues/server.md#sv14-dead-files-and-archives-sit-in-the-web-root)).

### Changing an arena operation

1. Keep the reply format. Old clients stay installed for years and parse replies by line position.
2. Use a new script name for an incompatible change, and point a new client version at it. `RequestFactory` holds the URL.
3. Keep integer replies integers. The client parses ratings and times with `Convert.ToInt32` ([SV1](issues/server.md#sv1-a-fractional-rating-reply-shows-a-connection-error)).
4. Filter opponents by `GameVersion` ≤ the client version, so the client can read every fleet it receives. If `FleetWebSerializer` gets a new format version, old fleets must stay readable.
5. Use prepared statements for every new query ([SV2](issues/server.md#sv2-queries-are-built-from-raw-request-fields)).

### Secrets

Keep credentials in one configuration file outside the web root, as `arena/config.php` started to do. Do not copy credentials into scripts, comments, test pages, or archives. If a secret was ever in a script, rotate it ([SV3](issues/server.md#sv3-credentials-are-stored-in-plain-text-in-the-web-root)).
