# Mwesigwa Moses VPN â€” Minimal & Secure Manager

What this is
- A WPF GUI (Mwesigwa Moses VPN) that manages WireGuard tunnels on Windows:
  - Import client `.conf` files
  - Install / Uninstall (start/stop) tunnels via wireguard.exe
  - Kill-switch using Windows Firewall COM API
  - Automatic monitoring & reconnect (auto-connect)
  - Auto-start on login (Run key)
  - Profile metadata (profiles.json)
  - Export logs
  - Inno Setup installer script (unsigned)
  - GitHub Actions workflow to build and publish unsigned installer (draft release)

Quick build (local)
1. Build / publish:
   dotnet publish WireGuardVpnManager/WireGuardVpnManager.csproj -c Release -r win-x64 --self-contained false -o publish

2. Build installer:
   "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" WireGuardVpnManagerInstaller.iss
   The installer will be output as Output\MwesigwaMosesVPN-Setup.exe

3. Compute SHA256:
   Get-FileHash .\Output\MwesigwaMosesVPN-Setup.exe -Algorithm SHA256 | Format-List

Support
- See INSTALLER_README.md for SmartScreen and verification guidance.
