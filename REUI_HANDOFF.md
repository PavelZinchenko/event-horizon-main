# ReUI 开发交接与改动记录

> 本文件用于 ChatGPT、Codex 或其他开发会话继续处理 ReUI 独立 Unity 工程。
> 后续每完成一个阶段，都应在“阶段记录”中追加：修改文件、修改目的、验证结果、已知问题和下一步。

## 1. 工程位置与边界

- 原项目根目录：`C:\Users\wjy25\Documents\Re-Threebody`
- Git/原 Beta3 工作目录：`C:\Users\wjy25\Documents\Re-Threebody\upstream`
- ReUI 独立 Unity 工程：`C:\Users\wjy25\Documents\Re-Threebody\upstream\ReUI`
- Git 分支：`codex/preview12`
- 基础提交：`a7196a15ff81`
- Unity 版本：`6000.0.75f1`
- Unity 编辑器：`C:\Program Files\Unity\Hub\Editor\6000.0.75f1\Editor\Unity.exe`

### 强制安全约束

1. 所有新增和修改只能发生在 `upstream\ReUI` 内。
2. 不得修改 `upstream` 下原 Beta3 工程的任何文件。
3. 不得执行 `git reset`、`git clean`、覆盖式 `checkout` 或其他可能丢失未提交内容的操作。
4. 不删除、清理或覆盖原 Beta3 工程。
5. 不推送 GitHub，不上传文件，不发送邮件。
6. 可以运行 Unity 编译、测试、构建和只读检查。
7. 修复问题时，只修复 `ReUI` 内文件。
8. 不应在未完成编译和画面验证前批量修改场景 YAML。

## 2. APK 状态

截至 2026-07-21，当前最新 ReUI Android 测试 APK 为：

- 文件：`Builds/Android/ThreeBody-EventHorizon-Beta3-ReUI12.apk`
- 绝对路径：`C:\Users\wjy25\Documents\Re-Threebody\upstream\ReUI\Builds\Android\ThreeBody-EventHorizon-Beta3-ReUI12.apk`
- 文件大小：`118,999,911` 字节
- SHA-256：`0a5a93a38e8e3a46eff3f96d79cddff6e77455fdf6047cc37d39345dd752dedf`
- 包名：`com.threebody.EventHorizon`
- 应用名：`三体视界`
- versionName：`Beta3-ReUI12`
- versionCode：`120012`
- buildNumber：`1880`
- minSdk：`23`
- targetSdk / compileSdk：`34`
- 原生架构：`arm64-v8a`
- 脚本后端：`IL2CPP`
- 图形 API：`OpenGLES3`
- 签名：Android Debug，APK Signature Scheme v1、v2 验证通过

旧版 ReUI APK 仍保留在同一目录用于真机对比。`ReUI5` 重点修复科技图标被错误隐藏、三体科技页在旧存档中不可点击、战斗舰船资源数字被统一刷白，以及战斗暂停菜单仍使用旧图标的问题。该包仍属于测试包，不是应用商店发布签名包。由于包名与原版一致，如果设备上已有使用不同证书签名的同包名版本，安装时可能需要先卸载旧版；卸载前应自行备份存档。

## 3. ReUI 架构

ReUI 采用运行时附加式表现层，尽量保留原有：

- 场景结构
- Prefab
- Button.onClick 事件
- Zenject 注入
- 游戏玩法脚本
- 数据和存档逻辑

核心目录：

`Assets\ReUI\Runtime`

编辑器验证目录：

`Assets\ReUI\Editor`

## 4. 当前文件说明

### `Assets/ReUI/Runtime/ReUIPalette.cs`

统一颜色系统，包括：

- 深色画布背景
- 玻璃面板颜色
- 描边颜色
- 主次文字色
- 青、蓝、紫、绿、金、红强调色
- 不同语义图标的强调色映射

### `Assets/ReUI/Runtime/ReUIIconGraphic.cs`

基于 UGUI `MaskableGraphic` 和 `VertexHelper` 的运行时矢量图标系统。

当前支持：

- 舰队
- 科技
- 舰船编辑器
- 装备
- 任务
- 设置
- 战斗
- 星图
- 商店
- 多人联机
- 图鉴
- 返回
- 关闭

目标是逐步替代原版单色或低信息量图标。

### `Assets/ReUI/Runtime/ReUIButtonMotion.cs`

为 UGUI `Selectable` 提供：

- 鼠标悬浮缩放
- 按下缩放
- 键盘或手柄选中缩放
- 使用 `Time.unscaledDeltaTime` 的平滑过渡
- 禁用时恢复原始缩放

### `Assets/ReUI/Runtime/ReUIAmbientGraphic.cs`

运行时绘制背景氛围层，包括：

- 上下渐变
- 青色柔光
- 紫色柔光
- 不拦截射线

### `Assets/ReUI/Runtime/ReUICanvasStyler.cs`

通用 UGUI 运行时样式器，负责扫描和处理：

- Canvas
- Image 面板
- Button
- Text
- Toggle
- Slider
- Scrollbar
- InputField

主要功能：

- 动态生成圆角九宫格 Sprite
- 应用玻璃面板颜色
- 添加柔和描边和阴影
- 设置按钮状态色
- 增加按钮动态效果
- 调整文字可读性
- 根据名称和本地化文本识别语义图标
- 跳过舰船贴图、头像、背景图等美术资源

2026-07-20 修复：

原实现会在本地化文本仍为数字索引时将按钮标记为已完成，导致后续无法识别“舰队”“科技”“设置”等语义并安装图标。现已将按钮基础样式标记与图标安装状态分离，允许本地化完成后的后续扫描补装或更新语义图标。

### `Assets/ReUI/Runtime/ReUIBootstrap.cs`

ReUI 自动启动入口：

- 使用 `RuntimeInitializeOnLoadMethod(BeforeSceneLoad)` 自动安装
- 使用 `DontDestroyOnLoad`
- 场景加载后多次延迟应用样式
- 定时扫描动态创建的 UI
- 通过 `PlayerPrefs` 键 `ReUI.Enabled` 控制启用状态
- 默认启用

### `Assets/ReUI/Runtime/ReUIMainMenuStyler.cs`

仅针对 `MainMenuScene` 的主菜单运行时视觉接管层。

当前功能：

- 不再向 `MainMenu` 根节点新增整块背景、描边或阴影
- 不再修改原 `VerticalLayoutGroup` 的边距、间距和按钮高度
- 删除旧版运行时生成的 `ReUI Menu Accent` 强调轨
- 保留原按钮 Transform、布局、文字节点、事件与 Zenject 引用
- 禁用 `MainMenuButton` 原有 `Left`、`Right` 装饰图形，但保留布局占位
- 将液态玻璃材质直接赋给原按钮的 `targetGraphic`
- 禁用 UGUI `Outline` 对玻璃网格的二次复制，边缘高光改由 Shader 生成
- 保留文字原锚点、对齐、Best Fit 和偏移，仅调整字重与颜色
- 强化 `ProgramTitle`，但不改变其位置和字号
- 使用一次性标记避免定时扫描重复处理

该文件不修改按钮事件、原场景 YAML 或 Zenject 注入。

### `Assets/ReUI/Runtime/ReUILiquidGlassMaterial.cs`

液态玻璃共享材质加载和运行时参数入口：

- 优先从 `Resources/ReUI/ReUILiquidGlass.mat` 加载，保证 Shader 被 Android 构建收录
- 资源缺失时使用 `Shader.Find("ReUI/LiquidGlassUI")` 回退
- 所有主菜单按钮共享一个材质，避免逐按钮复制 Material
- 统一设置背景模糊、折射、饱和度、描边高光、动态反光带与透明度
- 使用 `SubsystemRegistration` 重置静态引用，兼容关闭 Domain Reload 的编辑器设置

### `Assets/ReUI/Shaders/ReUILiquidGlass.shader`

基于内置渲染管线和 UGUI 的轻量液态玻璃 Shader：

- 使用命名 GrabPass `_ReUIGrabTexture`，同一帧首次使用时抓取一次背景
- 对抓取背景进行九点轻量模糊
- 在控件边缘施加轻微折射偏移
- 保留背景细节，只进行低强度冷色调制和轻微饱和度提升
- 在圆角边缘生成方向性高光
- 生成低强度移动反光带
- 支持 UGUI Stencil、RectMask/Mask 裁剪和 Alpha Clip
- 仅用于主菜单按钮，避免全界面 GrabPass 带来的性能损耗

该实现参考了 Unity GrabPass 文档、`zephyo/UI-Blur-LWRP-2020` 的背景采样结构以及 `mob-sakai/UIEffect` 的可裁剪 UGUI Effect 设计思路，但 Shader 和运行时代码为 ReUI 自行实现，没有直接复制外部包代码。

### `Assets/ReUI/Resources/ReUI/ReUILiquidGlass.mat`

序列化材质资源，直接引用 `ReUI/LiquidGlassUI` Shader，用于防止 Android 构建时 Shader 被剥离，并保存默认液态玻璃参数。

### `Assets/ReUI/Runtime/ReUISpecializedVisuals.cs`

专用场景样式器的公共工具，不保存玩法状态：

- 按明确层级查找运行时 UI
- 将液态玻璃材质直接赋给原按钮或原面板
- 禁用旧 `Background`、旧图标及重复 Outline/Shadow
- 统一按钮状态色和文字暗色阴影
- 保留原 Transform、Button.onClick、Zenject 和布局引用

### `Assets/ReUI/Runtime/ReUIStarMapStyler.cs`

仅处理 `StarMapScene` 的固定导航层：

- 明确处理底栏 `Fleet`、`Skills`、`Quests`、`Research`、`CargoHold`、`Exit`
- 将旧图标替换为 ReUI 彩色矢量图标
- 对运行时创建的 `Preview5RelationsButton` 和 `ThreeBodyCaptainButton` 使用同一视觉体系
- 将顶部资源栏改为较通透的共享玻璃材质
- 不修改星图节点、舰队数据或按钮事件

### `Assets/ReUI/Runtime/ReUIShipEditorStyler.cs`

仅处理 `ShipEditorScene`：

- 完整接管右侧六个固定快捷按钮：返回、舰船、撤销、预设、清空、退出
- 禁用每个按钮原有 `Background`、`Icon`、`Focus` 三层旧表现
- 为组件、舰船、卫星和预设面板应用共享玻璃材质
- 将组件列表、舰船列表、卫星列表和预设列表明确识别为内容项
- 内容项不再应用玻璃材质、额外 Outline、Shadow 或缩放动效
- 解决组件列表底部两行莫名青色发光的问题

### `Assets/ReUI/Runtime/ReUISkillTreeStyler.cs`

仅处理 `SkillTreeScene`，运行时隐藏中央 `ExitButton` 整个对象，删除截图中间的红色“❌”，同时避免留下不可见的点击区域。没有修改场景 YAML。

### `Assets/ReUI/Runtime/ReUIMultiplayerStyler.cs`

处理运行时创建的联机和联合进攻界面：

- 识别联机大厅、舰队选择和联合进攻对话框
- 统一主机、客机、准备、确认、关闭等按钮视觉
- 默认高亮第一支舰队，点击其他舰队后同步切换高亮
- 选中舰队使用高透明青色玻璃和亮边，未选中舰队降低亮度
- 将“联合进攻”前的空心符号改为实心 `■`
- 高亮状态仅保存在运行时静态集合中，不改变舰队数据和存档

### `Assets/ReUI/Editor/ReUIBuildSceneSanitizer.cs`

Android 构建时场景预处理器：

- 仅在 Unity 生成的临时构建场景副本中移除 Missing Script
- 不保存、不修改源 `.unity` 文件
- 规避 Unity 6000.0.75f1 在重新序列化包含 Missing Script 的场景时发生原生崩溃
- 已识别临时副本中的既有问题：`CommonGuiScene/Layout`、`SkillTreeScene/IconCache` 和 `SettingsScene` 中各 1 个 Missing Script

### `Assets/ReUI/Editor/ReUISceneProbe.cs`

编辑器只读场景探针，用于输出按钮层级、旧图标 Sprite、目标 Graphic、持久化事件和内容列表结构。该文件不写入场景。

### `Assets/ReUI/Editor/ReUIValidation.cs`

仅用于 Unity 编辑器批处理验证。

当前检查：

- `MainMenuScene` 是否可加载
- Canvas 数量
- `MainMenu` 根节点是否存在
- 直属按钮数量
- `ProgramTitle` 是否存在
- `VerstionInfo` 是否存在
- 场景是否存在 Missing Script
- 液态玻璃 Shader 是否可找到且受当前图形 API 支持
- `Resources/ReUI/ReUILiquidGlass.mat` 是否可加载并引用正确 Shader
- 模糊、折射和透明度参数是否正确序列化

### `Assets/ReUI/Editor/ReUIQuickAndroidBuild.cs`

ReUI 专用的可重复 Android 测试包构建入口，菜单位置为 `Build/ReUI/Quick Android APK`。

构建配置：

- 复用 `Assets/StreamingAssets/musicbundle`，避免每次重新构建音乐 AssetBundle
- IL2CPP
- ARM64 单架构
- OpenGLES3
- Android API 34
- Android Debug 签名
- 当前输出到 `Builds/Android/ThreeBody-EventHorizon-Beta3-ReUI3.apk`

命令行入口：

```text
-executeMethod ReUI.Editor.ReUIQuickAndroidBuild.Build
```

## 5. 已完成验证

Unity 版本：`6000.0.75f1`

已多次执行批处理编译，最终均返回：

```text
Batchmode quit successfully invoked - shutting down!
Exiting batchmode successfully now!
unity_exit_status=0
```

未发现：

- `error CSxxxx`
- `Compilation failed`
- `Scripts have compiler errors`
- `Assets/ReUI` 相关编译错误
- 原工程自身既有编译错误

主菜单结构验证结果：

```text
[ReUI Validation] scene=MainMenuScene, canvases=1, mainMenu=True, directButtons=9, programTitle=True, versionInfo=True, missingScripts=0
```

首次打开 `ReUI` 时已触发完整资源导入，并生成：

- `ReUI/Library`
- `ReUI/Logs`

Unity 正常退出后 `ReUI/Temp` 不存在。

## 6. 尚未完成的验证

以下项目尚未完成：

- 图形模式下主菜单实际截图
- Play Mode 交互验证
- 按钮事件逐项点击验证
- 不同语言下图标识别验证
- 16:9、18:9、20:9 等分辨率验证
- Android 安全区验证
- Android APK 实机安装与启动验证
- Android 实机性能测试

因此目前只能确认“代码编译和场景结构通过”，不能确认“最终视觉效果已经验收”。

## 7. 后续建议顺序

1. 图形模式打开 `MainMenuScene`，检查实际画面。
2. 根据截图调整主菜单容器尺寸、留白、文字和图标。
3. 验证 9 个按钮原有事件没有受到影响。
4. 将现有 APK 安装到 ARM64 Android 设备，验证启动、主菜单、按钮事件和存档兼容性。
5. 验证 Android 分辨率、安全区、性能和字体可读性。
6. 星图主导航与顶部资源栏。
7. 通用弹窗和面板。
8. 舰队与舰船管理界面。
9. 科技树。
10. 舰船编辑器。
11. 设置界面。
12. 战斗 HUD。
13. 重点功能图标替换。

## 8. 后续阶段记录格式

每次完成修改后，在本文件末尾追加：

```markdown
### YYYY-MM-DD 阶段名称

**修改文件**

- `相对路径`

**修改内容**

- 具体功能和视觉变化

**验证**

- Unity 编译结果
- 场景或测试结果
- APK 构建结果（如有）

**已知问题**

- 尚未完成或可能存在的风险

**下一步**

- 下一阶段工作
```

## 9. 阶段记录

### 2026-07-20 第一阶段：运行时主题系统和编译验证

**修改文件**

- `Assets/ReUI/Runtime/ReUIPalette.cs`
- `Assets/ReUI/Runtime/ReUIIconGraphic.cs`
- `Assets/ReUI/Runtime/ReUIButtonMotion.cs`
- `Assets/ReUI/Runtime/ReUIAmbientGraphic.cs`
- `Assets/ReUI/Runtime/ReUICanvasStyler.cs`
- `Assets/ReUI/Runtime/ReUIBootstrap.cs`

**修改内容**

- 建立 ReUI 运行时主题、调色板、矢量语义图标、按钮动效、背景氛围层和通用 Canvas 样式器。
- 使用自动 Bootstrap，避免逐个修改原玩法脚本。

**验证**

- Unity 6000.0.75f1 批处理编译通过。
- 首次打开触发完整资源导入。

**已知问题**

- 尚未进行图形模式画面验证。
- 尚未构建 APK。

### 2026-07-20 第二阶段：主菜单第一版运行时重构

**修改文件**

- `Assets/ReUI/Runtime/ReUICanvasStyler.cs`
- `Assets/ReUI/Runtime/ReUIMainMenuStyler.cs`
- `Assets/ReUI/Editor/ReUIValidation.cs`

**修改内容**

- 修复本地化完成前按钮被永久标记、导致语义图标无法补装的问题。
- 为主菜单增加玻璃容器、强调轨、按钮层次、标题与面板样式。
- 增加主菜单编辑器批处理验证工具。

**验证**

- Unity 编译通过，退出码为 0。
- `MainMenuScene` 加载成功。
- 找到 1 个 Canvas、9 个直属按钮、标题和版本信息。
- Missing Script 数量为 0。

**已知问题**

- 未完成主菜单实际截图和 Play Mode 点击验证。

### 2026-07-20 第三阶段：Android ARM64 测试 APK

**修改文件**

