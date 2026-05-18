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

        if (string.IsNullOrWhiteSpace(request.Orden.Fecha))
        {
            return Result.Failure("La fecha de la orden es obligatoria", "Fecha");
        }

        if (request.Lineas == null || request.Lineas.Count == 0)
        {
            return Result.Failure("La orden debe tener al menos una línea", "Lineas");
        }

        int index = 1;
        foreach (var linea in request.Lineas)
        {
            if (string.IsNullOrWhiteSpace(linea.Descripcion))
            {
                return Result.Failure($"La descripción de la línea {index} es obligatoria", "Descripcion");
            }

            if (linea.Cantidad <= 0)
            {
                return Result.Failure($"La cantidad de la línea {index} debe ser mayor que cero", "Cantidad");
            }

            if (linea.PrecioUnitario <= 0)
            {
                return Result.Failure($"El precio unitario de la línea {index} debe ser mayor que cero", "PrecioUnitario");
            }
            index++;
        }

        return Result.Ok();
    }
}
