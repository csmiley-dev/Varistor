@echo off
echo Testing VAR application...
echo.
echo Current directory: %CD%
echo.
echo Checking for project.db...
if exist "project.db" (
    echo [OK] project.db found
) else (
    echo [ERROR] project.db NOT FOUND
    echo VAR requires project.db to be in the same folder
)
echo.
echo Press any key to launch VAR...
pause > nul

cd "C:\Users\Cameron\Documents\My Documents\Programming\Varistor\VAR\bin\Release\net8.0-windows"
VAR.exe

echo.
echo VAR has exited. Press any key to close...
pause
