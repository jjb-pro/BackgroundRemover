@echo off
:: Elevate to admin
:: Check for admin rights
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo Requesting administrative privileges...
    powershell -Command "Start-Process -Verb RunAs -FilePath '%~f0'"
    exit /b
)

:: Run PowerShell script with execution policy bypass
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0installer.ps1"
pause