- `Assets/ReUI/Editor/ReUIQuickAndroidBuild.cs`
- `ProjectSettings/ProjectSettings.asset`
- `REUI_HANDOFF.md`

**构建过程产生或更新的文件**

- `Builds/Android/ThreeBody-EventHorizon-Beta3-ReUI.apk`
- `Assets/StreamingAssets/StreamingAssets`
- `Assets/Resources/Textures/ThreeBody/trisolaris_antigravity_core.png.meta`
- `Assets/Resources/Textures/ThreeBody/trisolaris_super_engine.png.meta`
- `ProjectSettings/AndroidResolverDependencies.xml`
- `ProjectSettings/UnityConnectSettings.asset`
- `Logs/ReUI-Android-Build.log`
- `Logs/ReUI-Quick-Android-Build.log`

其中两个 ThreeBody `.meta` 文件来自首次尝试调用既有 `AndroidDevelopmentBuild.Build` 时的贴图导入规格归一化；没有修改对应 PNG 像素内容。`Library`、`Logs` 和 Gradle 中间目录均属于可再生成文件。

**修改内容**

- 新增 ReUI 专用快速 Android 构建入口。
- 使用 IL2CPP + ARM64，避免生成无必要的 x86_64 库。
- 复用已存在的音乐 AssetBundle。
- 将目标 Android API 从工程原配置的 33 调整为本机已安装的 API 34。
- versionName 调整为 `Beta3-ReUI`，versionCode 调整为 `120001`。
- 保持包名 `com.threebody.EventHorizon` 和应用名 `三体视界`。

**故障与修复**

1. 首次完整构建超过单次工具调用上限，停止在资源/IL2CPP 构建阶段，未生成 APK。
2. Mono + ARM64 尝试失败，Unity 报 `Target architecture not specified`；原因是 Android Mono 后端不能用于该 ARM64 配置，随后改回 IL2CPP。
3. Gradle 首次失败，原因是工程要求 Android Platform 33，但 Unity 内置 SDK仅有 34、35、36，且系统 SDK目录为只读；通过在 ReUI 构建脚本中显式指定 API 34解决，未修改系统 SDK。

**验证**

- Unity 构建结果：`Succeeded`
- Unity 报告构建耗时：`00:02:38.8907350`
- APK 文件大小：`118,926,260` 字节
- `apksigner verify`：v1、v2 签名验证通过
- `aapt2 dump badging`：包名、版本、API 34和 `arm64-v8a` 均符合预期
- SHA-256：`ebd0eb28aa923817c23110581f8219f3ab897624a2338f84a97871a182926a63`

**已知问题**

- 尚未在 Android 真机安装和启动。
- 使用 Android Debug 证书，不适用于正式发布。
- 仅包含 ARM64，不支持 32 位 ARMv7 或 x86 设备。
- 包名与 Beta3 相同；若旧版签名不同，Android 会拒绝覆盖安装。

**下一步**

- 在 ARM64 真机安装 APK。
- 检查启动、主菜单实际画面、九个按钮事件、分辨率、安全区、字体和性能。
- 根据真机截图继续调整主菜单，而不是立即批量修改其他场景。

### 2026-07-20 第四阶段：移除叠加式 UI 并引入液态玻璃 Shader

**修改文件**

- `Assets/ReUI/Runtime/ReUIMainMenuStyler.cs`
- `Assets/ReUI/Runtime/ReUICanvasStyler.cs`
- `Assets/ReUI/Runtime/ReUIPalette.cs`
- `Assets/ReUI/Runtime/ReUILiquidGlassMaterial.cs`
- `Assets/ReUI/Shaders/ReUILiquidGlass.shader`
- `Assets/ReUI/Resources/ReUI/ReUILiquidGlass.mat`
- `Assets/ReUI/Editor/ReUIValidation.cs`
- `Assets/ReUI/Editor/ReUIQuickAndroidBuild.cs`
- `REUI_HANDOFF.md`

**修改内容**

- 根据第一版真机截图确认并修复“原 UI + ReUI 外框 + ReUI 按钮”多层叠加问题。
- 删除主菜单根节点新增的整块 Image、Outline、Shadow，不再创建外层玻璃容器。
- 不再修改原主菜单布局参数、按钮高度和文本锚点。
- 禁用原按钮的左右装饰图形，直接接管原 `targetGraphic`，保留原事件与布局对象。
- 主菜单不再创建全屏 `ReUI Ambient Layer`，避免背景整体被蓝灰蒙版压暗。
- 新增命名 GrabPass 液态玻璃 Shader，对真实背景执行九点轻模糊、边缘折射、方向高光和低强度动态反光。
- 所有主菜单按钮共享同一材质；GrabPass 同一帧仅抓取一次。
- 保留透明、清晰的背景细节，避免使用高不透明度蓝灰填充模拟玻璃。
- 测试包版本更新为 `Beta3-ReUI2` / `120002`，保留上一版 APK 便于对比。

**外部方案调研**

- Unity 官方 GrabPass 文档：确认该功能仅适用于内置渲染管线，命名纹理可减少一帧内重复抓屏，但仍应限制使用范围。
- `zephyo/UI-Blur-LWRP-2020`：参考其“背景采样、可裁剪、可调模糊”的 UI Blur 结构，不引入其包。
- `mob-sakai/UIEffect`：参考其 UGUI Shader、Stencil/Mask 和共享效果设计，不引入第三方依赖。
- ReUI Shader 与 C# 代码为自行实现；无需新增第三方运行时许可证文件。

**验证**

- Unity C# 编译通过，无 `error CS`。
- ShaderImporter 编译 `ReUILiquidGlass.shader` 时无 Shader error。
- 液态玻璃验证：`shader=ReUI/LiquidGlassUI`、`supported=True`、材质可加载。
- 参数验证：`blur=1.2`、`refraction=1.35`、`opacity=0.9`。
- 主菜单结构：1 个 Canvas、9 个直属按钮、标题和版本信息存在、Missing Script 为 0。
- Android 构建结果：`Succeeded`。
- 增量构建耗时：`00:01:29.1008621`。
- APK：`Builds/Android/ThreeBody-EventHorizon-Beta3-ReUI2.apk`。
- APK 大小：`118,937,068` 字节。
- `aapt`：`versionName=Beta3-ReUI2`、`versionCode=120002`、API 34、`arm64-v8a`。
- `apksigner`：v1、v2 验证通过，Android Debug 证书。
- SHA-256：`b938c8e485e788f4d6089dd0ff5aca829f85f729fb1c72fcd6387264ea7d9cc6`。

**已知问题**

- 尚未在真机确认 Screen Space Overlay Canvas 下的 GrabPass 最终显示效果。
- GrabPass 会增加 GPU 和带宽开销；当前仅用于主菜单按钮，必须通过真机帧率继续评估。
- 当前仍是 Debug 签名、ARM64 单架构测试包。

**下一步**

- 安装 `Beta3-ReUI2` 并提供主菜单截图。
- 检查是否彻底消除重复外框、旧左右装饰和全屏雾状蒙版。
- 检查折射和模糊是否过强、动态反光是否分散注意力。
- 根据真机性能决定保留 GrabPass，或改为低频 RenderTexture 背景缓存方案。

### 2026-07-20 第五阶段：ReUI3 多场景修复与真机反馈响应

**用户反馈来源**

本阶段针对用户提供的主菜单、星图、舰船编辑器、联机舰队选择和技能树真机截图处理以下问题：

1. 主菜单按钮文字与背景混合，难以辨认。
2. “开始游戏/继续游戏”仍显示旧 `mothership_icon`。
3. 星图底栏技能树、势力、舰长等入口没有统一为 ReUI 图标与玻璃底板。
4. 舰船编辑器右侧六个快捷按钮只改了一部分。
5. 舰船编辑器组件列表底部部分条目出现异常青色发光。
6. 玻璃染色、模糊和动态反光过强，整体呈塑料或雾状质感。
7. 联机舰队选择页缺少明确的当前选中高亮。
8. “联合进攻”前的状态符号应从空心改为实心。
9. 技能树中央红色“❌”需要删除。

**修改文件**

- `Assets/ReUI/Runtime/ReUICanvasStyler.cs`
- `Assets/ReUI/Runtime/ReUIMainMenuStyler.cs`
- `Assets/ReUI/Runtime/ReUILiquidGlassMaterial.cs`
- `Assets/ReUI/Runtime/ReUISpecializedVisuals.cs`
- `Assets/ReUI/Runtime/ReUIStarMapStyler.cs`
- `Assets/ReUI/Runtime/ReUIShipEditorStyler.cs`
- `Assets/ReUI/Runtime/ReUISkillTreeStyler.cs`
- `Assets/ReUI/Runtime/ReUIMultiplayerStyler.cs`
- `Assets/ReUI/Shaders/ReUILiquidGlass.shader`
- `Assets/ReUI/Resources/ReUI/ReUILiquidGlass.mat`
- `Assets/ReUI/Editor/ReUISceneProbe.cs`
- `Assets/ReUI/Editor/ReUIValidation.cs`
- `Assets/ReUI/Editor/ReUIBuildSceneSanitizer.cs`
- `Assets/ReUI/Editor/ReUIQuickAndroidBuild.cs`
- `REUI_HANDOFF.md`

**逐文件说明**

- `ReUICanvasStyler.cs`：取消所有场景的全屏氛围蒙版；增加专用场景样式器调用；识别并排除组件、舰船、卫星、任务和商店等内容列表项，避免对列表行错误应用玻璃、描边和缩放动效。
- `ReUIMainMenuStyler.cs`：每次动态扫描重新确保文字为纯白粗体并添加深色阴影；显式替换开始、继续、战斗、设置、编辑器、商店和退出图标；降低玻璃按钮透明度和悬浮增亮幅度。
- `ReUILiquidGlassMaterial.cs`：将模糊降至 `0.7`、折射降至 `0.65`、染色降至 `0.045`、动态反光降至 `0.055`、整体透明度降至 `0.7`。
- `ReUILiquidGlass.shader`：新增 `_BackgroundDim=0.14`，在保留背景细节的同时只做轻微压暗；降低边缘高光、移动反光、折射和色彩偏移，减轻塑料感。
- `ReUILiquidGlass.mat`：保存 ReUI3 的轻量玻璃参数并确保 Shader 被 Android 构建收录。
- `ReUISpecializedVisuals.cs`：提供专用场景控件接管工具，直接作用于原 `targetGraphic`，禁用旧 Background/Icon/Focus 表现但保留事件和布局。
- `ReUIStarMapStyler.cs`：明确处理星图底栏六个固定按钮以及运行时生成的势力、舰长入口；不再依赖名称模糊推断。
- `ReUIShipEditorStyler.cs`：完整处理右侧六个快捷按钮和五类右侧面板；对内容列表行恢复低亮度 Focus，禁用错误玻璃材质、Outline、Shadow 和缩放。
- `ReUISkillTreeStyler.cs`：在运行时隐藏整个中央 `ExitButton`，删除红色“❌”及其点击区域。
- `ReUIMultiplayerStyler.cs`：识别运行时联机界面，切换舰队时更新高亮；首项默认高亮；将“◇  联合进攻”改为“■  联合进攻”；不改变舰队数据或存档。
- `ReUISceneProbe.cs`：输出三个目标场景中的按钮路径、旧 Sprite、targetGraphic、事件方法和列表结构，为专用路径修改提供依据。
- `ReUIValidation.cs`：增加 ReUI3 场景目标和 Missing Script 检查。
- `ReUIBuildSceneSanitizer.cs`：仅从 Unity 临时构建副本中移除 Missing Script，源场景不保存、不修改。
- `ReUIQuickAndroidBuild.cs`：版本更新为 `Beta3-ReUI3` / `120003`，输出文件更新为 `ThreeBody-EventHorizon-Beta3-ReUI3.apk`。

**场景探针确认的目标**

- 主菜单：`Canvas/MainMenu/NewGame` 和 `Continue` 的旧图标均为 `Left/Image:mothership_icon`。
- 星图：`Canvas/GameMenu/Buttons/Fleet`、`Skills`、`Quests`、`Research`、`CargoHold`、`Exit`。
- 势力和舰长：运行时对象 `Preview5RelationsButton`、`ThreeBodyCaptainButton`。
- 舰船编辑器：`Canvas/ShipEditorWindow/Buttons` 下六个固定按钮。
- 舰船编辑器内容列表：`ComponentList`、`ShipList`、`SatelliteList`、`BuildList` 中的行项目。
- 技能树：中央对象为 128×128 的 `ExitButton`。

**构建故障与处理**

1. Unity 6000.0.75f1 在重新序列化构建场景时发生多次原生崩溃。
2. 构建日志确认原 ReUI 工程自身已有 Missing Script：
   - `CommonGuiScene/Layout`
   - `SkillTreeScene/IconCache`
   - `SettingsScene` 中 1 个未命名于日志摘要的对象
3. 新增 `ReUIBuildSceneSanitizer` 后，仅在临时构建场景中移除上述 3 个缺失组件，源场景文件保持不变。
4. 首次通过场景阶段后，Bee 的 `SetupCopyDataIl2cpp` 出现一次 `AccessViolationException`；该轮已生成绝大多数 IL2CPP 缓存。
5. 复用该缓存再次构建成功，没有删除 Library、没有执行 Git clean/reset，也没有修改原 Beta3 工程。

**验证**

- Unity C# 编译通过，无 `error CS`。
- Shader 编译通过，无 `Shader error`。
- 液态玻璃材质验证：`supported=True`。
- 参数验证：`blur=0.7`、`refraction=0.65`、`opacity=0.7`。
- `StarMapScene`：7 个目标对象存在，Missing Script 为 0。
- `ShipEditorScene`：8 个目标对象存在，Missing Script 为 0。
- Android 构建结果：`Succeeded`。
- Unity 报告构建耗时：`00:03:42.3461420`。
- APK：`Builds/Android/ThreeBody-EventHorizon-Beta3-ReUI3.apk`。
- APK 大小：`118,945,605` 字节。
- `aapt`：包名 `com.threebody.EventHorizon`，`versionName=Beta3-ReUI3`，`versionCode=120003`，minSdk 23，target/compileSdk 34，`arm64-v8a`。
- `apksigner`：v1、v2 签名验证通过，1 个 Android Debug 签名者。
- SHA-256：`35841a4c477d30c7ddf6f88d80e57e4baf51c072e5908fa45c116423ad328495`。

**已知问题**

- ReUI3 的实际视觉结果仍需 Android 真机截图确认。
- 势力和舰长入口暂时复用现有 ReUI 的星图、多人语义图标，以避免改变已序列化的 `ReUIIconKind` 枚举布局；后续可通过独立 Graphic 类增加专用图形。
- 技能树源场景仍含既有 `IconCache` Missing Script；当前只在构建副本中移除。
- `CommonGuiScene` 和 `SettingsScene` 也仍含既有 Missing Script；未修改源场景。
- GrabPass 仍有 GPU 带宽成本，应在真机检查帧率和发热。
- APK 仍为 Debug 签名、ARM64 单架构测试包。

**下一步**

- 安装 `Beta3-ReUI3`，分别截图主菜单、星图底栏、舰船编辑器、联机舰队选择和技能树。
- 确认主菜单文字、选中舰队高亮和组件列表异常发光是否完全修复。
- 根据真机效果继续微调玻璃透明度与边缘高光，不扩大到其他场景。

### 2026-07-20 第六阶段：ReUI4 首帧、舰长、联合进攻、科技树与 HUD 修复

**修改文件**

- `Assets/ReUI/Runtime/ReUIBootstrap.cs`
- `Assets/ReUI/Runtime/ReUICanvasStyler.cs`
- `Assets/ReUI/Runtime/ReUICaptainStyler.cs`
- `Assets/ReUI/Runtime/ReUITechTreeStyler.cs`
- `Assets/ReUI/Runtime/ReUISkillTreeStyler.cs`
- `Assets/ReUI/Runtime/ReUIStarMapStyler.cs`
- `Assets/ReUI/Runtime/ReUIMultiplayerStyler.cs`
- `Assets/ReUI/Runtime/ReUIHudStyler.cs`
- `Assets/ReUI/Editor/ReUISceneProbe.cs`
- `Assets/ReUI/Editor/ReUIQuickAndroidBuild.cs`
- `REUI_HANDOFF.md`

**修改内容**

- `ReUIBootstrap.cs`：场景加载回调中立即应用样式；动态 UI 扫描间隔由 1.25 秒降至 0.12 秒；用户点击/触摸结束后的同一帧 `LateUpdate` 再次应用样式，减少先显示原 UI 再替换的闪烁。
- `ReUICanvasStyler.cs`：技能树场景不再执行通用全量样式；舰长面板、研究/科技树、科技节点和势力选择列表列入保护范围，避免通用图标、文字和按钮样式破坏玩法状态。
- `ReUICaptainStyler.cs`：新增舰长专用样式器；修复舰长描述中的“战斗”被误判为战斗按钮并覆盖头像的问题；保留章北海、褚岩原头像；面板限制在屏幕左中区域，避开右侧星球面板和底部导航；已选舰长使用明确青色高亮。
- `ReUITechTreeStyler.cs`：科技树、科技节点和势力选择保持原生材质、锁定颜色、图标和 Toggle 逻辑；只清理旧 ReUI 自动生成图标和缩放动效，解决自由星系科技树错乱和三体科技树入口受干扰的问题。
- `ReUISkillTreeStyler.cs`：不再删除退出控件；保留原 `SkillTree.Exit` 事件，将同一按钮移动到右下角，尺寸 168×72，显示“退出”及 ReUI 关闭图标；技能节点、连线和三体技能页保持原生状态表现。
- `ReUIStarMapStyler.cs`：重做左下角 `StarViewButton`、`GalaxyViewButton`，统一采用轻量玻璃底板和星图语义图标，保留原 `ShowStarSystem` / `ShowStarMap` 事件。
- `ReUIMultiplayerStyler.cs`：联合进攻舰队按实际 `Toggle.isOn` 状态显示；支持再次点击取消；选中舰队使用青色实心高亮和描边；“联合进攻”状态显示为 `■`，关闭时显示 `□`；不再无条件强制实心。
- `ReUIHudStyler.cs`：战斗 HUD 生命/装甲、护盾、能量恢复绿、蓝、黄三色语义；通过反射清除原 `Gui.Controls.ProgressBar` 的平铺贴图，使生命、护盾和能量变为连续条；舰船切换列表的格子状状态条也改为连续条；舰船编辑器小型生命值和能量值分别使用绿色、黄色。
- `ReUISceneProbe.cs`：增加 HUD、Slider、ProgressBar、资源文字和目标 Prefab 的只读探针，用于确认实际路径和原始颜色。
- `ReUIQuickAndroidBuild.cs`：版本更新为 `Beta3-ReUI4` / `120004`，输出 `ThreeBody-EventHorizon-Beta3-ReUI4.apk`。

