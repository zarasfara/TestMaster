namespace TestMaster.Presentation.Services;

/// <summary>
/// Интерфейс для управления состоянием экзамена
/// </summary>
public interface IExamStateService
{
    /// <summary>
    /// Словарь ответов на вопросы (ключ - номер вопроса, значение - ответ)
    /// </summary>
    Dictionary<int, string> Answers { get; }

    /// <summary>
    /// Общее количество вопросов в экзамене
    /// </summary>
    int TotalQuestions { get; }
}

