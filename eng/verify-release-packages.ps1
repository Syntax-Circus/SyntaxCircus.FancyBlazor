[CmdletBinding()]
param(
    [string] $PackageDirectory = (Join-Path $PSScriptRoot "..\artifacts")
)

$ErrorActionPreference = "Stop"
$packageRoot = [System.IO.Path]::GetFullPath($PackageDirectory)
if (-not (Test-Path -LiteralPath $packageRoot -PathType Container)) {
    throw "Package directory does not exist: $packageRoot"
}

$packageNames = @(Get-ChildItem -LiteralPath $packageRoot -File -Filter "*.nupkg" |
    Where-Object { $_.Name -notlike "*.snupkg" } |
    ForEach-Object Name)
$corePackages = @($packageNames | Where-Object { $_ -match '^SyntaxCircus\.FancyBlazor\.(?!WebGL\.|UI\.)(?<version>.+)\.nupkg$' })
$webGlPackages = @($packageNames | Where-Object { $_ -match '^SyntaxCircus\.FancyBlazor\.WebGL\.(?<version>.+)\.nupkg$' })
$uiPackages = @($packageNames | Where-Object { $_ -match '^SyntaxCircus\.FancyBlazor\.UI\.(?<version>.+)\.nupkg$' })
$recognizedPackages = @($corePackages) + @($webGlPackages) + @($uiPackages)
$unexpectedPackages = @($packageNames | Where-Object { $_ -notin $recognizedPackages })

if ($unexpectedPackages.Count -gt 0) {
    throw "Release artifacts contain unexpected NuGet packages: $($unexpectedPackages -join ', ')."
}

if ($corePackages.Count -ne 1) {
    throw "Release artifacts must contain exactly one SyntaxCircus.FancyBlazor package; found $($corePackages.Count)."
}
if ($webGlPackages.Count -ne 1) {
    throw "Release artifacts must contain exactly one SyntaxCircus.FancyBlazor.WebGL package; found $($webGlPackages.Count)."
}
if ($uiPackages.Count -ne 1) {
    throw "Release artifacts must contain exactly one SyntaxCircus.FancyBlazor.UI package; found $($uiPackages.Count)."
}

$coreMatch = [regex]::Match($corePackages[0], '^SyntaxCircus\.FancyBlazor\.(?<version>.+)\.nupkg$')
$webGlMatch = [regex]::Match($webGlPackages[0], '^SyntaxCircus\.FancyBlazor\.WebGL\.(?<version>.+)\.nupkg$')
$uiMatch = [regex]::Match($uiPackages[0], '^SyntaxCircus\.FancyBlazor\.UI\.(?<version>.+)\.nupkg$')
$coreVersion = $coreMatch.Groups['version'].Value
$webGlVersion = $webGlMatch.Groups['version'].Value
$uiVersion = $uiMatch.Groups['version'].Value
if (-not [string]::Equals($coreVersion, $webGlVersion, [StringComparison]::Ordinal)) {
    throw "SyntaxCircus.FancyBlazor and SyntaxCircus.FancyBlazor.WebGL must use the same version; found '$coreVersion' and '$webGlVersion'."
}
if (-not [string]::Equals($coreVersion, $uiVersion, [StringComparison]::Ordinal)) {
    throw "SyntaxCircus.FancyBlazor and SyntaxCircus.FancyBlazor.UI must use the same version; found '$coreVersion' and '$uiVersion'."
}

Write-Host "Verified release package set: core, WebGL preview, and UI companion are version $coreVersion."