**关键定位结果**

- 舰长头像异常根因：舰长卡片为 Button，通用语义检测从描述文字“战斗”识别为 `Battle`，在 Portrait 上生成红色交叉图标。
- 联合进攻实际选择控件：`Preview7AlliedAttackDialog/Card/StarshipEarth` 为 Toggle，不是 Button。
- 星图左下角切换对象：`StarViewButton`、`GalaxyViewButton`。
- 技能树退出按钮：原 `ExitButton` 已绑定 `SkillTree.Exit`，本轮只运行时重定位，不重绑事件。
- 战斗资源条：`ArmorPoints`、`ShieldPoints`、`EnergyPoints` 均为 `Gui.Controls.ProgressBar`，原格子感来自私有 `_image` 的 UV 平铺。
- 原资源颜色：Armor/HitPoints 绿色，ShieldPoints 蓝色，EnergyPoints 黄色。
- 舰船切换格子条：`ShipToggleButton/Content/Slider/Fill` 使用 `tile` Sprite。

**验证**

- Unity 6000.0.75f1 C# 编译通过，无 `error CS`。
- 液态玻璃 Shader 验证通过，`supported=True`。
- Android 构建结果：`Succeeded`。
- 最终成功构建耗时：`00:02:11.0588924`。
- APK：`Builds/Android/ThreeBody-EventHorizon-Beta3-ReUI4.apk`。
- APK 大小：`118,954,037` 字节。
- `aapt`：`versionName=Beta3-ReUI4`、`versionCode=120004`、minSdk 23、target/compileSdk 34、`arm64-v8a`。
- `apksigner`：v1、v2 验证通过，Android Debug 签名，1 个签名者。
- SHA-256：`c2a013c67e229ecbe190ea48f0a1efccb87d0de1363131a47fb8be38ef4472fc`。

**构建过程**

- 前三次工具调用分别在 IL2CPP 生成、原生 C++ 编译和 Gradle 阶段超过 5 分钟调用上限；日志中没有 C#、Shader、IL2CPP 或 Gradle 错误。
- 清理被工具超时遗留的空闲 Gradle daemon 后，复用完整 IL2CPP 缓存增量构建成功。
- 未执行 Git clean/reset，未清理 Library，未修改原 Beta3 工程或源场景 YAML。

**已知问题与下一步**

- ReUI4 的功能与视觉结果仍需 Android 真机验证。
- 重点检查舰长头像、舰长面板边界、联合进攻开关状态、科技树/三体科技树、技能树右下角退出按钮，以及连续资源条。
- 动态扫描间隔已缩短到 0.12 秒，真机需观察 CPU 消耗；若性能受影响，应改用层级变动事件驱动，而不是继续缩短间隔。
- APK 仍为 Debug 签名、ARM64 单架构测试包。

### 2026-07-20 第七阶段：ReUI5 科技树恢复与战斗 HUD 修复

**用户真机反馈**

- 星图底栏科技入口图标在 ReUI4 中消失。
- 三体科技树仍无法点击打开。
- 战斗舰船头像旁的生命、护盾、能量数字仍显示为统一白色。
- 战斗暂停菜单的返回、切换舰船、下个敌人、设置和逃跑仍使用旧图标。

**修改文件**

- `Assets/ReUI/Runtime/ReUITechTreeStyler.cs`
- `Assets/ReUI/Runtime/ReUICanvasStyler.cs`
- `Assets/ReUI/Runtime/ReUIHudStyler.cs`
- `Assets/ReUI/Runtime/ReUICombatStyler.cs`
- `Assets/ReUI/Editor/ReUISceneProbe.cs`
- `Assets/ReUI/Editor/ReUIQuickAndroidBuild.cs`
- `REUI_HANDOFF.md`

**修改内容**

- `ReUITechTreeStyler.cs`：重写科技树恢复逻辑。旧逻辑把名称中含有 `Research` 的任何对象都视为科技树内容，误伤了星图底栏 `Research` 按钮。新逻辑只识别真实的 `ResearchPanel`、`TechTree`、`TechItemViewModel`、`FactionViewModel` 和相关 ViewModel 类型。
- `ReUITechTreeStyler.cs`：在真实科技节点与势力项中移除旧 ReUI 自动生成的矢量图标、动效和标记，重新启用原生 `Icon/Image` 并恢复无材质状态；若节点曾被 ReUI 干扰，则调用原 `TechItemViewModel.Initialize` 或 `FactionViewModel.SetFaction` 恢复原图标、颜色、名称和锁定语义。
- `ReUITechTreeStyler.cs`：通过反射读取势力 ID。三体势力 ID 22 的科技数据库已随工程提供，旧 Beta3 存档可能缺少发现标记，因此仅在 UI 层将其势力 Toggle 设为可交互；原 Toggle 事件和 `TechTree.Initialize(faction)` 链路保持不变。
- `ReUICanvasStyler.cs`：保护令牌从过宽的 `research` 收紧为 `researchpanel`；增加对科技节点、科技树、势力项和研究面板 ViewModel 类型的组件级保护；资源文本保护增加 `ResourceValue0/1/2` 及抗性数字名称，避免通用样式再次刷白；复用已存在的 ReUI 语义图标时强制重新启用对象，确保被 ReUI4 隐藏的星图科技入口图标能够恢复。
- `ReUIHudStyler.cs`：按运行时真实对象名直接处理舰船头像旁三行数字：`ResourceValue0` 绿色、`ResourceValue1` 蓝色、`ResourceValue2` 黄色；增加深色阴影和粗体，确保亮背景上仍可读。连续生命、护盾、能量条逻辑继续保留。
- `ReUICombatStyler.cs`：新增战斗场景专用样式器。明确接管 `Resume`、`ChangeShip`、`NextEnemy`、`Settings`、`KeySettings`、`KillThemAll`、`Surrender` 等按钮，禁用旧 `Left/Image` 图标，替换为 ReUI 返回、舰队、战斗、设置和关闭图标；保留所有原 `Button.onClick` 事件和布局。
- `ReUISceneProbe.cs`：增加科技 Prefab、战斗按钮和所有战斗 Text 的只读探针，确认原生图标路径、Toggle 事件、暂停菜单对象路径及运行时资源文字命名。
- `ReUIQuickAndroidBuild.cs`：版本更新为 `Beta3-ReUI5` / `120005`，输出 `ThreeBody-EventHorizon-Beta3-ReUI5.apk`。

**关键定位结果**

- 科技节点原图标：`ResearchPanel/Content/TechTree/Content/ScrollRect/Content/Item/Left/Icon`。
- 势力原图标：`ResearchPanel/Factions/Content/Factions/ScrollRect/Content/Faction/Icon/Image`。
- 三体势力：数据库 ID `22`，名称 `$Faction22`，科技与组件数据均存在。
- 战斗资源数字由 `Gui.Combat.ShipStatsPanel` 在运行时创建，名称固定为 `ResourceValue0`、`ResourceValue1`、`ResourceValue2`。
- 战斗暂停菜单路径：`Canvas/CombatMenu/Panel/Panel/*`；旧图标分别为 `play_icon`、`icon_ship`、`icon_drone`、`icon_repair`、`retreat_icon`。

**验证与构建**

- Unity 6000.0.75f1 C# 编译通过，无 `error CS`。
- 液态玻璃 Shader 验证通过，`supported=True`。
- Android 构建结果：`Succeeded`。
- 最终边界修复后的增量构建耗时：`00:02:33.4729927`。
- APK：`Builds/Android/ThreeBody-EventHorizon-Beta3-ReUI5.apk`。
- APK 大小：`118,956,783` 字节。
- `aapt`：包名 `com.threebody.EventHorizon`，`versionName=Beta3-ReUI5`，`versionCode=120005`，target/compileSdk 34，`arm64-v8a`。
- `apksigner`：v1、v2 签名验证通过，Android Debug 签名，1 个签名者。
- SHA-256：`91d2e199c18cfbd550b53abc16a6b6646bb86e5d1fc1ddba49ad8bb3e717eff6`。

**构建过程说明**

- 多次 DevSpace 调用在 IL2CPP 原生 C++ 编译阶段达到单次工具时限；日志中没有 C#、Shader、IL2CPP 或 Gradle 错误。
- 未清理 `Library`，持续复用已完成的目标文件，最终由后台批处理构建完成原生链接与 Gradle 打包。
- 未修改源场景 YAML，未执行 Git reset/clean，未修改原 Beta3 工程。

**真机验证重点**

- 星图底栏科技图标是否重新显示。
- 三体势力科技页是否可点击并正确生成节点。
- 自由势力及其他科技页的原生节点图标、锁定颜色和科技名称是否恢复。
- 战斗双方舰船头像旁三行数字是否按绿、蓝、黄显示。
- 战斗暂停菜单旧图标是否全部被替换，且返回、切换舰船、下个敌人、设置和逃跑功能仍正常。

### 2026-07-20 第八阶段：ReUI5 科技树数据、导弹改装与二向箔修复

**用户反馈与目标**

- “战斗”按钮可见度过低。
- 智子发射器贴图方向错误，需要顺时针旋转 90°。
- 三体和星舰地球科技树无法打开，需要追查数据与运行时逻辑根因。
- 所有导弹类武器应具备相应武器改装。
- 改装窗口关闭按钮需移动到右上角，避免遮挡。
- 二向箔马赛克只应作用于背景，不应覆盖舰船。
- 部分普通导弹错误使用二向箔的超大弹体贴图。
- “移除全部装置”按钮改用单斜杠禁止图标，与退出按钮的双斜杠关闭图标区分。

**科技树根因与修复**

- 星舰地球五个舰船科技 `StarshipEarthFrigate/Destroyer/Cruiser/Battleship/Flagship.json` 缺少 `Faction: 21`，导致节点未进入星舰地球科技集合，但其他科技仍依赖这些缺失节点；已补齐势力字段。
- 数据库存在两个科技 ID 冲突：`TrisolarisTitan.json` 与 `PhotonProjectileLauncher.json` 同为 406；`TrisolarisAntigravityCore.json` 与 `Xinghuan.json` 同为 412。已将三体泰坦调整为 415、反重力核心调整为 416，并同步更新其依赖科技。
- `TechTreePanelViewModel.SortTree` 原实现通过 `nodes[item]` 直接索引依赖；若依赖因势力筛选或坏数据不在当前树中，会抛出异常并阻止页面打开。已改为 `TryGetValue`，缺失依赖只记录警告并跳过，不再让整个科技树崩溃。
- `FactionViewModel` 增加异常保护，单个势力数据错误不会阻断其余科技页。
- `ReUITechTreeStyler` 取消通过反射重复调用 `Initialize/SetFaction` 的行为，避免 ReUI 扫描时重建科技树、重复监听或改变 Toggle 状态；现在只清理 ReUI 自身生成的视觉组件。

**导弹改装**

- `GameDatabase.DataModel.Component.OnDataDeserialized` 增加导弹改装兜底：名称包含 Missile/Rocket/Torpedo/导弹/鱼雷，或弹药控制器为 Homing 的武器，在 JSON 未配置 `PossibleModifications` 时自动获得标准导弹改装池。
- 标准池为原版导弹发射器使用的 10 项：重量、能耗、伤害、射程、弹速、冷却、二级伤害、二级弹速、弹体重量和范围效果。
- 数据库验证确认 4 个此前无显式改装的自定义导弹已获得改装：星舰地球制式导弹、星舰地球核导弹、三体反物质导弹、三体 EMP 导弹。

**弹体资源与二向箔**

- `DualVectorFoil.json` 的 BulletPrefab ID 从与 `Rocket4.json` 冲突的 21 调整为唯一 ID 23；二向箔弹药同步引用 23。
- 该冲突是普通幽灵导弹偶尔错误加载二向箔超大贴图的直接原因；修复后 Rocket4 保持 21、二向箔独占 23。
- 二向箔弹体视觉尺寸由 1.0 调整为 0.55，弹药 Body Size 由 2.0 调整为 0.85。
- `DualVectorFoilBackgroundMosaic.shader` 改为在背景 MeshRenderer 之后、舰船 SpriteRenderer 之前执行 GrabPass；马赛克绘制层级调整到背景层，避免覆盖舰船。
- `StrategicFieldEffect` 的马赛克排序层级由 24 降至背景层级，同时保留二向箔边界线在舰船之上。

**智子发射器贴图**

- `ResourceLocator` 增加运行时顺时针 90°旋转 Sprite 缓存，只对 `sophon_launcher` 舰船和舰船图标应用。
- 不修改原 PNG；编辑器、战斗、星图、舰队列表及其他通过 ResourceLocator 加载的界面自动使用旋转后的 Sprite。
- 玩家自定义贴图仍保持用户导入方向，不重复旋转。

**UI 修复**

- 主菜单“战斗”按钮改为更高亮度玻璃、纯白粗体文字、较强深色阴影和更醒目的战斗图标。
- 新增 `ReUIProhibitGraphic.cs`：单斜杠禁止图标。
- 舰船编辑器 `ClearButton` 使用单斜杠禁止图标；`ExitButton` 继续使用双斜杠关闭图标。
- 新增 `ReUIShipServiceStyler.cs`，将舰船改装窗口关闭按钮移动到右上角并保留原点击事件。

**修改文件**

- `Assets/Modules/Database/Resources/Database/Technology/StarshipEarthFrigate.json`
- `Assets/Modules/Database/Resources/Database/Technology/StarshipEarthDestroyer.json`
- `Assets/Modules/Database/Resources/Database/Technology/StarshipEarthCruiser.json`
- `Assets/Modules/Database/Resources/Database/Technology/StarshipEarthBattleship.json`
- `Assets/Modules/Database/Resources/Database/Technology/StarshipEarthFlagship.json`
- `Assets/Modules/Database/Resources/Database/Technology/TrisolarisTitan.json`
- `Assets/Modules/Database/Resources/Database/Technology/LightSpeedPositronBeam.json`
- `Assets/Modules/Database/Resources/Database/Technology/TrisolarisAntigravityCore.json`
- `Assets/Scripts/Legacy/GUI/ViewModel/StarMap/TechTreePanelViewModel.cs`
- `Assets/Scripts/Legacy/GUI/ViewModel/StarMap/FactionViewModel.cs`
- `Assets/Modules/Database/Scripts/DataModel/Component.cs`
- `Assets/Modules/Database/Resources/Database/Ammunition/Bullets/DualVectorFoil.json`
- `Assets/Modules/Database/Resources/Database/Ammunition/DualVectorFoilLauncher.json`
- `Assets/Resources/DualVectorFoilBackgroundMosaic.shader`
- `Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/StrategicFieldEffect.cs`
- `Assets/Modules/ResourceLocator/Scripts/ResourceLocator.cs`
- `Assets/ReUI/Runtime/ReUIMainMenuStyler.cs`
- `Assets/ReUI/Runtime/ReUITechTreeStyler.cs`
- `Assets/ReUI/Runtime/ReUIShipEditorStyler.cs`
- `Assets/ReUI/Runtime/ReUIProhibitGraphic.cs`
- `Assets/ReUI/Runtime/ReUIShipServiceStyler.cs`
- `Assets/ReUI/Runtime/ReUICanvasStyler.cs`
- `Assets/ReUI/Editor/ReUIValidation.cs`
- `REUI_HANDOFF.md`

**验证与最终构建**

- Unity C# 编译通过，无 `error CS`。
- 数据库级验证：星舰地球科技 26 项、三体科技 18 项；科技 ID 无重复；4 个自定义导弹获得默认改装池；二向箔 BulletPrefab 为 23；智子发射器旋转缓存成功。
- Android 构建结果：`Succeeded`。
- 最终构建耗时：`00:01:39.8361544`。
- APK：`Builds/Android/ThreeBody-EventHorizon-Beta3-ReUI5.apk`。
- APK 大小：`118,963,061` 字节。
- `aapt`：包名 `com.threebody.EventHorizon`，`versionName=Beta3-ReUI5`，`versionCode=120005`，target/compileSdk 34，`arm64-v8a`。
- `apksigner`：v1、v2 签名验证通过，Android Debug 签名。
- SHA-256：`0ace78c725493b69fdda859cfb12d5b378d6e0865639547ffeb251880b27e5a4`。

**真机验证重点**

- 三体与星舰地球科技树是否均能打开并完整显示节点。
- 智子发射器在战斗、舰队和编辑器中是否均为顺时针 90°后的方向。
- 普通幽灵导弹是否不再错误显示二向箔弹体。
- 二向箔马赛克是否只覆盖背景而不遮挡舰船。
- 自定义导弹是否均出现武器改装选项。
- 改装窗口关闭按钮是否位于右上角且不遮挡内容。
- “移除全部装置”是否为单斜杠禁止图标，退出是否仍为双斜杠关闭图标。

