using API_Connecio_ERP.Infrastructure.Persistence.Entities;
using API_Connecio_ERP.Infrastructure.DTO.Presupuestos;
using API_Connecio_ERP.Services;
using Microsoft.Data.SqlClient;

namespace API_Connecio_ERP.Infrastructure.Persistence.Repository;

public class PresupuestosADO
{
    public static List<OrdenResumenResponse> GetOrdenesResumen(DatabaseConnection dbConn)
    {
        if (!dbConn.Open())
        {
            throw new Exception("No se pudo abrir la conexión a la base de datos.");
        }

        try
        {
            const string sql = @"
                SELECT
                    o.id,
                    o.numero,
                    o.fecha,
                    c.nombre AS cliente_nombre,
                    o.moneda,
                    o.total_ttc,
                    COUNT(l.id) AS lineas
                FROM ordenes o
                INNER JOIN clientes c ON c.id = o.id_cliente
                LEFT JOIN lineas_orden l ON l.id_orden = o.id
                WHERE o.estado IS NULL OR o.estado = 0
                GROUP BY o.id, o.numero, o.fecha, c.nombre, o.moneda, o.total_ttc
                ORDER BY o.fecha DESC, o.numero DESC";

            using var cmd = new SqlCommand(sql, dbConn.SqlConnection);
            using var reader = cmd.ExecuteReader();
            var ordenes = new List<OrdenResumenResponse>();

            while (reader.Read())
            {
                ordenes.Add(new OrdenResumenResponse
                {
                    Id = reader.GetGuid(0),
                    Numero = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Fecha = reader.IsDBNull(2) ? DateTime.MinValue : reader.GetDateTime(2),
                    ClienteNombre = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Moneda = reader.IsDBNull(4) ? "EUR" : reader.GetString(4),
                    TotalTtc = reader.IsDBNull(5) ? 0 : Convert.ToDecimal(reader.GetValue(5)),
                    Lineas = reader.IsDBNull(6) ? 0 : Convert.ToInt32(reader.GetValue(6))
                });
            }

            return ordenes;
        }
        finally
        {
            dbConn.Close();
        }
    }

    public static PresupuestoDataEntity? GetByNumeroOrden(DatabaseConnection dbConn, string numeroOrden)
    {
        if (!dbConn.Open())
        {
            throw new Exception("No se pudo abrir la conexión a la base de datos.");
        }

        try
        {
            const string sqlCabecera = @"
                SELECT TOP 1
                    o.id,
                    o.numero,
                    o.fecha,
                    o.moneda,
                    c.nombre AS cliente_nombre,
                    c.nif_iva,
                    c.direccion,
                    o.estado
                FROM ordenes o
                INNER JOIN clientes c ON c.id = o.id_cliente
                WHERE o.numero = @numero";

            using var cmdCabecera = new SqlCommand(sqlCabecera, dbConn.SqlConnection);
            cmdCabecera.Parameters.AddWithValue("@numero", numeroOrden);

            using var readerCabecera = cmdCabecera.ExecuteReader();
            if (!readerCabecera.Read())
            {
                return null;
            }

            var data = new PresupuestoDataEntity
            {
                OrdenId = readerCabecera.GetGuid(0),
                NumeroOrden = readerCabecera.GetString(1),
                FechaOrden = readerCabecera.GetDateTime(2),
                Moneda = readerCabecera.IsDBNull(3) ? "EUR" : readerCabecera.GetString(3),
                ClienteNombre = readerCabecera.IsDBNull(4) ? string.Empty : readerCabecera.GetString(4),
                ClienteNifIva = readerCabecera.IsDBNull(5) ? string.Empty : readerCabecera.GetString(5),
                ClienteDireccion = readerCabecera.IsDBNull(6) ? string.Empty : readerCabecera.GetString(6),
                Estado = readerCabecera.IsDBNull(7) ? 0 : Convert.ToInt32(readerCabecera.GetValue(7))
            };

            readerCabecera.Close();

            const string sqlLineas = @"
                SELECT
                    descripcion,
                    cantidad,
                    precio_unitario,
                    codigo_cliente,
                    codigo_proveedor
                FROM lineas_orden
                WHERE id_orden = @idOrden";

            using var cmdLineas = new SqlCommand(sqlLineas, dbConn.SqlConnection);
            cmdLineas.Parameters.AddWithValue("@idOrden", data.OrdenId);
            using var readerLineas = cmdLineas.ExecuteReader();

            while (readerLineas.Read())
            {
                data.Lineas.Add(new PresupuestoLineaEntity
                {
                    Descripcion = readerLineas.IsDBNull(0) ? string.Empty : readerLineas.GetString(0),
                    Cantidad = readerLineas.IsDBNull(1) ? 0 : Convert.ToDecimal(readerLineas.GetValue(1)),
                    PrecioUnitario = readerLineas.IsDBNull(2) ? 0 : Convert.ToDecimal(readerLineas.GetValue(2)),
                    CodigoCliente = readerLineas.IsDBNull(3) ? string.Empty : readerLineas.GetString(3),
                    CodigoProveedor = readerLineas.IsDBNull(4) ? string.Empty : readerLineas.GetString(4)
                });
            }

            return data;
        }
        finally
        {
            dbConn.Close();
        }
    }

    public static void MarcarComoTraspasada(DatabaseConnection dbConn, Guid ordenId)
    {
        if (!dbConn.Open())
        {
            throw new Exception("No se pudo abrir la conexión a la base de datos.");
        }

        try
        {
            const string sql = "UPDATE ordenes SET estado = @estado WHERE id = @id";
            using var cmd = new SqlCommand(sql, dbConn.SqlConnection);
            cmd.Parameters.AddWithValue("@estado", 1);
            cmd.Parameters.AddWithValue("@id", ordenId);
            cmd.ExecuteNonQuery();
        }
        finally
        {
            dbConn.Close();
        }
    }
}
