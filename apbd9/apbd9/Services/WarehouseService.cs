using System.Data;
using System.Data.Common;
using apbd9.Models;
using Microsoft.Data.SqlClient;


namespace apbd9.Services;

public class WarehouseService : IWarehouseService
{
    private readonly string _connectionString =
        "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;";


    public async Task AddProductAsync(WarehouseDto warehouse)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        await connection.OpenAsync();

        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            if (warehouse.Amount <= 0)
            {
                throw new ArgumentException("Amount has to be greater than 0");
            }

            command.CommandText = @"
            SELECT COUNT(*) 
            FROM Product 
            WHERE IdProduct = @IdProduct";
            command.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);

            if ((int)await command.ExecuteScalarAsync() == 0)
                throw new ArgumentException("Product not found");

            command.Parameters.Clear();
            command.CommandText = @"
            SELECT COUNT(*) 
            FROM Warehouse 
            WHERE IdWarehouse = @IdWarehouse";
            command.Parameters.AddWithValue("@IdWarehouse", warehouse.IdWarehouse);

            if ((int)await command.ExecuteScalarAsync() == 0)
                throw new ArgumentException("Warehouse not found");

            command.Parameters.Clear();
            command.CommandText = @"
            SELECT IdOrder 
            FROM Order
            WHERE IdProduct = @IdProduct
            AND Amount = @Amount
            AND CreatedAt<@CreatedAt";
            command.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);
            command.Parameters.AddWithValue("@Amount", warehouse.Amount);
            command.Parameters.AddWithValue("@CreatedAt", warehouse.CreatedAt);

            var orderinho = await command.ExecuteScalarAsync();
            if (orderinho == null)
                throw new ArgumentException("Order not found");
            

            command.Parameters.Clear();
            command.CommandText = @"
            SELECT IdOrder
            FROM Product_Warehouse
            WHERE IdOrder = @IdOrder";
            command.Parameters.AddWithValue("@IdOrder", orderinho);
            if ((int)await command.ExecuteScalarAsync() > 0)
                throw new ArgumentException("Order already fulfilled");

            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }


    public async Task<int> AddProductToWarehouse(WarehouseDto warehouse)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        await command.Connection.OpenAsync();

        command.CommandText = "AddProductToWarehouse";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);
        command.Parameters.AddWithValue("@IdWarehouse", warehouse.IdWarehouse);
        command.Parameters.AddWithValue("@Amount", warehouse.Amount);
        command.Parameters.AddWithValue("@CreatedAt", warehouse.CreatedAt);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}