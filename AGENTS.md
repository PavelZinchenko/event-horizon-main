## Project Overview

Event Horizon is a Unity-based 2D space exploration and combat game with modular architecture. The project uses:
- Unity 6000.0.75f1 with C# .NET Standard 2.1
- Zenject for dependency injection
- Custom modular architecture with separate assemblies
- Data-driven design with JSON-based configuration
- Multi-platform support (Android, iOS, PC, Web)

## Documentation

- [Game design](docs/game.md) - Product vision, design pillars, core gameplay loops, and foundational mechanics.
- [Project structure](docs/project_structure.md) - High-level map of the repository and guidance on where new work belongs.
- [Database module](docs/database.md) - Schema, generated code, runtime loading, mod storage, migrations, tooling, and validation workflow.
- [GitHub issue handling](.issues/README.md) - Required workflow for syncing, analyzing, and responding to GitHub issues. Store local issue notes in `.issues/` and never modify GitHub issues without explicit user approval.

## Development Commands

### Database Code Generation
```bash
# Generate database code from schemas
Assets\Modules\Database\generate_code.cmd

# Generate migration code for database versioning
Assets\Modules\Database\generate_migration_code.cmd

# Rebuild database editor tools
Assets\Modules\Database\rebuild_editor.cmd

# Analyze database for issues
Assets\Modules\Database\analyzer.cmd
```

### GameSession Code Generation
```bash
# Generate session serialization code
Assets\Modules\GameSession\generate_code.cmd
```

### Testing
```bash
# Run database code generator tests
Assets\Modules\Database\.CodeGenerator\test.cmd
```

### Build Commands
The project uses Unity's build system. Build configurations are managed through Unity Editor's Build Settings.

## Architecture Overview

### Module System
The project is organized into autonomous modules using Unity Assembly Definitions:

**Core Modules (Assets/Modules/):**
- `Database` - Game data management, JSON serialization, code generation
- `GameSession` - Save/load system, session state management
- `BattleSimulator` - Combat mechanics and physics simulation
- `ShipConstructor` - Ship building and component system
- `Quests` - Quest system and narrative framework
- `CommonComponents` - Shared utilities and data types
- `AppConfiguration` - Application settings and configuration

**Shared Modules (Assets/ModulesShared/):**
- `Gui` - UI framework with theming and localization
- `Localization` - Multi-language support system
- `Diagnostics` - Logging and debugging utilities
- `ShipEditor` - Visual ship design tools
- `Utilites` - Common algorithms and data structures

**Private Modules (Assets/ModulesPrivate/):**
- `Security` - Encryption and anti-tampering
- `Services` - Platform-specific integrations
- `Plugins` - Third-party library wrappers

### Key Architecture Patterns

**Dependency Injection (Zenject):**
- `GameInstaller.cs` - Main game container setup
- Scene-specific installers for different game modes
- Interface-based service registration

**Data-Driven Design:**
- JSON configuration files in `Assets/Modules/Database/Resources/`
- Code generation from XML schemas for type safety
- Mod support through database replacement

**State Machine:**
- `GameStateMachine` manages game flow
- States: MainMenu, StarMap, Combat, Exploration, etc.
- Located in `Assets/Scripts/GameStateMachine/`

**Event System:**
- Signal-based communication between modules
- `CommonComponents.Signals` for decoupled messaging
- Quest system uses events for progression tracking

### Database System
- **Schema-driven**: XML schemas define data structure
- **Code generation**: `DatabaseCodeGenerator.exe` creates C# classes
- **Modding support**: Database can be replaced via file system
- **Versioning**: Migration system handles database updates
- **Storage backends**: Resources, Files, or Folders

### Combat System
- **Physics-based**: Unity 2D physics for ship movement
- **Component-driven**: Ships built from modular components
- **AI system**: Behavior trees for enemy ships
- **Real-time simulation**: 60fps combat with pause/resume

### Save System
- **Session-based**: `GameSession` module handles persistence
- **Serialization**: Custom binary format with compression
- **Versioning**: Migration system for save compatibility
- **Platform-agnostic**: Works across all supported platforms

### Localization
- **XML-based**: Translation files in `Assets/ModulesShared/Localization/Resources/`
- **Font management**: `FontTable` for language-specific fonts
- **Runtime switching**: Language can be changed without restart
- **Plural forms**: Grammar rules for different languages

## Important File Locations

- **Main game logic**: `Assets/Scripts/`
- **Module assemblies**: `Assets/Modules/*/Scripts/`
- **Database schemas**: `Assets/Modules/Database/.Schema/`
- **Game scenes**: `Assets/Scenes/`
- **Localization**: `Assets/ModulesShared/Localization/Resources/`
- **Resources**: `Assets/Resources/` and module-specific resource folders
- **Build outputs**: `Builds/` (excluded from git)

## Development Workflow

1. **Database changes**: Modify JSON files → run `generate_code.cmd` → rebuild
2. **New features**: Create in appropriate module → update assembly references
3. **UI changes**: Use GUI module framework → update themes if needed
4. **Localization**: Add keys to XML files → test with different languages
5. **Testing**: Use Unity Test Runner for unit tests

## Code Generation Dependencies

The database and session systems rely on custom code generators:
- `DatabaseCodeGenerator.exe` - Generates C# classes from XML schemas
- Session serialization code generation for save/load system
- Always run generators after schema changes

## MCP

Always use context7 when I need code generation, setup or configuration steps, or
library/API documentation. This means you should automatically use the Context7 MCP
tools to resolve library id and get library docs without me having to explicitly ask.

For all remote GitHub operations, including reading or modifying repositories,
issues, pull requests, comments, labels, and other GitHub metadata, use the
GitHub integration plugin when possible. Local Git commands may still be used for worktree
inspection and verification.

## Platform Considerations

- **Android**: Uses Google Play Services, UnityAds
- **iOS**: Keychain integration for secure storage
- **Steam**: Steamworks.NET integration for PC features
- **WebGL**: Limited storage and threading capabilities
