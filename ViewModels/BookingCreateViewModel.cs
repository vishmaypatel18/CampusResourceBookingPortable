using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CampusResourceBooking.ViewModels;

public class BookingCreateViewModel : IValidatableObject
{
    [Required]
    public int? ResourceId { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime BookingDate { get; set; } = DateTime.Today.AddDays(1);

    [Required]
    public TimeSpan StartTime { get; set; } = new(9, 0, 0);

    [Required]
    public TimeSpan EndTime { get; set; } = new(10, 0, 0);

    [Required, StringLength(300, MinimumLength = 5)]
    public string Purpose { get; set; } = "";

    public IEnumerable<SelectListItem> Resources { get; set; } = Array.Empty<SelectListItem>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (BookingDate.Date < DateTime.Today)
            yield return new ValidationResult("Booking date cannot be in the past.", new[] { nameof(BookingDate) });
        if (EndTime <= StartTime)
            yield return new ValidationResult("End time must be later than start time.", new[] { nameof(EndTime) });
        if (StartTime < TimeSpan.Zero || EndTime > TimeSpan.FromDays(1))
            yield return new ValidationResult("Enter valid times between 00:00 and 24:00.", new[] { nameof(StartTime), nameof(EndTime) });
    }
}
