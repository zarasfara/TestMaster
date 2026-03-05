namespace TestMaster.Presentation.Services;

public interface IFilePickerService
{
    Task<IReadOnlyList<string>> PickJsonFilesAsync(int maxFiles);

    Task<string?> SaveFileAsync(string suggestedFileName, string extension);
}

