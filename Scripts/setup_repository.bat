

cd %~dp0

mkdir "../TF_Application/Assets/TouchFree/"
mklink /J "../TF_Application/Assets/TouchFree/Tooling" "../TF_Settings_and_Tooling_Unity/Assets/TouchFree/Tooling"

mklink /J "../TF_Settings_Web/src/TouchFree" "../TF_Tooling_Web/src"
