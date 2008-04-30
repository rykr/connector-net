function InstallToGAC(filename: String; ver: Integer): Boolean;
external 'InstallToGAC@files:installtools.dll cdecl';

function RemoveFromGAC(name: String; ver: Integer): Boolean;
external 'RemoveFromGAC@{app}\installtools.dll cdecl uninstallonly';

function CheckForFramework(version: String; required: Boolean): Boolean;
var
    regkey : String;
    msg : String;
begin
	Result := true;
	regkey := Format('SOFTWARE\Microsoft\.NETFramework\policy\v%s', [version]);
	if not RegKeyExists(HKLM, regkey) then
	begin
		msg := Format('This setup requires .NET Framework %s.', [version]);
		MsgBox(msg, mbError, MB_OK);
		Result:=false;
	end
end;

function GetInstallUtilPath(version: Integer) : String;
var
  installroot : String;
begin
  if RegQueryStringValue(HKEY_LOCAL_MACHINE, 'Software\Microsoft\.NETFramework', 'InstallRoot', installroot) then
  begin
    if version = 2 then
      Result := Format('%s\v2.0.50727\installutil.exe', [installroot]);
    if version = 11 then
      Result := Format('%s\v1.1.4322\installutil.exe', [installroot]);
  end
  Log('Returning ' + Result + ' as path to installutil');
end;

function RegisterAssembly(name: String; version: Integer) : Boolean;
var
  ResultCode : Integer;
begin
    Result := true;
    Log(Format('Registering %s for version %d', [name, version]));

    // Install our assembly to the GAC now
    if Not InstallToGAC(name, version) then
    begin
      Log('Installing ' + name + ' into the GAC failed.');
      Result := false;
    end
end;

function UnRegisterAssembly(assemblyFile: String; assemblyName: String; version: Integer) : Boolean;
var
  ResultCode : Integer;
  InstallutilPath: String;
begin
    Result := true;
    Log(Format('Unregistering %s for version %d', [assemblyName, version]));

    // Remove our assembly from the GAC now
    if Not RemoveFromGAC(Format('%s, Version={#SetupSetting("AppVersion")}', [assemblyName]), version) then
    begin
      Log('Removing ' + assemblyName + ' from the GAC failed.');
      Result := false;
    end
    else
    begin
      InstallUtilPath := GetInstallUtilPath(version);

      Exec(InstallUtilPath, '/LogFile= /u "' + assemblyFile + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
      if ResultCode <> 0 then
      begin
        Log('Running remove methods in ' + assemblyName + ' failed.');
        Result := false;
      end
      else
        Log('Successfully unregistered ' + assemblyName);
    end
end;

function PreviousVersionsInstalled() : Boolean;
var
  Names: TArrayOfString;
  I : Integer;
begin
  Result := false;
  if RegGetSubkeyNames(HKEY_LOCAL_MACHINE, 'Software\MySQL AB', Names) then
  begin
    for I := 0 to GetArrayLength(Names)-1 do
      if Pos('MySQL Connector/Net', Names[I]) = 1 then
        Result := true;
  end
end;

function GetVS2005Path(Param: String) : String;
begin
  if Not RegQueryStringValue(HKEY_LOCAL_MACHINE, 'Software\Microsoft\VisualStudio\8.0\Setup\VS', 'EnvironmentDirectory', Result) Then
    RaiseException('Unable to locate the Visual Studio 2005 installation directory');
  Result := Format('%sdevenv.com', [Result]);
end;

function GetVS2008Path(Param: String) : String;
begin
  if Not RegQueryStringValue(HKEY_LOCAL_MACHINE, 'Software\Microsoft\VisualStudio\9.0\Setup\VS', 'EnvironmentDirectory', Result) Then
    RaiseException('Unable to locate the Visual Studio 2008 installation directory');
  Result := Format('%sdevenv.com', [Result]);
end;

