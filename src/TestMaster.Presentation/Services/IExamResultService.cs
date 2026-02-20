namespace TestMaster.Presentation.Services;

public interface IExamResultService
{
    Task<LookupResult> TryLoadStudentAsync(string blankNumberInput);
    Task<SaveResult> SaveResultsAsync(StudentInfo student, Dictionary<int, string> answers);
}

