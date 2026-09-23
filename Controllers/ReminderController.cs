using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Thiết lập nhắc nhở phiếu (Mst_EstablishRemindETicket — port từ SkyCS).
/// Cấu hình kênh thông báo khi phiếu tới hạn / quá hạn xử lý: bật/tắt 4 kênh
/// (Hệ thống/Email/SMS/Zalo) và gắn mẫu nội dung (SubFormCode) cho từng kênh.
/// </summary>
public class ReminderController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, RemindChannel? channel, string? q)
    {
        ViewBag.Active = active; ViewBag.Channel = channel; ViewBag.Q = q;
        ViewBag.Stats = await svc.ReminderStatsAsync();
        return View(await svc.ReminderRulesAsync(active, channel, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var r = await svc.ReminderRuleGetAsync(id);
        if (r == null) return NotFound();
        return View(r);
    }

    public IActionResult Create() => View("Edit", new ReminderRule { IsActive = true, NotifySystem = true });

    public async Task<IActionResult> Edit(int id)
    {
        var r = await svc.ReminderRuleGetAsync(id);
        if (r == null) return NotFound();
        return View(r);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ReminderRule model)
    {
        if (string.IsNullOrWhiteSpace(model.EstablishId))
        {
            TempData["Error"] = "Cần mã thiết lập nhắc nhở.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }
        if (model.ChannelCount == 0)
        {
            TempData["Error"] = "Cần bật ít nhất một kênh nhắc nhở.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        var id = await svc.ReminderRuleSaveAsync(model);
        TempData["Success"] = "Đã lưu thiết lập nhắc nhở phiếu.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.ReminderRuleToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái thiết lập nhắc nhở.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
