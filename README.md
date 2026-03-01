# TestMaster

## Сборка и установка MSIX

### Шаг 1 — Собрать пакет (одним кликом)

Дважды кликните на **`build-msix.cmd`** в корне репозитория.

Или запустите вручную из PowerShell:

```powershell
.\scripts\Build-Msix.ps1
```

Скрипт автоматически:
1. Создаёт самоподписанный сертификат (или использует существующий `CN=TestMaster`).
2. Экспортирует сертификат в `scripts\TestMaster.cer`.
3. Запускает `dotnet publish` и формирует MSIX-пакет.

Готовый пакет находится здесь:

```
src\TestMaster.Presentation\bin\Release\net10.0-windows10.0.19041.0\win-x64\AppPackages\
```

---

### Шаг 2 — Установить пакет (одним кликом)

Дважды кликните на **`install-msix.cmd`** **от имени администратора**.

Или запустите вручную из PowerShell (от имени администратора):

```powershell
.\scripts\Install-Msix.ps1
```

Скрипт автоматически:
1. Устанавливает сертификат `scripts\TestMaster.cer` в хранилище «Доверенные корневые ЦС».
2. Устанавливает MSIX-пакет через `Add-AppxPackage`.

После установки откройте **меню Пуск**, найдите **TestMaster** и запустите.

---

### Параметры скриптов

| Параметр | По умолчанию | Описание |
|---|---|---|
| `-Configuration` | `Release` | Конфигурация сборки |
| `-Architecture` | `x64` | Архитектура (`x64` / `x86` / `arm64`) |
| `-CertSubject` | `CN=TestMaster` | Subject самоподписанного сертификата |

Пример сборки x86:

```powershell
.\scripts\Build-Msix.ps1 -Architecture x86
.\scripts\Install-Msix.ps1 -Architecture x86
```

---

### Ручная сборка (без скриптов)

```powershell
# 1. Создать сертификат и получить его thumbprint
$cert = New-SelfSignedCertificate -Type Custom -Subject "CN=TestMaster" `
    -KeyUsage DigitalSignature -CertStoreLocation "Cert:\CurrentUser\My" `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

# 2. Собрать MSIX
dotnet publish src\TestMaster.Presentation\TestMaster.Presentation.csproj `
    -f net10.0-windows10.0.19041.0 -c Release `
    -p:RuntimeIdentifierOverride=win-x64 `
    -p:WindowsPackageType=MSIX `
    -p:AppxPackageSigningEnabled=true `
    -p:PackageCertificateThumbprint=$($cert.Thumbprint) `
    -p:WindowsAppSDKSelfContained=true
```
