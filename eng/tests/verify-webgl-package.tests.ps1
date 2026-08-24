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

try {
    Assert-VerifierRejects -Name "node-artifact" -EntryName "node_modules/three/index.js" -EntryText "export {};" -ExpectedMessage "Node artifacts"
    Assert-VerifierRejects -Name "external-import" -EntryName "staticwebassets/js/fancy-blazor-webgl.js" -EntryText 'import "https://example.test/runtime.js";' -ExpectedMessage "external URL"
    Write-Host "WebGL package verifier rejection cases passed."
}
finally {
    if (Test-Path -LiteralPath $scratchRoot) {
        Remove-Item -LiteralPath $scratchRoot -Recurse -Force
    }
}
