using Backend.Domain;
using Backend.Infrastructure.DTO.Ordres;
using Backend.Infrastructure.Persistence.Entities;
using System.Globalization;

namespace Backend.Mappers;

public static class OrdreMapper
{
    public static OrdreDomain ToDomain(OrdresRequest req)
    {
        return new OrdreDomain
        {
            Numero = req.Orden.Numero,
            Fecha = ParseDate(req.Orden.Fecha),
            FechaRecepcion = ParseDate(req.Orden.FechaRecepcion),
            ModoPago = req.Orden.ModoPago ?? string.Empty,
            GestionadoPor = req.Orden.GestionadoPor ?? string.Empty,
            DireccionEntrega = req.Orden.DireccionEntrega ?? string.Empty,
            TotalHT = req.Totales.TotalHT,
            TotalIVA = req.Totales.TotalIVA,
            TotalTTC = req.Totales.TotalTTC,
            Moneda = req.Totales.Moneda,
            IdProveedor = Guid.Empty, // Se debería resolver o asignar según lógica
            IdCliente = Guid.Empty,    // Se debería resolver o asignar según lógica
            Lineas = req.Lineas.Select(LineaOrdreMapper.ToDomain).ToList()
        };
    }

    private static DateTime ParseDate(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr)) return DateTime.Now;
        
        if (DateTime.TryParseExact(dateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
        {
            return result;
        }
        
        return DateTime.TryParse(dateStr, out result) ? result : DateTime.Now;
    }

    public static OrdreEntity ToEntity(OrdreDomain domain)
    {
        return new OrdreEntity
        {
            Numero = domain.Numero,
            Fecha = domain.Fecha,
            FechaRecepcion = domain.FechaRecepcion,
            ModoPago = domain.ModoPago,
            GestionadoPor = domain.GestionadoPor,
            DireccionEntrega = domain.DireccionEntrega,
            TotalHT = domain.TotalHT,
            TotalIVA = domain.TotalIVA,
            TotalTTC = domain.TotalTTC,
            Moneda = domain.Moneda,
            IdProveedor = domain.IdProveedor,
            IdCliente = domain.IdCliente
        };
    }

    public static List<LineaOrdreEntity> ToLineEntities(OrdreDomain domain)
    {
        return domain.Lineas.Select(LineaOrdreMapper.ToEntity).ToList();
    }
}
