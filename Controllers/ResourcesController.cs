using CampusResourceBooking.Data;
using CampusResourceBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusResourceBooking.Controllers;

public class ResourcesController : Controller
{
    private readonly ApplicationDbContext _db;
    public ResourcesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? search, string? category, string? location)
    {
        var q = _db.Resources.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) q = q.Where(r => r.Name.Contains(search));
        if (!string.IsNullOrWhiteSpace(category)) q = q.Where(r => r.Category == category);
        if (!string.IsNullOrWhiteSpace(location)) q = q.Where(r => r.Location.Contains(location));
        ViewBag.Search = search; ViewBag.Category = category; ViewBag.Location = location;
        ViewBag.Categories = await _db.Resources.Select(r => r.Category).Distinct().OrderBy(x => x).ToListAsync();
        return View(await q.OrderBy(r => r.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var resource = await _db.Resources.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        return resource is null ? NotFound() : View(resource);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View(new Resource());

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Resource resource)
    {
        if (!ModelState.IsValid) return View(resource);
        _db.Resources.Add(resource);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Resource created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var resource = await _db.Resources.FindAsync(id);
        return resource is null ? NotFound() : View(resource);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, Resource resource)
    {
        if (id != resource.Id) return BadRequest();
        if (!ModelState.IsValid) return View(resource);
        _db.Entry(resource).State = EntityState.Modified;
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { if (!await _db.Resources.AnyAsync(r => r.Id == id)) return NotFound(); throw; }
        TempData["Success"] = "Resource updated.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var resource = await _db.Resources.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        return resource is null ? NotFound() : View(resource);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var resource = await _db.Resources.FindAsync(id);
        if (resource is null) return NotFound();
        if (await _db.Bookings.AnyAsync(b => b.ResourceId == id))
        {
            TempData["Error"] = "This resource has booking history and cannot be deleted. Mark it unavailable instead.";
            return RedirectToAction(nameof(Index));
        }
        _db.Resources.Remove(resource);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Resource deleted.";
        return RedirectToAction(nameof(Index));
    }
}
