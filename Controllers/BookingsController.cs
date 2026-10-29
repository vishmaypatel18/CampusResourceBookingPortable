using System.Security.Claims;
using CampusResourceBooking.Data;
using CampusResourceBooking.Models;
using CampusResourceBooking.Services;
using CampusResourceBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CampusResourceBooking.Controllers;

[Authorize]
public class BookingsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IBookingService _bookingService;
    public BookingsController(ApplicationDbContext db, IBookingService bookingService)
    { _db = db; _bookingService = bookingService; }

    private async Task LoadResources(BookingCreateViewModel model)
    {
        model.Resources = await _db.Resources.Where(r => r.IsAvailable).OrderBy(r => r.Name)
            .Select(r => new SelectListItem($"{r.Name} — {r.Location}", r.Id.ToString())).ToListAsync();
    }

    public async Task<IActionResult> Create()
    {
        var model = new BookingCreateViewModel();
        await LoadResources(model);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingCreateViewModel model)
    {
        await LoadResources(model);
        if (!ModelState.IsValid) return View(model);

        var booking = new Booking
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            ResourceId = model.ResourceId!.Value,
            BookingDate = model.BookingDate.Date,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            Purpose = model.Purpose.Trim()
        };
        var result = await _bookingService.CreateAsync(booking);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }
        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(MyBookings));
    }

    public async Task<IActionResult> MyBookings()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var bookings = await _db.Bookings.AsNoTracking().Include(b => b.Resource)
            .Where(b => b.UserId == userId).OrderByDescending(b => b.CreatedAt).ToListAsync();
        return View(bookings);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var booking = await _db.Bookings.SingleOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        if (booking is null) return NotFound();
        if (booking.Status is not (BookingStatus.Pending or BookingStatus.Approved))
        {
            TempData["Error"] = "This booking cannot be cancelled.";
            return RedirectToAction(nameof(MyBookings));
        }
        if (booking.BookingDate.Date < DateTime.Today ||
            (booking.BookingDate.Date == DateTime.Today && booking.StartTime <= DateTime.Now.TimeOfDay))
        {
            TempData["Error"] = "A booking that has already started or passed cannot be cancelled.";
            return RedirectToAction(nameof(MyBookings));
        }
        booking.Status = BookingStatus.Cancelled;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Booking cancelled.";
        return RedirectToAction(nameof(MyBookings));
    }
}
