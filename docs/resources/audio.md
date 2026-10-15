# Sound effects and music

[Resource index](index.md)

## Game sound effects

`Assets/Audio/` holds the clips that `ResourceLocator` collects into `ResourceLocator.prefab` during its editor `Reload` action. The locator indexes clips by Unity asset name. `SoundPlayer` resolves an `AudioClipId` through the locator, then assigns the clip to an `AudioSource`. If the clip is absent from the locator, the locator asks the database for a clip with the same ID.

The `Explosion` and `Hit` groups supply impact and destruction sounds. Combat triggers, including asteroid and drone explosions, request named explosion clips. The `Shot`, `Launch`, and `Flux` groups support weapon firing, launch, charging, and related effects chosen by weapon and device sound IDs. `WeaponFactory` attaches sound triggers for shot and charge events. `Gui` contains interface and service cues such as buying, repair, scrap, alarms, and engine sounds. UI components can also hold direct `AudioClip` references; `KeyBindingPanel` passes its serialized clip to `SoundPlayer`.

The source groups are a naming convention, not separate runtime directories. `ResourceLocator` gathers the entire `Assets/Audio/` tree into one clip array. A clip's basename must be unique within that array, because the runtime dictionary uses the basename as its key.

## Bundled music

`Assets/BundledAssets/MusicBundle/Playlist.asset` assigns music clips to playlist roles. Its six source tracks live in the adjacent `Music/` folder. `LocalAssetLoader` opens a built `musicbundle` file from `Application.streamingAssetsPath` and loads the asset named `Playlist`. `MusicPlayer` then selects a clip for the active track type and plays it through its `AudioSource`. It loads the bundle when music volume is positive.

`MusicPlayer` also accepts a custom playlist. The current game initialization supplies `DatabaseMusicPlaylist`, which retrieves tracks from game data. The bundled playlist remains the player's default playlist, while custom tracks take precedence when available. Source files in `BundledAssets` are inputs to the bundle; the runtime loader reads the built bundle file.

## Gamemaster Audio - Pro Sound Collection

The vendor collection contains reusable UI and other effects. Two clips have verified serialized references outside the collection:

- `User_Interface_Menu/ui_button_simple_click_03.wav` is referenced by shared and ship editor buttons, settings, and `CommonGuiScene`.
- `User_Interface_Menu/ui_menu_button_beep_01.wav` is referenced by the toggle and controls panel.

The remaining collection files have no verified serialized GUID reference outside the collection. This check does not prove that filename based loading cannot reach them.
