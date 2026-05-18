using Backend.Infrastructure.DTO.Ordres;
using Backend.Business;
using Backend.Services;
using Backend.ServiceOCR;

namespace Backend.Application;

public static class OrdresEndpoints
{
    public static void MapOrdresEndpoints(this IEndpointRouteBuilder app, DatabaseConnection dbConn)
    {
        var business = new OrdreBusiness(dbConn);

        app.MapPost("/OCRservice/ordres", async (OrdresRequest req, IOcrService ocrService) =>
        {
            try
            {
                var message = business.CrearOrdre(req);
                return Results.Ok(new { message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ORDRES] EXCEPCIÓN: {ex.Message}");
                bool movedToErronies = false;
                if (!string.IsNullOrWhiteSpace(req.SourceFileName))
                {
                    var (moved, moveMsg) = await ocrService.MoveToErroniesAsync(req.SourceFileName.Trim(), ex.Message);
                    movedToErronies = moved;
                    if (!moved)
                    {
                        Console.WriteLine($"[ORDRES] Erronies: {moveMsg}");
                    }
                }

                return Results.BadRequest(new { error = "VAL_ERROR", message = ex.Message, movedToErronies });
            }
        });
    }
}
