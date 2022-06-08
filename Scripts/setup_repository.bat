@echo off
if not "%1"=="1" (
    powershell -Command "Start-Process -Verb RunAs -FilePath '%0' -ArgumentList '1'"
    exit /b
)

cd %~dp0

mkdir "../TF_Application/Assets/TouchFree/"
mklink /J "../TF_Application/Assets/TouchFree/Tooling" "../TF_Settings_and_Tooling_Unity/Assets/TouchFree/Tooling"
