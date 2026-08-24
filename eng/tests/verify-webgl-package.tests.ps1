[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$verifier = Join-Path $repositoryRoot "eng\verify-webgl-package.ps1"
$scratchRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("fancyblazor-webgl-verifier-tests-" + [Guid]::NewGuid().ToString("N"))

function New-TestPackage {
    param(
        [Parameter(Mandatory)] [string] $Directory,
        [Parameter(Mandatory)] [string] $EntryName,
        [Parameter(Mandatory)] [string] $EntryText
    )

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $packagePath = Join-Path $Directory "SyntaxCircus.FancyBlazor.WebGL.0.2.1-preview.1.nupkg"
    $archive = [System.IO.Compression.ZipFile]::Open($packagePath, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        $entry = $archive.CreateEntry($EntryName)
        $writer = [System.IO.StreamWriter]::new($entry.Open())
        try { $writer.Write($EntryText) }
        finally { $writer.Dispose() }
    }
    finally { $archive.Dispose() }
}

function New-CompleteShapePackage {
    param(
        [Parameter(Mandatory)] [string] $Directory,
        [Parameter(Mandatory)] [string] $AdapterText,
        [Parameter(Mandatory)] [string] $RendererText
    )

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $packagePath = Join-Path $Directory "SyntaxCircus.FancyBlazor.WebGL.0.2.1-preview.1.nupkg"
    $entries = @{
        "THIRD-PARTY-NOTICES.md" = "notice"
        "licenses/three-LICENSE" = "license"
        "third-party/three/PROVENANCE.md" = "provenance"
        "lib/net10.0/SyntaxCircus.FancyBlazor.WebGL.dll" = "assembly"
        "staticwebassets/js/fancy-blazor-webgl.js" = $AdapterText
        "staticwebassets/js/holographic-surface-renderer.js" = $RendererText
        "staticwebassets/vendor/three/LICENSE" = "license"
        "staticwebassets/vendor/three/build/three.core.js" = "export {};"
        "staticwebassets/vendor/three/build/three.module.js" = "export {};"
        "buildTransitive/SyntaxCircus.FancyBlazor.WebGL.props" = "<Project />"
    }
    $archive = [System.IO.Compression.ZipFile]::Open($packagePath, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($name in $entries.Keys) {
            $entry = $archive.CreateEntry($name)
            $writer = [System.IO.StreamWriter]::new($entry.Open())
            try { $writer.Write($entries[$name]) }
            finally { $writer.Dispose() }
        }
    }
    finally { $archive.Dispose() }
}

function Assert-VerifierRejects {
    param(
        [Parameter(Mandatory)] [string] $Name,
        [Parameter(Mandatory)] [string] $EntryName,
        [Parameter(Mandatory)] [string] $EntryText,
        [Parameter(Mandatory)] [string] $ExpectedMessage
    )

    $caseDirectory = Join-Path $scratchRoot $Name
    New-Item -ItemType Directory -Path $caseDirectory | Out-Null
    New-TestPackage -Directory $caseDirectory -EntryName $EntryName -EntryText $EntryText
    $output = & pwsh -NoProfile -File $verifier -PackageDirectory $caseDirectory 2>&1
    if ($LASTEXITCODE -eq 0) {
        throw "$Name should be rejected by the WebGL package verifier."
    }

    $outputText = $output | Out-String
    if ($outputText -notmatch [regex]::Escape($ExpectedMessage)) {
        throw "$Name produced the wrong failure. Expected '$ExpectedMessage'; actual: $outputText"
    }
}

function Assert-CompleteShapeRejection {
    param(
        [Parameter(Mandatory)] [string] $Name,
        [Parameter(Mandatory)] [string] $AdapterText,
        [Parameter(Mandatory)] [string] $RendererText,
        [Parameter(Mandatory)] [string] $ExpectedMessage
    )

    $caseDirectory = Join-Path $scratchRoot $Name
    New-Item -ItemType Directory -Path $caseDirectory | Out-Null
    New-CompleteShapePackage -Directory $caseDirectory -AdapterText $AdapterText -RendererText $RendererText
    $output = & pwsh -NoProfile -File $verifier -PackageDirectory $caseDirectory -CorePackageDirectory $caseDirectory 2>&1
    if ($LASTEXITCODE -eq 0) {
        throw "$Name should be rejected by the WebGL package verifier."
    }

    $outputText = $output | Out-String
    if ($outputText -notmatch [regex]::Escape($ExpectedMessage)) {
        throw "$Name produced the wrong failure. Expected '$ExpectedMessage'; actual: $outputText"
    }
}

try {
    Assert-VerifierRejects -Name "node-artifact" -EntryName "node_modules/three/index.js" -EntryText "export {};" -ExpectedMessage "Node artifacts"
    Assert-VerifierRejects -Name "node-package-manifest" -EntryName "package.json" -EntryText '{ "name": "three" }' -ExpectedMessage "Node artifacts"
    foreach ($case in @(
        @{ Name = "static-import"; Text = 'import "https://example.test/runtime.js";' },
        @{ Name = "dynamic-import"; Text = 'import("https://example.test/runtime.js");' },
        @{ Name = "fetch"; Text = 'fetch("https://example.test/runtime.js");' },
        @{ Name = "import-scripts"; Text = 'importScripts("https://example.test/runtime.js");' },
        @{ Name = "protocol-relative"; Text = 'const runtime = "//example.test/runtime.js";' }
    )) {
        Assert-VerifierRejects -Name $case.Name -EntryName "staticwebassets/js/fancy-blazor-webgl.js" -EntryText $case.Text -ExpectedMessage "external URL"
    }
    Assert-CompleteShapeRejection -Name "raw-budget" -AdapterText ("a" * 1048576) -RendererText ";" -ExpectedMessage "limit is below 1 MiB"
    Write-Host "WebGL package verifier rejection cases passed."
}
finally {
    if (Test-Path -LiteralPath $scratchRoot) {
        Remove-Item -LiteralPath $scratchRoot -Recurse -Force
    }
}
