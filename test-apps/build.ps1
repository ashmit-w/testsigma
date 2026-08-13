<#
    Builds every project in TestTargets.sln in Release configuration and
    copies each project's output (exe, pdb, config) into TestTargets\dist\.
#>
param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$distDir = Join-Path $root "dist"

$projects = @(
    "LegacyForms\LegacyForms.csproj",
    "MenuDemo\MenuDemo.csproj",
    "ControlZoo\ControlZoo.csproj",
    "SimpleCalc\SimpleCalc.csproj",
    "TaskList\TaskList.csproj"
)

if (Test-Path $distDir) {
    Remove-Item $distDir -Recurse -Force
}
New-Item -ItemType Directory -Path $distDir | Out-Null

foreach ($proj in $projects) {
    $fullPath = Join-Path $root $proj
    Write-Host "Building $fullPath ..." -ForegroundColor Cyan

    dotnet build $fullPath -c $Configuration
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed for $fullPath"
    }

    $projDir = Split-Path $fullPath -Parent
    $outDir = Join-Path $projDir "bin\$Configuration\net48"

    if (-not (Test-Path $outDir)) {
        throw "Expected output directory not found: $outDir"
    }

    Copy-Item (Join-Path $outDir "*") -Destination $distDir -Recurse -Force
}

Write-Host ""
Write-Host "All projects built. Executables are in: $distDir" -ForegroundColor Green
Get-ChildItem $distDir -Filter *.exe | ForEach-Object { Write-Host " - $($_.Name)" }
