using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniCSKH.Data;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Cong viec (Work).
/// Port 1:1 tu 2023.8A.SkyCS: <c>Views/Work/AddWork.cshtml</c>, controller nguon
/// <c>Controllers/WorkController.cs:359</c> (GET) va <c>:410</c> (POST),
/// biz nguon <c>Services/WorkManager.cs:63 AddWithDetail</c> + <c>:237 GetOverlappedWorks</c>.
/// </summary>
public class WorkController(ISkyCsService svc, AppDbContext db) : Controller
{
    /// <summary>Xem ghi chu cung ten o ConversationController — nguon dung CurrentUser.Id.</summary>
    private async Task<int> NguoiDangNhapAsync()
    {
        if (Request.Cookies.TryGetValue("agent_id", out var s) && int.TryParse(s, out var id)
            && await db.Agents.AnyAsync(a => a.Id == id)) return id;
        return await db.Agents.Where(a => a.IsActive).Select(a => a.Id).FirstOrDefaultAsync();
    }

    public async Task<IActionResult> Index(WorkStatuses? status, int? userId, string? q)
    {
        ViewBag.Agents = await db.Agents.Where(a => a.IsActive).OrderBy(a => a.Name).ToListAsync();
        ViewBag.Status = status; ViewBag.UserId = userId; ViewBag.Q = q;
        return View(await svc.WorkListAsync(status, userId, q));
    }

    /// <summary>Port 1:1 WorkController.cs:359 AddWork() [HttpGet].</summary>
    public async Task<IActionResult> Create()
    {
        ViewBag.Lists = await svc.WorkAddListsAsync();
        return View(svc.WorkNew());
    }

    /// <summary>
    /// Port 1:1 WorkController.cs:410 AddWork(Work model, bool confirm = false) [HttpPost].
    /// <c>inchargeId</c> + <c>memberIds</c> tai dung khoi "Members" cua AddWork.cshtml:83-89:
    /// nguoi DAU TIEN duoc them la Incharge (AddWork.cshtml:374-378 — <c>hasInchaged ? Normal : Incharge</c>),
    /// nhung nguoi sau la Normal. Chi co MOT Incharge (AddWork.cshtml:321-326 ha vai cu xuong Normal).
    /// <c>confirm=true</c> la lan post lai sau khi nguoi dung dong y du TRUNG GIO.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Work model, int? inchargeId, int[]? memberIds, bool confirm = false)
    {
        var followers = new List<WorkFollow>();
        if (inchargeId is > 0) followers.Add(new WorkFollow { UserId = inchargeId.Value, Role = WorkUserRoles.Incharge });
        foreach (var uid in (memberIds ?? []).Distinct().Where(u => u > 0 && u != inchargeId))
            followers.Add(new WorkFollow { UserId = uid, Role = WorkUserRoles.Normal });

        var (ok, msg, id, overlapped) = await svc.WorkAddAsync(model, followers, await NguoiDangNhapAsync(), confirm);
        if (!ok)
        {
            TempData["Error"] = msg;
            ViewBag.Lists = await svc.WorkAddListsAsync();
            ViewBag.Overlapped = overlapped;                 // nguon tra ve partial WorkOverlapped (WorkController.cs:428)
            ViewBag.InchargeId = inchargeId; ViewBag.MemberIds = memberIds;
            return View(model);
        }
        TempData["Success"] = msg;
        return RedirectToAction(nameof(Index));
    }
}
