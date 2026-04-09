using Microsoft.Data.SqlClient;
using static System.Console;

namespace Backend.Services;

public class DatabaseConnection
{
    private readonly string _connectionString;
    public SqlConnection? sqlConnection;
    public DatabaseConnection(string connectionString)
    {
        _connectionString = connectionString;
    }
    public bool Open()
    {
        sqlConnection = new SqlConnection(_connectionString);

        try
        {
            Console.WriteLine("Obrim la connexió");
            sqlConnection.Open();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Dades Connexió {_connectionString}");
            Console.WriteLine("Error en obrir la connexió");
            WriteLine(ex.Message);
            return false;
        }
    }

    public SqlConnection? GetSqlConnection1()
    {
        return sqlConnection;
    }

    public void Close(SqlConnection? sqlConnection1)
    {
        Console.WriteLine("Tanquem la connexió");
        sqlConnection1.Close();
    }
}
