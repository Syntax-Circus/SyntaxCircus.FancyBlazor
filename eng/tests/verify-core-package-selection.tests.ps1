[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$verifier = Join-Path $repositoryRoot "eng\verify-package.ps1"
$scratchRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("fancyblazor-core-selection-tests-" + [Guid]::NewGuid().ToString("N"))

function New-TestArchive {
    param(
        [Parameter(Mandatory)] [string] $Path,
        [hashtable] $Entries = @{}
    )

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [System.IO.Compression.ZipFile]::Open($Path, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($name in $Entries.Keys) {
            $entry = $archive.CreateEntry($name)
            $writer = [System.IO.StreamWriter]::new($entry.Open())
            try { $writer.Write($Entries[$name]) }
            finally { $writer.Dispose() }
        }
    }
    finally { $archive.Dispose() }
}

try {
    New-Item -ItemType Directory -Path $scratchRoot | Out-Null
    $webGlCase = Join-Path $scratchRoot "webgl-excluded"
    New-Item -ItemType Directory -Path $webGlCase | Out-Null
    $corePath = Join-Path $webGlCase "SyntaxCircus.FancyBlazor.0.3.0-preview.2.nupkg"
    $webGlPath = Join-Path $webGlCase "SyntaxCircus.FancyBlazor.WebGL.0.3.0-preview.2.nupkg"
    New-TestArchive -Path $corePath -Entries @{ "README.md" = "core readme" }
    New-TestArchive -Path $webGlPath
    [System.IO.File]::SetLastWriteTimeUtc($corePath, [DateTime]::UtcNow.AddMinutes(-1))
    [System.IO.File]::SetLastWriteTimeUtc($webGlPath, [DateTime]::UtcNow)

    $output = & pwsh -NoProfile -File $verifier -PackageDirectory $webGlCase 2>&1
    if ($LASTEXITCODE -eq 0) {
        throw "The incomplete core package should be rejected."
    }

    $outputText = $output | Out-String
    if ($outputText -match "missing required entries: README.md") {
        throw "The core verifier selected the WebGL companion instead of the core package. Actual: $outputText"
    }
    if ($outputText -notmatch "Package is missing required entries") {
        throw "The core verifier produced an unexpected failure. Actual: $outputText"
    }

    $uiCase = Join-Path $scratchRoot "ui-excluded"
    New-Item -ItemType Directory -Path $uiCase | Out-Null
    $uiCorePath = Join-Path $uiCase "SyntaxCircus.FancyBlazor.0.3.0-preview.2.nupkg"
    $uiPath = Join-Path $uiCase "SyntaxCircus.FancyBlazor.UI.0.3.0-preview.2.nupkg"
    New-TestArchive -Path $uiCorePath -Entries @{ "README.md" = "core readme" }
    New-TestArchive -Path $uiPath
    [System.IO.File]::SetLastWriteTimeUtc($uiCorePath, [DateTime]::UtcNow.AddMinutes(-1))
    [System.IO.File]::SetLastWriteTimeUtc($uiPath, [DateTime]::UtcNow)

    $uiOutput = & pwsh -NoProfile -File $verifier -PackageDirectory $uiCase 2>&1
    if ($LASTEXITCODE -eq 0) {
        throw "The incomplete core package should be rejected."
    }

    $uiOutputText = $uiOutput | Out-String
    if ($uiOutputText -match "missing required entries: README.md") {
        throw "The core verifier selected the UI companion instead of the core package. Actual: $uiOutputText"
    }
    if ($uiOutputText -notmatch "Package is missing required entries") {
        throw "The core verifier produced an unexpected failure. Actual: $uiOutputText"
    }

    Write-Host "Core package selection cases passed."
}
finally {
    if (Test-Path -LiteralPath $scratchRoot) {
        Remove-Item -LiteralPath $scratchRoot -Recurse -Force
    }
}
