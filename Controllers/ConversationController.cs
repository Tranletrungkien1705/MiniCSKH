using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniCSKH.Data;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Hoi thoai / eTicket.
/// Port 1:1 tu 2023.8A.SkyCS: <c>Views/Conversation/ConvEdit.cshtml</c> (them/sua),
/// controller nguon <c>Controllers/ConversationController.cs:1072</c> (GET) va <c>:1134</c> (POST),
/// biz nguon <c>Services/ConversationManager.cs:650 UpdateDetail</c> / <c>:909 Add</c>.
/// Man danh sach dung lai cot cua <c>ConvList_Table.cshtml</c> / <c>ConvItem_Tr.cshtml</c>.
/// </summary>
public class ConversationController(ISkyCsService svc, AppDbContext db) : Controller
{
    /// <summary>
    /// "Nguoi dang dang nhap" o nguon la <c>CurrentUser.Id</c> (SysUser qua iNOS).
    /// Dich chua co bang nguoi dung: lay Agent theo cookie <c>agent_id</c>, khong co thi lay
    /// Agent dau tien. DON GIAN HOA CO CHU DICH — khai bao ro, khong im lang.
    /// </summary>
    private async Task<int> NguoiDangNhapAsync()
    {
        if (Request.Cookies.TryGetValue("agent_id", out var s) && int.TryParse(s, out var id)
            && await db.Agents.AnyAsync(a => a.Id == id)) return id;
        return await db.Agents.Where(a => a.IsActive).Select(a => a.Id).FirstOrDefaultAsync();
    }

    public async Task<IActionResult> Index(int? statusId, int? priorityId, int? picId, string? q)
    {
        var lists = await svc.ConvEditListsAsync();
        ViewBag.Lists = lists;
        ViewBag.StatusId = statusId; ViewBag.PriorityId = priorityId; ViewBag.PicId = picId; ViewBag.Q = q;
        return View(await svc.ConvListAsync(statusId, priorityId, picId, q));
    }

    /// <summary>Port 1:1 ConversationController.cs:1072 ConvEdit(long id = 0) [HttpGet].</summary>
    public async Task<IActionResult> Edit(int id = 0)
    {
        var conv = id > 0 ? await svc.ConvGetAsync(id) : await svc.ConvNewAsync(await NguoiDangNhapAsync());
        if (conv == null) return NotFound();
        ViewBag.Lists = await svc.ConvEditListsAsync();
        // nguon ConversationController.cs:1104/1110 — tieu de man doi theo them/sua
        ViewBag.PageTitle = id > 0 ? "Sửa hội thoại" : "Thêm hội thoại";
        return View(conv);
    }

    /// <summary>
    /// Port 1:1 ConversationController.cs:1134 ConvEdit(Conversation model) [HttpPost].
    /// Nhom truong khach hang o nguon post duoi ten <c>Customer.Name</c>, <c>Customer.Code</c>…
    /// (ConvEdit.cshtml:58/80/86/95/115/122) — giu DUNG ten do de binding thang vao
    /// <c>Conversation.Customer</c>.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Conversation model)
    {
        var (ok, msg, id) = await svc.ConvSaveAsync(model, await NguoiDangNhapAsync());
        if (!ok)
        {
            TempData["Error"] = msg;
            ViewBag.Lists = await svc.ConvEditListsAsync();
            ViewBag.PageTitle = model.Id > 0 ? "Sửa hội thoại" : "Thêm hội thoại";
            return View(model);
        }
        TempData["Success"] = msg;
        return RedirectToAction(nameof(Edit), new { id });
    }
}
