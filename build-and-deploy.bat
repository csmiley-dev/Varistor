@echo off
cd /d "%~dp0"
echo ================================================
echo Build and Deploy to X:\BMS\Programs
echo ================================================
echo.
echo Working directory: %CD%
echo.

echo [Step 1/6] Building PDC (Release, Self-Contained)...
dotnet publish PDC\PDC.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false
if errorlevel 1 (
    echo ERROR: Failed to build PDC
    pause
    exit /b 1
)
echo              Done.

echo.
echo [Step 2/6] Building Varistor (Release, Self-Contained)...
dotnet publish VAR\VAR.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false
if errorlevel 1 (
    echo ERROR: Failed to build Varistor
    pause
    exit /b 1
)
echo              Done.

echo.
echo [Step 3/6] Checking X: drive...
if not exist "X:\" (
    echo ERROR: X: drive not found. Is the network drive connected?
    pause
    exit /b 1
)
echo              X: drive found.

echo.
echo [Step 4/6] Creating folder structure on X: drive...
if not exist "X:\BMS" mkdir "X:\BMS"
if not exist "X:\BMS\Programs" mkdir "X:\BMS\Programs"
if not exist "X:\BMS\Programs\PDC" mkdir "X:\BMS\Programs\PDC"
if not exist "X:\BMS\Programs\VAR" mkdir "X:\BMS\Programs\VAR"
if not exist "X:\Projects" mkdir "X:\Projects"
echo              Done.

echo.
echo [Step 5/6] Copying PDC to X:\BMS\Programs\PDC\ ...
xcopy "PDC\bin\Release\net8.0-windows\win-x64\publish\*.*" "X:\BMS\Programs\PDC\" /Y /E /I /Q
if errorlevel 1 (
    echo ERROR: Failed to copy PDC files
    pause
    exit /b 1
)
echo              Done.

echo.
echo [Step 6/6] Copying Varistor to X:\BMS\Programs\VAR\ ...
xcopy "VAR\bin\Release\net8.0-windows\win-x64\publish\*.*" "X:\BMS\Programs\VAR\" /Y /E /I /Q
if errorlevel 1 (
    echo ERROR: Failed to copy Varistor files
    pause
    exit /b 1
)
echo              Done.

echo.
echo [Step 7/7] Creating PDC shortcut at X:\Projects\PDC.lnk ...
powershell -NoProfile -ExecutionPolicy Bypass -Command "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('X:\Projects\PDC.lnk'); $Shortcut.TargetPath = 'X:\BMS\Programs\PDC\PDC.exe'; $Shortcut.WorkingDirectory = 'X:\BMS\Programs\PDC'; $Shortcut.Save()"
echo              Done.

echo.
echo ================================================
echo Build and Deployment Complete!
echo ================================================
echo.
echo Deployed to:
echo   X:\BMS\Programs\PDC\PDC.exe
echo   X:\BMS\Programs\VAR\Varistor.exe
echo   X:\Projects\PDC.lnk
echo.
echo USAGE:
echo 1. Go to X:\Projects\
echo 2. Double-click PDC.lnk
echo 3. Create a new project
echo 4. Use Varistor.lnk in the project's Variations folder
echo.
pause
