; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define CompanyURL "https://ultraleap.com"
#define ProductName "TouchFree"
#define Publisher "Ultraleap Inc."
#define SettingsUIExeName "TouchFreeSettingsUI.exe"
#define SettingsUIName "TouchFree Settings"
#define TouchFreeAppExeName "TouchFree.exe"
#define TouchFreeAppName "TouchFree"
#define TrayAppExeName "ServiceUITray.exe"
#define TrayAppName "TouchFree Service Control Panel"
#define WrapperExeName "ServiceWrapper.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{FE678A79-DE6B-4FC4-8160-F2D036CE27D2}
AppName={#ProductName}
AppVersion={#TouchFreeVersion}
AppVerName={#ProductName}
AppPublisher={#Publisher}
AppPublisherURL={#CompanyURL}
AppSupportURL={#CompanyURL}
AppUpdatesURL={#CompanyURL}
CreateUninstallRegKey=yes
DefaultDirName={autopf64}\Ultraleap\{#ProductName}
DisableProgramGroupPage=yes
LicenseFile={#SourcePath}..\..\EULA.rtf
SetupIconFile={#SourcePath}..\..\TF_Service_Utilities\TouchFree_Icon.ico
InfoBeforeFile={#SourcePath}..\..\PrivacyPolicy.rtf
UninstallDisplayIcon={app}\TouchFree\{#TouchFreeAppExeName}
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputDir="{#SourcePath}..\..\Installer_Build"
OutputBaseFilename={#ProductName}_{#TouchFreeVersion}_Installer
Compression=lzma
SolidCompression=yes
VersionInfoCompany={#Publisher}
VersionInfoProductName={#ProductName}
UpdateUninstallLogAppName=no
VersionInfoVersion={#TouchFreeVersion}
WizardStyle=modern
PrivilegesRequired=admin

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#SourcePath}..\..\Service_Package\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#SourcePath}..\..\TouchFree_Build\*"; DestDir: "{app}\TouchFree"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#SourcePath}..\..\Scripts\Tracking_Build\Tracking_for_TouchFree_{#TouchFreeVersion}.exe"; DestDir: "{app}\Tracking"; Flags: ignoreversion deleteafterinstall
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#TouchFreeAppName}"; Filename: "{app}\TouchFree\{#TouchFreeAppExeName}";
Name: "{autoprograms}\{#SettingsUIName}"; Filename: "{app}\SettingsUI\{#SettingsUIExeName}";
Name: "{autostartup}\{#TrayAppName}"; Filename: "{app}\Tray\{#TrayAppExeName}";

[Registry]
Root: HKA64; Subkey: "Software\Ultraleap"; Flags: uninsdeletekeyifempty
Root: HKA64; Subkey: "Software\Ultraleap\TouchFree"; Flags: uninsdeletekeyifempty
Root: HKA64; Subkey: "Software\Ultraleap\TouchFree\Service"; Flags: uninsdeletekey
Root: HKA64; Subkey: "Software\Ultraleap\TouchFree\Service\Settings"; ValueType: string; ValueName: "WrapperExePath"; ValueData: "{app}\Wrapper\{#WrapperExeName}"
Root: HKA64; Subkey: "SOFTWARE\WOW6432Node\Ultraleap\TrackingServiceInstall"; ValueType: dword; ValueName: "VisualiserInStartMenu"; ValueData: 0
Root: HKA64; Subkey: "SOFTWARE\WOW6432Node\Ultraleap\TrackingServiceInstall"; ValueType: dword; ValueName: "ControlPanelOnStartup"; ValueData: 0
Root: HKA64; Subkey: "SOFTWARE\WOW6432Node\Ultraleap\TrackingServiceInstall"; ValueType: dword; ValueName: "ControlPanelInStartMenu"; ValueData: 0

[Run]
Filename: "{app}\SettingsUI\{#SettingsUIExeName}"; Description: "Configure TouchFree"; Flags: runascurrentuser nowait postinstall skipifsilent
Filename: "{app}\Tray\{#TrayAppExeName}"; Flags: runhidden nowait;
Filename: "{app}\Wrapper\{#WrapperExeName}"; Parameters: "install"; Flags: runhidden
Filename: "net.exe"; Parameters: "start ""TouchFree Service"""; Flags: runhidden
Filename: "{app}\Tracking\Tracking_for_TouchFree_{#TouchFreeVersion}.exe"; Parameters: "/S /D={app}\Tracking"; Flags: runhidden

[UninstallRun]
Filename: "{cmd}"; Parameters: "/C taskkill /im ServiceUITray.exe /f /t"; RunOnceId: "StopTrayIconApp"; Flags: runhidden
Filename: "net.exe"; Parameters: "stop ""TouchFree Service"""; RunOnceId: "StopService"; Flags: runhidden
Filename: "powershell.exe"; Parameters: "Start-Process {app}\Tracking\Uninstall.exe /S -NoNewWindow -Wait"; WorkingDir: {app}; Flags: runhidden waituntilterminated
Filename: "{app}\Wrapper\{#WrapperExeName}"; Parameters: "uninstall"; RunOnceId: "UninstallService"; Flags: runhidden

[Code]
function GetWrapperPath: string;
var
  wrapperExePath: string;
  wrapperRegistryPath: String;
begin
  Result := '';
  wrapperRegistryPath := ExpandConstant('Software\Ultraleap\TouchFree\Service\Settings');
  wrapperExePath := '';
  if not RegQueryStringValue(HKLM64, wrapperRegistryPath, 'WrapperExePath', wrapperExePath) then
    RegQueryStringValue(HKCU64, wrapperRegistryPath, 'WrapperExePath', wrapperExePath);
  Result := wrapperExePath;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  ResultCode: integer;
  WrapperPath: string;
begin
  WrapperPath := GetWrapperPath();

  Log(WrapperPath);

  if CompareText(WrapperPath, '') > 0 then
  begin
    Exec('cmd', '/C taskkill /im ServiceUITray.exe /f /t', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Exec('net', 'stop "TouchFree Service"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Exec(ExpandConstant(WrapperPath), 'uninstall', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  end;

  // Proceed Setup
  Result := '';
end;

{ Scripts to uninstall a previous version before installing the new one from and article on StackOverflow: }
{ https://stackoverflow.com/questions/2000296/inno-setup-how-to-automatically-uninstall-previous-installed-version }

function GetUninstallString(): String;
var
  sUnInstPath: String;
  sUnInstallString: String;
begin
  sUnInstPath := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');
  sUnInstallString := '';
  if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString) then
    RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString);
  Result := sUnInstallString;
end;

{ ///////////////////////////////////////////////////////////////////// }
function IsUpgrade(): Boolean;
begin
  Result := (GetUninstallString() <> '');
end;

{ ///////////////////////////////////////////////////////////////////// }
function UnInstallOldVersion(): Integer;
var
  sUnInstallString: String;
  iResultCode: Integer;
begin
{ Return Values: }
{ 1 - uninstall string is empty }
{ 2 - error executing the UnInstallString }
{ 3 - successfully executed the UnInstallString }

  { default return value }
  Result := 0;

  { get the uninstall string of the old app }
  sUnInstallString := GetUninstallString();
  if sUnInstallString <> '' then begin
    sUnInstallString := RemoveQuotes(sUnInstallString);
    if Exec(sUnInstallString, '/SILENT /NORESTART /SUPPRESSMSGBOXES','', SW_HIDE, ewWaitUntilTerminated, iResultCode) then
      Result := 3
    else
      Result := 2;
  end else
    Result := 1;
end;

{ ///////////////////////////////////////////////////////////////////// }
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep=ssInstall) then
  begin
    if (IsUpgrade()) then
    begin
      UnInstallOldVersion();
    end;
  end;
end;
