using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

public class HomeController : Controller
{
    // SPA React (agent) ở "/". Portal khách tự phục vụ /Portal (Razor) giữ nguyên.
    public IActionResult Index() => Redirect("/index.html");
}

public class LegacyController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Stats = await svc.DashboardAsync();
        ViewBag.Recent = (await svc.ListAsync(null, null, null, null)).Take(8).ToList();
        return View("~/Views/Home/Index.cshtml");
    }
}
