@echo off
setlocal

REM Get the project root (where this batch file is located)
set "PROJECT_ROOT=%~dp0"
set "LAUNCHER_DIR=%PROJECT_ROOT%Launcher"
set "PUBLISH_DIR=%PROJECT_ROOT%publish"
set "GAME_DIR=%PROJECT_ROOT%Game"
set "CONTENTBUILDER_CONTENT=C:\Software\steamworks_sdk\sdk\tools\ContentBuilder\content"

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
echo Copying Steamworks native DLLs...
echo ========================================

REM Steamworks.NET requires steam_api64.dll next to the executable for P/Invoke to work.
set "STEAM_SDK_REDIST=C:\Software\steamworks_sdk\sdk\redistributable_bin\win64"
if exist "%STEAM_SDK_REDIST%\steam_api64.dll" (
    copy /Y "%STEAM_SDK_REDIST%\steam_api64.dll" "%PUBLISH_DIR%\steam_api64.dll" >nul
    if errorlevel 1 (
        echo ERROR: Failed to copy steam_api64.dll
        pause
        exit /b 1
    )
    echo steam_api64.dll copied successfully.
) else (
    echo ERROR: steam_api64.dll not found at %STEAM_SDK_REDIST%
    echo Make sure the Steamworks SDK is installed at C:\Software\steamworks_sdk
    pause
    exit /b 1
)

echo.
echo ========================================
echo Copying to ContentBuilder content...
echo ========================================

REM Copy contents of publish folder to ContentBuilder content
if not exist "%CONTENTBUILDER_CONTENT%" (
    mkdir "%CONTENTBUILDER_CONTENT%"
)
xcopy /E /I /Y "%PUBLISH_DIR%\*" "%CONTENTBUILDER_CONTENT%\"
if errorlevel 1 (
    echo ERROR: Failed to copy to ContentBuilder content
    pause
    exit /b 1
)

echo.
echo ========================================
echo Publish completed successfully!
echo ========================================
echo Output: %PUBLISH_DIR%
echo ContentBuilder: %CONTENTBUILDER_CONTENT%
echo ========================================
pause
