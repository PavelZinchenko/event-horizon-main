# Project Tasks

## Active / Open (database editor — `docs/database/editor.md`)
- [ ] Port `Migrate_3_4` (component-mod replacement from `Resources/DatabaseMigration/v1.3`) to `.Editor/DatabaseModel/DatabaseModel/Migration/v1/`
- [ ] `rebuild_editor.cmd`: replace the hard-coded `MSBuild.exe` path (vswhere or `dotnet build`)

## Active / Open (code generator — `docs/database/code_generator.md`)
- [ ] `DatabaseCodeGenerator` returns a non-zero exit code on schema or generation errors
- [ ] Fix `StringExtensions.ContainsKey` word-boundary check (indexes `key` instead of `source`, inverted condition)
- [ ] Emit enum `default` values as `Enum.Value`, not a quoted string, in game serializable DTOs
- [ ] Parse expression `minvalue`/`maxvalue` as floats for `float` results

## Active / Open (engine-free modules — `TODO/engine_free_modules.md`)
- [In Progress] Make Database and GameSession engine-free (7 phases)