### 2026-07-20 第九阶段：ReUI6 主菜单、竞技场、底栏图标与透明填充修复

**用户反馈**

- 主菜单部分按钮图标缺失。
- 快速战斗配置舰队按钮被错误添加齿轮图标。
- 侵袭者/竞技场确认页中间的战斗按钮不可见。
- 星图右侧商店按钮存在黄色实心填充。
- 星图底栏技能树图标与科技重复，势力图标与星图重复，舰长图标过小。
- 多类蓝色、紫色、黄色、红色按钮背景填充需要改为透明玻璃。
- 战斗结算窗口中的彩色奖励块和红色退出填充需要透明化。
- 中文名称“中子电锯”改为“中子切割器”。

**修改文件**

- `Assets/ReUI/Runtime/ReUIIconGraphic.cs`
- `Assets/ReUI/Runtime/ReUIPalette.cs`
- `Assets/ReUI/Runtime/ReUISpecializedVisuals.cs`
- `Assets/ReUI/Runtime/ReUIStarMapStyler.cs`
- `Assets/ReUI/Runtime/ReUIMainMenuStyler.cs`
- `Assets/ReUI/Runtime/ReUICanvasStyler.cs`
- `Assets/ReUI/Runtime/ReUIArenaStyler.cs`
- `Assets/ReUI/Runtime/ReUIMultiplayerStyler.cs`
- `Assets/ReUI/Runtime/ReUICombatStyler.cs`
- `Assets/ReUI/Editor/ReUIQuickAndroidBuild.cs`
- `Assets/ModulesShared/Localization/Resources/Localization/Chinese/Components.xml`
- `REUI_HANDOFF.md`

**实现内容**

- 主菜单按钮图标不再只依赖固定对象名；同时读取按钮名称与本地化文字，识别开始、继续、战斗、联机、图鉴、设置、编辑器、商店和退出。
- 主菜单与专用按钮的背景改为低透明度玻璃，不再使用橙色、蓝色或红色大面积填充；危险与商店状态仅通过描边、图标及文字强调。
- `ConfigureEnemyFleet`、`ConfigureAllyFleet` 加入语义图标黑名单，并清理此前误生成的齿轮图标。
- 新增 `ReUIArenaStyler.cs`，显式恢复 `ArenaFight/FightButton` 与 `CancelButton`，保留 `OkButtonClicked` 和关闭事件，重新生成战斗/关闭图标。
- `ReUIIconGraphic` 新增 `Skills`、`Faction`、`Captain` 三种独立矢量图标：技能树使用分支节点，势力使用三方关系网络，舰长使用人物军帽轮廓。
- 星图底栏 `Skills` 不再与 `Research` 共用科技图标；动态势力按钮使用 `Faction`，舰长按钮使用 `Captain` 并扩大至 124×124。
- `ReUISpecializedVisuals.StyleGlassButton` 统一限制背景 Alpha 不超过 0.28，并恢复细描边；退出和危险按钮不再实心红色。
- 联合进攻舰队选中/未选中底色均改为透明玻璃，状态继续由描边和文字亮度区分。
- 战斗结算窗口 `ExpItem`、`PlayerExpItem`、`RewardItem` 和 `Focus` 的蓝/紫/黄实心块改为透明玻璃。
- 中文本地化 `$CircularSaw` 从“中子电锯”改为“中子切割器”。

**验证与构建**

- Unity 6000.0.75f1 C# 编译通过，无 `error CS`。
- 液态玻璃 Shader 验证通过：`supported=True`、blur 0.7、refraction 0.65、opacity 0.7。
- 前两次构建调用在 IL2CPP 代码生成阶段达到 DevSpace 单次 5 分钟上限，日志中无编译或 Gradle 错误。
- 第三次复用缓存构建成功，耗时 `00:04:18.1198551`。
- APK：`Builds/Android/ThreeBody-EventHorizon-Beta3-ReUI6.apk`。
- APK 大小：`118,967,448` 字节。
- 包名：`com.threebody.EventHorizon`。
- versionName：`Beta3-ReUI6`。
- versionCode：`120006`。
- target/compileSdk：34。
- 原生架构：`arm64-v8a`。
- 签名：Android Debug，v1、v2 验证通过。
- SHA-256：`f6747c57cece1852bd5ada7e374c52ec82a21691d9fde2430d03c81f83446cfe`。

**真机验证重点**

- 主菜单开始/继续/战斗等按钮图标是否全部显示。
- 快速战斗两个配置舰队按钮是否已移除错误齿轮。
- 竞技场/侵袭者确认页中间的战斗按钮是否恢复且点击有效。
- 商店、退出、联合进攻与战斗结算奖励块是否均为透明玻璃。
- 技能树、科技、势力、星图、舰长图标是否清晰区分，舰长图标尺寸是否合适。

### 2026-07-20 第十阶段：ReUI7 精确路径修复与构建前审查

**用户要求与工作方式**

- 设置页右侧红色叉号需恢复为与其他设置分类一致的青色导航风格。
- 侵袭者/竞技场确认页中央战斗按钮在 ReUI6 中仍不可见。
- 星图底栏势力图标过大、舰长图标过小、CargoHold 图标仍像太阳。
- 星币右侧黄色购物车图标需要彻底删除。
- 市场页面购买/卖出页签、购买按钮和退出按钮等有色背景需完全透明。
- 本阶段严格先定位具体对象、Prefab、事件与视觉层，再分析根因和实施；构建 APK 前新增自动审查，不以“编译通过”代替视觉结构验证。

**精确定位与根因**

- 设置页错误红叉实际对象为 `Canvas/Settings/Buttons/Exit`。原按钮子节点 `Icon` 使用青色 `icon_exit`；通用语义系统根据对象名将其替换为 `ReUIIconKind.Close`，所以出现了与其他设置页签不一致的红色叉号。
- 竞技场战斗按钮实际路径为 `Canvas/Panels/ArenaFight/Buttons/FightButton`，原事件为 `ArenaFightDialog.OkButtonClicked`。Prefab 的可见 140×140 图形位于按钮根节点，但 `Button.targetGraphic` 指向嵌套子节点 `Image`；ReUI6 禁用了根图形并把新图标挂到旧子节点，导致按钮整体不可见。
- 星币右侧黄色购物车实际是 `Canvas/GameMenu/Filters/Shop` 的星图商店地点筛选 Toggle，不是商店窗口入口，因此可直接移除且不影响商店购买逻辑。
- 底栏“太阳”实际为 `GameMenu/Buttons/CargoHold`，ReUI6 的 `Equipment` 图标用齿轮绘制，在小尺寸下被识别为太阳。
- 市场页实际 Prefab 为 `Assets/Resources/Gui/StarMapScene/MarketDialog.prefab`。顶部购买/卖出是 `ItemsPanel/Buttons/Buy`、`Sell` 两个 Toggle；右侧购买、卖出、出售废品、退出按钮分别保留 `Left`、`Right`、`Image`、`Background`、`Focus` 等独立装饰层。通用样式只处理 `targetGraphic`，因此蓝色、黄色、红色填充仍然存在。

**新增文件**

- `Assets/ReUI/Runtime/ReUISettingsStyler.cs`
- `Assets/ReUI/Runtime/ReUIMarketStyler.cs`

**修改文件**

- `Assets/ReUI/Runtime/ReUICanvasStyler.cs`
- `Assets/ReUI/Runtime/ReUIIconGraphic.cs`
- `Assets/ReUI/Runtime/ReUIStarMapStyler.cs`
- `Assets/ReUI/Runtime/ReUIArenaStyler.cs`
- `Assets/ReUI/Editor/ReUISceneProbe.cs`
- `Assets/ReUI/Editor/ReUIValidation.cs`
- `Assets/ReUI/Editor/ReUIQuickAndroidBuild.cs`
- `REUI_HANDOFF.md`

**实现内容**

- `ReUISettingsStyler` 专门接管设置页 `General`、`Combat`、`Controls`、`Account`、`LoadSave`、`Database` 及 `Exit`：移除 ReUI 自动语义图标，恢复原生青色图标；退出按钮恢复 `icon_exit`，使用与其他分类一致的透明玻璃底和青色描边，不再显示红色叉号。
- `ReUIArenaStyler` 将 `FightButton.targetGraphic` 显式改为按钮根节点可见 Image；中央战斗按钮设为 156×156、位置 `(0,0)`，战斗图标 112×112；取消按钮单独置于右侧。保留原 `OkButtonClicked` 和关闭逻辑，不创建替代事件。
- `ReUIStarMapStyler` 直接禁用 `GameMenu/Filters/Shop`，同时移除黄色购物车和布局槽；势力/舰长按钮统一为 112×112，势力图标缩至 72×72，舰长图标放大至 102×102，并同步调整动态生成的 `ReUI Icon Host`。
- `ReUIIconGraphic.DrawEquipment` 从齿轮改为货舱/储物箱轮廓，使 CargoHold 与科技、设置、太阳图标明确区分。
- `ReUIMarketStyler` 对市场页所有分类 Toggle、购买/卖出页签及右侧操作按钮逐层处理：目标图形、选中层、Left、Right、Background、Focus 等装饰层全部 `Color.clear`；选中、购买和危险操作只由描边、图标及文字颜色表达，不再使用实心蓝/黄/红填充。
- `ReUICanvasStyler` 在通用扫描后调用设置页和市场页专用样式器，保证主题、本地化或动态窗口生成后仍由精确路径逻辑完成最终接管。
- `ReUIQuickAndroidBuild` 更新为 `Beta3-ReUI7` / `120007`，标准输出文件为 `ThreeBody-EventHorizon-Beta3-ReUI7.apk`。

**构建前自动审查**

新增 `ReUIValidation.ValidateReUI7Presentation()`，在构建前自动实例化/打开真实场景和 Prefab，并验证：

- 设置 Exit 不存在活动的 ReUI 红色关闭图标；原 `icon_exit` 已启用且为青色；背景 Alpha 不高于 0.08。
- `GameMenu/Filters/Shop` 已禁用。
- CargoHold 使用重新绘制的 `Equipment` 货舱图标。
- 势力按钮 112、图标 72；舰长按钮 112、图标 102。
- Arena `FightButton` 使用根 Image 作为 `targetGraphic`，图标可见，按钮居中且尺寸 156。
- Market Buy/Sell 页签的目标图形和选中层完全透明。
- Market BuyButton、ExitButton 的目标图形及各装饰层完全透明。

审查结果：

`[ReUI7 Validation] settingsExit=restored, arenaFight=visible, shopFilter=removed, cargoIcon=storage, shortcutSizes=verified, marketFills=transparent`

Unity 退出码为 0，无 `error CS`、`Shader error` 或脚本编译失败。

**Android 构建与验证**

- 多轮 Unity 构建调用在 IL2CPP 原生编译和 Gradle 阶段达到 DevSpace 单次 5 分钟上限；日志中未出现项目编译错误。
- 原生 ARM64 `libil2cpp.so` 于 ReUI7 工程生成完成，Gradle 工程已确认 `versionName=Beta3-ReUI7`、`versionCode=120007`。
- Unity 最终封装阶段遇到一次临时授权令牌异常及进程锁竞态；未清理 Library，也未修改工程资源。
- 使用 Unity 内置 `gradle-launcher-8.13.jar` 对已生成的 ReUI7 Gradle 工程执行 `assembleRelease`，结果为 `BUILD SUCCESSFUL in 1m 45s`，84 项任务中 30 项执行、54 项复用缓存。
- APK 已复制到标准输出：`Builds/Android/ThreeBody-EventHorizon-Beta3-ReUI7.apk`。
- APK 大小：`118,978,657` 字节。
- 包名：`com.threebody.EventHorizon`。
- versionName：`Beta3-ReUI7`。
- versionCode：`120007`。
- target/compileSdk：34。
- 原生架构：`arm64-v8a`。
- 应用名：`三体视界`。
- 签名：Android Debug；APK Signature Scheme v1、v2 验证通过；1 个签名者。
- SHA-256：`8e8bd9b7e635f92f996885ffd88f167a3ead607a80296b857ac30b29e5c68c5b`。

**真机验证重点**

- 设置页右侧退出图标是否与其他分类统一为青色，而非红叉。
- 侵袭者/竞技场确认页中央战斗按钮是否可见、可点击且不遮挡舰队内容。
- 顶部黄色购物车筛选是否完全消失。
- 势力和舰长图标尺寸是否达到预期，CargoHold 是否显示为储物箱而非太阳。
- 市场购买/卖出页签、购买和退出按钮是否不再出现任何蓝/黄/红实心背景。

### 2026-07-20 第十一阶段：ReUI8 闪退防护与 Shop 过滤器回退

**用户反馈**

- ReUI7 安装后出现游戏闪退。
- `Filters/Shop` 属于星图过滤器功能，不能删除或停用；只应隐藏独立的 `BuyButton`。

**分析结论**

- 当前未连接 Android 设备，无法取得真机 `adb logcat`，因此不能声称已获得唯一、确定的原生闪退堆栈。
- 代码检查确认 `GameMenu.ShopFilterToggle` 是正式序列化引用，并在 `GameMenu.Start → OnFiltersChanged()` 中读取以更新 `_starMap.ShowStores`。ReUI7 直接停用 `GameMenu/Filters/Shop` 属于不必要的功能状态改动，与“只隐藏 BuyButton”的需求不符。
- ReUI 动态样式扫描原先没有异常隔离：任何单个动态窗口结构与专用样式器预期不一致时，异常会从 `ApplyNow()` 传播并中断该轮扫描；高频重复异常可能导致日志洪泛、卡死或被系统结束进程。
- ReUI7 专项验证还暴露出战斗按钮验证取到了旧的禁用图标，而不是新建的 `ReUI Arena Icon`；同时按钮父级与 `LayoutElement` 可见状态需要显式恢复。

**修改文件**

- `Assets/ReUI/Runtime/ReUIStarMapStyler.cs`
- `Assets/ReUI/Runtime/ReUIBootstrap.cs`
- `Assets/ReUI/Runtime/ReUIArenaStyler.cs`
- `Assets/ReUI/Editor/ReUIValidation.cs`
- `Assets/ReUI/Editor/ReUIQuickAndroidBuild.cs`
- `REUI_HANDOFF.md`

**实现内容**

- 完全撤销对 `GameMenu/Filters/Shop` 的 `SetActive(false)`；不再修改该 Toggle 的激活状态、选中状态、事件或布局。
- 只隐藏独立的 premium-currency `BuyButton`。
- `ReUIBootstrap.ApplyNow()` 对每个 Canvas 单独捕获异常；ReUI 作为表现层发生错误时只记录一次异常并继续处理其余 Canvas，不允许样式异常中断游戏逻辑或动态扫描协程。
- `ReUIArenaStyler` 显式恢复 `Buttons` 父级 CanvasGroup 的 alpha、交互和射线状态；同步更新 Fight/Cancel 的 LayoutElement 尺寸，避免 HorizontalLayoutGroup 覆盖目标大小。
- 竞技场验证改为按名称读取按钮根节点下的 `ReUI Arena Icon`，不再误判旧的禁用图标。
- 构建版本更新为 `Beta3-ReUI8` / `120008`。

**构建前验证**

- `ValidateReUI8Presentation` 通过：
  - 设置退出图标恢复；
  - 竞技场战斗按钮及直接图标可见；
  - `Filters/Shop` 保持激活；
  - `BuyButton` 已隐藏；
  - CargoHold 使用装备箱图标；
  - 势力/舰长尺寸正确；
  - 市场有色填充透明化。
- 新增 `ValidateAllEnabledScenesSmoke`，逐场加载全部 11 个启用场景，对 14 个 Canvas 执行 `ReUICanvasStyler.Apply`。
- 烟雾测试覆盖：Loader、CommonGui、MainMenu、StarMap、SkillTree、ConfigureControls、Combat、Exploration、Ehopedia、ShipEditor、Settings。
- 全部场景执行完成，无 `NullReferenceException`、`InvalidOperationException`、C# 编译错误或 Shader 错误。

**Android 构建与验证**

- Android 构建结果：`Succeeded`。
- 构建耗时：`00:01:57.4509470`。
- APK：`Builds/Android/ThreeBody-EventHorizon-Beta3-ReUI8.apk`。
- APK 大小：`118,980,499` 字节。
- 包名：`com.threebody.EventHorizon`。
- versionName：`Beta3-ReUI8`。
- versionCode：`120008`。
- target/compileSdk：34。
- 原生架构：`arm64-v8a`。
- 签名：Android Debug；APK Signature Scheme v1、v2 验证通过。
- SHA-256：`7ac231475eb9c555b08a53ad5476f29e79a9c6ebc5f3adb9727dfb9991e41524`。

**真机验证重点**

- 进入主菜单和星图时是否仍会闪退。
- 星图 `Filters/Shop` 是否仍可见、可切换并正确控制商店地点显示。
- 星币旁独立黄色 `BuyButton` 是否已隐藏。
- 侵袭者/竞技场战斗按钮是否可见且可点击。
- 若 ReUI8 仍闪退，必须连接设备后导出 `adb logcat`，依据 `FATAL EXCEPTION`、`AndroidRuntime` 或 native tombstone 精确定位，不再继续推测。

### 2026-07-21 第十二阶段：战斗主按钮、图标一致性、设置选项与取消按钮修复

**用户反馈**

