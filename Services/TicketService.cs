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
    // Phân loại nghiệp vụ (Mst_TicketType)
    Task<List<TicketType>> TicketTypesAsync(bool? active, BusinessType? businessType, string? q);
    Task<TicketType?> TicketTypeGetAsync(int id);
    Task<int> TicketTypeSaveAsync(TicketType model);
    Task TicketTypeToggleAsync(int id);
    Task<TicketTypeStats> TicketTypeStatsAsync();
    Task<DashStats> DashboardAsync();
    // Knowledge base
    Task<List<KbArticle>> KbListAsync(string? q, string? category);
    Task<KbArticle?> KbGetAsync(int id, bool countView = false);
    Task<List<string>> KbCategoriesAsync();
    // Call center
    Task<List<CallLog>> CallsAsync(CallDirection? dir, CallOutcome? outcome, string? q);
    Task<int> LogCallAsync(CallLog call);
    Task<(int todayTotal, int missed, int avgSeconds)> CallStatsAsync();
    // SLA
    Task<List<SlaPolicy>> SlaListAsync();
    Task<SlaPolicy?> SlaGetAsync(int id);
    Task<int> SlaSaveAsync(SlaPolicy p);
    Task<SlaStats> SlaStatsAsync();
    // Campaign (chiến dịch gọi ra)
    Task<List<Campaign>> CampaignsAsync(CampaignStatus? status, string? q);
    Task<Campaign?> CampaignGetAsync(int id);
    Task<int> CampaignCreateAsync(Campaign c);
    Task CampaignChangeStatusAsync(int id, CampaignStatus status);
    Task<CampaignCustomer?> CampaignCustomerGetAsync(int id);
    Task CampaignCustomerSaveAsync(int customerId, int? agentId, CampaignCustomerStatus status, string? feedback, string? remark);
    Task<(int total, int running, int doneCustomers, int pendingCustomers)> CampaignStatsAsync();
    // Đánh giá phiếu (eTicket Rating)
    Task<List<TicketRating>> RatingsAsync(RateType? type, RateResult? result, string? q);
    Task<TicketRating?> RatingGetAsync(int id);
    Task<List<TicketRating>> RatingsByTicketAsync(int ticketId);
    Task<int> RateTicketAsync(int ticketId, int score, RateResult result, string? comment, string ratedBy);
    Task ReviewRatingAsync(int ratingId, string reviewedBy, string? reviewNote);
    Task<RatingStats> RatingStatsAsync();
    // Mẫu khảo sát hài lòng (St_SurveyForm)
    Task<List<SurveyForm>> SurveyFormsAsync(bool? active, string? q);
    Task<SurveyForm?> SurveyFormGetAsync(int id);
    Task<int> SurveyFormSaveAsync(SurveyForm form, List<SurveyFormField> fields);
    Task SurveyFormToggleAsync(int id);
    Task<SurveyStats> SurveyStatsAsync();
    // Cải tiến chất lượng dịch vụ (SvImp_SvImprv)
    Task<List<ServiceImprovement>> SvImprvsAsync(bool? active, SvImprvItemType? type, string? q);
    Task<ServiceImprovement?> SvImprvGetAsync(int id);
    Task<int> SvImprvSaveAsync(ServiceImprovement model, List<SvImprvCriterion> criteria);
    Task SvImprvToggleAsync(int id);
    Task<SvImprvStats> SvImprvStatsAsync();
}

public record SlaStats(int Policies, int TicketsWithSla, int ViolatingFirstRes, int ViolatingResolution);

public record RatingStats(int Total, int Rated, int Reviewed, int Satisfied, int Unsatisfied, double AvgScore);

public record SurveyStats(int Total, int Active, int Used, int Fields);

public record SvImprvStats(int Total, int Active, int Used, int Criteria);

