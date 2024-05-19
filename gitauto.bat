@echo off

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

echo Done!
pause