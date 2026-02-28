namespace TestMaster.Presentation.Services;

public interface IFileDownloadService
{
    Task<string> DownloadAllExamFilesAsync();
    Task<string?> DownloadAllExamFilesWithPickerAsync();
}

