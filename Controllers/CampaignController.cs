using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

public class CampaignController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(CampaignStatus? status, string? q)
    {
        ViewBag.Status = status; ViewBag.Q = q;
        ViewBag.Stats = await svc.CampaignStatsAsync();
        return View(await svc.CampaignsAsync(status, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var c = await svc.CampaignGetAsync(id);
        if (c == null) return NotFound();
        ViewBag.Agents = await svc.AgentsAsync();
        return View(c);
    }

    public IActionResult Create() => View(new Campaign());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Campaign model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["Error"] = "Cần tên chiến dịch.";
            return View(model);
        }
        var id = await svc.CampaignCreateAsync(model);
        TempData["Success"] = "Đã tạo chiến dịch.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Status(int id, CampaignStatus status)
    {
        await svc.CampaignChangeStatusAsync(id, status);
        TempData["Success"] = "Đã đổi trạng thái chiến dịch.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCustomer(int customerId, int? agentId, CampaignCustomerStatus status, string? feedback, string? remark)
    {
        var cc = await svc.CampaignCustomerGetAsync(customerId);
        if (cc == null) return NotFound();
        await svc.CampaignCustomerSaveAsync(customerId, agentId, status, feedback, remark);
        TempData["Success"] = "Đã cập nhật kết quả gọi.";
        return RedirectToAction(nameof(Details), new { id = cc.CampaignId });
    }
}
