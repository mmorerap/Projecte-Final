namespace Backend.Infrastructure.Persistence.Entities;

public class OrdreEntity
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public DateTime FechaRecepcion { get; set; }
    public string ModoPago { get; set; } = string.Empty;
    public string GestionadoPor { get; set; } = string.Empty;
    public string DireccionEntrega { get; set; } = string.Empty;
    public decimal TotalHT { get; set; }
    public decimal TotalIVA { get; set; }
    public decimal TotalTTC { get; set; }
    public string Moneda { get; set; } = "EUR";
    public Guid IdProveedor { get; set; }
    public Guid IdCliente { get; set; }
}