- 竞技场/侵袭者页面的战斗按钮仍然几乎不可见，需要重新绘制并确保位于中央。
- 星图底栏和设置右侧导航的图标大小不一致，部分按钮因 disabled 颜色或 CanvasGroup 继承而过暗。
- 动态生成的战斗地图大小选择器与其他设置 Toggle 视觉不一致。
- 部分确认弹窗的取消按钮只显示文字旁的小红叉，缺少与主操作一致的完整按钮表面。

**精确定位与分析**

- 战斗入口仍为 `Canvas/Panels/ArenaFight/Buttons/FightButton`，原事件 `ArenaFightDialog.OkButtonClicked` 保持不变。问题不在功能绑定，而在旧交叉刀刃图标线条过细、表面 Alpha 过低，且缺少文字标签。
- 星图底栏固定按钮位于 `GameMenu/Buttons`；动态势力与舰长按钮分别是 `Preview5RelationsButton` 和 `ThreeBodyCaptainButton`。此前每类按钮使用不同 iconSize，造成视觉重量不统一。
- 星舰基地右侧设施按钮来自 `FactionPanel.prefab`：`Store`、`Factory`、`Shipyard` 等。声望不足时 `Button.interactable=false`，原 ColorBlock 的 disabledColor 会把整组图标和文字压暗到近乎不可读。
- `CombatMapSize` 由 `SettingsCombat.CreateCombatMapSizeSelector()` 动态复制 `EnemyTransmissions` 行；其内部仍是原生 Toggle。旧专用样式把整个 checkmark 拉成大色块，没有复用其他设置 Toggle 的尺寸模板。
- CommonGui 的确认弹窗通过 `CloseWithResultOption1` / `CloseWithResultOption2` 区分主操作与取消操作。通用语义识别在第二选项文字旁生成 Close 图标，却没有恢复完整矩形按钮表面。

**新增文件**

- `Assets/ReUI/Runtime/ReUIFactionPanelStyler.cs`
  - 专门处理星舰基地设施按钮的亮度、透明玻璃底、图标尺寸及 disabled 状态。
  - 只改变表现，不改变 `interactable`、声望条件或点击事件。
- `Assets/ReUI/Runtime/ReUIDialogStyler.cs`
  - 按持久化事件识别确认弹窗的 Option1 / Option2。
  - 保留原窗口返回值绑定，移除错误的小关闭图标，恢复统一矩形玻璃按钮。

**修改文件**

- `Assets/ReUI/Runtime/ReUIIconGraphic.cs`
  - `DrawBattle` 重绘为高对比实心青色盾徽、深色内衬、粗白色交叉刃和金色中心环；未新增枚举值，不改变已有 MonoBehaviour 序列化布局。
- `Assets/ReUI/Runtime/ReUIArenaStyler.cs`
  - 战斗按钮扩大为 176×176，主图标 124×124；表面和描边显著提亮。
  - 新增直接子节点 `ReUI Arena Label`，显示“战斗”；取消按钮显示“取消”。
  - 保留 `OkButtonClicked` 与原关闭事件。
- `Assets/ReUI/Runtime/ReUIStarMapStyler.cs`
  - 六个固定底栏图标统一为 72×72。
  - 势力与舰长按钮统一为 112×112，图标统一为 72×72。
  - 地图视图按钮也统一使用 72×72 图标。
  - 强制恢复 CanvasGroup alpha，并将 disabledColor 提升为正常可读亮度。
- `Assets/ReUI/Runtime/ReUISettingsStyler.cs`
  - 设置右侧导航图标统一为 64×64并提高亮度。
  - 对设置内容区全部 Toggle 使用统一 80×80 背景和 34×34状态标记。
  - `CombatMapSize` 复用同一 Toggle 模板，不再出现单独的大青色方块。
- `Assets/ReUI/Runtime/ReUICanvasStyler.cs`
  - 注册 `ReUIFactionPanelStyler` 与 `ReUIDialogStyler`，确保专用规则在通用扫描后完成最终校正。
- `Assets/ReUI/Editor/ReUIValidation.cs`
  - 增加战斗徽记、战斗文字、底栏统一尺寸、设置导航亮度、地图大小 Toggle、势力设施 disabled 亮度和确认弹窗取消按钮的自动审查。
- `Assets/ReUI/Editor/ReUIQuickAndroidBuild.cs`
  - 保持最终版本为 `Beta3-ReUI8` / `120008`。
- `REUI_HANDOFF.md`
  - 追加本阶段定位、实现、验证和最终构建记录。

**构建前审查结果**

- `ValidateReUI8Presentation` 通过：
  - 战斗主徽记和“战斗”标签可见；
  - 设置导航和内容 Toggle 尺寸统一；
  - `Filters/Shop` 保持功能状态，只有独立 `BuyButton` 隐藏；
  - 星图底栏图标统一；
  - Store/Factory/Shipyard 即使不可交互也保持正常可读亮度；
  - 确认弹窗取消按钮为完整矩形按钮，不再带文字旁小红叉；
  - 市场按钮填充仍为透明。
- 全启用场景烟雾测试通过：11 个场景、14 个 Canvas，Unity 退出码 0。
- 未发现 C# 编译错误、Shader 错误或样式器托管异常。

**最终 Android 构建与验证**

- 构建结果：`Succeeded`。
- 独立构建耗时：`00:07:30.1585097`。
- APK：`Builds/Android/ThreeBody-EventHorizon-Beta3-ReUI8.apk`。
- APK 大小：`118,990,125` 字节。
- 包名：`com.threebody.EventHorizon`。
- versionName：`Beta3-ReUI8`。
- versionCode：`120008`。
- minSdk：23；target/compileSdk：34。
- 原生架构：`arm64-v8a`。
- 签名：Android Debug；APK Signature Scheme v1、v2 验证通过；1 个签名者。
- SHA-256：`c79e38e192e6858c92d4833ca110311f606c685c5efdcef03f6e0a33f54f0f8a`。

**真机验证重点**

- 竞技场/侵袭者中央战斗盾徽、文字和点击区域是否清晰可见。
- 底栏所有图标是否同尺寸，势力与舰长视觉重量是否一致。
- 声望不足的商店、工厂、船坞按钮是否仍清晰，但保持不可点击。
- 地图大小选择器是否与其他设置开关一致。
- 攻击星舰基地等确认弹窗的取消按钮是否为完整矩形按钮且原取消功能正常。

### 2026-07-21 第十四阶段：ReUI11 启用态统一高亮、禁用态动态压暗与战斗菜单层级修复

**真机反馈与根因**

- 战斗暂停菜单中“返回”处于选中态时清晰，但“下个敌人”“设置”等未选中按钮明显更暗。
- 这些按钮的 `targetGraphic` 是按钮下的直接子节点 `Image`，原层级位于 `Left`（图标）和 `Right`（文字）之后。液态玻璃底板因此最后绘制，覆盖在文字和图标上方，造成即使内容 Alpha=1 仍被洗白、模糊或压暗。
- 各 Button 的 normal/highlighted/selected ColorBlock 亮度仍不一致；此外“下个敌人”等按钮会在运行时动态切换 `interactable`，仅在场景加载时设置颜色无法稳定表达禁用态。

**修改文件**

- `Assets/ReUI/Runtime/ReUIButtonMotion.cs`
- `Assets/ReUI/Runtime/ReUISpecializedVisuals.cs`
- `Assets/ReUI/Runtime/ReUICanvasStyler.cs`
- `Assets/ReUI/Editor/ReUIValidation.cs`
- `Assets/ReUI/Editor/ReUIQuickAndroidBuild.cs`
- `Assets/Modules/AppConfiguration/Scripts/Generated/AppConfig.cs`
- `REUI_HANDOFF.md`

**关键改动**

- `StyleGlassButton()` 对直接位于按钮根下的玻璃 `targetGraphic` 设置全拉伸、`LayoutElement.ignoreLayout=true` 并 `SetAsFirstSibling()`；底板不再占据水平布局槽位，也不再覆盖后绘制的图标和文字。
- 所有非禁用按钮的 normal、highlighted、selected 使用同一个高亮颜色，亮度与此前选中态一致；pressed 只保留轻微颜色反馈。
- 禁用按钮的 targetGraphic 使用更暗的 disabledColor；文字和语义图标通过 `ReUIButtonMotion` 额外降至 0.42 CanvasRenderer Alpha，按钮辅助装饰降至 0.55，仍保持可辨认。
- `ReUIButtonMotion` 运行时监测 `Button.interactable`。当“下个敌人”等按钮动态启用或禁用时，会立即恢复为完整亮度或切换到额外压暗状态，不依赖重新打开场景或再次执行全局样式。
- 启用态文字和图标继续保持自身颜色 Alpha=1；禁用态只通过 CanvasRenderer 统一压暗，不污染原始颜色，恢复可用时不会累积变暗。
- 雷达彩色圆点、战利品透明背景、独立战斗徽记和发光回退等 ReUI10 行为保持不变。

**实际验证**

- Unity 6000.0.75f1 编译通过：0 个编译错误，128 条项目既有警告。
- `ValidateReUI11Presentation` 通过：`enabledButtons=selected-brightness`、`disabledButtons=extra-dimmed`、`combatMenuSurface=behind-content`、`dynamicInteractable=refreshed`。
- ReUI10 回归验证同时通过：按钮底板保留、内容不透明、无额外发光、战斗徽记、战利品透明、雷达颜色与圆形标记均未回归。
- 液态玻璃材质验证通过；数据验证通过；主菜单验证通过。
- 全部启用场景烟雾测试通过：11 个场景、14 个 Canvas。

**Android 构建**

- 构建结果：`Succeeded`；耗时：`00:03:49.0819581`。
- APK：`Builds/Android/ThreeBody-EventHorizon-Beta3-ReUI11.apk`。
- APK 大小：`118,995,624` 字节。
- 包名：`com.threebody.EventHorizon`。
- versionName：`Beta3-ReUI11`；versionCode：`120011`；buildNumber：`1878`。
- minSdk：23；target/compileSdk：34；架构：`arm64-v8a`。
- Android Debug 签名；v1、v2 验证通过；证书 SHA-256：`12db3813c08e0d85d97884f7bf87e337d91c9e376e470a41e086a1f682402a96`。
- APK SHA-256：`cc812d5c76503890fa6c191c578a55cc5d46cda279cbddcb9ef19ffc2c5f4bd0`。

**剩余风险**

- 当前未连接 Android 设备，尚未执行真机安装、点击和截图验证。重点复核战斗暂停菜单：所有可用按钮应与选中态同亮度；不可用的“下个敌人”等按钮应明显更暗但仍可辨认；状态动态变化后应立即恢复或压暗。

### 2026-07-21 第十三阶段：ReUI10 独立战斗徽记、按钮内容不透明、发光回退与雷达圆点保护

**真机反馈**

- 竞技场/侵袭者进入战斗时，中央战斗图标仍不可见。
- 星图底栏、快捷入口和右侧星系对象等大量按钮整体过暗。
- 战斗结算物品槽仍显示紫色、灰色、青色等不同颜色的实心背景。

**根因**

- 战斗入口虽然已拥有独立视觉节点，但仍使用通用 `ReUIIconGraphic.Battle`。该类型会参与通用语义图标清理与主题重扫；同时按钮表面使用液态玻璃材质且 Alpha 仅 0.12，真机深色星空取样后对比不足。
- 通用按钮和星图专用规则原本使用 0.035–0.12 的低透明玻璃底板；随后尝试的双层边框/文字发光在真机上反而降低了文字清晰度。最终规则改为：保留各页面原有按钮底板、边框、选中态和禁用态，只把按钮文字、原生图标和 ReUI 绘制图标的 Alpha 固定为 1，并移除上一轮新增的双层发光。
- `ReUICombatStyler.NormalizeCombatRewardFills()` 仍将 `ExpItem`、`PlayerExpItem`、`RewardItem` 和 `Focus` 设置为 0.08–0.18 Alpha 的玻璃色块，并未实现完全透明。
- `Preview5CombatMinimap` 和旧 `RadarPanel` 中的目标点、盟军点、导弹点及其他雷达标记携带用户设计的分类色，但通用 Image/Button/Text 扫描此前没有整棵层级保护，动态创建的点存在被换 Sprite、改色或套材质的风险；同时小地图点使用白色方形 Sprite，未满足圆形标记要求。

**新增文件**

- `Assets/ReUI/Runtime/ReUIFightIconGraphic.cs`
- `Assets/ReUI/Runtime/ReUIFightIconGraphic.cs.meta`

**修改文件**

- `Assets/ReUI/Runtime/ReUIArenaStyler.cs`
- `Assets/ReUI/Runtime/ReUICanvasStyler.cs`
- `Assets/ReUI/Runtime/ReUISpecializedVisuals.cs`
- `Assets/ReUI/Runtime/ReUIStarMapStyler.cs`
- `Assets/ReUI/Runtime/ReUICombatStyler.cs`
- `Assets/Scripts/Gui/Combat/CombatMinimap.cs`
- `Assets/ReUI/Editor/ReUIValidation.cs`
- `Assets/ReUI/Editor/ReUIQuickAndroidBuild.cs`
- `Assets/Modules/AppConfiguration/Scripts/Generated/AppConfig.cs`
- `REUI_HANDOFF.md`

**关键改动**

- 新增 `ReUIFightIconGraphic`，使用 Unity `VertexHelper` 在项目内直接绘制高对比战斗徽记：青色外环、深色内衬、实心盾牌、粗白/青交叉刃与金色中心。它不再使用通用 `ReUIIconGraphic`，不会被通用图标清理或主题重刷误伤。
- `ReUIArenaStyler` 保留原 `FightButton`、`OkButtonClicked` 和布局，只将视觉层替换为独立 `ReUI Fight Emblem`；战斗按钮继续使用低透明玻璃底板（普通 0.10、危险 0.07），徽记和“战斗”文字强制 Alpha=1，并移除上一轮新增的双层发光效果。
- `ReUISpecializedVisuals` 回退统一发光 API；清理按钮及文字上由上一轮创建的额外 Outline/Shadow，仅保留普通单层边框。按钮文字和图标显式启用，并把自身颜色 Alpha 与 CanvasRenderer Alpha 固定为 1。
- `ReUICanvasStyler` 在所有专用样式器执行后增加最终内容规范化：明确跳过 `targetGraphic`、Background、Focus、Surface、Left、Right 等按钮背景层，不改写它们的材质、颜色、ColorBlock 或 CanvasGroup；只对非背景文字、原生 Image 图标及 ReUI 矢量图标设置颜色 Alpha=1 和 CanvasRenderer Alpha=1，并禁用上一轮遗留的额外 Outline。
- 通用按钮恢复 0.035/0.10 的低透明玻璃底板；星图底栏恢复 0.060–0.065，势力设施恢复 0.060，星系对象恢复 0.10/0.12。禁用按钮保持 `interactable=false`，但其文字和图标仍为 100% 不透明；不修改窗口祖先 CanvasGroup。
- `ReUICanvasStyler` 将 `RadarPanel`、`Preview5CombatMinimap`、`CombatMinimap` 及 `Gui.Combat.RadarPanel/Radar/BeaconRadar/CombatMinimap` 组件列入玩法内容保护范围。其后代 Image、Button、Text 均跳过通用 ReUI 样式，保留用户制作的彩色点、Sprite、材质和透明度。
- `CombatMinimap.MarkerSprite` 改为运行时生成的抗锯齿圆形 Sprite；目标、导弹和瞬时标记继续沿用原有分类色，只改变点的几何形状，不统一改色。
- 战斗结算的 `ExpItem`、`PlayerExpItem`、`RewardItem` 和 `Focus` 背景全部设为 `Color.clear` 并移除液态玻璃材质。`Focus` 仅保留青色描边表达选中状态，不再使用任何有色填充。
- `ValidateReUI10Presentation()` 最终检查独立战斗徽记、按钮低透明底板被保留、文字与图标 Alpha=1、双层发光已移除、禁用态仍不可点击、战利品卡片完全透明、雷达分类色不变且标记 Sprite 为圆形，以及 Shop/BuyButton 等既有约束。

**实际验证**

- Unity 6000.0.75f1 C# 编译通过：0 个编译错误；128 条为项目既有过时 API 等警告。
- `ValidateReUI10Presentation` 通过：`buttonSurfaces=preserved`、`buttonTextIcons=opaque`、`glow=removed`、`arenaFight=dedicated-opaque-emblem`、`rewardCards=transparent`、`radarColors=preserved`、`radarMarkers=circular`、Shop 保留和 BuyButton 隐藏均通过。
- 液态玻璃材质验证通过：Shader supported=True，blur 0.7，refraction 0.65，opacity 0.7。
- 数据验证通过：星舰地球科技 26 项、三体科技 18 项、4 个导弹改装池、二向箔 BulletPrefab 23、智子发射器顺时针 90°。
- 主菜单验证通过：1 个 Canvas、9 个直接按钮、标题与版本信息存在、缺失脚本 0。
- 全部启用场景烟雾测试通过：11 个场景、14 个 Canvas；包括 StarMap、SkillTree、Combat、ShipEditor、Settings 等。
- 构建临时处理完成后，`CommonGuiScene`、`SkillTreeScene`、`SettingsScene` 的 Git blob 哈希均与当前分支 HEAD 一致，源场景未被改写。

**Android 构建**

- 构建结果：`Succeeded`。
- 最终成功构建耗时：`00:04:58.3341683`。
- APK：`Builds/Android/ThreeBody-EventHorizon-Beta3-ReUI10.apk`。
- APK 大小：`118,996,201` 字节。
- 包名：`com.threebody.EventHorizon`。
- versionName：`Beta3-ReUI10`。
- versionCode：`120010`。
- buildNumber：`1877`。
- minSdk：23；target/compileSdk：34。
- 原生架构：`arm64-v8a`。
- 签名：Android Debug；APK Signature Scheme v1、v2 验证通过；1 个签名者。
- 签名证书 SHA-256：`12db3813c08e0d85d97884f7bf87e337d91c9e376e470a41e086a1f682402a96`。
- APK SHA-256：`3c3a7dce7b389239e3269161d9c37d225405b1a2f1a7aab3e6a7a63ab55fac0e`。

