# Build-Msix.ps1 — собирает MSIX-пакет для TestMaster одной командой.
#
# Использование:
#   .\scripts\Build-Msix.ps1
#   .\scripts\Build-Msix.ps1 -Architecture x86
#   .\scripts\Build-Msix.ps1 -Configuration Debug
#
# Или дважды кликните build-msix.cmd в корне репозитория.

param(
    [string]$Configuration = "Release",
    [string]$Architecture  = "x64",
    [string]$CertSubject   = "CN=TestMaster"
)

$ErrorActionPreference = "Stop"

$ScriptDir   = Split-Path -Parent $MyInvocation.MyCommand.Definition
$RepoRoot    = Split-Path -Parent $ScriptDir
$ProjectPath = Join-Path $RepoRoot "src\TestMaster.Presentation\TestMaster.Presentation.csproj"
$TFM         = "net10.0-windows10.0.19041.0"
$CerPath     = Join-Path $ScriptDir "TestMaster.cer"

Write-Host ""
Write-Host "=== TestMaster — сборка MSIX ===" -ForegroundColor Cyan
Write-Host ""

# ── 1. Сертификат ──────────────────────────────────────────────────────────────
Write-Host "[1/3] Проверка сертификата..." -ForegroundColor Yellow

$cert = Get-ChildItem Cert:\CurrentUser\My |
        Where-Object { $_.Subject -eq $CertSubject } |
        Select-Object -First 1

if (-not $cert) {
    Write-Host "      Создаём самоподписанный сертификат '$CertSubject'..." -ForegroundColor Gray
    $cert = New-SelfSignedCertificate `
        -Type Custom `
        -Subject $CertSubject `
        -KeyUsage DigitalSignature `
        -FriendlyName "TestMaster MSIX" `
        -CertStoreLocation "Cert:\CurrentUser\My" `
        -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")
    Write-Host "      Создан: $($cert.Thumbprint)" -ForegroundColor Green
} else {
    Write-Host "      Используем существующий: $($cert.Thumbprint)" -ForegroundColor Green
}

Export-Certificate -Cert $cert -FilePath $CerPath -Force | Out-Null
Write-Host "      Сертификат сохранён: $CerPath" -ForegroundColor Gray

# ── 2. Сборка ──────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "[2/3] Сборка ($Configuration | $Architecture)..." -ForegroundColor Yellow
Write-Host ""

& dotnet publish $ProjectPath `
    -f $TFM `
    -c $Configuration `
    -p:RuntimeIdentifierOverride=win-$Architecture `
    -p:WindowsPackageType=MSIX `
    -p:AppxPackageSigningEnabled=true `
    -p:PackageCertificateThumbprint=$($cert.Thumbprint) `
    -p:WindowsAppSDKSelfContained=true

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ОШИБКА: dotnet publish завершился с кодом $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}

# ── 3. Результат ───────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "[3/3] Поиск готового пакета..." -ForegroundColor Yellow

$PackageRoot = Join-Path $RepoRoot "src\TestMaster.Presentation\bin\$Configuration\$TFM\win-$Architecture\AppPackages"
$MsixFile    = Get-ChildItem -Path $PackageRoot -Filter "*.msix" -Recurse -ErrorAction SilentlyContinue |
               Select-Object -First 1

if (-not $MsixFile) {
    Write-Host "ОШИБКА: .msix не найден в $PackageRoot" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "┌──────────────────────────────────────────────────────────────────────┐" -ForegroundColor Green
Write-Host "│  MSIX успешно собран!                                                │" -ForegroundColor Green
Write-Host "└──────────────────────────────────────────────────────────────────────┘" -ForegroundColor Green
Write-Host ""
Write-Host "  Пакет:      $($MsixFile.FullName)" -ForegroundColor White
Write-Host "  Сертификат: $CerPath" -ForegroundColor White
Write-Host ""
Write-Host "Для установки запустите от имени администратора:" -ForegroundColor Cyan
Write-Host "  .\scripts\Install-Msix.ps1" -ForegroundColor Yellow
Write-Host "Или дважды кликните install-msix.cmd (запуск от имени администратора)." -ForegroundColor Gray
Write-Host ""
