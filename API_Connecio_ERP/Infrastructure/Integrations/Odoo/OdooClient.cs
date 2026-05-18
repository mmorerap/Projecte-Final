using System.Text;
using System.Text.Json;
using API_Connecio_ERP.Infrastructure.Persistence.Entities;

namespace API_Connecio_ERP.Infrastructure.Integrations.Odoo;

public class OdooClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _db;
    private readonly string _user;
    private readonly string _password;
    private readonly string _defaultProductCode;

    public OdooClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = (configuration["Odoo:BaseUrl"] ?? "http://localhost:8069").TrimEnd('/');
        _db = configuration["Odoo:Database"] ?? throw new Exception("Falta Odoo:Database en appsettings.json");
        _user = configuration["Odoo:Username"] ?? throw new Exception("Falta Odoo:Username en appsettings.json");
        _password = configuration["Odoo:Password"] ?? throw new Exception("Falta Odoo:Password en appsettings.json");
        _defaultProductCode = configuration["Odoo:DefaultProductCode"] ?? "SERVICIO";
    }

    public async Task<(int SaleOrderId, string SaleOrderName)> CrearPresupuestoAsync(PresupuestoDataEntity data)
    {
        int uid = await AuthenticateAsync();
        int partnerId = await EnsurePartnerAsync(uid, data);

        var orderLines = new List<object>();
        foreach (var linea in data.Lineas)
        {
            int productId = await ResolveProductIdAsync(uid, linea);
            orderLines.Add(new object[]
            {
                0,
                0,
                new Dictionary<string, object?>
                {
                    ["product_id"] = productId,
                    ["name"] = string.IsNullOrWhiteSpace(linea.Descripcion) ? "Línea sin descripción" : linea.Descripcion,
                    ["product_uom_qty"] = linea.Cantidad <= 0 ? 1 : linea.Cantidad,
                    ["price_unit"] = linea.PrecioUnitario
                }
            });
        }

        var orderIdObj = await ExecuteKwAsync(uid, "sale.order", "create", new object[]
        {
            new Dictionary<string, object?>
            {
                ["partner_id"] = partnerId,
                ["origin"] = data.NumeroOrden,
                ["client_order_ref"] = data.NumeroOrden,
                ["date_order"] = data.FechaOrden.ToString("yyyy-MM-dd HH:mm:ss"),
                ["currency_id"] = await ResolveCurrencyIdAsync(uid, data.Moneda),
                ["order_line"] = orderLines
            }
        });

        int orderId = ExtractInt(orderIdObj, "No se pudo crear el presupuesto en Odoo.");
        var name = await GetSaleOrderNameAsync(uid, orderId);
        return (orderId, name);
    }

    private async Task<int> AuthenticateAsync()
    {
        var response = await JsonRpcAsync("common", "authenticate", new object[] { _db, _user, _password, new { } });
        return ExtractInt(response, "Autenticación contra Odoo fallida.");
    }

    private async Task<int> EnsurePartnerAsync(int uid, PresupuestoDataEntity data)
    {
        var domain = new List<object>();
        if (!string.IsNullOrWhiteSpace(data.ClienteNifIva))
        {
            domain.Add(new object[] { "vat", "=", data.ClienteNifIva });
        }
        else
        {
            domain.Add(new object[] { "name", "=", data.ClienteNombre });
        }

        var existing = await ExecuteKwAsync(uid, "res.partner", "search", new object[] { domain, 0, 1 });
        int[] partnerIds = ExtractIntArray(existing);
        if (partnerIds.Length > 0)
        {
            return partnerIds[0];
        }

        var createRes = await ExecuteKwAsync(uid, "res.partner", "create", new object[]
        {
            new Dictionary<string, object?>
            {
                ["name"] = string.IsNullOrWhiteSpace(data.ClienteNombre) ? "Cliente API" : data.ClienteNombre,
                ["vat"] = string.IsNullOrWhiteSpace(data.ClienteNifIva) ? null : data.ClienteNifIva,
                ["street"] = string.IsNullOrWhiteSpace(data.ClienteDireccion) ? null : data.ClienteDireccion,
                ["customer_rank"] = 1
            }
        });

        return ExtractInt(createRes, "No se pudo crear el cliente en Odoo.");
    }

    private async Task<int> ResolveProductIdAsync(int uid, PresupuestoLineaEntity linea)
    {
        string[] candidateCodes = { linea.CodigoCliente, linea.CodigoProveedor, _defaultProductCode };

        foreach (var code in candidateCodes.Where(c => !string.IsNullOrWhiteSpace(c)))
        {
            var matchByCode = await ExecuteKwAsync(uid, "product.product", "search", new object[]
            {
                new object[] { new object[] { "default_code", "=", code } }, 0, 1
            });

            int[] idsByCode = ExtractIntArray(matchByCode);
            if (idsByCode.Length > 0)
            {
                return idsByCode[0];
            }
        }

        if (!string.IsNullOrWhiteSpace(linea.Descripcion))
        {
            var matchByName = await ExecuteKwAsync(uid, "product.product", "search", new object[]
            {
                new object[] { new object[] { "name", "=", linea.Descripcion } }, 0, 1
            });

            int[] idsByName = ExtractIntArray(matchByName);
            if (idsByName.Length > 0)
            {
                return idsByName[0];
            }
        }

        throw new Exception("No se encontró producto en Odoo para una línea.");
    }

    private async Task<int?> ResolveCurrencyIdAsync(int uid, string moneda)
    {
        if (string.IsNullOrWhiteSpace(moneda))
        {
            return null;
        }

        var currencySearch = await ExecuteKwAsync(uid, "res.currency", "search", new object[]
        {
            new object[] { new object[] { "name", "=", moneda.ToUpperInvariant() } }, 0, 1
        });

        int[] ids = ExtractIntArray(currencySearch);
        return ids.Length > 0 ? ids[0] : null;
    }

    private async Task<string> GetSaleOrderNameAsync(int uid, int orderId)
    {
        var readResult = await ExecuteKwAsync(uid, "sale.order", "read", new object[]
        {
            new object[] { orderId },
            new object[] { "name" }
        });

        if (readResult.ValueKind == JsonValueKind.Array && readResult.GetArrayLength() > 0)
        {
            var first = readResult[0];
            if (first.TryGetProperty("name", out var name))
            {
                return name.GetString() ?? string.Empty;
            }
        }

        return string.Empty;
    }

    private async Task<JsonElement> ExecuteKwAsync(int uid, string model, string method, object[] args)
    {
        return await JsonRpcAsync("object", "execute_kw", new object[] { _db, uid, _password, model, method, args });
    }

    private async Task<JsonElement> JsonRpcAsync(string service, string method, object[] args)
    {
        var payload = new
        {
            jsonrpc = "2.0",
            method = "call",
            @params = new { service, method, args },
            id = Guid.NewGuid().ToString("N")
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/jsonrpc")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        string rawJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(rawJson);
        if (doc.RootElement.TryGetProperty("error", out var error))
        {
            throw new Exception("Error de Odoo: " + BuildOdooErrorMessage(error));
        }

        return doc.RootElement.GetProperty("result").Clone();
    }

    private static string BuildOdooErrorMessage(JsonElement error)
    {
        string message = error.TryGetProperty("message", out var messageElement)
            ? messageElement.GetString() ?? "Error desconocido"
            : "Error desconocido";

        if (error.TryGetProperty("data", out var data))
        {
            string? detail = data.TryGetProperty("message", out var detailElement)
                ? detailElement.GetString()
                : null;
            string? name = data.TryGetProperty("name", out var nameElement)
                ? nameElement.GetString()
                : null;

            if (!string.IsNullOrWhiteSpace(detail))
            {
                return string.IsNullOrWhiteSpace(name)
                    ? detail
                    : $"{detail} ({name})";
            }
        }

        return message;
    }

    private static int ExtractInt(JsonElement element, string errorMessage)
    {
        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out int value))
        {
            return value;
        }

        throw new Exception(errorMessage);
    }

    private static int[] ExtractIntArray(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<int>();
        }

        return element.EnumerateArray()
            .Where(x => x.ValueKind == JsonValueKind.Number && x.TryGetInt32(out _))
            .Select(x => x.GetInt32())
            .ToArray();
    }
}
