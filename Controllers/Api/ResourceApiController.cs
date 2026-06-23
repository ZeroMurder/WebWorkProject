using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebWorkNew.Data;
using WebWorkNew.Enums;

namespace WebWorkNew.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResourceApiController : ControllerBase
{
    private readonly AppDbContext _db;

    public ResourceApiController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees()
    {
        var employees = await _db.Employees
            .Select(e => new
            {
                id = e.Id,
                name = $"{e.Surname} {e.Name}",
                fullName = $"{e.Surname} {e.Name} {e.Patronymic}".Trim(),
                position = e.Position,
                salary = e.MonthlySalary,
                taxRate = e.TaxRate
            })
            .OrderBy(e => e.name)
            .ToListAsync();

        return Ok(employees);
    }

    [HttpGet("executors")]
    public async Task<IActionResult> GetExecutors()
    {
        var executors = await _db.Executors
            .Select(e => new
            {
                id = e.Id,
                name = e.FullName,
                fullName = e.FullName,
                employmentType = e.EmploymentType.ToString(),
                unit = e.Unit.ToString(),
                cost = e.CostPerUnit,
                taxRate = e.TaxRate
            })
            .OrderBy(e => e.name)
            .ToListAsync();

        return Ok(executors);
    }

    [HttpGet("subcontractors")]
    public async Task<IActionResult> GetSubcontractors()
    {
        var subcontractors = await _db.Subcontractors
            .Select(s => new
            {
                id = s.Id,
                name = s.Name,
                fullName = s.Name,
                inn = s.Inn,
                contact = s.ContactName,
                unit = s.Unit.ToString(),
                cost = s.CostPerUnit,
                taxRate = s.TaxRate
            })
            .OrderBy(s => s.name)
            .ToListAsync();

        return Ok(subcontractors);
    }

    [HttpGet("equipment")]
    public async Task<IActionResult> GetEquipment()
    {
        var equipment = await _db.Equipments
            .Select(e => new
            {
                id = e.Id,
                name = e.Title,
                title = e.Title,
                description = e.Description,
                acquisitionType = e.AcquisitionType.ToString(),
                unit = e.Unit.ToString(),
                cost = e.CostPerUnit,
                operationalCost = e.OperationalCost
            })
            .OrderBy(e => e.name)
            .ToListAsync();

        return Ok(equipment);
    }

    // GET: api/ResourceApi/by-type?type=1..4
    [HttpGet("by-type")]
    public async Task<IActionResult> GetByType([FromQuery] ResourceType type)
    {
        return type switch
        {
            ResourceType.Employee => Ok(await GetEmployeesInternalTyped()),
            ResourceType.Executor => Ok(await GetExecutorsInternalTyped()),
            ResourceType.Subcontractor => Ok(await GetSubcontractorsInternalTyped()),
            ResourceType.Equipment => Ok(await GetEquipmentInternalTyped()),
            _ => BadRequest("Неизвестный тип ресурса")
        };
    }

    private async Task<List<EmployeeDto>> GetEmployeesInternalTyped() =>
        await _db.Employees
            .Select(e => new EmployeeDto(
                e.Id,
                $"{e.Surname} {e.Name}",
                $"{e.Surname} {e.Name} {e.Patronymic}".Trim(),
                e.Position,
                e.MonthlySalary,
                e.TaxRate
            ))
            .OrderBy(e => e.name)
            .ToListAsync();

    private async Task<List<ExecutorDto>> GetExecutorsInternalTyped() =>
        await _db.Executors
            .Select(e => new ExecutorDto(
                e.Id,
                e.FullName,
                e.FullName,
                e.EmploymentType.ToString(),
                e.Unit.ToString(),
                e.CostPerUnit,
                e.TaxRate
            ))
            .OrderBy(e => e.name)
            .ToListAsync();

    private async Task<List<SubcontractorDto>> GetSubcontractorsInternalTyped() =>
        await _db.Subcontractors
            .Select(s => new SubcontractorDto(
                s.Id,
                s.Name,
                s.Name,
                s.Inn,
                s.ContactName,
                s.Unit.ToString(),
                s.CostPerUnit,
                s.TaxRate
            ))
            .OrderBy(s => s.name)
            .ToListAsync();

    private async Task<List<EquipmentDto>> GetEquipmentInternalTyped() =>
        await _db.Equipments
            .Select(e => new EquipmentDto(
                e.Id,
                e.Title,
                e.Title,
                e.Description,
                e.AcquisitionType.ToString(),
                e.Unit.ToString(),
                e.CostPerUnit,
                e.OperationalCost
            ))
            .OrderBy(e => e.name)
            .ToListAsync();

    private record EmployeeDto(int id, string name, string fullName, string position, decimal salary, decimal taxRate);
    private record ExecutorDto(int id, string name, string fullName, string employmentType, string unit, decimal cost, decimal taxRate);
    private record SubcontractorDto(int id, string name, string fullName, string inn, string contact, string unit, decimal cost, decimal taxRate);
    private record EquipmentDto(int id, string name, string title, string description, string acquisitionType, string unit, decimal cost, decimal? operationalCost);

}

