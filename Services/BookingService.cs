using CampusResourceBooking.Data;
using CampusResourceBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusResourceBooking.Services;

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _db;
    public BookingService(ApplicationDbContext db) => _db = db;

    private static bool IsBlocking(BookingStatus status) =>
        status is BookingStatus.Pending or BookingStatus.Approved;

    private async Task<bool> HasConflictAsync(int resourceId, DateTime date, TimeSpan start, TimeSpan end, int? excludeId = null)
    {
        var query = _db.Bookings.Where(b => b.ResourceId == resourceId &&
            b.BookingDate.Date == date.Date &&
            (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Approved) &&
            b.StartTime < end && b.EndTime > start);
        if (excludeId.HasValue) query = query.Where(b => b.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<(bool Success, string Message, Booking? Booking)> CreateAsync(Booking booking)
    {
        if (booking.BookingDate.Date < DateTime.Today)
            return (false, "Booking date cannot be in the past.", null);
        if (booking.EndTime <= booking.StartTime)
            return (false, "End time must be later than start time.", null);

        // Serializable transaction reduces concurrent conflicting submissions.
        await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var resource = await _db.Resources.SingleOrDefaultAsync(r => r.Id == booking.ResourceId);
        if (resource is null || !resource.IsAvailable)
            return (false, "This resource is not available.", null);

        if (await HasConflictAsync(booking.ResourceId, booking.BookingDate, booking.StartTime, booking.EndTime))
            return (false, "This time slot conflicts with a pending or approved booking.", null);

        booking.Status = BookingStatus.Pending;
        booking.CreatedAt = DateTime.UtcNow;
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return (true, "Booking request submitted.", booking);
    }

    public async Task<(bool Success, string Message)> ApproveAsync(int bookingId)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var booking = await _db.Bookings.SingleOrDefaultAsync(b => b.Id == bookingId);
        if (booking is null) return (false, "Booking not found.");
        if (booking.Status != BookingStatus.Pending) return (false, "Only pending bookings can be approved.");

        var resource = await _db.Resources.SingleOrDefaultAsync(r => r.Id == booking.ResourceId);
        if (resource is null || !resource.IsAvailable) return (false, "Resource is unavailable.");
        if (await HasConflictAsync(booking.ResourceId, booking.BookingDate, booking.StartTime, booking.EndTime, booking.Id))
            return (false, "Cannot approve: another pending or approved booking overlaps this slot.");

        booking.Status = BookingStatus.Approved;
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return (true, "Booking approved.");
    }
}
