@echo off
setlocal ENABLEEXTENSIONS

REM Build script: runs Unity in batch mode and calls the editor build method.
REM You can override UNITY_PATH by setting it as an environment variable before calling this script.
REM Example:
REM   set UNITY_PATH=C:\Program Files\Unity\Hub\Editor\2021.3.0f1\Editor\Unity.exe

if not defined UNITY_PATH (
  set UNITY_PATH=C:\Program Files\Unity\Hub\Editor\2021.3.0f1\Editor\Unity.exe
)

if not exist "%UNITY_PATH%" (
  echo [ERROR] Unity editor not found at:
  echo   %UNITY_PATH%
  echo Set the UNITY_PATH environment variable to your Unity.exe or edit Tools\build_windows.bat.
  exit /b 1
)

set LOG_PATH=%CD%\build_log.txt
echo Invoking Unity at "%UNITY_PATH%"...
"%UNITY_PATH%" -projectPath "%CD%" -quit -batchmode -buildTarget StandaloneWindows64 -executeMethod RogueLike2D.Editor.BuildScript.PerformWindowsBuild -logFile "%LOG_PATH%"

set EXITCODE=%ERRORLEVEL%
if %EXITCODE% NEQ 0 (
  echo [ERROR] Unity exited with code %EXITCODE%. See "%LOG_PATH%" for details.
  exit /b %EXITCODE%
)

echo Build finished successfully. See build_log.txt for details.
pause
