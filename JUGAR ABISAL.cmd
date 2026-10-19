@echo off
setlocal
cd /d "%~dp0"
if exist "%~dp0AbisalStealness.exe" (
    start "" /D "%~dp0" "%~dp0AbisalStealness.exe" -screen-fullscreen 0 -screen-width 1280 -screen-height 720
    exit /b 0
)
if exist "Builds\Windows\AbisalStealness.exe" (
    start "" /D "%~dp0Builds\Windows" "%~dp0Builds\Windows\AbisalStealness.exe" -screen-fullscreen 0 -screen-width 1280 -screen-height 720
    exit /b 0
)
echo No se encontro el ejecutable. Abre el proyecto en Unity y usa Abisal ^> Abrir puerto y jugar.
pause
