using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Danh mục bài viết Knowledge Base (port từ SkyCS: KB_Category —
/// 11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `KB_Category_Get`/`KB_Category_Create`/
/// `KB_Category_Update`/`KB_Category_Delete`/`KB_Category_SaveX`; model
/// 12.Dev.Common/idn.SkyCS.Common/Models/KB_Category.cs; controller
/// 13.ClientGate/V20/idn.SkyCS.WebAPI/Controllers/KBCategoryController.cs).
/// Danh mục là CÂY PHÂN CẤP (CategoryParentCode) dùng để phân loại bài viết tri thức
/// (KB_Post), có slug (dùng cho URL/tìm kiếm), trạng thái (FlagActive) và loại chia sẻ
/// (KB_CategoryShareType → Mst_CateShareType). Là master data nền cho Knowledge Base.
/// </summary>
public class KbCategoryController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, string? parentCode, string? q)
    {
        ViewBag.Active = active; ViewBag.ParentCode = parentCode; ViewBag.Q = q;
        ViewBag.Stats = await svc.KbCategoryStatsAsync();
        ViewBag.Parents = await svc.KbCategoriesAdminAsync(null, null, null);
        return View(await svc.KbCategoriesAdminAsync(active, parentCode, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var m = await svc.KbCategoryGetAsync(id);
        if (m == null) return NotFound();
        ViewBag.Parent = string.IsNullOrWhiteSpace(m.ParentCode)
            ? null
            : (await svc.KbCategoriesAdminAsync(null, null, null)).FirstOrDefault(c => c.Code == m.ParentCode);
        return View(m);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Parents = await svc.KbCategoriesAdminAsync(true, null, null);
        return View("Edit", new KbCategory { IsActive = true });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var m = await svc.KbCategoryGetAsync(id);
        if (m == null) return NotFound();
        ViewBag.Parents = await svc.KbCategoriesAdminAsync(null, null, null);
        return View(m);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(KbCategory model)
    {
        // Theo KB_Category_SaveX: CategoryName bắt buộc; để trống mã/slug sẽ tự sinh.
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["Error"] = "Cần nhập tên danh mục (CategoryName).";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }
        if (model.Name.Length > 200)
        {
            TempData["Error"] = "Tên danh mục tối đa 200 ký tự.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }
        // Danh mục cha (nếu có) phải tồn tại và đang dùng (theo KB_Category_CheckDB).
        if (!string.IsNullOrWhiteSpace(model.ParentCode))
        {
            var parents = await svc.KbCategoriesAdminAsync(true, null, null);
            if (!parents.Any(c => c.Code == model.ParentCode))
            {
                TempData["Error"] = "Danh mục cha không tồn tại hoặc đã ngừng dùng.";
                return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
            }
        }

        var id = await svc.KbCategorySaveAsync(model);
        TempData["Success"] = "Đã lưu danh mục bài viết.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.KbCategoryToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái danh mục.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await svc.KbCategoryDeleteAsync(id);
        TempData["Success"] = "Đã xóa danh mục bài viết.";
        return RedirectToAction(nameof(Index));
    }
}
