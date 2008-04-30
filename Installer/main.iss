; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

[Setup]
AppName=MySQL Connector/Net
AppVersion=5.1.6
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
Source: ..\Driver\bin\net-2.0\Release\MySql.Data.dll; DestDir: {app}\.NET Framework; Flags: ignoreversion; AfterInstall: AfterMySqlDataInstall
Source: ..\Driver\bin\net-2.0\Release\MySql.Data.CF.dll; DestDir: {app}\Compact Framework; Flags: ignoreversion; Components: CF
Source: ..\Documentation\Output\MySql.Data.chm; DestDir: {app}\Documentation; Flags: ignoreversion; Components: Docs
Source: ..\CHANGES; DestDir: {app}; Flags: ignoreversion
Source: ..\Release Notes.txt; DestDir: {app}; Flags: ignoreversion
Source: ..\MySql.Web\Providers\bin\release\MySql.Web.dll; DestDir: {app}\Web Providers; Flags: ignoreversion; AfterInstall: AfterWebInstall; Components: Providers

; Handle conditional licensing
#if defined (GPL)
Source: ..\COPYING; DestDir: {app}; Flags: ignoreversion
Source: ..\EXCEPTIONS; DestDir: {app}; Flags: ignoreversion
#else
Source: ..\License.txt; DestDir: {app}; Flags: ignoreversion
#endif

Source: ..\Samples\*.*; DestDir: {app}\Samples; Excludes: bin,obj,bin\debug,bin\release,obj\debug,obj\release; Flags: ignoreversion createallsubdirs recursesubdirs
Source: binary\installtools.dll; DestDir: {app}; Attribs: hidden

; Documentation files
Source: ..\Documentation\Output\MySql.Data.chm; DestDir: {app}\Documentation; Components: Docs
Source: ..\Documentation\CollectionFiles\COL_Master.HxC; DestDir: {app}\Documentation; Components: Docs
Source: ..\Documentation\CollectionFiles\COL_Master.HxT; DestDir: {app}\Documentation; Components: Docs
Source: ..\Documentation\CollectionFiles\COL_Master_A.HxK; DestDir: {app}\Documentation; Components: Docs
Source: ..\Documentation\CollectionFiles\COL_Master_F.HxK; DestDir: {app}\Documentation; Components: Docs
Source: ..\Documentation\CollectionFiles\COL_Master_K.HxK; DestDir: {app}\Documentation; Components: Docs
Source: ..\Documentation\CollectionFiles\COL_Master_N.HxK; DestDir: {app}\Documentation; Components: Docs
Source: ..\Documentation\Output\MySql.Data.HxS; DestDir: {app}\Documentation; Components: Docs

; Documentation registration tools
Source: ..\Installer\Binary\H2Reg.exe; DestDir: {app}\Documentation; Components: Docs
Source: ..\Installer\Binary\h2reg.ini; DestDir: {app}\Documentation; Components: Docs

; VS 2005 files
Source: ..\VisualStudio\bin\Release\MySql.VisualStudio.dll; DestDir: {app}\Visual Studio Integration; Components: VS/2005

[Icons]
Name: {group}\{cm:UninstallProgram,MySQL Connector Net}; Filename: {uninstallexe}
Name: {group}\Change Log; Filename: {app}\CHANGES
Name: {group}\Release Notes; Filename: {app}\Release Notes.txt
Name: {group}\Help; Filename: {app}\Documentation\MySql.Data.chm

[Components]
Name: Core; Description: Core assemblies; Flags: fixed; Types: full custom compact
Name: Docs; Description: Documentation; Types: full custom
Name: Providers; Description: ASP.NET 2.0 Web Providers; Types: full custom
Name: VS; Description: Visual Studio Integration; Types: full custom
Name: VS/2005; Description: Visual Studio 2005; Types: full custom; Check: VS2005Installed
Name: CF; Description: Compact Framework Support; Types: full custom
Name: Samples; Description: Samples; Types: full custom

