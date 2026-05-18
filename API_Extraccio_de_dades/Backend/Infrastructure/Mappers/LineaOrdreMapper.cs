using Backend.Domain;
using Backend.Infrastructure.DTO.Ordres;
using Backend.Infrastructure.Persistence.Entities;

namespace Backend.Mappers;

public static class LineaOrdreMapper
{
    private static string ResolveCodigoLinea(LineaOrdreRequest req) =>
        req.Codigo ?? req.CodigoProveedor ?? req.CodigoProducto
        ?? (string.Equals(req.CodigoCliente, "BOSS", StringComparison.OrdinalIgnoreCase) ? null : req.CodigoCliente)
        ?? string.Empty;

    public static LineaOrdreDomain ToDomain(LineaOrdreRequest req)
    {
        return new LineaOrdreDomain
        {
            Descripcion = req.Descripcion,
            Cantidad = Math.Max(1, (int)Math.Round(req.Cantidad, MidpointRounding.AwayFromZero)),
            PrecioUnitario = req.PrecioUnitario,
            Descuento = req.Descuento ?? 0,
            PrecioNeto = req.PrecioUnitario,
            ImporteHT = req.ImporteHT ?? req.Cantidad * req.PrecioUnitario,
            TVA = req.TVA ?? 0,
            CodigoCliente = ResolveCodigoLinea(req),
            CodigoProveedor = string.Empty
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
            TVA = domain.TVA,
            CodigoCliente = domain.CodigoCliente,
            CodigoProveedor = domain.CodigoProveedor
        };
    }
}
