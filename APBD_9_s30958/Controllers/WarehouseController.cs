using APBD_9_s30958.Models.DTOs;
using APBD_9_s30958.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_9_s30958.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehouseController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    [HttpPost()]
    public async Task<IActionResult> AddWarehouseProduct([FromBody] WarehouseDTO warehouseDto)
    {
        try
        {
            var productWarehouseId = await _warehouseService.AddWarehouseProduct(warehouseDto);
            return CreatedAtAction(nameof(AddWarehouseProduct), new { id = productWarehouseId }, new { IdProductWarehouse = productWarehouseId });
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}