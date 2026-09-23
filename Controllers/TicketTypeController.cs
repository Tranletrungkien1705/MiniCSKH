using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Phân loại nghiệp vụ eTicket (Mst_TicketType — port từ SkyCS).
/// Mỗi phân loại là một mã nghiệp vụ dùng để phân loại phiếu, gắn mẫu bố cục
/// màn hình tạo/chi tiết và loại nghiệp vụ (eTicket/Chiến dịch); có tên riêng
/// cho agent và cho khách hàng.
/// </summary>
public class TicketTypeController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, BusinessType? businessType, string? q)
    {
        ViewBag.Active = active; ViewBag.BusinessType = businessType; ViewBag.Q = q;
        ViewBag.Stats = await svc.TicketTypeStatsAsync();
        return View(await svc.TicketTypesAsync(active, businessType, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var t = await svc.TicketTypeGetAsync(id);
        if (t == null) return NotFound();
        return View(t);
    }

    public IActionResult Create() => View("Edit", new TicketType { IsActive = true });

    public async Task<IActionResult> Edit(int id)
    {
        var t = await svc.TicketTypeGetAsync(id);
        if (t == null) return NotFound();
        return View(t);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(TicketType model)
    {
        if (string.IsNullOrWhiteSpace(model.AgentName))
        {
            TempData["Error"] = "Cần tên phân loại (cho agent).";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }
        if (string.IsNullOrWhiteSpace(model.CustomerName)) model.CustomerName = model.AgentName;
        var id = await svc.TicketTypeSaveAsync(model);
        TempData["Success"] = "Đã lưu phân loại nghiệp vụ.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.TicketTypeToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái phân loại.";
        return RedirectToAction(nameof(Details), new { id });
    }
}