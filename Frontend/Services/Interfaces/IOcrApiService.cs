using OCRDesktop.Infrastructure.Models;

namespace OCRDesktop.Services.Interfaces;

public interface IOcrApiService
{
    Task<(List<string> files, string path)> GetFilesToProcessAsync();
    Task<List<OcrProcessResult>> ProcessFilesAsync(List<string> fileNames);
    Task<OcrProcessResult?> GetPreviewAsync(string fileName);
    Task<(bool success, string message, bool movedToErronies)> SaveToDbAsync(object extractedData, string? sourceFileName = null);
    Task<(bool success, string message)> FinalizeProcessAsync(string fileName);
    Task<bool> ClearHistoryAsync();
}
