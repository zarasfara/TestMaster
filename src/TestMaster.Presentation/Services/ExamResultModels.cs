using System.Text.Json.Serialization;

namespace TestMaster.Presentation.Services;

public record StudentInfo(
    string BlankNumber,
    string LastName,
    string FirstName,
    string MiddleName,
    string School);

public record LookupResult(bool Success, string? Error, StudentInfo? Student);

public record SaveResult(bool Success, string? Error, bool IsPermissionError);

public class ExamResultPayload
{
    [JsonPropertyName("blank_number")]
    public string BlankNumber { get; init; } = string.Empty;

    [JsonPropertyName("last_name")]
    public string LastName { get; init; } = string.Empty;

    [JsonPropertyName("first_name")]
    public string FirstName { get; init; } = string.Empty;

    [JsonPropertyName("middle_name")]
    public string MiddleName { get; init; } = string.Empty;

    [JsonPropertyName("school")]
    public string School { get; init; } = string.Empty;

    [JsonPropertyName("results")]
    public Dictionary<string, object> Results { get; init; } = new();
}

