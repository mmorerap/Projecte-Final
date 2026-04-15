using Backend.Infrastructure.DTO;
using Backend.Domain;
using Backend.Common;
using Backend.Mappers;
using Backend.Services;
using Backend.Infrastructure.Validators;
using Backend.Domain.Validators;
using Backend.Infrastructure.Persistence;

namespace Backend.Business;

public class ProveedorBusiness
{
    private readonly DatabaseConnection _db;

    public ProveedorBusiness(DatabaseConnection db)
    {
        _db = db;
    }

    public ProveidorResponse CrearProveedor(ProveidorRequest req)
    {
        // 1. Validar DTO (Infraestructuta)
        var resultI = ProveidorIValidator.Validate(req);
        if (!resultI.IsOk)
            throw new Exception(resultI.ErrorMessage);

        // 2. DTO → Domain
        var domain = ProveedorMapper.ToDomain(req);

        // 3. Validar Domain (Domain)
        var resultD = ProveidorDValidator.Validate(domain);
        if (!resultD.IsOk)
             throw new Exception(resultD.ErrorMessage);

        // 4. Domain → Entity
        var id = Guid.NewGuid();
        var entity = ProveedorMapper.ToEntity(domain, id, req.Fax);

        // 5. Guardar BD (Persistence)
        ProveidorADO.Insert(_db, entity);

        // 6. Entity → Response
        return ProveedorMapper.ToResponse(entity);
    }
}