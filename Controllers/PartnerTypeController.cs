using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Đối tượng khách hàng (port từ SkyCS: Mst_PartnerType —
/// 11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_PartnerType_Get`; model
/// 12.Dev.Common/idn.SkyCS.Common/Models/Mst_PartnerType.cs; controller
/// 13.ClientGate/V20/idn.SkyCS.WebAPI/Controllers/MstPartnerTypeController.cs;
/// cột xác nhận qua TblMst_PartnerType trong Const.Main.cs).
/// Đây là master data quy định VAI TRÒ của một đối tác trong hệ thống —
/// Khách hàng / Nhà cung cấp / Cả hai. Hồ sơ khách hàng (Mst_Customer.PartnerType)
/// tham chiếu mã này để phân biệt khách mua và nhà cung cấp.
/// </summary>
public class PartnerTypeController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, string? q)
    {
        ViewBag.Active = active; ViewBag.Q = q;
        ViewBag.Stats = await svc.PartnerTypeStatsAsync();
        return View(await svc.PartnerTypesAsync(active, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var m = await svc.PartnerTypeGetAsync(id);
        if (m == null) return NotFound();
        return View(m);
    }

    public IActionResult Create() => View("Edit", new PartnerTypeCatalog { IsActive = true });

    public async Task<IActionResult> Edit(int id)
    {
        var m = await svc.PartnerTypeGetAsync(id);
        if (m == null) return NotFound();
        return View(m);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(PartnerTypeCatalog model)
    {
        // Theo Mst_PartnerType: tên đối tượng bắt buộc; để trống mã sẽ tự sinh.
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["Error"] = "Cần nhập tên đối tượng khách hàng (PartnerTypeName).";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        var id = await svc.PartnerTypeSaveAsync(model);
        TempData["Success"] = "Đã lưu đối tượng khách hàng.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.PartnerTypeToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái đối tượng khách hàng.";
        return RedirectToAction(nameof(Details), new { id });
    }
}