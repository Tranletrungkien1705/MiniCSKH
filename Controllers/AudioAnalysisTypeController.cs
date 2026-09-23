using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// Loại phân tích âm thanh (port từ SkyCS: Mst_AudioAnalysisType —
/// 11.BackEnd/V10/idn.SkyCS.Biz/ServiceImprovement.cs, `Mst_AudioAnalysisType_Get`/
/// `Mst_AudioAnalysisType_CheckDB`; model 12.Dev.Common/idn.SkyCS.Common/Models/
/// Mst_AudioAnalysisType.cs; controller 13.ClientGate/V20/idn.SkyCS.WebAPI/
/// Controllers/MstAudioAnalysisTypeController.cs; hằng số TConst.AudioAnalysisType
/// = SPEECHTOTEXT/AUDIOTOSOUNDVALUE; cột xác nhận qua TblMst_AudioAnalysisType
/// trong Const.Main.1.cs; seed thật 20230825.z11.UpdDB.50.ServiceImprovement.sql).
/// Đây là master data của phân hệ Cải tiến chất lượng dịch vụ — quy định CÁCH
/// phân tích file ghi âm cuộc gọi (chuyển giọng nói thành văn bản, phân tích
/// âm lượng/tần số…). Bảng con SvImp_AudioAnalysis tham chiếu mã này qua cột
/// AudioAnalysisType để cấu hình phân tích tự động cuộc gọi.
/// </summary>
public class AudioAnalysisTypeController(ITicketService svc) : Controller
{
    public async Task<IActionResult> Index(bool? active, string? q)
    {
        ViewBag.Active = active; ViewBag.Q = q;
        ViewBag.Stats = await svc.AudioAnalysisTypeStatsAsync();
        return View(await svc.AudioAnalysisTypesAsync(active, q));
    }

    public async Task<IActionResult> Details(int id)
    {
        var m = await svc.AudioAnalysisTypeGetAsync(id);
        if (m == null) return NotFound();
        return View(m);
    }

    public IActionResult Create() => View("Edit", new AudioAnalysisType { IsActive = true });

    public async Task<IActionResult> Edit(int id)
    {
        var m = await svc.AudioAnalysisTypeGetAsync(id);
        if (m == null) return NotFound();
        return View(m);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(AudioAnalysisType model)
    {
        // Theo Mst_AudioAnalysisType: tên loại phân tích bắt buộc; để trống mã sẽ tự sinh.
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["Error"] = "Cần nhập tên loại phân tích âm thanh (AudioAnalysisTypeName).";
            return RedirectToAction(model.Id == 0 ? nameof(Create) : nameof(Edit), new { id = model.Id });
        }

        var id = await svc.AudioAnalysisTypeSaveAsync(model);
        TempData["Success"] = "Đã lưu loại phân tích âm thanh.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await svc.AudioAnalysisTypeToggleAsync(id);
        TempData["Success"] = "Đã đổi trạng thái loại phân tích âm thanh.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
