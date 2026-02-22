﻿using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Cell = DocumentFormat.OpenXml.Spreadsheet.Cell;

namespace TestMaster.Presentation.Services;

public class ExamResultService : IExamResultService
{
    private const string ResultsFolderName = "results";

    public async Task<LookupResult> TryLoadStudentAsync(string blankNumberInput, string excelFilePath)
    {
        var normalizedBlank = NormalizeBlankNumber(blankNumberInput);
        if (string.IsNullOrWhiteSpace(normalizedBlank))
        {
            return new LookupResult(false, "Номер бланка не указан", null);
        }

        if (string.IsNullOrWhiteSpace(excelFilePath))
        {
            return new LookupResult(false, "Не выбран файл с кодами учеников", null);
        }

        var inputPath = excelFilePath.Trim();
        if (!File.Exists(inputPath))
        {
            return new LookupResult(false, "Выбранный Excel-файл не найден", null);
        }

        try
        {
            using var document = SpreadsheetDocument.Open(inputPath, false);
            var workbookPart = document.WorkbookPart;
            if (workbookPart == null)
            {
                return new LookupResult(false, "Не удалось открыть input.xlsx", null);
            }

            var worksheetParts = workbookPart.WorksheetParts.ToList();
            if (worksheetParts.Count == 0)
            {
                return new LookupResult(false, "Не найден лист в input.xlsx", null);
            }

            foreach (var worksheetPart in worksheetParts)
            {
                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                if (sheetData == null)
                {
                    continue;
                }

                var rows = sheetData.Elements<Row>();
                foreach (var row in rows)
                {
                    var blankCell = GetCellByColumn(row, "A");
                    var fioCell = GetCellByColumn(row, "B");
                    var schoolCell = GetCellByColumn(row, "C");

                    var blankValue = NormalizeBlankNumber(GetCellValue(workbookPart, blankCell));
                    if (blankValue != normalizedBlank)
                    {
                        continue;
                    }

                    var fioValue = (GetCellValue(workbookPart, fioCell) ?? string.Empty).Trim();
                    var schoolValue = (GetCellValue(workbookPart, schoolCell) ?? string.Empty).Trim();

                    var (lastName, firstName, middleName) = ParseFullName(fioValue);

                    var student = new StudentInfo(
                        normalizedBlank,
                        lastName,
                        firstName,
                        middleName,
                        schoolValue);

                    return new LookupResult(true, null, student);
                }
            }

            return new LookupResult(false, "Номер бланка не найден", null);
        }
        catch (Exception ex)
        {
            return new LookupResult(false, $"Ошибка чтения input.xlsx: {ex.Message}", null);
        }
    }

    public async Task<SaveResult> SaveResultsAsync(StudentInfo student, Dictionary<int, string> answers)
    {
        var resultsPayload = new ExamResultPayload
        {
            BlankNumber = student.BlankNumber,
            LastName = student.LastName,
            FirstName = student.FirstName,
            MiddleName = student.MiddleName,
            School = student.School,
            Results = BuildResults(answers)
        };

        var fileName = BuildFileName(student);
        var resultsPath = Path.Combine(AppContext.BaseDirectory, ResultsFolderName);

        try
        {
            Directory.CreateDirectory(resultsPath);
            var fullPath = Path.Combine(resultsPath, fileName);

            var json = JsonSerializer.Serialize(resultsPayload, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            await File.WriteAllTextAsync(fullPath, json);
            return new SaveResult(true, null, false);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new SaveResult(false, ex.Message, true);
        }
        catch (Exception ex)
        {
            return new SaveResult(false, ex.Message, false);
        }
    }

    private static Dictionary<string, object> BuildResults(Dictionary<int, string> answers)
    {
        var results = new Dictionary<string, object>();

        foreach (var (taskId, rawAnswer) in answers.OrderBy(k => k.Key))
        {
            if (string.IsNullOrWhiteSpace(rawAnswer))
            {
                continue;
            }

            if (taskId == 25)
            {
                var pairs = BuildPairs(rawAnswer);
                if (pairs.Count > 0)
                {
                    results[taskId.ToString()] = pairs;
                }
                continue;
            }

            if (IsMultiInput(taskId))
            {
                var list = rawAnswer
                    .Split(',')
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .ToList();

                if (list.Count > 0)
                {
                    results[taskId.ToString()] = list;
                }

                continue;
            }

            results[taskId.ToString()] = new List<string> { rawAnswer.Trim() };
        }

        return results;
    }

    private static List<List<string>> BuildPairs(string rawAnswer)
    {
        var parts = rawAnswer
            .Split(',')
            .Select(p => p.Trim())
            .ToList();

        var pairs = new List<List<string>>();

        for (int i = 0; i < parts.Count; i += 2)
        {
            var first = parts[i];
            var second = i + 1 < parts.Count ? parts[i + 1] : string.Empty;

            if (string.IsNullOrWhiteSpace(first) && string.IsNullOrWhiteSpace(second))
            {
                continue;
            }

            pairs.Add(new List<string> { first, second });
        }

        return pairs;
    }

    private static bool IsMultiInput(int taskId)
    {
        return taskId is 17 or 18 or 20 or 26 or 27;
    }

    private static (string lastName, string firstName, string middleName) ParseFullName(string fullName)
    {
        var parts = Regex.Split(fullName.Trim(), "\\s+")
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();

        if (parts.Count == 0)
        {
            return (string.Empty, string.Empty, string.Empty);
        }

        if (parts.Count == 1)
        {
            return (parts[0], string.Empty, string.Empty);
        }

        if (parts.Count == 2)
        {
            return (parts[0], parts[1], string.Empty);
        }

        return (parts[0], parts[1], string.Join(" ", parts.Skip(2)));
    }

    private static string NormalizeBlankNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Concat(value.Where(c => !char.IsWhiteSpace(c)));
    }

    private static string BuildFileName(StudentInfo student)
    {
        var baseName = string.IsNullOrWhiteSpace(student.MiddleName)
            ? $"{student.BlankNumber}_{student.LastName}_{student.FirstName}"
            : $"{student.BlankNumber}_{student.LastName}_{student.FirstName}_{student.MiddleName}";

        var sanitized = new string(baseName
            .Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)
            .ToArray());

        return $"{sanitized}.json";
    }

    private static Cell? GetCellByColumn(Row row, string columnName)
    {
        return row.Elements<Cell>()
            .FirstOrDefault(c => string.Equals(GetColumnFromReference(c.CellReference?.Value), columnName, StringComparison.OrdinalIgnoreCase));
    }

    private static string? GetColumnFromReference(string? cellReference)
    {
        if (string.IsNullOrWhiteSpace(cellReference))
        {
            return null;
        }

        var letters = new string(cellReference.TakeWhile(char.IsLetter).ToArray());
        return string.IsNullOrWhiteSpace(letters) ? null : letters;
    }

    private static string? GetCellValue(WorkbookPart workbookPart, Cell? cell)
    {
        if (cell == null)
        {
            return null;
        }

        var value = cell.CellValue?.Text;
        if (cell.DataType == null)
        {
            return value;
        }

        if (cell.DataType.Value == CellValues.SharedString)
        {
            var stringTable = workbookPart.SharedStringTablePart?.SharedStringTable;
            if (stringTable == null)
            {
                return value;
            }

            if (int.TryParse(value, out var index))
            {
                return stringTable.ElementAt(index).InnerText;
            }
        }

        return value;
    }
}

