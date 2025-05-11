using APBD_9_s30958.Models.DTOs;

namespace APBD_9_s30958.Services;

public interface IWarehouseService
{
    Task<int> AddWarehouseProduct(WarehouseDTO warehouseDto);
}