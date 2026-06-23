using Microsoft.AspNetCore.Mvc;

namespace WebWorkNew.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Projects");
    }
}