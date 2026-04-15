namespace Backend.Domain;

public class LineaOrdreDomain
{
    public string Descripcion { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; }
    public decimal PrecioNeto { get; set; }
    public decimal ImporteHT { get; set; }
    public decimal TVA { get; set; }
}
