using Backend.Common;

namespace Backend.Domain.Validators;

public static class OrdreDValidator
{
    public static Result Validate(OrdreDomain domain)
    {
        if (domain.Fecha == DateTime.MinValue)
        {
            return Result.Failure("La fecha de la orden no es válida", "Fecha");
        }
        
        return Result.Ok();
    }
}
