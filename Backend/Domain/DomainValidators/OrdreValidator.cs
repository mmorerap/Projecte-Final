using Backend.Common;

namespace Backend.Domain.Validators;

public static class OrdreDValidator
{
    public static Result Validate(OrdreDomain domain)
    {
        // Validación de dominio (ejemplo: el total no puede ser negativo)
        if (domain.TotalTTC < 0)
        {
            return Result.Failure("El total TTC no puede ser negativo", "TotalTTC");
        }
        return Result.Ok();
    }
}
