using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Quản lý thông báo (port từ SkyCS: Mst_NotifyType + Mst_ManageNotify +
/// Map_UserInNotifyType — 11.BackEnd/V10/idn.SkyCS.Biz/System.cs,
/// `Mst_ManageNotify_CreateX`/`_DeleteX`; Delete/Delete.Master.Cloud.cs,
/// `Mst_NotifyType_Get`/`Mst_ManageNotify_Get`/`Map_UserInNotifyType_Get`;
/// schema 11.BackEnd/V10/05.Refs.Biz/Migrate/20200806.z11.CreateTable.MapNotify.sql).
/// Hệ thống thông báo nội bộ gồm: danh mục LOẠI thông báo (NotifyType),
/// danh sách NGƯỜI QUẢN LÝ nhận thông báo (NotifyManager) và MA TRẬN phân quyền
/// thông báo (NotifySubscription: user × loại thông báo, bật/tắt).
/// </summary>
public class NotifyController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, string? q)
    {
        ViewBag.Active = active; ViewBag.Q = q;
        ViewBag.Stats = await svc.NotifyStatsAsync();
        return View(await svc.NotifyTypesAsync(active, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var t = await svc.NotifyTypeGetAsync(id);
        if (t == null) return NotFound();
        // Ma trận phân quyền của loại thông báo này (theo NotifyTypeCode).
        var subs = await svc.NotifySubscriptionsAsync(null);
        ViewBag.Subscriptions = subs.Where(s => s.NotifyTypeCode == t.Code).ToList();
        ViewBag.Managers = await svc.NotifyManagersAsync(null);
        return View(t);
    }

    public IActionResult Create() =>
        View("Edit", new NotifyType { DefaultActive = true });

    public async Task<IActionResult> Edit(int id)
    {
        var t = await svc.NotifyTypeGetAsync(id);
        if (t == null) return NotFound();
        return View(t);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(NotifyType model)
    {
        // Theo Mst_NotifyType: mã loại thông báo bắt buộc.
        if (string.IsNullOrWhiteSpace(model.Code))
        {
            TempData["Error"] = "Cần mã loại thông báo.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        var id = await svc.NotifyTypeSaveAsync(model);
        TempData["Success"] = "Đã lưu loại thông báo.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.NotifyTypeToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái mặc định của loại thông báo.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── Người quản lý nhận thông báo (Mst_ManageNotify) ──────────────
    public async Task<IActionResult> Managers(string? q)
    {
        ViewBag.Q = q;
        ViewBag.Stats = await svc.NotifyStatsAsync();
        return View(await svc.NotifyManagersAsync(q));
    }

    public IActionResult ManagerCreate() =>
        View("ManagerEdit", new NotifyManager());

    public async Task<IActionResult> ManagerEdit(int id)
    {
        var m = await svc.NotifyManagerGetAsync(id);
        if (m == null) return NotFound();
        return View(m);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ManagerSave(NotifyManager model)
    {
        // Theo Mst_ManageNotify: mã user bắt buộc.
        if (string.IsNullOrWhiteSpace(model.UserCode))
        {
            TempData["Error"] = "Cần mã user (UserCode).";
            return RedirectToAction(model.Id == 0 ? nameof(ManagerCreate) : nameof(ManagerEdit), new { id = model.Id });
        }

        await svc.NotifyManagerSaveAsync(model);
        TempData["Success"] = "Đã lưu người quản lý nhận thông báo.";
        return RedirectToAction(nameof(Managers));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ManagerDelete(int id)
    {
        await svc.NotifyManagerDeleteAsync(id);
        TempData["Success"] = "Đã xóa người quản lý nhận thông báo.";
        return RedirectToAction(nameof(Managers));
    }

    // ── Ma trận phân quyền thông báo (Map_UserInNotifyType) ──────────
    public async Task<IActionResult> Matrix(string? userCode)
    {
        var managers = await svc.NotifyManagersAsync(null);
        var types = await svc.NotifyTypesAsync(null, null);
        var subs = await svc.NotifySubscriptionsAsync(userCode);

        ViewBag.Managers = managers;
        ViewBag.Types = types;
        ViewBag.UserCode = userCode;
        ViewBag.Stats = await svc.NotifyStatsAsync();
        return View(subs);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MatrixSave(string userCode, int[]? notifyTypeIds)
    {
        if (string.IsNullOrWhiteSpace(userCode))
        {
            TempData["Error"] = "Cần chọn người quản lý.";
            return RedirectToAction(nameof(Matrix));
        }

        var types = await svc.NotifyTypesAsync(null, null);
        var selected = (notifyTypeIds ?? []).ToHashSet();
        // Mỗi loại thông báo là 1 dòng; cờ FlagNotify = có được chọn hay không.
        var subs = types.Select(t => new NotifySubscription
        {
            NotifyTypeCode = t.Code,
            FlagNotify = selected.Contains(t.Id)
        }).ToList();

        await svc.NotifySubscriptionSaveAsync(userCode, subs);
        TempData["Success"] = "Đã lưu ma trận phân quyền thông báo.";
        return RedirectToAction(nameof(Matrix), new { userCode });
    }
}
