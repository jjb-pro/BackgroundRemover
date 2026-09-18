#Requires -Version 5.1
<#
    Builds BackgroundRemover in Release and zips it up together with the installer scripts
    into a distributable package.

    Usage:
        .\scripts\package.ps1
        .\scripts\package.ps1 -Version 1.3.0 -OutputDirectory C:\temp\out
#>
[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$Version,
    [string]$OutputDirectory = (Join-Path $PSScriptRoot "..\dist")
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$csproj = Join-Path $repoRoot "src\BackgroundRemover.csproj"

if (-not (Test-Path $csproj)) {
    throw "Project file not found at '$csproj'."
}

[xml]$projectXml = Get-Content $csproj

if (-not $Version) {
    $Version = (Select-Xml -Xml $projectXml -XPath "//Project/PropertyGroup/Version").Node.InnerText
    if (-not $Version) {
        throw "Could not determine version from '$csproj'. Pass -Version explicitly."
    }
}

Write-Host "Building BackgroundRemover $Version ($Configuration)..." -ForegroundColor Cyan

dotnet build $csproj -c $Configuration -p:PostBuildEvent= | Out-Host
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE."
}

$targetFramework = (Select-Xml -Xml $projectXml -XPath "//Project/PropertyGroup/TargetFramework").Node.InnerText
$buildOutputDir = Join-Path $repoRoot "src\bin\$Configuration\$targetFramework"

if (-not (Test-Path $buildOutputDir)) {
    throw "Build output not found at '$buildOutputDir'."
}

if (Test-Path $OutputDirectory) {
    Remove-Item $OutputDirectory -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

$stagingDir = Join-Path $OutputDirectory "staging"
$pluginDir = Join-Path $stagingDir "BackgroundRemover"
New-Item -ItemType Directory -Path $pluginDir -Force | Out-Null

Write-Host "Copying build output..." -ForegroundColor Cyan
Copy-Item -Path (Join-Path $buildOutputDir '*') -Destination $pluginDir -Recurse -Exclude "*.pdb", "*.lib", "*.xml"

Write-Host "Copying installer scripts..." -ForegroundColor Cyan
Copy-Item -Path (Join-Path $PSScriptRoot "installer.ps1") -Destination $stagingDir
Copy-Item -Path (Join-Path $PSScriptRoot "Open to install.bat") -Destination $stagingDir

$zipPath = Join-Path $OutputDirectory "BackgroundRemover-v$Version.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Write-Host "Creating '$zipPath'..." -ForegroundColor Cyan
Compress-Archive -Path (Join-Path $stagingDir '*') -DestinationPath $zipPath

Remove-Item $stagingDir -Recurse -Force

Write-Host "`nDone: $zipPath" -ForegroundColor Green
Write-Host "Note: ONNX model files are not included." -ForegroundColor Yellow

Write-Output $zipPath