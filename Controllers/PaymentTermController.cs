using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Điều khoản thanh toán (port từ SkyCS: Mst_PaymentTerm —
/// 11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_PaymentTerm_Get`/`_SaveX`;
/// model 12.Dev.Common/idn.SkyCS.Common/Models/Mst_PaymentTerm.cs).
/// Quy định cách khách hàng thanh toán: loại (bán/mua — TConst.PTType SALE/PURCHASE),
/// số ngày được nợ (OwedDay), hạn mức công nợ tối đa (CreditLimit) và % đặt cọc
/// (DepositPercent). Là master data nền cho hồ sơ khách hàng / đơn hàng.
/// </summary>
public class PaymentTermController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, PTType? type, string? q)
    {
        ViewBag.Active = active; ViewBag.Type = type; ViewBag.Q = q;
        ViewBag.Stats = await svc.PaymentTermStatsAsync();
        return View(await svc.PaymentTermsAsync(active, type, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var p = await svc.PaymentTermGetAsync(id);
        if (p == null) return NotFound();
        return View(p);
    }

    public IActionResult Create() =>
        View("Edit", new PaymentTerm { IsActive = true, Type = PTType.Sale });

    public async Task<IActionResult> Edit(int id)
    {
        var p = await svc.PaymentTermGetAsync(id);
        if (p == null) return NotFound();
        return View(p);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(PaymentTerm model)
    {
        // Kiểm tra theo SkyCS Mst_PaymentTerm_SaveX: mã/tên/mô tả bắt buộc,
        // OwedDay/DepositPercent/CreditLimit không âm.
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["Error"] = "Cần tên điều khoản thanh toán.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }
        if (string.IsNullOrWhiteSpace(model.Description))
        {
            TempData["Error"] = "Cần mô tả điều khoản thanh toán (PTDesc).";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }
        if (model.OwedDay < 0 || model.CreditLimit < 0 || model.DepositPercent < 0)
        {
            TempData["Error"] = "Số ngày nợ, hạn mức công nợ và % đặt cọc không được âm.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        var id = await svc.PaymentTermSaveAsync(model);
        TempData["Success"] = "Đã lưu điều khoản thanh toán.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.PaymentTermToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái điều khoản thanh toán.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
