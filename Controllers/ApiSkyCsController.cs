using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// API JSON cho cum port 1:1 tu 2023.8A.SkyCS (SLA / Conversation / Work).
/// Nguon la ung dung MVC tra JsonView, moi hanh dong vua la trang vua la API — tach ra day
/// de trang .cshtml va SPA dung chung mot tang nghiep vu (Services/SkyCsService.cs).
/// </summary>
[ApiController]
[Route("api/v1")]
[Produces("application/json")]
public class ApiSkyCsController(ISkyCsService svc) : ControllerBase
{
    // ── SLA — nguon WorkSettingController.cs:556/577/602 ────────────────────────────
    [HttpGet("slas")]
    public async Task<IActionResult> Slas() => Ok((await svc.SlaListAsync()).Select(s => new
    {
        s.Id, s.Level, s.Description,
        firstResponseTime = s.FirstResponseTime, firstResponseTimeDesc = s.FirstResponseTimeDesc,
        everyResponseTime = s.EveryResponseTime, everyResponseTimeDesc = s.EveryResponseTimeDesc,
        resolutionTime = s.ResolutionTime, resolutionTimeDesc = s.ResolutionTimeDesc,
        s.BusinessHourId, s.ANDConditionDetails, s.ORConditionDetails,
        availableStatus = (int)s.AvailableStatus, s.IsActive
    }));

    [HttpGet("slas/{id:int}")]
    public async Task<IActionResult> Sla(int id)
    {
        var s = await svc.SlaGetAsync(id);
        return s == null ? NotFound(new { error = "Không tìm thấy SLA." }) : Ok(s);
    }

    [HttpPost("slas")]
    public async Task<IActionResult> SlaSave([FromBody] SlaReq r)
    {
        var m = new Sla
        {
            Id = r.Id, Level = r.Level ?? "", Description = r.Description ?? "",
            BusinessHourId = r.BusinessHourId, ANDConditionDetails = r.ANDConditionDetails,
            ORConditionDetails = r.ORConditionDetails, IsActive = r.IsActive,
            FirstResponseTimeDesc = r.FirstResponseTimeDesc ?? "00:00",
            EveryResponseTimeDesc = r.EveryResponseTimeDesc ?? "00:00",
            ResolutionTimeDesc = r.ResolutionTimeDesc ?? "00:00"
        };
        var (ok, msg, id) = await svc.SlaSaveAsync(m);
        return ok ? Ok(new { id, message = msg }) : BadRequest(new { error = msg });
    }

    // ── Hoi thoai — nguon ConversationController.cs:1072/1134 ───────────────────────
    [HttpGet("conversations")]
    public async Task<IActionResult> Conversations([FromQuery] int? statusId, [FromQuery] int? priorityId,
        [FromQuery] int? picId, [FromQuery] string? q)
        => Ok((await svc.ConvListAsync(statusId, priorityId, picId, q)).Select(c => new
        {
            c.Id, c.Title, c.CustomerId, customerName = c.Customer?.Name ?? c.CustomerName,
            c.ChannelId, c.SourceId, c.TypeId, c.StatusId, c.PriorityId, c.ProductId,
            c.SLAId, c.Tags, tagList = c.TagList, c.Deadline, c.TeamId, team = c.Team?.Name,
            c.PicId, pic = c.Pic?.Name, c.FirstResponseTime, c.MaxResponseTime, c.IsSlaSatisfied
        }));

    [HttpGet("conversations/{id:int}")]
    public async Task<IActionResult> Conversation(int id)
    {
        var c = await svc.ConvGetAsync(id);
        return c == null ? NotFound(new { error = "Không tìm thấy hội thoại." }) : Ok(c);
    }

