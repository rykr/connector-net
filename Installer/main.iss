; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

[Setup]
AppName=MySQL Connector/Net
AppVersion=1.0.10.1
AppVerName=MySQL Connector/Net {#SetupSetting("AppVersion")}
AppPublisher=MySQL, Inc.
AppPublisherURL=http://www.mysql.com.com/
AppSupportURL=http://www.mysql.com.com/
AppUpdatesURL=http://www.mysql.com.com/
DefaultDirName={pf}\MySQL\MySQL Connector Net {#SetupSetting("AppVersion")}
DefaultGroupName=MySQL\MySQL Connector Net {#SetupSetting("AppVersion")}
AllowNoIcons=true
OutputBaseFilename=setup
Compression=lzma
SolidCompression=true
PrivilegesRequired=admin
WizardImageFile=Bitmaps\dlgbmp-is.bmp
WizardImageStretch=false
WizardSmallImageFile=compiler:wizmodernsmallimage-is.bmp
VersionInfoVersion={#SetupSetting("AppVersion")}

[Languages]
Name: english; MessagesFile: compiler:Default.isl

[Files]
Source: ..\mysqlclient\bin\net-1.1\Release\MySql.Data.dll; DestDir: {app}\Binaries\.NET 1.1; Flags: ignoreversion; AfterInstall: AfterMySqlDataInstall
Source: ..\doc\MySql.Data.chm; DestDir: {app}\Documentation; Flags: ignoreversion
Source: ..\CHANGES; DestDir: {app}; Flags: ignoreversion
Source: ..\README; DestDir: {app}; Flags: ignoreversion

; Handle conditional licensing
#if defined (GPL)
Source: ..\COPYING; DestDir: {app}; Flags: ignoreversion
Source: ..\EXCEPTIONS; DestDir: {app}; Flags: ignoreversion
#else
Source: ..\MySQLEULA.txt; DestDir: {app}; Flags: ignoreversion
#endif

Source: ..\Samples\*.*; DestDir: {app}\Samples; Excludes: bin,obj,bin\debug,bin\release,obj\debug,obj\release; Flags: ignoreversion createallsubdirs recursesubdirs

Source: binary\installtools.dll; DestDir: {app}; Attribs: hidden

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: {group}\{cm:UninstallProgram,MySQL Connector Net}; Filename: {uninstallexe}
Name: {group}\Change Log; Filename: {app}\CHANGES
Name: {group}\Help; Filename: {app}\Documentation\MySql.Data.chm

[Registry]
Root: HKLM; Subkey: Software\MySQL AB\MySQL Connector/Net {#SetupSetting("AppVersion")}; Flags: uninsdeletekey
Root: HKLM; Subkey: Software\MySQL AB\MySQL Connector/Net {#SetupSetting("AppVersion")}; ValueType: string; ValueName: Location; ValueData: {app}
Root: HKLM; Subkey: Software\MySQL AB\MySQL Connector/Net {#SetupSetting("AppVersion")}; ValueType: string; ValueName: Version; ValueData: {#SetupSetting('AppVersion')}

; make our assembly visible to Visual Studio
Root: HKLM; Subkey: Software\Microsoft\.NETFramework\AssemblyFolders\MySQL Connector/Net {#SetupSetting("AppVersion")}; Flags: uninsdeletekey
Root: HKLM; Subkey: Software\Microsoft\.NETFramework\AssemblyFolders\MySQL Connector/Net {#SetupSetting("AppVersion")}; ValueType: string; ValueData: "{app}\Binaries\.NET 1.1"

[Code]
#include "misc.iss"

function InitializeSetup(): Boolean;
begin
  Result := true;
	if not CheckForFramework('1.1', true) then
		Result := false
end;

procedure AfterMySqlDataInstall();
begin
    if Not RegisterAssembly(ExpandConstant('{app}' + '\Binaries\.NET 1.1\mysql.data.dll'), 11) then
      MsgBox('Registration of the Connector/Net core components failed.', mbError, MB_OK);
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
  begin
    if Not UnRegisterAssembly(ExpandConstant('{app}' + '\Binaries\.NET 1.1\mysql.data.dll'), 11) then
      MsgBox('Unregistration of the Connector/Net core components failed.', mbError, MB_OK);

    // Now that we're finished with it, unload MyDll.dll from memory.
    // We have to do this so that the uninstaller will be able to remove the DLL and the {app} directory.
    UnloadDLL(ExpandConstant('{app}\installtools.dll'));
  end
end;
