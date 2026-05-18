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
        var resultI = OrdresValidator.Validate(req);
        if (!resultI.IsOk)
            throw new Exception(resultI.ErrorMessage ?? "Validació fallida");


        string nif = (req.Cliente.NifIva ?? string.Empty).Trim();
        Guid? idCliente = ClientesADO.GetIdByNif(_db, nif);

        if (idCliente == null)
            throw new Exception("Aquest client no existeix");

        var domain = OrdreMapper.ToDomain(req);
        domain.IdCliente = idCliente.Value; 
        
       
        var resultD = OrdreDValidator.Validate(domain);
        if (!resultD.IsOk)
            throw new Exception(resultD.ErrorMessage ?? "Validació de domini fallida");

        var entity = OrdreMapper.ToEntity(domain);
        var lines = OrdreMapper.ToLineEntities(domain);

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
