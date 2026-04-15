
using Backend.Domain;
using Backend.Common;

namespace Backend.Domain.Validators;

public static class ProveidorDValidator
{
    public static Result Validate(ProveidorDomain proveidor)
    {
        if (proveidor == null)
            return Result.Failure("La compra no pot ser null", "COMPRA_NULL");

        if (proveidor.Nombre == null)
            return Result.Failure("El nom de proveidor no pot ser null", "PROVEIDOR_NULL");


        return Result.Ok();
    }
}
