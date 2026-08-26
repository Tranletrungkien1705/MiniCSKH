using Microsoft.EntityFrameworkCore;
using MiniCSKH.Data;
using MiniCSKH.Models;

namespace MiniCSKH.Services;

public record DashStats(int Open, int Overdue, int UnassignedOpen, int ResolvedToday,
    Dictionary<TicketStatus, int> ByStatus, List<(string Agent, int Open)> ByAgent);

public interface ITicketService
{
    Task<List<Ticket>> ListAsync(TicketStatus? status, int? agentId, TicketPriority? priority, string? q);
    Task<Ticket?> GetAsync(int id);
    Task<int> CreateAsync(Ticket t);
    Task AddCommentAsync(int ticketId, string author, string body, bool isInternal);
    Task AssignAsync(int ticketId, int? agentId);
    Task ChangeStatusAsync(int ticketId, TicketStatus status);
    Task ChangePriorityAsync(int ticketId, TicketPriority priority);
    Task<List<Agent>> AgentsAsync();
    Task<List<TicketCategory>> CategoriesAsync();
    Task<DashStats> DashboardAsync();
    // Knowledge base
    Task<List<KbArticle>> KbListAsync(string? q, string? category);
    Task<KbArticle?> KbGetAsync(int id, bool countView = false);
    Task<List<string>> KbCategoriesAsync();
}

public class TicketService(AppDbContext db) : ITicketService
{
    public async Task<List<Ticket>> ListAsync(TicketStatus? status, int? agentId, TicketPriority? priority, string? q)
    {
        var query = db.Tickets.Include(t => t.AssignedAgent).Include(t => t.Category).AsQueryable();
        if (status.HasValue) query = query.Where(t => t.Status == status.Value);
        if (agentId.HasValue) query = query.Where(t => t.AssignedAgentId == agentId.Value);
        if (priority.HasValue) query = query.Where(t => t.Priority == priority.Value);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(t => t.Subject.Contains(q) || t.Code.Contains(q) || t.CustomerName.Contains(q));
        var list = await query.ToListAsync();
        return list.OrderByDescending(t => t.CreatedAt).ToList();
    }

    public Task<Ticket?> GetAsync(int id) =>
        db.Tickets.Include(t => t.AssignedAgent).Include(t => t.Category).Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<int> CreateAsync(Ticket t)
    {
        var count = await db.Tickets.CountAsync();
        t.Code = $"TK{DateTime.Now:yyMM}{count + 1:D4}";
        if (t.CategoryId is { } cid)
        {
            var cat = await db.Categories.FindAsync(cid);
            if (cat != null) t.DueAt = t.CreatedAt.AddHours(cat.SlaHours);
        }
        t.Comments.Add(new TicketComment { Author = "Hệ thống", Body = $"Phiếu tạo qua kênh {t.Channel}.", CreatedAt = DateTime.Now });
        db.Tickets.Add(t);
        await db.SaveChangesAsync();
        return t.Id;
    }

    public async Task AddCommentAsync(int ticketId, string author, string body, bool isInternal)
    {
        var t = await db.Tickets.FindAsync(ticketId) ?? throw new KeyNotFoundException();
        db.Comments.Add(new TicketComment { TicketId = ticketId, Author = author, Body = body, IsInternal = isInternal });
        t.FirstResponseAt ??= DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task AssignAsync(int ticketId, int? agentId)
    {
        var t = await db.Tickets.FindAsync(ticketId) ?? throw new KeyNotFoundException();
        t.AssignedAgentId = agentId;
        if (agentId != null && t.Status == TicketStatus.New) t.Status = TicketStatus.InProgress;
        await db.SaveChangesAsync();
    }

    public async Task ChangeStatusAsync(int ticketId, TicketStatus status)
    {
        var t = await db.Tickets.FindAsync(ticketId) ?? throw new KeyNotFoundException();
        t.Status = status;
        if (status == TicketStatus.Resolved) t.ResolvedAt ??= DateTime.Now;
        if (status == TicketStatus.Closed) t.ClosedAt ??= DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task ChangePriorityAsync(int ticketId, TicketPriority priority)
    {
        var t = await db.Tickets.FindAsync(ticketId) ?? throw new KeyNotFoundException();
        t.Priority = priority;
        await db.SaveChangesAsync();
    }

    public Task<List<Agent>> AgentsAsync() => db.Agents.Where(a => a.IsActive).OrderBy(a => a.Name).ToListAsync();
    public Task<List<TicketCategory>> CategoriesAsync() => db.Categories.OrderBy(c => c.Name).ToListAsync();

    public async Task<DashStats> DashboardAsync()
    {
        var all = await db.Tickets.Include(t => t.AssignedAgent).ToListAsync();
        var open = all.Where(t => t.IsOpen).ToList();
        var byStatus = all.GroupBy(t => t.Status).ToDictionary(g => g.Key, g => g.Count());
        var byAgent = open.Where(t => t.AssignedAgent != null)
            .GroupBy(t => t.AssignedAgent!.Name)
            .Select(g => (g.Key, g.Count())).OrderByDescending(x => x.Item2).ToList();
        var today = DateTime.Today;
        return new DashStats(
            open.Count,
            open.Count(t => t.IsOverdue),
            open.Count(t => t.AssignedAgentId == null),
            all.Count(t => t.ResolvedAt?.Date == today),
            byStatus, byAgent);
    }

    public async Task<List<KbArticle>> KbListAsync(string? q, string? category)
    {
        var query = db.KbArticles.Where(a => a.IsPublished);
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(a => a.Category == category);
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(a => a.Title.Contains(q) || a.Body.Contains(q));
        var list = await query.ToListAsync();
        return list.OrderByDescending(a => a.Views).ToList();
    }

    public async Task<KbArticle?> KbGetAsync(int id, bool countView = false)
    {
        var a = await db.KbArticles.FindAsync(id);
        if (a != null && countView) { a.Views++; await db.SaveChangesAsync(); }
        return a;
    }

    public async Task<List<string>> KbCategoriesAsync() =>
        (await db.KbArticles.Where(a => a.IsPublished).Select(a => a.Category).ToListAsync())
        .Distinct().OrderBy(c => c).ToList();
}
