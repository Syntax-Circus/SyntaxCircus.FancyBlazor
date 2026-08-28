[CmdletBinding()]
param(
    [string] $PackageDirectory = (Join-Path $PSScriptRoot "..\artifacts"),
    [string] $CorePackageDirectory,
    [string] $PackageVersion,
    [string] $ProvenancePath = (Join-Path $PSScriptRoot "..\third-party\three\PROVENANCE.md")
)

$ErrorActionPreference = "Stop"
$packageRoot = [System.IO.Path]::GetFullPath($PackageDirectory)
if (-not (Test-Path -LiteralPath $packageRoot -PathType Container)) { throw "Package directory does not exist: $packageRoot" }
if (-not $CorePackageDirectory) { $CorePackageDirectory = Join-Path $packageRoot ".." }
$corePackageRoot = [System.IO.Path]::GetFullPath($CorePackageDirectory)
if (-not (Test-Path -LiteralPath $corePackageRoot -PathType Container)) { throw "Core package directory does not exist: $corePackageRoot" }

$packages = Get-ChildItem -LiteralPath $packageRoot -Filter "SyntaxCircus.FancyBlazor.WebGL.*.nupkg" | Where-Object { $_.Name -notlike "*.snupkg" } | Sort-Object LastWriteTimeUtc -Descending
if (-not $packages) { throw "No SyntaxCircus.FancyBlazor.WebGL package was found in $packageRoot." }
$package = $packages[0]
if (-not $PackageVersion) {
    $PackageVersion = $package.Name.Substring("SyntaxCircus.FancyBlazor.WebGL.".Length)
    $PackageVersion = $PackageVersion.Substring(0, $PackageVersion.Length - ".nupkg".Length)
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
function Read-ArchiveEntryBytes {
    param([Parameter(Mandatory)] [System.IO.Compression.ZipArchiveEntry] $Entry)
    $stream = $Entry.Open(); $memory = [System.IO.MemoryStream]::new()
    try { $stream.CopyTo($memory); return $memory.ToArray() }
    finally { $memory.Dispose(); $stream.Dispose() }
}
function Get-BrotliLength {
    param([Parameter(Mandatory)] [byte[]] $Bytes)
    $memory = [System.IO.MemoryStream]::new()
    try {
        $brotli = [System.IO.Compression.BrotliStream]::new($memory, [System.IO.Compression.CompressionLevel]::Optimal, $true)
        try { $brotli.Write($Bytes, 0, $Bytes.Length) } finally { $brotli.Dispose() }
        return $memory.Length
    }
    finally { $memory.Dispose() }
}
function Get-Sha256Hex {
    param([Parameter(Mandatory)] [byte[]] $Bytes)
    return [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($Bytes))
}

function Test-ExternalExecutableLoad {
    param([Parameter(Mandatory)] [string] $ScriptText)

    $directLoaderPattern = '(?is)(?:\b(?:fetch|importScripts)\s*(?:/\*.*?\*/\s*)*\(\s*(?:/\*.*?\*/\s*)*["''`]\s*(?:(?:https?:)?//)|\bimport\s*(?:/\*.*?\*/\s*)*(?:\(\s*(?:/\*.*?\*/\s*)*["''`]\s*(?:(?:https?:)?//)|["''`]\s*(?:(?:https?:)?//)|[^;\r\n]*?\bfrom\s*(?:/\*.*?\*/\s*)*["''`]\s*(?:(?:https?:)?//)))'
    if ($ScriptText -match $directLoaderPattern) {
        return $true
    }

    $externalAssignments = [regex]::Matches(
        $ScriptText,
        '(?is)\b(?:const|let|var)\s+([A-Za-z_$][\w$]*)\s*=\s*["''`]\s*(?:(?:https?:)?//)')
    foreach ($assignment in $externalAssignments) {
        $name = [regex]::Escape($assignment.Groups[1].Value)
        $variableLoaderPattern = "(?is)\b(?:fetch|importScripts|import)\s*(?:/\*.*?\*/\s*)*\(\s*(?:/\*.*?\*/\s*)*$name\b"
        if ($ScriptText -match $variableLoaderPattern) {
            return $true
        }
    }

    return $false
}

$archive = [System.IO.Compression.ZipFile]::OpenRead($package.FullName)
try {
    $entryNames = @($archive.Entries | ForEach-Object FullName)
    $unexpectedEntries = @($entryNames | Where-Object { $_ -match '(?i)(^|/)(node_modules|package(?:-lock)?\.json|npm-shrinkwrap\.json|pnpm-lock\.yaml|yarn\.lock|\.pnp\.(?:cjs|js)|\.npmrc|\.yarnrc(?:\.yml)?|bower\.json)(/|$)' })
    if ($unexpectedEntries.Count -gt 0) { throw "Package contains Node artifacts: $($unexpectedEntries -join ', ')" }

    foreach ($entry in @($archive.Entries | Where-Object { $_.FullName -match '\.js$' })) {
        $scriptText = [System.Text.Encoding]::UTF8.GetString((Read-ArchiveEntryBytes -Entry $entry))
        if (Test-ExternalExecutableLoad -ScriptText $scriptText) {
            throw "$($entry.FullName) loads executable assets from an external URL."
        }
    }

    $requiredEntries = @("README.md", "THIRD-PARTY-NOTICES.md", "licenses/three-LICENSE", "third-party/three/PROVENANCE.md", "lib/net10.0/SyntaxCircus.FancyBlazor.WebGL.dll", "staticwebassets/js/fancy-blazor-webgl.js", "staticwebassets/js/holographic-surface-renderer.js", "staticwebassets/js/wave-field-renderer.js", "staticwebassets/js/refractive-orb-renderer.js", "staticwebassets/js/prism-field-renderer.js", "staticwebassets/js/particle-field-renderer.js", "staticwebassets/vendor/three/LICENSE", "staticwebassets/vendor/three/build/three.core.js", "staticwebassets/vendor/three/build/three.module.js", "buildTransitive/SyntaxCircus.FancyBlazor.WebGL.props")
    $missingEntries = @($requiredEntries | Where-Object { $_ -notin $entryNames })
    if ($missingEntries.Count -gt 0) { throw "Package is missing required entries: $($missingEntries -join ', ')" }

    $resolvedProvenancePath = [System.IO.Path]::GetFullPath($ProvenancePath)
    if (-not (Test-Path -LiteralPath $resolvedProvenancePath -PathType Leaf)) {
        throw "Three.js provenance file does not exist: $resolvedProvenancePath"
    }
    [byte[]] $recordedProvenanceBytes = [System.IO.File]::ReadAllBytes($resolvedProvenancePath)
    [byte[]] $packagedProvenanceBytes = Read-ArchiveEntryBytes -Entry $archive.GetEntry("third-party/three/PROVENANCE.md")
    if ((Get-Sha256Hex -Bytes $recordedProvenanceBytes) -ne (Get-Sha256Hex -Bytes $packagedProvenanceBytes)) {
        throw "Packaged Three.js provenance does not match $resolvedProvenancePath."
    }

    $provenanceText = [System.Text.Encoding]::UTF8.GetString($recordedProvenanceBytes)
    $vendorEntries = @(
        @{ Source = "src/SyntaxCircus.FancyBlazor.WebGL/wwwroot/vendor/three/build/three.module.js"; Package = "staticwebassets/vendor/three/build/three.module.js" },
        @{ Source = "src/SyntaxCircus.FancyBlazor.WebGL/wwwroot/vendor/three/build/three.core.js"; Package = "staticwebassets/vendor/three/build/three.core.js" },
        @{ Source = "src/SyntaxCircus.FancyBlazor.WebGL/wwwroot/vendor/three/LICENSE"; Package = "staticwebassets/vendor/three/LICENSE" }
    )
    foreach ($vendorEntry in $vendorEntries) {
        $hashPattern = ('(?m)\|\s*`{0}`\s*\|[^|\r\n]*\|\s*`(?<hash>[0-9A-Fa-f]{{64}})`\s*\|' -f [regex]::Escape($vendorEntry.Source))
        $hashMatch = [regex]::Match($provenanceText, $hashPattern)
        if (-not $hashMatch.Success) {
            throw "Three.js provenance does not record a SHA-256 value for $($vendorEntry.Source)."
        }

        [byte[]] $vendorBytes = Read-ArchiveEntryBytes -Entry $archive.GetEntry($vendorEntry.Package)
        $actualHash = Get-Sha256Hex -Bytes $vendorBytes
        $expectedHash = $hashMatch.Groups['hash'].Value
        if (-not [string]::Equals($actualHash, $expectedHash, [StringComparison]::OrdinalIgnoreCase)) {
            throw "Three.js provenance hash mismatch for $($vendorEntry.Package): expected $expectedHash; found $actualHash."
        }
    }

    $ownedScripts = @($archive.GetEntry("staticwebassets/js/fancy-blazor-webgl.js"), $archive.GetEntry("staticwebassets/js/holographic-surface-renderer.js"), $archive.GetEntry("staticwebassets/js/wave-field-renderer.js"), $archive.GetEntry("staticwebassets/js/refractive-orb-renderer.js"), $archive.GetEntry("staticwebassets/js/prism-field-renderer.js"), $archive.GetEntry("staticwebassets/js/particle-field-renderer.js"))
    [long] $rawLength = 0
    [long] $brotliLength = 0
    foreach ($ownedScript in $ownedScripts) {
        [byte[]] $scriptBytes = Read-ArchiveEntryBytes -Entry $ownedScript
        $rawLength += $scriptBytes.Length
        $brotliLength += Get-BrotliLength -Bytes $scriptBytes
    }
    if ($rawLength -ge 1MB) { throw "Combined adapter/renderer JavaScript is $rawLength bytes raw; the limit is below 1 MiB." }
    if ($brotliLength -ge 250KB) { throw "Combined adapter/renderer JavaScript is $brotliLength bytes Brotli; the limit is below 250 KiB." }
}
finally { $archive.Dispose() }

$corePackage = Get-ChildItem -LiteralPath $corePackageRoot -Filter "SyntaxCircus.FancyBlazor.$PackageVersion.nupkg" | Where-Object { $_.Name -notlike "*.snupkg" } | Select-Object -First 1
if (-not $corePackage) { throw "A matching core package SyntaxCircus.FancyBlazor.$PackageVersion.nupkg is required in $corePackageRoot for clean-consumer validation." }

$scratchRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("fancyblazor-webgl-consumer-" + [Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $scratchRoot | Out-Null
$previousNuGetScratch = $env:NUGET_SCRATCH; $previousAppData = $env:APPDATA; $previousNuGetPackages = $env:NUGET_PACKAGES
$env:NUGET_SCRATCH = Join-Path $scratchRoot ".nuget-scratch"; $env:APPDATA = Join-Path $scratchRoot ".appdata"; $env:NUGET_PACKAGES = Join-Path $scratchRoot ".nuget-packages"
try {
    $projectFile = Join-Path $scratchRoot "PackageConsumer.csproj"; $importsFile = Join-Path $scratchRoot "_Imports.razor"; $componentFile = Join-Path $scratchRoot "Showcase.razor"; $registrationFile = Join-Path $scratchRoot "Registration.cs"; $nugetConfig = Join-Path $scratchRoot "NuGet.config"
    Set-Content -LiteralPath $projectFile -Encoding utf8 -Value @"
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable></PropertyGroup>
  <ItemGroup><PackageReference Include="SyntaxCircus.FancyBlazor.WebGL" Version="$PackageVersion" /></ItemGroup>
</Project>
"@
    Set-Content -LiteralPath $importsFile -Encoding utf8 -Value '@using SyntaxCircus.FancyBlazor'
    Set-Content -LiteralPath $componentFile -Encoding utf8 -Value @'
<HolographicSurface Interactive="true" Palette="FancyPalettes.Witchlight"><article><h1>Semantic package consumer</h1><p>No script tag or Node tooling.</p></article></HolographicSurface>
'@
    Set-Content -LiteralPath $registrationFile -Encoding utf8 -Value @'
using Microsoft.Extensions.DependencyInjection;
using SyntaxCircus.FancyBlazor;
namespace PackageConsumer;
public static class Registration
{
    public static IServiceCollection AddConsumerEffects(this IServiceCollection services) => services.AddFancyBlazorWebGl();
}
'@
    $escapedPackageRoot = [System.Security.SecurityElement]::Escape($packageRoot); $escapedCorePackageRoot = [System.Security.SecurityElement]::Escape($corePackageRoot)
    Set-Content -LiteralPath $nugetConfig -Encoding utf8 -Value @"
<?xml version="1.0" encoding="utf-8"?>
<configuration><packageSources><clear /><add key="webgl-preview" value="$escapedPackageRoot" /><add key="core-package" value="$escapedCorePackageRoot" /><add key="nuget.org" value="https://api.nuget.org/v3/index.json" /></packageSources></configuration>
"@
    & dotnet restore $projectFile --configfile $nugetConfig
    if ($LASTEXITCODE -ne 0) { throw "Clean WebGL package consumer restore failed." }
    & dotnet build $projectFile --no-restore --configuration Release
    if ($LASTEXITCODE -ne 0) { throw "Clean WebGL package consumer build failed." }
}
finally {
    $env:NUGET_SCRATCH = $previousNuGetScratch; $env:APPDATA = $previousAppData; $env:NUGET_PACKAGES = $previousNuGetPackages
    if (Test-Path -LiteralPath $scratchRoot) { Remove-Item -LiteralPath $scratchRoot -Recurse -Force }
}

Write-Host "Verified $($package.Name): local Three assets, provenance, size budget, and clean Razor consumer (raw $rawLength bytes; Brotli $brotliLength bytes)."
