using Backend.Infrastructure.DTO.Ordres;
using Backend.Business;
using Backend.Services;

namespace Backend.Application.Endpoints;

public static class Ordres
{
    public static void MapOrdresEndpoints(this WebApplication app, DatabaseConnection dbConn)
    {
        var business = new OrdreBusiness(dbConn);

        app.MapPost("/OCRservice/ordres", (OrdresRequest req) =>
        {
            try 
            {
                var message = business.CrearOrdre(req);
                return Results.Ok(new { message });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = "VAL_ERROR", message = ex.Message });
            }
        });
    }
}
