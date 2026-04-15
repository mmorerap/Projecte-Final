using Backend.Infrastructure.DTO.Ordres;
using Backend.Infrastructure.Persistence.Entities;

namespace Backend.Mappers;

public static class ClienteMapper
{
    public static ClienteEntity ToEntity(ClienteRequest req)
    {
        return new ClienteEntity
        {
            CodigoCliente = req.CodigoCliente ?? "GENERIC",
            Nombre = req.Nombre,
            Direccion = req.Direccion,
            Ciudad = req.Ciudad,
            CodigoPostal = req.CodigoPostal ?? string.Empty,
            Pais = req.Pais ?? string.Empty,
            Telefono = req.Telefono ?? string.Empty,
            NifIva = req.NifIva ?? string.Empty
        };
    }
}
