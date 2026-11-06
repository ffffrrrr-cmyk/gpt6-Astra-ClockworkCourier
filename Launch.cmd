@echo off
cd /d "%~dp0"
if not exist "Builds\Windows\ClockworkCourier.exe" (
  echo Build missing. Open this project in Unity 6000.6.7f1 or run Build.ps1.
  pause
  exit /b 1
)
start "Clockwork Courier" "Builds\Windows\ClockworkCourier.exe" -screen-fullscreen 0 -screen-width 1440 -screen-height 900
