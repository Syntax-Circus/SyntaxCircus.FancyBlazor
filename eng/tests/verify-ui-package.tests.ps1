[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$verifier = Join-Path $repositoryRoot "eng\verify-ui-package.ps1"
$scratchRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("fancyblazor-ui-verifier-tests-" + [Guid]::NewGuid().ToString("N"))

function New-TestPackage {
    param(
        [Parameter(Mandatory)] [string] $Directory,
        [hashtable] $Entries = @{}
    )

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $packagePath = Join-Path $Directory "SyntaxCircus.FancyBlazor.UI.0.3.0-preview.1.nupkg"
    $archive = [System.IO.Compression.ZipFile]::Open($packagePath, [System.IO.Compression.ZipArchiveMode]::Create)
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

function Get-CompleteShapeEntries {
    return @{
        "README.md" = "readme"
        "THIRD-PARTY-NOTICES.md" = "notice"
        "lib/net10.0/SyntaxCircus.FancyBlazor.UI.dll" = "assembly"
        "buildTransitive/SyntaxCircus.FancyBlazor.UI.props" = "<Project />"
    }
}

function Assert-VerifierRejects {
    param(
        [Parameter(Mandatory)] [string] $Name,
        [Parameter(Mandatory)] [hashtable] $Entries,
        [Parameter(Mandatory)] [string] $ExpectedMessage,
        [switch] $WithCorePackage
    )

    $caseDirectory = Join-Path $scratchRoot $Name
    New-Item -ItemType Directory -Path $caseDirectory | Out-Null
    New-TestPackage -Directory $caseDirectory -Entries $Entries
    if ($WithCorePackage) {
        New-Item -ItemType File -Path (Join-Path $caseDirectory "SyntaxCircus.FancyBlazor.0.3.0-preview.1.nupkg") | Out-Null
    }

    $rejected = $false
    try {
        $output = & $verifier -PackageDirectory $caseDirectory -CorePackageDirectory $caseDirectory
    }
    catch {
        $rejected = $true
        $output = $_
    }
    if (-not $rejected) {
        throw "$Name should be rejected by the UI package verifier."
    }

    $outputText = ($output | ForEach-Object {
        if ($_ -is [System.Management.Automation.ErrorRecord]) { $_.Exception.Message }
        else { $_.ToString() }
    }) -join [Environment]::NewLine
    if ($outputText -notmatch [regex]::Escape($ExpectedMessage)) {
        throw "$Name produced the wrong failure. Expected '$ExpectedMessage'; actual: $outputText"
    }
}

try {
    $nodeEntries = Get-CompleteShapeEntries
    $nodeEntries["node_modules/left-pad/index.js"] = "module.exports = () => {};"
    Assert-VerifierRejects -Name "node-artifact" -Entries $nodeEntries -ExpectedMessage "Node artifacts"

    $bootstrapEntries = Get-CompleteShapeEntries
    $bootstrapEntries["staticwebassets/vendor/bootstrap/bootstrap.min.css"] = "body{}"
    Assert-VerifierRejects -Name "bootstrap-asset" -Entries $bootstrapEntries -ExpectedMessage "Bootstrap asset"

    $missingReadmeEntries = Get-CompleteShapeEntries
    $missingReadmeEntries.Remove("README.md")
    Assert-VerifierRejects -Name "missing-readme" -Entries $missingReadmeEntries -ExpectedMessage "missing required entries: README.md"

    Assert-VerifierRejects -Name "missing-core-package" -Entries (Get-CompleteShapeEntries) -ExpectedMessage "matching core package"

    Write-Host "UI package verifier rejection cases passed."
}
finally {
    if (Test-Path -LiteralPath $scratchRoot) {
        Remove-Item -LiteralPath $scratchRoot -Recurse -Force
    }
}
