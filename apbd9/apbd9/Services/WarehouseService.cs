using System.Data;
using System.Data.Common;
using apbd9.Models;
using Microsoft.Data.SqlClient;


namespace apbd9.Services;

public class WarehouseService : IWarehouseService
{
    private readonly string _connectionString =
        "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;";


    public async Task<int> AddProductAsync(WarehouseDto warehouse)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        await connection.OpenAsync();

        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
        if (command.Transaction == null)
            throw new Exception("Transaction assignment failed");


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
            FROM [Order]
            WHERE IdProduct = @IdProduct
            AND Amount = @Amount
            AND CreatedAt<@CreatedAt";
            command.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);
            command.Parameters.AddWithValue("@Amount", warehouse.Amount);
            command.Parameters.AddWithValue("@CreatedAt", warehouse.CreatedAt);

            var orderinho = await command.ExecuteScalarAsync();
            if (orderinho == null)
                throw new ArgumentException("Order not found");
            
            int orderId = Convert.ToInt32(orderinho);

            
            command.Parameters.Clear();
            command.CommandText = @"
            SELECT IdOrder
            FROM Product_Warehouse
            WHERE IdOrder = @IdOrder";
            command.Parameters.AddWithValue("@IdOrder",orderId);
            if (await command.ExecuteScalarAsync() != null )
                throw new ArgumentException("Order already fulfilled");

            command.Parameters.Clear();
            command.CommandText = @"
            UPDATE [Order]
            SET FulfilledAt =@Date
            WHERE IdOrder = @IdOrder";
            command.Parameters.AddWithValue("@Date", DateTime.Now);
            command.Parameters.AddWithValue("@IdOrder", orderId);
            await command.ExecuteNonQueryAsync();
            
            command.Parameters.Clear();
            command.CommandText = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";
            command.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);
            var priceObj = await command.ExecuteScalarAsync();

            if (priceObj == null)
                throw new ArgumentException("Cannot retrieve product price.");
            
            decimal price = Convert.ToDecimal(priceObj);
            decimal totalPrice = price * warehouse.Amount;
            
            command.Parameters.Clear();
            command.CommandText = @"
            INSERT INTO Product_Warehouse(IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
            VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt);
            SELECT SCOPE_IDENTITY();";

            command.Parameters.AddWithValue("@IdWarehouse", warehouse.IdWarehouse);
            command.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);
            command.Parameters.AddWithValue("@IdOrder", orderId);
            command.Parameters.AddWithValue("@Amount", warehouse.Amount);
            command.Parameters.AddWithValue("@Price", totalPrice);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

            var result = await command.ExecuteScalarAsync();
            await transaction.CommitAsync();
            return Convert.ToInt32(result);

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