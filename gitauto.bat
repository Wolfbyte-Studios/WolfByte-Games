@echo off
REM Get the directory of the script
set SCRIPT_DIR=%~dp0

REM Change directory to the script directory
cd /d "%SCRIPT_DIR%"

REM Trigger the Unity build process and wait for it to finish
echo Starting Unity build process...
REM call "%SCRIPT_DIR%build.bat" windows

REM After the build is finished, proceed with the Git operations
REM Pull the latest changes from the remote repository
echo Pulling the latest changes from the remote repository...
git pull origin main

REM Add all changes to the staging area
echo Adding changes to the staging area...
git add .

REM Generate a random commit message using the current date and time
set commit_message="Auto-commit %date% %time:~0,2%-%time:~3,2%-%time:~6,2%"

REM Commit the changes
echo Committing changes with message: %commit_message%
git commit -m %commit_message%

REM Push the changes to the remote repository
echo Pushing changes to the remote repository...
git push origin main

REM Check if the task already exists
schtasks /query /tn "GitAutoUpdateMain1" >nul 2>&1
if %errorlevel%==0 (
    REM Task already exists, do nothing
) else (
    REM Create a scheduled task to run run_silent.vbs every 3 hours
    schtasks /create /sc hourly /mo 3 /tn "GitAutoUpdate1" /tr "wscript.exe \"%SCRIPT_DIR%run_silent.vbs\"" /f
)

echo Done!
pause
