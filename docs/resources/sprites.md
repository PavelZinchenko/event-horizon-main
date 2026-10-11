# Sprites and shared icons

[Resource index](index.md)

## Model and item art

`Assets/Sprites/Ships/` and `Starbases/` hold the full size craft images. `ResourceLocator` gathers both groups into its ship sprite array during its editor `Reload` action. Combat can use a model sprite when `ShipFactory` falls back to the default ship prefab. The star map, loading screen, ship editor, and ship lists also request model sprites through the locator.

`ShipIcons/` holds smaller craft pictures for list and selection views. The locator gathers them separately. When an icon ID has no entry, `GetShipIconSprite` tries the full size ship sprite with the same ID. This fallback matters for new ships that have model art but no dedicated icon.

`Components/` holds equipment icons. The resource locator indexes them by name, and component lists, item descriptions, and the ship editor request them from component data. The ship editor also uses their textures while building the module layout. `Satellites/` holds satellite and turret art. The locator indexes these images for satellite previews and for combat objects that use a general prefab with a supplied sprite.

`Skills/` contains player skill and ability pictures. `PlayerSkillSettings.asset` maps skill types to selected sprites and supplies a default icon when no mapping exists. UI assets can also reference skill sprites directly. The folder is not one of the groups collected by `ResourceLocator.Reload`.

## Interface sprites and atlases

`Assets/Sprites/UI/` supplies the visual pieces for buttons, panels, frames, scroll areas, progress bars, and backgrounds. UI prefabs and scenes use these as serialized sprite references. They are separate from the ID based ship and component lookup.

The sprite atlas assets at `Assets/Sprites/` pack the component, satellite, ship icon, ship, skill, and UI groups for rendering. Atlases optimize how Unity supplies sprites to the renderer. `ResourceLocator` still resolves an individual sprite by ID; an atlas does not replace the locator's name mapping.

## Shared Resources icons

`Assets/ModulesShared/Resources/Icons/` contains icons used by shared GUI and ship editor prefabs: navigation, equipment, fleet, journal, scanner, and similar actions. `Icons/WeaponGroup/` contains size or class symbols for weapon groups. `ResourceLocator.Reload` does not collect this folder. The icons reach the game by three routes: serialized UI references; `Resources.Load` by path for the ship editor's component groups (`icons/icon_shield` and others); and the hand-edited `_guiIconSprites` list of the locator for the weapon slot icons. See [Gui module: Shared icons](../gui.md#shared-icons).

## ID and update rules

`ResourceLocator.prefab` stores the editor collected ship, ship icon, component, and satellite arrays. The locator builds dictionaries keyed by sprite name. A data `SpriteId` selects a category and name; the locator returns that sprite or asks the database for a fallback image. Keep names unique within each collected category, and refresh the locator prefab after adding or renaming art in these groups.
