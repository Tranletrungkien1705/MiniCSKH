using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Mẫu khảo sát hài lòng (St_SurveyForm — port từ SkyCS).
/// Thiết lập mẫu khảo sát gồm nhiều trường (St_SurveyFormDetail); mẫu được
/// dùng để đánh giá eTicket sau khi đóng phiếu (ET_TicketRated.HstIdx).
/// </summary>
public class SurveyController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, string? q)
    {
        ViewBag.Active = active; ViewBag.Q = q;
        ViewBag.Stats = await svc.SurveyStatsAsync();
        return View(await svc.SurveyFormsAsync(active, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var f = await svc.SurveyFormGetAsync(id);
        if (f == null) return NotFound();
        return View(f);
    }

    public IActionResult Create() => View("Edit", new SurveyForm { IsActive = true });

    public async Task<IActionResult> Edit(int id)
    {
        var f = await svc.SurveyFormGetAsync(id);
        if (f == null) return NotFound();
        return View(f);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(SurveyForm model, string[]? fieldName, string[]? fieldType, string[]? fieldRequired, string[]? fieldOptions)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["Error"] = "Cần tên mẫu khảo sát.";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        // Dựng danh sách trường từ các mảng form (bỏ dòng trống).
        var fields = new List<SurveyFormField>();
        var names = fieldName ?? [];
        for (int i = 0; i < names.Length; i++)
        {
            var name = names[i]?.Trim();
            if (string.IsNullOrWhiteSpace(name)) continue;
            var type = Enum.TryParse<SurveyFieldType>(fieldType?.ElementAtOrDefault(i), out var t) ? t : SurveyFieldType.Text;
            var required = (fieldRequired?.ElementAtOrDefault(i)) == "on" || (fieldRequired?.ElementAtOrDefault(i)) == "true";
            fields.Add(new SurveyFormField
            {
                Name = name,
                Code = "F" + (i + 1),
                FieldType = type,
                Order = i + 1,
                IsRequired = required,
                Options = fieldOptions?.ElementAtOrDefault(i)
            });
        }

        var id = await svc.SurveyFormSaveAsync(model, fields);
        TempData["Success"] = "Đã lưu mẫu khảo sát.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.SurveyFormToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái mẫu khảo sát.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
