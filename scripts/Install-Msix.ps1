# Install-Msix.ps1 — устанавливает сертификат и MSIX-пакет TestMaster.
#
# Требуется запуск от имени администратора!
#
# Использование:
#   .\scripts\Install-Msix.ps1
#   .\scripts\Install-Msix.ps1 -MsixPath "C:\path\to\TestMaster.msix" -CerPath "C:\path\to\TestMaster.cer"
#
# Или дважды кликните install-msix.cmd (запуск от имени администратора).
#
# Перед установкой убедитесь, что MSIX уже собран: .\scripts\Build-Msix.ps1

#Requires -RunAsAdministrator

param(
    [string]$MsixPath,
    [string]$CerPath,
    [string]$Configuration = "Release",
    [string]$Architecture  = "x64"
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$RepoRoot  = Split-Path -Parent $ScriptDir
$TFM       = "net10.0-windows10.0.19041.0"

Write-Host ""
Write-Host "=== TestMaster — установка MSIX ===" -ForegroundColor Cyan
Write-Host ""

# Автоопределение пути к сертификату
if (-not $CerPath) {
    $CerPath = Join-Path $ScriptDir "TestMaster.cer"
}

# Автоопределение пути к MSIX
if (-not $MsixPath) {
    $PackageRoot = Join-Path $RepoRoot "src\TestMaster.Presentation\bin\$Configuration\$TFM\win-$Architecture\AppPackages"
    $MsixFile    = Get-ChildItem -Path $PackageRoot -Filter "*.msix" -Recurse -ErrorAction SilentlyContinue |
                   Select-Object -First 1
    if ($MsixFile) { $MsixPath = $MsixFile.FullName }
}

if (-not (Test-Path $CerPath)) {
    Write-Host "ОШИБКА: сертификат не найден: $CerPath" -ForegroundColor Red
    Write-Host "Сначала запустите сборку: .\scripts\Build-Msix.ps1" -ForegroundColor Yellow
    exit 1
}

if (-not $MsixPath -or -not (Test-Path $MsixPath)) {
    Write-Host "ОШИБКА: .msix пакет не найден." -ForegroundColor Red
    Write-Host "Сначала запустите сборку: .\scripts\Build-Msix.ps1" -ForegroundColor Yellow
    exit 1
}

# ── 1. Установка сертификата ───────────────────────────────────────────────────
Write-Host "[1/2] Установка сертификата в хранилище 'Доверенные корневые ЦС'..." -ForegroundColor Yellow

$cert  = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new($CerPath)
$store = [System.Security.Cryptography.X509Certificates.X509Store]::new(
    [System.Security.Cryptography.X509Certificates.StoreName]::Root,
    [System.Security.Cryptography.X509Certificates.StoreLocation]::LocalMachine)
$store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
$store.Add($cert)
$store.Close()

Write-Host "      Готово. Отпечаток: $($cert.Thumbprint)" -ForegroundColor Green

# ── 2. Установка MSIX ─────────────────────────────────────────────────────────
Write-Host ""
Write-Host "[2/2] Установка пакета: $MsixPath" -ForegroundColor Yellow

Add-AppxPackage -Path $MsixPath

Write-Host ""
Write-Host "┌──────────────────────────────────────────────────────────────────────┐" -ForegroundColor Green
Write-Host "│  TestMaster успешно установлен!                                      │" -ForegroundColor Green
Write-Host "│  Найдите приложение в меню Пуск и запустите.                         │" -ForegroundColor Green
Write-Host "└──────────────────────────────────────────────────────────────────────┘" -ForegroundColor Green
Write-Host ""
