using OCRDesktop.Infrastructure.Models;

namespace OCRDesktop.Services.Interfaces;

public interface IErpApiService
{
    Task<(bool success, string message, List<ErpOrdenResumen> ordenes)> GetOrdenesAsync();
    Task<(bool success, string message, ErpPresupuestoResponse? data)> CrearPresupuestoAsync(string numeroOrden);
}
