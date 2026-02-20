#if WINDOWS
using Windows.Storage.Pickers;
using WinRT.Interop;
#endif

namespace TestMaster.Presentation.Services;

public sealed class FileDownloadService : IFileDownloadService
{
    private static readonly string[] ExamFiles =
    [
        "3.ods",
        "9.ods",
        "10.odt",
        "17.txt",
        "18.ods",
        "22.ods",
        "24.txt",
        "26.txt",
        "27_A.txt",
        "27_B.txt"
    ];

    public async Task<string> DownloadAllExamFilesAsync()
    {
        try
        {
            var downloadsPath = GetDownloadsPath();

            foreach (var fileName in ExamFiles) await CopyFileToDownloadsAsync(fileName, downloadsPath);

            return downloadsPath;
        }
        catch (Exception ex)
        {
            // Логирование ошибки
            throw new InvalidOperationException("Не удалось скачать файлы", ex);
        }
    }

    public async Task<string?> DownloadAllExamFilesWithPickerAsync()
    {
        try
        {
            var folderPath = await PickFolderAsync();
            if (string.IsNullOrEmpty(folderPath))
            {
                return null;
            }

            foreach (var fileName in ExamFiles) await CopyFileToDownloadsAsync(fileName, folderPath);

            return folderPath;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Не удалось скачать файлы", ex);
        }
    }

    private async Task<string?> PickFolderAsync()
    {
#if WINDOWS
        return await PickFolderWindowsAsync();
#elif ANDROID
        return await PickFolderAndroidAsync();
#else
        // Для iOS/MacCatalyst возвращаем папку Documents
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ExamFiles");
#endif
    }

#if WINDOWS
    private async Task<string?> PickFolderWindowsAsync()
    {
        var folderPicker = new FolderPicker();

        // Получаем handle окна
        var window = Application.Current?.Windows[0];
        if (window?.Handler?.PlatformView is not MauiWinUIWindow winUiWindow)
        {
            return null;
        }

        var hwnd = winUiWindow.WindowHandle;
        InitializeWithWindow.Initialize(folderPicker, hwnd);

        folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
        folderPicker.FileTypeFilter.Add("*");

        var folder = await folderPicker.PickSingleFolderAsync();
        return folder?.Path;
    }
#endif

#if ANDROID
    private async Task<string?> PickFolderAndroidAsync()
    {
        // Для Android используем стандартную папку Downloads
        var downloadsPath =
 Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)?.AbsolutePath;
        if (!string.IsNullOrEmpty(downloadsPath))
        {
            var examFilesPath = Path.Combine(downloadsPath, "ExamFiles");
            Directory.CreateDirectory(examFilesPath);
            return examFilesPath;
        }
        return null;
    }
#endif


    private static async Task CopyFileToDownloadsAsync(string fileName, string destinationPath)
    {
        try
        {
            // Путь к файлу в wwwroot/files
            var sourceStream = await FileSystem.OpenAppPackageFileAsync($"wwwroot/files/{fileName}");

            var targetFile = Path.Combine(destinationPath, fileName);

            await using var fileStream = File.Create(targetFile);
            await sourceStream.CopyToAsync(fileStream);
        }
        catch (Exception)
        {
            // Пропускаем файлы, которые не удалось скопировать
        }
    }

    private static string GetDownloadsPath()
    {
#if WINDOWS
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
#elif ANDROID
        return Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)?.AbsolutePath 
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Downloads");
#elif IOS || MACCATALYST
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Downloads");
#else
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
#endif
    }
}