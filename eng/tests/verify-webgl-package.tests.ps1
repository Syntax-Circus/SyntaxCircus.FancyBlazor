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
        [Parameter(Mandatory)] [string] $RendererText,
        [string] $CoreVendorText = "export {};",
        [string] $ExpectedCoreVendorText = "export {};",
        [string] $VendorText,
        [switch] $OmitReadme
    )

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $packagePath = Join-Path $Directory "SyntaxCircus.FancyBlazor.WebGL.0.2.1-preview.1.nupkg"
    $moduleVendorText = "export {};"
    $licenseText = "license"
    function Get-TextSha256 {
        param([Parameter(Mandatory)] [string] $Text)
        return [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($Text)))
    }
    $provenanceText = @"
# Three.js provenance

| Local file | Source | SHA-256 |
| --- | --- | --- |
| ``src/SyntaxCircus.FancyBlazor.WebGL/wwwroot/vendor/three/build/three.module.js`` | source | ``$(Get-TextSha256 -Text $moduleVendorText)`` |
| ``src/SyntaxCircus.FancyBlazor.WebGL/wwwroot/vendor/three/build/three.core.js`` | source | ``$(Get-TextSha256 -Text $ExpectedCoreVendorText)`` |
| ``src/SyntaxCircus.FancyBlazor.WebGL/wwwroot/vendor/three/LICENSE`` | source | ``$(Get-TextSha256 -Text $licenseText)`` |
"@
    $provenancePath = Join-Path $Directory "PROVENANCE.md"
    Set-Content -LiteralPath $provenancePath -Value $provenanceText -Encoding utf8 -NoNewline
    $entries = @{
        "THIRD-PARTY-NOTICES.md" = "notice"
        "README.md" = "readme"
        "licenses/three-LICENSE" = "license"
        "third-party/three/PROVENANCE.md" = $provenanceText
        "lib/net10.0/SyntaxCircus.FancyBlazor.WebGL.dll" = "assembly"
        "staticwebassets/js/fancy-blazor-webgl.js" = $AdapterText
        "staticwebassets/js/holographic-surface-renderer.js" = $RendererText
        "staticwebassets/js/wave-field-renderer.js" = ";"
        "staticwebassets/js/refractive-orb-renderer.js" = ";"
        "staticwebassets/js/prism-field-renderer.js" = ";"
        "staticwebassets/js/particle-field-renderer.js" = ";"
        "staticwebassets/vendor/three/LICENSE" = $licenseText
        "staticwebassets/vendor/three/build/three.core.js" = $CoreVendorText
        "staticwebassets/vendor/three/build/three.module.js" = $moduleVendorText
        "buildTransitive/SyntaxCircus.FancyBlazor.WebGL.props" = "<Project />"
    }
    if ($PSBoundParameters.ContainsKey("VendorText")) {
        $entries["staticwebassets/vendor/three/build/vendor-extension.js"] = $VendorText
    }
    if ($OmitReadme) {
        $entries.Remove("README.md")
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
    $rejected = $false
    try {
        $output = & $verifier -PackageDirectory $caseDirectory
    }
    catch {
        $rejected = $true
        $output = $_
    }
    if (-not $rejected) {
        throw "$Name should be rejected by the WebGL package verifier."
    }

    $outputText = ($output | ForEach-Object {
        if ($_ -is [System.Management.Automation.ErrorRecord]) { $_.Exception.Message }
        else { $_.ToString() }
    }) -join [Environment]::NewLine
    if ($outputText -notmatch [regex]::Escape($ExpectedMessage)) {
        throw "$Name produced the wrong failure. Expected '$ExpectedMessage'; actual: $outputText"
    }
}

function Assert-CompleteShapeRejection {
    param(
        [Parameter(Mandatory)] [string] $Name,
        [Parameter(Mandatory)] [string] $AdapterText,
        [Parameter(Mandatory)] [string] $RendererText,
        [string] $CoreVendorText = "export {};",
        [string] $ExpectedCoreVendorText = "export {};",
        [string] $VendorText,
        [switch] $OmitReadme,
        [Parameter(Mandatory)] [string] $ExpectedMessage
    )

    $caseDirectory = Join-Path $scratchRoot $Name
    New-Item -ItemType Directory -Path $caseDirectory | Out-Null
    $packageArguments = @{ Directory = $caseDirectory; AdapterText = $AdapterText; RendererText = $RendererText; CoreVendorText = $CoreVendorText; ExpectedCoreVendorText = $ExpectedCoreVendorText }
    if ($PSBoundParameters.ContainsKey("VendorText")) { $packageArguments.VendorText = $VendorText }
    if ($OmitReadme) { $packageArguments.OmitReadme = $true }
    New-CompleteShapePackage @packageArguments
    $rejected = $false
    try {
        $output = & $verifier -PackageDirectory $caseDirectory -CorePackageDirectory $caseDirectory -ProvenancePath (Join-Path $caseDirectory "PROVENANCE.md")
    }
    catch {
        $rejected = $true
        $output = $_
    }
    if (-not $rejected) {
        throw "$Name should be rejected by the WebGL package verifier."
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
    Assert-VerifierRejects -Name "node-artifact" -EntryName "node_modules/three/index.js" -EntryText "export {};" -ExpectedMessage "Node artifacts"
    Assert-VerifierRejects -Name "node-package-manifest" -EntryName "package.json" -EntryText '{ "name": "three" }' -ExpectedMessage "Node artifacts"
    foreach ($case in @(
        @{ Name = "static-import"; Text = 'import "https://example.test/runtime.js";' },
        @{ Name = "dynamic-import"; Text = 'import("https://example.test/runtime.js");' },
        @{ Name = "fetch"; Text = 'fetch/* comment */("https://example.test/runtime.js");' },
        @{ Name = "import-scripts"; Text = 'importScripts("https://example.test/runtime.js");' },
        @{ Name = "protocol-relative-variable"; Text = 'const runtime = "//example.test/runtime.js"; fetch(runtime);' }
    )) {
        Assert-VerifierRejects -Name $case.Name -EntryName "staticwebassets/js/fancy-blazor-webgl.js" -EntryText $case.Text -ExpectedMessage "external URL"
    }
    Assert-CompleteShapeRejection -Name "raw-budget" -AdapterText ("a" * 1048576) -RendererText ";" -ExpectedMessage "limit is below 1 MiB"
    Assert-CompleteShapeRejection -Name "vendor-external-url" -AdapterText "const local = 1;" -RendererText ";" -VendorText 'fetch("https://example.test/vendor.js");' -ExpectedMessage "external URL"
    Assert-CompleteShapeRejection -Name "missing-readme" -AdapterText "const local = 1;" -RendererText ";" -OmitReadme -ExpectedMessage "missing required entries: README.md"
    Assert-CompleteShapeRejection -Name "mutated-three-vendor" -AdapterText "const local = 1;" -RendererText ";" -CoreVendorText "export const changed = true;" -ExpectedMessage "provenance hash mismatch"
    Assert-CompleteShapeRejection -Name "ordinary-comment" -AdapterText "//todo" -RendererText ";" -ExpectedMessage "matching core package"
    Assert-CompleteShapeRejection -Name "non-executable-namespace-url" -AdapterText "const local = 1;" -RendererText ";" -VendorText "document.createElementNS('http://www.w3.org/1999/xhtml', 'canvas');" -ExpectedMessage "matching core package"
    Write-Host "WebGL package verifier rejection cases passed."
}
finally {
    if (Test-Path -LiteralPath $scratchRoot) {
        Remove-Item -LiteralPath $scratchRoot -Recurse -Force
    }
}
