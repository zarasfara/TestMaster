// Services/ExamStateService.cs
using System.Collections.Generic;

namespace KEGE_Emulator.Services;

public class ExamStateService
{
    public Dictionary<int, string> Answers { get; } = new();
    public int TotalQuestions => 27;
}