[CmdletBinding()]
param(
    [string] $PackageDirectory = (Join-Path $PSScriptRoot "..\artifacts"),
    [string] $PackageVersion
)

$ErrorActionPreference = "Stop"
$packageRoot = [System.IO.Path]::GetFullPath($PackageDirectory)

if (-not (Test-Path -LiteralPath $packageRoot -PathType Container)) {
    throw "Package directory does not exist: $packageRoot"
}

$packages = Get-ChildItem -LiteralPath $packageRoot -Filter "SyntaxCircus.FancyBlazor.*.nupkg" |
    Where-Object { $_.Name -notlike "*.snupkg" } |
    Sort-Object LastWriteTimeUtc -Descending

if (-not $packages) {
    throw "No SyntaxCircus.FancyBlazor package was found in $packageRoot."
}

$package = $packages[0]
if (-not $PackageVersion) {
    $PackageVersion = $package.Name.Substring("SyntaxCircus.FancyBlazor.".Length)
    $PackageVersion = $PackageVersion.Substring(0, $PackageVersion.Length - ".nupkg".Length)
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::OpenRead($package.FullName)

try {
    $entryNames = @($archive.Entries | ForEach-Object FullName)
    $requiredEntries = @(
        "README.md",
        "THIRD-PARTY-NOTICES.md",
        "licenses/shader-gallery-LICENSE",
        "lib/net10.0/SyntaxCircus.FancyBlazor.dll",
        "staticwebassets/js/fancy-blazor.js",
        "staticwebassets/js/shader-gallery-renderer.js",
        "staticwebassets/vendor/shader-gallery/nacre.frag",
        "buildTransitive/SyntaxCircus.FancyBlazor.props"
    )

    $missingEntries = @($requiredEntries | Where-Object { $_ -notin $entryNames })
    if ($missingEntries.Count -gt 0) {
        throw "Package is missing required entries: $($missingEntries -join ', ')"
    }

    $unexpectedEntries = @($entryNames | Where-Object {
        $_ -match '(^|/)(node_modules|package-lock\.json|pnpm-lock\.yaml|yarn\.lock)(/|$)'
    })
    if ($unexpectedEntries.Count -gt 0) {
        throw "Package contains Node artifacts: $($unexpectedEntries -join ', ')"
    }

    foreach ($scriptName in @(
        "staticwebassets/js/fancy-blazor.js",
        "staticwebassets/js/shader-gallery-renderer.js"
    )) {
        $scriptEntry = $archive.GetEntry($scriptName)
        $reader = [System.IO.StreamReader]::new($scriptEntry.Open())
        try {
            $scriptText = $reader.ReadToEnd()
            if ($scriptText -match '(?i)(?:import\s*(?:\(|[^;]*?from\s*)|fetch\s*\()\s*["'']https?://') {
                throw "$scriptName loads executable assets from an external URL."
            }
        }
        finally {
            $reader.Dispose()
        }
    }
}
finally {
    $archive.Dispose()
}

$scratchRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("fancyblazor-consumer-" + [Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $scratchRoot | Out-Null
$previousNuGetScratch = $env:NUGET_SCRATCH
$previousAppData = $env:APPDATA
$previousNuGetPackages = $env:NUGET_PACKAGES
$env:NUGET_SCRATCH = Join-Path $scratchRoot ".nuget-scratch"
$env:APPDATA = Join-Path $scratchRoot ".appdata"
$env:NUGET_PACKAGES = Join-Path $scratchRoot ".nuget-packages"

try {
    $projectFile = Join-Path $scratchRoot "PackageConsumer.csproj"
    $importsFile = Join-Path $scratchRoot "_Imports.razor"
    $componentFile = Join-Path $scratchRoot "Showcase.razor"
    $registrationFile = Join-Path $scratchRoot "Registration.cs"
    $nugetConfig = Join-Path $scratchRoot "NuGet.config"

    Set-Content -LiteralPath $projectFile -Encoding utf8 -Value @"
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="SyntaxCircus.FancyBlazor" Version="$PackageVersion" />
  </ItemGroup>
</Project>
"@

    Set-Content -LiteralPath $importsFile -Encoding utf8 -Value @'
@using SyntaxCircus.FancyBlazor
'@

    Set-Content -LiteralPath $componentFile -Encoding utf8 -Value @'
<ShaderBackground Palette="FancyPalettes.Midnight">
    <GlowBorder>
        <Reveal Effect="RevealEffect.FadeUp">
            <Tilt Glare="true"><button type="button">Action</button></Tilt>
        </Reveal>
    </GlowBorder>
</ShaderBackground>
'@

    Set-Content -LiteralPath $registrationFile -Encoding utf8 -Value @'
using Microsoft.Extensions.DependencyInjection;
using SyntaxCircus.FancyBlazor;

namespace PackageConsumer;

public static class Registration
{
    public static IServiceCollection AddConsumerEffects(this IServiceCollection services)
        => services.AddFancyBlazor(options => options.EnableDiagnostics = true);
}
'@

    $escapedPackageRoot = [System.Security.SecurityElement]::Escape($packageRoot)
    Set-Content -LiteralPath $nugetConfig -Encoding utf8 -Value @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local-package" value="$escapedPackageRoot" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@

    & dotnet restore $projectFile --configfile $nugetConfig
    if ($LASTEXITCODE -ne 0) {
        throw "Clean consumer restore failed."
    }

    & dotnet build $projectFile --no-restore --configuration Release
    if ($LASTEXITCODE -ne 0) {
        throw "Clean consumer build failed."
    }
}
finally {
    $env:NUGET_SCRATCH = $previousNuGetScratch
    $env:APPDATA = $previousAppData
    $env:NUGET_PACKAGES = $previousNuGetPackages
    if (Test-Path -LiteralPath $scratchRoot) {
        Remove-Item -LiteralPath $scratchRoot -Recurse -Force
    }
}

Write-Host "Verified $($package.Name): required assets present and clean Razor consumer builds."
