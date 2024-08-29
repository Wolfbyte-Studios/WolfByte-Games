@echo off
SET UNITY_PATH="C:\Program Files\Unity\Hub\Editor\6000.0.5f1\Editor\Unity.exe"
SET PROJECT_PATH="C:\Users\user1\3d zeldaish\Ruin The Runner"
SET BUILD_TARGET="C:\Users\user1\3d zeldaish\Builds\Ruin The Runner Current\Ruin The Runner.exe"
SET ARCHIVE_PATH="C:\Users\user1\3d zeldaish\Builds\Ruin_The_Runner_Current.zip"

%UNITY_PATH% -quit -batchmode -buildWindows64Player "%BUILD_TARGET%"

echo Build completed. Creating archive...

REM Creating a zip archive of the build
powershell -Command "Compress-Archive -Path 'C:\Users\user1\3d zeldaish\Builds\Ruin The Runner Current\*' -DestinationPath '%ARCHIVE_PATH%'"

echo Archive created at %ARCHIVE_PATH%
