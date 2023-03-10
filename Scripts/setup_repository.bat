@echo off
if not "%1"=="1" (
    powershell -Command "Start-Process -Verb RunAs -FilePath '%0' -ArgumentList '1'"
    exit /b
)

cd %~dp0

mklink /J "../TF_Application/Assets/TouchFree" "../TF_Tooling_Unity/Assets/TouchFree"
mklink /J "../TF_Settings_Unity/Assets/TouchFree" "../TF_Tooling_Unity/Assets/TouchFree"
