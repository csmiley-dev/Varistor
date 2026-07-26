@echo off
cd /d "%~dp0"
echo Adding GitHub remote...
git remote add origin https://github.com/csmiley-dev/Varistor.git

echo Pushing to GitHub...
git branch -M main
git push -u origin main

echo Done!
pause
