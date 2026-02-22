namespace TestMaster.Presentation.Services;

public interface IExamResultService
{
    Task<LookupResult> TryLoadStudentAsync(string blankNumberInput, string excelFilePath);
    Task<SaveResult> SaveResultsAsync(StudentInfo student, Dictionary<int, string> answers);
}

