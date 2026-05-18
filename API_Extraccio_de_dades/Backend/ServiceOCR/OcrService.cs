using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Backend.ServiceOCR
{
    public class OcrService : IOcrService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _toProcessPath;
        private readonly string _processedPath;
        private readonly string _jsonPath;
        private readonly string _jsonProcessedPath;
        private readonly string _erroniesPath;
        private readonly string _systemPrompt;

        public OcrService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey not found");

            var storageBase = configuration["Storage:BasePath"] ?? "Storage";
            _toProcessPath = Path.Combine(storageBase, "ToProcess");
            _processedPath = Path.Combine(storageBase, "Processed");
            _jsonPath = Path.Combine(storageBase, "JSON");
            _jsonProcessedPath = Path.Combine(storageBase, "JSON_Processed");
            _erroniesPath = Path.Combine(storageBase, "Erronies");

            Directory.CreateDirectory(_toProcessPath);
            Directory.CreateDirectory(_processedPath);
            Directory.CreateDirectory(_jsonPath);
            Directory.CreateDirectory(_jsonProcessedPath);
            Directory.CreateDirectory(_erroniesPath);

            var promptPath = Path.Combine(AppContext.BaseDirectory, "ServiceOCR", "SystemPrompt.txt");
            if (File.Exists(promptPath))
            {
                _systemPrompt = File.ReadAllText(promptPath);
            }
            else
            {
                var fallbackPath = Path.Combine(Directory.GetCurrentDirectory(), "ServiceOCR", "SystemPrompt.txt");
                _systemPrompt = File.Exists(fallbackPath) ? File.ReadAllText(fallbackPath) : "Eres un extractor de datos.";
            }
        }

        public async Task<IEnumerable<string>> ListFilesToProcessAsync()
        {
            if (!Directory.Exists(_toProcessPath)) return Enumerable.Empty<string>();

            return await Task.Run(() =>
                Directory.GetFiles(_toProcessPath)
                         .Select(Path.GetFileName)
                         .Where(f => f != null)
                         .Cast<string>());
        }

        public async Task<OcrProcessResult> ProcessDocumentAsync(string fileName)
        {
            var result = new OcrProcessResult { FileName = fileName };
            var filePath = Path.Combine(_toProcessPath, fileName);

            if (!File.Exists(filePath))
            {
                result.ErrorMessage = "El archivo no existe.";
                return result;
            }

            try
            {
                var fileId = await UploadFileToOpenAiAsync(filePath);

                var jsonContent = await GetChatCompletionAsync(fileId);

                var jsonFileName = Path.GetFileNameWithoutExtension(fileName) + ".json";
                var finalJsonPath = Path.Combine(_jsonPath, jsonFileName);
                await File.WriteAllTextAsync(finalJsonPath, jsonContent);


                result.Success = true;
                result.JsonPath = finalJsonPath;
                result.ExtractedData = JsonSerializer.Deserialize<object>(jsonContent);

                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        public async Task<IEnumerable<OcrProcessResult>> ProcessDocumentsInParallelAsync(IEnumerable<string> fileNames)
        {
            var tasks = fileNames.Select(ProcessDocumentAsync);
            return await Task.WhenAll(tasks);
        }

        public async Task<OcrProcessResult> GetPreviewAsync(string fileName)
        {
            var result = new OcrProcessResult { FileName = fileName };
            var jsonFileName = Path.Combine(_jsonPath, Path.GetFileNameWithoutExtension(fileName) + ".json");

            if (File.Exists(jsonFileName))
            {
                try
                {
                    var jsonContent = await File.ReadAllTextAsync(jsonFileName);
                    result.Success = true;
                    result.JsonPath = jsonFileName;
                    result.ExtractedData = JsonSerializer.Deserialize<object>(jsonContent);
                    return result;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = "Error al leer la previsualización: " + ex.Message;
                    return result;
                }
            }

            return await ProcessDocumentAsync(fileName);
        }

        private async Task<string> UploadFileToOpenAiAsync(string filePath)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/files");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");

            content.Add(fileContent, "file", Path.GetFileName(filePath));
            content.Add(new StringContent("user_data"), "purpose");

            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            return doc.RootElement.GetProperty("id").GetString()!;
        }

        private async Task<string> GetChatCompletionAsync(string fileId)
        {
            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new object[]
                {
                    new { role = "system", content = _systemPrompt },
                    new { role = "user", content = new object[]
                        {
                            new { type = "file", file = new { file_id = fileId } },
                            new { type = "text", text = "Extrae toda la información de esta orden de compra y devuélvela en JSON siguiendo exactamente la estructura indicada. Está dirigido a BOSS AUTO." }
                        }
                    }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            var content = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()!;

            return content;
        }

        public async Task<(bool Success, string Message)> FinalizeProcessAsync(string fileName)
        {
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmm");
            var newFileName = $"{nameWithoutExt}-{timestamp}{extension}";

            var sourcePath = Path.Combine(_toProcessPath, fileName);
            var destPath = Path.Combine(_processedPath, newFileName);

            var jsonFileName = nameWithoutExt + ".json";
            var jsonSourcePath = Path.Combine(_jsonPath, jsonFileName);
            var newJsonFileName = $"{nameWithoutExt}-{timestamp}.json";
            var jsonDestPath = Path.Combine(_jsonProcessedPath, newJsonFileName);

            if (!File.Exists(sourcePath))
            {
                return (false, $"El archivo '{fileName}' no se encontró en la carpeta de origen '{_toProcessPath}'.");
            }

            int maxAttempts = 5;
            int delayMs = 500;
            Exception? lastException = null;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await Task.Run(() => File.Move(sourcePath, destPath, true));

                    if (File.Exists(jsonSourcePath))
                    {
                        try 
                        {
                            await Task.Run(() => File.Move(jsonSourcePath, jsonDestPath, true));
                        }
                        catch { }
                    }

                    return (true, "Archivo movido correctamente.");
                }
                catch (IOException ex)
                {
                    lastException = ex;
                    if (attempt < maxAttempts)
                    {
                        await Task.Delay(delayMs);
                    }
                }
                catch (Exception ex)
                {
                    return (false, $"Error fatal al mover el archivo: {ex.Message}");
                }
            }

            return (false, $"No se pudo mover el archivo tras {maxAttempts} intentos. Es posible que esté abierto en otro programa. Error: {lastException?.Message}");
        }

        public async Task<(bool Success, string Message)> MoveToErroniesAsync(string fileName, string? errorDetail = null)
        {
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmm");
            var newFileName = $"{nameWithoutExt}-{timestamp}{extension}";

            var sourcePath = Path.Combine(_toProcessPath, fileName);
            var destPath = Path.Combine(_erroniesPath, newFileName);

            var jsonFileName = nameWithoutExt + ".json";
            var jsonSourcePath = Path.Combine(_jsonPath, jsonFileName);
            var newJsonFileName = $"{nameWithoutExt}-{timestamp}.json";
            var jsonDestPath = Path.Combine(_erroniesPath, newJsonFileName);

            if (!File.Exists(sourcePath))
            {
                return (false, $"El archivo '{fileName}' no se encontró en '{_toProcessPath}'.");
            }

            int maxAttempts = 5;
            int delayMs = 500;
            Exception? lastException = null;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await Task.Run(() => File.Move(sourcePath, destPath, true));

                    if (File.Exists(jsonSourcePath))
                    {
                        try
                        {
                            await Task.Run(() => File.Move(jsonSourcePath, jsonDestPath, true));
                        }
                        catch { }
                    }

                    if (!string.IsNullOrWhiteSpace(errorDetail))
                    {
                        var notePath = Path.Combine(_erroniesPath, $"{nameWithoutExt}-{timestamp}_error.txt");
                        try
                        {
                            await File.WriteAllTextAsync(notePath, errorDetail.Trim());
                        }
                        catch { }
                    }

                    return (true, "Archivo movido a Erronies.");
                }
                catch (IOException ex)
                {
                    lastException = ex;
                    if (attempt < maxAttempts)
                    {
                        await Task.Delay(delayMs);
                    }
                }
                catch (Exception ex)
                {
                    return (false, $"Error al mover a Erronies: {ex.Message}");
                }
            }

            return (false, $"No se pudo mover a Erronies tras {maxAttempts} intentos. {lastException?.Message}");
        }

        public async Task<bool> ClearHistoryAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    if (Directory.Exists(_processedPath))
                    {
                        var files = Directory.GetFiles(_processedPath);
                        foreach (var f in files) File.Delete(f);
                    }

                    if (Directory.Exists(_jsonPath))
                    {
                        var files = Directory.GetFiles(_jsonPath);
                        foreach (var f in files) File.Delete(f);
                    }

                    if (Directory.Exists(_jsonProcessedPath))
                    {
                        var files = Directory.GetFiles(_jsonProcessedPath);
                        foreach (var f in files) File.Delete(f);
                    }
                });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
