# Character

A character is a named speaker in quest dialogs. A `ShowDialog` node shows the character's name and avatar. Quest nodes and requirements also keep a relation value per character in the session. The game reads only `Name` and `AvatarIcon` from the record.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `19` |
| Kind | collection |
| Folder | [`Resources/Database/Quests/Characters/`](../../../Assets/Modules/Database/Resources/Database/Quests/Characters/) |
| Records in the built-in catalog | 7. Only character 2 sets `Relations` (10). Character 3 has no avatar. |
| Schema | [Character.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Quests/Character.xml) |
| Generated class | `GameDatabase.DataModel.Character` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/Character.cs)) |

## Fields

The record also has the [common fields](index.md#common-fields).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Name` | string | | Speaker name in the dialog, usually a localization key ([NodeBuilder.cs:61](../../../Assets/Modules/Quests/Scripts/Factory/NodeBuilder.cs#L61)). |
| `AvatarIcon` | image (`AvatarIcon`) | | Portrait in the dialog. The game looks in `Resources/Textures/Avatars/` first, then in the database images ([ResourceLocator.cs:69](../../../Assets/Modules/ResourceLocator/Scripts/ResourceLocator.cs#L69)). Empty: no portrait. The built-in folder has `A_12`, `K_07`, `M_07`, `M_12`, `M_16`. |
| `Faction` | ref [Faction](Faction.md) | | Not read. |
| `Inventory` | ref [Loot](Loot.md) | | Not read. |
| `Fleet` | ref [Fleet](Fleet.md) | | Not read. |
| `Relations` | int | −100…100 | Not read. Session relations start at 0, not at this value. |
| `IsUnique` | bool | | Not read. |

The five unread fields are listed in [DB11](../../issues/database.md#db11-schema-fields-without-runtime-effect).

## Notes

### Relations

The session stores one relation value per character ID, from −100 to 100, starting at 0 ([QuestData.cs:157](../../../Assets/Modules/GameSession/Scripts/Facade/Content/QuestData.cs#L157)). The value is shared by all quests and survives quest completion.

| Consumer | Rule |
| --- | --- |
| Node `SetCharacterRelations`, `ChangeCharacterRelations` | Sets or adds `Value`, clamped to −100…100. See [Quest: Node types](Quest.md#node-types). |
| Requirement `CharacterRelations` | The value is in `MinValue`…`MaxValue`. When min > max, a max of `0` means no upper bound, and a min of `0` means no lower bound ([CharacterRelationsRequirement.cs:11](../../../Assets/Modules/Quests/Scripts/Requirements/CharacterRelationsRequirement.cs#L11)). |

A mod that removes a character keeps its relation value in old saves. A new character with the same ID inherits it.

### Data rules

1. Set `Character` on every relation node and every `CharacterRelations` requirement. A `0` throws: in a node when the quest is built, in a requirement when it is checked ([DB15.24](../../issues/database.md#db15-smaller-defects)).
2. Do not rely on `Relations` for a start value. Use a `SetCharacterRelations` node at the start of the first quest.
3. Do not reuse a character ID for a different person in a mod. Old saves keep the relation value of the ID.
