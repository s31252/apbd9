using apbd9.Models;
using apbd9.Services;
using Microsoft.AspNetCore.Mvc;

namespace apbd9.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehouseController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddProductAsync(WarehouseDto warehouse)
    {
        try
        {
            var newId = await _warehouseService.AddProductAsync(warehouse);
            return Ok(new { NewId = newId });
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e )
        {
            return StatusCode(500, "Something went wrong "+e.Message);
        }
    }
    
    

    [HttpPost("addFromProcedure")]
    public async Task<IActionResult> AddProductUsingProcedure([FromBody]WarehouseDto warehouse)
    {
        
        try
        {
            var newId = await _warehouseService.AddProductToWarehouse(warehouse);
            return Ok(new { NewId = newId });
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e )
        {
            return StatusCode(500, "Something went wrong " +e.Message );
        }
    }
    
    
}