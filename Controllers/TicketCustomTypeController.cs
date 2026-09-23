using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Loại phiếu tùy chỉnh (port từ SkyCS: Mst_TicketCustomType —
/// 11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_TicketCustomType_Get`/
/// `Mst_TicketCustomType_GetByTicketType`/`Mst_TicketCustomType_Save`;
/// model 12.Dev.Common/idn.SkyCS.Common/Models/Mst_TicketCustomType.cs,
/// Mst_TicketTypeMapCustom.cs).
/// Là phân loại con của eTicket: mỗi loại có mã, tên cho agent, tên cho khách,
/// phạm vi sử dụng (FlagUseType) và trạng thái. Loại tùy chỉnh được gán cho
/// từng "phân loại nghiệp vụ" (Mst_TicketType) qua bảng map Mst_TicketTypeMapCustom.
/// </summary>
public class TicketCustomTypeController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, CatalogUseType? useType, string? q)
    {
        ViewBag.Active = active; ViewBag.UseType = useType; ViewBag.Q = q;
        ViewBag.Stats = await svc.TicketCustomTypeStatsAsync();
        return View(await svc.TicketCustomTypesAsync(active, useType, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var c = await svc.TicketCustomTypeGetAsync(id);
        if (c == null) return NotFound();
        return View(c);
    }

    public IActionResult Create() =>
        View("Edit", new TicketCustomType { IsActive = true, UseType = CatalogUseType.Type2 });

    public async Task<IActionResult> Edit(int id)
    {
        var c = await svc.TicketCustomTypeGetAsync(id);
        if (c == null) return NotFound();
        return View(c);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(TicketCustomType model, List<string>? mapTypeCode)
    {
        // Theo Mst_TicketCustomType_Save: tên cho agent và cho khách bắt buộc.
        if (string.IsNullOrWhiteSpace(model.AgentName))
        {
            TempData["Error"] = "Cần tên loại tùy chỉnh cho agent.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }
        if (string.IsNullOrWhiteSpace(model.CustomerName))
        {
            TempData["Error"] = "Cần tên loại tùy chỉnh cho khách hàng.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        var maps = (mapTypeCode ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => new TicketCustomTypeMap { TicketTypeCode = x.Trim() })
            .ToList();

        var id = await svc.TicketCustomTypeSaveAsync(model, maps);
        TempData["Success"] = "Đã lưu loại phiếu tùy chỉnh.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.TicketCustomTypeToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái loại phiếu tùy chỉnh.";
        return RedirectToAction(nameof(Details), new { id });
    }
}