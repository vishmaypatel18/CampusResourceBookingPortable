using CampusResourceBooking.Data;
using CampusResourceBooking.Models;
using CampusResourceBooking.Services;
using CampusResourceBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusResourceBooking.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IBookingService _bookingService;
    public AdminController(ApplicationDbContext db, IBookingService bookingService)
    { _db = db; _bookingService = bookingService; }

    public async Task<IActionResult> Index()
    {
        var statuses = _db.Bookings.AsNoTracking();
        return View(new AdminDashboardViewModel
        {
            TotalResources = await _db.Resources.CountAsync(),
            TotalBookings = await statuses.CountAsync(),
            PendingBookings = await statuses.CountAsync(b => b.Status == BookingStatus.Pending),
            ApprovedBookings = await statuses.CountAsync(b => b.Status == BookingStatus.Approved),
            RejectedOrCancelledBookings = await statuses.CountAsync(b => b.Status == BookingStatus.Rejected || b.Status == BookingStatus.Cancelled)
        });
    }

    public async Task<IActionResult> Bookings(BookingStatus? status)
    {
        var q = _db.Bookings.AsNoTracking().Include(b => b.Resource).Include(b => b.User).AsQueryable();
        if (status.HasValue) q = q.Where(b => b.Status == status.Value);
        ViewBag.Status = status;
        return View(await q.OrderByDescending(b => b.CreatedAt).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var result = await _bookingService.ApproveAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Bookings));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? note)
    {
        var booking = await _db.Bookings.SingleOrDefaultAsync(b => b.Id == id);
        if (booking is null) return NotFound();
        if (booking.Status != BookingStatus.Pending)
            TempData["Error"] = "Only pending bookings can be rejected.";
        else
        {
            booking.Status = BookingStatus.Rejected;
            booking.AdminNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim()[..Math.Min(note.Trim().Length, 300)];
            await _db.SaveChangesAsync();
            TempData["Success"] = "Booking rejected.";
        }
        return RedirectToAction(nameof(Bookings));
    }
}
