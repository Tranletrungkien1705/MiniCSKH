using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Cam ket chat luong dich vu (SLA).
/// Port 1:1 tu 2023.8A.SkyCS: <c>Views/WorkSetting/SLAs.cshtml</c> (danh sach) +
/// <c>Views/WorkSetting/EditSLA.cshtml</c> (them/sua), controller nguon
/// <c>Controllers/WorkSettingController.cs:556/577/602</c>.
/// </summary>
public class SlaController(ISkyCsService svc) : Controller
{
    /// <summary>Port 1:1 WorkSettingController.cs:556 SLAs().</summary>
    public async Task<IActionResult> Index() => View(await svc.SlaListAsync());

    /// <summary>Port 1:1 WorkSettingController.cs:577 EditSLA(long id) — id=0 la them moi.</summary>
    public async Task<IActionResult> Edit(int id = 0)
    {
        var sla = id > 0 ? await svc.SlaGetAsync(id) : new Sla { IsActive = true };
        if (sla == null) return NotFound();
        ViewBag.BizHours = await svc.BizHoursAsync();
        ViewBag.PageTitle = id == 0 ? "Thêm SLA" : "Sửa SLA";   // nguon WorkSettingController.cs:588
        return View(sla);
    }

    /// <summary>
    /// Port 1:1 WorkSettingController.cs:602 EditSLA(SLA model) [HttpPost].
    /// Ba truong <c>*TimeDesc</c> nhan dang "HH:mm" dung nhu form nguon post len
    /// (EditSLA.cshtml:79/100/118), viec quy doi ra phut nam trong Models/SkyCs.cs (Sla).
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Sla model, string? firstResponseTimeDesc,
        string? everyResponseTimeDesc, string? resolutionTimeDesc)
    {
        model.FirstResponseTimeDesc = firstResponseTimeDesc ?? "00:00";
        model.EveryResponseTimeDesc = everyResponseTimeDesc ?? "00:00";
        model.ResolutionTimeDesc = resolutionTimeDesc ?? "00:00";
        var (ok, msg, id) = await svc.SlaSaveAsync(model);
        if (!ok)
        {
            TempData["Error"] = msg;
            ViewBag.BizHours = await svc.BizHoursAsync();
            ViewBag.PageTitle = model.Id == 0 ? "Thêm SLA" : "Sửa SLA";
            return View(model);
        }
        TempData["Success"] = msg;
        return RedirectToAction(nameof(Index), new { id });
    }

    /// <summary>Port 1:1 hanh dong <c>DeleteSLA</c> ma SLAs.cshtml:86 goi.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, msg) = await svc.SlaDeleteAsync(id);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }
}
