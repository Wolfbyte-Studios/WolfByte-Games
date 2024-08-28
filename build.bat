@echo off
SET UNITY_PATH="C:\Program Files\Unity\Hub\Editor\6000.0.5f1\Editor\Unity.exe"
SET PROJECT_PATH="C:\Users\user1\3d zeldaish\Ruin The Runner"
SET BUILD_TARGET="C:\Users\user1\3d zeldaish\Builds\Ruin The Runner Current\Ruin The Runner.exe"

%UNITY_PATH% -quit -batchmode -buildWindows64Player %BUILD_TARGET%

echo Build completed.
pause
