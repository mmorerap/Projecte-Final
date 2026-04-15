using Microsoft.Data.SqlClient;
using Backend.Services;
using Backend.Infrastructure.Persistence.Entities;

namespace Backend.Infrastructure.Persistence.Repository;

public class ClientesADO
{
    public static Guid GetOrCreateByCodigo(DatabaseConnection dbConn, ClienteEntity cliente)
    {
        if (!dbConn.Open()) throw new Exception("Error al abrir conexión");

        try
        {
            // 1. Intentar buscar por código_cliente
            string selectSql = "SELECT id FROM clientes WHERE codigo_cliente = @codigo";
            using (SqlCommand selectCmd = new SqlCommand(selectSql, dbConn.sqlConnection))
            {
                selectCmd.Parameters.AddWithValue("@codigo", cliente.CodigoCliente);
                var result = selectCmd.ExecuteScalar();
                if (result != null) return (Guid)result;
            }

            // 2. Si no existe, insertar con un nuevo GUID
            Guid newId = Guid.NewGuid();
            string insertSql = @"INSERT INTO clientes (id, codigo_cliente, nombre, direccion, ciudad, codigo_postal, pais, telefono, nif_iva)
                                VALUES (@id, @codigo, @nombre, @direccion, @ciudad, @cp, @pais, @tel, @nif)";
            
            using (SqlCommand insertCmd = new SqlCommand(insertSql, dbConn.sqlConnection))
            {
                insertCmd.Parameters.AddWithValue("@id", newId);
                insertCmd.Parameters.AddWithValue("@codigo", cliente.CodigoCliente);
                insertCmd.Parameters.AddWithValue("@nombre", cliente.Nombre ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@direccion", cliente.Direccion ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@ciudad", cliente.Ciudad ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@cp", cliente.CodigoPostal ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@pais", cliente.Pais ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@tel", cliente.Telefono ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@nif", cliente.NifIva ?? (object)DBNull.Value);
                
                insertCmd.ExecuteNonQuery();
            }
            return newId;
        }
        finally
        {
            dbConn.Close();
        }
    }
}
