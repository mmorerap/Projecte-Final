using System.Text.Json.Serialization;

namespace API_Connecio_ERP.Infrastructure.DTO.Presupuestos;

public class PresupuestoRequest
{
    [JsonPropertyName("numero_orden")]
    public string NumeroOrden { get; set; } = string.Empty;
}

public class PresupuestoResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("odoo_sale_order_id")]
    public int OdooSaleOrderId { get; set; }

    [JsonPropertyName("odoo_sale_order_name")]
    public string OdooSaleOrderName { get; set; } = string.Empty;
}

public class OrdenResumenResponse
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
}
