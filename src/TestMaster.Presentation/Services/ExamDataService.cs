namespace TestMaster.Presentation.Services;

/// <summary>
/// Сервис для хранения данных о заданиях ЕГЭ по информатике.
/// Центральное место для правильных ответов, максимальных баллов и таблицы перевода баллов.
/// </summary>
public class ExamDataService : IExamDataService
{
    /// <summary>
    /// Правильные ответы для всех заданий (1-27)
    /// </summary>
    public IReadOnlyDictionary<int, string> CorrectAnswers { get; } = new Dictionary<int, string>
    {
        { 1, "52" },
        { 2, "zyxw" },
        { 3, "133228" },
        { 4, "16" },
        { 5, "26" },
        { 6, "251" },
        { 7, "123937" },
        { 8, "5058" },
        { 9, "901" },
        { 10, "13" },
        { 11, "257" },
        { 12, "999" },
        { 13, "191191255254" },
        { 14, "3367" },
        { 15, "24" },
        { 16, "15588" },
        { 17, "150,9930" },
        { 18, "2362,1205" },
        { 19, "124" },
        { 20, "127,128" },
        { 21, "132" },
        { 22, "12" },
        { 23, "68" },
        { 24, "2981" },
        { 25, "800004,400004,800009,114294,800013,266674,800024,400014,800033,61554" },
        { 26, "564,444" },
        { 27, "38471,61225,142058,25299" }
    };

    /// <summary>
    /// Максимальные баллы за каждое задание (1-27)
    /// </summary>
    public IReadOnlyDictionary<int, int> MaxPoints { get; } = new Dictionary<int, int>
    {
        { 1, 1 }, { 2, 1 }, { 3, 1 }, { 4, 1 }, { 5, 1 },
        { 6, 1 }, { 7, 1 }, { 8, 1 }, { 9, 1 }, { 10, 1 },
        { 11, 1 }, { 12, 1 }, { 13, 1 }, { 14, 1 }, { 15, 1 },
        { 16, 1 }, { 17, 1 }, { 18, 1 }, { 19, 1 }, { 20, 1 },
        { 21, 1 }, { 22, 1 }, { 23, 1 }, { 24, 1 }, { 25, 1 },
        { 26, 2 }, { 27, 2 }
    };

    /// <summary>
    /// Таблица перевода первичного балла во вторичный (по ФИПИ, 2025)
    /// </summary>
    public IReadOnlyDictionary<int, int> PrimaryToSecondary { get; } = new Dictionary<int, int>
    {
        { 0, 0 },
        { 1, 7 }, { 2, 14 }, { 3, 20 }, { 4, 27 }, { 5, 34 },
        { 6, 40 }, { 7, 43 }, { 8, 46 }, { 9, 48 }, { 10, 51 },
        { 11, 54 }, { 12, 56 }, { 13, 59 }, { 14, 62 }, { 15, 64 },
        { 16, 67 }, { 17, 70 }, { 18, 72 }, { 19, 75 }, { 20, 78 },
        { 21, 80 }, { 22, 83 }, { 23, 85 }, { 24, 88 },
        { 25, 90 }, { 26, 93 }, { 27, 95 }, { 28, 98 }, { 29, 100 }
    };

    /// <summary>
    /// Задания с частичными баллами (многочастные ответы)
    /// </summary>
    public IReadOnlySet<int> MultiPartTasks { get; } = new HashSet<int> { 17, 18, 20, 26, 27 };

    /// <summary>
    /// Максимальный первичный балл
    /// </summary>
    public int MaxPrimaryScore => 29;

    /// <summary>
    /// Максимальный вторичный балл
    /// </summary>
    public int MaxSecondaryScore => 100;

    /// <summary>
    /// Общее количество заданий
    /// </summary>
    public int TotalTasks => 27;

    /// <summary>
    /// Вычисляет балл за задание с учетом частичного зачета
    /// </summary>
    /// <param name="taskId">Номер задания (1-27)</param>
    /// <param name="userAnswer">Ответ пользователя</param>
    /// <returns>Кортеж (балл, правильный ответ)</returns>
    public (int score, string correctAnswer) CalculateTaskScore(int taskId, string userAnswer)
    {
        var correctAnswer = CorrectAnswers.GetValueOrDefault(taskId, "—");
        var maxPoint = MaxPoints.GetValueOrDefault(taskId, 0);

        if (string.IsNullOrWhiteSpace(userAnswer))
            return (0, correctAnswer);

        if (userAnswer.Equals(correctAnswer, StringComparison.OrdinalIgnoreCase))
            return (maxPoint, correctAnswer);

        // Частичный зачет для многочастных заданий
        if (MultiPartTasks.Contains(taskId))
        {
            var userParts = userAnswer.Split(',');
            var correctParts = correctAnswer.Split(',');
            
            if (userParts.Length == correctParts.Length)
            {
                var correctCount = 0;
                for (var i = 0; i < userParts.Length; i++)
                {
                    if (userParts[i].Trim().Equals(correctParts[i].Trim(), StringComparison.OrdinalIgnoreCase))
                        correctCount++;
                }

                return (correctCount, correctAnswer);
            }
        }

        return (0, correctAnswer);
    }

    /// <summary>
    /// Вычисляет первичный балл по всем ответам
    /// </summary>
    /// <param name="answers">Словарь ответов (номер задания -> ответ)</param>
    /// <returns>Первичный балл</returns>
    public int CalculatePrimaryScore(Dictionary<int, string> answers)
    {
        var total = 0;
        for (var i = 1; i <= TotalTasks; i++)
        {
            var userAnswer = answers.GetValueOrDefault(i, "");
            var (score, _) = CalculateTaskScore(i, userAnswer);
            total += score;
        }
        return total;
    }

    /// <summary>
    /// Конвертирует первичный балл во вторичный
    /// </summary>
    /// <param name="primaryScore">Первичный балл</param>
    /// <returns>Вторичный балл</returns>
    public int ConvertToSecondary(int primaryScore)
    {
        return PrimaryToSecondary.GetValueOrDefault(primaryScore, 0);
    }
}



