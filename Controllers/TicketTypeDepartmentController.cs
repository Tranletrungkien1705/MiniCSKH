using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Gán loại phiếu cho phòng ban (port từ SkyCS: Map_TicketTypeDepartment —
/// 11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Map_TicketTypeDepartment_Get`/
/// `Map_TicketTypeDepartment_SaveX`; model 12.Dev.Common/idn.SkyCS.Common/
/// Models/Map_TicketTypeDepartment.cs; controller 13.ClientGate/V20/
/// idn.SkyCS.WebAPI/Controllers/MapTicketTypeDepartmentController.cs).
/// Bảng map quy định PHÒNG BAN nào phụ trách xử lý LOẠI PHIẾU nào — khi tạo
/// eTicket, hệ thống dò bảng này để định tuyến phiếu về đúng phòng ban theo
/// phân loại nghiệp vụ (Mst_TicketType). Là nền cho phân bổ phiếu tự động.
/// </summary>
public class TicketTypeDepartmentController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, string? ticketTypeCode, string? departmentCode, string? q)
    {
        ViewBag.Active = active; ViewBag.TicketTypeCode = ticketTypeCode;
        ViewBag.DepartmentCode = departmentCode; ViewBag.Q = q;
        ViewBag.Stats = await svc.TicketTypeDepartmentStatsAsync();
        ViewBag.TicketTypes = await svc.TicketTypesAsync(null, null, null);
        ViewBag.Departments = await svc.DepartmentsAsync(null, null);
        return View(await svc.TicketTypeDepartmentsAsync(active, ticketTypeCode, departmentCode, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var m = await svc.TicketTypeDepartmentGetAsync(id);
        if (m == null) return NotFound();
        return View(m);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.TicketTypes = await svc.TicketTypesAsync(true, null, null);
        ViewBag.Departments = await svc.DepartmentsAsync(true, null);
        return View("Edit", new TicketTypeDepartment { IsActive = true });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var m = await svc.TicketTypeDepartmentGetAsync(id);
        if (m == null) return NotFound();
        ViewBag.TicketTypes = await svc.TicketTypesAsync(null, null, null);
        ViewBag.Departments = await svc.DepartmentsAsync(null, null);
        return View(m);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(TicketTypeDepartment model)
    {
        // Theo Map_TicketTypeDepartment_SaveX: TicketType + DepartmentCode bắt buộc.
        if (string.IsNullOrWhiteSpace(model.TicketTypeCode))
        {
            TempData["Error"] = "Cần chọn loại phiếu (TicketType).";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }
        if (string.IsNullOrWhiteSpace(model.DepartmentCode))
        {
            TempData["Error"] = "Cần chọn phòng ban (DepartmentCode).";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        var id = await svc.TicketTypeDepartmentSaveAsync(model);
        TempData["Success"] = "Đã lưu gán loại phiếu cho phòng ban.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.TicketTypeDepartmentToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái gán loại phiếu.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await svc.TicketTypeDepartmentDeleteAsync(id);
        TempData["Success"] = "Đã xóa gán loại phiếu cho phòng ban.";
        return RedirectToAction(nameof(Index));
    }
}
