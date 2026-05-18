using Backend.Infrastructure.DTO;
using Backend.Business;
using Backend.Services;

namespace Backend.Application;

public static class ProveidorEndpoints
{
    public static void MapProveidorEndpoints(this IEndpointRouteBuilder app, DatabaseConnection dbConn)
    {
        var business = new ProveedorBusiness(dbConn);

        app.MapPost("/OCRservice/proveidor", (ProveidorRequest req) =>
        {
            try 
            {
                var response = business.CrearProveedor(req);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = "VAL_ERROR", message = ex.Message });
            }
        });
    }
}
