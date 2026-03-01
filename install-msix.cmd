@echo off
:: Одним кликом устанавливает MSIX-пакет TestMaster.
:: ТРЕБУЕТСЯ запуск от имени администратора!
echo.
echo Запуск установки MSIX (требуются права администратора)...
echo.
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\Install-Msix.ps1"
if %ERRORLEVEL% neq 0 (
    echo.
    echo Установка завершилась с ошибкой. Код: %ERRORLEVEL%
    pause
    exit /b %ERRORLEVEL%
)
pause
