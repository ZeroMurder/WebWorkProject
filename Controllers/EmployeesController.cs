using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebWorkNew.Data;
using WebWorkNew.Models;

namespace WebWorkNew.Controllers;

[Authorize(Roles = "HR")]
public class EmployeesController : Controller
{
    private readonly AppDbContext _db;

    public EmployeesController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _db.Employees.ToListAsync();
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View(new Employee());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Employee model)
    {
        if (!ModelState.IsValid) return View(model);

        _db.Employees.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.Employees.FirstOrDefaultAsync(x => x.Id == id);
        if (item == null) 
        return NotFound();
                TempData["Success"] = "Строка успешно сработала на строке 48 условие edit == null в Edit в EmployeesController";
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Employee model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);

        _db.Employees.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Employees.FirstOrDefaultAsync(x => x.Id == id);
        if (item == null) 
        return NotFound();
        TempData["Success"] = "Строка успешно сработала на строке 68 условие item == null в Delete в EmployeesController";
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _db.Employees.FirstOrDefaultAsync(x => x.Id == id);
        if (item != null)
        {
            _db.Employees.Remove(item);
            await _db.SaveChangesAsync();
        }
        TempData["Success"] = "Строка 80 DeleteConfirmed работает в EmployeesController";
        return RedirectToAction(nameof(Index));
    }
}

