using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Kênh liên hệ (port từ SkyCS: Mst_ContactChannel —
/// 11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_ContactChannel_Get`/`_Save`;
/// model 12.Dev.Common/idn.SkyCS.Common/Models/Mst_ContactChannel.cs).
/// Master data quy định CÁCH liên hệ với khách hàng (gọi điện, email, Zalo,
/// SMS, gặp trực tiếp…), khác với "kênh tiếp nhận" (Mst_ReceptionChannel —
/// nơi phiếu được tạo đến). Mỗi kênh có mã, tên cho agent, tên cho khách,
/// phạm vi sử dụng (FlagUseType) và trạng thái hoạt động (FlagActive).
/// </summary>
public class ContactChannelController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, CatalogUseType? useType, string? q)
    {
        ViewBag.Active = active; ViewBag.UseType = useType; ViewBag.Q = q;
        ViewBag.Stats = await svc.ContactChannelStatsAsync();
        return View(await svc.ContactChannelsAsync(active, useType, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var c = await svc.ContactChannelGetAsync(id);
        if (c == null) return NotFound();
        return View(c);
    }

    public IActionResult Create() =>
        View("Edit", new ContactChannel { IsActive = true, UseType = CatalogUseType.Type2 });

    public async Task<IActionResult> Edit(int id)
    {
        var c = await svc.ContactChannelGetAsync(id);
        if (c == null) return NotFound();
        return View(c);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ContactChannel model)
    {
        // Theo Mst_ContactChannel_Save: tên cho agent và cho khách bắt buộc.
        if (string.IsNullOrWhiteSpace(model.AgentName))
        {
            TempData["Error"] = "Cần tên kênh cho agent.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }
        if (string.IsNullOrWhiteSpace(model.CustomerName))
        {
            TempData["Error"] = "Cần tên kênh cho khách hàng.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        var id = await svc.ContactChannelSaveAsync(model);
        TempData["Success"] = "Đã lưu kênh liên hệ.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.ContactChannelToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái kênh liên hệ.";
        return RedirectToAction(nameof(Details), new { id });
    }
}