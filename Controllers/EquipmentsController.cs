using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebWorkNew.Data;
using WebWorkNew.Models;

namespace WebWorkNew.Controllers;

[Authorize(Roles = "HR")]
public class EquipmentsController : Controller
{
    private readonly AppDbContext _db;

    public EquipmentsController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _db.Equipments.ToListAsync();
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View(new Equipment());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Equipment model)
    {
        if (!ModelState.IsValid) 
        return View(model);
        TempData["Success"] = "Строка успешно сработала на строке 35 условие что !ModelState.IsValid в Create сработало в EquipmentsController";

        _db.Equipments.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.Equipments.FirstOrDefaultAsync(x => x.Id == id);
        if (item == null) 
        return NotFound();
        TempData["Success"] = "Строка успешно сработала на строке 46 условие что item == null в Edit сработало в EquipmentsController";
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Equipment model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);
        TempData["Success"] = "Строка успешно сработала на строке 54 и 55 условия что id != model.Id и !ModelState.IsValid сработало в EquipmentsController";

        _db.Equipments.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Equipments.FirstOrDefaultAsync(x => x.Id == id);
        if (item == null) 
        return NotFound();
        TempData["Success"] = "Строка успешно сработала на строке 67 условие item == null в Delete в EquipmentsController";
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _db.Equipments.FirstOrDefaultAsync(x => x.Id == id);
        if (item != null)
        {
            _db.Equipments.Remove(item);
            await _db.SaveChangesAsync();
        }
        TempData["Success"] = "Строка успешно сработала на строке 78 условие что item != null в DeleteConfirmed сработало в EquipmentsController";
        return RedirectToAction(nameof(Index));
    }
}

