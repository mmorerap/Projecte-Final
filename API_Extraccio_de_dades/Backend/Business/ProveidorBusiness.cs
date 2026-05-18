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
        var resultI = ProveidorIValidator.Validate(req);
        if (!resultI.IsOk)
            throw new Exception(resultI.ErrorMessage);

        var domain = ProveedorMapper.ToDomain(req);

        var resultD = ProveidorDValidator.Validate(domain);
        if (!resultD.IsOk)
             throw new Exception(resultD.ErrorMessage);

        var id = Guid.NewGuid();
        var entity = ProveedorMapper.ToEntity(domain, id, req.Fax);

        ProveidorADO.Insert(_db, entity);

        return ProveedorMapper.ToResponse(entity);
    }
}