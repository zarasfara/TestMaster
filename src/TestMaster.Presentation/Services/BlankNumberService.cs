using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Cell = DocumentFormat.OpenXml.Spreadsheet.Cell;

namespace TestMaster.Presentation.Services;

public class BlankNumberService : IBlankNumberService
{
    private const string BlankPrefix = "2 832503 19";
    private const int MaxBlankNumbers = 10000;

    public async Task<string?> ProcessParticipantsFileAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException($"Файл не найден: {filePath}");

            using var spreadsheetDocument = SpreadsheetDocument.Open(filePath, true);
            var workbookPart = spreadsheetDocument.WorkbookPart;

            if (workbookPart == null)
                throw new InvalidOperationException("Не удалось открыть файл как Excel документ");

            var worksheetPart = workbookPart.WorksheetParts.FirstOrDefault();

            if (worksheetPart == null) throw new InvalidOperationException("Не найдены листы в документе");

            var worksheet = worksheetPart.Worksheet;
            var sheetData = worksheet.GetFirstChild<SheetData>();

            if (sheetData == null) throw new InvalidOperationException("Не найдены данные в листе");

            var rows = sheetData.Elements<Row>().ToList();

            if (rows.Count < 2)
                throw new InvalidOperationException(
                    "Файл должен содержать хотя бы строку с данными (кроме заголовка)");

            var dataRowCount = rows.Count - 1;

            if (dataRowCount > 10000)
                throw new InvalidOperationException($"Слишком много участников ({dataRowCount}). Максимум: 10000");

            var blankCodes = GenerateBlankNumbers(dataRowCount);
            var codeIndex = 0;

            // Пропускаем первую строку (заголовок), обрабатываем остальные
            for (var i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                uint rowIndex = row.RowIndex ?? (uint)(i + 1);

                var cellC = row.Descendants<Cell>()
                    .FirstOrDefault(c => c.CellReference?.Value == $"C{rowIndex}");

                if (cellC == null)
                {
                    cellC = new Cell
                    {
                        CellReference = $"C{rowIndex}",
                        DataType = CellValues.String
                    };
                    cellC.CellValue = new CellValue(blankCodes[codeIndex]);
                    row.AppendChild(cellC);
                }
                else
                {
                    cellC.DataType = CellValues.String;

                    var oldValue = cellC.CellValue;
                    if (oldValue != null) cellC.RemoveChild(oldValue);

                    cellC.CellValue = new CellValue(blankCodes[codeIndex]);
                }

                codeIndex++;
            }

            workbookPart.Workbook.Save();

            return filePath;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Ошибка при обработке файла: {ex.Message}", ex);
        }
    }

    private List<string> GenerateBlankNumbers(int count)
    {
        if (count <= 0 || count > MaxBlankNumbers)
            throw new ArgumentException($"Количество должно быть от 1 до {MaxBlankNumbers}", nameof(count));

        var blankNumbers = new List<string>(count);

        for (var i = 0; i < count; i++)
        {
            var suffix = i.ToString("D4");
            blankNumbers.Add($"{BlankPrefix}{suffix}");
        }

        return blankNumbers;
    }
}