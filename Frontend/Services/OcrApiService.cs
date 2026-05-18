using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OCRDesktop.Infrastructure;
using OCRDesktop.Infrastructure.Models;
using OCRDesktop.Services.Interfaces;

namespace OCRDesktop.Services;

public class OcrApiService : IOcrApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:5000";

    public OcrApiService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<(List<string> files, string path)> GetFilesToProcessAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/OCRservice/files");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(json);

            var files = result.GetProperty("files").EnumerateArray()
                .Select(x => x.GetString() ?? "")
                .ToList();
            var path = result.GetProperty("path").GetString() ?? "";

            return (files, path);
        }
        catch
        {
            return (new List<string>(), "Error conectando con la API");
        }
    }

    public async Task<List<OcrProcessResult>> ProcessFilesAsync(List<string> fileNames)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/OCRservice/process", new { fileNames });
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<OcrProcessResult>>() ?? new List<OcrProcessResult>();
        }
        catch
        {
            return new List<OcrProcessResult>();
        }
    }

    public async Task<OcrProcessResult?> GetPreviewAsync(string fileName)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/OCRservice/preview/{Uri.EscapeDataString(fileName)}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<OcrProcessResult>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool success, string message, bool movedToErronies)> SaveToDbAsync(object extractedData, string? sourceFileName = null)
    {
        try
        {
            var (payloadJson, buildError) = BuildOrdresPayload(extractedData, sourceFileName);
            if (buildError != null)
            {
                return (false, buildError, false);
            }

            using var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{BaseUrl}/OCRservice/ordres", content);

            if (response.IsSuccessStatusCode)
            {
                return (true, "Guardado correctamente", false);
            }

            var errorJson = await response.Content.ReadAsStringAsync();
            var (message, movedToErronies) = ParseErrorResponse(errorJson);
            if (message != null)
            {
                return (false, message, movedToErronies);
            }

            return (false, $"Error {response.StatusCode}: {errorJson}", false);
        }
        catch (Exception ex)
        {
            return (false, "Error de conexión: " + ex.Message, false);
        }
    }

    private static (string Json, string? Error) BuildOrdresPayload(object extractedData, string? sourceFileName)
    {
        var dto = OcrOrderMapper.ParseExtractedData(extractedData);

        if (dto == null)
        {
            return ("", "No se pudieron interpretar los datos extraídos del PDF.");
        }

        return (OcrOrderMapper.ToOrdresApiJson(dto, sourceFileName), null);
    }

    private static (string? Message, bool MovedToErronies) ParseErrorResponse(string errorJson)
    {
        if (string.IsNullOrWhiteSpace(errorJson))
        {
            return ("El servidor rechazó la petición (400). Comprueba que el backend esté actualizado.", false);
        }

        try
        {
            var errorObj = JsonSerializer.Deserialize<JsonElement>(errorJson);
            bool moved = errorObj.TryGetProperty("movedToErronies", out var movedEl)
                && movedEl.ValueKind == JsonValueKind.True;
            if (errorObj.TryGetProperty("message", out var msg))
            {
                return (msg.GetString() ?? "Error desconocido en el servidor", moved);
            }
            if (errorObj.TryGetProperty("title", out var title) && errorObj.TryGetProperty("detail", out var detail))
            {
                return ($"{title.GetString()}: {detail.GetString()}", moved);
            }
        }
        catch { }

        return (null, false);
    }

    public async Task<(bool success, string message)> FinalizeProcessAsync(string fileName)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/OCRservice/finalize", new { fileName });

            if (response.IsSuccessStatusCode)
            {
                return (true, "Finalizado correctamente");
            }

            var errorJson = await response.Content.ReadAsStringAsync();
            try
            {
                var errorObj = JsonSerializer.Deserialize<JsonElement>(errorJson);
                if (errorObj.TryGetProperty("detail", out var detail))
                {
                    return (false, detail.GetString() ?? "Error desconocido al finalizar");
                }
            }
            catch { }

            return (false, $"Error {response.StatusCode}: {errorJson}");
        }
        catch (Exception ex)
        {
            return (false, "Error de conexión: " + ex.Message);
        }
    }

    public async Task<bool> ClearHistoryAsync()
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/OCRservice/history");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
