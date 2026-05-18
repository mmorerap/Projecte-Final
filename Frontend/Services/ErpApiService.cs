using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using OCRDesktop.Infrastructure.Models;
using OCRDesktop.Services.Interfaces;

namespace OCRDesktop.Services;

public class ErpApiService : IErpApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:5100";

    public ErpApiService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<(bool success, string message, List<ErpOrdenResumen> ordenes)> GetOrdenesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/erp/ordenes");
            var body = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var ordenes = JsonSerializer.Deserialize<List<ErpOrdenResumen>>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ErpOrdenResumen>();
                return (true, "Órdenes cargadas correctamente.", ordenes);
            }

            try
            {
                var err = JsonSerializer.Deserialize<JsonElement>(body);
                if (err.TryGetProperty("message", out var m))
                {
                    return (false, m.GetString() ?? body, new List<ErpOrdenResumen>());
                }
            }
            catch { }

            return (false, $"Error {response.StatusCode}: {body}", new List<ErpOrdenResumen>());
        }
        catch (Exception ex)
        {
            return (false, "Error de conexión con API_Connecio_ERP: " + ex.Message, new List<ErpOrdenResumen>());
        }
    }

    public async Task<(bool success, string message, ErpPresupuestoResponse? data)> CrearPresupuestoAsync(string numeroOrden)
    {
        if (string.IsNullOrWhiteSpace(numeroOrden))
        {
            return (false, "Indica el número de orden guardado en la base de datos.", null);
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/erp/presupuesto", new { numero_orden = numeroOrden.Trim() });
            var body = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<ErpPresupuestoResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return (true, data?.Message ?? "OK", data);
            }

            try
            {
                var err = JsonSerializer.Deserialize<JsonElement>(body);
                if (err.TryGetProperty("message", out var m))
                {
                    return (false, m.GetString() ?? body, null);
                }
            }
            catch { }

            return (false, $"Error {response.StatusCode}: {body}", null);
        }
        catch (Exception ex)
        {
            return (false, "Error de conexión con API_Connecio_ERP: " + ex.Message, null);
        }
    }
}
