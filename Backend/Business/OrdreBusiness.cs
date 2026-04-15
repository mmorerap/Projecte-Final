using Backend.Infrastructure.DTO.Ordres;
using Backend.Infrastructure.Persistence.Repository;
using Backend.Infrastructure.Persistence.Entities;
using Backend.Infrastructure.Validators;
using Backend.Domain.Validators;
using Backend.Mappers;
using Backend.Services;
using Backend.Common;

namespace Backend.Business;

public class OrdreBusiness
{
    private readonly DatabaseConnection _db;

    public OrdreBusiness(DatabaseConnection db)
    {
        _db = db;
    }

    public string CrearOrdre(OrdresRequest req)
    {
        // 1. Validar DTO (Infraestructuta)
        var resultI = OrdresValidator.Validate(req);
        if (!resultI.IsOk)
            throw new Exception(resultI.ErrorMessage);

        // 2. Gestionar Cliente (Resolución de FK)
        // Mapeamos el cliente del request a entidad y lo buscamos/creamos en la BD
        var clienteEntity = ClienteMapper.ToEntity(req.Cliente);
        Guid idCliente = ClientesADO.GetOrCreateByCodigo(_db, clienteEntity);

        // 3. DTO → Domain
        var domain = OrdreMapper.ToDomain(req);
        domain.IdCliente = idCliente; 
        
        // TODO: Gestionar IdProveedor si es necesario. Por ahora usamos 0 o nulo si lo permite la BD.
        // Si el Proveedor es obligatorio, deberías aplicar una lógica similar a la del cliente.

        // 4. Validar Domain (Domain)
        var resultD = OrdreDValidator.Validate(domain);
        if (!resultD.IsOk)
            throw new Exception(resultD.ErrorMessage);

        // 5. Domain → Entity
        var entity = OrdreMapper.ToEntity(domain);
        var lines = OrdreMapper.ToLineEntities(domain);

        // 6. Guardar BD (Persistence)
        try 
        {
            OrdresADO.Insert(_db, entity, lines);
        }
        catch (Exception ex)
        {
            throw new Exception("Error al guardar en la base de datos: " + ex.Message);
        }

        return "Orden creada correctamente";
    }
}
