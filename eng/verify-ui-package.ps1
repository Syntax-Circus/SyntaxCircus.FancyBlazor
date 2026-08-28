[CmdletBinding()]
param(
    [string] $PackageDirectory = (Join-Path $PSScriptRoot "..\artifacts"),
    [string] $CorePackageDirectory,
    [string] $PackageVersion
)

$ErrorActionPreference = "Stop"
$packageRoot = [System.IO.Path]::GetFullPath($PackageDirectory)
if (-not (Test-Path -LiteralPath $packageRoot -PathType Container)) { throw "Package directory does not exist: $packageRoot" }
if (-not $CorePackageDirectory) { $CorePackageDirectory = Join-Path $packageRoot ".." }
$corePackageRoot = [System.IO.Path]::GetFullPath($CorePackageDirectory)
if (-not (Test-Path -LiteralPath $corePackageRoot -PathType Container)) { throw "Core package directory does not exist: $corePackageRoot" }

$packages = Get-ChildItem -LiteralPath $packageRoot -Filter "SyntaxCircus.FancyBlazor.UI.*.nupkg" | Where-Object { $_.Name -notlike "*.snupkg" } | Sort-Object LastWriteTimeUtc -Descending
if (-not $packages) { throw "No SyntaxCircus.FancyBlazor.UI package was found in $packageRoot." }
$package = $packages[0]
if (-not $PackageVersion) {
    $PackageVersion = $package.Name.Substring("SyntaxCircus.FancyBlazor.UI.".Length)
    $PackageVersion = $PackageVersion.Substring(0, $PackageVersion.Length - ".nupkg".Length)
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::OpenRead($package.FullName)
try {
    $entryNames = @($archive.Entries | ForEach-Object FullName)

    $unexpectedEntries = @($entryNames | Where-Object {
        $_ -match '(?i)(^|/)(node_modules|package(?:-lock)?\.json|npm-shrinkwrap\.json|pnpm-lock\.yaml|yarn\.lock|\.pnp\.(?:cjs|js)|\.npmrc|\.yarnrc(?:\.yml)?|bower\.json)(/|$)'
    })
    if ($unexpectedEntries.Count -gt 0) { throw "Package contains Node artifacts: $($unexpectedEntries -join ', ')" }

    $bootstrapEntries = @($entryNames | Where-Object { $_ -match '(?i)bootstrap' })
    if ($bootstrapEntries.Count -gt 0) {
        throw "Package contains a Bootstrap asset, which is test/demo-only and must never ship: $($bootstrapEntries -join ', ')"
    }

    $requiredEntries = @(
        "README.md",
        "THIRD-PARTY-NOTICES.md",
        "lib/net10.0/SyntaxCircus.FancyBlazor.UI.dll",
        "staticwebassets/js/faq-accordion.js",
        "buildTransitive/SyntaxCircus.FancyBlazor.UI.props"
    )
    $missingEntries = @($requiredEntries | Where-Object { $_ -notin $entryNames })
    if ($missingEntries.Count -gt 0) { throw "Package is missing required entries: $($missingEntries -join ', ')" }
}
finally { $archive.Dispose() }

$corePackage = Get-ChildItem -LiteralPath $corePackageRoot -Filter "SyntaxCircus.FancyBlazor.$PackageVersion.nupkg" | Where-Object { $_.Name -notlike "*.snupkg" } | Select-Object -First 1
if (-not $corePackage) { throw "A matching core package SyntaxCircus.FancyBlazor.$PackageVersion.nupkg is required in $corePackageRoot for clean-consumer validation." }

$scratchRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("fancyblazor-ui-consumer-" + [Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $scratchRoot | Out-Null
$previousNuGetScratch = $env:NUGET_SCRATCH; $previousAppData = $env:APPDATA; $previousNuGetPackages = $env:NUGET_PACKAGES
$env:NUGET_SCRATCH = Join-Path $scratchRoot ".nuget-scratch"; $env:APPDATA = Join-Path $scratchRoot ".appdata"; $env:NUGET_PACKAGES = Join-Path $scratchRoot ".nuget-packages"
try {
    $projectFile = Join-Path $scratchRoot "PackageConsumer.csproj"
    $importsFile = Join-Path $scratchRoot "_Imports.razor"
    $componentFile = Join-Path $scratchRoot "Showcase.razor"
    $registrationFile = Join-Path $scratchRoot "Registration.cs"
    $nugetConfig = Join-Path $scratchRoot "NuGet.config"

    Set-Content -LiteralPath $projectFile -Encoding utf8 -Value @"
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable></PropertyGroup>
  <ItemGroup><PackageReference Include="SyntaxCircus.FancyBlazor.UI" Version="$PackageVersion" /></ItemGroup>
</Project>
"@

    Set-Content -LiteralPath $importsFile -Encoding utf8 -Value '@using SyntaxCircus.FancyBlazor'

    Set-Content -LiteralPath $componentFile -Encoding utf8 -Value @'
<FancyNavbar AriaLabel="Site">
    <Brand><FancyLink Href="/">Acme</FancyLink></Brand>
    <Links><FancyLink Href="/pricing">Pricing</FancyLink></Links>
    <Actions><FancyButton>Sign in</FancyButton></Actions>
</FancyNavbar>
<FancyCard>
    <Header><FancyBadge>New</FancyBadge></Header>
    <ChildContent><p>No script tag or Node tooling.</p></ChildContent>
</FancyCard>
'@

    Set-Content -LiteralPath $registrationFile -Encoding utf8 -Value @'
using Microsoft.Extensions.DependencyInjection;
using SyntaxCircus.FancyBlazor;
namespace PackageConsumer;
public static class Registration
{
    public static IServiceCollection AddConsumerUi(this IServiceCollection services) => services.AddFancyBlazorUi();
}
'@

    $escapedPackageRoot = [System.Security.SecurityElement]::Escape($packageRoot)
    $escapedCorePackageRoot = [System.Security.SecurityElement]::Escape($corePackageRoot)
    Set-Content -LiteralPath $nugetConfig -Encoding utf8 -Value @"
<?xml version="1.0" encoding="utf-8"?>
<configuration><packageSources><clear /><add key="ui-companion" value="$escapedPackageRoot" /><add key="core-package" value="$escapedCorePackageRoot" /><add key="nuget.org" value="https://api.nuget.org/v3/index.json" /></packageSources></configuration>
"@

    & dotnet restore $projectFile --configfile $nugetConfig
    if ($LASTEXITCODE -ne 0) { throw "Clean UI package consumer restore failed." }

    $assetsPath = Join-Path $scratchRoot "obj\project.assets.json"
    if (-not (Test-Path -LiteralPath $assetsPath -PathType Leaf)) { throw "Clean UI package consumer did not produce a restore assets file." }
    $assetsText = Get-Content -LiteralPath $assetsPath -Raw
    if ($assetsText -notmatch '(?i)"SyntaxCircus\.FancyBlazor/') {
        throw "Clean UI package consumer's dependency graph does not contain the core package."
    }
    if ($assetsText -match '(?i)"SyntaxCircus\.FancyBlazor\.WebGL') {
        throw "Clean UI package consumer's dependency graph unexpectedly contains the WebGL companion; the UI companion must depend only on core."
    }

    & dotnet build $projectFile --no-restore --configuration Release
    if ($LASTEXITCODE -ne 0) { throw "Clean UI package consumer build failed." }
}
finally {
    $env:NUGET_SCRATCH = $previousNuGetScratch; $env:APPDATA = $previousAppData; $env:NUGET_PACKAGES = $previousNuGetPackages
    if (Test-Path -LiteralPath $scratchRoot) { Remove-Item -LiteralPath $scratchRoot -Recurse -Force }
}

Write-Host "Verified $($package.Name): required assets present, no Bootstrap asset packed, core-only dependency graph, and clean Razor consumer builds."
