namespace TestMaster.Presentation.Services;

public interface IExamResultService
{
    Task<LookupResult> TryLoadStudentAsync(string studentIdInput);
    Task<SaveResult> SaveResultsAsync(StudentInfo student, Dictionary<int, string> answers);
}