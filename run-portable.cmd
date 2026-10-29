@echo off
setlocal
set "ROOT=%~dp0"
set "DOTNET_ROOT=%ROOT%.tools\dotnet"
set "PATH=%DOTNET_ROOT%;%DOTNET_ROOT%\tools;%PATH%"
if not exist "%DOTNET_ROOT%\dotnet.exe" (
  echo Portable SDK not found. Run setup-portable-dotnet.cmd first.
  pause
  exit /b 1
)
cd /d "%ROOT%"
dotnet restore
if errorlevel 1 goto fail
dotnet run
goto end
:fail
echo Restore or run failed. Read the error above.
:end
pause
