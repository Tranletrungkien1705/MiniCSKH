using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Thẻ (port từ SkyCS: Mst_Tag —
/// 11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_Tag_Get`/`Mst_Tag_Create`;
/// model 12.Dev.Common/idn.SkyCS.Common/Models/Mst_Tag.cs;
/// controller 13.ClientGate/V20/idn.SkyCS.WebAPI/Controllers/MstTagController.cs).
/// Thẻ là nhãn dùng chung để gắn/phân loại nội dung CSKH (bài viết Knowledge Base,
/// phiếu hỗ trợ). Mỗi thẻ có mã (TagID), tên (TagName), mô tả (TagDesc) và slug (Slug).
/// </summary>
public class TagController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, string? q)
    {
        ViewBag.Active = active; ViewBag.Q = q;
        ViewBag.Stats = await svc.TagStatsAsync();
        return View(await svc.TagsAsync(active, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var t = await svc.TagGetAsync(id);
        if (t == null) return NotFound();
        return View(t);
    }

    public IActionResult Create() =>
        View("Edit", new Tag { IsActive = true });

    public async Task<IActionResult> Edit(int id)
    {
        var t = await svc.TagGetAsync(id);
        if (t == null) return NotFound();
        return View(t);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Tag model)
    {
        // Kiểm tra theo SkyCS Mst_Tag_Create: TagID và TagName bắt buộc.
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["Error"] = "Cần tên thẻ (TagName).";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        var id = await svc.TagSaveAsync(model);
        TempData["Success"] = "Đã lưu thẻ.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.TagToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái thẻ.";
        return RedirectToAction(nameof(Details), new { id });
    }
}