using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Lịch làm việc SLA (port từ SkyCS: Mst_SLAWorkingDay + Mst_SLAHoliday —
/// 11.BackEnd/V10/idn.SkyCS.Biz/Master.1.cs, `Mst_SLA_Get`/`Mst_SLA_SaveX`/
/// `Mst_SLA_CalcDeadlineX`/`Mst_SLA_CalcFirstResDTimeX`; models
/// 12.Dev.Common/idn.SkyCS.Common/Models/Mst_SLAWorkingDay.cs, Mst_SLAHoliday.cs).
/// Mỗi chính sách SLA gắn một lịch làm việc (giờ theo từng thứ, 2 ca/ngày) và
/// danh sách ngày nghỉ; dùng để tính hạn xử lý theo giờ hành chính thay vì 24/7.
/// </summary>
public class SlaCalendarController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        ViewBag.Stats = await svc.SlaCalendarStatsAsync();
        return View(await svc.SlaCalendarsAsync(q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var p = await svc.SlaCalendarGetAsync(id);
        if (p == null) return NotFound();
        ViewBag.WorkingDays = await svc.SlaWorkingDaysAsync(id);
        ViewBag.Holidays = await svc.SlaHolidaysAsync(id);
        return View(p);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var p = await svc.SlaCalendarGetAsync(id);
        if (p == null) return NotFound();
        ViewBag.WorkingDays = await svc.SlaWorkingDaysAsync(id);
        ViewBag.Holidays = await svc.SlaHolidaysAsync(id);
        return View(p);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, List<SlaWorkingDay> workingDays, List<SlaHoliday> holidays)
    {
        var p = await svc.SlaCalendarGetAsync(id);
        if (p == null) return NotFound();

        // Lọc dòng hợp lệ: ca có From < To; ngày nghỉ có giá trị dd-MM.
        workingDays = (workingDays ?? []).Where(w => w.ToMinutes > w.FromMinutes).ToList();
        holidays = (holidays ?? []).Where(h => !string.IsNullOrWhiteSpace(h.Holiday)).ToList();

        await svc.SlaCalendarSaveAsync(p, workingDays, holidays);
        TempData["Success"] = "Đã lưu lịch làm việc SLA.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>Tính thử hạn xử lý theo lịch làm việc (Mst_SLA_CalcDeadlineX).</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Calc(int id, DateTime reception, bool firstResponse)
    {
        var p = await svc.SlaCalendarGetAsync(id);
        if (p == null) return NotFound();
        var deadline = await svc.SlaCalcDeadlineAsync(id, reception, firstResponse);
        TempData["Success"] = deadline is { } d
            ? $"Hạn {(firstResponse ? "phản hồi đầu tiên" : "xử lý")}: {d:dd/MM/yyyy HH:mm} (nhận {reception:dd/MM/yyyy HH:mm})."
            : "Không tính được hạn (thiếu lịch làm việc).";
        return RedirectToAction(nameof(Details), new { id });
    }
}