using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Danh mục phiếu (port từ SkyCS: Mst_TicketStatus / Mst_TicketPriority /
/// Mst_TicketSource / Mst_ReceptionChannel — 11.BackEnd/V10/idn.SkyCS.Biz/Master.cs).
/// Quản trị tập trung 4 danh mục dùng chung cho eTicket: trạng thái, mức ưu tiên,
/// nguồn phiếu và kênh tiếp nhận. Mỗi dòng có mã, tên cho agent, tên cho khách,
/// phạm vi sử dụng (FlagUseType) và trạng thái hoạt động (FlagActive).
/// </summary>
public class TicketCatalogController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(TicketCatalogKind? kind, bool? active, string? q)
    {
        ViewBag.Kind = kind; ViewBag.Active = active; ViewBag.Q = q;
        ViewBag.Stats = await svc.TicketCatalogStatsAsync();
        return View(await svc.TicketCatalogsAsync(kind, active, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var c = await svc.TicketCatalogGetAsync(id);
        if (c == null) return NotFound();
        return View(c);
    }

    public IActionResult Create(TicketCatalogKind? kind) =>
        View("Edit", new TicketCatalog { Kind = kind ?? TicketCatalogKind.Status, IsActive = true, UseType = CatalogUseType.Type2 });

    public async Task<IActionResult> Edit(int id)
    {
        var c = await svc.TicketCatalogGetAsync(id);
        if (c == null) return NotFound();
        return View(c);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(TicketCatalog model)
    {
        if (string.IsNullOrWhiteSpace(model.AgentName))
        {
            TempData["Error"] = "Cần tên hiển thị cho agent.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }
        if (string.IsNullOrWhiteSpace(model.CustomerName))
        {
            TempData["Error"] = "Cần tên hiển thị cho khách hàng.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        var id = await svc.TicketCatalogSaveAsync(model);
        TempData["Success"] = "Đã lưu danh mục phiếu.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.TicketCatalogToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái danh mục.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
