using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Trung tâm khách hàng (Mst_Customer — port từ SkyCS CustomerCenter/Customer.cs).
/// Hồ sơ khách hàng là trung tâm dữ liệu CSKH: gắn nhóm khách hàng, người liên hệ
/// (Mst_CustomerContact) và lịch sử thay đổi (Mst_CustomerHist).
/// </summary>
public class CustomerController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, CustomerType? type, string? group, string? q)
    {
        ViewBag.Active = active; ViewBag.Type = type; ViewBag.Group = group; ViewBag.Q = q;
        ViewBag.Stats = await svc.CustomerStatsAsync();
        ViewBag.Groups = await svc.CustomerGroupsAsync();
        return View(await svc.CustomersAsync(active, type, group, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var c = await svc.CustomerGetAsync(id);
        if (c == null) return NotFound();
        ViewBag.Groups = await svc.CustomerGroupsAsync();
        return View(c);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Groups = await svc.CustomerGroupsAsync();
        return View("Edit", new Customer { IsActive = true });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var c = await svc.CustomerGetAsync(id);
        if (c == null) return NotFound();
        ViewBag.Groups = await svc.CustomerGroupsAsync();
        return View(c);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Customer model, string[]? ctName, string[]? ctTitle, string[]? ctPhone, string[]? ctEmail)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["Error"] = "Cần tên khách hàng.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        // Dựng danh sách người liên hệ từ các mảng form (bỏ dòng trống).
        var contacts = new List<CustomerContact>();
        var names = ctName ?? [];
        for (int i = 0; i < names.Length; i++)
        {
            var name = names[i]?.Trim();
            if (string.IsNullOrWhiteSpace(name)) continue;
            contacts.Add(new CustomerContact
            {
                Name = name,
                Title = ctTitle?.ElementAtOrDefault(i)?.Trim(),
                Phone = ctPhone?.ElementAtOrDefault(i)?.Trim(),
                Email = ctEmail?.ElementAtOrDefault(i)?.Trim()
            });
        }

        var id = await svc.CustomerSaveAsync(model, contacts);
        TempData["Success"] = "Đã lưu hồ sơ khách hàng.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.CustomerToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái khách hàng.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
