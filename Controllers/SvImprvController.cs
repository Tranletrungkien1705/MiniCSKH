using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Cải tiến chất lượng dịch vụ (SvImp_SvImprv — port từ SkyCS).
/// Bộ tiêu chí đánh giá chất lượng cuộc gọi (lời chào/kính ngữ, từ cấm,
/// thời lượng gọi, phân tích audio); mỗi bộ gồm nhiều tiêu chí con.
/// </summary>
public class SvImprvController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, SvImprvItemType? type, string? q)
    {
        ViewBag.Active = active; ViewBag.Type = type; ViewBag.Q = q;
        ViewBag.Stats = await svc.SvImprvStatsAsync();
        return View(await svc.SvImprvsAsync(active, type, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var s = await svc.SvImprvGetAsync(id);
        if (s == null) return NotFound();
        return View(s);
    }

    public IActionResult Create() => View("Edit", new ServiceImprovement { IsActive = true });

    public async Task<IActionResult> Edit(int id)
    {
        var s = await svc.SvImprvGetAsync(id);
        if (s == null) return NotFound();
        return View(s);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ServiceImprovement model, string[]? critWord, string[]? critQty, string[]? critRequired, string[]? critMin, string[]? critMax, string[]? critAllow)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["Error"] = "Cần tên bộ cải tiến.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        // Dựng danh sách tiêu chí từ các mảng form (bỏ dòng trống).
        var criteria = new List<SvImprvCriterion>();
        var words = critWord ?? [];
        for (int i = 0; i < words.Length; i++)
        {
            var word = words[i]?.Trim();
            if (string.IsNullOrWhiteSpace(word)) continue;
            criteria.Add(new SvImprvCriterion
            {
                Kind = model.ItemType,
                Word = word,
                QtyStd = ParseInt(critQty?.ElementAtOrDefault(i)),
                IsRequired = (critRequired?.ElementAtOrDefault(i)) == "on" || (critRequired?.ElementAtOrDefault(i)) == "true",
                MinValue = ParseInt(critMin?.ElementAtOrDefault(i)),
                MaxValue = ParseInt(critMax?.ElementAtOrDefault(i)),
                QtyAllow = ParseInt(critAllow?.ElementAtOrDefault(i))
            });
        }

        var id = await svc.SvImprvSaveAsync(model, criteria);
        TempData["Success"] = "Đã lưu bộ cải tiến.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.SvImprvToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái bộ cải tiến.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private static int ParseInt(string? s) => int.TryParse(s, out var v) ? v : 0;
}
