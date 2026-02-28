using TestMaster.Presentation.Services;

namespace TestMaster.Presentation;

public partial class App
{
    public App()
    {
        InitializeComponent();
        EnsureInputFileExists();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage()) { Title = "TestMaster.Presentation" };
    }

    private static void EnsureInputFileExists()
    {
        try
        {
            var inputPath = Path.Combine(AppContext.BaseDirectory, "input.xlsx");
            if (!File.Exists(inputPath))
            {
                InputFileGenerator.GenerateSampleInputFile(inputPath);
                Console.WriteLine($"Создан тестовый файл: {inputPath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка создания input.xlsx: {ex.Message}");
        }
    }
}