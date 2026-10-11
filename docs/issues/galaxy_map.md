# GalaxyMap Module Issues

This file records problems in `Assets/Modules/GalaxyMap/` and in the star map scripts in `Assets/Scripts/Legacy/Map/`. For the design, see [GalaxyMap module](../galaxy_map.md).

The analysis was done by reading the code, the prefabs, and `StarMapScene` on 2026-10-12. No issue was reproduced in Play Mode. Severity is High, Medium, or Low. Status is **Confirmed**, **Confirmed (latent)**, or **Plausible**.

Several items change seeded or visual output. Per project policy, ask before changing anything that alters generated layouts or save-affecting results.

## Defects

### GM1. Star rendering reseeds the global Unity random generator

**Severity:** Low. **Status:** Confirmed.

**Where.** `Random.InitState(star.Id)` in [Star.cs:59](../../Assets/Scripts/Legacy/Map/Star.cs#L59) and `Random.seed = id` in [Star.cs:326](../../Assets/Scripts/Legacy/Map/Star.cs#L326).

**Failure.** Every star that is created, re-initialized, or colored resets `UnityEngine.Random`. Any code that runs later and uses `UnityEngine.Random` without its own seed (combat effects, weapon spread, decoys, AI turns in the BattleSimulator module) starts from the seed of the last star drawn. The results are not wrong, but they become predictable and repeatable across sessions. `Random.seed` is also obsolete.

**Fix.** Use a local `System.Random(star.Id)` (or a hash) for star size, rotation, blink speed, and color. Note: the exact random values change, so star sizes and colors would change once. Confirm with the owner first.

### GM2. `MapBackground` modifies the material asset

**Severity:** Low. **Status:** Confirmed.

**Where.** `Material.SetTextureOffset` and `SetTextureScale` on the serialized `Material` field ([MapBackground.cs:75](../../Assets/Scripts/Legacy/Map/MapBackground.cs#L75)). The field points to `Resources/Materials/MapBackground.mat` in `StarMapScene` and `SkillTreeScene`.

**Failure.** In the editor, every Play session writes new offsets into the asset. `MapBackground.mat` shows up as modified in Git after playing. In a player build, the change stays in memory and carries over between the star map and the skill tree.

**Fix.** Use `_meshRenderer.material` (an instance) or a `MaterialPropertyBlock`, as [`Background`](../../Assets/Scripts/Legacy/Map/Background.cs) does.

### GM3. System-view layout is not stable between sessions

**Severity:** Low. **Status:** Confirmed.

**Where.** `new System.Random(GetHashCode())` in [OrbitalObjects.cs:221](../../Assets/Scripts/Legacy/Map/System/OrbitalObjects.cs#L221), [line 228](../../Assets/Scripts/Legacy/Map/System/OrbitalObjects.cs#L228), [line 240](../../Assets/Scripts/Legacy/Map/System/OrbitalObjects.cs#L240), [FleetObjects.cs:84](../../Assets/Scripts/Legacy/Map/System/FleetObjects.cs#L84), [Ship.cs:51](../../Assets/Scripts/Legacy/Map/System/Ship.cs#L51). `boss.GetHashCode()` and `ruins.GetHashCode()` in `CreateBoss` and `CreateRuins`.

**Failure.** `GetHashCode` of a `MonoBehaviour` or a facade object depends on the instance, not on the star. Arena, military base, black market, wormhole, beacon, and boss positions change each time the player enters the star system. Planets use `System.Random(starId)` and stay fixed. The player sees stations jump between visits.

**Fix.** Seed every position from the star ID (plus an object index).

### GM4. `MapScaler` logs warnings on every mode change

**Severity:** Low. **Status:** Confirmed.

**Where.** `Debug.LogWarning` in [MapScaler.cs:128](../../Assets/Scripts/Legacy/Map/MapScaler.cs#L128), [line 138](../../Assets/Scripts/Legacy/Map/MapScaler.cs#L138), and [line 148](../../Assets/Scripts/Legacy/Map/MapScaler.cs#L148).

**Failure.** Debug output in release builds, and warnings that hide real ones in the editor console.

**Fix.** Remove the calls.

## Structural issues

### GM5. The module has no code; its code lives in `Legacy/`

**Problem.** `GalaxyMap.asmdef` declares seven references but contains no scripts. All behaviour is in `Assembly-CSharp` under `Scripts/Legacy/Map/`, in the global namespace and in `Map`, `StarSystem`, and `Scripts.Galaxy`.

**Evidence.** [GalaxyMap.asmdef](../../Assets/Modules/GalaxyMap/GalaxyMap.asmdef); `Scripts/` folder of the module is empty.

**Direction.** Move `Legacy/Map/` into the module. The scripts depend on `MotherShip`, `StarData`, `StarMap`, `PlayerSkills`, and `Planet.Factory` from the application layer, so these need interfaces in a lower assembly first.

### GM6. Unused assets

**Problem.** `GalaxyMapStar.mat`, `StarSurface.mat`, and their shaders are not referenced. `LabIcon` and `MultiplayerIcon` are assigned in the `Star` prefab but never added. `AttentionIcon` and `ShipIcon` prefabs are not referenced by `Star`.

**Evidence.** GUID search over all scenes, prefabs, and materials.

**Direction.** Delete them, or record why they are kept.

### GM7. Per-frame `Camera.main` and `SendMessage`

**Problem.** Each `Star` reads `Camera.main.orthographicSize` every frame. State changes reach icons through `SendMessage("OnStateChanged")`. A full star map view has hundreds of stars.

**Evidence.** [Star.cs:207](../../Assets/Scripts/Legacy/Map/Star.cs#L207), `OnStateChanged` in `Star`.

**Direction.** Let `MapScaler` compute the state once per frame and notify stars through an event or a direct list.
