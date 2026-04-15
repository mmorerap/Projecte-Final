using Backend.Domain;
using Backend.Infrastructure.DTO.Ordres;
using Backend.Infrastructure.Persistence.Entities;

namespace Backend.Mappers;

public static class LineaOrdreMapper
{
    public static LineaOrdreDomain ToDomain(LineaOrdreRequest req)
    {
        return new LineaOrdreDomain
        {
            Descripcion = req.Descripcion,
            Cantidad = (int)req.Cantidad,
            PrecioUnitario = req.PrecioUnitario,
            Descuento = req.Descuento ?? 0,
            PrecioNeto = req.PrecioUnitario, // Simplificación si no viene precio_neto
            ImporteHT = req.ImporteHT,
            TVA = req.TVA
        };
    }

    public static LineaOrdreEntity ToEntity(LineaOrdreDomain domain)
    {
        return new LineaOrdreEntity
        {
            Descripcion = domain.Descripcion,
            Cantidad = domain.Cantidad,
            PrecioUnitario = domain.PrecioUnitario,
            Descuento = domain.Descuento,
            PrecioNeto = domain.PrecioNeto,
            ImporteHT = domain.ImporteHT,
            TVA = domain.TVA
        };
    }
}
