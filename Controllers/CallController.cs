using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

public class CallController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(CallDirection? dir, CallOutcome? outcome, string? q)
    {
        ViewBag.Dir = dir; ViewBag.Outcome = outcome; ViewBag.Q = q;
        ViewBag.Stats = await svc.CallStatsAsync();
        ViewBag.Agents = await svc.AgentsAsync();
        return View(await svc.CallsAsync(dir, outcome, q));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Log(CallDirection direction, string phoneNumber, string? customerName,
        int? agentId, int durationMinutes, int durationSecs, CallOutcome outcome, string? note)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) { TempData["Error"] = "Cần số điện thoại."; return RedirectToAction(nameof(Index)); }
        await svc.LogCallAsync(new CallLog
        {
            Direction = direction, PhoneNumber = phoneNumber.Trim(), CustomerName = customerName,
            AgentId = agentId, DurationSeconds = Math.Max(0, durationMinutes) * 60 + Math.Max(0, durationSecs),
            Outcome = outcome, Note = note
        });
        TempData["Success"] = "Đã ghi nhật ký cuộc gọi.";
        return RedirectToAction(nameof(Index));
    }
}
