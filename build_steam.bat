@echo off
setlocal

REM WARNING: This script contains plain-text credentials. 
REM Do NOT commit it to source control or share it.

REM Get the project root (where this batch file is located)
set "PROJECT_ROOT=%~dp0"
set "LAUNCHER_DIR=%PROJECT_ROOT%Launcher"
set "PUBLISH_DIR=%PROJECT_ROOT%publish"
set "GAME_DIR=%PROJECT_ROOT%Game"

echo ========================================
echo Building CC2 Launcher...
echo ========================================

REM Build and publish the launcher
cd /d "%LAUNCHER_DIR%"
if errorlevel 1 (
    echo ERROR: Failed to change to Launcher directory
    pause
    exit /b 1
)

dotnet publish -c Release -r win-x64 --self-contained true -o "%PUBLISH_DIR%"
if errorlevel 1 (
    echo ERROR: Build failed
    pause
    exit /b 1
)

echo.
echo ========================================
echo Copying Game folder to publish...
echo ========================================

REM Copy Game folder to publish directory
if exist "%GAME_DIR%" (
    xcopy /E /I /Y "%GAME_DIR%" "%PUBLISH_DIR%\Game\" >nul
    if errorlevel 1 (
        echo ERROR: Failed to copy Game folder
        pause
        exit /b 1
    )
    echo Game folder copied successfully.
) else (
    echo WARNING: Game folder not found at %GAME_DIR%
)

REM Remove steam_appid.txt from publish (should not be in Steam builds)
if exist "%PUBLISH_DIR%\steam_appid.txt" (
    del /F /Q "%PUBLISH_DIR%\steam_appid.txt"
    echo Removed steam_appid.txt from publish folder.
)

echo.
echo ========================================
echo Uploading to Steam...
echo ========================================

REM Path to SteamCMD (adjust if your SDK is in a different location)
set "STEAMCMD_EXE=C:\Software\steamworks_sdk\sdk\tools\ContentBuilder\builder\steamcmd.exe"

REM Path to steam folder (where the VDF files live)
set "STEAM_DIR=%PROJECT_ROOT%steam"

REM App build script
set "APP_BUILD_VDF=%STEAM_DIR%\app_build_721450.vdf"

"%STEAMCMD_EXE%" ^
  +login visionhitman "Jkuo8501.S" ^
  +run_app_build_http "%APP_BUILD_VDF%" ^
  +quit

if errorlevel 1 (
    echo.
    echo ERROR: Steam upload failed
    pause
    exit /b 1
)

echo.
echo ========================================
echo Build and upload completed successfully!
echo ========================================
pause
