using Backend.Common;
using Backend.Infrastructure.DTO.Ordres;

namespace Backend.Infrastructure.Validators;

public static class OrdresValidator
{
    public static Result Validate(OrdresRequest request)
    {
        if (request.Orden == null || string.IsNullOrWhiteSpace(request.Orden.Numero))
        {
            return Result.Failure("El número de orden es obligatorio", "Numero");
        }

        if (request.Lineas == null || request.Lineas.Count == 0)
        {
            return Result.Failure("La orden debe tener al menos una línea", "Lineas");
        }

        return Result.Ok();
    }
}
