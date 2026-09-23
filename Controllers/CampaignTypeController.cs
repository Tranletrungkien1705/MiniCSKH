using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Loại chiến dịch (port từ SkyCS: Mst_CampaignType —
/// 11.BackEnd/V10/idn.SkyCS.Biz/Campaign.cs, `Mst_CampaignType_Get`/`_SaveX`;
/// models 12.Dev.Common/idn.SkyCS.Common/Models/Mst_CampaignType.cs,
/// Mst_CustomColumnCampaignType.cs, Mst_CustomerFeedBack.cs).
/// Loại chiến dịch phân loại chiến dịch gọi ra (Cpn_Campaign.CampaignTypeCode),
/// gắn danh sách trường tùy chỉnh (bố cục nhập liệu) và danh sách phản hồi khách
/// hàng chuẩn. Không xóa được loại đang có chiến dịch sử dụng.
/// </summary>
public class CampaignTypeController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, string? q)
    {
        ViewBag.Active = active; ViewBag.Q = q;
        ViewBag.Stats = await svc.CampaignTypeStatsAsync();
        return View(await svc.CampaignTypesAsync(active, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var t = await svc.CampaignTypeGetAsync(id);
        if (t == null) return NotFound();
        ViewBag.Usage = await svc.CampaignTypeUsageAsync(t.Code);
        return View(t);
    }

    public IActionResult Create() => View("Edit", new CampaignType { IsActive = true });

    public async Task<IActionResult> Edit(int id)
    {
        var t = await svc.CampaignTypeGetAsync(id);
        if (t == null) return NotFound();
        return View(t);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(CampaignType model,
        string[]? colName, string[]? colType, string[]? colRequired,
        string[]? fbName)
    {
        // Kiểm tra theo SkyCS Mst_CampaignType_SaveX: mã + tên bắt buộc.
        if (string.IsNullOrWhiteSpace(model.Code))
        {
            TempData["Error"] = "Cần mã loại chiến dịch (CampaignTypeCode).";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["Error"] = "Cần tên loại chiến dịch (CampaignTypeName).";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        // Dựng danh sách trường tùy chỉnh từ các mảng form (bỏ dòng trống).
        var columns = new List<CampaignTypeColumn>();
        var names = colName ?? [];
        for (int i = 0; i < names.Length; i++)
        {
            var name = names[i]?.Trim();
            if (string.IsNullOrWhiteSpace(name)) continue;
            var type = Enum.TryParse<SurveyFieldType>(colType?.ElementAtOrDefault(i), out var t) ? t : SurveyFieldType.Text;
            var required = (colRequired?.ElementAtOrDefault(i)) == "on" || (colRequired?.ElementAtOrDefault(i)) == "true";
            columns.Add(new CampaignTypeColumn
            {
                Name = name,
                Code = "C" + (i + 1),
                FieldType = type,
                Order = i + 1,
                IsRequired = required
            });
        }

        // Dựng danh sách phản hồi khách hàng (bỏ dòng trống).
        var feedbacks = new List<CampaignFeedback>();
        var fbs = fbName ?? [];
        for (int i = 0; i < fbs.Length; i++)
        {
            var name = fbs[i]?.Trim();
            if (string.IsNullOrWhiteSpace(name)) continue;
            feedbacks.Add(new CampaignFeedback { Name = name, Code = "FB" + (i + 1) });
        }

        var id = await svc.CampaignTypeSaveAsync(model, columns, feedbacks);
        TempData["Success"] = "Đã lưu loại chiến dịch.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.CampaignTypeToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái loại chiến dịch.";
        return RedirectToAction(nameof(Details), new { id });
    }
}