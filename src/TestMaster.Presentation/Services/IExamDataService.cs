namespace TestMaster.Presentation.Services;

/// <summary>
/// Интерфейс сервиса для хранения данных о заданиях ЕГЭ по информатике.
/// </summary>
public interface IExamDataService
{
    /// <summary>
    /// Правильные ответы для всех заданий (1-27)
    /// </summary>
    IReadOnlyDictionary<int, string> CorrectAnswers { get; }

    /// <summary>
    /// Максимальные баллы за каждое задание (1-27)
    /// </summary>
    IReadOnlyDictionary<int, int> MaxPoints { get; }

    /// <summary>
    /// Таблица перевода первичного балла во вторичный (по ФИПИ, 2025)
    /// </summary>
    IReadOnlyDictionary<int, int> PrimaryToSecondary { get; }

    /// <summary>
    /// Задания с частичными баллами (многочастные ответы)
    /// </summary>
    IReadOnlySet<int> MultiPartTasks { get; }

    /// <summary>
    /// Максимальный первичный балл
    /// </summary>
    int MaxPrimaryScore { get; }

    /// <summary>
    /// Максимальный вторичный балл
    /// </summary>
    int MaxSecondaryScore { get; }

    /// <summary>
    /// Общее количество заданий
    /// </summary>
    int TotalTasks { get; }

    /// <summary>
    /// Вычисляет балл за задание с учетом частичного зачета
    /// </summary>
    /// <param name="taskId">Номер задания (1-27)</param>
    /// <param name="userAnswer">Ответ пользователя</param>
    /// <returns>Кортеж (балл, правильный ответ)</returns>
    (int score, string correctAnswer) CalculateTaskScore(int taskId, string userAnswer);

    /// <summary>
    /// Вычисляет первичный балл по всем ответам
    /// </summary>
    /// <param name="answers">Словарь ответов (номер задания -> ответ)</param>
    /// <returns>Первичный балл</returns>
    int CalculatePrimaryScore(Dictionary<int, string> answers);

    /// <summary>
    /// Конвертирует первичный балл во вторичный
    /// </summary>
    /// <param name="primaryScore">Первичный балл</param>
    /// <returns>Вторичный балл</returns>
    int ConvertToSecondary(int primaryScore);
}


