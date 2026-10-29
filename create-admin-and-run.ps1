$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$env:DOTNET_ROOT = Join-Path $root ".tools\dotnet"
$env:PATH = "$env:DOTNET_ROOT;$env:DOTNET_ROOT\tools;$env:PATH"
if (-not (Test-Path (Join-Path $env:DOTNET_ROOT "dotnet.exe"))) {
    Write-Host "Run setup-portable-dotnet.cmd first." -ForegroundColor Yellow
    exit 1
}
$env:ConnectionStrings__DefaultConnection = Read-Host "Paste MySQL connection string (password stays in this terminal session)"
$env:ADMIN_EMAIL = Read-Host "Admin email"
$secure = Read-Host "Admin password" -AsSecureString
$env:ADMIN_PASSWORD = [System.Net.NetworkCredential]::new("", $secure).Password
Set-Location $root
dotnet restore
dotnet run
