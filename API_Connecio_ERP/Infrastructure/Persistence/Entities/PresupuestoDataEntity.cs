namespace API_Connecio_ERP.Infrastructure.Persistence.Entities;

public class PresupuestoDataEntity
{
    public Guid OrdenId { get; set; }
    public string NumeroOrden { get; set; } = string.Empty;
    public DateTime FechaOrden { get; set; }
    public string Moneda { get; set; } = "EUR";
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteNifIva { get; set; } = string.Empty;
    public string ClienteDireccion { get; set; } = string.Empty;
    public int Estado { get; set; }
    public List<PresupuestoLineaEntity> Lineas { get; set; } = new();
}

public class PresupuestoLineaEntity
{
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string CodigoCliente { get; set; } = string.Empty;
    public string CodigoProveedor { get; set; } = string.Empty;
}