public record TicketTypeStats(int Total, int Active, int ETicket, int Campaign);

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
        db.Tickets.Include(t => t.AssignedAgent).Include(t => t.Category).Include(t => t.SlaPolicy).Include(t => t.Comments).Include(t => t.Ratings)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<int> CreateAsync(Ticket t)
    {
        var count = await db.Tickets.CountAsync();
        t.Code = $"TK{DateTime.Now:yyMM}{count + 1:D4}";
        if (t.CategoryId is { } cid)
        {
            var cat = await db.Categories.FirstOrDefaultAsync(x => x.Id == cid);
            if (cat != null) t.DueAt = t.CreatedAt.AddHours(cat.SlaHours);
        }
        t.Comments.Add(new TicketComment { Author = "Hệ thống", Body = $"Phiếu tạo qua kênh {t.Channel}.", CreatedAt = DateTime.Now });
        db.Tickets.Add(t);
        await db.SaveChangesAsync();
        return t.Id;
    }

    public async Task AddCommentAsync(int ticketId, string author, string body, bool isInternal)
    {
        var t = await db.Tickets.FirstOrDefaultAsync(x => x.Id == ticketId) ?? throw new KeyNotFoundException();
        db.Comments.Add(new TicketComment { TicketId = ticketId, Author = author, Body = body, IsInternal = isInternal });
        t.FirstResponseAt ??= DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task AssignAsync(int ticketId, int? agentId)
    {
        var t = await db.Tickets.FirstOrDefaultAsync(x => x.Id == ticketId) ?? throw new KeyNotFoundException();
        t.AssignedAgentId = agentId;
        if (agentId != null && t.Status == TicketStatus.New) t.Status = TicketStatus.InProgress;
        await db.SaveChangesAsync();
    }

    public async Task ChangeStatusAsync(int ticketId, TicketStatus status)
    {
        var t = await db.Tickets.FirstOrDefaultAsync(x => x.Id == ticketId) ?? throw new KeyNotFoundException();
        t.Status = status;
        if (status == TicketStatus.Resolved) t.ResolvedAt ??= DateTime.Now;
        if (status == TicketStatus.Closed) t.ClosedAt ??= DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task ChangePriorityAsync(int ticketId, TicketPriority priority)
    {
        var t = await db.Tickets.FirstOrDefaultAsync(x => x.Id == ticketId) ?? throw new KeyNotFoundException();
        t.Priority = priority;
        await db.SaveChangesAsync();
    }

    public Task<List<Agent>> AgentsAsync() => db.Agents.Where(a => a.IsActive).OrderBy(a => a.Name).ToListAsync();
    public Task<List<TicketCategory>> CategoriesAsync() => db.Categories.OrderBy(c => c.Name).ToListAsync();

    // ── Phân loại nghiệp vụ (Mst_TicketType) ─────────────────────────
    public async Task<List<TicketType>> TicketTypesAsync(bool? active, BusinessType? businessType, string? q)
    {
        var query = db.TicketTypes.AsQueryable();
        if (active.HasValue) query = query.Where(t => t.IsActive == active.Value);
        if (businessType.HasValue) query = query.Where(t => t.BusinessType == businessType.Value);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(t => t.Code.Contains(q) || t.AgentName.Contains(q) || t.CustomerName.Contains(q));
        var list = await query.ToListAsync();
        return list.OrderBy(t => t.Order).ThenBy(t => t.Code).ToList();
    }

    public Task<TicketType?> TicketTypeGetAsync(int id) =>
        db.TicketTypes.FirstOrDefaultAsync(t => t.Id == id);

    public async Task<int> TicketTypeSaveAsync(TicketType model)
    {
        if (model.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(model.Code)) model.Code = "TT-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            model.CreatedAt = model.UpdatedAt = DateTime.Now;
            db.TicketTypes.Add(model);
            await db.SaveChangesAsync();
            return model.Id;
        }
        var e = await db.TicketTypes.FirstOrDefaultAsync(x => x.Id == model.Id) ?? throw new KeyNotFoundException();
        e.Code = model.Code; e.AgentName = model.AgentName; e.CustomerName = model.CustomerName;
        e.CreateTemplate = model.CreateTemplate; e.DetailTemplate = model.DetailTemplate;
        e.HoCode = model.HoCode; e.BusinessType = model.BusinessType; e.IsActive = model.IsActive;
        e.Order = model.Order; e.Remark = model.Remark; e.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return e.Id;
    }

    public async Task TicketTypeToggleAsync(int id)
    {
        var t = await db.TicketTypes.FirstOrDefaultAsync(x => x.Id == id) ?? throw new KeyNotFoundException();
        t.IsActive = !t.IsActive;
        t.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task<TicketTypeStats> TicketTypeStatsAsync()
    {
        var list = await db.TicketTypes.ToListAsync();
        return new TicketTypeStats(
            list.Count,
            list.Count(t => t.IsActive),
            list.Count(t => t.BusinessType == BusinessType.ETicket),
            list.Count(t => t.BusinessType == BusinessType.Campaign));
    }

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
        var a = await db.KbArticles.FirstOrDefaultAsync(x => x.Id == id);
        if (a != null && countView) { a.Views++; await db.SaveChangesAsync(); }
        return a;
    }

    public async Task<List<string>> KbCategoriesAsync() =>
        (await db.KbArticles.Where(a => a.IsPublished).Select(a => a.Category).ToListAsync())
        .Distinct().OrderBy(c => c).ToList();

    public async Task<List<CallLog>> CallsAsync(CallDirection? dir, CallOutcome? outcome, string? q)
    {
        var query = db.Calls.Include(c => c.Agent).Include(c => c.Ticket).AsQueryable();
        if (dir.HasValue) query = query.Where(c => c.Direction == dir.Value);
        if (outcome.HasValue) query = query.Where(c => c.Outcome == outcome.Value);
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(c => c.PhoneNumber.Contains(q) || (c.CustomerName ?? "").Contains(q));
        var list = await query.ToListAsync();
        return list.OrderByDescending(c => c.StartedAt).ToList();
    }

    public async Task<int> LogCallAsync(CallLog call)
    {
        db.Calls.Add(call);
        await db.SaveChangesAsync();
        return call.Id;
    }

    public async Task<(int todayTotal, int missed, int avgSeconds)> CallStatsAsync()
    {
        var calls = await db.Calls.ToListAsync();
        var today = calls.Where(c => c.StartedAt.Date == DateTime.Today).ToList();
        var answered = today.Where(c => c.Outcome == CallOutcome.Answered && c.DurationSeconds > 0).ToList();
        var avg = answered.Count > 0 ? (int)answered.Average(c => c.DurationSeconds) : 0;
        return (today.Count, today.Count(c => c.Outcome == CallOutcome.Missed), avg);
    }

    // ── SLA ──────────────────────────
    public async Task<List<SlaPolicy>> SlaListAsync()
    {
        var list = await db.SlaPolicies.ToListAsync();
        return list.OrderBy(p => p.FirstResMinutes).ToList();
    }

    public Task<SlaPolicy?> SlaGetAsync(int id) => db.SlaPolicies.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<int> SlaSaveAsync(SlaPolicy p)
    {
        if (p.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(p.Code)) p.Code = "SLA-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            db.SlaPolicies.Add(p);
        }
        else
        {
            var e = await db.SlaPolicies.FirstOrDefaultAsync(x => x.Id == p.Id) ?? throw new KeyNotFoundException();
            e.Code = p.Code; e.Level = p.Level; e.Description = p.Description;
            e.FirstResMinutes = p.FirstResMinutes; e.ResolutionMinutes = p.ResolutionMinutes; e.IsActive = p.IsActive;
        }
        await db.SaveChangesAsync();
        return p.Id;
    }

    public async Task<SlaStats> SlaStatsAsync()
    {
        var policies = await db.SlaPolicies.CountAsync();
        var tickets = await db.Tickets.Include(t => t.SlaPolicy).Where(t => t.SlaPolicyId != null).ToListAsync();
        return new SlaStats(policies, tickets.Count,
            tickets.Count(t => t.ViolatesFirstResponse), tickets.Count(t => t.ViolatesResolution));
    }

    // ── Campaign ─────────────────────
    public async Task<List<Campaign>> CampaignsAsync(CampaignStatus? status, string? q)
    {
        var query = db.Campaigns.Include(c => c.Customers).AsQueryable();
        if (status.HasValue) query = query.Where(c => c.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c => c.Name.Contains(q) || c.Code.Contains(q));
        var list = await query.ToListAsync();
        return list.OrderByDescending(c => c.CreatedAt).ToList();
    }

    public Task<Campaign?> CampaignGetAsync(int id) =>
        db.Campaigns.Include(c => c.Customers).ThenInclude(x => x.Agent)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<int> CampaignCreateAsync(Campaign c)
    {
        var count = await db.Campaigns.CountAsync();
        c.Code = $"CP{DateTime.Now:yyMM}{count + 1:D4}";
        db.Campaigns.Add(c);
        await db.SaveChangesAsync();
        return c.Id;
    }

    public async Task CampaignChangeStatusAsync(int id, CampaignStatus status)
    {
        var c = await db.Campaigns.FirstOrDefaultAsync(x => x.Id == id) ?? throw new KeyNotFoundException();
        c.Status = status;
        if (status == CampaignStatus.Approved) c.ApprovedAt ??= DateTime.Now;
        if (status == CampaignStatus.Started) c.StartAt ??= DateTime.Now;
        if (status == CampaignStatus.Finished) c.FinishAt ??= DateTime.Now;
        await db.SaveChangesAsync();
    }

    public Task<CampaignCustomer?> CampaignCustomerGetAsync(int id) =>
        db.CampaignCustomers.Include(x => x.Agent).Include(x => x.Campaign)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task CampaignCustomerSaveAsync(int customerId, int? agentId, CampaignCustomerStatus status, string? feedback, string? remark)
    {
        var cc = await db.CampaignCustomers.FirstOrDefaultAsync(x => x.Id == customerId) ?? throw new KeyNotFoundException();
        cc.AgentId = agentId;
        cc.Status = status;
        cc.Feedback = feedback;
        cc.Remark = remark;
        cc.LastCallAt = DateTime.Now;
        cc.CallCount++;
        await db.SaveChangesAsync();
    }

    public async Task<(int total, int running, int doneCustomers, int pendingCustomers)> CampaignStatsAsync()
    {
        var campaigns = await db.Campaigns.Include(c => c.Customers).ToListAsync();
        var customers = campaigns.SelectMany(c => c.Customers).ToList();
        return (
            campaigns.Count,
            campaigns.Count(c => c.Status == CampaignStatus.Started),
            customers.Count(c => c.Status == CampaignCustomerStatus.Done),
            customers.Count(c => c.Status == CampaignCustomerStatus.Pending));
    }

    // ── Đánh giá phiếu (eTicket Rating) ──────────────────────────────
    public async Task<List<TicketRating>> RatingsAsync(RateType? type, RateResult? result, string? q)
    {
        var query = db.Ratings.Include(r => r.Ticket).AsQueryable();
        if (type.HasValue) query = query.Where(r => r.RateType == type.Value);
        if (result.HasValue) query = query.Where(r => r.Result == result.Value);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(r => r.Ticket.Code.Contains(q) || r.Ticket.Subject.Contains(q) || r.RatedBy.Contains(q));
        var list = await query.ToListAsync();
        return list.OrderByDescending(r => r.RatedAt).ToList();
    }

    public Task<TicketRating?> RatingGetAsync(int id) =>
        db.Ratings.Include(r => r.Ticket).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<List<TicketRating>> RatingsByTicketAsync(int ticketId)
    {
        var list = await db.Ratings.Where(r => r.TicketId == ticketId).ToListAsync();
        return list.OrderByDescending(r => r.RatedAt).ToList();
    }

    public async Task<int> RateTicketAsync(int ticketId, int score, RateResult result, string? comment, string ratedBy)
    {
        var t = await db.Tickets.FirstOrDefaultAsync(x => x.Id == ticketId) ?? throw new KeyNotFoundException();
        // Theo SkyCS: chỉ đánh giá khi cờ FlagRated chưa bật (0/null).
        if (t.FlagRated) throw new InvalidOperationException("Phiếu đã được đánh giá.");
        var round = await db.Ratings.CountAsync(r => r.TicketId == ticketId) + 1;
        var rating = new TicketRating
        {
            TicketId = ticketId,
            RateRound = round,
            RateType = RateType.Rate,
            Status = RateStatus.Rated,
            Result = result,
            Score = Math.Clamp(score, 1, 5),
            Comment = comment,
            RatedBy = string.IsNullOrWhiteSpace(ratedBy) ? "Khách hàng" : ratedBy,
            RatedAt = DateTime.Now
        };
        db.Ratings.Add(rating);
        t.FlagRated = true;   // ET_Ticket.FlagRated = '1'
        await db.SaveChangesAsync();
        return rating.Id;
    }

    public async Task ReviewRatingAsync(int ratingId, string reviewedBy, string? reviewNote)
    {
        var r = await db.Ratings.FirstOrDefaultAsync(x => x.Id == ratingId) ?? throw new KeyNotFoundException();
        r.RateType = RateType.Review;
        r.Status = RateStatus.Reviewed;
        r.ReviewedBy = string.IsNullOrWhiteSpace(reviewedBy) ? "Agent" : reviewedBy;
        r.ReviewNote = reviewNote;
        await db.SaveChangesAsync();
    }

    public async Task<RatingStats> RatingStatsAsync()
    {
        var all = await db.Ratings.ToListAsync();
        var rated = all.Where(r => r.RateType == RateType.Rate).ToList();
        var avg = rated.Count > 0 ? Math.Round(rated.Average(r => r.Score), 2) : 0;
        return new RatingStats(
            all.Count,
            rated.Count,
            all.Count(r => r.Status == RateStatus.Reviewed),
            rated.Count(r => r.Result == RateResult.Satisfied),
            rated.Count(r => r.Result == RateResult.Unsatisfied),
            avg);
    }

    // ── Mẫu khảo sát hài lòng (St_SurveyForm) ────────────────────────
    public async Task<List<SurveyForm>> SurveyFormsAsync(bool? active, string? q)
    {
        var query = db.SurveyForms.Include(f => f.Fields).AsQueryable();
        if (active.HasValue) query = query.Where(f => f.IsActive == active.Value);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(f => f.Name.Contains(q) || f.Code.Contains(q));
        var list = await query.ToListAsync();
        return list.OrderByDescending(f => f.UpdatedAt).ToList();
    }

    public Task<SurveyForm?> SurveyFormGetAsync(int id) =>
        db.SurveyForms.Include(f => f.Fields).FirstOrDefaultAsync(f => f.Id == id);

    public async Task<int> SurveyFormSaveAsync(SurveyForm form, List<SurveyFormField> fields)
    {
        if (form.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(form.Code)) form.Code = "SAT-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            form.CreatedAt = form.UpdatedAt = DateTime.Now;
            foreach (var f in fields) form.Fields.Add(f);
            db.SurveyForms.Add(form);
            await db.SaveChangesAsync();
            return form.Id;
        }
        var e = await db.SurveyForms.Include(x => x.Fields).FirstOrDefaultAsync(x => x.Id == form.Id)
            ?? throw new KeyNotFoundException();
        e.Name = form.Name; e.Description = form.Description; e.Remark = form.Remark;
        e.IsActive = form.IsActive; e.UpdatedAt = DateTime.Now;
        // Thay toàn bộ danh sách trường (mẫu khảo sát là cấu hình, không giữ lịch sử trường).
        db.SurveyFormFields.RemoveRange(e.Fields);
        e.Fields.Clear();
        foreach (var f in fields) e.Fields.Add(f);
        await db.SaveChangesAsync();
        return e.Id;
    }

    public async Task SurveyFormToggleAsync(int id)
    {
        var f = await db.SurveyForms.FirstOrDefaultAsync(x => x.Id == id) ?? throw new KeyNotFoundException();
        f.IsActive = !f.IsActive;
        f.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task<SurveyStats> SurveyStatsAsync()
    {
        var forms = await db.SurveyForms.Include(f => f.Fields).ToListAsync();
        return new SurveyStats(
            forms.Count,
            forms.Count(f => f.IsActive),
            forms.Count(f => f.IsUsed),
            forms.Sum(f => f.Fields.Count));
    }

    // ── Cải tiến chất lượng dịch vụ (SvImp_SvImprv) ──────────────────
    public async Task<List<ServiceImprovement>> SvImprvsAsync(bool? active, SvImprvItemType? type, string? q)
    {
        var query = db.ServiceImprovements.Include(s => s.Criteria).AsQueryable();
        if (active.HasValue) query = query.Where(s => s.IsActive == active.Value);
        if (type.HasValue) query = query.Where(s => s.ItemType == type.Value);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(s => s.Name.Contains(q) || s.Code.Contains(q));
        var list = await query.ToListAsync();
        return list.OrderByDescending(s => s.UpdatedAt).ToList();
    }

    public Task<ServiceImprovement?> SvImprvGetAsync(int id) =>
        db.ServiceImprovements.Include(s => s.Criteria).FirstOrDefaultAsync(s => s.Id == id);

    public async Task<int> SvImprvSaveAsync(ServiceImprovement model, List<SvImprvCriterion> criteria)
    {
        if (model.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(model.Code)) model.Code = "SI-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            model.CreatedAt = model.UpdatedAt = DateTime.Now;
            foreach (var c in criteria) model.Criteria.Add(c);
            db.ServiceImprovements.Add(model);
            await db.SaveChangesAsync();
            return model.Id;
        }
        var e = await db.ServiceImprovements.Include(x => x.Criteria).FirstOrDefaultAsync(x => x.Id == model.Id)
            ?? throw new KeyNotFoundException();
        e.Name = model.Name; e.ItemType = model.ItemType; e.Remark = model.Remark;
        e.IsActive = model.IsActive; e.UpdatedAt = DateTime.Now;
        // Thay toàn bộ danh sách tiêu chí (bộ cải tiến là cấu hình, không giữ lịch sử tiêu chí).
        db.SvImprvCriteria.RemoveRange(e.Criteria);
        e.Criteria.Clear();
        foreach (var c in criteria) e.Criteria.Add(c);
        await db.SaveChangesAsync();
        return e.Id;
    }

    public async Task SvImprvToggleAsync(int id)
    {
        var s = await db.ServiceImprovements.FirstOrDefaultAsync(x => x.Id == id) ?? throw new KeyNotFoundException();
        s.IsActive = !s.IsActive;
        s.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task<SvImprvStats> SvImprvStatsAsync()
    {
        var list = await db.ServiceImprovements.Include(s => s.Criteria).ToListAsync();
        return new SvImprvStats(
            list.Count,
            list.Count(s => s.IsActive),
            list.Count(s => s.IsUsed),
            list.Sum(s => s.Criteria.Count));
    }
}
