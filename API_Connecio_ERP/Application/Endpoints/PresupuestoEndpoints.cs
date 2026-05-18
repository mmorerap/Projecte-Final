using API_Connecio_ERP.Business;
using API_Connecio_ERP.Infrastructure.DTO.Presupuestos;
using API_Connecio_ERP.Infrastructure.Integrations.Odoo;
using API_Connecio_ERP.Infrastructure.Persistence.Repository;
using API_Connecio_ERP.Services;

namespace API_Connecio_ERP.Application.Endpoints;

public static class PresupuestoEndpoints
{
    public static void MapPresupuestoEndpoints(this IEndpointRouteBuilder app, DatabaseConnection dbConn)
    {
        app.MapGet("/erp/ordenes", () =>
        {
            try
            {
                return Results.Ok(PresupuestosADO.GetOrdenesResumen(dbConn));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = "VAL_ERROR", message = ex.Message });
            }
        });

        app.MapPost("/erp/presupuesto", async (PresupuestoRequest req, IConfiguration configuration, IHttpClientFactory httpClientFactory) =>
        {
            try
            {
                var odooClient = new OdooClient(httpClientFactory.CreateClient(), configuration);
                var business = new PresupuestoBusiness(dbConn, odooClient);
                var response = await business.CrearPresupuestoAsync(req);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = "VAL_ERROR", message = ex.Message });
            }
        });
    }
}
