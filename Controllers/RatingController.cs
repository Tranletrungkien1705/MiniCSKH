using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Đánh giá phiếu (eTicket Rating — port từ SkyCS ET_TicketRated).
/// Khách đánh giá phiếu đã xử lý (RATE); agent kiểm soát lại đánh giá (REVIEW).
/// </summary>
public class RatingController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(RateType? type, RateResult? result, string? q)
    {
        ViewBag.Stats = await svc.RatingStatsAsync();
        ViewBag.Type = type; ViewBag.Result = result; ViewBag.Q = q;
        return View(await svc.RatingsAsync(type, result, q));
    }

    // Khách đánh giá phiếu (chỉ khi phiếu chưa có cờ FlagRated).
    [HttpGet]
    public async Task<IActionResult> Rate(int ticketId)
    {
        var t = await svc.GetAsync(ticketId);
        if (t == null) return NotFound();
        if (t.FlagRated)
        {
            TempData["Error"] = "Phiếu này đã được đánh giá.";
            return RedirectToAction("Details", "Ticket", new { id = ticketId });
        }
        ViewBag.Ticket = t;
        return View(new TicketRating { TicketId = ticketId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Rate(int ticketId, int score, RateResult result, string? comment, string? ratedBy)
    {
        try
        {
            await svc.RateTicketAsync(ticketId, score, result, comment, ratedBy ?? "");
            TempData["Success"] = "Cảm ơn bạn đã đánh giá phiếu hỗ trợ.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction("Details", "Ticket", new { id = ticketId });
    }

    // Agent kiểm soát lại đánh giá (REVIEW).
    [HttpGet]
    public async Task<IActionResult> Review(int id)
    {
        var r = await svc.RatingGetAsync(id);
        if (r == null) return NotFound();
        return View(r);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(int id, string? reviewedBy, string? reviewNote)
    {
        await svc.ReviewRatingAsync(id, reviewedBy ?? "", reviewNote);
        TempData["Success"] = "Đã ghi nhận kiểm soát đánh giá.";
        return RedirectToAction(nameof(Index));
    }
}
