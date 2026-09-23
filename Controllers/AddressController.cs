using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Danh mục địa chỉ (port từ SkyCS: Mst_Province / Mst_District / Mst_Ward —
/// 11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_Province_Get`/`Mst_District_Get`/`Mst_Ward_Get`;
/// models 12.Dev.Common/idn.SkyCS.Common/Models/Mst_Province.cs, Mst_District.cs,
/// CustomerCentrer/Mst_Ward.cs).
/// Danh mục địa chỉ hành chính 3 cấp (Tỉnh/Thành → Quận/Huyện → Phường/Xã),
/// là master data nền cho địa chỉ khách hàng (Mst_Customer.ProvinceCode/DistrictCode).
/// </summary>
public class AddressController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(AddressLevel? level, bool? active, string? parentCode, string? q)
    {
        ViewBag.Level = level; ViewBag.Active = active; ViewBag.ParentCode = parentCode; ViewBag.Q = q;
        ViewBag.Stats = await svc.AddressStatsAsync();
        return View(await svc.AddressesAsync(level, active, parentCode, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var a = await svc.AddressGetAsync(id);
        if (a == null) return NotFound();
        return View(a);
    }

    public IActionResult Create(AddressLevel? level) =>
        View("Edit", new Address { IsActive = true, Level = level ?? AddressLevel.Province });

    public async Task<IActionResult> Edit(int id)
    {
        var a = await svc.AddressGetAsync(id);
        if (a == null) return NotFound();
        return View(a);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Address model)
    {
        // Kiểm tra theo SkyCS Mst_Province/District/Ward: mã và tên bắt buộc.
        if (string.IsNullOrWhiteSpace(model.Code))
        {
            TempData["Error"] = "Cần mã địa chỉ (Code).";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["Error"] = "Cần tên địa chỉ (Name).";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }
        // Quận/Huyện và Phường/Xã phải gắn cấp trên (ProvinceCode / DistrictCode).
        if (model.Level != AddressLevel.Province && string.IsNullOrWhiteSpace(model.ParentCode))
        {
            TempData["Error"] = "Quận/Huyện và Phường/Xã cần mã cấp trên (ParentCode).";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        var id = await svc.AddressSaveAsync(model);
        TempData["Success"] = "Đã lưu địa chỉ.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.AddressToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái địa chỉ.";
        return RedirectToAction(nameof(Details), new { id });
    }
}