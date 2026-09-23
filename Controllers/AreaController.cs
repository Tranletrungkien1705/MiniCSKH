using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Vùng thị trường (port từ SkyCS: Mst_Area —
/// 11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_Area_Get`/`_SaveX`;
/// model 12.Dev.Common/idn.SkyCS.Common/Models/CustomerCentrer/Mst_Area.cs).
/// Master data của Trung tâm khách hàng: phân vùng khách hàng theo khu vực,
/// có phân cấp (AreaCodeParent) và cấp vùng (AreaLevel). Dùng để phân vùng
/// phục vụ và báo cáo theo khu vực.
/// </summary>
public class AreaController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, string? q)
    {
        ViewBag.Active = active; ViewBag.Q = q;
        ViewBag.Stats = await svc.AreaStatsAsync();
        return View(await svc.AreasAsync(active, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var a = await svc.AreaGetAsync(id);
        if (a == null) return NotFound();
        return View(a);
    }

    public IActionResult Create() =>
        View("Edit", new Area { IsActive = true, Level = 1 });

    public async Task<IActionResult> Edit(int id)
    {
        var a = await svc.AreaGetAsync(id);
        if (a == null) return NotFound();
        return View(a);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Area model)
    {
        // Kiểm tra theo SkyCS Mst_Area_SaveX: mã vùng (AreaCode) bắt buộc.
        if (string.IsNullOrWhiteSpace(model.Code))
        {
            TempData["Error"] = "Cần mã vùng thị trường (AreaCode).";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["Error"] = "Cần tên vùng thị trường (AreaName).";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }
        if (model.Level < 1)
        {
            TempData["Error"] = "Cấp vùng (AreaLevel) phải từ 1 trở lên.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        var id = await svc.AreaSaveAsync(model);
        TempData["Success"] = "Đã lưu vùng thị trường.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.AreaToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái vùng thị trường.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
