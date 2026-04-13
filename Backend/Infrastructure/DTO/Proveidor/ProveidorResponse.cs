using Backend.Infrastructure.Persistence;
//Demanar Informacio a la BDD
namespace dbdemo.DTO;

public record ProveidorResponse(Guid Id, string? Nombre, string? Direccion, string? Ciudad, string? CodigoPostal, string? Pais, string? Telefono, string? Fax, string? email, string? nif_iva) 
{
    // Guanyem CONTROL sobre com es fa la conversió

    public static ProveidorResponse FromCarritoCompras(ProveidorEntity proveidorEntity)   // Conversió de model a response
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
