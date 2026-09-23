using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Quốc gia (port từ SkyCS: Mst_Country —
/// 11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_Country_Get`;
/// model 12.Dev.Common/idn.SkyCS.Common/Models/Mst_Country.cs;
/// controller 13.ClientGate/V20/idn.SkyCS.WebAPI/Controllers/MstCountryController.cs).
/// Master data nền của Trung tâm khách hàng — danh mục quốc gia/vùng lãnh thổ
/// kèm mã bưu chính mặc định. Được tham chiếu bởi địa chỉ hành chính
/// (Mst_Province.CountryCode) và hồ sơ khách hàng. Mỗi quốc gia có mã
/// (CountryCode), tên (CountryName), mã bưu chính (PostCode) và trạng thái
/// hoạt động (FlagActive).
/// </summary>
public class CountryController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, string? q)
    {
        ViewBag.Active = active; ViewBag.Q = q;
        ViewBag.Stats = await svc.CountryStatsAsync();
        return View(await svc.CountriesAsync(active, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var c = await svc.CountryGetAsync(id);
        if (c == null) return NotFound();
        return View(c);
    }

    public IActionResult Create() =>
        View("Edit", new Country { IsActive = true });

    public async Task<IActionResult> Edit(int id)
    {
        var c = await svc.CountryGetAsync(id);
        if (c == null) return NotFound();
        return View(c);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Country model)
    {
        // Theo Mst_Country: tên quốc gia bắt buộc.
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["Error"] = "Cần tên quốc gia.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        var id = await svc.CountrySaveAsync(model);
        TempData["Success"] = "Đã lưu quốc gia.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.CountryToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái quốc gia.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
