using Microsoft.Data.SqlClient;
using Backend.Services;
using Backend.Infrastructure.Persistence.Entities;

namespace Backend.Infrastructure.Persistence.Repository;

public class ClientesADO
{
    public static Guid? GetIdByNif(DatabaseConnection dbConn, string nif)
    {
        if (!dbConn.Open()) throw new Exception("Error al abrir conexión");

        try
        {
            string selectSql = "SELECT id FROM clientes WHERE nif_iva = @nif";
            using (SqlCommand selectCmd = new SqlCommand(selectSql, dbConn.sqlConnection))
            {
                selectCmd.Parameters.AddWithValue("@nif", nif);
                var result = selectCmd.ExecuteScalar();
                if (result != null) return (Guid)result;
            }

            return null;
        }
        finally
        {
            dbConn.Close();
        }
    }
}
