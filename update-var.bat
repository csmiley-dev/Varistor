@echo off
cd /d "%~dp0"
echo Updating Varistor on X:\BMS\Programs\VAR\ ...
xcopy "VAR\bin\Release\net8.0-windows\win-x64\publish\*.*" "X:\BMS\Programs\VAR\" /Y /E /I /Q
echo Done!
pause
