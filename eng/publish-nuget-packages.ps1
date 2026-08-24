[CmdletBinding(SupportsShouldProcess)]
param(
    [string] $PackageDirectory = (Join-Path $PSScriptRoot "..\artifacts"),
    [Parameter(Mandatory)] [string] $ApiKey,
    [string] $Source = "https://api.nuget.org/v3/index.json"
)

$ErrorActionPreference = "Stop"
$packageRoot = [System.IO.Path]::GetFullPath($PackageDirectory)
if (-not (Test-Path -LiteralPath $packageRoot -PathType Container)) {
    throw "Package directory does not exist: $packageRoot"
}

$packages = @(Get-ChildItem -LiteralPath $packageRoot -File -Filter "*.nupkg" |
    Where-Object { $_.Name -notlike "*.snupkg" } |
    Sort-Object Name)
if ($packages.Count -eq 0) {
    throw "No NuGet packages were found in $packageRoot."
}

foreach ($package in $packages) {
    if ($PSCmdlet.ShouldProcess($package.FullName, "Push to $Source")) {
        & dotnet nuget push $package.FullName --api-key $ApiKey --source $Source --skip-duplicate
        if ($LASTEXITCODE -ne 0) {
            throw "NuGet push failed for $($package.Name)."
        }
    }
}
