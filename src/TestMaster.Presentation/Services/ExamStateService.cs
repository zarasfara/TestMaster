namespace TestMaster.Presentation.Services;

public class ExamStateService
{
    public Dictionary<int, string> Answers { get; } = new();
    public int TotalQuestions => 27;
}