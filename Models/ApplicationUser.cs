using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CampusResourceBooking.Models;

public class ApplicationUser : IdentityUser
{
    [Required, StringLength(80)]
    public string FullName { get; set; } = "";
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
