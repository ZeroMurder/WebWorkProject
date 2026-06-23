using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebWorkNew.Data;
using WebWorkNew.Enums;
using WebWorkNew.Models;

namespace WebWorkNew.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourceController : ControllerBase
{
    private readonly AppDbContext _db;
    
    public ResourceController(AppDbContext db) => _db = db;
    
    [HttpGet("list")]
    public async Task<IActionResult> GetResourceList(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Employee:
                return Ok(await _db.Employees.Select(e => new { 
                    id = e.Id, 
                    name = $"{e.Surname} {e.Name}", 
                    position = e.Position 
                }).ToListAsync());
            case ResourceType.Executor:
                return Ok(await _db.Executors.Select(e => new { 
                    id = e.Id, 
                    name = e.FullName,
                    unit = e.Unit.ToString(),
                    cost = e.CostPerUnit
                }).ToListAsync());
            // и т.д.
        }
        return BadRequest();
    }
}