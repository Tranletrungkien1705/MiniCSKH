using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>Cổng tự phục vụ cho khách hàng: tra cứu KB + tự tạo phiếu.</summary>
public class PortalController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        ViewBag.Articles = await svc.KbListAsync(q, null);
        return View();
    }

    public async Task<IActionResult> Article(int id)
    {
        var a = await svc.KbGetAsync(id, countView: true);
        return a == null ? NotFound() : View(a);
    }

    [HttpGet]
    public IActionResult NewTicket() => View(new Ticket());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> NewTicket(Ticket model)
    {
        if (string.IsNullOrWhiteSpace(model.Subject) || string.IsNullOrWhiteSpace(model.CustomerName))
        {
            TempData["Error"] = "Vui lòng nhập tiêu đề và họ tên.";
            return View(model);
        }
        model.Channel = Channel.Web;
        var id = await svc.CreateAsync(model);
        var t = await svc.GetAsync(id);
        return View("TicketCreated", t);
    }
}
