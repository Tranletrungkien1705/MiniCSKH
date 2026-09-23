using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Thiết lập phân bổ phiếu tự động (Mst_EstablishAllocateETicket — port từ SkyCS).
/// Cấu hình cách hệ thống tự động phân bổ eTicket mới về phòng ban và gán cho agent:
/// chia đều (FlagAllocateEven), tự gán agent (FlagAssignAgent), gán cả cuộc gọi nhỡ
/// (FlagAllMissedCall) + danh sách agent nhận phiếu (Mst_EstablishAllocateETAssignAgent).
/// </summary>
public class AllocateController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, string? q)
    {
        ViewBag.Active = active; ViewBag.Q = q;
        ViewBag.Stats = await svc.AllocateStatsAsync();
        return View(await svc.AllocateRulesAsync(active, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var r = await svc.AllocateRuleGetAsync(id);
        if (r == null) return NotFound();
        return View(r);
    }

    public IActionResult Create() => View("Edit", new AllocateRule { IsActive = true, AllocateEven = true, AssignAgent = true });

    public async Task<IActionResult> Edit(int id)
    {
        var r = await svc.AllocateRuleGetAsync(id);
        if (r == null) return NotFound();
        return View(r);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(AllocateRule model, string[]? agentCode, string[]? agentRemark)
    {
        if (string.IsNullOrWhiteSpace(model.DepartmentCode))
        {
            TempData["Error"] = "Cần mã phòng ban nhận phiếu.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        // Dựng danh sách agent nhận phiếu từ các mảng form (bỏ dòng trống).
        var agents = new List<AllocateAgent>();
        var codes = agentCode ?? [];
        for (int i = 0; i < codes.Length; i++)
        {
            var code = codes[i]?.Trim();
            if (string.IsNullOrWhiteSpace(code)) continue;
            agents.Add(new AllocateAgent
            {
                AgentCode = code,
                Remark = agentRemark?.ElementAtOrDefault(i)?.Trim()
            });
        }

        var id = await svc.AllocateRuleSaveAsync(model, agents);
        TempData["Success"] = "Đã lưu thiết lập phân bổ phiếu.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.AllocateRuleToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái thiết lập phân bổ.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
