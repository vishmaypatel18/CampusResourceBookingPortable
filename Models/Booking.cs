using System.ComponentModel.DataAnnotations;

namespace CampusResourceBooking.Models;

public class Booking
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }

    [Required]
    public int ResourceId { get; set; }
    public Resource? Resource { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime BookingDate { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required, StringLength(300, MinimumLength = 5)]
    public string Purpose { get; set; } = "";

    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [StringLength(300)]
    public string? AdminNote { get; set; }
}
