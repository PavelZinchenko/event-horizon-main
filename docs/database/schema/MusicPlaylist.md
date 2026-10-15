# MusicPlaylist

The music playlist lists the music tracks for the main menu, the star map, combat, and planet exploration. A list replaces the game's own music for that screen. An empty list keeps the game's own music. Only the database music playlist reads this record.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `112` |
| Kind | settings (singleton) |
| File | [`Resources/Database/Settings/MusicPlaylist.json`](../../../Assets/Modules/Database/Resources/Database/Settings/MusicPlaylist.json) |
| Schema | [MusicPlaylist.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Settings/MusicPlaylist.xml) |
| Generated class | `GameDatabase.DataModel.MusicPlaylist` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/MusicPlaylist.cs)), read as `IDatabase.MusicPlaylist` |
| Runtime | [`DatabaseMusicPlaylist`](../../../Assets/Scripts/GameServices/Audio/DatabaseMusicPlaylist.cs) |

## Fields

The record has the `ItemType` [common field](index.md#common-fields) and no `Id`. The built-in catalog has no tracks, so the game plays its own music everywhere.

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `MainMenuMusic` | struct list [SoundTrack](structures.md#soundtrack) | | Main menu. |
| `GalaxyMapMusic` | struct list [SoundTrack](structures.md#soundtrack) | | Star map ([StarMapState.cs:99](../../../Assets/Scripts/GameStateMachine/States/StarMapState.cs#L99)). |
| `CombatMusic` | struct list [SoundTrack](structures.md#soundtrack) | | Combat. A battle's [CombatRules](CombatRules.md) `CustomSoundtrack` replaces it when that list is not empty ([DatabaseMusicPlaylist.cs:32](../../../Assets/Scripts/GameServices/Audio/DatabaseMusicPlaylist.cs#L32)). |
| `ExplorationMusic` | struct list [SoundTrack](structures.md#soundtrack) | | Planet exploration. |

## Notes

### How a track is chosen

1. At startup, the game sets the database playlist as the custom playlist of the music player ([InitializationState.cs:149](../../../Assets/Scripts/GameStateMachine/States/InitializationState.cs#L149)).
2. When a track ends, the player asks the database playlist for the next clip of the current screen. The playlist picks a random entry of the list ([DatabaseMusicPlaylist.cs:39](../../../Assets/Scripts/GameServices/Audio/DatabaseMusicPlaylist.cs#L39)). The same track can play twice in a row.
3. When the list is empty or the clip is not found, the player uses the game's own music bundle ([MusicPlayer.cs:67](../../../Assets/Modules/Services/Scripts/Audio/MusicPlayer.cs#L67)).

### Clip lookup

The playlist looks up clips only in the audio files of the loaded mod (`IDatabase.GetAudioClip`), not in the built-in clips ([DatabaseMusicPlaylist.cs:43](../../../Assets/Scripts/GameServices/Audio/DatabaseMusicPlaylist.cs#L43)). This differs from sound effects, where built-in clips win ([Media](../../database.md#media-images-and-audio)). A name of a built-in clip finds nothing, and the game's own music plays. The loop marker `*` is removed from the name and has no effect on music.

### Data rules

1. Put the music files into the mod. A folder mod accepts `*.wav` and `*.ogg`; the track name is the file name without the extension ([Storage backends](../../database.md#storage-backends)).
2. Leave a list empty to keep the game's own music for that screen.
3. A mod settings record replaces the whole record. Copy every value, not only the changed one ([DB7](../../issues/database.md#db7-settings-records-cannot-be-partly-overridden)).
