using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Điều kiện áp dụng SLA (port từ SkyCS: Mst_SLATicketType / Mst_SLATicketCustomType /
/// Mst_SLACustomerCN / Mst_SLACustomerGroupCN / Mst_SLACustomerDN / Mst_SLACustomerGroupDN
/// + các cờ FlagAll* trên Mst_SLA — 11.BackEnd/V10/idn.SkyCS.Biz/Master.1.cs,
/// `Mst_SLA_Get`/`Mst_SLA_SaveX`).
/// Mỗi chính sách SLA được gán cho một tập đối tượng áp dụng: loại phiếu, loại phiếu
/// tùy chỉnh, khách hàng cá nhân/doanh nghiệp và nhóm khách tương ứng. Các cờ
/// "áp dụng cho tất cả" cho phép bỏ qua việc liệt kê chi tiết. Khi tạo eTicket,
/// hệ thống dò các điều kiện này để chọn đúng chính sách SLA.
/// </summary>
public class SlaScopeController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        ViewBag.Stats = await svc.SlaScopeStatsAsync();
        return View(await svc.SlaScopesAsync(q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var p = await svc.SlaScopeGetAsync(id);
        if (p == null) return NotFound();
        return View(p);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var p = await svc.SlaScopeGetAsync(id);
        if (p == null) return NotFound();
        return View(p);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(SlaPolicy model, List<string>? scopeKind, List<string>? scopeCode)
    {
        // Gộp 2 mảng song song (kind + code) thành danh sách đối tượng áp dụng.
        var scopes = new List<SlaScope>();
        var kinds = scopeKind ?? [];
        var codes = scopeCode ?? [];
        for (var i = 0; i < kinds.Count && i < codes.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(codes[i])) continue;
            if (!Enum.TryParse<SlaScopeKind>(kinds[i], out var kind)) continue;
            scopes.Add(new SlaScope { Kind = kind, RefCode = codes[i].Trim() });
        }

        var id = await svc.SlaScopeSaveAsync(model, scopes);
        TempData["Success"] = "Đã lưu điều kiện áp dụng SLA.";
        return RedirectToAction(nameof(Details), new { id });
    }
}