namespace TestMaster.Presentation.Services;

public class ExamStateService : IExamStateService
{
    public Dictionary<int, string> Answers { get; } = new();
    public int TotalQuestions => 27;

    public string? BlankNumber { get; set; }
    public StudentInfo? Student { get; set; }
}