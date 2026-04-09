
using Backend.Domain;
using Backend.Common;
using Backend.Application.Proveidor;

namespace dbdemo.Domain.Validators;

public static class CompraValidator
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
