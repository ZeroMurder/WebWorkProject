using Microsoft.AspNetCore.Mvc;

namespace WebWork.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Projects");
    }
}