# Server Issues

This file records problems in the PHP scripts in `.server/`. For the design, see [Server](../server.md). The missing authentication of all requests is [MP15](modules_private.md#mp15-server-requests-are-not-authenticated) and is not repeated here. Note that account keys are platform IDs, and some of them (Steam IDs) are public. So anyone can read or replace the arena record and the purchase backup of a known player.

The analysis was done by reading the code on 2026-10-12. No issue was reproduced against the live server. The PHP version and the MySQL schema of the host are unknown. Severity is High, Medium, or Low. Status is **Confirmed** (the code path is certain), **Confirmed (latent)** (certain, but it depends on a host setting such as the PHP version), or **Plausible** (needs conditions that were not checked).

This page names no host, database, user, password, or token. SV2, SV3, and SV14 describe weaknesses of a live service. Fix them, or decide whether they belong in a public repository, before you publish this file.

## Defects

### SV1. A fractional rating reply shows a connection error

**Severity:** Medium. **Status:** Confirmed (client parse); Plausible (how often it happens).

**Where.** `arena_v2.php:136` and `:141` compute the rating changes with `/`, which gives a float in PHP. `:63` echoes the result. [PlayerInfo.cs:160](../../Assets/Scripts/GameServices/OfflineMultiplayer/PlayerInfo.cs#L160) parses it with `Convert.ToInt32(string)`.

**Failure.**
1. A player with rating 1,500 attacks a fleet with fleet rating 1,234 and wins.
2. The gain is `max(100, 1234/10)` = 123.4. The server replies `1623.4`.
3. `Convert.ToInt32("1623.4")` throws `FormatException`. `TryUpdateRating` returns false.
4. The arena status becomes `ConnectionError`, and the window keeps the old rating until the next `find`.

This happens after a victory when `min(R, A)` is above 1,000 and not a multiple of 10, and after a defeat when `R` is above 1,000 and not a multiple of 10. Fleet ratings start at the fleet power, which is rarely a multiple of 10. If the rating columns are floating-point, `find` also returns fractions and the arena never becomes ready.

**Fix.** On the server, round the new ratings to integers before the `UPDATE` and the reply. In the client, parse with `double.TryParse` and the invariant culture, then round. Ship both, because old clients stay installed.

### SV2. Queries are built from raw request fields

**Severity:** High. **Status:** Confirmed (code); Plausible (which platform names contain the characters).

**Where.** `arena_v2.php:2–11` reads every field with `urldecode` only, and every query concatenates the values (for example `:82`, `:95`, `:98`, `:198`). The same pattern is in `iap.php:2–3`, `:29`, `:35`, `:47`, in `iap_frontier.php`, and in `eh_support.php:2–3`, `:13`, `:27`. `arena_frontier.php` and the `arena/` classes escape or validate their fields.

**Failure.**
1. A Steam player's display name contains an apostrophe, for example `O'Neil`. The client sends it as `n` in `upd`.
2. `INSERT INTO UserInfo` fails with a syntax error. `upd` replies `Could not update data…`. The `UserIds` row was already inserted.
3. Every later `upd` finds the user ID, tries the same insert, and fails. The player cannot use the arena while the name contains the character. A backslash has the same effect.

The same defect lets any caller change the SQL statements through any field. The scripts share one database user, so this reaches every table of the first database, including the purchase backups and the Frontier tables.

**Fix.** Use `mysqli` prepared statements with bound parameters in every query. Validate numeric fields as integers. Until the rewrite, escape every string with `real_escape_string`, as `arena_frontier.php` does.

### SV3. Credentials are stored in plain text in the web root

**Severity:** High. **Status:** Confirmed (present in the files); Plausible (exposure).

**Where.** The password of the first database is written inline in `arena.php`, `arena_v2.php`, `arena_frontier.php`, `arena_frontier_test.php`, `iap.php`, `iap_frontier.php`, `eh_support.php`, and `rating_builder.php`. The two ZIP files in `archive/` contain copies of `arena/config.php` with the passwords of both databases. `cloud_build_webhook.php` and `github_test.php` contain a VK community access token and a GitHub token, and a comment in `cloud_build_webhook.php` contains a Unity Cloud Build API key. Only `arena/config.php` in this copy has placeholders.

**Failure.** Anyone who gets one of these files gets full access to the database and can post as the VK community. Possible paths: a backup or shared copy of this folder, a server error that serves PHP as text, or the ZIP files over HTTP ([SV14](#sv14-dead-files-and-archives-sit-in-the-web-root)). All scripts use the same database user, so one leak exposes all data.

**Fix.**
1. Rotate every credential and token named above.
2. Read credentials from one file outside the web root, or from environment variables.
3. Give each script group its own database user with only the grants it needs.
4. Delete tokens from comments and test pages.

### SV4. Defeat uses the reduced rating, and old records are cut to the cap

**Severity:** Low. **Status:** Confirmed (formula); Plausible (old records still exist).

**Where.** `arena_v2.php:141–142` reduces `$rating` before it computes the defender's gain from `min($rating, $airating)`. `:145–148` clamp both ratings to 50,000.

**Failure.**
- An attacker with 5,000 loses against a fleet with 8,000. The attacker drops to 4,500, and the fleet gains `max(100, 4500/10)` = 450 instead of 500.
- The first arena (`arena.php`) had no cap. The `rating.html` snapshot shows player ratings up to about 238,000. If such records still exist, the next result clamps them to 50,000, even after a victory.

**Fix.** Compute both deltas from the old values. Decide what over-cap records should become, and migrate them once instead of on the next fight.

### SV5. The fleet rating ignores fleet changes

**Severity:** Low. **Status:** Confirmed.

**Where.** `arena_v2.php:89–98`. `upd` uses `r` and `air` only when it creates the `UserInfo` row. Later uploads change only the fleet, the name, the version, and the upload time. The client still computes the fleet power for every upload ([PlayerInfo.cs:118](../../Assets/Scripts/GameServices/OfflineMultiplayer/PlayerInfo.cs#L118)).

**Failure.** A player registers with a fleet power of 2,000 and later uploads a fleet with power 20,000. The fleet rating stays near 2,000. Matchmaking sends players with a rating near 2,000 against a much stronger fleet. The fleet rating rises only by the defeat gains. The reverse case gives high-rated attackers a weak fleet with a large power multiplier.

**Fix.** Decide the intended design. One option: set the fleet rating from `air` when the uploaded fleet power changes by more than a set ratio.

### SV6. The Frontier clamp returns an undefined constant

**Severity:** Medium (Frontier only). **Status:** Confirmed (latent).

**Where.** `arena/arena-database-2966.php:409` returns `min` without `$` when the value is below the minimum. `:340` checks `$conn->error`, but the variable does not exist.

**Failure.**
- On PHP 7, `min` becomes the string `"min"` with a warning, and arithmetic treats it as 0. The result is accidentally correct.
- On PHP 8, an undefined constant is a fatal error. `completed-ai` fails with HTTP 500 whenever the fleet wins against a weaker player or loses against a stronger one. `completed-player` fails when the attacker wins against a fleet more than 5,000 points weaker.
- The error check in `updateFleet` never fires. A failed `SELECT` then fails on `$result->num_rows`.

**Fix.** Use `$min` and `$this->connection`.

### SV7. Errors are HTTP 200 text with database messages

**Severity:** Low. **Status:** Confirmed.

**Where.** `DieRead`, `DieUpdate`, and `DieWrite` in every script call `die('Could not read data'.$database->error)`. Unknown flags reply `Bad request`. All of them use HTTP status 200.

**Failure.**
- The client cannot tell an error from data. For example, `time` for an unknown account key replies `Could not read data`. `CheckAvailableBattles` fails to parse it and the arena reports a connection error instead of "no fights yet". In the editor, the search skips registration, so this is what happens unless the `Cheater` account exists on the server.
- Any caller sees MySQL error text with table and column names, which makes [SV2](#sv2-queries-are-built-from-raw-request-fields) easier to abuse.

**Fix.** Reply with HTTP 4xx or 5xx and a fixed message. Log the database error on the server. The client already handles `WWWErrorException`.

### SV8. Several scripts fail on PHP 8

**Severity:** Medium. **Status:** Confirmed (latent); the host's PHP version is unknown.

**Where.**
- `eh_support.php:7` passes the bare constant `localhost` to `mysqli`.
- `arena.php` and `rating_builder.php` use the `mysql_*` API, which PHP 7.0 removed.
- `arena-frontier-2966.php` calls `join("\n", $playerInfo)` when `top-opponent`, `opponent`, `opponentid`, or `stats` finds nothing and returns `null`.
- [SV6](#sv6-the-frontier-clamp-returns-an-undefined-constant).

**Failure.** On PHP 8 the item transfer fails with HTTP 500 for every request. On PHP 7 or later, `arena.php` and `rating_builder.php` cannot run. On PHP 8, "no opponent" in the Frontier arena becomes HTTP 500 instead of an empty reply.

**Fix.** Quote `'localhost'`. Delete `arena.php` and `rating_builder.php`. Return an empty reply when a lookup returns `null`.

### SV9. The cloud build webhook is unauthenticated and probably broken

**Severity:** Low. **Status:** Confirmed (no check, public file); Plausible (external API changes).

**Where.** `cloud_build_webhook.php`.

**Failure.**
- The script checks only that the JSON has `projectName`. Any HTTP POST makes the VK community post a message to its board.
- It writes the whole payload, with the name and e-mail of the person who started the build, to `request.txt` in the web root.
- GitHub stopped accepting the `access_token` query parameter, so the commit message lookup is expected to return nothing. VK API version 5.63 and the Unity Cloud Build payload are old; the script was not tested against the current services.

**Fix.** Delete the script and `github_test.php` if nobody uses them. Otherwise verify a shared secret, send the GitHub token in the `Authorization` header, keep tokens in the configuration file ([SV3](#sv3-credentials-are-stored-in-plain-text-in-the-web-root)), and do not write the payload to the web root.

### SV10. The music bundles were built with older Unity versions

**Severity:** Low. **Status:** Plausible.

**Where.** The headers of `assetbundles/musicbundle_eh_*` show Unity 2021.3 (Android, iOS, Windows) and 2022.3 (WebGL). The project uses Unity 6000.0.

**Failure.** Unity does not guarantee that a player loads bundles from an older editor. They usually load because they contain type trees, but nobody tested these files with the Unity 6 client. If a load fails, WebGL, Android, and iOS play no music and report nothing ([MP9](modules_private.md#mp9-assetloader-never-reports-a-failure-and-never-refreshes-the-cache)).

**Fix.** Test the current bundles in a Unity 6 build. If they fail, rebuild them per platform ([Server: Asset bundles](../server.md#asset-bundles)), upload them under a new `id`, and change the URL and the cache version in `AssetLoader` in the same release.

### SV11. `app-ads.txt` does not list Unity Ads

**Severity:** Low. **Status:** Plausible.

**Where.** `app-ads.txt` has one AdMob line. Android and iOS builds use Unity Ads (`ADS_UNITY`). No platform defines `ADS_GOOGLE` ([ModulesPrivate: Build variants](../modules_private.md#build-variants)).

**Failure.** Ad buyers that check `app-ads.txt` do not find Unity as an authorized seller and can skip the inventory. Revenue can drop without an error.

**Fix.** Add the lines from the Unity Ads dashboard. Remove the AdMob line if AdMob is no longer used.

## Structural issues

### SV12. The privacy policy omits the server data

**Problem.** The server keeps the platform account ID, the display name, the uploaded fleet, the ratings, the fight times, and the purchase backup. It keeps them without a time limit, and there is no deletion path. `rating.html` publishes player names. `policy.html` (last updated 23 June 2021) mentions only install data, the device ID, the advertising ID, AdMob, and Unity.

**Evidence.** [Server: Database tables](../server.md#database-tables); `policy.html`.

**Direction.** Describe the arena and purchase-backup data, the retention time, and how a player asks for deletion. Add a delete operation for one account key. Remove `rating.html`. Compare the result with the store data-safety forms.

### SV13. The schema is not in source control

**Problem.** Column types, keys, and defaults are known only from the live database. Some behaviour depends on them: [SV1](#sv1-a-fractional-rating-reply-shows-a-connection-error) depends on the rating column type, duplicate account keys depend on a unique key on `UserIds.UserKey`, and the Frontier leaderboard tables have no writer in the scripts. The scripts themselves have no version control; history exists only as numbered copies and ZIP files.

**Evidence.** [Server: Database tables](../server.md#database-tables); `.gitignore:50`.

**Direction.** Export the schema without data and keep it with the scripts. Put the scripts and the schema in a private repository, for example next to `ModulesPrivate`.

### SV14. Dead files and archives sit in the web root

**Problem.** The folder mirrors the web root and contains files that no client uses. If they are deployed:

- `archive/*.zip` give anyone the passwords of both databases ([SV3](#sv3-credentials-are-stored-in-plain-text-in-the-web-root)).
- `github_test.php` calls the GitHub API with the stored token and prints commit data to any visitor.
- `arena_frontier_test.php` is a second live copy of `arena_frontier.php`. The `archive/arena-frontier-29*.php` scripts are old live endpoints with old defects, including the `migrate` operation.
- `request.txt` exposes the last build payload ([SV9](#sv9-the-cloud-build-webhook-is-unauthenticated-and-probably-broken)).
- `arena.php`, `rating_builder.php`, `rating.html`, `policy_old.html`, `github.txt`, and `spoiler.php` (its image folder is missing) have no function.

**Evidence.** [Server: File map](../server.md#file-map).

**Direction.** Check the live web root and delete these files there. Keep history in version control ([SV13](#sv13-the-schema-is-not-in-source-control)), not in the web root.