[Registry]
Root: HKLM; Subkey: Software\MySQL AB\MySQL Connector/Net {#SetupSetting('AppVersion')}; Flags: uninsdeletekey
Root: HKLM; Subkey: Software\MySQL AB\MySQL Connector/Net {#SetupSetting('AppVersion')}; ValueType: string; ValueName: Location; ValueData: {app}
Root: HKLM; Subkey: Software\MySQL AB\MySQL Connector/Net {#SetupSetting('AppVersion')}; ValueType: string; ValueName: Version; ValueData: {#SetupSetting('AppVersion')}

; make our assembly visible to Visual Studio
Root: HKLM; Subkey: Software\Microsoft\.NETFramework\AssemblyFolders\MySQL Connector/Net {#SetupSetting('AppVersion')}; Flags: uninsdeletekey
Root: HKLM; Subkey: Software\Microsoft\.NETFramework\AssemblyFolders\MySQL Connector/Net {#SetupSetting('AppVersion')}; ValueType: string; ValueData: {app}\.NET Framework

#include "vs2005.iss"

[Run]
Filename: "{code:GetVersion2InstallUtil}"; Parameters: {app}\.NET Framework\mysql.data.dll; WorkingDir: {app}; StatusMsg: Adding data provider to machine.config; Flags: runhidden
Filename: "{code:GetVersion2InstallUtil}"; Parameters: {app}\Web Providers\mysql.web.dll; WorkingDir: {app}; StatusMsg: Adding web providers to machine.config; Flags: runhidden; Components: Providers
Filename: "{code:GetVS2005Path}"; Parameters: /setup; WorkingDir: {app}; StatusMsg: Installing Visual Studio 2005 support.  Please wait...; Flags: runhidden; Components: VS/2005
Filename: {app}\Documentation\h2reg.exe; Parameters: -r -q; WorkingDir: {app}\Documentation; StatusMsg: Registering help collection; Flags: runhidden; Components: docs and VS/2005

[UninstallRun]
Filename: "{code:GetVS2005Path}"; Parameters: /setup; WorkingDir: {app}; StatusMsg: Removing Visual Studio 2005 support; Flags: runhidden runascurrentuser; Components: VS/2005
Filename: "{code:GetVersion2InstallUtil}"; Parameters: /u {app}\.NET Framework\mysql.data.dll; WorkingDir: {app}; StatusMsg: Removing data provider from machine.config; Flags: runhidden
Filename: "{code:GetVersion2InstallUtil}"; Parameters: /u {app}\Web Providers\mysql.web.dll; WorkingDir: {app}; StatusMsg: Removing web providers from machine.config; Flags: runhidden; Components: Providers
Filename: {app}\Documentation\h2reg.exe; Parameters: -u -q; WorkingDir: {app}\Documentation; Flags: runhidden; Components: docs and VS/2005


[Code]
#include "misc.iss"

function InitializeSetup(): Boolean;
begin
  Result := true;
	if not CheckForFramework('2.0', true) then
		Result := false

  if PreviousVersionsInstalled() then
  begin
    MsgBox('There is already a version of Connector/Net installed.  ' +
           'Please uninstall all versions before installing this product.', mbError, MB_OK);
    Result := false
  end;
end;

procedure AfterMySqlDataInstall();
begin
    if Not RegisterAssembly(ExpandConstant('{app}' + '\.NET Framework\mysql.data.dll'), 2) then
      MsgBox('Registration of the Connector/Net core components failed.', mbError, MB_OK);
end;

procedure AfterWebInstall();
begin
    if Not RegisterAssembly(ExpandConstant('{app}' + '\Web Providers\mysql.web.dll'), 2) then
      MsgBox('Registration of the Connector/Net web components failed.', mbError, MB_OK);
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
  begin
    if Not UnRegisterAssembly(ExpandConstant('{app}' + '\.NET Framework\mysql.data.dll'), 'mysql.data', 2) then
      MsgBox('Unregistration of the Connector/Net core components failed.', mbError, MB_OK);

    if FileExists(ExpandConstant('{app}' + '\Web Providers\mysql.web.dll')) then
      if Not UnRegisterAssembly(ExpandConstant('{app}' + '\Web Providers\mysql.web.dll'), 'mysql.web', 2) then
        MsgBox('Unregistration of the Connector/Net web components failed.', mbError, MB_OK);

    // Now that we're finished with it, unload MyDll.dll from memory.
    // We have to do this so that the uninstaller will be able to remove the DLL and the {app} directory.
    UnloadDLL(ExpandConstant('{app}\installtools.dll'));
  end
end;

function VS2005Installed() : Boolean;
begin
  Result := RegKeyExists(HKEY_LOCAL_MACHINE, 'Software\Microsoft\VisualStudio\8.0\Setup\VS');
end;

function GetVersion2InstallUtil(Param: String) : String;
begin
  Result := GetInstallUtilPath(2);
end;


