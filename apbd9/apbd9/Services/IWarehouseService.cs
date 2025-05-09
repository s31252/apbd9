using apbd9.Models;

namespace apbd9.Services;

public interface IWarehouseService
{
    Task AddProductAsync(WarehouseDto warehouse);
    Task<int> AddProductToWarehouse(WarehouseDto warehouse);
}