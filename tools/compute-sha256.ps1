# compute-sha256.ps1
param(
    [string]$Path = ".\Output\MwesigwaMosesVPN-Setup.exe"
)

if (-not (Test-Path $Path)) {
    Write-Error "File not found: $Path"
    exit 1
}

$hash = Get-FileHash $Path -Algorithm SHA256
$txt = "$($hash.Hash)  $([System.IO.Path]::GetFileName($Path))"
$shaFile = "$($Path).sha256.txt"
$txt | Out-File -FilePath $shaFile -Encoding ascii
Write-Host "SHA256 written to $shaFile"
