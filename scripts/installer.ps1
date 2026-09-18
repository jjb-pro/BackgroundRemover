$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition

$sourceFolder = Join-Path $scriptDir "BackgroundRemover"
$regPath = "HKLM:\SOFTWARE\paint.net"
$regValue = "TARGETDIR"

function PauseAndExit($message, [int]$code = 1) {
    Write-Host $message -ForegroundColor Red
    Write-Host "Press Enter to exit..."
    Read-Host | Out-Null
    exit $code
}

try {
    if (-not (Test-Path $sourceFolder)) {
        PauseAndExit "Error: Source folder '$sourceFolder' not found in current directory."
    }

    Write-Host "Reading Paint.NET installation directory from registry..."
    $paintNetDir = (Get-ItemProperty -Path $regPath -Name $regValue -ErrorAction Stop).$regValue

    if (-not $paintNetDir -or -not (Test-Path $paintNetDir)) {
        PauseAndExit "Error: Paint.NET installation directory is invalid or not found: $paintNetDir"
    }

    $targetFolder = Join-Path $paintNetDir "Effects\BackgroundRemover"
    if (-not (Test-Path $targetFolder)) {
        Write-Host "Creating directory '$targetFolder'..."
        New-Item -ItemType Directory -Path $targetFolder -Force | Out-Null
    }

    Write-Host "Copying plugin files from '$sourceFolder' to '$targetFolder'..."
    Copy-Item -Path (Join-Path $sourceFolder '*') -Destination $targetFolder -Recurse -Force

    Write-Host "`nPlugin installed successfully!" -ForegroundColor Green
    Write-Host "You can now start Paint.NET and use the BackgroundRemover plugin."
    Write-Host "Press Enter to exit..."
    Read-Host | Out-Null

} catch {
    PauseAndExit "An unexpected error occurred: $_"
}

