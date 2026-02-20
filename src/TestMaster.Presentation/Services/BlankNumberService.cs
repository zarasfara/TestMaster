#if WINDOWS
using Windows.Storage.Pickers;
using WinRT.Interop;
#endif

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace TestMaster.Presentation.Services;

public class BlankNumberService : IBlankNumberService
{
    private const string BlankPrefix = "2 832503 19";
    private const int MaxBlankNumbers = 10000;

    public List<string> GenerateBlankNumbers(int count)
    {
        if (count <= 0 || count > MaxBlankNumbers)
        {
            throw new ArgumentException($"Количество должно быть от 1 до {MaxBlankNumbers}", nameof(count));
        }

        var blankNumbers = new List<string>(count);
        
        for (int i = 0; i < count; i++)
        {
            // Форматируем число с ведущими нулями (0000-9999)
            var suffix = i.ToString("D4");
            blankNumbers.Add($"{BlankPrefix}{suffix}");
        }

        return blankNumbers;
    }

    public async Task<string> GenerateBlankNumbersExcelAsync(int count)
    {
        var blankNumbers = GenerateBlankNumbers(count);
        
        var fileName = $"Blanks_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        var downloadsPath = GetDownloadsPath();
        var filePath = Path.Combine(downloadsPath, fileName);
        
        await Task.Run(() => CreateExcelFile(filePath, blankNumbers));
        
        return filePath;
    }

    public async Task<string?> GenerateBlankNumbersExcelWithPickerAsync(int count)
    {
        var blankNumbers = GenerateBlankNumbers(count);
        
        var folderPath = await PickFolderAsync();
        if (string.IsNullOrEmpty(folderPath))
        {
            return null; // Пользователь отменил выбор
        }
        
        var fileName = $"Blanks_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        var filePath = Path.Combine(folderPath, fileName);
        
        await Task.Run(() => CreateExcelFile(filePath, blankNumbers));
        
        return filePath;
    }

    private async Task<string?> PickFolderAsync()
    {
#if WINDOWS
        return await PickFolderWindowsAsync();
#elif ANDROID
        return await PickFolderAndroidAsync();
#else
        // Для iOS/MacCatalyst возвращаем папку Documents
        var documentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BlankNumbers");
        Directory.CreateDirectory(documentsPath);
        return documentsPath;
#endif
    }

#if WINDOWS
    private async Task<string?> PickFolderWindowsAsync()
    {
        var folderPicker = new Windows.Storage.Pickers.FolderPicker();
        
        // Получаем handle окна
        var window = Application.Current?.Windows[0];
        if (window?.Handler?.PlatformView is not MauiWinUIWindow winUIWindow)
        {
            return null;
        }
        
        var hwnd = winUIWindow.WindowHandle;
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);
        
        folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
        folderPicker.FileTypeFilter.Add("*");
        
        var folder = await folderPicker.PickSingleFolderAsync();
        return folder?.Path;
    }
#endif

#if ANDROID
    private async Task<string?> PickFolderAndroidAsync()
    {
        // Для Android используем стандартную папку Downloads
        var downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)?.AbsolutePath;
        if (!string.IsNullOrEmpty(downloadsPath))
        {
            var blankNumbersPath = Path.Combine(downloadsPath, "BlankNumbers");
            Directory.CreateDirectory(blankNumbersPath);
            return blankNumbersPath;
        }
        return null;
    }
#endif

    private void CreateExcelFile(string filePath, List<string> blankNumbers)
    {
        using var spreadsheetDocument = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook);
        
        // Добавляем WorkbookPart
        var workbookPart = spreadsheetDocument.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();
        
        // Добавляем WorksheetPart
        var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        worksheetPart.Worksheet = new Worksheet(new SheetData());
        
        // Добавляем Sheets в Workbook
        var sheets = spreadsheetDocument.WorkbookPart!.Workbook.AppendChild(new Sheets());
        var sheet = new Sheet
        {
            Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
            SheetId = 1,
            Name = "Номера бланков"
        };
        sheets.Append(sheet);
        
        var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>()!;
        
        // Добавляем заголовок
        var headerRow = new Row { RowIndex = 1 };
        var headerCell = new DocumentFormat.OpenXml.Spreadsheet.Cell
        {
            CellReference = "A1",
            DataType = CellValues.String,
            CellValue = new CellValue("Номер бланка")
        };
        headerRow.Append(headerCell);
        sheetData.Append(headerRow);
        
        // Добавляем данные
        for (int i = 0; i < blankNumbers.Count; i++)
        {
            var dataRow = new Row { RowIndex = (uint)(i + 2) };
            var dataCell = new DocumentFormat.OpenXml.Spreadsheet.Cell
            {
                CellReference = $"A{i + 2}",
                DataType = CellValues.String,
                CellValue = new CellValue(blankNumbers[i])
            };
            dataRow.Append(dataCell);
            sheetData.Append(dataRow);
        }
        
        workbookPart.Workbook.Save();
    }

    private string GetDownloadsPath()
    {
#if WINDOWS
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
#elif ANDROID
        var downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)?.AbsolutePath;
        return downloadsPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Downloads");
#elif IOS || MACCATALYST
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Downloads");
#else
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
#endif
    }
}





