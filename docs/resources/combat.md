# Combat Resources

[Resource index](index.md)

`Assets/Modules/BattleSimulator/Resources/Combat/` holds prefab templates for the battle simulation. `PrefabCache` loads a `Combat/...` path through Unity `Resources.Load<GameObject>` and caches the result. A `PrefabId` can also supply the path. Game data chooses many prefab IDs, so code references alone do not identify every prefab used by a player build.

## Ships and satellites

`Ships/` contains visible ship templates. `ShipFactory` loads `Combat/Ships/` plus the ship model image ID. If no matching prefab exists, it loads `Combat/Ships/Default` and applies the model sprite through `ResourceLocator`. This lets one template cover ships that have art but no dedicated prefab.

`Satellites/` works the same way for drones and attached craft. `SatelliteFactory` loads the satellite model ID and falls back to `Combat/Satellites/default`. The ship and satellite prefabs provide object structure and behavior components; the separate sprite folders provide much of their visible art.

## Projectiles, guns, and effects

`Bullets/` supplies projectile and beam templates. `PrefabCache.GetBulletPrefab` selects templates by bullet shape, including common projectiles, rockets, lasers, lightning, mines, arcs, hooks, and saws. It instantiates a template and initializes it with bullet data and sprites. Older bullet prefab IDs can still be loaded through the obsolete path. A projectile's visual and behavior therefore depend on both its prefab and its data.

`Guns/` holds arm and turret structures mounted on ships. `ShipFactory` explicitly loads `Combat/Guns/turret` for turrets.

`Effects/` contains flashes, smoke, sparks, lightning, waves, trails, shield and text effects. `PrefabCache.GetEffectPrefab` maps visual effect types to reusable templates and initializes them with effect data. `EffectFactory` can also load a named `Combat/Effects/` prefab. These prefabs present combat events; they are separate from the textures and sprites chosen for an individual effect.

## World and exploration objects

`Objects/` holds gameplay objects such as areas, asteroids, decoys, shields, gravity fields, planets, and repair bots. `SpaceObjectFactory` and `SatelliteFactory` load named object prefabs when creating those mechanics. `CustomPrefabLoader` also uses object templates to construct data defined objects.

`Exploration/` holds terrain and encounter objects. `SpaceObjectFactory` loads its gas clouds, craters, volcanoes, vortices, containers, boxes, rocks, asteroids, and ship wrecks by name while building exploration scenes. The prefab supplies renderers, colliders, and behavior; the factory applies placement, size, color, and other runtime values.
