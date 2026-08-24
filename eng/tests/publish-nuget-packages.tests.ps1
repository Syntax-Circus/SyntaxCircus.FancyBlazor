[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$publisher = Join-Path $repositoryRoot "eng\publish-nuget-packages.ps1"
$scratchRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("fancyblazor-publisher-tests-" + [Guid]::NewGuid().ToString("N"))

if (-not (Test-Path -LiteralPath $publisher -PathType Leaf)) {
    throw "NuGet package publisher is missing."
}

try {
    New-Item -ItemType Directory -Path $scratchRoot | Out-Null
    New-Item -ItemType File -Path (Join-Path $scratchRoot "SyntaxCircus.FancyBlazor.0.3.0.nupkg") | Out-Null
    New-Item -ItemType File -Path (Join-Path $scratchRoot "SyntaxCircus.FancyBlazor.WebGL.0.3.0.nupkg") | Out-Null
    New-Item -ItemType File -Path (Join-Path $scratchRoot "SyntaxCircus.FancyBlazor.0.3.0.snupkg") | Out-Null
    New-Item -ItemType File -Path (Join-Path $scratchRoot "notes.txt") | Out-Null

    $output = & pwsh -NoProfile -File $publisher `
        -PackageDirectory $scratchRoot `
        -ApiKey "test-key" `
        -Source "https://example.test/v3/index.json" `
        -WhatIf 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Publisher dry run should pass. Actual: $($output | Out-String)"
    }

    $outputText = $output | Out-String
    foreach ($packageName in @(
        "SyntaxCircus.FancyBlazor.0.3.0.nupkg",
        "SyntaxCircus.FancyBlazor.WebGL.0.3.0.nupkg"
    )) {
        if ($outputText -notmatch [regex]::Escape($packageName)) {
            throw "Publisher dry run omitted $packageName. Actual: $outputText"
        }
    }
    if ($outputText -match [regex]::Escape("SyntaxCircus.FancyBlazor.0.3.0.snupkg")) {
        throw "Publisher dry run should not push symbol packages directly. Actual: $outputText"
    }

    Write-Host "NuGet package publisher selection case passed."
}
finally {
    if (Test-Path -LiteralPath $scratchRoot) {
        Remove-Item -LiteralPath $scratchRoot -Recurse -Force
    }
}
