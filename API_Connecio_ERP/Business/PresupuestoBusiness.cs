using API_Connecio_ERP.Infrastructure.DTO.Presupuestos;
using API_Connecio_ERP.Infrastructure.Integrations.Odoo;
using API_Connecio_ERP.Infrastructure.Persistence.Repository;
using API_Connecio_ERP.Services;

namespace API_Connecio_ERP.Business;

public class PresupuestoBusiness
{
    private readonly DatabaseConnection _db;
    private readonly OdooClient _odooClient;

    public PresupuestoBusiness(DatabaseConnection db, OdooClient odooClient)
    {
        _db = db;
        _odooClient = odooClient;
    }

    public async Task<PresupuestoResponse> CrearPresupuestoAsync(PresupuestoRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.NumeroOrden))
        {
            throw new Exception("numero_orden es obligatorio.");
        }

        var data = PresupuestosADO.GetByNumeroOrden(_db, req.NumeroOrden.Trim());
        if (data == null)
        {
            throw new Exception("No existe una orden en la BDD con ese número.");
        }

        if (data.Estado == 1)
        {
            throw new Exception("Esta orden ya está traspasada a Odoo.");
        }

        if (data.Lineas.Count == 0)
        {
            throw new Exception("La orden no tiene líneas para facturar.");
        }

        var result = await _odooClient.CrearPresupuestoAsync(data);
        PresupuestosADO.MarcarComoTraspasada(_db, data.OrdenId);

        return new PresupuestoResponse
        {
            Message = "Presupuesto creado correctamente en Odoo.",
            OdooSaleOrderId = result.SaleOrderId,
            OdooSaleOrderName = result.SaleOrderName
        };
    }
}
