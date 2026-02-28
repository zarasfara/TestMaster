using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Cell = DocumentFormat.OpenXml.Spreadsheet.Cell;

namespace TestMaster.Presentation.Services;

public class ExamResultService : IExamResultService
{
    private const string InputFileName = "input.xlsx";
    private const string ResultsFolderName = "results";
    private const string IdColumn = "A";
    private const string SubjectColumn = "D";
    private const string FioColumn = "E";
    private const string SchoolColumn = "F";
    private const string RequiredSubjectFragment = "информатик";

    private const string ErrorIdMissing = "ID не указан";
    private const string ErrorInputMissing = "Файл input.xlsx не найден";
    private const string ErrorInputOpen = "Не удалось открыть input.xlsx";
    private const string ErrorInputSheetMissing = "Не найден лист в input.xlsx";
    private const string ErrorIdNotFoundForSubject = "ID не найден для предмета «Информатика»";

    public Task<LookupResult> TryLoadStudentAsync(string studentIdInput)
    {
        var normalizedStudentId = NormalizeStudentId(studentIdInput);
        if (string.IsNullOrWhiteSpace(normalizedStudentId))
        {
            return Task.FromResult(FailedLookup(ErrorIdMissing));
        }

        var inputPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", InputFileName);
        if (!File.Exists(inputPath))
        {
            return Task.FromResult(FailedLookup(ErrorInputMissing));
        }

        try
        {
            using var document = SpreadsheetDocument.Open(inputPath, false);
            var workbookPart = document.WorkbookPart;
            if (workbookPart == null)
            {
                return Task.FromResult(FailedLookup(ErrorInputOpen));
            }

            var worksheetParts = workbookPart.WorksheetParts.ToList();
            if (worksheetParts.Count == 0)
            {
                return Task.FromResult(FailedLookup(ErrorInputSheetMissing));
            }

            var student = FindStudent(workbookPart, normalizedStudentId);
            if (student == null)
            {
                return Task.FromResult(FailedLookup(ErrorIdNotFoundForSubject));
            }

            return Task.FromResult(new LookupResult(true, null, student));
        }
        catch (Exception ex)
        {
            return Task.FromResult(FailedLookup($"Ошибка чтения input.xlsx: {ex.Message}"));
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

    private static LookupResult FailedLookup(string message)
    {
        return new LookupResult(false, message, null);
    }

    private static StudentInfo? FindStudent(WorkbookPart workbookPart, string normalizedStudentId)
    {
        foreach (var worksheetPart in workbookPart.WorksheetParts)
        {
            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            if (sheetData == null)
            {
                continue;
            }

            var rows = sheetData.Elements<Row>().ToList();
            if (rows.Count <= 1)
            {
                continue;
            }

            foreach (var row in rows.Skip(1))
            {
                var idCell = GetCellByColumn(row, IdColumn);
                var subjectCell = GetCellByColumn(row, SubjectColumn);
                var fioCell = GetCellByColumn(row, FioColumn);
                var schoolCell = GetCellByColumn(row, SchoolColumn);

                var idValue = NormalizeStudentId(GetCellValue(workbookPart, idCell));
                if (!string.Equals(idValue, normalizedStudentId, StringComparison.Ordinal))
                {
                    continue;
                }

                var subjectValue = GetCellValue(workbookPart, subjectCell);
                if (!IsInformaticsSubject(subjectValue))
                {
                    continue;
                }

                var fioValue = (GetCellValue(workbookPart, fioCell) ?? string.Empty).Trim();
                var schoolRawValue = (GetCellValue(workbookPart, schoolCell) ?? string.Empty).Trim();
                var schoolValue = ExtractSchoolNumber(schoolRawValue);
                var (lastName, firstName, middleName) = ParseFullName(fioValue);

                return new StudentInfo(
                    normalizedStudentId,
                    lastName,
                    firstName,
                    middleName,
                    schoolValue);
            }
        }

        return null;
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

        for (var i = 0; i < parts.Count; i += 2)
        {
            var first = parts[i];
            var second = i + 1 < parts.Count ? parts[i + 1] : string.Empty;

            if (string.IsNullOrWhiteSpace(first) && string.IsNullOrWhiteSpace(second))
            {
                continue;
            }

            pairs.Add([first, second]);
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

    private static string NormalizeStudentId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = string.Concat(value.Where(c => !char.IsWhiteSpace(c)));

        if (normalized.EndsWith(".0", StringComparison.Ordinal))
        {
            normalized = normalized[..^2];
        }

        return normalized;
    }

    private static bool IsInformaticsSubject(string? subjectValue)
    {
        if (string.IsNullOrWhiteSpace(subjectValue))
        {
            return false;
        }

        var normalized = subjectValue.Trim();
        return normalized.Contains(RequiredSubjectFragment, StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractSchoolNumber(string? schoolValue)
    {
        if (string.IsNullOrWhiteSpace(schoolValue))
        {
            return string.Empty;
        }

        var match = Regex.Match(schoolValue, "\\d+");
        if (!match.Success)
        {
            return string.Empty;
        }

        return match.Value;
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
            .FirstOrDefault(c =>
                string.Equals(GetColumnName(c.CellReference?.Value), columnName, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetColumnName(string? cellReference)
    {
        if (string.IsNullOrWhiteSpace(cellReference))
        {
            return string.Empty;
        }

        var letters = new string(cellReference.Where(char.IsLetter).ToArray());
        return letters;
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

        var dataType = cell.DataType.Value;

        if (dataType == CellValues.SharedString)
        {
            return GetSharedStringValue(workbookPart, value);
        }

        if (dataType == CellValues.InlineString)
        {
            return cell.InlineString?.Text?.Text ?? cell.InnerText;
        }

        if (dataType == CellValues.String)
        {
            return value ?? cell.InnerText;
        }

        return value;
    }

    private static string? GetSharedStringValue(WorkbookPart workbookPart, string? rawValue)
    {
        var stringTable = workbookPart.SharedStringTablePart?.SharedStringTable;
        if (stringTable == null)
        {
            return rawValue;
        }

        if (int.TryParse(rawValue, out var index))
        {
            return stringTable.ElementAt(index).InnerText;
        }

        return rawValue;
    }
}