param(
    [switch]$Aab,
    [switch]$Help,
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$Targets
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ($Help) {
    Write-Host 'Usage: build.cmd <android|windows|linux|mac|ios> [more targets] [-Aab]'
    Write-Host 'Android defaults to APK. macOS uses Mono; all other targets use IL2CPP.'
    Write-Host 'iOS exports an Xcode project; compile and sign it on macOS.'
    Write-Host 'Configure Unity and signing in build.local.ps1 or environment variables. See docs/build.md.'
    exit 0
}

$buildLock = $null
try {
    if (-not $Targets -or $Targets.Count -eq 0) { throw 'Specify android, windows, linux, mac, or ios. Use -Help for usage.' }
    $Targets = @($Targets | ForEach-Object {
        $target = $_.ToLowerInvariant()
        if ($target -notin @('android', 'windows', 'linux', 'mac', 'ios')) { throw "Unknown target: $_" }
        $target
    } | Select-Object -Unique)
    if ($Aab -and 'android' -notin $Targets) { throw '-Aab requires the android target.' }

    $projectPath = Split-Path -Parent $PSScriptRoot
    $localSettings = Join-Path $PSScriptRoot 'build.local.ps1'
    if (Test-Path -LiteralPath $localSettings) { . $localSettings }

    # Serialize the entire run, including gaps between Unity processes.
    $buildLock = [IO.File]::Open((Join-Path $PSScriptRoot '.build.lock'), 'OpenOrCreate', 'ReadWrite', 'None')
    $editorLockPath = Join-Path $projectPath 'Temp/UnityLockfile'
    if (Test-Path -LiteralPath $editorLockPath) {
        try {
            $editorLock = [IO.File]::Open($editorLockPath, 'Open', 'ReadWrite', 'None')
            $editorLock.Dispose()
        } catch { throw 'Close this project in the Unity Editor before running a batch build.' }
    }

    $versionText = Get-Content -LiteralPath (Join-Path $projectPath 'ProjectSettings/ProjectVersion.txt') -Raw
    $versionMatch = [regex]::Match($versionText, '(?m)^m_EditorVersion: (\S+)')
    if (-not $versionMatch.Success) { throw 'Cannot read the project Unity version.' }
    $version = $versionMatch.Groups[1].Value
    $unityEditor = $env:UNITY_EDITOR
    if (-not $unityEditor) {
        $candidates = @("$env:ProgramFiles/Unity/Hub/Editor/$version/Editor/Unity.exe")
        # Unity Hub permits a custom installation root.
        $hubSettings = Join-Path $env:APPDATA 'UnityHub/secondaryInstallPath.json'
        if (Test-Path -LiteralPath $hubSettings) {
            $hubRoot = Get-Content -LiteralPath $hubSettings -Raw | ConvertFrom-Json
            if ($hubRoot -is [string]) { $candidates += Join-Path $hubRoot "$version/Editor/Unity.exe" }
        }
        $unityEditor = $candidates | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1
    }
    if (-not $unityEditor -or -not (Test-Path -LiteralPath $unityEditor -PathType Leaf)) {
        throw "Set UNITY_EDITOR to the full path of Unity $version Editor/Unity.exe (see docs/build.md)."
    }
    $unityEditor = (Resolve-Path -LiteralPath $unityEditor).Path

    if ('android' -in $Targets) {
        foreach ($name in @('EH_ANDROID_KEYSTORE_PASSWORD', 'EH_ANDROID_KEY_PASSWORD')) {
            if (-not [Environment]::GetEnvironmentVariable($name)) { throw "Set $name in build.local.ps1 or the environment." }
        }
    }

    $runPath = Join-Path $PSScriptRoot ('Logs/' + (Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
    New-Item -ItemType Directory -Path $runPath -Force | Out-Null
    $receipt = Join-Path $runPath 'build-number.txt'
    $buildNumber = $null
    $failed = @()

    function Invoke-BuildUnity([string]$Target, [string]$Method, [string]$Stage, [string[]]$ExtraArgs) {
        $unityTarget = @{
            android = 'Android'
            windows = 'Win64'
            linux = 'Linux64'
            mac = 'OSXUniversal'
            ios = 'iOS'
        }[$Target]
        $logPath = Join-Path $runPath "$Target-$Stage.log"
        $arguments = @('-batchmode', '-quit', '-projectPath', $projectPath,
            '-buildTarget', $unityTarget, '-executeMethod', "EditorWindows.CommandLineBuild.$Method",
            '-logFile', $logPath, '-ehPlatform', $Target, '-ehTargets', ($Targets -join ','),
            '-ehReceipt', $receipt, '-ehUnityVersion', $version) + $ExtraArgs
        # All values are internal flags or Windows paths (which cannot contain a quote).
        $quotedArguments = ($arguments | ForEach-Object { '"' + $_ + '"' }) -join ' '
        Write-Host "$Target / $Stage - log: $logPath"
        $process = Start-Process -FilePath $unityEditor -ArgumentList $quotedArguments -WorkingDirectory $projectPath -Wait -PassThru -WindowStyle Hidden
        if ($process.ExitCode -ne 0) { throw "Unity exited with code $($process.ExitCode). See $logPath" }
    }

    function New-WindowsArchive {
        $gameVersion = (Get-Content -LiteralPath (Join-Path $runPath 'windows-version.txt') -Raw).Trim()
        if ($gameVersion -notmatch '^\d[A-Za-z0-9._-]*$') { throw 'Invalid game version for the Windows ZIP filename.' }
        $windowsPath = Join-Path $PSScriptRoot 'Windows'
        if (-not (Test-Path -LiteralPath (Join-Path $windowsPath 'eventhorizon.exe') -PathType Leaf) -or
            -not (Test-Path -LiteralPath (Join-Path $windowsPath 'eventhorizon_Data') -PathType Container)) {
            throw 'Windows player output is incomplete; cannot create the ZIP.'
        }
        $archivePath = Join-Path $PSScriptRoot "eventhorizon_win_$gameVersion.zip"
        $temporaryZip = Join-Path $runPath 'windows.zip'
        Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
        Write-Host "Packaging Windows build: $archivePath"
        try {
            $archive = [IO.Compression.ZipFile]::Open($temporaryZip, [IO.Compression.ZipArchiveMode]::Create)
            try {
                foreach ($file in Get-ChildItem -LiteralPath $windowsPath -Recurse -File -Force) {
                    $entryName = $file.FullName.Substring($windowsPath.Length + 1).Replace('\', '/')
                    if ($entryName -match '(^|/)[^/]*_BackUpThisFolder_ButDontShipItWithYourGame/') { continue }
                    [IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
                        $archive, $file.FullName, $entryName, [IO.Compression.CompressionLevel]::Optimal) | Out-Null
                }
            } finally { $archive.Dispose() }
            # Replace an existing release ZIP only after compression succeeds.
            Move-Item -LiteralPath $temporaryZip -Destination $archivePath -Force
        } finally {
            if (Test-Path -LiteralPath $temporaryZip) { Remove-Item -LiteralPath $temporaryZip -Force }
        }
        Write-Host "Created $archivePath"
    }

    foreach ($target in $Targets) {
        try {
            $prepareArgs = if ($null -eq $buildNumber) { @('-ehIncrementBuildNumber') } else { @('-ehBuildNumber', "$buildNumber") }
            Invoke-BuildUnity $target 'Prepare' 'prepare' $prepareArgs
            $numberText = (Get-Content -LiteralPath $receipt -Raw).Trim()
            $parsedNumber = 0
            if (-not [int]::TryParse($numberText, [ref]$parsedNumber) -or $parsedNumber -lt 1) { throw 'Invalid build-number receipt.' }
            if ($null -ne $buildNumber -and $buildNumber -ne $parsedNumber) { throw 'Build number changed during this run.' }
            $buildNumber = $parsedNumber
            $buildArgs = @('-ehBuildNumber', "$buildNumber")
            if ($Aab) { $buildArgs += '-ehAab' }
            Invoke-BuildUnity $target 'Build' 'build' $buildArgs
            if ($target -eq 'windows') { New-WindowsArchive }
            Write-Host "$target succeeded (build $buildNumber)."
        } catch {
            Write-Warning "${target}: $($_.Exception.Message)"
            $failed += $target
            # Preparation might have reserved a number before Unity failed. Do not reserve another.
            if ($null -eq $buildNumber) { break }
        }
    }
    if ($failed.Count -gt 0) { throw "Build failed: $($failed -join ', '). Logs: $runPath" }
    Write-Host "Completed $($Targets -join ', ') with build number $buildNumber."
} catch {
    [Console]::Error.WriteLine($_.Exception.Message)
    exit 1
} finally {
    if ($null -ne $buildLock) { $buildLock.Dispose() }
}
