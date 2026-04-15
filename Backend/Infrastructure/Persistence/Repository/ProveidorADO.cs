using Microsoft.Data.SqlClient;
using Backend.Services;


namespace Backend.Infrastructure.Persistence;

class ProveidorADO
{
   
    //POST
    public static void Insert(DatabaseConnection dbConn,ProveidorEntity proveidorEntity)
    {

        dbConn.Open();

        string sql = @"INSERT INTO Familia (Id, Nom, Descripcio)         
                        VALUES (@Id, @Nom, @Descripcio)";            

        using SqlCommand cmd = new SqlCommand(sql, dbConn.sqlConnection);
        cmd.Parameters.AddWithValue("@Id", proveidorEntity.Id);
        cmd.Parameters.AddWithValue("@Nom", proveidorEntity.email);
        cmd.Parameters.AddWithValue("@Descripcio", proveidorEntity.Ciudad);

        cmd.ExecuteNonQuery();

        dbConn.Close();
    }

}
