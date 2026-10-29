@echo off
setlocal
set "ROOT=%~dp0"
set "DOTNET_ROOT=%ROOT%.tools\dotnet"
if not exist "%DOTNET_ROOT%" mkdir "%DOTNET_ROOT%"
echo Downloading the official Microsoft .NET 10 SDK installer...
powershell -NoProfile -ExecutionPolicy Bypass -Command "$ErrorActionPreference='Stop'; Invoke-WebRequest https://dot.net/v1/dotnet-install.ps1 -OutFile '%ROOT%dotnet-install.ps1'"
if errorlevel 1 goto fail
powershell -NoProfile -ExecutionPolicy Bypass -File "%ROOT%dotnet-install.ps1" -Channel 10.0 -InstallDir "%DOTNET_ROOT%"
if errorlevel 1 goto fail
echo.
echo Portable .NET SDK installed to:
echo %DOTNET_ROOT%
echo Open PowerShell in this folder and run:
echo   $env:DOTNET_ROOT = "$PWD\.tools\dotnet"
echo   $env:PATH = "$env:DOTNET_ROOT;$env:DOTNET_ROOT\tools;$env:PATH"
echo   dotnet --info
exit /b 0
:fail
echo SDK setup failed. Check internet access and retry.
exit /b 1
