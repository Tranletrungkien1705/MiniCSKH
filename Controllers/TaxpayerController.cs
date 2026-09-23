using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Người nộp thuế (port từ SkyCS: Mst_NNT —
/// 11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_NNT_Get`/`Mst_NNT_Update`/
/// `Mst_NNT_CreateForNetwork`; model 12.Dev.Common/idn.SkyCS.Common/Models/Mst_NNT.cs;
/// controller 13.ClientGate/V20/idn.SkyCS.WebAPI/Controllers/MstNNTController.cs).
/// Hồ sơ doanh nghiệp/tổ chức nộp thuế trong Trung tâm khách hàng — gắn mã số
/// thuế (MST), thông tin pháp lý (giấy phép KD, người đại diện, chữ ký số),
/// địa chỉ (tỉnh/huyện), ngân hàng và người liên hệ. Là master data nền cho
/// hồ sơ khách hàng doanh nghiệp và đăng ký dịch vụ TVAN (TCTStatus).
/// </summary>
public class TaxpayerController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, TctStatus? tctStatus, string? q)
    {
        ViewBag.Active = active; ViewBag.TctStatus = tctStatus; ViewBag.Q = q;
        ViewBag.Stats = await svc.TaxpayerStatsAsync();
        return View(await svc.TaxpayersAsync(active, tctStatus, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var t = await svc.TaxpayerGetAsync(id);
        if (t == null) return NotFound();
        return View(t);
    }

    public IActionResult Create() =>
        View("Edit", new Taxpayer { IsActive = true, Level = 1, TctStatus = TctStatus.None });

    public async Task<IActionResult> Edit(int id)
    {
        var t = await svc.TaxpayerGetAsync(id);
        if (t == null) return NotFound();
        return View(t);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Taxpayer model)
    {
        // Theo Mst_NNT_Update: MST, tên doanh nghiệp, địa chỉ, người liên hệ (tên/ĐT/email) bắt buộc.
        if (string.IsNullOrWhiteSpace(model.TaxCode))
        {
            TempData["Error"] = "Cần mã số thuế (MST).";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }
        if (string.IsNullOrWhiteSpace(model.FullName))
        {
            TempData["Error"] = "Cần tên doanh nghiệp.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }
        if (string.IsNullOrWhiteSpace(model.Address))
        {
            TempData["Error"] = "Cần địa chỉ người nộp thuế.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }
        if (string.IsNullOrWhiteSpace(model.ContactName) || string.IsNullOrWhiteSpace(model.ContactPhone)
            || string.IsNullOrWhiteSpace(model.ContactEmail))
        {
            TempData["Error"] = "Cần đủ tên, điện thoại và email người liên hệ.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        var id = await svc.TaxpayerSaveAsync(model);
        TempData["Success"] = "Đã lưu người nộp thuế.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.TaxpayerToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái người nộp thuế.";
        return RedirectToAction(nameof(Details), new { id });
    }
}