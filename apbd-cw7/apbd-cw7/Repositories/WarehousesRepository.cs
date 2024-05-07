using System.Data;
using apbd_cw7.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace apbd_cw7.Repositories;

public class WarehousesRepository : IWarehousesRepository
{
    private readonly IConfiguration _configuration;

    public WarehousesRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> DoesProductExist(int idProduct)
    {
        var query = "select 1 from product where IdProduct = @id";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@id", idProduct);

        await connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();

        return result is not null;
    }

    public async Task<bool> DoesWarehouseExist(int IdWarehouse)
    {
        var query = "select 1 from warehouse where IdWarehouse = @id";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@id", IdWarehouse);

        await connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();

        return result is not null;
    }

    public async Task<bool> OrderAmountPositive(int amount)
    {
        return amount > 0;
    }

    public async Task<double> GetPriceOfProduct(int IdProduct)
    {
        var query = "select price from product where IdProduct = @id";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@id", IdProduct);

        await connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();

        return Convert.ToDouble(result);
    }

    public async Task<int?> GetOrderOfProduct(int IdProduct, int amount)
    {
        var query =
            "SELECT TOP 1 IdOrder FROM [Order] WHERE IdProduct = @id AND Amount = @amount AND FulfilledAt IS NULL";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@id", IdProduct);
        command.Parameters.AddWithValue("@amount", amount);

        await connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();

        return result != null ? Convert.ToInt32(result) : (int?)null;
    }


    public async Task<int> AddProductToWarehouse(NewProductInWarehouse newProductInWarehouse, int? idOrder,
        double price)
    {
        var insert =
            @"insert into product_warehouse values(@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt); select @@identity as IdProductWarehouse";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = insert;
        command.Parameters.AddWithValue("@IdWarehouse", newProductInWarehouse.IdWarehouse);
        command.Parameters.AddWithValue("@IdProduct", newProductInWarehouse.IdProduct);
        command.Parameters.AddWithValue("@IdOrder", idOrder);
        command.Parameters.AddWithValue("@Amount", newProductInWarehouse.Amount);
        command.Parameters.AddWithValue("@Price", price * newProductInWarehouse.Amount);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

        await connection.OpenAsync();

        var id = await command.ExecuteScalarAsync();

        if (id is null) throw new Exception();

        return Convert.ToInt32(id);
    }

    public async Task UpdateFullfilledAt(int idOrder)
    {
        var update = "UPDATE [Order] SET FulfilledAt = CURRENT_TIMESTAMP WHERE IdOrder = @idOrder";
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = update;

        command.Parameters.AddWithValue("@idOrder", idOrder);

        await connection.OpenAsync();

        await command.ExecuteScalarAsync();
    }

    public async Task AddProductToWarehouseWithStoredProcedure(int idProduct, int idWarehouse, int amount,
        double price)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.CommandText = "AddProductToWarehouse";
        command.CommandType = CommandType.StoredProcedure;


        // Add parameters
        command.Parameters.AddWithValue("@IdProduct", idProduct);
        command.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
        command.Parameters.AddWithValue("@Amount", amount);
        command.Parameters.AddWithValue("@Price", price);
        await command.ExecuteNonQueryAsync();
    }
}

public interface IWarehousesRepository
{
    Task<bool> DoesProductExist(int id);
    Task<bool> DoesWarehouseExist(int id);
    Task<bool> OrderAmountPositive(int id);
    Task<double> GetPriceOfProduct(int id);
    public Task<int?> GetOrderOfProduct(int id, int amount);
    Task<int> AddProductToWarehouse(NewProductInWarehouse newProductInWarehouse, int? idOrder, double price);
    Task UpdateFullfilledAt(int idOrder);

    Task AddProductToWarehouseWithStoredProcedure(int idProduct, int idWarehouse, int amount, double price);
}