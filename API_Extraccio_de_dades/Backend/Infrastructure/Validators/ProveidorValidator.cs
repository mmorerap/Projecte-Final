
using Backend.Common;
using Backend.Infrastructure.DTO;

namespace Backend.Infrastructure.Validators;

public static class ProveidorIValidator
{
    public static Result Validate(ProveidorRequest proveidorRequest)
    {

        if (proveidorRequest.Nombre.Length > 10)
        {
            return Result.Failure("Longitud de Nom excedida", "Nom");
        }

        if (proveidorRequest.Nombre == string.Empty)
        {
            return Result.Failure("No ha posat cap valor a Nom", "Nom");
        }

        return Result.Ok();
        
    }
    
    

}