**剩余风险与真机验证重点**

- 当前未连接 Android 设备，无法执行安装、实际点击和截图级验证；自动验证只能证明运行时对象结构、材质、Alpha、事件和场景加载符合预期。
- 真机重点检查中央战斗徽记与“战斗”文字是否在低透明玻璃按钮上清晰且点击仍触发原战斗事件；设置页、底栏、快捷入口和“太空要塞”等按钮是否保留原按钮底板，同时文字/图标保持 100% 不透明；战斗结算所有物品槽是否无任何有色背景；雷达和小地图上的目标、盟军、导弹等标记是否保持原配色并显示为圆形。

### 2026-07-21 第十三阶段：ReUI9 真实运行时战斗按钮、太空要塞禁用态与底栏宿主修复

**本阶段目标**

- 修复真机中竞技场/侵袭者页面中央战斗按钮仍不可见的问题，不再以“组件存在”作为通过条件。
- 修复右侧“太空要塞”等不可用对象按钮被 `disabledColor` 压暗到难以辨认的问题，同时保持 `interactable=false`。
- 修复星图底栏原 Image、旧 ReUI 图标和动态 Icon Host 混用导致的图标缺失、重复及尺寸不一致。
- 保留 `Filters/Shop`，只隐藏独立 `BuyButton`；复核地图大小、取消按钮、科技树、联合进攻和舰长相关入口。

**真实对象定位与根因**

- 竞技场真实对象仍是 `Canvas/Panels/ArenaFight/Buttons/FightButton`，原持久化事件为 `ArenaFightDialog.OkButtonClicked`；取消按钮原事件为 `Close`。
- `ArenaFight` 在场景/Prefab 中默认未激活。旧验证只对未激活模板执行一次样式扫描，没有模拟窗口打开、布局重算、主题生命周期和后续扫描，因此产生真机假阳性。
- 原 `FightButton.targetGraphic` 指向嵌套的 `ThemedImage`，按钮根节点、旧图标宿主和新 ReUI 图标之间存在多套视觉层；同时父级 `Buttons` 使用 `HorizontalLayoutGroup`，会在 ReUI 设置绝对位置后再次改写按钮布局。
- 截图中的“太空要塞”不是 `FactionPanel` 的 Store/Factory/Shipyard。真实对象是 `InformationPanel.prefab` 动态复用的 `Gui.StarMap.StarSystemObjectItem`，文本来自 `$ObjectStarBase`。其原 Button `disabledColor` 为纯黑，`targetGraphic` 指向低 Alpha 的 `Focus`，因此此前针对设施按钮的验证并未覆盖真机目标。
- 固定底栏按钮在场景尚未完成布局时可出现 `rect=0×0`；动态势力按钮有原生 `Icon`，动态舰长按钮只有文字。旧逻辑通过 `GetComponentInChildren<ReUIIconGraphic>(true)` 任取第一个图标，可能命中已禁用宿主中的残留图标，验证仍通过但实际不渲染。

**修改文件与依据**

- `Assets/ReUI/Runtime/ReUIArenaStyler.cs`
  - 保留原 Button 根节点和原 `onClick`，不创建替代战斗事件。
  - 在按钮根节点创建唯一、独立的 `ReUI Arena Surface`、`ReUI Arena Icon`、`ReUI Arena Label`。
  - Surface 使用普通 `Image`，不再受 `ThemedImage.Start()` 重刷；显式设置 `targetGraphic`、CanvasRenderer alpha、`cullTransparentMesh=false`、`maskable=false`。
  - 禁用旧 Image、旧文字和残留 ReUI 图标，只保留新的 Surface/Icon/Label 渲染栈。
  - 禁用竞技场按钮容器的旧 `LayoutGroup`，Fight/Cancel 使用 `LayoutElement.ignoreLayout=true` 和确定性尺寸/位置，避免窗口打开后的布局回写。
  - Fight 为 176×176、图标 124×124、标签“战斗”；Cancel 为 118×118、图标 78×78、标签“取消”。
  - 只校正按钮自身 CanvasGroup，不修改用于窗口显示/隐藏动画的祖先 CanvasGroup。
- `Assets/ReUI/Runtime/ReUIStarMapStyler.cs`
  - 固定底栏、动态势力和动态舰长统一使用按钮 112×112、直接子宿主 `ReUI Icon Host` 72×72、唯一子图标 `ReUI Vector Icon`。
  - 不再任取任意后代图标；禁用所有旧/重复 ReUIIconGraphic 和原 `Icon` Image，Points 徽标保持在最上层。
  - 图标显式启用、Alpha=1、`maskable=false`，按钮和 LayoutElement 尺寸同步。
  - 新增对真实 `StarSystemObjectItem` 的专用处理：创建独立 `ReUI Object Surface`，替换低 Alpha `Focus` 作为 `targetGraphic`；文本和实际 `Image` 图标保持正常亮度。
  - 禁用态 ColorBlock 改为高亮度不透明色，仅用较弱灰蓝描边表达不可用；样式前后恢复原 `interactable`，不改变游戏条件或点击事件。
  - 只读取/校正按钮自身 CanvasGroup，绝不改动祖先 AnimatedWindow CanvasGroup。
  - `Filters/Shop` 保持激活和功能；只隐藏独立 premium-currency `BuyButton`。
- `Assets/ReUI/Editor/ReUIValidation.cs`
  - 新增 `ValidateReUI9Presentation()`。
  - 竞技场验证先激活真实窗口对象，再执行布局重建、二次样式扫描，检查 `activeInHierarchy`、精确 `targetGraphic`、CanvasRenderer alpha、有效 CanvasGroup alpha、Mask、LayoutGroup、尺寸与原持久化事件。
  - 底栏验证要求每个按钮只有一个活动矢量图标，且按钮、Host、Vector Icon 的最终 Rect 尺寸一致。
  - 直接打开真实 `StarMapScene/InformationPanel`，强制切换 `interactable=false` 后检查 Surface、文本、图标亮度，并确认祖先 CanvasGroup alpha 未被修改。
  - 继续覆盖地图大小设置、确认弹窗取消按钮、市场透明填充、`Filters/Shop` 与 `BuyButton` 边界。
- `Assets/ReUI/Editor/ReUIQuickAndroidBuild.cs`
  - versionName 更新为 `Beta3-ReUI9`。
  - versionCode 更新为 `120009`。
  - 输出文件更新为 `ThreeBody-EventHorizon-Beta3-ReUI9.apk`。
- `Assets/Modules/AppConfiguration/Scripts/Generated/AppConfig.cs`
  - Unity 构建流程自动生成：version `Beta3-ReUI9`、versionCode `120009`、buildNumber `1872`。
- `REUI_HANDOFF.md`
  - 记录本阶段真实对象定位、修改依据、验证结果和剩余风险。

**实际执行的验证**

- Unity 6000.0.75f1 脚本编译：退出码 0，无 `error CS`；126 条为项目既有 Zenject 过时 API 等警告。
- `ValidateReUI9Presentation`：退出码 0。
  - `arenaFight=runtime-visible-stack`
  - `arenaLayout=deterministic`
  - `starObjectDisabled=readable`
  - `shopFilter=preserved`
  - `buyButton=hidden`
  - `bottomButtonHosts=uniform`
  - `mapSize=uniform`
  - `dialogCancel=rectangular`
  - `marketFills=transparent`
- `ValidateLiquidGlass`：通过；Shader supported=True，blur 0.7，refraction 0.65，opacity 0.7。
- `ValidateReUI5Data`：通过；星舰地球科技 26 项、三体科技 18 项、4 个导弹改装池有效、二向箔 BulletPrefab=23、智子发射器 Clockwise90。
- `ValidateMainMenu`：通过；1 个 Canvas、9 个直接按钮、标题/版本对象存在、missingScripts=0。
- `ValidateReUI3Targets`：StarMapScene 和 ShipEditorScene 目标/缺失脚本检查通过；SkillTreeScene 因当前分支 `HEAD` 已存在的根级 `IconCache` 缺失脚本而返回失败。该 Scene 文件哈希与 `HEAD` 完全一致，不是本轮回归。
- `ValidateAllEnabledScenesSmoke`：通过；11 个启用场景、14 个 Canvas 全部完成 ReUI 样式扫描：Loader、CommonGui、MainMenu、StarMap、SkillTree、ConfigureControls、Combat、Exploration、Ehopedia、ShipEditor、Settings。
- 科技树：数据库验证和 SkillTreeScene 样式烟雾测试完成；未发现本轮新增异常。
- 舰长：动态 `ThreeBodyCaptainButton` 在专项验证中模拟创建，按钮/Host/图标尺寸和唯一活动图标检查通过。
- 联合进攻：代码审查确认 `ConfigurePreview4Layout`、`Preview5JointAttackButton`、`Preview7AlliedAttackDialog`、开关状态和点击回调仍保留；FactionPanel Prefab 验证及 StarMapScene 烟雾测试通过。
- 构建临时场景清理后，CommonGuiScene、SkillTreeScene、SettingsScene 源文件 Git blob/hash 均与 `HEAD` 相同，确认构建未改写源场景。

**Android 构建与 APK 核验**

- Unity Android 构建结果：`Succeeded`。
- 构建耗时：`00:05:40.4371482`。
- APK：`Builds/Android/ThreeBody-EventHorizon-Beta3-ReUI9.apk`。
- APK 大小：`118,991,765` 字节（约 113.48 MiB）。
- 包名：`com.threebody.EventHorizon`。
- versionName：`Beta3-ReUI9`。
- versionCode：`120009`。
- buildNumber：`1872`。
- minSdk：23；targetSdk/compileSdk：34。
- 原生架构：`arm64-v8a`。
- 签名：Android Debug，RSA 2048，1 个签名者；APK Signature Scheme v1、v2 验证通过。
- 签名证书 SHA-256：`12db3813c08e0d85d97884f7bf87e337d91c9e376e470a41e086a1f682402a96`。
- APK SHA-256：`5F0513DE07AD0E54738BF4442408B304DDA9CBA5584A156980EC4E36488EFA27`。

**尚未验证与剩余风险**

- `adb devices -l` 未发现连接设备，因此未执行真机安装、竞技场实际打开、按钮点击、截图和帧级渲染验证；最终可见性仍需在目标 Android 设备确认。
- 联合进攻仅完成代码结构、Prefab/场景扫描和样式烟雾检查，未在已注入真实存档/势力状态下完整点击流程。
- 当前分支基线的 SkillTreeScene 根级 `IconCache` 存在 1 个缺失脚本；构建流程会从临时场景副本移除缺失组件，源场景保持不变。该问题不是 ReUI9 引入，但仍属于项目既有技术债。
- CommonGuiScene 与 SettingsScene 的临时构建副本也各清理了 1 个既有缺失组件；源文件哈希与 `HEAD` 一致。本阶段未扩大范围修复这些历史引用。

**真机复核重点**

- 竞技场/侵袭者页面打开后，中央“战斗”Surface、盾徽与文字是否始终可见，点击是否仍触发原战斗逻辑。
- “太空要塞”等 `interactable=false` 对象是否保持文字与图标清晰，同时确实不可点击。
- 星图底栏固定按钮、势力和舰长是否均为相同视觉尺寸，且无重复、消失或残留原图标。
- 地图大小选项、竞技场取消按钮和其他确认弹窗取消按钮是否保持统一样式与原功能。
- `Filters/Shop` 是否可见并正常筛选商店地点；独立 `BuyButton` 是否保持隐藏。
- 三体/星舰地球科技树、联合进攻选择/取消和舰长页面是否能在真实存档中正常进入与返回。

### 2026-07-21 第十五阶段：ReUI11 图标恒亮、编辑器撤回箭头、战略武器与智子修复

> 本阶段规则覆盖第十四阶段中“禁用图标额外压暗”的旧规则。按钮是否可点击仍由原 `interactable`、冷却和玩法条件控制，但所有按钮文字及图标不再通过亮度区分普通、焦点、选中、按下或禁用状态。

**用户反馈与根因**

- 部分按钮叠加了不应出现的 `❌`：通用语义识别把 `clear/removeall/清空/全部移除` 自动映射为 `Close`，与按钮已有的禁用/清空标记重叠。
- 改船页面“撤回”和“返回”均使用直向左箭头，语义无法区分。
- 二向箔弹体仍使用普通弹丸贴图和偏大尺寸，不符合“白色纸片”设定。
- 恒星级氢弹使用旧式 `AmmunitionObsolete` 管线；将新效果挂到新版 `BulletFactory` 不会执行。其真实创建路径为 `ShipBuilder -> WeaponFactory -> BulletFactoryObsolete`。
- 智子装置脚本存在，但 AI 的 `SpecialRules.UseDevices()` 没有为 `SophonJammerDevice` 注册激活策略；同时原按键边沿逻辑只适合玩家点击，不适合 AI 冷却后再次激活。
- 部分原生 Sprite 图标和设施按钮仍会被 `ColorTint`、`CanvasRenderer` 或命名启发式压暗；势力设施的玻璃底板对象名为 `Image`，曾被误判为图标并改成 Alpha 1 的实心块。

**关键改动**

- `ReUIIconKind` 新增末尾成员 `Undo`，避免改变既有枚举序列化值；`DrawUndo()` 使用 `VertexHelper` 绘制 180° U 形掉头箭头。ShipEditor 的 `UndoButton` 使用 `Undo`，`BackButton` 保留直向返回箭头。
- `clear/removeall` 不再自动安装 `Close` 图标；最终规范化会移除非关闭/退出/取消按钮上的误生成 X 图标，并只在真实 `ClearButton` 保留单斜杠 `ReUIProhibitGraphic`。
- `ReUIButtonMotion`、`ReUICanvasStyler` 和 `ActionButton` 统一将文字、原生 Sprite 图标、ReUI 矢量图标和战斗徽记的颜色 Alpha、CanvasRenderer Alpha 固定为 1。普通、焦点、选中、按下和禁用状态使用同一最亮图标颜色；状态仍控制输入与冷却，不再控制图标亮度。
- 背景识别优先检查 ReUI 圆角玻璃 Sprite、Toggle `targetGraphic` 及明确的 Background/Focus/Fill/Mask/Frame 等对象，避免再次把玻璃底板当作图标。势力设施按钮禁用 `ColorTint` 过渡，但保留原 `interactable` 逻辑和低透明玻璃底板。
- `ResourceLocator` 对 `dual_vector_foil_projectile` 生成并缓存 48×18 的白色纸片 Sprite，带 1 像素浅灰边缘；弹体数据库尺寸改为 `Size=0.38`、`Margins=0.06`。
- 新增 `CreateBattlewideEmpAction`。恒星级氢弹在旧 `BulletFactoryObsolete` 的真实引爆路径中识别后触发：除 `scene.PlayerShip` 外，对场上全部活动敌我舰船施加 30 秒雷达/锁定干扰，立即抽取 20% 最大能量，并持续消耗 1 能量/秒。该参数与模组现有三体 EMP 弹的 `RadarInterference` 机制一致，仅延长范围与持续时间。
- 为避免扩大 `IWeaponDataObsolete` 接口并触发 Unity 6000 TypeCache/场景序列化不稳定，恒星级氢弹通过不受舰船倍率修改的旧弹药稳定属性识别：`AcidRocket`、20 HP、Size 2.25、AoE 50、`SatelliteRocket` Prefab。
- `SpecialRules.UseDevices()` 为 `SophonJammerDevice` 增加 AI 激活策略；玩家仍使用按压边沿触发，非玩家在策略激活、能量和冷却允许时触发。`DeviceClass.SophonJammer` 加入一次性按钮立即释放类别，使玩家和 AI 均可在冷却后重新使用。
- 恒星级氢弹中文说明已补充 30 秒 EMP 和玩家舰船豁免规则。

**主要修改文件**

- `Assets/ReUI/Runtime/ReUIIconGraphic.cs`
- `Assets/ReUI/Runtime/ReUIPalette.cs`
- `Assets/ReUI/Runtime/ReUIShipEditorStyler.cs`
- `Assets/ReUI/Runtime/ReUICanvasStyler.cs`
- `Assets/ReUI/Runtime/ReUIButtonMotion.cs`
- `Assets/ReUI/Runtime/ReUIFactionPanelStyler.cs`
- `Assets/Scripts/Gui/Controls/ActionButton.cs`
- `Assets/Modules/ResourceLocator/Scripts/ResourceLocator.cs`
- `Assets/Modules/Database/Resources/Database/Ammunition/Bullets/DualVectorFoil.json`
- `Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Bullet/Action/CreateEmpAction.cs`
- `Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactoryObsolete.cs`
- `Assets/Modules/BattleSimulator/Scripts/Combat/AI/Strategy/Factories/SpecialRules.cs`
- `Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/SophonJammerDevice.cs`
- `Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Ship/ShipSystemsExtensions.cs`
- `Assets/Resources/Localization/Chinese/ThreeBody.xml`
- `Assets/ReUI/Editor/ReUIValidation.cs`
- `REUI_HANDOFF.md`

**实际验证**

