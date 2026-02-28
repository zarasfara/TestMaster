namespace TestMaster.Presentation.Services;

public interface IFilePickerService
{
    Task<string?> PickExcelFileAsync();
}

