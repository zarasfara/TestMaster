\# TestMaster



\## Публикация



В директории проекта:



```bash

dotnet publish -f net10.0-windows10.0.19041.0 -c Release -p:RuntimeIdentifierOverride=win-x64

```



Сформированная папка:

\\src\\TestMaster.Presentation\\bin\\Release\\net10.0-windows10.0.19041.0\\win-x64\\AppPackages\\TestMaster.Presentation\_0.0.1.0\_Test



\## Установка


На устройстве пользователя устанавливаем сертификат:

1. Двойным кликом: TestMaster.Presentation\_0.0.1.0\_x64.cer
2. "Установить сертификат..."
3. "Локальный компьютер"
4. "Поместить все сертификаты в следующее хранилище", "Обзор"
5. Выбираем "Доверенные корневые центры сертификации" ("Trusted Root Certification Authorities")
6. Далее, готово, ок



В PowerShell



```bash

Add-AppxPackage -Path .\\TestMaster.Presentation\_0.0.1.0\_x64.msix

```



Произойдет установка приложения, в меню Пуск найти и запустить







