using Backend.Infrastructure.Persistence;
namespace Backend.Infrastructure.DTO;

public record ProveidorResponse(Guid Id, string? Nombre, string? Direccion, string? Ciudad, string? CodigoPostal, string? Pais, string? Telefono, string? Fax, string? email, string? nif_iva) 
{
    public static ProveidorResponse FromCarritoCompras(ProveidorEntity proveidorEntity)
    {
        return new ProveidorResponse(   proveidorEntity.Id, 
                                        proveidorEntity.Nombre, 
                                        proveidorEntity.Direccion,
                                        proveidorEntity.Ciudad, 
                                        proveidorEntity.CodigoPostal, 
                                        proveidorEntity.Pais, 
                                        proveidorEntity.Telefono, 
                                        proveidorEntity.Fax,
                                        proveidorEntity.email,
                                        proveidorEntity.nif_iva);
    }
}
