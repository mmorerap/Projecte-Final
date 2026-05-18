using System.Text.Json.Serialization;

namespace OCRDesktop.Infrastructure.Models;

public class OcrExtractedOrderDto
{
    [JsonPropertyName("cliente")]
    public OcrClienteDto? Cliente { get; set; }

    [JsonPropertyName("orden")]
    public OcrOrdenDto? Orden { get; set; }

    [JsonPropertyName("lineas")]
    public List<OcrLineaDto> Lineas { get; set; } = new();

    [JsonPropertyName("totales")]
    public OcrTotalesDto? Totales { get; set; }
}

public class OcrClienteDto
{
    [JsonPropertyName("nombre")]
    public string? Nombre { get; set; }

    [JsonPropertyName("direccion")]
    public string? Direccion { get; set; }

    [JsonPropertyName("ciudad")]
    public string? Ciudad { get; set; }

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

public class OcrOrdenDto
{
    [JsonPropertyName("numero")]
    public string? Numero { get; set; }

    [JsonPropertyName("fecha")]
    public string? Fecha { get; set; }

    [JsonPropertyName("modo_pago")]
    public string? ModoPago { get; set; }

    [JsonPropertyName("direccion_entrega")]
    public string? DireccionEntrega { get; set; }
}

public class OcrLineaDto
{
    [JsonPropertyName("codigo")]
    public string? Codigo { get; set; }

    [JsonPropertyName("codigo_producto")]
    public string? CodigoProducto { get; set; }

    [JsonPropertyName("codigo_proveedor")]
    public string? CodigoProveedor { get; set; }

    [JsonPropertyName("codigo_cliente")]
    public string? CodigoCliente { get; set; }

    [JsonPropertyName("descripcion")]
    public string? Descripcion { get; set; }

    [JsonPropertyName("cantidad")]
    public decimal Cantidad { get; set; }

    [JsonPropertyName("precio_unitario")]
    public decimal PrecioUnitario { get; set; }

    [JsonPropertyName("importe_ht")]
    public decimal? ImporteHt { get; set; }
}

public class OcrTotalesDto
{
    [JsonPropertyName("total_ht")]
    public decimal TotalHt { get; set; }

    [JsonPropertyName("total_iva")]
    public decimal TotalIva { get; set; }

    [JsonPropertyName("total_ttc")]
    public decimal TotalTtc { get; set; }

    [JsonPropertyName("moneda")]
    public string? Moneda { get; set; }
}
