namespace Backend.Infrastructure.Persistence;

public class ProveidorEntity
{
    public Guid Id{ get; set; }
    public string? Nombre{ get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Pais { get; set; }
    public string? Telefono { get; set; }
    public string? Fax { get; set; }
    public string? email { get; set; }  
    public string? nif_iva { get; set; }
    
}