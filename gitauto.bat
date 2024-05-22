@echo off
REM Get the directory of the script
set SCRIPT_DIR=%~dp0

REM Change directory to the script directory
cd /d "%SCRIPT_DIR%"

REM Pull the latest changes from the remote repository
echo Pulling the latest changes from the remote repository...
git pull origin main

REM Add all changes to the staging area
echo Adding changes to the staging area...
git add .

REM Commit the changes
echo Committing changes...
git commit -m "Auto-commit from script"

REM Push the changes to the remote repository
echo Pushing changes to the remote repository...
git push origin main

REM Check if the task already exists
schtasks /query /tn "GitAutoUpdate" >nul 2>&1
if %errorlevel%==0 (
    ) else (
    schtasks /create /sc daily /tn "GitAutoUpdate" /tr "wscript.exe \"%SCRIPT_DIR%run_silent.vbs\"" /st 05:00
)

echo Done!
pause
