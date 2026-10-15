# ComponentStatUpgrade

`ComponentStatUpgrade` is an unused table. Its schema has no fields of its own, the built-in catalog has no records, and the game never reads one ([DB11](../../issues/database.md#db11-schema-fields-without-runtime-effect)).

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `31` |
| Kind | collection, unused |
| Folder | none in the built-in catalog |
| Records | 0 |
| Schema | [ComponentStatUpgrade.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/ComponentStatUpgrade.xml) |
| Generated class | `GameDatabase.DataModel.ComponentStatUpgrade` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/ComponentStatUpgrade.cs)) |

## Fields

The record has only the [common fields](index.md#common-fields) `ItemType`, `Id`, and `Disabled`.

## Notes

### Code references

The loader accepts the records and keeps them in `IDatabase.ComponentStatUpgradeList`. The only other reference is the type of a field in the ship editor's `ComponentUpgradeLevel` structure ([IComponentUpgradesProvider.cs:15](../../../Assets/Modules/ShipConstructor/Scripts/Upgrades/IComponentUpgradesProvider.cs#L15)). Both upgrade providers return no upgrades ([ShipEditorState.cs:103](../../../Assets/Scripts/GameStateMachine/States/ShipEditorState.cs#L103), [DatabaseEditorContext.cs:129](../../../Assets/ModulesShared/ShipEditor/Scripts/Context/DatabaseEditorContext.cs#L129)).

Component upgrades in the game come from [ComponentMod](ComponentMod.md) and the quality of a component, not from this table.

### Data rules

1. Do not add records. They load and have no effect.
