@echo off
setlocal
set CONTENT_ROOT=C:\Software\steamworks_sdk\sdk\tools\ContentBuilder\content

echo Stopping any running BG.Server...
taskkill /IM BG.Server.exe /F >nul 2>&1
timeout /t 1 /nobreak >nul

echo Building...
dotnet publish Server\BG.Server.csproj -c Release -o "%CONTENT_ROOT%"
if errorlevel 1 (
  echo Build failed.
  exit /b 1
)

echo Done. Content is at %CONTENT_ROOT%
echo Run BG.Server.exe to host the game and open the browser to http://localhost:5079
endlocal
