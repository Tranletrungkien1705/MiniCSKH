using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

public class KbController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(string? q, string? category)
    {
        ViewBag.Categories = await svc.KbCategoriesAsync();
        ViewBag.Q = q; ViewBag.Category = category;
        return View(await svc.KbListAsync(q, category));
    }

    public async Task<IActionResult> Details(int id)
    {
        var a = await svc.KbGetAsync(id, countView: true);
        return a == null ? NotFound() : View(a);
    }
}
