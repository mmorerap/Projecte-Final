using System.Text.Json.Serialization;

namespace Backend.Infrastructure.DTO.Ordres;

public class ClienteRequest
{
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [JsonPropertyName("direccion")]
    public string Direccion { get; set; } = string.Empty;

    [JsonPropertyName("ciudad")]
    public string Ciudad { get; set; } = string.Empty;

    [JsonPropertyName("codigo_postal")]
    public string? CodigoPostal { get; set; }

    [JsonPropertyName("pais")]
    public string? Pais { get; set; }

    [JsonPropertyName("telefono")]
    public string? Telefono { get; set; }

    [JsonPropertyName("nif_iva")]
    public string? NifIva { get; set; }

    [JsonPropertyName("codigo_cliente")]
    public string? CodigoCliente { get; set; }
}

public class OrdenInfoRequest
{
    [JsonPropertyName("numero")]
    public string Numero { get; set; } = string.Empty;

    [JsonPropertyName("fecha")]
    public string Fecha { get; set; } = string.Empty; // Usamos string por el formato dd/MM/yyyy

    [JsonPropertyName("fecha_recepcion")]
    public string? FechaRecepcion { get; set; }

    [JsonPropertyName("modo_pago")]
    public string? ModoPago { get; set; }

    [JsonPropertyName("gestionado_por")]
    public string? GestionadoPor { get; set; }

    [JsonPropertyName("direccion_entrega")]
    public string? DireccionEntrega { get; set; }
}

public class LineaOrdreRequest
{
    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; } = string.Empty;

    [JsonPropertyName("cantidad")]
    public decimal Cantidad { get; set; }

    [JsonPropertyName("precio_unitario")]
    public decimal PrecioUnitario { get; set; }

    [JsonPropertyName("descuento")]
    public decimal? Descuento { get; set; }

    [JsonPropertyName("importe_ht")]
    public decimal ImporteHT { get; set; }

    [JsonPropertyName("tva")]
    public decimal TVA { get; set; }
}

public class TotalesRequest
{
    [JsonPropertyName("total_ht")]
    public decimal TotalHT { get; set; }

    [JsonPropertyName("total_iva")]
    public decimal TotalIVA { get; set; }

    [JsonPropertyName("total_ttc")]
    public decimal TotalTTC { get; set; }

    [JsonPropertyName("moneda")]
    public string Moneda { get; set; } = "EUR";
}

public class OrdresRequest
{
    [JsonPropertyName("cliente")]
    public ClienteRequest Cliente { get; set; } = new();

    [JsonPropertyName("orden")]
    public OrdenInfoRequest Orden { get; set; } = new();

    [JsonPropertyName("lineas")]
    public List<LineaOrdreRequest> Lineas { get; set; } = new();

    [JsonPropertyName("totales")]
    public TotalesRequest Totales { get; set; } = new();
}
