using Backend.Domain;
using Backend.Infrastructure.DTO;
using Backend.Infrastructure.Persistence;

namespace Backend.Mappers;

public static class ProveedorMapper
{
    public static ProveidorDomain ToDomain(ProveedorRequest req)
    {
        return new ProveidorDomain
        {
            Nombre = req.Nombre,
            Direccion = req.Direccion,
            Ciudad = req.Ciudad,
            CodigoPostal = req.CodigoPostal,
            Pais = req.Pais,
            Telefono = req.Telefono
        };
    }

     public static ProveidorEntity ToEntity(ProveidorDomain domain, Guid id, string fax)
    {
        return new ProveidorEntity
        {
            Id = id,
            Nombre = domain.Nombre,
            Direccion = domain.Direccion,
            Ciudad = domain.Ciudad,
            CodigoPostal = domain.CodigoPostal,
            Pais = domain.Pais,
            Telefono = domain.Telefono,
            Fax = fax
        };
    }

   
}