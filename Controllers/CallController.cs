using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MiniCSKH.Controllers;

public class CallController(ITicketService svc, IConfiguration cfg) : Controller
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

    /// <summary>Phòng gọi WebRTC trình duyệt↔trình duyệt (agent↔agent hoặc agent↔khách qua link chia sẻ) — mượt nhất vì kết nối P2P trực tiếp, không qua trung gian điện thoại, KHÔNG gọi được số điện thoại thật (khác VoipToken/StringeeX ở trên).</summary>
    [Route("/Call/WebRtc/{room?}")]
    public IActionResult WebRtc(string? room)
    {
        ViewBag.Room = string.IsNullOrWhiteSpace(room) ? Guid.NewGuid().ToString("N")[..8] : room;
        return View();
    }

    /// <summary>Sinh access-token StringeeX cho Web SDK gọi VoIP thật trong trình duyệt.
    /// Cần ENV STRINGEE_SID + STRINGEE_KEY + STRINGEE_SECRET (đăng ký tại stringee.com) — chưa cấu hình sẽ báo lỗi rõ ràng, không giả lập.</summary>
    [HttpGet("/api/v1/calls/voip-token")]
    public IActionResult VoipToken(string? userId)
    {
        var sid = cfg["STRINGEE_SID"]; var key = cfg["STRINGEE_KEY"]; var secret = cfg["STRINGEE_SECRET"];
        if (string.IsNullOrWhiteSpace(sid) || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(secret))
            return StatusCode(503, new { error = "Chưa cấu hình StringeeX (thiếu ENV STRINGEE_SID/STRINGEE_KEY/STRINGEE_SECRET). Đăng ký tài khoản tại stringee.com rồi set ENV để bật gọi VoIP thật." });

        var uid = string.IsNullOrWhiteSpace(userId) ? "agent-" + (User.Identity?.Name ?? "demo") : userId;
        var now = DateTimeOffset.UtcNow;
        var header = new { typ = "JWT", alg = "HS256", cty = "stringee-api;v=1" };
        var payload = new { jti = $"{key}-{now.ToUnixTimeSeconds()}", iss = key, exp = now.AddHours(1).ToUnixTimeSeconds(), userId = uid };
        string B64Url(byte[] b) => Convert.ToBase64String(b).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var h = B64Url(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(header)));
        var p = B64Url(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload)));
        var toSign = $"{h}.{p}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var sig = B64Url(hmac.ComputeHash(Encoding.UTF8.GetBytes(toSign)));
        return Ok(new { token = $"{toSign}.{sig}", projectId = sid, userId = uid, expiresAt = payload.exp });
    }
}
