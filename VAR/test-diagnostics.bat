@echo off
echo VAR Diagnostics Tool
echo =====================
echo.
echo Current Directory: %CD%
echo.
echo Checking for required files...
echo.

if exist "VAR.exe" (
    echo [OK] VAR.exe found
) else (
    echo [MISSING] VAR.exe NOT found
)

if exist "project.db" (
    echo [OK] project.db found
    dir project.db | find "project.db"
) else (
    echo [MISSING] project.db NOT found
)

if exist "System.Data.SQLite.dll" (
    echo [OK] System.Data.SQLite.dll found
) else (
    echo [MISSING] System.Data.SQLite.dll NOT found - THIS IS REQUIRED!
)

if exist "runtimes" (
    echo [OK] runtimes folder found
) else (
    echo [MISSING] runtimes folder NOT found - THIS IS REQUIRED!
)

echo.
echo Attempting to launch VAR.exe...
echo If VAR closes immediately, check above for missing files.
echo.
pause

VAR.exe

echo.
echo VAR has closed.
pause
