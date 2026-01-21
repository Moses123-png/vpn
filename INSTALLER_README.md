Mwesigwa Moses VPN â€” Installer notes, SmartScreen, and verification

Unsigned installer
- The provided installer is unsigned (no code-signing certificate).
- Windows SmartScreen / Defender may show an "Unknown publisher" warning for unsigned installers.

How to verify the installer (recommended)
1. Download both:
   - MwesigwaMosesVPN-Setup.exe
   - MwesigwaMosesVPN-Setup.sha256.txt

2. Verify SHA256 locally (PowerShell):
   Get-FileHash .\MwesigwaMosesVPN-Setup.exe -Algorithm SHA256 | Format-List

3. Compare the printed hash to the contents of the downloaded .sha256.txt file.

SmartScreen guidance for end-users
- When SmartScreen warns "Windows protected your PC" â†’ Click "More info" â†’ Click "Run anyway" (if you verified checksum and trust the source).

Pre-install prerequisites
- Install WireGuard for Windows first: https://www.wireguard.com/install/
- The manager EXE requests Administrator privileges (to manage firewall and install tunnels).

Publishing notes (for maintainers)
- Publish the SHA256 checksum together with the installer (display on the release page).
- Later: obtain a code-signing certificate and sign the installer to avoid SmartScreen warnings.
