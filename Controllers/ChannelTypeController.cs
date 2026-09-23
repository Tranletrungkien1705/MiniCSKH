using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Loại kênh (port từ SkyCS: Mst_ChannelType — OmniChannel —
/// 11.BackEnd/V10/idn.SkyCS.Biz/OmniChannel/OminiChannel.cs, `Mst_ChannelType_Get`;
/// model 12.Dev.Common/idn.SkyCS.Common/Models/OminiChannel/Mst_ChannelType.cs;
/// controller 13.ClientGate/V20/idn.SkyCS.WebAPI/Controllers/MstChannelTypeController.cs).
/// Master data của OmniChannel — danh mục các LOẠI kênh liên lạc đa kênh
/// (email, sms, zalo…). Mỗi loại kênh có mã (ChannelType), tên hiển thị
/// (ChannelTypeName) và trạng thái hoạt động (FlagActive). Mst_Channel gắn loại
/// kênh cho eTicket (ChannelTypeETicket) và cho OTP (ChannelTypeOTP).
/// </summary>
public class ChannelTypeController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, string? q)
    {
        ViewBag.Active = active; ViewBag.Q = q;
        ViewBag.Stats = await svc.ChannelTypeStatsAsync();
        return View(await svc.ChannelTypesAsync(active, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var c = await svc.ChannelTypeGetAsync(id);
        if (c == null) return NotFound();
        return View(c);
    }

    public IActionResult Create() =>
        View("Edit", new ChannelType { IsActive = true });

    public async Task<IActionResult> Edit(int id)
    {
        var c = await svc.ChannelTypeGetAsync(id);
        if (c == null) return NotFound();
        return View(c);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ChannelType model)
    {
        // Theo Mst_ChannelType: tên loại kênh bắt buộc.
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["Error"] = "Cần tên loại kênh.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        var id = await svc.ChannelTypeSaveAsync(model);
        TempData["Success"] = "Đã lưu loại kênh.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.ChannelTypeToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái loại kênh.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
