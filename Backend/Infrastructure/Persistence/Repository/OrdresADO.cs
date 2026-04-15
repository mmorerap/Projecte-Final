using Microsoft.Data.SqlClient;
using Backend.Services;
using Backend.Infrastructure.Persistence.Entities;

namespace Backend.Infrastructure.Persistence.Repository;

public class OrdresADO
{
    public static void Insert(DatabaseConnection dbConn, OrdreEntity ordre, List<LineaOrdreEntity> lineas)
    {
        if (!dbConn.Open()) 
        {
            throw new Exception("No se pudo abrir la conexión a la base de datos. Verifique la cadena de conexión en appsettings.json.");
        }

        using var transaction = dbConn.sqlConnection!.BeginTransaction();
        try
        {
            Guid ordreId = Guid.NewGuid();
            string sqlOrdre = @"INSERT INTO ordenes (id, numero, fecha, fecha_recepcion, modo_pago, gestionado_por, direccion_entrega, total_ht, total_iva, total_ttc, moneda, id_proveedor, id_cliente) 
                                VALUES (@id, @numero, @fecha, @fecha_recepcion, @modo_pago, @gestionado_por, @direccion_entrega, @total_ht, @total_iva, @total_ttc, @moneda, @id_proveedor, @id_cliente)";

            using SqlCommand cmdOrdre = new SqlCommand(sqlOrdre, dbConn.sqlConnection, transaction);
            cmdOrdre.Parameters.AddWithValue("@id", ordreId);
            cmdOrdre.Parameters.AddWithValue("@numero", ordre.Numero);
            cmdOrdre.Parameters.AddWithValue("@fecha", ordre.Fecha);
            cmdOrdre.Parameters.AddWithValue("@fecha_recepcion", ordre.FechaRecepcion);
            cmdOrdre.Parameters.AddWithValue("@modo_pago", ordre.ModoPago);
            cmdOrdre.Parameters.AddWithValue("@gestionado_por", ordre.GestionadoPor);
            cmdOrdre.Parameters.AddWithValue("@direccion_entrega", ordre.DireccionEntrega);
            cmdOrdre.Parameters.AddWithValue("@total_ht", ordre.TotalHT);
            cmdOrdre.Parameters.AddWithValue("@total_iva", ordre.TotalIVA);
            cmdOrdre.Parameters.AddWithValue("@total_ttc", ordre.TotalTTC);
            cmdOrdre.Parameters.AddWithValue("@moneda", ordre.Moneda);
            
            // Si son Guid.Empty, pasamos DBNull o un valor por defecto si la base de datos lo permite
            cmdOrdre.Parameters.AddWithValue("@id_proveedor", ordre.IdProveedor == Guid.Empty ? (object)DBNull.Value : ordre.IdProveedor);
            cmdOrdre.Parameters.AddWithValue("@id_cliente", ordre.IdCliente == Guid.Empty ? (object)DBNull.Value : ordre.IdCliente);

            cmdOrdre.ExecuteNonQuery();

            string sqlLinea = @"INSERT INTO lineas_orden (id, id_orden, descripcion, cantidad, precio_unitario, descuento, precio_neto, importe_ht, tva)
                                VALUES (@id, @id_orden, @descripcion, @cantidad, @precio_unitario, @descuento, @precio_neto, @importe_ht, @tva)";

            foreach (var linea in lineas)
            {
                using SqlCommand cmdLinea = new SqlCommand(sqlLinea, dbConn.sqlConnection, transaction);
                cmdLinea.Parameters.AddWithValue("@id", Guid.NewGuid());
                cmdLinea.Parameters.AddWithValue("@id_orden", ordreId);
                cmdLinea.Parameters.AddWithValue("@descripcion", linea.Descripcion);
                cmdLinea.Parameters.AddWithValue("@cantidad", linea.Cantidad);
                cmdLinea.Parameters.AddWithValue("@precio_unitario", linea.PrecioUnitario);
                cmdLinea.Parameters.AddWithValue("@descuento", linea.Descuento);
                cmdLinea.Parameters.AddWithValue("@precio_neto", linea.PrecioNeto);
                cmdLinea.Parameters.AddWithValue("@importe_ht", linea.ImporteHT);
                cmdLinea.Parameters.AddWithValue("@tva", linea.TVA);
                cmdLinea.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
        finally
        {
            dbConn.Close();
        }
    }
}
