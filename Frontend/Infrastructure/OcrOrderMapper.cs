using System.Text.Json;
using System.Text.Json.Nodes;
using OCRDesktop.Infrastructure.Models;

namespace OCRDesktop.Infrastructure;

public static class OcrOrderMapper
{
    public static string ResolveCodigoLinea(OcrLineaDto linea) =>
        linea.Codigo ?? linea.CodigoProveedor ?? linea.CodigoProducto
        ?? (string.Equals(linea.CodigoCliente, "BOSS", StringComparison.OrdinalIgnoreCase) ? null : linea.CodigoCliente)
        ?? string.Empty;

    public static OcrExtractedOrderDto? ParseExtractedData(object extractedData)
    {
        if (extractedData is OrderReview review)
        {
            return FromOrderReview(review);
        }

        string json = extractedData switch
        {
            JsonElement element => element.GetRawText(),
            JsonNode node => node.ToJsonString(),
            _ => JsonSerializer.Serialize(extractedData),
        };

        return JsonSerializer.Deserialize<OcrExtractedOrderDto>(json, OcrJsonOptions.Default);
    }

    public static OcrExtractedOrderDto FromOrderReview(OrderReview review)
    {
        var dto = new OcrExtractedOrderDto
        {
            Cliente = new OcrClienteDto
            {
                Nombre = review.Cliente.Nombre,
                Direccion = review.Cliente.Direccion,
                Ciudad = review.Cliente.Ciudad,
                CodigoPostal = review.Cliente.CodigoPostal,
                Pais = review.Cliente.Pais,
                Telefono = review.Cliente.Telefono,
                NifIva = review.Cliente.NifIva,
                CodigoCliente = review.Cliente.CodigoCliente,
            },
            Orden = new OcrOrdenDto
            {
                Numero = review.Orden.Numero,
                Fecha = review.Orden.Fecha,
                ModoPago = review.Orden.ModoPago,
                DireccionEntrega = review.Orden.DireccionEntrega,
            },
            Totales = new OcrTotalesDto
            {
                TotalHt = review.Totales.TotalHT,
                TotalIva = review.Totales.TotalIVA,
                TotalTtc = review.Totales.TotalTTC,
                Moneda = review.Totales.Moneda,
            },
        };

        foreach (var linea in review.Lineas)
        {
            if (string.IsNullOrWhiteSpace(linea.Descripcion)
                && linea.Cantidad <= 0
                && linea.PrecioUnitario <= 0)
            {
                continue;
            }

            dto.Lineas.Add(new OcrLineaDto
            {
                Codigo = linea.CodigoProducto,
                CodigoCliente = linea.CodigoProducto,
                Descripcion = linea.Descripcion,
                Cantidad = linea.Cantidad,
                PrecioUnitario = linea.PrecioUnitario,
                ImporteHt = linea.ImporteHT > 0 ? linea.ImporteHT : null,
            });
        }

        return dto;
    }

    public static OrderReview ToOrderReview(OcrExtractedOrderDto dto)
    {
        var review = new OrderReview();

        if (dto.Cliente != null)
        {
            review.Cliente.Nombre = dto.Cliente.Nombre ?? string.Empty;
            review.Cliente.Direccion = dto.Cliente.Direccion ?? string.Empty;
            review.Cliente.Ciudad = dto.Cliente.Ciudad ?? string.Empty;
            review.Cliente.CodigoPostal = dto.Cliente.CodigoPostal;
            review.Cliente.Pais = dto.Cliente.Pais;
            review.Cliente.Telefono = dto.Cliente.Telefono;
            review.Cliente.NifIva = dto.Cliente.NifIva;
            review.Cliente.CodigoCliente = dto.Cliente.CodigoCliente;
        }

        if (dto.Orden != null)
        {
            review.Orden.Numero = dto.Orden.Numero ?? string.Empty;
            review.Orden.Fecha = dto.Orden.Fecha ?? string.Empty;
            review.Orden.ModoPago = dto.Orden.ModoPago;
            review.Orden.DireccionEntrega = dto.Orden.DireccionEntrega;
        }

        review.Lineas.Clear();
        foreach (var linea in dto.Lineas)
        {
            var codigo = linea.Codigo ?? linea.CodigoProveedor ?? linea.CodigoProducto ?? string.Empty;
            var importe = linea.ImporteHt ?? linea.Cantidad * linea.PrecioUnitario;

            review.Lineas.Add(new ReviewLinea
            {
                Descripcion = linea.Descripcion ?? string.Empty,
                Cantidad = linea.Cantidad,
                PrecioUnitario = linea.PrecioUnitario,
                ImporteHT = importe,
                CodigoProducto = codigo,
                CodigoCliente = codigo,
            });
        }

        if (dto.Totales != null)
        {
            review.Totales.TotalHT = dto.Totales.TotalHt;
            review.Totales.TotalIVA = dto.Totales.TotalIva;
            review.Totales.TotalTTC = dto.Totales.TotalTtc;
            review.Totales.Moneda = dto.Totales.Moneda ?? "EUR";
        }

        return review;
    }

    public static string ToOrdresApiJson(OcrExtractedOrderDto dto, string? sourceFileName)
    {
        var lineas = dto.Lineas.Select(l =>
        {
            var codigoLinea = ResolveCodigoLinea(l);
            return new JsonObject
            {
                ["descripcion"] = l.Descripcion,
                ["cantidad"] = l.Cantidad,
                ["precio_unitario"] = l.PrecioUnitario,
                ["importe_ht"] = l.ImporteHt ?? l.Cantidad * l.PrecioUnitario,
                ["codigo_cliente"] = codigoLinea,
            };
        }).ToArray<JsonNode?>();

        var root = new JsonObject
        {
            ["cliente"] = new JsonObject
            {
                ["nombre"] = dto.Cliente?.Nombre,
                ["direccion"] = dto.Cliente?.Direccion,
                ["ciudad"] = dto.Cliente?.Ciudad,
                ["codigo_postal"] = dto.Cliente?.CodigoPostal,
                ["pais"] = dto.Cliente?.Pais,
                ["telefono"] = dto.Cliente?.Telefono,
                ["nif_iva"] = dto.Cliente?.NifIva,
                ["codigo_cliente"] = dto.Cliente?.CodigoCliente,
            },
            ["orden"] = new JsonObject
            {
                ["numero"] = dto.Orden?.Numero,
                ["fecha"] = dto.Orden?.Fecha,
                ["modo_pago"] = dto.Orden?.ModoPago,
                ["direccion_entrega"] = dto.Orden?.DireccionEntrega,
            },
            ["lineas"] = new JsonArray(lineas),
            ["totales"] = new JsonObject
            {
                ["total_ht"] = dto.Totales?.TotalHt ?? 0,
                ["total_iva"] = dto.Totales?.TotalIva ?? 0,
                ["total_ttc"] = dto.Totales?.TotalTtc ?? 0,
                ["moneda"] = dto.Totales?.Moneda ?? "EUR",
            },
        };

        if (!string.IsNullOrWhiteSpace(sourceFileName))
        {
            root["source_file_name"] = sourceFileName;
        }

        return root.ToJsonString();
    }
}
