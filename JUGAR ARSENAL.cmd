@echo off
cd /d "%~dp0"
if exist "Builds\Arsenal-0.6\AbisalStealness.exe" (
start "" /D "%~dp0Builds\Arsenal-0.6" "%~dp0Builds\Arsenal-0.6\AbisalStealness.exe" -screen-fullscreen 0 -screen-width 1280 -screen-height 720
) else (
echo Extrae el paquete Windows y abre JUGAR ABISAL.cmd.
pause
)
