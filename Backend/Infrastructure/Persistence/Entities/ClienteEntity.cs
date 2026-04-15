namespace Backend.Infrastructure.Persistence.Entities;

public class ClienteEntity
{
    public Guid Id { get; set; }
    public string CodigoCliente { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string CodigoPostal { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string NifIva { get; set; } = string.Empty;
}
