using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Phòng ban (port từ SkyCS: Mst_Department — 11.BackEnd/V10/idn.SkyCS.Biz/Master.cs,
/// `Mst_Department_Get`/`_Update`; model 12.Dev.Common/idn.SkyCS.Common/Models/Mst_Department.cs).
/// Phòng ban là đơn vị tổ chức nhận và xử lý eTicket, có phân cấp (DepartmentCodeParent)
/// và cờ phân chia tự động đều cho thành viên (FlagAutoDiv). Là master data nền cho
/// phân bổ phiếu (Mst_EstablishAllocateETicket.DepartmentCode) và gán agent theo phòng ban.
/// </summary>
public class DepartmentController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, string? q)
    {
        ViewBag.Active = active; ViewBag.Q = q;
        ViewBag.Stats = await svc.DepartmentStatsAsync();
        return View(await svc.DepartmentsAsync(active, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var d = await svc.DepartmentGetAsync(id);
        if (d == null) return NotFound();
        return View(d);
    }

    public IActionResult Create() =>
        View("Edit", new Department { IsActive = true, Level = 1, AutoDiv = true });

    public async Task<IActionResult> Edit(int id)
    {
        var d = await svc.DepartmentGetAsync(id);
        if (d == null) return NotFound();
        return View(d);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Department model, string[]? memberUserCode, string[]? memberFullName, string[]? memberEmail, string[]? memberPhone)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["Error"] = "Cần tên phòng ban.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        var members = new List<DepartmentMember>();
        if (memberUserCode != null)
        {
            for (var i = 0; i < memberUserCode.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(memberUserCode[i]) && string.IsNullOrWhiteSpace(memberFullName?[i]))
                    continue;
                members.Add(new DepartmentMember
                {
                    UserCode = memberUserCode[i]?.Trim() ?? "",
                    FullName = memberFullName != null && i < memberFullName.Length ? memberFullName[i]?.Trim() ?? "" : "",
                    Email = memberEmail != null && i < memberEmail.Length ? memberEmail[i]?.Trim() : null,
                    Phone = memberPhone != null && i < memberPhone.Length ? memberPhone[i]?.Trim() : null,
                    IsActive = true
                });
            }
        }

        var id = await svc.DepartmentSaveAsync(model, members);
        TempData["Success"] = "Đã lưu phòng ban.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.DepartmentToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái phòng ban.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
