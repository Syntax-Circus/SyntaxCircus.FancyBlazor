[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$markdownFiles = @(
    Get-Item -LiteralPath (Join-Path $repositoryRoot "README.md")
    Get-Item -LiteralPath (Join-Path $repositoryRoot "AGENTS.md")
    Get-Item -LiteralPath (Join-Path $repositoryRoot "CHANGELOG.md")
    Get-ChildItem -LiteralPath (Join-Path $repositoryRoot "docs") -Filter "*.md" -Recurse
)

$brokenLinks = [System.Collections.Generic.List[string]]::new()
$linkPattern = '!?(?:\[[^\]]*\])\((?<target>[^)]+)\)'

foreach ($file in $markdownFiles) {
    $content = Get-Content -LiteralPath $file.FullName -Raw
    foreach ($match in [regex]::Matches($content, $linkPattern)) {
        $target = $match.Groups["target"].Value.Trim()
        if ($target.StartsWith("<") -and $target.EndsWith(">")) {
            $target = $target.Substring(1, $target.Length - 2)
        }

        $target = ($target -split '#', 2)[0]
        if ([string]::IsNullOrWhiteSpace($target) -or
            $target -match '^(?:https?|mailto):' -or
            $target.StartsWith("#")) {
            continue
        }

        $decodedTarget = [Uri]::UnescapeDataString($target)
        $resolvedTarget = [System.IO.Path]::GetFullPath((Join-Path $file.DirectoryName $decodedTarget))
        if (-not $resolvedTarget.StartsWith($repositoryRoot, [StringComparison]::OrdinalIgnoreCase) -or
            -not (Test-Path -LiteralPath $resolvedTarget)) {
            $relativeFile = [System.IO.Path]::GetRelativePath($repositoryRoot, $file.FullName)
            $brokenLinks.Add("$relativeFile -> $target")
        }
    }
}

if ($brokenLinks.Count -gt 0) {
    throw "Broken local Markdown links:`n$($brokenLinks -join "`n")"
}

$requiredDocumentation = @(
    "docs/getting-started.md",
    "docs/components/shader-background.md",
    "docs/components/glow-border.md",
    "docs/components/reveal.md",
    "docs/components/tilt.md",
    "docs/components/gradient-background.md",
    "docs/components/spotlight.md",
    "docs/components/magnetic.md",
    "docs/components/parallax.md",
    "docs/components/stagger.md",
    "docs/components/shimmer.md",
    "docs/guides/accessibility.md",
    "docs/guides/performance.md",
    "docs/guides/hosting-modes.md",
    "docs/guides/palettes-and-styling.md",
    "docs/guides/troubleshooting.md",
    "samples/FancyBlazor.Demo.Client/Pages/Background.razor",
    "samples/FancyBlazor.Demo.Client/Pages/Border.razor",
    "samples/FancyBlazor.Demo.Client/Pages/RevealPage.razor",
    "samples/FancyBlazor.Demo.Client/Pages/TiltPage.razor"
    ,"samples/FancyBlazor.Demo.Client/Pages/ExpandedEffects.razor"
)

$missingDocumentation = @($requiredDocumentation | Where-Object {
    -not (Test-Path -LiteralPath (Join-Path $repositoryRoot $_))
})

if ($missingDocumentation.Count -gt 0) {
    throw "Required documentation or compiling examples are missing: $($missingDocumentation -join ', ')"
}

Write-Host "Verified $($markdownFiles.Count) Markdown files and all required component guides/examples."