    // ── Cong viec — nguon WorkController.cs:359/410 ─────────────────────────────────
    [HttpGet("works")]
    public async Task<IActionResult> Works([FromQuery] WorkStatuses? status, [FromQuery] int? userId, [FromQuery] string? q)
        => Ok((await svc.WorkListAsync(status, userId, q)).Select(w => new
        {
            w.Id, w.ConvId, w.ParentId, w.RootId, w.Name, w.Description, w.TypeId, workType = w.WorkType?.Name,
            w.WUId, w.TeamId, w.UserId, w.StdTime, w.ActualTime, w.Duration,
            w.Start, w.End, w.StartActual, w.EndActual, w.Deadline,
            status = (int)w.Status, w.StatusDesc, priority = (int)w.Priority,
            w.Price, w.Remark, w.WorkPlace, w.ProductId, w.IsGroup,
            followers = w.Followers.Select(f => new { f.UserId, role = (int)f.Role })
        }));

    /// <summary>
    /// Kiem tra TRUNG GIO ma khong ghi gi — tach rieng phan guard cua WorkManager.cs:237
    /// de goi truoc khi tao (nguon lam viec nay ben trong POST AddWork, tra ve NeedConfirm).
    /// </summary>
    [HttpPost("works/check-overlap")]
    public async Task<IActionResult> CheckOverlap([FromBody] WorkReq r)
    {
        var m = new Work { Id = r.Id, Start = r.Start, End = r.End, UserId = r.UserId };
        if (r.InchargeId is > 0) m.Followers.Add(new WorkFollow { UserId = r.InchargeId.Value, Role = WorkUserRoles.Incharge });
        var ds = await svc.WorkOverlappedAsync(m);
        return Ok(new { overlapped = ds.Count, items = ds.Select(w => new { w.Id, w.Name, w.Start, w.End, w.UserId }) });
    }

    [HttpPost("works")]
    public async Task<IActionResult> WorkAdd([FromBody] WorkReq r)
    {
        var m = new Work
        {
            Name = r.Name ?? "", Description = r.Description, Start = r.Start, End = r.End, Deadline = r.Deadline,
            TypeId = r.TypeId, ProductId = r.ProductId, WUId = r.WUId, TeamId = r.TeamId, ConvId = r.ConvId,
            ParentId = r.ParentId, StdTime = r.StdTime, Duration = r.Duration, Remark = r.Remark,
            WorkPlace = r.WorkPlace, Priority = r.Priority ?? WorkPriorites.Normal
        };
        var followers = new List<WorkFollow>();
        if (r.InchargeId is > 0) followers.Add(new WorkFollow { UserId = r.InchargeId.Value, Role = WorkUserRoles.Incharge });
        foreach (var uid in (r.MemberIds ?? []).Distinct().Where(u => u > 0 && u != r.InchargeId))
            followers.Add(new WorkFollow { UserId = uid, Role = WorkUserRoles.Normal });

        var (ok, msg, id, overlapped) = await svc.WorkAddAsync(m, followers, r.CurrentUserId, r.Confirm);
        if (ok) return Ok(new { id, message = msg });
        // nguon tra { Success=true, NeedConfirm=true, Html } khi trung gio (WorkController.cs:430-434)
        return overlapped.Count > 0
            ? Conflict(new { needConfirm = true, error = msg, items = overlapped.Select(w => new { w.Id, w.Name, w.Start, w.End }) })
            : BadRequest(new { error = msg });
    }
}

/// <summary>Thân yêu cầu lưu SLA — ba trường *Desc nhận dạng "HH:mm" đúng như EditSLA.cshtml post lên.</summary>
public record SlaReq(int Id, string? Level, string? Description, int BusinessHourId,
    string? FirstResponseTimeDesc, string? EveryResponseTimeDesc, string? ResolutionTimeDesc,
    string? ANDConditionDetails, string? ORConditionDetails, bool IsActive);

/// <summary>Thân yêu cầu tạo công việc — Start/End/Deadline là CHUỖI "yyyy-MM-dd HH:mm" (nguồn Work.cs:104-113).</summary>
public record WorkReq(int Id, string? Name, string? Description, string? Start, string? End, string? Deadline,
    int TypeId, int ProductId, int WUId, int TeamId, int ConvId, int ParentId, int StdTime, int Duration,
    string? Remark, string? WorkPlace, WorkPriorites? Priority, int UserId, int? InchargeId, int[]? MemberIds,
    int CurrentUserId, bool Confirm);
