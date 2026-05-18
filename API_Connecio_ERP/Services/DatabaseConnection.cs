using Microsoft.Data.SqlClient;

namespace API_Connecio_ERP.Services;

public class DatabaseConnection
{
    private readonly string _connectionString;
    public SqlConnection? SqlConnection { get; private set; }

    public DatabaseConnection(string connectionString)
    {
        _connectionString = connectionString;
    }

    public bool Open()
    {
        SqlConnection = new SqlConnection(_connectionString);

        try
        {
            SqlConnection.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Close()
    {
        SqlConnection?.Close();
    }
}
