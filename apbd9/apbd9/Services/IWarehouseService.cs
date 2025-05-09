using apbd9.Models;

namespace apbd9.Services;

public interface IWarehouseService
{
    Task<int> AddProductAsync(WarehouseDto warehouse);
    Task<int> AddProductToWarehouse(WarehouseDto warehouse);
}