param(
    [Parameter(Position = 0)]
    [string]$Version,
    [switch]$Help
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ($Help) {
    Write-Host 'Usage: build-database-editor.cmd [version]'
    Write-Host 'Builds the editor and creates Builds/database_editor_<version>.zip.'
    Write-Host 'The default version is bundleVersion in ProjectSettings/ProjectSettings.asset.'
    exit 0
}

$buildLock = $null
$workDirectory = $null
$exitCode = 0
try {
    $projectRoot = Split-Path -Parent $PSScriptRoot
    $databaseRoot = Join-Path $projectRoot 'Assets/Modules/Database'
    $editorRoot = Join-Path $databaseRoot '.Editor'
    if (-not $Version) {
        $settings = Get-Content -LiteralPath (Join-Path $projectRoot 'ProjectSettings/ProjectSettings.asset') -Raw
        $match = [regex]::Match($settings, '(?m)^\s*bundleVersion:\s*(.+?)\s*$')
        if (-not $match.Success) { throw 'Cannot read bundleVersion. Supply the version as an argument.' }
        $Version = $match.Groups[1].Value.Trim().Trim('"').Trim("'")
    }
    if ($Version -notmatch '^\d[A-Za-z0-9._-]*$') { throw 'Version must start with a digit and contain only letters, digits, dots, underscores, or hyphens.' }

    $dotnet = (Get-Command dotnet -CommandType Application -ErrorAction Stop).Source
    $generator = Join-Path $databaseRoot '.CodeGenerator/DatabaseCodeGenerator/bin/Release/DatabaseCodeGenerator.exe'
    if (-not (Test-Path -LiteralPath $generator -PathType Leaf)) { throw "Build the database code generator first: $generator" }
    $solution = Join-Path $editorRoot 'GameDatabase.sln'
    $editorProject = Join-Path $editorRoot 'GameDatabase/DatabaseEditor.csproj'
    [xml]$project = Get-Content -LiteralPath $editorProject -Raw
    $framework = $project.SelectSingleNode('//TargetFramework').InnerText
    $binaries = Join-Path $editorRoot "GameDatabase/bin/Release/$framework"
    $database = Join-Path $databaseRoot 'Resources/Database'
    if (-not (Test-Path -LiteralPath $database -PathType Container)) { throw "Database folder missing: $database" }

    $buildLock = [IO.File]::Open((Join-Path $PSScriptRoot '.database-editor.lock'), 'OpenOrCreate', 'ReadWrite', 'None')
    $logDirectory = Join-Path $PSScriptRoot ('Logs/database-editor-' + (Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
    New-Item -ItemType Directory -Path $logDirectory | Out-Null
    $generatorLog = Join-Path $logDirectory 'generate.log'
    $buildLog = Join-Path $logDirectory 'build.log'

    Write-Host "Generating editor models. Log: $generatorLog"
    Push-Location -LiteralPath $databaseRoot
    try {
        & $generator '.Schema/versions.xml' '.Editor/DatabaseModel/DatabaseModel' editor upgrade_editor > $generatorLog 2>&1
        if ($LASTEXITCODE -ne 0) { throw "Database code generation failed. See $generatorLog" }
        # This generator catches exceptions without setting an exit code. Only its known
        # progress messages count as success; errors and unexpected output stop packaging.
        $unexpected = @(Get-Content -LiteralPath $generatorLog | Where-Object {
            $_.Trim() -and $_ -notmatch '^(Processing file |File updated: |New file: |Deleting old file: )'
        })
        if ($unexpected.Count) { throw "Database code generation reported an error or unexpected output. See $generatorLog" }

        Write-Host "Building database editor (Release). Log: $buildLog"
        & $dotnet build $solution --configuration Release --nologo --verbosity minimal > $buildLog 2>&1
        if ($LASTEXITCODE -ne 0) { throw "Database editor build failed. See $buildLog" }
    } finally { Pop-Location }

    $workDirectory = Join-Path $PSScriptRoot ('.database-editor-' + [Guid]::NewGuid().ToString('N'))
    $package = Join-Path $workDirectory 'package'
    $packageBin = Join-Path $package 'bin'
    $packageDatabase = Join-Path $package 'Database'
    New-Item -ItemType Directory -Path $packageBin, $packageDatabase -Force | Out-Null
    # Match Release/UpdateFiles.cmd without deleting or reusing its local package folder.
    foreach ($file in @(
        'BouncyCastle.Cryptography.dll', 'DatabaseEditor.dll', 'DatabaseEditor.exe',
        'DatabaseModel.dll', 'Cyotek.Windows.Forms.ColorPicker.dll',
        'DatabaseEditor.deps.json', 'DatabaseEditor.runtimeconfig.json',
        'IonicZlib.dll', 'Newtonsoft.Json.dll'
    )) {
        Copy-Item -LiteralPath (Join-Path $binaries $file) -Destination $packageBin
    }
    $jsonFiles = @(Get-ChildItem -LiteralPath $database -Recurse -File -Filter '*.json')
    if ($jsonFiles.Count -eq 0) { throw 'The source database contains no JSON files.' }
    foreach ($file in $jsonFiles) {
        $relativePath = $file.FullName.Substring($database.Length + 1)
        $destination = Join-Path $packageDatabase $relativePath
        New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
        Copy-Item -LiteralPath $file.FullName -Destination $destination
    }
    Copy-Item -LiteralPath (Join-Path $editorRoot 'Release/editor.cmd') -Destination $package

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $temporaryZip = Join-Path $workDirectory 'package.zip'
    [IO.Compression.ZipFile]::CreateFromDirectory($package, $temporaryZip, [IO.Compression.CompressionLevel]::Optimal, $false)
    $archivePath = Join-Path $PSScriptRoot "database_editor_$Version.zip"
    Move-Item -LiteralPath $temporaryZip -Destination $archivePath -Force
    Write-Host "Created $archivePath ($($jsonFiles.Count) database files)."
} catch {
    [Console]::Error.WriteLine($_.Exception.Message)
    $exitCode = 1
} finally {
    if ($null -ne $workDirectory -and (Test-Path -LiteralPath $workDirectory)) {
        # Only recursively delete this run's staging directory inside Builds.
        $resolvedWork = (Resolve-Path -LiteralPath $workDirectory).Path
        $resolvedBuilds = (Resolve-Path -LiteralPath $PSScriptRoot).Path.TrimEnd('\') + '\'
        if ($resolvedWork.StartsWith($resolvedBuilds, [StringComparison]::OrdinalIgnoreCase) -and
            (Split-Path -Leaf $resolvedWork) -match '^\.database-editor-[a-f0-9]{32}$') {
            Remove-Item -LiteralPath $resolvedWork -Recurse -Force
        } else { throw "Refusing to remove unexpected staging directory: $resolvedWork" }
    }
    if ($null -ne $buildLock) { $buildLock.Dispose() }
}
exit $exitCode
