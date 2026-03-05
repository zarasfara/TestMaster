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

    private async Task<string?> SaveFileWindowsAsync(string suggestedFileName, string extension)
    {
#if WINDOWS
        var filePicker = new FileSavePicker();

        var window = Application.Current?.Windows[0];
        if (window?.Handler?.PlatformView is not MauiWinUIWindow winUIWindow)
        {
            return null;
        }

        var hwnd = winUIWindow.WindowHandle;
        InitializeWithWindow.Initialize(filePicker, hwnd);

        filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        filePicker.SuggestedFileName = Path.GetFileNameWithoutExtension(suggestedFileName);

        filePicker.FileTypeChoices.Add(
            extension.ToUpper().Replace(".", "") + " file",
            new List<string> { extension });

        var file = await filePicker.PickSaveFileAsync();

        return file?.Path;
#else
    return null;
#endif
    }

    public async Task<string?> SaveFileAsync(string suggestedFileName, string extension)
    {
#if WINDOWS
        return await SaveFileWindowsAsync(suggestedFileName, extension);
#else
    return null;
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

}

