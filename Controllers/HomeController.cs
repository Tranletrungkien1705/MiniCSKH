using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

public class HomeController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Stats = await svc.DashboardAsync();
        ViewBag.Recent = (await svc.ListAsync(null, null, null, null)).Take(8).ToList();
        return View();
    }
}
