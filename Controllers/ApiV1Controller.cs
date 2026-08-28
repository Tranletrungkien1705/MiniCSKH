using Microsoft.AspNetCore.Mvc;
using MiniCSKH.Data;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Controllers;

/// <summary>
/// API JSON cho SPA React (agent). DTO phẳng. Dashboard cache Redis 15s theo tenant (X-Cache).
/// CSKH đa kênh: eTicket (New→InProgress→WaitingCustomer→Resolved→Closed), gán agent, comment, SLA; call center; KB. Portal /Portal giữ nguyên.
/// </summary>
[ApiController]
[Route("api/v1")]
[Produces("application/json")]
public class ApiV1Controller(ITicketService svc, ICache cache, ITenantContext tenant) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var key = $"cskh:dash:{tenant.OrgId}";
        var hit = await cache.GetAsync<DashDto>(key);
        if (hit != null) { Response.Headers["X-Cache"] = "HIT"; return Ok(hit); }
        var d = await svc.DashboardAsync();
        var dto = new DashDto(d.Open, d.Overdue, d.UnassignedOpen, d.ResolvedToday,
            d.ByStatus.Select(kv => new StatusCountDto((int)kv.Key, kv.Value)).ToList(),
            d.ByAgent.Select(x => new AgentCountDto(x.Agent, x.Open)).ToList());
        await cache.SetAsync(key, dto, TimeSpan.FromSeconds(15));
        Response.Headers["X-Cache"] = "MISS";
        return Ok(dto);
    }

    [HttpGet("agents")]
    public async Task<IActionResult> Agents() => Ok((await svc.AgentsAsync()).Select(a => new { a.Id, a.Name }));

    [HttpGet("categories")]
    public async Task<IActionResult> Categories() => Ok((await svc.CategoriesAsync()).Select(c => new { c.Id, c.Name }));

    [HttpGet("tickets")]
    public async Task<IActionResult> Tickets([FromQuery] TicketStatus? status, [FromQuery] int? agentId, [FromQuery] TicketPriority? priority, [FromQuery] string? q)
        => Ok((await svc.ListAsync(status, agentId, priority, q)).Select(ToListDto));

    [HttpGet("tickets/{id:int}")]
    public async Task<IActionResult> Ticket(int id)
    {
        var t = await svc.GetAsync(id);
        if (t == null) return NotFound(new { error = "Không tìm thấy ticket." });
        return Ok(new
        {
            t.Id, t.Code, t.Subject, t.Description, t.CustomerName, t.CustomerPhone, t.CustomerEmail,
            channel = (int)t.Channel, channelText = t.Channel.ToString(), priority = (int)t.Priority, status = (int)t.Status,
            category = t.Category?.Name, agent = t.AssignedAgent?.Name, agentId = t.AssignedAgentId,
            t.CreatedAt, t.DueAt, t.ResolvedAt, overdue = t.IsOverdue, isOpen = t.IsOpen,
            comments = t.Comments.OrderBy(c => c.Id).Select(c => new { c.Author, c.Body, c.IsInternal, c.CreatedAt })
        });
    }

    [HttpPost("tickets")]
    public async Task<IActionResult> Create([FromBody] TicketReq r)
    {
        if (string.IsNullOrWhiteSpace(r.Subject) || string.IsNullOrWhiteSpace(r.CustomerName))
            return BadRequest(new { error = "Cần tiêu đề và tên khách." });
        var id = await svc.CreateAsync(new Ticket
        {
            Subject = r.Subject.Trim(), Description = r.Description, CustomerName = r.CustomerName.Trim(), CustomerPhone = r.CustomerPhone, CustomerEmail = r.CustomerEmail,
            Channel = (Channel)r.Channel, Priority = (TicketPriority)r.Priority, CategoryId = r.CategoryId
        });
        return Ok(new { id });
    }

    [HttpPost("tickets/{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] StatusReq r)
    {
        await svc.ChangeStatusAsync(id, (TicketStatus)r.Status);
        return Ok(new { ok = true });
    }

    [HttpPost("tickets/{id:int}/priority")]
    public async Task<IActionResult> ChangePriority(int id, [FromBody] PriorityReq r)
    {
        await svc.ChangePriorityAsync(id, (TicketPriority)r.Priority);
        return Ok(new { ok = true });
    }

    [HttpPost("tickets/{id:int}/assign")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignReq r)
    {
        await svc.AssignAsync(id, r.AgentId);
        return Ok(new { ok = true });
    }

    [HttpPost("tickets/{id:int}/comments")]
    public async Task<IActionResult> Comment(int id, [FromBody] CommentReq r)
    {
        await svc.AddCommentAsync(id, r.Author ?? "Agent", r.Body ?? "", r.IsInternal);
        return Ok(new { ok = true });
    }

    [HttpGet("kb")]
    public async Task<IActionResult> Kb([FromQuery] string? q, [FromQuery] string? category)
        => Ok((await svc.KbListAsync(q, category)).Select(a => new { a.Id, a.Title, a.Category, a.Views }));

    [HttpGet("kb/{id:int}")]
    public async Task<IActionResult> KbArticle(int id)
    {
        var a = await svc.KbGetAsync(id, true);
        return a == null ? NotFound(new { error = "Không tìm thấy bài viết." }) : Ok(new { a.Id, a.Title, a.Category, a.Body, a.Views });
    }

    [HttpGet("calls")]
    public async Task<IActionResult> Calls([FromQuery] CallDirection? dir, [FromQuery] CallOutcome? outcome, [FromQuery] string? q)
        => Ok((await svc.CallsAsync(dir, outcome, q)).Select(c => new { c.Id, dir = (int)c.Direction, dirText = c.Direction.ToString(), outcome = (int)c.Outcome, outcomeText = c.Outcome.ToString(), c.PhoneNumber, c.CustomerName, c.DurationSeconds, c.Note, c.StartedAt }));

    [HttpGet("call-stats")]
    public async Task<IActionResult> CallStats()
    {
        var (todayTotal, missed, avgSeconds) = await svc.CallStatsAsync();
        return Ok(new { todayTotal, missed, avgSeconds });
    }

    [HttpPost("calls")]
    public async Task<IActionResult> LogCall([FromBody] CallReq r)
    {
        var id = await svc.LogCallAsync(new CallLog { Direction = (CallDirection)r.Direction, Outcome = (CallOutcome)r.Outcome, PhoneNumber = r.PhoneNumber ?? "", CustomerName = r.CustomerName ?? "", DurationSeconds = r.DurationSeconds, Note = r.Note });
        return Ok(new { id });
    }

    private static object ToListDto(Ticket t) => new
    {
        t.Id, t.Code, t.Subject, t.CustomerName, channel = t.Channel.ToString(), priority = (int)t.Priority, status = (int)t.Status,
        category = t.Category?.Name, agent = t.AssignedAgent?.Name, t.CreatedAt, t.DueAt, overdue = t.IsOverdue
    };
}

public record DashDto(int Open, int Overdue, int UnassignedOpen, int ResolvedToday, List<StatusCountDto> ByStatus, List<AgentCountDto> ByAgent);
public record StatusCountDto(int Status, int Count);
public record AgentCountDto(string Agent, int Open);

public class TicketReq { public string Subject { get; set; } = ""; public string? Description { get; set; } public string CustomerName { get; set; } = ""; public string? CustomerPhone { get; set; } public string? CustomerEmail { get; set; } public int Channel { get; set; } public int Priority { get; set; } public int? CategoryId { get; set; } }
public class StatusReq { public int Status { get; set; } }
public class PriorityReq { public int Priority { get; set; } }
public class AssignReq { public int? AgentId { get; set; } }
public class CommentReq { public string? Author { get; set; } public string? Body { get; set; } public bool IsInternal { get; set; } }
public class CallReq { public int Direction { get; set; } public int Outcome { get; set; } public string? PhoneNumber { get; set; } public string? CustomerName { get; set; } public int DurationSeconds { get; set; } public string? Note { get; set; } }
