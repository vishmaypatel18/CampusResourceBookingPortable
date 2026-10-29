using Microsoft.AspNetCore.Mvc;

namespace CampusResourceBooking.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Error() => View();
}
