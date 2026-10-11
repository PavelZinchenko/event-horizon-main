# Gui Module

The Gui module is the shared uGUI layer. It has a window manager that opens and closes windows by name and applies stacking rules, an animated window component, a color and font theme, an in-game debug console, a component tree for the ship editor's inventory list, and layout helpers. It also carries two vendored plugins: `ListScrollRect` (a recycling scroll list) and Unity UI Extensions.

| | |
| --- | --- |
| Assembly | `Gui` ([Gui.asmdef](../Assets/ModulesShared/Gui/Gui.asmdef)). References: AppConfiguration, CommonComponents, ServicesFacade, Unity.InputSystem, Zenject, ShipConstructor, Localization, Database, Diagnostics, Utilites. AppConfiguration is not used. Auto-referenced. |
| Editor assemblies | `GuiThemeEditor` ([Theme/Editor](../Assets/ModulesShared/Gui/Scripts/Theme/Editor/)), `ListScrollRectEditor`, `UnityUIExtensions.editor` |
| Vendored runtime assembly | `UnityUIExtensions` (Unity UI Extensions 2.3.2) |
| Namespaces | `Services.Gui` (window manager API), `Gui.Windows`, `Gui.Theme`, `Gui.Theme.Wrappers`, `Gui.ComponentList`, `Gui.DebugConsole`, `Gui.Utils`. `LayoutGroupExtensions`, `ListScrollRect`, and `IContentFiller` are in the global namespace. |
| Location | [`Assets/ModulesShared/Gui/`](../Assets/ModulesShared/Gui/), inside the `Assets/ModulesShared` Git submodule. |
| Source | [`Scripts/`](../Assets/ModulesShared/Gui/Scripts/) (30 files, 2,740 lines; 2 of them editor files) |
| Plugins | [`Plugins/ListScrollRect/`](../Assets/ModulesShared/Gui/Plugins/ListScrollRect/) (4 files, 1,561 lines); [`Plugins/com.unity.uiextensions/`](../Assets/ModulesShared/Gui/Plugins/com.unity.uiextensions/) (191 C# files, about 31,700 lines) |
| Prefabs | [`Prefabs/`](../Assets/ModulesShared/Gui/Prefabs/) (6 prefabs) |
| Bindings | [`ServicesInstaller`](../Assets/Scripts/Installers/ServicesInstaller.cs) (window manager, window signals, theme loader, debug console logger); [`TestSceneInstaller`](../Assets/Scripts/Installers/TestSceneInstaller.cs) (window manager and signals; sets the theme directly) |
| Default theme | [`UiTheme.asset`](../Assets/Modules/AppConfiguration/ScriptableObjects/UiTheme.asset) in the [AppConfiguration module](app_configuration.md#assets) |
| Tests | None. |
| Known issues | [Gui issues](issues/gui.md) |

## Contents

1. [Shared code constraint](#shared-code-constraint)
2. [Terms](#terms)
3. [Source map](#source-map)
4. [Composition](#composition)
5. [Window manager](#window-manager)
6. [Window classes](#window-classes)
7. [Window components](#window-components)
8. [Themes](#themes)
9. [Component list](#component-list)
10. [Debug console](#debug-console)
11. [Layout helpers](#layout-helpers)
12. [Prefabs](#prefabs)
13. [Shared icons](#shared-icons)
14. [Vendored plugins](#vendored-plugins)
15. [Adding a window](#adding-a-window)
16. [Adding a theme color](#adding-a-theme-color)

## Shared code constraint

`Assets/ModulesShared` is a separate Git repository ([event-horizon-modules](https://github.com/PavelZinchenko/event-horizon-modules)). Its README says it holds "components shared between Event Horizon and Frontier". This repository does not contain Frontier, so the effect of a change on Frontier cannot be checked here.

The `asmdef` references show what any game that uses this module must provide:

1. Assemblies named `AppConfiguration`, `CommonComponents`, `ServicesFacade`, `ShipConstructor`, `Localization`, `Database`, `Diagnostics`, and `Utilites`.
2. The scene types `GameScene`, `SceneLoadedSignal`, and `SceneBeforeUnloadSignal` from `ServicesFacade` (the window manager uses them).
3. `GameDatabase.IDatabase` with `UiSettings`, `DebugSettings`, `WeaponSlots`, and `GetQuestItem` (the theme loader, the debug console, and the component tree use them).
4. `Economy.Currency`, `Economy.Price`, and `Economy.ItemType.ItemQuality` from `CommonComponents`.
5. Windows named `ShowMessageWindow`, `ConfirmationDialog`, and `BuyConfirmationDialog`, when code calls the helpers in [`GuiManagerExtensions`](#helpers).

Some public types have no user in Event Horizon: `ShipNode`, `ShipGroupNode`, `ComponentListNode`, `ComponentQuantityProvider`, `BlueprintQuantityProvider`, and the `Gui.ComponentList.ComponentListItem` component. Frontier may use them. Do not delete them without checking Frontier.

## Terms

| Term | Meaning |
| --- | --- |
| Window | A scene object with a component that implements `IWindow`: `Window` or `AnimatedWindow`. |
| Window ID | The name of the window's GameObject. The window manager finds windows by this name. |
| Window class | A `WindowClass` value on the window. It decides which windows close, block, or disable each other. |
| Active window | A window that the manager counts as open. |
| Enabled window | An active window whose `CanvasGroup.interactable` is true. A disabled window is visible but ignores input. |
| Exit code | A `WindowExitCode` (`Ok`, `Cancel`, `Option1`, `Option2`) that a window reports when it closes. |
| Automatic window | A window of class `HudElement` or `Singleton`. When it closes with any exit code except `Ok`, the manager reopens it as soon as nothing blocks it. |
| Theme | A `UiTheme` asset with named colors, font sizes, and fonts. `UiTheme.Current` is the active theme. |
| Theme wrapper | `ThemedImage` or `ThemedText`: a uGUI `Image` or `Text` that takes its color, and for text its font, from the theme. |

## Source map

| File | Contents |
| --- | --- |
| [Core/IGuiManager.cs](../Assets/ModulesShared/Gui/Scripts/Core/IGuiManager.cs) | `IGuiManager`: find, open, and close windows; `AutoWindowsAllowed`. |
| [Core/GuiManager.cs](../Assets/ModulesShared/Gui/Scripts/Core/GuiManager.cs) | `GuiManager` and the signals `WindowOpenedSignal`, `WindowClosedSignal`, `WindowDestroyedSignal`, `EscapeKeyPressedSignal`. |
| [Core/IWindow.cs](../Assets/ModulesShared/Gui/Scripts/Core/IWindow.cs) | `IWindow`, `EscapeKeyAction`. |
| [Core/WindowClass.cs](../Assets/ModulesShared/Gui/Scripts/Core/WindowClass.cs) | `WindowClass` and the rules between classes. |
| [Core/WindowArgs.cs](../Assets/ModulesShared/Gui/Scripts/Core/WindowArgs.cs) | `WindowArgs`: positional, untyped window arguments. |
| [Core/WindowExitCode.cs](../Assets/ModulesShared/Gui/Scripts/Core/WindowExitCode.cs) | `WindowExitCode`. |
| [Core/Window.cs](../Assets/ModulesShared/Gui/Scripts/Core/Window.cs) | `Window`: a window without animation. |
| [Core/AnimatedWindowBase.cs](../Assets/ModulesShared/Gui/Scripts/Core/AnimatedWindowBase.cs), [AnimatedWindow.cs](../Assets/ModulesShared/Gui/Scripts/Core/AnimatedWindow.cs) | `AnimatedWindow`: a window driven by an `Animator`. |
| [Core/WindowInitializedEvent.cs](../Assets/ModulesShared/Gui/Scripts/Core/WindowInitializedEvent.cs) | Serializable `UnityEvent<WindowArgs>` and `UnityEvent<bool>`. |
| [Utils/GuiManagerExtensions.cs](../Assets/ModulesShared/Gui/Scripts/Utils/GuiManagerExtensions.cs) | `ShowMessage`, `ShowConfirmationDialog`, `ShowBuyConfirmationDialog`. |
| [Utils/LayoutGroupExtensions.cs](../Assets/ModulesShared/Gui/Scripts/Utils/LayoutGroupExtensions.cs) | Fill a layout group from a list by reusing and cloning child items. |
| [Theme/UiTheme.cs](../Assets/ModulesShared/Gui/Scripts/Theme/UiTheme.cs) | `UiTheme` ScriptableObject; enums `ThemeColor`, `TechColor`, `ThemeColorMode`, `ThemeFont`, `ThemeFontSize`; `ColorExtension`. |
| [Theme/UiThemeLoader.cs](../Assets/ModulesShared/Gui/Scripts/Theme/UiThemeLoader.cs) | Copies database colors into the default theme. |
| [Theme/Wrappers/ThemedImage.cs](../Assets/ModulesShared/Gui/Scripts/Theme/Wrappers/ThemedImage.cs), [ThemedText.cs](../Assets/ModulesShared/Gui/Scripts/Theme/Wrappers/ThemedText.cs) | Theme wrappers. Menu **UI > ThemedImage** and **UI > ThemedText**. |
| [Theme/Editor/Wrappers/](../Assets/ModulesShared/Gui/Scripts/Theme/Editor/Wrappers/) | Inspectors that show the theme fields above the standard `Image` and `Text` fields. |
| [ComponentList/](../Assets/ModulesShared/Gui/Scripts/ComponentList/) (8 files) | Component tree nodes, quantity providers, list items, and `ComponentContentFiller` for `ListScrollRect`. |
| [DebugConsole/](../Assets/ModulesShared/Gui/Scripts/DebugConsole/) (3 files) | `DebugConsoleLogger` (service), `DebugConsole` and `DebugConsoleLogEntry` (UI). |

## Composition

[`ServicesInstaller`](../Assets/Scripts/Installers/ServicesInstaller.cs) in `LoaderScene` binds the module into the project container:

| Binding | Lifetime | Notes |
| --- | --- | --- |
| `GuiManager` as `IGuiManager`, `IInitializable`, `IDisposable`, `ITickable` | Single, non-lazy | Exists for the whole session. |
| `WindowOpenedSignal`, `WindowClosedSignal`, `WindowDestroyedSignal`, `EscapeKeyPressedSignal` and their triggers | Single | Weak signals from [CommonComponents](common_components.md). |
| `UiThemeLoader` | Single | Argument: `_defaultUiTheme` (the `UiTheme.asset` of AppConfiguration). |
| `DebugConsoleLogger` as `IDebugConsoleLogger`, `ITickable`, and the lifecycle interfaces | Single | |

[`TestSceneInstaller`](../Assets/Scripts/Installers/TestSceneInstaller.cs) binds `GuiManager` and the signals for test scenes. It sets `UiTheme.Current` directly and does not bind `UiThemeLoader` or `DebugConsoleLogger`.

Window components, theme wrappers, and list items receive services through `[Inject]`. They must be in a scene with a Zenject context, or be created with `IGameObjectFactory`.

## Window manager

[`GuiManager`](../Assets/ModulesShared/Gui/Scripts/Core/GuiManager.cs) keeps four collections:

| Collection | Contents |
| --- | --- |
| `_windows` | All known windows, by ID. |
| `_activeWindows` | Windows that are open now. |
| `_showWhenPossibleWindows` | Automatic windows that wait to reopen. |
| `_onCloseActions` | One close callback per window. |

### Finding windows

1. `Initialize` scans every loaded scene. It takes all `IWindow` components under the root objects, including inactive ones.
2. On `SceneLoadedSignal`, it scans the new scene the same way. On `SceneBeforeUnloadSignal`, it removes that scene's windows. A pending close callback of a removed window runs with `Cancel`.
3. A window that is visible at registration is counted as active.
4. A second window with the same ID calls `Debug.Break()` and then throws. See [GU4](issues/gui.md#gu4-a-duplicate-window-id-stops-the-scene-registration).

The manager finds windows in scenes only. A window in a prefab must be placed in a scene. A window that is created later, for example with `Instantiate`, is unknown to the manager.

### Opening a window

`OpenWindow(id, [args], [onClose])` ([line 53](../Assets/ModulesShared/Gui/Scripts/Core/GuiManager.cs#L53), [line 70](../Assets/ModulesShared/Gui/Scripts/Core/GuiManager.cs#L70)):

1. Find the window by ID. An unknown ID throws `ArgumentException` without a message.
2. Store the close callback. The overload without arguments logs a warning when a callback already exists. The overload with arguments throws. See [GU3](issues/gui.md#gu3-openwindow-with-arguments-throws-when-a-callback-is-pending).
3. Call `IWindow.Open(args)`. The window fires `WindowOpenedSignal(id)`.

The manager handles the signal in `OnWindowOpened` ([line 110](../Assets/ModulesShared/Gui/Scripts/Core/GuiManager.cs#L110)). For each other active window, it applies the [class rules](#window-classes):

1. If the new window cannot open because of an active window, the new window closes at once with `Cancel`. Processing stops.
2. If an active window must close because of the new window, it is put on a list. The list closes after the new window is active.
3. If an active window must be disabled because of the new window, it is disabled.
4. If the new window must be disabled because of an active window, it opens disabled.

Then the new window is added to the active set.

### Closing a window

A window closes itself and fires `WindowClosedSignal(id, exitCode)`. `OnWindowClosed` ([line 158](../Assets/ModulesShared/Gui/Scripts/Core/GuiManager.cs#L158)):

1. Remove the window from the active set.
2. Recompute `Enabled` for every active window. A window is enabled when no other active window disables it.
3. Run and remove the close callback with the exit code.
4. If the window is automatic and the exit code is not `Ok`, queue it to reopen.
5. Try to reopen queued windows.

`CloseAllWindows(predicate)` closes the matching active windows. It turns `AutoWindowsAllowed` off during the loop, so closed automatic windows do not reopen in the middle. When it restores the old value, queued automatic windows reopen unless an open window blocks them.

### Automatic windows

`HudElement` and `Singleton` windows are automatic. They are panels that should be on screen whenever nothing covers them. A `TopLevel` window closes them; when it closes, they come back.

1. When an automatic window closes with an exit code other than `Ok`, it goes into `_showWhenPossibleWindows`. Adding it removes queued windows that its class would close. So only the last queued `Singleton` survives.
2. `TryOpenShowWhenPossibleWindows` opens each queued window that no active window blocks or would close. It stops after 100 windows and then throws `InvalidOperationException`.
3. While `AutoWindowsAllowed` is false, nothing reopens. Setting it to true reopens the queue. [`TravelState`](../Assets/Scripts/GameStateMachine/States/TravelState.cs) and [`RetreatState`](../Assets/Scripts/GameStateMachine/States/RetreatState.cs) turn it off while they run.

To close an automatic window for good, close it with `Ok`. `AnimatedWindow.Close(WindowExitCode)` allows this. `Window.Close()` always reports `Cancel`.

### Escape key

`Tick` ([line 350](../Assets/ModulesShared/Gui/Scripts/Core/GuiManager.cs#L350)) reads `Keyboard.current.escapeKey` from the Input System every frame.

1. If any active window has `EscapeKeyAction.Block`, nothing happens.
2. Otherwise, among enabled windows with `EscapeKeyAction.Close`, the manager picks one by close priority and closes it with `Cancel`. Balloons win, then modal dialogs. Between two other windows, the later one in the set wins; the order of a `HashSet` is not defined.
3. If no window closes, the manager fires `EscapeKeyPressedSignal`. [`StarMapState`](../Assets/Scripts/GameStateMachine/States/StarMapState.cs), [`ExplorationState`](../Assets/Scripts/GameStateMachine/States/ExplorationState.cs), and [`BackButtonEvent`](../Assets/Scripts/Gui/Utils/BackButtonEvent.cs) listen to it.

`Keyboard.current` is `null` when no keyboard device exists. See [GU1](issues/gui.md#gu1-the-escape-check-has-no-null-check-for-the-keyboard).

### Window arguments

`WindowArgs` wraps `params object[]`. `Get<T>(index)` casts and throws on a wrong index or type. `TryGet<T>(index, out value)` returns `false` instead. A window receives the arguments in its `OnInitializedEvent`. When `OpenWindow` has no arguments, the window receives an empty `WindowArgs`. The weak typing is an application-level problem; see [application issue 8](issues/application_layer.md#8-ui-talks-to-domain-services-directly-and-passes-untyped-arguments).

### Helpers

[`GuiManagerExtensions`](../Assets/ModulesShared/Gui/Scripts/Utils/GuiManagerExtensions.cs) opens three windows that the application must provide:

| Method | Window ID | Arguments | Callback |
| --- | --- | --- | --- |
| `ShowMessage(text)` | `ShowMessageWindow` | text | none |
| `ShowConfirmationDialog(text, action)` | `ConfirmationDialog` | text | `action` runs on `Ok` |
| `ShowBuyConfirmationDialog(text, price, action)` | `BuyConfirmationDialog` | text, `Economy.Price` | `action` runs on `Ok` |

### Signals

| Signal | Fired by | Used by |
| --- | --- | --- |
| `WindowOpenedSignal(id)` | `Window`, `AnimatedWindow` | `GuiManager` only. |
| `WindowClosedSignal(id, exitCode)` | `Window`, `AnimatedWindow` | `GuiManager` only. |
| `WindowDestroyedSignal(window)` | Nothing. | `GuiManager` (handler is dead). See [GU12](issues/gui.md#gu12-windowdestroyedsignal-is-never-fired). |
| `EscapeKeyPressedSignal` | `GuiManager` | Star map and exploration states, `BackButtonEvent`; the application [`SignalsTranslator`](../Assets/Scripts/Services/Messenger/SignalsTranslator.cs) forwards it to the messenger as `EventType.EscapeKeyPressed`. |

## Window classes

[`WindowClass.cs`](../Assets/ModulesShared/Gui/Scripts/Core/WindowClass.cs) defines six classes and four rules.

| Class | Intended use | Automatic |
| --- | --- | --- |
| `HudElement` | Always-on panel, such as a HUD part. Default for `Window`. | Yes |
| `TopLevel` | Full-screen window that replaces the current view. | No |
| `Singleton` | Main panel; only one at a time. Default for `AnimatedWindow`. | Yes |
| `Common` | Ordinary panel. No serialized window uses it. | No |
| `ModalDialog` | Dialog that blocks everything below it. | No |
| `Balloon` | Small popup, such as a tooltip; only one at a time. | No |

The rules, when window **N** opens and window **A** is active:

| Rule | Method | Result |
| --- | --- | --- |
| N cannot open | `N.CantBeOpenedDueTo(A)` | A is `ModalDialog` and N is not `Balloon`; or A is `TopLevel` and N is `HudElement`, `Singleton`, or `Common`. |
| A closes | `A.MustBeClosedDueTo(N)` | N is `TopLevel` and A is not `ModalDialog` or `Balloon`; or N is `Singleton` and A is `Singleton` or `Common`; or both are `Balloon`. |
| A is disabled | `A.MustBeDisabledDueTo(N)` | N is `ModalDialog` or `TopLevel`, and A is not `ModalDialog` or `Balloon`. |
| N opens disabled | `N.MustBeDisabledDueTo(A)` | A is `ModalDialog` or `TopLevel`, and N is not `ModalDialog` or `Balloon`. |

Consequences:

1. While a modal dialog is open, only balloons can open. Two modal dialogs cannot be open at once; the second one closes at once.
2. A top-level window closes all panels and HUD parts. They come back when it closes.
3. Balloons are never disabled or blocked, except by another balloon.

## Window components

### Window

[`Window`](../Assets/ModulesShared/Gui/Scripts/Core/Window.cs) shows and hides its GameObject. It needs a `CanvasGroup`.

| Field | Default | Meaning |
| --- | --- | --- |
| `_class` | `HudElement` | Window class. |
| `_escapeKeyAction` | `None` | `None`, `Block`, or `Close`. |
| `OnInitializedEvent` | | `UnityEvent<WindowArgs>` on every `Open`. |
| `OnActivatedEvent` | | `UnityEvent<bool>`: true when visible and enabled. |

`Open` activates the GameObject, invokes `OnInitializedEvent`, and fires the opened signal. `Close` deactivates it and fires the closed signal with `Cancel`. There is no way to set another exit code. One `Window` exists, in `StarMapScene`.

### AnimatedWindow

[`AnimatedWindow`](../Assets/ModulesShared/Gui/Scripts/Core/AnimatedWindow.cs) plays an open and a close animation. All other windows in the project use it.

| Field | Default | Meaning |
| --- | --- | --- |
| `_class` | `Singleton` | Window class. |
| `_waitOpenAnimationDone` | false | Fire the opened signal after the open animation, not before. |
| `_waitCloseAnimationDone` | false | Fire the closed signal after the close animation, not before. |
| `_initiallyHidden` | false | Deactivate the GameObject during injection. |
| `_escapeKeyAction` | `None` | `None`, `Block`, or `Close`. |
| Events | | `OnInitializedEvent(args)`, `OnWindowOpeningEvent`, `OnWindowOpenedEvent`, `OnWindowClosingEvent`, `OnWindowClosedEvent`. |

The `Animator` on the same object must have:

1. A bool parameter `IsVisible`.
2. A state `Open` and a state `Closed` on layer 0.

The controllers in [`Assets/Animation/`](../Assets/Animation/) (`CenterPanel`, `FadeoutPanel`, `SidePanel`, `SidePanelLeft`) follow this contract.

Life cycle ([AnimatedWindowBase.cs](../Assets/ModulesShared/Gui/Scripts/Core/AnimatedWindowBase.cs)):

1. **Open.** Activate the GameObject, call `OnWindowOpening`, set `IsVisible` to true, and start a coroutine. The coroutine waits until the animator is not in a transition and is in state `Open`, then calls `OnWindowOpened`.
2. **Close.** Call `OnWindowClosing`, set `IsVisible` to false, and wait for state `Closed`. Then deactivate the GameObject and call `OnWindowClosed`.
3. During the opening and closing phases, `CanvasGroup.blocksRaycasts` is false, so the window ignores clicks while it animates.
4. A GameObject that is active when the scene starts counts as open from the start. `OnEnable` sets the open flag.

`IsVisible` returns the open flag, not `activeSelf`. A window that is closing reports `false` while its animation still plays.

An open from the inactive state calls `OnWindowOpened` twice. See [GU11](issues/gui.md#gu11-an-animated-window-reports-opened-twice).

## Themes

A theme is a [`UiTheme`](../Assets/ModulesShared/Gui/Scripts/Theme/UiTheme.cs) ScriptableObject (menu **Create > AppConfig > UiTheme**). The project has one, [`UiTheme.asset`](../Assets/Modules/AppConfiguration/ScriptableObjects/UiTheme.asset). A mod changes its colors through the [UiSettings](database/schema/UiSettings.md) record.

### Current theme

`UiTheme.Current` is a static property. When nothing is assigned, the getter creates an empty instance with the default field values from the code.

1. The `UiThemeLoader` constructor sets `Current` to the default theme asset.
2. `Initialize` subscribes to `IDatabase.DatabaseLoaded` and imports at once.
3. On every database load, `UiTheme.Import(database)` copies the colors from `UiSettings` into the asset. It also copies the color of quest item 25 (snowflakes) into the snowflake color, if that item exists.

`Import` writes into the asset object itself. In the Unity Editor the imported colors stay in the asset after Play mode ends. See [GU8](issues/gui.md#gu8-the-theme-import-changes-the-shared-asset).

`Import` does not change fonts or font sizes. They come only from the asset.

### Colors

`GetColor(ThemeColor)` returns a color. `ThemeColor.Default` and unknown values throw `InvalidOperationException`.

| `ThemeColor` | `UiSettings` field |
| --- | --- |
| `Window`, `ScrollBar`, `Icon`, `Selection`, `BackgroundDark` | `WindowColor`, `ScrollBarColor`, `IconColor`, `SelectionColor`, `BackgroundDark` |
| `Button`, `ButtonFocus`, `ButtonText`, `ButtonIcon` | `ButtonColor`, `ButtonFocusColor`, `ButtonTextColor`, `ButtonIconColor` |
| `WarningButton*` (4 values) | `WarningButton*Color` |
| `PremiumButton*` (4 values) | `PremiumButton*Color` |
| `Text`, `HeaderText`, `PaleText`, `BrightText`, `ErrorText` | `TextColor`, `HeaderTextColor`, `PaleTextColor`, `BrightTextColor`, `ErrorTextColor` |
| `Credits`, `Stars`, `Money`, `Fuel`, `Tokens` | `CreditsColor`, `StarsColor`, `MoneyColor`, `FuelColor`, `TokensColor` |
| `Snowflakes` | Color of quest item 25 |

Other lookups:

| Method | Key | Source |
| --- | --- | --- |
| `GetQualityColor` | `Economy.ItemType.ItemQuality` (Low, Common, Medium, High, Perfect) | `UiSettings.*QualityItemColor` |
| `GetCurrencyColor` | `Economy.Currency` | Same fields as the currency colors. `None` returns white. |
| `GetTechColor` | `TechColor` (Available, NotAvailable, Obtained, Hidden) | `UiSettings.*TechColor` |

`ThemeColorMode` changes a color after lookup: `SemiTransparent25/50/75` multiplies alpha; `Brightness25/50` and `Brightbess75` (sic) multiply RGB.

### Fonts and sizes

`ThemeFont` has `Font1`–`Font3`. Each maps to a `FontInfo` (a `Font` and a `SizeMultiplier`). When the font is not set, `GetFont` logs an editor-only error and returns the built-in Arial with multiplier 1.

`ThemeFontSize` names eight sizes: `SmallText18`, `SmallText20`, `CompactText22`, `ButtonText24`, `NormalText24`, `TitleText26`, `LargeText32`, `LargeText38`. The number in the name is the default size in the asset. The enum values are not in size order; they are serialized in scenes, so do not renumber them.

### Theme wrappers

`ThemedImage` and `ThemedText` replace `Image` and `Text`. They apply the theme once, in `Start`, and only in Play mode:

1. If the color was not set by code before `Start`, and `_themeColor` is not `Default`, set the color from the theme with the color mode.
2. `ThemedText` only: if `_themeFont` is not `Default`, set the font. Set the size to the theme size (or the current size when `_themeFontSize` is `Default`) times the font's multiplier. With best fit, scale the minimum size and set the maximum size the same way.
3. Exceptions are logged in the Editor only.

The wrappers do not listen for theme changes. An element that has started keeps its colors until the scene reloads. See [GU7](issues/gui.md#gu7-theme-changes-do-not-reach-ui-that-already-exists).

Direct users of `UiTheme.Current` in code: item colors in [ShipEditor](ship_editor.md), the application's item types ([`Scripts/Domain/Economy/ItemType/`](../Assets/Scripts/Domain/Economy/ItemType/)), and several list items in `Assets/Scripts/Gui/`.

## Component list

[`Gui.ComponentList`](../Assets/ModulesShared/Gui/Scripts/ComponentList/) builds the grouped inventory list of the ship editor. The [ShipEditor](ship_editor.md#ui) `ComponentsPanel` is the only user in Event Horizon.

### Tree

`IComponentTreeNode` has a parent, child nodes, components, an item count, a name (a localization key), an icon, and a color.

| Node | Children | Name and icon |
| --- | --- | --- |
| `RootNode` | Extra nodes (front), weapons, armor, energy, drones, engines, special, extra nodes (end) | `$GroupAll`, `icons/icon_gear` |
| `WeaponNode` | One `CommonNode` per weapon slot from the [WeaponSlots](database/schema/WeaponSlots.md) settings, plus one default slot | `$GroupWeapon`, `textures/icons/icon_weapon` |
| `CommonNode` | One `ComponentNode` per component ID | Given by the parent |
| `ComponentNode` | The `ComponentInfo` values (component plus modification) of one component | Component name and icon |
| `SatellitesNode` | None. `ItemCount` is fixed at 10. | `$GroupLeftSatellite` or `$GroupRightSatellite`, `icons/icon_scanner` |

`RootNode.Add` sorts a component by type: a component with a weapon goes to the weapon node, which sorts by `WeaponSlotType`. Other components go by `DisplayCategory`. An unknown weapon slot goes to the default slot and logs an error.

`RootNode` needs a database to create the weapon node. Without it (as `SatellitesPanel` creates it, only for a back button), adding a weapon throws.

### Expansion

A group node with few items is shown inline instead of as a group ([IComponentTreeNode.cs:30](../Assets/ModulesShared/Gui/Scripts/ComponentList/IComponentTreeNode.cs#L30)). `ShouldExpand(node)` is true when:

1. the node is not visible; or
2. the node has a parent, uses the same quantity provider as its parent (or has no items), and has fewer than 3 items.

An expanded node's children appear in its parent's list. `GetVisibleParent` walks up past expanded nodes; the **Back** button uses it.

Item counts are cached. The cache is not reset after `Clear`, and never in `WeaponNode`. See [GU9](issues/gui.md#gu9-group-item-counts-are-cached-and-go-stale).

### List filling

[`ComponentContentFiller`](../Assets/ModulesShared/Gui/Scripts/ComponentList/ComponentContentFiller.cs) implements `IContentFiller` for `ListScrollRect`:

1. `InitializeItems(node)` collects the visible parent groups (if `_showParents`), the node itself (if `_showSelected`), the child nodes, and the node's components with quantity above 0.
2. Item type 0 is a group (`GroupListItem`); type 1 is a component (`ComponentListItemBase`).
3. New items are created with `IGameObjectFactory`, so they are injected.
4. `SelectedItem` fires `_itemSelectedEvent` when it changes.

Quantities come from an `IComponentQuantityProvider`. The ship editor supplies its own provider.

## Debug console

The debug console shows log messages inside the game. It is meant for mod authors.

### Logger service

[`DebugConsoleLogger`](../Assets/ModulesShared/Gui/Scripts/DebugConsole/DebugConsoleLogger.cs) is enabled when:

1. a database is loading from editable storage (a folder, not a packed mod); or
2. after loading, `IDatabase.IsEditable` is true, or [DebugSettings](database/schema/DebugSettings.md) `EnableDebugConsole` is true.

While enabled:

1. It sets itself as [`GameDiagnostics.Trace.Logger`](diagnostics.md#redirecting-trace). Every `Trace` call is stored and forwarded to the Unity log.
2. It listens to `Application.logMessageReceivedThreaded` and stores entries of type `Exception` only.
3. In `Tick`, if a stored entry is not a plain log (warnings count), it fires `MessageReceived`.

The store is a list under a lock. `GetMessages(list, max)` copies the last `max` entries and clears the store. Nothing else limits its size. See [GU6](issues/gui.md#gu6-the-debug-console-store-has-no-size-limit).

### UI

[`DebugConsole`](../Assets/ModulesShared/Gui/Scripts/DebugConsole/DebugConsole.cs) is in [`CommonGuiScene`](../Assets/Scenes/CommonGuiScene.unity). On `MessageReceived` it shows its button. When the console panel is enabled, it takes up to `_maxMessageCount` (100 in the scene) entries and fills its layout group with `DebugConsoleLogEntry` items. An entry with a stack trace shows an icon; its toggle shows or hides the trace.

The entries are removed from the store when the panel shows them. Reopening the panel shows only new entries.

## Layout helpers

[`LayoutGroupExtensions`](../Assets/ModulesShared/Gui/Scripts/Utils/LayoutGroupExtensions.cs) fills a container from a data list. The container's children are the item views.

`transform.InitializeElements<TView, TModel>(data, initializer, ...)`:

1. Walk the children that have a `TView` component. Give each one the next data item and activate it. Deactivate the rest.
2. If data remains, clone the last view for each extra item and insert it after the last one.
3. Return the number of active items.

| Overload | Clones with | Extra |
| --- | --- | --- |
| `(data, initializer, IGameObjectFactory factory)` | `factory.Create` (injected) | |
| `(data, initializer, bool explicitType = false)` | `GameObject.Instantiate` (not injected) | `explicitType` skips views of a derived type. `data` can be `null`. |

The container needs at least one view as a template. With no template, extra data is ignored.

`EnumerateElements<T>()` returns the `T` components of the direct children.

## Prefabs

| Prefab | Used in |
| --- | --- |
| [Scrollbar](../Assets/ModulesShared/Gui/Prefabs/Scrollbar.prefab) | `CommonGuiScene`, `SettingsScene`, `ShipEditorScene`, `RightPanel.prefab` |
| [SettingsSlider](../Assets/ModulesShared/Gui/Prefabs/SettingsSlider.prefab) | `SettingsScene` |
| [Buttons/IconButton_128](../Assets/ModulesShared/Gui/Prefabs/Buttons/IconButton_128.prefab) | `SettingsScene`, `ShipEditorScene` |
| [Buttons/MainMenuButton](../Assets/ModulesShared/Gui/Prefabs/Buttons/MainMenuButton.prefab) | `MainMenuScene` |
| [Buttons/ToggleIconButton_128](../Assets/ModulesShared/Gui/Prefabs/Buttons/ToggleIconButton_128.prefab) | `SettingsScene` |
| [Buttons/ToggleIconButton_96](../Assets/ModulesShared/Gui/Prefabs/Buttons/ToggleIconButton_96.prefab) | `ShipEditorScene`, `ToggleIconButton_128` (nested) |

The prefabs use `ThemedImage` and `ThemedText`, so they follow the theme.

## Shared icons

[`Assets/ModulesShared/Resources/Icons/`](../Assets/ModulesShared/Resources/Icons/) holds 22 PNG icons. The folder is part of the `ModulesShared` submodule, but not of any assembly. The [media guide](resources/sprites.md#shared-resources-icons) lists it with the other sprites.

They reach the game by three routes:

| Route | Icons |
| --- | --- |
| `Resources.Load` by path through a `SpriteId` of type `Default` (see [ResourceLocator](resource_locator.md)). Unity matches `Resources` paths without regard to case. | The component tree groups: `icons/icon_shield`, `icon_battery`, `icon_drone`, `icon_engine`, `icon_gear`, `icon_scanner`, `icon_fleet`. |
| The hand-edited `_guiIconSprites` list of the `ResourceLocator` in `LoaderScene`, looked up by name as `SpriteId.Type.GuiIcon` | `WeaponGroup/icon_weapon_c`, `_l`, `_m`, `_s`, `_t`, `_x`: weapon slot icons named in [WeaponSlots](database/schema/WeaponSlots.md). |
| Serialized references in scenes and prefabs | All other icons (`icon_exit`, `icon_ship`, `icon_swords`, `icon_remove`, `icon_undo`, `icon_eye`, `icon_refresh`, `icon_journal`, `icon_left`) and most of the above. |

`WeaponNode` uses `textures/icons/icon_weapon`, which is in `Assets/Resources/Textures/Icons/`, not in the shared folder. Frontier needs that path too if it uses the weapon node.

Rules:

1. Do not rename or move an icon that code loads by path. The path is a string in `ComponentTreeNode.cs`, `SatellitesNode.cs`, or `ShipNode.cs`.
2. A new weapon slot icon must be added to `_guiIconSprites` by hand. **Reload** on the locator does not collect it.

## Vendored plugins

### ListScrollRect

[`ListScrollRect`](../Assets/ModulesShared/Gui/Plugins/ListScrollRect/Scripts/ListScrollRect.cs) is a `ScrollRect` that creates views only for the visible items and reuses them while scrolling. It is an Asset Store package (the `.meta` files say `licenseType: Store`). No license file or version is in the folder.

Contract:

1. The `contentFillerInterface` GameObject must have a component that implements [`IContentFiller`](../Assets/ModulesShared/Gui/Plugins/ListScrollRect/Scripts/IContentFiller.cs): `GetItemCount()`, `GetItemType(index)`, and `GetListItem(index, itemType, reusedObject)`.
2. `GetListItem` gets a pooled object of the same item type, or `null`. It must return a configured object. When it gets `null`, it must create one.
3. With `initializeOnStart`, the list initializes in `Start`. Otherwise call `Initialize()`.
4. Call `RefreshContent()` after the data changes. It keeps the scroll position. `RebuildContent()` destroys all views and starts again.
5. The list adds a `ListScrollRectHelper` to the content object. It marks the list for refresh when the content width (vertical list) or height (horizontal list) changes.
6. Item size can vary. The list measures each item and estimates the total from the average.

Users: the ship editor (`ComponentsPanel`, `ShipsPanel`) and application lists (`FleetPanel` (two), `MultiplayerWindow`, `SpecialStore`, `CargoHold`, `IapStore`, `ManageShipsPanel`, `ScrapPanelViewModel`, `MarketDialog`).

### Unity UI Extensions

[`com.unity.uiextensions/`](../Assets/ModulesShared/Gui/Plugins/com.unity.uiextensions/) is a copy of [Unity UI Extensions](https://github.com/Unity-UI-Extensions/com.unity.uiextensions) 2.3.2 (BSD 3-clause license, see its `LICENSE.md`). It is not installed through the Package Manager.

The project uses one component from it: `FlowLayoutGroup`, in `CommonGuiScene`. No code refers to the `UnityEngine.UI.Extensions` namespace. The other 190 files compile into every build. See [GU15](issues/gui.md#gu15-the-ui-extensions-copy-is-almost-unused).

## Adding a window

1. Create a GameObject under a canvas in the scene that should host the window. Its name is the window ID. The name must be unique among all loaded scenes.
2. Add a `CanvasGroup` and an `AnimatedWindow`. Add an `Animator` with a controller that follows the [contract](#animatedwindow), such as `CenterPanel`.
3. Choose the class. Use `ModalDialog` for dialogs, `TopLevel` for full-screen windows, and `Singleton` for main panels.
4. Set `_escapeKeyAction` to `Close` if Escape or Back should close it.
5. Connect `OnInitializedEvent` to a method that reads the `WindowArgs`.
6. Close the window with `AnimatedWindow.Close(WindowExitCode)`. Use `Ok` to keep an automatic window closed.
7. Add the ID to the `WindowNames` class of the owning `Gui/` folder in the application.
8. Open it with `IGuiManager.OpenWindow(WindowNames.X, new WindowArgs(...), onClose)`.

## Adding a theme color

1. Add the value to `ThemeColor`. Do not renumber existing values; scenes store the numbers.
2. Add a serialized field with a default to `UiTheme`, and a case in `GetColor`.
3. If mods should change it, add the field to the `UiSettings` schema, run the [database code generator](database.md#generated-code), and copy it in `Import`.
4. Set the color in `UiTheme.asset`.
5. Check Frontier, which compiles the same enum and class.
