#if WINDOWS
using Windows.Storage.Pickers;
using WinRT.Interop;
#endif

namespace TestMaster.Presentation.Services;

public class FilePickerService : IFilePickerService
{
    public async Task<string?> PickExcelFileAsync()
    {
#if WINDOWS
        return await PickExcelFileWindowsAsync();
#elif ANDROID
        return await PickExcelFileAndroidAsync();
#else
        return null;
#endif
    }

    public async Task<IReadOnlyList<string>> PickJsonFilesAsync(int maxFiles)
    {
#if WINDOWS
        return await PickJsonFilesWindowsAsync(maxFiles);
#else
        return Array.Empty<string>();
#endif
    }

#if WINDOWS
    private async Task<string?> PickExcelFileWindowsAsync()
    {
        var filePicker = new FileOpenPicker();
        
        var window = Application.Current?.Windows[0];
        if (window?.Handler?.PlatformView is not MauiWinUIWindow winUIWindow)
        {
            return null;
        }
        
        var hwnd = winUIWindow.WindowHandle;
        InitializeWithWindow.Initialize(filePicker, hwnd);
        
        filePicker.SuggestedStartLocation = PickerLocationId.Downloads;
        filePicker.FileTypeFilter.Add(".xlsx");
        filePicker.FileTypeFilter.Add(".xls");
        
        var file = await filePicker.PickSingleFileAsync();
        return file?.Path;
    }

    private async Task<IReadOnlyList<string>> PickJsonFilesWindowsAsync(int maxFiles)
    {
        var filePicker = new FileOpenPicker();

        var window = Application.Current?.Windows[0];
        if (window?.Handler?.PlatformView is not MauiWinUIWindow winUIWindow)
        {
            return [];
        }

        var hwnd = winUIWindow.WindowHandle;
        InitializeWithWindow.Initialize(filePicker, hwnd);

        filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        filePicker.FileTypeFilter.Add(".json");

        var files = await filePicker.PickMultipleFilesAsync();
        if (files == null || files.Count == 0)
        {
            return [];
        }

        var limitedFiles = files
            .Take(Math.Max(1, maxFiles))
            .Select(f => f.Path)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .ToList();

        return limitedFiles;
    }
#endif

#if ANDROID
    private async Task<string?> PickExcelFileAndroidAsync()
    {
        // На Android используем встроенный файловый диалог через Activities
        var intent = new Android.Content.Intent(Android.Content.Intent.ActionGetContent);
        intent.SetType("application/*");
        intent.PutExtra(Android.Content.Intent.ExtraLocalOnly, true);
        
        try
        {
            // Это требует более сложной реализации через Activity результаты
            // Пока возвращаем null
            return null;
        }
        catch
        {
            return null;
        }
    }
#endif
}