- Unity 6000.0.75f1 最终脚本编译：0 个 C# 错误；日志中的警告均为项目既有或重复输出。
- `ValidateReUI11Presentation` 通过：`icons=uniform-full-brightness`、`accidentalCloseOverlays=removed`、`shipEditorUndo=180-degree-arrow`、`dualVectorFoil=small-white-paper`、`stellarHydrogenBomb=battlewide-30s-emp`、`sophon=player-and-ai-activation`。
- ReUI10 回归同时通过：按钮底板保留、文字和图标不透明、无额外发光、独立战斗徽记、战利品背景透明、雷达原配色和圆形点均保持。
- 液态玻璃验证通过：Shader supported=True，blur 0.7，refraction 0.65，opacity 0.7。
- 数据验证通过：星舰地球科技 26、三体科技 18、导弹改装 4、二向箔 BulletPrefab 23、智子发射器顺时针 90°。
- 主菜单验证通过：1 个 Canvas、9 个直接按钮、标题和版本信息存在、缺失脚本 0。
- 全部启用场景烟雾测试通过：11 个场景、14 个 Canvas。

**Android 构建与核验**

- Unity 构建结果：`Succeeded`；成功构建耗时：`00:03:31.8519085`。
- APK：`Builds/Android/ThreeBody-EventHorizon-Beta3-ReUI11.apk`。
- 文件大小：`119,003,807` 字节。
- SHA-256：`d468e69ed5a85ee1c4ef43e4d2588d1b90afc8e3c7ca994a5dd8bd21d03f94b6`。
- 包名：`com.threebody.EventHorizon`；应用名：`三体视界`。
- versionName：`Beta3-ReUI11`；versionCode：`120011`；buildNumber：`1879`。
- minSdk：23；target/compileSdk：34；ABI：`arm64-v8a`。
- Android Debug 签名；v1、v2 签名方案验证通过；签名者 1。
- 证书 SHA-256：`12db3813c08e0d85d97884f7bf87e337d91c9e376e470a41e086a1f682402a96`。
- 构建临时清理后，`CommonGuiScene`、`SkillTreeScene`、`SettingsScene` 的工作区 Git blob 哈希均与 `HEAD` 对应源场景一致，源场景未被改写。

**已知风险与真机复核重点**

- 当前 `adb devices -l` 没有连接设备，因此未实际安装、点击或以 Logcat 验证战斗逻辑。
- EMP 的旧弹药创建路径、动作挂载、目标过滤和参数已通过编译与专项检查，但仍需在真机战斗中确认：玩家当前舰船不受影响，其他敌我舰船均持续 30 秒无法正常雷达锁定并被抽取能量。
- 智子玩家/AI 激活路径已修复并通过结构验证，但仍需真机确认 UI 按键、AI 自动释放、冷却后再次释放及实际雷达干扰表现。
- 所有图标应始终保持最亮外观；禁用状态只允许通过不可点击、冷却遮罩或按钮底板表达，不应再降低图标与文字亮度。

### 2026-07-21 第十六阶段：ReUI13 智子直接请求、按钮闪烁根因与语义图标重绘

**本轮纠正**

- ReUI12 的智子校验只确认了 FixedUpdate 协程代码存在，没有验证触屏请求能可靠到达装置；真机反馈证明该结论无效。
- “快速战斗”和“下个敌人”不应删除图标。原交叉武器在小尺寸下像关闭 X，本轮改为无交叉线条的专用矢量图标。
- 按钮明暗变化的直接写入者是 `ImageBlink.LateUpdate()`：它每帧把 Alpha 在 0.5–1.0 之间变化，晚于 ReUI 的普通状态刷新执行。

**关键改动**

- `SophonJammerDevice.RequestActivation()` 直接锁存触屏激活请求，由装置自己的下一次物理更新消费，不再依赖按下/松开状态跨过 FixedUpdate。
- 智子干扰应用后立即清空敌舰所有武器目标和武器控制状态；`ShipSystems` 在雷达干扰期间于武器平台更新前持续压制所有 `IWeapon`，防止 AI 或残留控制继续射击。
- `RadarStatus` 对 `Effects`/`Systems` 为空的诱饵和虫体分段改为安全跳过，避免某个辅助单位中断整次全场智子释放。
- `ImageBlink` 的旧正弦 Alpha 动画改为恒定 1；`ReUIButtonMotion` 和 `ActionButton` 使用高执行顺序的 `LateUpdate` 每帧恢复完整亮度。
- `ReUIIconKind` 新增末尾成员 `QuickBattle` 和 `NextEnemy`：快速战斗使用闪电与速度线；下个敌人使用右箭头与终点标记。两者都不含交叉笔画。
- 手动舰队的泰坦支持保持 ReUI12 结果不变。

**主要修改文件**

- `Assets/Scripts/Gui/Combat/ShipControlsPanel.cs`
- `Assets/Scripts/Gui/Controls/ActionButton.cs`
- `Assets/Scripts/Legacy/GUI/Helpers/ImageBlink.cs`
- `Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/SophonJammerDevice.cs`
- `Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/ShipSystems.cs`
- `Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Ship/Effects/RadarStatusEffect.cs`
- `Assets/ReUI/Runtime/ReUIButtonMotion.cs`
- `Assets/ReUI/Runtime/ReUIIconGraphic.cs`
- `Assets/ReUI/Runtime/ReUIMainMenuStyler.cs`
- `Assets/ReUI/Runtime/ReUICombatStyler.cs`
- `Assets/ReUI/Editor/ReUIValidation.cs`
- `Assets/ReUI/Editor/ReUIQuickAndroidBuild.cs`

**专项验证边界**

- ReUI13 验证会实际执行 `ImageBlink.LateUpdate()`、按钮稳定化 `LateUpdate()` 和 `SophonJammerDevice.RequestActivation()`，并检查两个新图标实际安装到对应按钮。
- 编辑器专项验证不能替代 Android 真机战斗；智子对真实敌舰停火、能量扣除、冷却和视觉反馈仍必须安装 APK 后复核。

**实际验证与最终构建**

- Unity 6000.0.75f1 最终 C# 编译通过：0 个编译错误；最终编译日志中 18 条为项目既有警告。
- `ValidateReUI13Presentation` 通过：`sophonRequest=runtime-latched`、`jammedWeaponGuard=present`、`buttonBlink=executed-and-suppressed`、`quickBattleIcon=lightning`、`nextEnemyIcon=next-marker`、`configurableTitans=listed-and-parsed`。
- 液态玻璃 Shader 验证通过：`ReUI/LiquidGlassUI` supported=True，blur=0.7，refraction=0.65，opacity=0.7。
- ReUI5 数据验证通过：星舰地球科技 26、三体科技 18、导弹改装 4、二向箔 BulletPrefab 23、智子发射器顺时针 90°。
- 主菜单验证通过：1 个 Canvas、9 个直接按钮、标题与版本信息存在、缺失脚本 0。
- 全部启用场景烟雾测试通过：11 个场景、14 个 Canvas。
- 构建临时处理完成后，`CommonGuiScene`、`SkillTreeScene`、`SettingsScene` 的 Git blob 哈希均与 `HEAD` 一致，源场景未被改写。
- 第一次 Android 构建在 Unity Licensing Client 握手阶段退出，未进入项目构建方法；许可证客户端恢复后重试成功，不属于代码、Gradle 或 IL2CPP 错误。
- Android 构建结果：`Succeeded`；耗时：`00:04:15.4736249`。
- APK：`Builds/Android/ThreeBody-EventHorizon-Beta3-ReUI13.apk`。
- APK 大小：`119,004,136` 字节。
- SHA-256：`e28d452f4a9d5a7bb7c7370397293c85def950aec350d4b8a03a8852a3f7f1bc`。
- 包名：`com.threebody.EventHorizon`；应用名：`三体视界`。
- versionName：`Beta3-ReUI13`；versionCode：`120013`；buildNumber：`1881`。
- minSdk：23；target/compileSdk：34；ABI：`arm64-v8a`。
- Android Debug 签名；APK Signature Scheme v1、v2 验证通过；证书 SHA-256：`12db3813c08e0d85d97884f7bf87e337d91c9e376e470a41e086a1f682402a96`。
- `adb devices -l` 返回空设备列表，因此没有执行安装、触屏点击、战斗行为或 Logcat 验证。

### 2026-07-21 第十六阶段：ReUI12 智子物理帧脉冲、按钮状态冻结、手动泰坦舰队与误识别战斗图标清理

**真机反馈与根因**

- 智子仍无效果。真实原因不在 `RadarStatus`：战斗按钮把 `SophonJammer` 归入“立即释放”，触屏按下与松开在同一渲染帧内先后写入 `SystemsState=true/false`，而 `ShipSystems.UpdatePhysics()` 只在 FixedUpdate 读取最终状态，因此从未看到激活脉冲。
- 按钮仍时亮时暗。通用 `ColorBlock`、专用样式器、`ReUIButtonMotion` 和主菜单禁用态同时修改颜色、CanvasRenderer Alpha 与缩放；即使图标本身 Alpha 为 1，Selectable 仍会把整套 Button 乘以不同状态色。
- 手动配置敌方舰队没有泰坦。两艘新增泰坦使用 `SizeClass.TitanP = 6`，而手动列表和字符串解析均复用了随机快速战斗过滤器；该过滤器明确排除 `TitanP`。
- “快速战斗”和“下个敌人”的红色 `❌` 并非 Close 图标，而是 `ReUIIconKind.Battle` 的交叉武器在小尺寸下呈现为 X。

**关键改动**

- `ShipControlsPanel` 为 `SophonJammerDevice` 增加一次性物理帧脉冲：按下后保持对应 `SystemsState` 为 true，协程等待 `WaitForFixedUpdate`，确保至少一次 `ShipSystems.UpdatePhysics()` 读取激活状态后才复位。触屏松开事件对该脉冲不再提前清零；切换舰船时会安全清理未完成脉冲。
- 保留智子原有 60 秒敌方雷达干扰、揭露隐身、能量与 90 秒冷却逻辑；AI 激活规则不变。
- 所有 ReUI Button 的 normal、highlighted、selected、pressed、disabled 统一为白色乘算，fadeDuration=0，transition=None；文字、图标和玻璃层 CanvasRenderer Alpha 始终为 1。`ReUIButtonMotion` 不再因焦点、选择、按下或禁用改变缩放和子图层 Alpha。
- 主菜单按钮不再按 `interactable` 改变玻璃透明度或文字颜色；不可点击逻辑保持原样。
- `ForceSemanticIcon(None)` 现在会主动禁用已经由通用扫描安装的语义图标。主菜单 `Combat`（快速战斗）和战斗暂停菜单 `NextEnemy` 明确使用 None，因此交叉武器/X 被删除；原按钮事件和点击区域不变。
- 新增 `QuickCombatState.IsConfigurableQuickBattleBuild()`：开发者势力仍排除，但允许 `TitanP`。手动敌方/友军舰队列表与配置字符串解析使用此过滤器；随机快速战斗池继续使用 `IsQuickBattleBuild()`，因此两艘战略泰坦不会随机刷出。
- Earth Titan build `94008` 与 Trisolaris Titan build `1145140` 均通过数据库实例检查，可出现在手动配置列表并被配置字符串读取。

**主要修改文件**

- `Assets/Scripts/Gui/Combat/ShipControlsPanel.cs`
- `Assets/Scripts/GameStateMachine/States/QuickCombatState.cs`
- `Assets/Scripts/Gui/MainMenu/MainMenu.cs`
- `Assets/ReUI/Runtime/ReUIButtonMotion.cs`
- `Assets/ReUI/Runtime/ReUICanvasStyler.cs`
- `Assets/ReUI/Runtime/ReUISpecializedVisuals.cs`
- `Assets/ReUI/Runtime/ReUIMainMenuStyler.cs`
- `Assets/ReUI/Runtime/ReUICombatStyler.cs`
- `Assets/ReUI/Editor/ReUIValidation.cs`
- `Assets/ReUI/Editor/ReUIQuickAndroidBuild.cs`
- `Assets/Modules/AppConfiguration/Scripts/Generated/AppConfig.cs`
- `REUI_HANDOFF.md`

**实际验证**

- Unity 6000.0.75f1 最终脚本编译：0 个 C# 错误。
- `ValidateReUI12Presentation` 通过：`sophon=fixed-update-pulse`、`buttons=state-invariant`、`quickBattleIcon=removed`、`nextEnemyIcon=removed`、`configurableTitans=listed-and-parsed`。
- ReUI10 回归验证通过；ReUI11 的撤回箭头、二向箔、恒星级氢弹和智子数据/AI 路径检查继续通过。
- 液态玻璃验证通过：Shader supported=True，blur 0.7，refraction 0.65，opacity 0.7。
- 数据验证通过：星舰地球科技 26、三体科技 18、导弹改装 4、二向箔 BulletPrefab 23、智子发射器顺时针 90°。
- 主菜单验证通过：1 个 Canvas、9 个直接按钮、标题和版本信息存在、缺失脚本 0。
- 全部启用场景烟雾测试通过：11 个场景、14 个 Canvas。
- 构建临时清理后，`CommonGuiScene`、`SkillTreeScene`、`SettingsScene` 的工作区哈希与 `HEAD` 一致，源场景未被改写。

**Android 构建与核验**

- 构建结果：`Succeeded`；耗时：`00:05:45.2451307`。
- APK：`Builds/Android/ThreeBody-EventHorizon-Beta3-ReUI12.apk`。
- 文件大小：`118,999,911` 字节。
- SHA-256：`0a5a93a38e8e3a46eff3f96d79cddff6e77455fdf6047cc37d39345dd752dedf`。
- 包名：`com.threebody.EventHorizon`；应用名：`三体视界`。
- versionName：`Beta3-ReUI12`；versionCode：`120012`；buildNumber：`1880`。
- minSdk：23；target/compileSdk：34；ABI：`arm64-v8a`。
- Android Debug 签名；v1、v2 验证通过；证书 SHA-256：`12db3813c08e0d85d97884f7bf87e337d91c9e376e470a41e086a1f682402a96`。

**尚未验证与真机重点**

- 当前没有连接 Android 设备，尚未执行安装、触屏点击或 Logcat。真机应重点确认智子点击后立即闪光并使敌舰失去雷达/锁定能力，冷却后可再次使用。
- 确认快速战斗与下个敌人左侧不再出现交叉武器/X；按钮在可用、禁用、焦点、选择和按下状态之间不再改变明暗。
- 打开“配置敌方舰队”确认列表包含星环号/星舰地球泰坦和三体泰坦，并能添加后正常进入快速战斗。

### 2026-07-21 第十七阶段：ReUI14 智子隐形 EMP 弹体、按钮最终稳定、空间站显隐与舰船不可用遮罩

**真机反馈与根因**

- 智子即使改为装置内部锁存请求仍无法在真机可靠生效，因此不再继续依赖设备直接遍历敌舰的路径。
- 按钮仍有明暗变化。除普通 `Button` 外，`Toggle`、内容列表按钮和自定义 `ActionButton` 仍存在独立的状态颜色写入；仅修复 `ImageBlink` 不足以覆盖全部控件。
- 空间站右侧操作显示混乱。`FactionPanelViewModel` 已按原逻辑隐藏“占领、和平交接、联合进攻”等按钮，但 `ReUIFactionPanelStyler` 随后调用 `target.gameObject.SetActive(true)`，把已隐藏按钮重新激活。
- 舰船管理界面在所选槽位尚未通过技能解锁时，舰船条目的 `Disabled` 遮罩使用近黑色主题图层，导致整艘舰船看起来像黑块。

**关键改动**

- 智子改走与恒星级氢弹相同的旧式弹药触发管线：点击后创建 `Combat/Bullets/Empty` 隐形弹体，尺寸 0.03、射程 0.5、速度 20，实际约 0.025 秒后过期引爆。
- `BulletFactoryObsolete.CreateTriggers()` 在弹体加入战场前挂载 `CreateEnemyFleetEmpAction`，不再由智子装置在弹体创建后临时追加动作。
- 智子 EMP 只作用于全部敌军舰船：60 秒雷达/锁定与武器抑制、20% 最大能量立即抽取、持续 1 能量/秒，并揭露敌方雷达隐身；玩家和友军不受影响。
- `ReUIButtonMotion` 扩展为覆盖全部 `Selectable`，包括 `Button` 和 `Toggle`；每帧最终统一 normal/highlighted/selected/pressed/disabled 颜色、关闭状态过渡并恢复图层 Alpha。
- 内容列表按钮与 Toggle 不再保留状态色差或禁用该稳定器；`ActionButton` 继续使用高执行顺序 `LateUpdate` 固定最亮图标。
- 空间站样式器不再更改任何按钮、文字或图标对象的 activeSelf，仅修改已由原业务逻辑显示的对象外观。`FactionPanelViewModel.OnEnable()` 完成原显示判断后再刷新 ReUI 外观。
- 空间站右侧设施按钮底板 Alpha 从 0.06 提升至 0.12，边框 Alpha 提升至 0.90；文字和图标保持完全不透明。
- 舰船列表不可用遮罩改为 Alpha 0.18 的冷灰蓝轻遮罩，并在 LateUpdate 防止主题系统重新写回近黑色；不可安装逻辑保持不变。

**主要修改文件**

- `Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Systems/DeviceFactory.cs`
- `Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactoryObsolete.cs`
- `Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/SophonJammerDevice.cs`
- `Assets/Modules/BattleSimulator/Scripts/Combat/Unit/Bullet/Action/CreateEmpAction.cs`
- `Assets/ReUI/Runtime/ReUIButtonMotion.cs`
- `Assets/ReUI/Runtime/ReUICanvasStyler.cs`
- `Assets/ReUI/Runtime/ReUIFactionPanelStyler.cs`
- `Assets/Scripts/Legacy/GUI/ViewModel/StarMap/FactionPanelViewModel.cs`
- `Assets/Scripts/Gui/StarMap/Ships/ShipListItem.cs`
- `Assets/ReUI/Editor/ReUIValidation.cs`
- `Assets/ReUI/Editor/ReUIQuickAndroidBuild.cs`

