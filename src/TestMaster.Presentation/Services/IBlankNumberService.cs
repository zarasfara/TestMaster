namespace TestMaster.Presentation.Services;

public interface IBlankNumberService
{
    Task<string?> ProcessParticipantsFileAsync(string filePath);
}