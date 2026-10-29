namespace CampusResourceBooking.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalResources { get; set; }
    public int TotalBookings { get; set; }
    public int PendingBookings { get; set; }
    public int ApprovedBookings { get; set; }
    public int RejectedOrCancelledBookings { get; set; }
}
