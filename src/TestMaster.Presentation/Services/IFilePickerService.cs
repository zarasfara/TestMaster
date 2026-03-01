namespace TestMaster.Presentation.Services;

public interface IFilePickerService
{
    Task<IReadOnlyList<string>> PickJsonFilesAsync(int maxFiles);
}

