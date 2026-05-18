using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.ServiceOCR
{
    public interface IOcrService
    {
        Task<IEnumerable<string>> ListFilesToProcessAsync();
        Task<OcrProcessResult> ProcessDocumentAsync(string fileName);
        Task<OcrProcessResult> GetPreviewAsync(string fileName);
        Task<IEnumerable<OcrProcessResult>> ProcessDocumentsInParallelAsync(IEnumerable<string> fileNames);
        Task<(bool Success, string Message)> FinalizeProcessAsync(string fileName);
        Task<(bool Success, string Message)> MoveToErroniesAsync(string fileName, string? errorDetail = null);
        Task<bool> ClearHistoryAsync();
    }

    public class OcrProcessResult
    {
        public string FileName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? JsonPath { get; set; }
        public object? ExtractedData { get; set; }
    }
}
