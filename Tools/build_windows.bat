@echo off
REM Build script: runs Unity in batch mode and calls the editor build method.
REM Adjust UNITY_PATH to point to your installed Unity editor executable.
set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\2021.3.0f1\Editor\Unity.exe"

"%UNITY_PATH%" -projectPath "%CD%" -quit -batchmode -executeMethod RogueLike2D.Editor.BuildScript.PerformWindowsBuild -logFile "%CD%\build_log.txt"

echo Build finished. See build_log.txt for details.
pause
