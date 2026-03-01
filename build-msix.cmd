@echo off
:: Одним кликом собирает MSIX-пакет TestMaster.
:: Не требует прав администратора.
echo.
echo Запуск сборки MSIX...
echo.
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\Build-Msix.ps1"
if %ERRORLEVEL% neq 0 (
    echo.
    echo Сборка завершилась с ошибкой. Код: %ERRORLEVEL%
    pause
    exit /b %ERRORLEVEL%
)
pause