**专项验证**

- Unity C# 编译通过：0 个错误。
- `ValidateReUI14Presentation` 通过：`sophon=invisible-short-range-expiring-projectile`、`enemyEmp=legacy-trigger-pipeline`、`buttons=button-toggle-actionbutton-stable`、`starbaseVisibility=original-logic-preserved`、`starbaseButtons=readable`、`unavailableShips=light-overlay-not-black-block`。
- 专项验证实际构造智子弹药统计、检查旧式弹药工厂保存的 EMP 参数、执行 Button/Toggle/ActionButton 最终稳定器、比较空间站按钮样式化前后的 activeSelf，并执行舰船不可用遮罩 LateUpdate。

**完整回归与 Android 构建**

- Unity 6000.0.75f1 最终 C# 编译通过：0 个错误；最终稳定编译日志中 18 条为项目既有警告。
- `ValidateReUI14Presentation` 最终通过。
- 液态玻璃验证通过：Shader `ReUI/LiquidGlassUI` supported=True，blur=0.7，refraction=0.65，opacity=0.7。
- ReUI5 数据验证通过：星舰地球科技 26、三体科技 18、导弹改装 4、二向箔 BulletPrefab 23、智子发射器顺时针 90°。
- 主菜单验证通过：1 个 Canvas、9 个直接按钮、标题与版本信息存在、缺失脚本 0。
- 全部启用场景烟雾测试通过：11 个场景、14 个 Canvas。
- Android 构建结果：`Succeeded`；耗时：`00:10:48.0384761`。
- APK：`Builds/Android/ThreeBody-EventHorizon-Beta3-ReUI14.apk`。
- APK 大小：`119,007,708` 字节。
- SHA-256：`ea30007af159ef597f50eea3471ffb9ece5c0280dbf6476e68320ffa3c5b0001`。
- 包名：`com.threebody.EventHorizon`；versionName：`Beta3-ReUI14`；versionCode：`120014`；buildNumber：`1882`。
- minSdk：23；target/compileSdk：34；ABI：`arm64-v8a`。
- Android Debug 签名；APK Signature Scheme v1、v2 验证通过；证书 SHA-256：`12db3813c08e0d85d97884f7bf87e337d91c9e376e470a41e086a1f682402a96`。
- 构建临时处理后，`CommonGuiScene`、`SkillTreeScene`、`SettingsScene` 的 Git blob 哈希均与 `HEAD` 一致，源场景未被改写。

**尚未验证与真机重点**

- `adb devices -l` 返回空设备列表，因此未执行 APK 安装、触屏操作、战斗行为或 Logcat。
- 真机应重点确认智子点击后隐形短程弹体快速过期，并让全部敌军进入 60 秒 EMP；玩家和友军不得受影响。
- 继续观察普通 Button、Toggle、内容列表按钮和 ActionButton 是否仍有任何状态闪烁。
- 在已占领空间站打开右侧面板，确认仅显示商店、工厂、船坞、防卫等适用操作，不再出现和平交接、占领或联合进攻。
- 在未解锁对应舰船槽位时，舰船条目应显示轻度冷灰蓝不可用遮罩，而不是近黑色块。

### 2026-07-21 第十八阶段：ReUI15 UI 范围回退与星图性能修复

**用户最终范围**

- UI 改动仅保留：
  - 改船页面的装置列表；
  - 战斗页面的新版雷达；
  - 战斗页面的生命、护盾和能量连续条。
- 改船页面顶部按钮和右侧图标全部恢复原版。
- 星图、主菜单、设置、科技树、技能树、舰长、商店、空间站、竞技场、战斗菜单、战利品等其他页面不再应用 ReUI 视觉覆盖。
- 非 UI 的玩法、数据库和联机功能不随本次范围回退撤销。

**卡顿根因**

- `ReUIBootstrap` 原先每 0.12 秒调用一次 `Resources.FindObjectsOfTypeAll<Canvas>()`，并对全部 Canvas 反复执行完整层级扫描。
- 每次扫描又会遍历 Image、Button、Text、Toggle、Slider、Scrollbar、InputField 和多个专用样式器。
- `ReUIButtonMotion.LateUpdate()` 还会在每个 Selectable 上逐帧调用 `GetComponentsInChildren<Graphic>(true)`。
- 星图对象数量和动态按钮数量最多，因此该页受到的 CPU 和 GC 压力最明显。

**关键改动**

- 移除 ReUI 的 0.12 秒动态扫描协程、触摸抬起后的全 Canvas 重刷及运行时全局 `ReUICanvasStyler.Apply()`。
- `ReUIBootstrap` 只在 `ShipEditorScene` 和 `CombatScene` 加载时执行有限次数的一次性处理；其他场景立即返回。
- `ReUIShipEditorStyler` 只处理 `RightPanel/ComponentList` 和其中的组件列表行；不再处理顶部按钮、撤回、返回、退出、舰船列表、卫星列表、构建列表或任何右侧图标。
- `ReUIHudStyler` 只处理 `CombatScene` 的生命、护盾和能量条及对应数值；移除舰船选择条和改船页迷你属性样式。
- 恢复原版 `ActionButton`、`ImageBlink`、`ShipListItem` 状态逻辑。
- 移除 `FactionPanelViewModel` 对 ReUI 空间站样式器的显式调用，空间站回归原业务显隐与原版视觉。
- 新版 `CombatMinimap` 保留，并继续使用原有分类颜色和圆形标记。

**主要修改文件**

- `Assets/ReUI/Runtime/ReUIBootstrap.cs`
- `Assets/ReUI/Runtime/ReUIShipEditorStyler.cs`
- `Assets/ReUI/Runtime/ReUIHudStyler.cs`
- `Assets/Scripts/Gui/Controls/ActionButton.cs`
- `Assets/Scripts/Legacy/GUI/Helpers/ImageBlink.cs`
- `Assets/Scripts/Gui/StarMap/Ships/ShipListItem.cs`
- `Assets/Scripts/Legacy/GUI/ViewModel/StarMap/FactionPanelViewModel.cs`
- `Assets/ReUI/Editor/ReUIValidation.cs`
- `Assets/ReUI/Editor/ReUIQuickAndroidBuild.cs`

**实际验证**

- Unity 6000.0.75f1 C# 编译通过：0 个错误。
- `ValidateScopedUiRollback` 通过：`starMap=untouched`、`runtimeScan=removed`、`shipEditor=device-list-only`、`shipEditorIcons=original`、`combat=radar-and-resource-bars-only`。
- 专项验证在星图实际调用运行时入口并比较全部 Image 签名，确认没有新增 ReUI 图标、按钮状态组件或颜色/材质变化。
- 专项验证在改船页面比较处理前后的顶部按钮和右侧非背景图像，确认原版图标、Sprite、颜色、材质与启用状态均不变。
- 专项验证确认装置列表仍使用保留的玻璃面板和组件行样式。
- 专项验证确认战斗菜单图像完全不变，生命和能量条仍使用连续条颜色，雷达圆形标记及原配色继续保留。
- `ValidateAllEnabledScenesScopedSmoke` 通过：11 个启用场景、14 个 Canvas。

**构建版本**

- versionName：`Beta3-ReUI15`
- versionCode：`120015`
- APK：`Builds/Android/ThreeBody-EventHorizon-Beta3-ReUI15.apk`

**最终 Android 构建与核验**

- Unity 构建结果：`Succeeded`；耗时：`00:08:56.6602688`。
- APK 大小：`119,002,281` 字节。
- SHA-256：`69047cb6d3ede0d9d6c57af42fb44c7a0399f12b1486c30c90d87cdf4e60dff7`。
- 包名：`com.threebody.EventHorizon`；buildNumber：`1883`。
- minSdk：23；target/compileSdk：34；ABI：`arm64-v8a`。
- Android Debug 签名；APK Signature Scheme v1、v2 验证通过；证书 SHA-256：`12db3813c08e0d85d97884f7bf87e337d91c9e376e470a41e086a1f682402a96`。
- 构建临时处理后，`CommonGuiScene`、`SkillTreeScene`、`SettingsScene` 的 Git blob 哈希均与 `HEAD` 一致，源场景未被改写。
- `adb devices -l` 返回空设备列表，因此未执行真机安装与帧率对比。

### 2026-07-22 Beta4：舰长图标、连续资源条、智子 EMP 弹体、紫色主题与控制图标

**实现依据与关键改动**

- 星图舰长入口由纯文字改为独立舰长头像矢量图标，保持原点击逻辑；`StatusPanel/BuyButton` 继续按既有方法单独隐藏。
- 战斗生命、护盾与能量显示保持连续长条；数值格式改为 64 位整数，避免超过 Int32 上限后溢出或显示错误。
- 智子不再依赖按钮状态跨越 FixedUpdate。点击后立即发射一个无可见贴图、射程 0.5、速度 20、寿命 0.05 秒的 `Empty` 弹体，弹体到期后沿旧式弹药触发链对全部敌军施加 60 秒 EMP。
- 智子原能耗约 28000 的原因是 `1000 × 0.05 × 560` 的舰船尺寸缩放；现关闭 `ScaleEnergyWithShipSize` 并将实际固定能耗设为 2000。
- 星环号自带曲速引擎的默认 `KeyBinding` 改为 6。
- 水滴自带智子导引装置使用项目内现有 `controls_sophon_guidance.png`。修正了该 256×256 图片错误的 1090×1070 Sprite 切片，并增加 ResourceLocator 的运行时加载回退。
- 普通 EMP 导弹和三体 EMP 导弹的控制按钮图标均改为 `controls_missile`。
- `UiTheme.json` 中窗口、按钮、焦点、图标、选择、正文、科技状态等蓝色主色字段统一改为紫色系；警告色、品质色、货币资源色保持原语义。

**验证与构建**

- Unity `--rebuildLibrary` 成功完成，重建后的日志中无 LMDB、数据库损坏或原生崩溃。
- 最终脚本编译：0 个 C# 错误、0 条编译警告。
- `ValidateBeta4` 通过：`captainButton=icon`、`buyButton=hidden`、`combatBars=continuous`、`resourceNumbers=int64`、`sophon=immediate-invisible-emp-projectile`、`earthTitanWarp=sixth-action-button`、`guidanceIcon=applied`、`empIcon=missile`、`uiTheme=purple`。
- UI 范围回归通过：星图未重新启用全局 ReUI 扫描，改船页仍只保留装置列表样式，战斗页仍只保留雷达与资源条。
- 全部启用场景烟雾测试通过：11 个场景、14 个 Canvas。
- 数据验证通过：星舰地球科技 26、三体科技 18、导弹改装 4、二向箔 BulletPrefab 23、智子发射器顺时针 90°。
- Android 构建结果：`Succeeded`；耗时 `00:08:16.8958737`。
- APK：`Builds/Android/ThreeBody-EventHorizon-Beta4.apk`。
- 文件大小：`119,003,847` 字节。
- SHA-256：`d6eb68737a52453f67cc5ac1d178c6c2f65a394a5cc5aad98eb7f0ee4e92661c`。
- 包名：`com.threebody.EventHorizon`；应用名：`三体视界`。
- versionName：`Beta4`；versionCode：`130000`；buildNumber：`1884`。
- minSdk：23；target/compileSdk：34；ABI：`arm64-v8a`。
- Android Debug 签名；APK Signature Scheme v1、v2 验证通过；证书 SHA-256：`12db3813c08e0d85d97884f7bf87e337d91c9e376e470a41e086a1f682402a96`。
- 构建前后 `CommonGuiScene`、`SkillTreeScene`、`SettingsScene` 的 Git blob 哈希一致，源场景未被改写。

**尚未验证**

- 当前无 Android 设备连接，未执行真机安装、触屏点击、智子 EMP 实战和 Logcat。

### 2026-07-22 Beta4 最终视觉修正与重构建

**根因与修正**

- 战斗生命、护盾和能量条此前只在场景加载时由 ReUI 处理；实际 `ShipStatsPanel` 来自对象池并在之后启用，因此仍保留平铺格子 Sprite。现由 `ShipStatsPanel.Awake/Open` 直接调用 `ProgressBar.UseSolidTexture()`，确保对象池实例也是连续实心长条。
- 舰长入口不再使用 `ReUIIconGraphic` 运行时矢量绘制，改为 128×128 透明背景位图资源并由普通 `Image` 显示。
- 主页不再创建 `ThreeBodyMultiplayerButton`，并隐藏旧实例；开发组信息颜色改为读取紫色主题标题色。
- 新增动态窗口不再在打开后递归扫描 Graphic/Selectable 并做蓝转紫，而是在创建时直接读取数据库 `UiSettings`。
- 实际遮挡内容的关闭按钮位于 `ModulesShared/ShipEditor/Scripts/UI/ComponentPanel.cs` 动态创建的“改装 · 武器”选择器，而不是 `Gui.Craft.ModificationsPanel`。现将其移到右上角，并按可用高度动态计算改装选项行高，避免 5～6 个选项越界重叠。
- 开局剧情框继续使用原 `prologue_frame` 素材，但首次加载时仅把蓝色框像素转换为紫色并缓存为 `prologue_frame_purple`；六张剧情插画不变，也不存在逐帧转换。
- `UiTheme.json` 补齐 `HeaderTextColor`、`PaleTextColor`、`BrightTextColor`、`BackgroundDark` 和 `CreditsColor`，避免动态窗口继承未覆盖的原版蓝色默认值。

**主要修改文件**

- `Assets/Scripts/Gui/Controls/ProgressBar.cs`
- `Assets/Scripts/Gui/Combat/ShipStatsPanel.cs`
- `Assets/Scripts/Gui/Common/ThreeBodyUiPalette.cs`
- `Assets/Scripts/Gui/MainMenu/MainMenu.cs`
- `Assets/Scripts/Gui/StarMap/GameMenu.cs`
- `Assets/Scripts/Gui/MainMenu/SettingsProgress.cs`
- `Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentPanel.cs`
- `Assets/Scripts/Gui/Quests/ThreeBodyPrologueOverlay.cs`
- `Assets/Modules/Database/Resources/Database/Settings/UiTheme.json`
- `Assets/Modules/Database/.Editor/Database/Settings/UiTheme.json`
- `Assets/ReUI/Editor/ReUIValidation.cs`

**最终验证与构建**

- Unity 6000.0.75f1 最终脚本编译通过：0 个 C# 错误。
- `ValidateBeta4` 通过：`dynamicTheme=ui-settings`、`multiplayerEntry=hidden`、`modClose=top-right`、`prologueFrame=purple`，并继续覆盖舰长位图、连续资源条、Int64 数值、智子弹体、控制图标和紫色主题。
- ReUI5 数据验证通过：星舰地球科技 26、三体科技 18、导弹改装 4、二向箔 BulletPrefab 23、智子发射器顺时针 90°。
- 全部启用场景烟雾测试通过：11 个场景、14 个 Canvas。
- Android 构建结果：`Succeeded`；耗时 `00:05:07.1115227`。
- APK：`Builds/Android/ThreeBody-EventHorizon-Beta4.apk`。
- 文件大小：`119,011,119` 字节。
- SHA-256：`5b6e6744c0f37b67dc13cf9b1546e8c850355fd440cf634045b95df835581929`。
- 包名：`com.threebody.EventHorizon`；应用名：`三体视界`。
- versionName：`Beta4`；versionCode：`130000`；buildNumber：`1885`。
- minSdk 23；target/compileSdk 34；ABI：`arm64-v8a`。
- Android Debug 签名；APK Signature Scheme v1、v2 验证通过；证书 SHA-256：`12db3813c08e0d85d97884f7bf87e337d91c9e376e470a41e086a1f682402a96`。
- 构建前后 `CommonGuiScene`、`SkillTreeScene`、`SettingsScene` 的 SHA-256 一致，源场景未被改写。
- `adb devices -l` 无连接设备，因此未执行真机安装、触屏检查和 Logcat。

### 2026-07-22 Beta5: authored icons, serialized dynamic theme, and exploration viewport fix

**Implemented**

- Replaced the captain shortcut's generated/vector fallback with the authored 512x512 PNG at `Resources/Textures/UI/captain.png`.
- Added authored transparent 512x512 faction emblems for factions 21 through 28 and made the developer faction use `faction_28`.
- Added a narrowly scoped texture importer so only the new captain and faction assets import as single Sprites; existing original multi-sprite assets are untouched.
- Moved dynamic settings and ship-editor surfaces to direct database `UiSettings` colours rather than a runtime-wide visual scan.
- Corrected the exploration surface background: it now scales from the active orthographic camera's viewport and aspect ratio, with a small overscan. This eliminates the square background and exposed blue side bars visible on wide displays.
- Set release identifiers to `Beta5` and version code `140000`. The completed APK contains build number `1886`; Unity's post-build hook advanced the source configuration to `1887` for the next build.

**Verification**

- Unity 6000.0.75f1 compiled without C# errors.
- `ValidateBeta5`, `ValidateReUI5Data`, and `ValidateAllEnabledScenesScopedSmoke` passed. The scoped smoke test opened 11 enabled scenes and 14 canvases.
- Android build succeeded in `00:08:14.6353640`.
- APK: `Builds/Android/ThreeBody-EventHorizon-Beta5.apk`
- APK size: `118,825,587` bytes
- SHA-256: `55110CD48D99F7A460460E241CC52AEA80CBD5A09855EBE88FC9239530A836CA`
- Package: `com.threebody.EventHorizon`; versionName: `Beta5`; versionCode: `140000`; minSdk: `23`; target/compileSdk: `34`; ABI: `arm64-v8a`.
- APK signature verifies with v1 and v2 using the Android Debug certificate. No Android device was connected, so this build was not installed or exercised on-device.
