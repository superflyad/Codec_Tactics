param(
    [switch]$SkipDotNet
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

Write-Host "Validating Codec_Tactics repository foundation..."

$requiredPaths = @(
    "README.md",
    "LICENSE",
    ".gitignore",
    ".editorconfig",
    "CONTRIBUTING.md",
    "CHANGELOG.md",
    "ROADMAP.md",
    "CODEX.md",
    "docs",
    "docs/game-design.md",
    "docs/architecture.md",
    "docs/network-integrity.md",
    "docs/milestones.md",
    "docs/visible-prototype.md",
    "scripts",
    "src/CodecTactics.Core/CodecTactics.Core.csproj",
    "src/CodecTactics.MonoGame/CodecTactics.MonoGame.csproj",
    "tests/CodecTactics.Core.Tests/CodecTactics.Core.Tests.csproj",
    "Codec_Tactics.sln"
)

$missing = @()
foreach ($path in $requiredPaths) {
    if (-not (Test-Path -LiteralPath $path)) {
        $missing += $path
    }
}

if ($missing.Count -gt 0) {
    Write-Error "Missing required path(s): $($missing -join ', ')"
}

Write-Host "Godot validation is retired; legacy Godot files are intentionally not part of the active workflow."

if (-not $SkipDotNet) {
    dotnet format --version *> $null
    if ($LASTEXITCODE -eq 0) {
        dotnet format .\Codec_Tactics.sln --verify-no-changes --verbosity minimal
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet format verification failed."
        }
    } else {
        Write-Host "dotnet format is not available; skipping format verification."
    }

    dotnet build .\Codec_Tactics.sln --configuration Release
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed."
    }

    dotnet run --project .\tests\CodecTactics.Core.Tests\CodecTactics.Core.Tests.csproj --configuration Release --no-build
    if ($LASTEXITCODE -ne 0) {
        throw "test run failed."
    }
} else {
    Write-Host "Skipping .NET build/tests because -SkipDotNet was provided."
}

Write-Host "Validation completed."
