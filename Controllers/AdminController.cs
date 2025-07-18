using Microsoft.AspNetCore.Mvc;
using Hotel.Services;

namespace Hotel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly SeedService _seedService;

    public AdminController(SeedService seedService)
    {
        _seedService = seedService;
    }

    [HttpPost("seed/small")]
    public async Task<IActionResult> SeedSmallData()
    {
        try
        {
            await _seedService.SeedFromSqlFileAsync("small_seed.sql");
            var summary = await _seedService.GetDataSummaryAsync();
            return Ok(new
            {
                message = "Small seed data loaded successfully",
                summary
            });
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("seed/large")]
    public async Task<IActionResult> SeedLargeData()
    {
        try
        {
            await _seedService.SeedFromSqlFileAsync("large_seed.sql");
            var summary = await _seedService.GetDataSummaryAsync();
            return Ok(new
            {
                message = "Large seed data loaded successfully",
                summary
            });
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("seed/clear")] 
    public async Task<IActionResult> ClearAllData()
    {
        try
        {
            await _seedService.ClearAllDataAsync();
            var summary = await _seedService.GetDataSummaryAsync();
            return Ok(new
            {
                message = "All data cleared successfully",
                summary
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    [HttpGet("data/summary")]
    public async Task<IActionResult> GetDataSummary()
    {
        try
        {
            var summary = await _seedService.GetDataSummaryAsync();
            return Ok(summary);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

}