using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Mức đánh giá hài lòng (port từ SkyCS: Mst_SatisfactionRating —
/// 11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_SatisfactionRating_Get`/
/// `Mst_SatisfactionRating_CheckDB`; model 12.Dev.Common/idn.SkyCS.Common/
/// Models/Mst_SatisfactionRating.cs; controller 13.ClientGate/V20/idn.SkyCS.WebAPI/
/// Controllers/MstSatisfactionRatingController.cs).
/// Master data quy định các MỨC độ hài lòng chuẩn (Rất hài lòng / Hài lòng /
/// Bình thường / Không hài lòng / Rất không hài lòng…) để agent chọn khi ghi
/// nhận kết quả chăm sóc. eTicket gắn mức này qua ET_Ticket.SatRatingCode
/// ("Mã Đánh giá mức độ hài lòng"); chiến dịch khảo sát cũng tham chiếu.
/// Mỗi mức có mã (SatRatingCode), tên (SatRatingName), thứ tự hiển thị (OrdIdx),
/// ghi chú và trạng thái hoạt động (FlagActive).
/// </summary>
public class SatisfactionRatingController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, string? q)
    {
        ViewBag.Active = active; ViewBag.Q = q;
        ViewBag.Stats = await svc.SatisfactionRatingStatsAsync();
        return View(await svc.SatisfactionRatingsAsync(active, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var s = await svc.SatisfactionRatingGetAsync(id);
        if (s == null) return NotFound();
        return View(s);
    }

    public IActionResult Create() =>
        View("Edit", new SatisfactionRating { IsActive = true });

    public async Task<IActionResult> Edit(int id)
    {
        var s = await svc.SatisfactionRatingGetAsync(id);
        if (s == null) return NotFound();
        return View(s);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(SatisfactionRating model)
    {
        // Theo Mst_SatisfactionRating: tên mức đánh giá bắt buộc.
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["Error"] = "Cần tên mức đánh giá.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        var id = await svc.SatisfactionRatingSaveAsync(model);
        TempData["Success"] = "Đã lưu mức đánh giá hài lòng.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.SatisfactionRatingToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái mức đánh giá.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
