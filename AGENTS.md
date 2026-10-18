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
- [Architecture](docs/architecture.md) - Dependency injection, game state machine, signals, data-driven design, and links to each subsystem's doc.
- [Database module](docs/database.md) - Schema, generated code, runtime loading, mod storage, migrations, tooling, and validation workflow.
- [Application layer](docs/application_layer.md) - `Assets/Scripts/`: composition root, game states, domain, UI, live vs. dead legacy code. Refactoring backlog: [issues](docs/issues/application_layer.md).
- [GitHub issue handling](.issues/README.md) - Required workflow for syncing, analyzing, and responding to GitHub issues. Store local issue notes in `.issues/` and never modify GitHub issues without explicit user approval.
- [GitHub pull request handling](.pull_requests/README.md) - Follow the active pull request sync workflow and local file format.

## Keeping this file small

This file loads into every session, so its size is a per-session tax. It is an
**index**, not a design record.

- New rationale — why a rule exists, the defect that motivated it, a worked example, a
  schema, a code trace — goes in the owning `docs/<component>.md`. Never here.
- This file may gain at most a **one-line** rule, and only when it spans components or
  would be violated by default. The line names the doc that owns the detail.
- If a bullet needs a second sentence, it belongs in a doc.
- Same rule for every nested `CLAUDE.md` / `AGENTS.md`: budget **80 lines**, pointing at
  its component doc.
- When a section outgrows the budget, **move** it into a doc — don't summarize in place.

## Task Management

- Check `TASKS.md` to understand project status.
- Before executing any work, mark the task `[In Progress]`. Except trivial tasks like
  investigation, writing documentation, bug fixes, etc. Do not mark such tasks in TASKS.md.
- When a task is complete, **delete its entry** — git remembers what was done.
  `TASKS.md` holds only open, in-progress and planned work. Any residue the finished
  work leaves behind (deferred steps, calibration, follow-ups) stays as its own `[ ]`.
- Don't attempt unlisted tasks without modifying `TASKS.md` first — except bug fixes,
  which are not added to the list.
- One task, one line. No detailed task descriptions.

## Tests

For small edits or bug fixes, do not write new test suites automatically. Only write targeted tests
if the touched file has an obvious nearby test file, or if explicitly requested. Prioritize cheap
static checks (linting, typechecking) over generating or running heavy test suites. Stop generating
tests once the specific user-defined acceptance criteria are met.

Engine-free modules (no `UnityEngine`) run their tests with `dotnet test Tests~/EngineFree.Tests.csproj`; rules in [docs/architecture.md](docs/architecture.md#engine-free-modules).

## Output style

You are my critical advisor, not my assistant. Never agree with me first; always challenge 
my core assumptions and look for flaws or logical inconsistencies before offering support.
Ban all filler phrases, sycophantic cheerleading, and empty praise (e.g., 'That's a great
point,' 'Absolutely'). Lead with the uncomfortable truth or the core problem immediately.
If I push back without providing new, valid evidence or arguments, hold your position instead
of capitulating.

No preamble. Tool result first. No explanation of actions. Stop.
Use plain English, no corporate jargon, and lead with the conclusion.
Give the answer in one line first. No caveats, background, or restating the point.

Don't start implementing anything unless you're explicitly asked to do so. Otherwise just answer my questions.

## How to explain things to me

When explaining code, a plan, an error, or changes, aim for about 80% adherence to ASD-STE100:

- Express one idea or action per sentence. Use no more than 20 words for instructions and 25 words for descriptions.
- Use active voice: say who does what.
- Use the same term for the same thing throughout. Define it once, then use it consistently.
- Explain each new technical term in simple words when it first appears.
- Give the answer first, then the details.
- Present steps in a numbered list. Keep each paragraph to one topic and no more than six sentences.
- Do not omit words just to make the text shorter.

## Git & Commit Guidelines

- NEVER create a git commit unless explicitly requested.
- NEVER create a new branch unless explicitly requested.
- Always present the file changes to the user and wait for manual approval before
  staging or committing code.

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
