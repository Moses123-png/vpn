; Inno Setup script: WireGuardVpnManagerInstaller.iss
; Output to Output\MwesigwaMosesVPN-Setup.exe

[Setup]
AppName=Mwesigwa Moses VPN
AppVersion=1.0
DefaultDirName={pf}\MwesigwaMosesVPN
DefaultGroupName=Mwesigwa Moses VPN
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin
OutputDir=Output
OutputBaseFilename=MwesigwaMosesVPN-Setup

[Files]
; Adjust Source path to publish folder created by dotnet publish
Source: "WireGuardVpnManager/publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Mwesigwa Moses VPN"; Filename: "{app}\WireGuardVpnManager.exe"

[Run]
Filename: "{app}\WireGuardVpnManager.exe"; Description: "Launch Mwesigwa Moses VPN"; Flags: nowait postinstall skipifsilent
