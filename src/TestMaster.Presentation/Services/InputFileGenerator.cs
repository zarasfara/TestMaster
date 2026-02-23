using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace TestMaster.Presentation.Services;

public static class InputFileGenerator
{
    public static void GenerateSampleInputFile(string filePath)
    {
        using var document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook);
        
        var workbookPart = document.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();
        
        var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        worksheetPart.Worksheet = new Worksheet(new SheetData());
        
        var sheets = workbookPart.Workbook.AppendChild(new Sheets());
        var sheet = new Sheet
        {
            Id = workbookPart.GetIdOfPart(worksheetPart),
            SheetId = 1,
            Name = "Участники"
        };
        sheets.Append(sheet);
        
        var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>()!;
        
        // Заголовок
        var headerRow = new Row { RowIndex = 1 };
        AppendCell(headerRow, "A1", "Номер бланка");
        AppendCell(headerRow, "B1", "ФИО");
        AppendCell(headerRow, "C1", "Школа");
        sheetData.Append(headerRow);
        
        // Тестовые данные
        var data = new[]
        {
            ("2 832503 190000", "Иван Иванович Иванов", "МКОУ СОШ №27"),
            ("2 832503 190001", "Петр Петрович Петров", "Гимназия №47"),
            ("2 832503 190002", "Сидоров Сидор Сидорович", "Лицей №5"),
        };
        
        for (int i = 0; i < data.Length; i++)
        {
            var row = new Row { RowIndex = (uint)(i + 2) };
            AppendCell(row, $"A{i + 2}", data[i].Item1);
            AppendCell(row, $"B{i + 2}", data[i].Item2);
            AppendCell(row, $"C{i + 2}", data[i].Item3);
            sheetData.Append(row);
        }
        
        workbookPart.Workbook.Save();
    }
    
    private static void AppendCell(Row row, string reference, string value)
    {
        var cell = new DocumentFormat.OpenXml.Spreadsheet.Cell
        {
            CellReference = reference,
            DataType = CellValues.String,
            CellValue = new CellValue(value)
        };
        row.Append(cell);
    }
}


