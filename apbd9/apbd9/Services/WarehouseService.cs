using System.Data.Common;
using apbd9.Models;
using Microsoft.Data.SqlClient;


namespace apbd9.Services;

public class WarehouseService : IWarehouseService
{
    
    private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;";


    public async Task GetWarehousesAsync()
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await command.Connection.OpenAsync();
        
        DbTransaction transaction = await command.Connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
        
    }
}