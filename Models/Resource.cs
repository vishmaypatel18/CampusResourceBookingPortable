using System.ComponentModel.DataAnnotations;

namespace CampusResourceBooking.Models;

public class Resource
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    [Required, StringLength(40)]
    public string Category { get; set; } = "Classroom";

    [Required, StringLength(120)]
    public string Location { get; set; } = "";

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(1, 1000)]
    public int Capacity { get; set; } = 1;

    public bool IsAvailable { get; set; } = true;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
