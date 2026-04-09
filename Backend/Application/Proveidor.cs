using Backend.Infrastructure.DTO;
using Backend.Services;
using Backend.Common;
namespace Backend.Application.Proveidor;

public static class Proveidor
{
    public static void MapProveidorEndpoints(this WebApplication app, DatabaseConnection dbConn)
    {
        
        // POST /product
        app.MapPost("/OCRservice/provisionalname", (ProveedorRequest req) =>
        {
            Guid id;
            Result result = ProducteValidator.Validate(req);

            if (!result.IsOk)
            {
                return Results.BadRequest(new 
                {
                    error = result.ErrorCode,
                    message = result.ErrorMessage
                });
            }


            id =  Guid.NewGuid();
            Product product = req.ToProducte(id);
            ProductADO.Insert(dbConn, product);

            return Results.Created($"/products/{product.Id}", product);
        });
 
    }


}


