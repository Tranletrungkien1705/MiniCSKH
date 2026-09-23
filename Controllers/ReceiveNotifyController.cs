using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Người nhận thông báo phiếu (port từ SkyCS: Mst_EstablishReceiveNotifyETicket —
/// 11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_EstablishReceiveNotifyETicket_Get`/`_SaveX`;
/// model 12.Dev.Common/idn.SkyCS.Common/Models/Mst_EstablishReceiveNotifyETicket.cs;
/// controller 13.ClientGate/V20/idn.SkyCS.WebAPI/Controllers/MstEstablishReceiveNotifyETicketController.cs).
/// Danh sách agent (Sys_User.UserCode) sẽ nhận thông báo khi có eTicket mới.
/// </summary>
public class ReceiveNotifyController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        ViewBag.Stats = await svc.ReceiveNotifyStatsAsync();
        return View(await svc.ReceiveNotifiesAsync(q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var r = await svc.ReceiveNotifyGetAsync(id);
        if (r == null) return NotFound();
        return View(r);
    }

    public IActionResult Create() => View("Edit", new ReceiveNotify());

    public async Task<IActionResult> Edit(int id)
    {
        var r = await svc.ReceiveNotifyGetAsync(id);
        if (r == null) return NotFound();
        return View(r);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ReceiveNotify model)
    {
        // Kiểm tra theo SkyCS Mst_EstablishReceiveNotifyETicket_SaveX: AgentCode bắt buộc.
        if (string.IsNullOrWhiteSpace(model.AgentCode))
        {
            TempData["Error"] = "Cần mã agent (AgentCode).";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        var id = await svc.ReceiveNotifySaveAsync(model);
        TempData["Success"] = "Đã lưu người nhận thông báo phiếu.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await svc.ReceiveNotifyDeleteAsync(id);
        TempData["Success"] = "Đã xóa người nhận thông báo.";
        return RedirectToAction(nameof(Index));
    }
}