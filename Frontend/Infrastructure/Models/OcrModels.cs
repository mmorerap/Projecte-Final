using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace OCRDesktop.Infrastructure.Models;

public partial class OcrFile : ObservableObject
{
    public string Name { get; set; } = string.Empty;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private bool _isProcessed;
}

public class OcrProcessResult
{
    public string FileName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? JsonPath { get; set; }
    public object? ExtractedData { get; set; }
}

public partial class ReviewClient : ObservableObject
{
    [ObservableProperty]
    [JsonPropertyName("nombre")]
    private string _nombre = string.Empty;

    [ObservableProperty]
    [JsonPropertyName("direccion")]
    private string _direccion = string.Empty;

    [ObservableProperty]
    [JsonPropertyName("ciudad")]
    private string _ciudad = string.Empty;

    [ObservableProperty]
    [JsonPropertyName("codigo_postal")]
    private string? _codigoPostal;

    [ObservableProperty]
    [JsonPropertyName("pais")]
    private string? _pais;

    [ObservableProperty]
    [JsonPropertyName("telefono")]
    private string? _telefono;

    [ObservableProperty]
    [JsonPropertyName("nif_iva")]
    private string? _nifIva;

    [ObservableProperty]
    [JsonPropertyName("codigo_cliente")]
    private string? _codigoCliente;
}

public partial class ReviewOrden : ObservableObject
{
    [ObservableProperty]
    [JsonPropertyName("numero")]
    private string _numero = string.Empty;

    [ObservableProperty]
    [JsonPropertyName("fecha")]
    private string _fecha = string.Empty;

    [ObservableProperty]
    [JsonPropertyName("modo_pago")]
    private string? _modoPago;

    [ObservableProperty]
    [JsonPropertyName("direccion_entrega")]
    private string? _direccionEntrega;
}

public partial class ReviewLinea : ObservableObject
{
    [ObservableProperty]
    [JsonPropertyName("descripcion")]
    private string _descripcion = string.Empty;

    [ObservableProperty]
    [JsonPropertyName("cantidad")]
    private decimal _cantidad;

    [ObservableProperty]
    [JsonPropertyName("precio_unitario")]
    private decimal _precioUnitario;

    [ObservableProperty]
    [JsonPropertyName("importe_ht")]
    private decimal _importeHT;

    [ObservableProperty]
    [JsonPropertyName("codigo_proveedor")]
    private string _codigoProducto = string.Empty;

    [ObservableProperty]
    [JsonPropertyName("codigo_cliente")]
    private string? _codigoCliente;
}

public partial class ReviewTotales : ObservableObject
{
    [ObservableProperty]
    [JsonPropertyName("total_ht")]
    private decimal _totalHT;

    [ObservableProperty]
    [JsonPropertyName("total_iva")]
    private decimal _totalIVA;

    [ObservableProperty]
    [JsonPropertyName("total_ttc")]
    private decimal _totalTTC;

    [ObservableProperty]
    [JsonPropertyName("moneda")]
    private string _moneda = "EUR";
}

public partial class OrderReview : ObservableObject
{
    [ObservableProperty]
    [JsonPropertyName("cliente")]
    private ReviewClient _cliente = new();

    [ObservableProperty]
    [JsonPropertyName("orden")]
    private ReviewOrden _orden = new();

    [ObservableProperty]
    [JsonPropertyName("lineas")]
    private ObservableCollection<ReviewLinea> _lineas = new();

    [ObservableProperty]
    [JsonPropertyName("totales")]
    private ReviewTotales _totales = new();
}
