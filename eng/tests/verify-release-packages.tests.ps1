[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$verifier = Join-Path $repositoryRoot "eng\verify-release-packages.ps1"
$scratchRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("fancyblazor-release-verifier-tests-" + [Guid]::NewGuid().ToString("N"))

if (-not (Test-Path -LiteralPath $verifier -PathType Leaf)) {
    throw "Release package verifier is missing."
}

function New-PackageFile {
    param(
        [Parameter(Mandatory)] [string] $Directory,
        [Parameter(Mandatory)] [string] $Name
    )

    New-Item -ItemType File -Path (Join-Path $Directory $Name) | Out-Null
}

function Assert-VerifierRejects {
    param(
        [Parameter(Mandatory)] [string] $Name,
        [Parameter(Mandatory)] [string[]] $Packages,
        [Parameter(Mandatory)] [string] $ExpectedMessage
    )

    $caseDirectory = Join-Path $scratchRoot $Name
    New-Item -ItemType Directory -Path $caseDirectory | Out-Null
    foreach ($package in $Packages) {
        New-PackageFile -Directory $caseDirectory -Name $package
    }

    $output = & pwsh -NoProfile -File $verifier -PackageDirectory $caseDirectory 2>&1
    if ($LASTEXITCODE -eq 0) {
        throw "$Name should be rejected by the release package verifier."
    }

    $outputText = ($output | ForEach-Object {
        if ($_ -is [System.Management.Automation.ErrorRecord]) { $_.Exception.Message }
        else { $_.ToString() }
    }) -join [Environment]::NewLine
    $outputText = ($outputText -replace '\s*\|\s*', ' ') -replace '\s+', ' '
    $expectedPattern = (($ExpectedMessage -split ' ') | ForEach-Object { [regex]::Escape($_) }) -join '\W+'
    if ($outputText -notmatch $expectedPattern) {
        throw "$Name produced the wrong failure. Expected '$ExpectedMessage'; actual: $outputText"
    }
}

try {
    $matchingDirectory = Join-Path $scratchRoot "matching"
    New-Item -ItemType Directory -Path $matchingDirectory | Out-Null
    New-PackageFile -Directory $matchingDirectory -Name "SyntaxCircus.FancyBlazor.0.3.0-preview.2.nupkg"
    New-PackageFile -Directory $matchingDirectory -Name "SyntaxCircus.FancyBlazor.WebGL.0.3.0-preview.2.nupkg"
    New-PackageFile -Directory $matchingDirectory -Name "SyntaxCircus.FancyBlazor.0.3.0-preview.2.snupkg"
    New-PackageFile -Directory $matchingDirectory -Name "SyntaxCircus.FancyBlazor.WebGL.0.3.0-preview.2.snupkg"

    $matchingOutput = & pwsh -NoProfile -File $verifier -PackageDirectory $matchingDirectory 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Matching release packages should pass. Actual: $($matchingOutput | Out-String)"
    }
    if (($matchingOutput | Out-String) -notmatch [regex]::Escape("0.3.0-preview.2")) {
        throw "Matching release output should identify the shared package version."
    }

    Assert-VerifierRejects -Name "missing-webgl" `
        -Packages @("SyntaxCircus.FancyBlazor.0.3.0.nupkg") `
        -ExpectedMessage "exactly one SyntaxCircus.FancyBlazor.WebGL package"

    Assert-VerifierRejects -Name "mismatched-version" `
        -Packages @(
            "SyntaxCircus.FancyBlazor.0.3.0.nupkg",
            "SyntaxCircus.FancyBlazor.WebGL.0.3.1.nupkg"
        ) `
        -ExpectedMessage "must use the same version"

    Assert-VerifierRejects -Name "unexpected-package" `
        -Packages @(
            "SyntaxCircus.FancyBlazor.0.3.0.nupkg",
            "SyntaxCircus.FancyBlazor.WebGL.0.3.0.nupkg",
            "Unexpected.Package.0.3.0.nupkg"
        ) `
        -ExpectedMessage "unexpected NuGet packages"

    Write-Host "Release package same-version cases passed."
}
finally {
    if (Test-Path -LiteralPath $scratchRoot) {
        Remove-Item -LiteralPath $scratchRoot -Recurse -Force
    }
}
