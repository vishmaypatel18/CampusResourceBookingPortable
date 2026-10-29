using CampusResourceBooking.Models;

namespace CampusResourceBooking.Services;

public interface IBookingService
{
    Task<(bool Success, string Message, Booking? Booking)> CreateAsync(Booking booking);
    Task<(bool Success, string Message)> ApproveAsync(int bookingId);
}
