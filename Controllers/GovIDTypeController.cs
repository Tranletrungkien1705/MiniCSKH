using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Loại giấy tờ định danh (port từ SkyCS: Mst_GovIDType —
/// 11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_GovIDType_Get`;
/// model 12.Dev.Common/idn.SkyCS.Common/Models/Mst_GovIDType.cs;
/// controller 13.ClientGate/V20/idn.SkyCS.WebAPI/Controllers/MstGovIDTypeController.cs).
/// Master data quy định các loại giấy tờ tùy thân/định danh dùng khi ghi nhận
/// thông tin khách hàng (CMTND/Thẻ căn cước, Hộ chiếu, Bằng lái xe, Giấy tờ khác…).
/// Mỗi loại có mã (GovIDType), tên hiển thị (GovIDTypeName), ghi chú và trạng thái
/// hoạt động (FlagActive). Hồ sơ khách hàng/người nộp thuế tham chiếu qua PresentIDType.
/// </summary>
public class GovIDTypeController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, string? q)
    {
        ViewBag.Active = active; ViewBag.Q = q;
        ViewBag.Stats = await svc.GovIDTypeStatsAsync();
        return View(await svc.GovIDTypesAsync(active, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var t = await svc.GovIDTypeGetAsync(id);
        if (t == null) return NotFound();
        return View(t);
    }

    public IActionResult Create() =>
        View("Edit", new GovIDType { IsActive = true });

    public async Task<IActionResult> Edit(int id)
    {
        var t = await svc.GovIDTypeGetAsync(id);
        if (t == null) return NotFound();
        return View(t);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(GovIDType model)
    {
        // Theo Mst_GovIDType: tên loại giấy tờ bắt buộc.
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["Error"] = "Cần tên loại giấy tờ định danh.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        var id = await svc.GovIDTypeSaveAsync(model);
        TempData["Success"] = "Đã lưu loại giấy tờ định danh.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.GovIDTypeToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái loại giấy tờ định danh.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
