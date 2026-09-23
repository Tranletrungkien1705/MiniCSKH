using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

public class TicketController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(TicketStatus? status, int? agentId, TicketPriority? priority, string? q)
    {
        ViewBag.Agents = await svc.AgentsAsync();
        ViewBag.Status = status; ViewBag.AgentId = agentId; ViewBag.Priority = priority; ViewBag.Q = q;
        return View(await svc.ListAsync(status, agentId, priority, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var t = await svc.GetAsync(id);
        if (t == null) return NotFound();
        ViewBag.Agents = await svc.AgentsAsync();
        return View(t);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await svc.CategoriesAsync();
        ViewBag.Agents = await svc.AgentsAsync();
        return View(new Ticket());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Ticket model)
    {
        if (string.IsNullOrWhiteSpace(model.Subject) || string.IsNullOrWhiteSpace(model.CustomerName))
        {
            TempData["Error"] = "Cần tiêu đề và tên khách hàng.";
            ViewBag.Categories = await svc.CategoriesAsync(); ViewBag.Agents = await svc.AgentsAsync();
            return View(model);
        }
        var id = await svc.CreateAsync(model);
        TempData["Success"] = "Đã tạo phiếu hỗ trợ.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int id, int? agentId)
    { await svc.AssignAsync(id, agentId); TempData["Success"] = "Đã gán agent."; return RedirectToAction(nameof(Details), new { id }); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Status(int id, TicketStatus status)
    { await svc.ChangeStatusAsync(id, status); TempData["Success"] = "Đã đổi trạng thái."; return RedirectToAction(nameof(Details), new { id }); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Priority(int id, TicketPriority priority)
    { await svc.ChangePriorityAsync(id, priority); TempData["Success"] = "Đã đổi độ ưu tiên."; return RedirectToAction(nameof(Details), new { id }); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Comment(int id, string author, string body, bool isInternal = false)
    {
        if (!string.IsNullOrWhiteSpace(body))
            await svc.AddCommentAsync(id, string.IsNullOrWhiteSpace(author) ? "Agent" : author, body, isInternal);
        return RedirectToAction(nameof(Details), new { id });
    }
}
