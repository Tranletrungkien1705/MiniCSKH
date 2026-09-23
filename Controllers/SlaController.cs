using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>Quản lý chính sách SLA (mức cam kết phản hồi đầu tiên + thời gian xử lý).</summary>
public class SlaController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Stats = await svc.SlaStatsAsync();
        return View(await svc.SlaListAsync());
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is { } i)
        {
            var p = await svc.SlaGetAsync(i);
            if (p == null) return NotFound();
            return View(p);
        }
        return View(new SlaPolicy());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SlaPolicy model)
    {
        if (string.IsNullOrWhiteSpace(model.Level))
        {
            TempData["Error"] = "Cần tên mức SLA.";
            return View(model);
        }
        if (model.FirstResMinutes < 1 || model.ResolutionMinutes < 1)
        {
            TempData["Error"] = "Thời gian phản hồi và xử lý phải lớn hơn 0.";
            return View(model);
        }
        await svc.SlaSaveAsync(model);
        TempData["Success"] = "Đã lưu chính sách SLA.";
        return RedirectToAction(nameof(Index));
    }
}
