using System.Text.Json.Serialization;

namespace OCRDesktop.Infrastructure.Models;

public class ErpPresupuestoResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("odoo_sale_order_id")]
    public int OdooSaleOrderId { get; set; }

    [JsonPropertyName("odoo_sale_order_name")]
    public string OdooSaleOrderName { get; set; } = string.Empty;
}

public class ErpOrdenResumen
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("numero")]
    public string Numero { get; set; } = string.Empty;

    [JsonPropertyName("fecha")]
    public DateTime Fecha { get; set; }

    [JsonPropertyName("cliente_nombre")]
    public string ClienteNombre { get; set; } = string.Empty;

    [JsonPropertyName("moneda")]
    public string Moneda { get; set; } = "EUR";

    [JsonPropertyName("total_ttc")]
    public decimal TotalTtc { get; set; }

    [JsonPropertyName("lineas")]
    public int Lineas { get; set; }

    public string FechaTexto => Fecha == DateTime.MinValue ? "Sin fecha" : Fecha.ToString("dd/MM/yyyy");
    public string TotalTexto => $"{TotalTtc:N2} {Moneda}";
}
