# UiSettings

UI settings define the color theme of the interface: windows, buttons, text, item quality, technology states, and currencies. They also select the main-menu background and can hide the credits text. The theme loader copies the colors into the UI theme after each database load, and the main menu reads the last two fields.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `110` |
| Kind | settings (singleton) |
| File | [`Resources/Database/Settings/UiTheme.json`](../../../Assets/Modules/Database/Resources/Database/Settings/UiTheme.json) |
| Schema | [UiSettings.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Settings/UiSettings.xml) |
| Generated class | `GameDatabase.DataModel.UiSettings` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/UiSettings.cs)), read as `IDatabase.UiSettings` |
| Runtime | [`UiTheme`](../../../Assets/ModulesShared/Gui/Scripts/Theme/UiTheme.cs) |

## Fields

The record has the `ItemType` [common field](index.md#common-fields) and no `Id`. The built-in catalog uses the schema defaults. Colors use the `#RRGGBB` or `#AARRGGBB` [format](../../database.md#json-format). All colors are copied in [UiTheme.cs:296](../../../Assets/ModulesShared/Gui/Scripts/Theme/UiTheme.cs#L296).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `WindowColor` | color | `#50C0FF` | Windows. |
| `ScrollBarColor` | color | `#C050C0FF` | Scroll bars. |
| `IconColor` | color | `#80FFFF` | Icons. |
| `SelectionColor` | color | `#80FFFF` | Selected items. |
| `ButtonColor` | color | `#50C0FF` | Normal button. |
| `ButtonFocusColor` | color | `#4050C0FF` | Normal button, focus state. |
| `ButtonTextColor` | color | `#80FFFF` | Normal button text. |
| `ButtonIconColor` | color | `#E080FFFF` | Normal button icon. |
| `WarningButtonColor` | color | `#FF8050` | Warning-style button. |
| `WarningButtonFocusColor` | color | `#20FF8050` | Warning button, focus state. |
| `WarningButtonTextColor` | color | `#FFFFC0` | Warning button text. |
| `WarningButtonIconColor` | color | `#FFFFC0` | Warning button icon. |
| `PremiumButtonColor` | color | `#FFFFC0` | Premium-style button. |
| `PremiumButtonFocusColor` | color | `#40FFFFC0` | Premium button, focus state. |
| `PremiumButtonTextColor` | color | `#FFFFE0` | Premium button text. |
| `PremiumButtonIconColor` | color | `#FFFFC0` | Premium button icon. |
| `TextColor` | color | `#80FFFF` | Normal text. |
| `ErrorTextColor` | color | `#FF4040` | Error text. |
| `HeaderTextColor` | color | `#FFFFC0` | Headers. |
| `PaleTextColor` | color | `#A0FFFFFF` | Secondary text. |
| `BrightTextColor` | color | `#FFFFFF` | Highlighted text. |
| `BackgroundDark` | color | `#000000` | Dark backgrounds. |
| `LowQualityItemColor` | color | `#C0C0C0` | Items of quality `Low` ([UiTheme.cs:254](../../../Assets/ModulesShared/Gui/Scripts/Theme/UiTheme.cs#L254)). |
| `CommonQualityItemColor` | color | `#80FFFF` | Quality `Common`. |
| `MediumQualityItemColor` | color | `#80FF80` | Quality `Medium`. |
| `HighQualityItemColor` | color | `#F09FFF` | Quality `High`. |
| `PerfectQualityItemColor` | color | `#FFDF51` | Quality `Perfect`. |
| `AvailableTechColor` | color | `#FFFFC0` | Research tree: technology that the player can research now ([UiTheme.cs:283](../../../Assets/ModulesShared/Gui/Scripts/Theme/UiTheme.cs#L283)). |
| `UnavailableTechColor` | color | `#808080` | Research tree: technology that the player cannot research yet. |
| `ObtainedTechColor` | color | `#50C0FF` | Research tree: researched technology. |
| `HiddenTechColor` | color | `#8080FF` | Research tree: `Hidden` technology that the player has not researched ([TechItemViewModel.cs:48](../../../Assets/Scripts/Legacy/GUI/ViewModel/StarMap/TechItemViewModel.cs#L48)). |
| `CreditsColor` | color | `#00FF00` | Credits ([UiTheme.cs:268](../../../Assets/ModulesShared/Gui/Scripts/Theme/UiTheme.cs#L268)). |
| `StarsColor` | color | `#FFF0A0` | Stars (premium currency). |
| `MoneyColor` | color | `#FFF0A0` | In-app purchase prices. |
| `FuelColor` | color | `#00FFFF` | Fuel. |
| `TokensColor` | color | `#8080FF` | Tokens. |
| `MainMenuBackgroundImage` | image | | Background image of the main menu. Empty: the animated background ([MainMenu.cs:148](../../../Assets/Scripts/Gui/MainMenu/MainMenu.cs#L148)). |
| `NoCreditsText` | bool | | `true`: hides the credits text in the main menu ([MainMenu.cs:162](../../../Assets/Scripts/Gui/MainMenu/MainMenu.cs#L162)). |

## Notes

### How the theme loads

The services installer binds one `UiTheme` asset as the current theme. [`UiThemeLoader`](../../../Assets/ModulesShared/Gui/Scripts/Theme/UiThemeLoader.cs#L30) calls `UiTheme.Import` after every database load. Import overwrites every color of the asset with this record. The colors serialized in the asset therefore never show: a catalog without the record gets the schema defaults. Fonts and font sizes are not in the database.

The snowflake currency color is not a field. Import takes it from the color of [QuestItem](QuestItem.md) 25 ([UiTheme.cs:346](../../../Assets/ModulesShared/Gui/Scripts/Theme/UiTheme.cs#L346), [DB15.10](../../issues/database.md#db15-smaller-defects)).

### Main menu

The main menu reads `MainMenuBackgroundImage` and `NoCreditsText` at start and after each database load. The image lookup checks built-in sprites first, then mod images ([Media](../../database.md#media-images-and-audio)). `NoCreditsText` only hides the text. After the player switches to a catalog without it, the text stays hidden until the game restarts ([DB15.28](../../issues/database.md#db15-smaller-defects)).

### Data rules

1. Keep text colors readable on `WindowColor` and `BackgroundDark`.
2. Use alpha in focus colors. The defaults use alpha `20`–`40`.
3. A mod settings record replaces the whole record. Copy every value, not only the changed one ([DB7](../../issues/database.md#db7-settings-records-cannot-be-partly-overridden)).
