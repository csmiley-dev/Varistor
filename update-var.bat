@echo off
cd /d "%~dp0"
echo Building Varistor (Release, Self-Contained)...
dotnet publish VAR\VAR.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false
if errorlevel 1 (
    echo ERROR: Failed to build Varistor
    pause
    exit /b 1
)
echo.
echo Updating Varistor on X:\BMS\Programs\VAR\ ...
xcopy "VAR\bin\Release\net8.0-windows\win-x64\publish\*.*" "X:\BMS\Programs\VAR\" /Y /E /I /Q

if not exist "X:\BMS\Programs\VAR\staff.json" copy "VAR\config-templates\staff.json" "X:\BMS\Programs\VAR\staff.json" >nul

echo Done!
pause
