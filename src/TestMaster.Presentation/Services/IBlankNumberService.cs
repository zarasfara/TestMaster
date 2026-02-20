namespace TestMaster.Presentation.Services;

public interface IBlankNumberService
{
    Task<string> GenerateBlankNumbersExcelAsync(int count);
    Task<string?> GenerateBlankNumbersExcelWithPickerAsync(int count);
    List<string> GenerateBlankNumbers(int count);
}


