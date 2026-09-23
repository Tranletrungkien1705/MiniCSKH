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
    // Trung tâm khách hàng (Mst_Customer)
    Task<List<Customer>> CustomersAsync(bool? active, CustomerType? type, string? groupCode, string? q);
    Task<Customer?> CustomerGetAsync(int id);
    Task<int> CustomerSaveAsync(Customer model, List<CustomerContact> contacts);
    Task CustomerToggleAsync(int id);
    Task<CustomerStats> CustomerStatsAsync();
    Task<List<CustomerGroup>> CustomerGroupsAsync();
    // Thiết lập phân bổ phiếu tự động (Mst_EstablishAllocateETicket)
    Task<List<AllocateRule>> AllocateRulesAsync(bool? active, string? q);
    Task<AllocateRule?> AllocateRuleGetAsync(int id);
    Task<int> AllocateRuleSaveAsync(AllocateRule model, List<AllocateAgent> agents);
    Task AllocateRuleToggleAsync(int id);
    Task<AllocateStats> AllocateStatsAsync();
    // Thiết lập nhắc nhở phiếu (Mst_EstablishRemindETicket)
    Task<List<ReminderRule>> ReminderRulesAsync(bool? active, RemindChannel? channel, string? q);
    Task<ReminderRule?> ReminderRuleGetAsync(int id);
    Task<int> ReminderRuleSaveAsync(ReminderRule model);
    Task ReminderRuleToggleAsync(int id);
    Task<ReminderStats> ReminderStatsAsync();
    // Danh mục phiếu (Mst_TicketStatus/TicketPriority/TicketSource/ReceptionChannel)
    Task<List<TicketCatalog>> TicketCatalogsAsync(TicketCatalogKind? kind, bool? active, string? q);
    Task<TicketCatalog?> TicketCatalogGetAsync(int id);
    Task<int> TicketCatalogSaveAsync(TicketCatalog model);
    Task TicketCatalogToggleAsync(int id);
    Task<TicketCatalogStats> TicketCatalogStatsAsync();
    // Phòng ban (Mst_Department)
    Task<List<Department>> DepartmentsAsync(bool? active, string? q);
    Task<Department?> DepartmentGetAsync(int id);
    Task<int> DepartmentSaveAsync(Department model, List<DepartmentMember> members);
    Task DepartmentToggleAsync(int id);
    Task<DepartmentStats> DepartmentStatsAsync();
    // Điều khoản thanh toán (Mst_PaymentTerm)
    Task<List<PaymentTerm>> PaymentTermsAsync(bool? active, PTType? type, string? q);
    Task<PaymentTerm?> PaymentTermGetAsync(int id);
    Task<int> PaymentTermSaveAsync(PaymentTerm model);
    Task PaymentTermToggleAsync(int id);
    Task<PaymentTermStats> PaymentTermStatsAsync();
    // Vùng thị trường (Mst_Area)
    Task<List<Area>> AreasAsync(bool? active, string? q);
    Task<Area?> AreaGetAsync(int id);
    Task<int> AreaSaveAsync(Area model);
    Task AreaToggleAsync(int id);
    Task<AreaStats> AreaStatsAsync();
    // Thẻ (Mst_Tag)
    Task<List<Tag>> TagsAsync(bool? active, string? q);
    Task<Tag?> TagGetAsync(int id);
    Task<int> TagSaveAsync(Tag model);
    Task TagToggleAsync(int id);
    Task<TagStats> TagStatsAsync();
    // Người nhận thông báo phiếu (Mst_EstablishReceiveNotifyETicket)
    Task<List<ReceiveNotify>> ReceiveNotifiesAsync(string? q);
    Task<ReceiveNotify?> ReceiveNotifyGetAsync(int id);
    Task<int> ReceiveNotifySaveAsync(ReceiveNotify model);
    Task ReceiveNotifyDeleteAsync(int id);
    Task<ReceiveNotifyStats> ReceiveNotifyStatsAsync();
    // Danh mục địa chỉ (Mst_Province/Mst_District/Mst_Ward)
    Task<List<Address>> AddressesAsync(AddressLevel? level, bool? active, string? parentCode, string? q);
    Task<Address?> AddressGetAsync(int id);
    Task<int> AddressSaveAsync(Address model);
    Task AddressToggleAsync(int id);
    Task<AddressStats> AddressStatsAsync();
    // Lịch làm việc SLA (Mst_SLAWorkingDay / Mst_SLAHoliday)
    Task<List<SlaPolicy>> SlaCalendarsAsync(string? q);
    Task<SlaPolicy?> SlaCalendarGetAsync(int id);
    Task<List<SlaWorkingDay>> SlaWorkingDaysAsync(int slaPolicyId);
    Task<List<SlaHoliday>> SlaHolidaysAsync(int slaPolicyId);
    Task<int> SlaCalendarSaveAsync(SlaPolicy model, List<SlaWorkingDay> workingDays, List<SlaHoliday> holidays);
    Task<SlaCalendarStats> SlaCalendarStatsAsync();
    Task<DateTime?> SlaCalcDeadlineAsync(int slaPolicyId, DateTime reception, bool firstResponse);
}

public record SlaStats(int Policies, int TicketsWithSla, int ViolatingFirstRes, int ViolatingResolution);

public record RatingStats(int Total, int Rated, int Reviewed, int Satisfied, int Unsatisfied, double AvgScore);

public record SurveyStats(int Total, int Active, int Used, int Fields);

public record SvImprvStats(int Total, int Active, int Used, int Criteria);

public record CustomerStats(int Total, int Active, int Business, int Individual, int Contacts);

public record AllocateStats(int Total, int Active, int AssignAgent, int AllMissedCall, int Agents);

public record ReminderStats(int Total, int Active, int System, int Email, int Sms, int Zalo);

public record TicketCatalogStats(int Total, int Active, int Status, int Priority, int Source, int ReceptionChannel);

public record DepartmentStats(int Total, int Active, int Root, int AutoDiv, int Members);

public record PaymentTermStats(int Total, int Active, int Sale, int Purchase, int WithCredit);

public record AreaStats(int Total, int Active, int Root, int Child, int MaxLevel);

public record TagStats(int Total, int Active, int WithSlug, int Inactive);

public record ReceiveNotifyStats(int Total, int WithName, int WithRemark, int DistinctAgents);

public record AddressStats(int Total, int Active, int Provinces, int Districts, int Wards);

public record SlaCalendarStats(int Policies, int WithWorkingDays, int WithHolidays, int WorkingDayRows, int HolidayRows);

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

    // ── Trung tâm khách hàng (Mst_Customer) ──────────────────────────
    public async Task<List<Customer>> CustomersAsync(bool? active, CustomerType? type, string? groupCode, string? q)
    {
        var query = db.Customers.Include(c => c.Contacts).AsQueryable();
        if (active.HasValue) query = query.Where(c => c.IsActive == active.Value);
        if (type.HasValue) query = query.Where(c => c.Type == type.Value);
        if (!string.IsNullOrWhiteSpace(groupCode)) query = query.Where(c => c.GroupCode == groupCode);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c => c.Name.Contains(q) || c.Code.Contains(q) || (c.Phone ?? "").Contains(q) || (c.Email ?? "").Contains(q));
        var list = await query.ToListAsync();
        return list.OrderByDescending(c => c.UpdatedAt).ToList();
    }

    public Task<Customer?> CustomerGetAsync(int id) =>
        db.Customers.Include(c => c.Contacts).Include(c => c.Histories)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<int> CustomerSaveAsync(Customer model, List<CustomerContact> contacts)
    {
        if (model.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(model.Code)) model.Code = "KH" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            model.CreatedAt = model.UpdatedAt = DateTime.Now;
            foreach (var c in contacts) model.Contacts.Add(c);
            model.Histories.Add(new CustomerHistory { Action = "Tạo mới", Detail = "Khởi tạo hồ sơ khách hàng.", ChangedBy = model.CreatedBy, ChangedAt = DateTime.Now });
            db.Customers.Add(model);
            await db.SaveChangesAsync();
            return model.Id;
        }
        var e = await db.Customers.Include(x => x.Contacts).FirstOrDefaultAsync(x => x.Id == model.Id)
            ?? throw new KeyNotFoundException();
        e.Name = model.Name; e.NameEN = model.NameEN; e.Type = model.Type; e.Partner = model.Partner;
        e.TaxCode = model.TaxCode; e.GroupCode = model.GroupCode; e.Phone = model.Phone; e.Email = model.Email;
        e.Address = model.Address; e.Province = model.Province; e.District = model.District;
        e.CodeInvoice = model.CodeInvoice; e.IsActive = model.IsActive; e.Remark = model.Remark;
        e.UpdatedAt = DateTime.Now;
        // Thay toàn bộ danh sách người liên hệ (hồ sơ là cấu hình, không giữ lịch sử liên hệ).
        db.CustomerContacts.RemoveRange(e.Contacts);
        e.Contacts.Clear();
        foreach (var c in contacts) e.Contacts.Add(c);
        db.CustomerHistories.Add(new CustomerHistory { CustomerId = e.Id, Action = "Cập nhật", Detail = "Cập nhật hồ sơ khách hàng.", ChangedBy = model.CreatedBy, ChangedAt = DateTime.Now });
        await db.SaveChangesAsync();
        return e.Id;
    }

    public async Task CustomerToggleAsync(int id)
    {
        var c = await db.Customers.FirstOrDefaultAsync(x => x.Id == id) ?? throw new KeyNotFoundException();
        c.IsActive = !c.IsActive;
        c.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task<CustomerStats> CustomerStatsAsync()
    {
        var list = await db.Customers.Include(c => c.Contacts).ToListAsync();
        return new CustomerStats(
            list.Count,
            list.Count(c => c.IsActive),
            list.Count(c => c.Type == CustomerType.Business),
            list.Count(c => c.Type == CustomerType.Individual),
            list.Sum(c => c.Contacts.Count));
    }

    public async Task<List<CustomerGroup>> CustomerGroupsAsync()
    {
        var list = await db.CustomerGroups.Include(g => g.Customers).ToListAsync();
        return list.OrderBy(g => g.Name).ToList();
    }

    // ── Thiết lập phân bổ phiếu tự động (Mst_EstablishAllocateETicket) ──
    public async Task<List<AllocateRule>> AllocateRulesAsync(bool? active, string? q)
    {
        var query = db.AllocateRules.Include(r => r.Agents).AsQueryable();
        if (active.HasValue) query = query.Where(r => r.IsActive == active.Value);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(r => r.DepartmentCode.Contains(q) || (r.Remark ?? "").Contains(q));
        var list = await query.ToListAsync();
        return list.OrderByDescending(r => r.UpdatedAt).ToList();
    }

    public Task<AllocateRule?> AllocateRuleGetAsync(int id) =>
        db.AllocateRules.Include(r => r.Agents).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<int> AllocateRuleSaveAsync(AllocateRule model, List<AllocateAgent> agents)
    {
        if (model.Id == 0)
        {
            model.CreatedAt = model.UpdatedAt = DateTime.Now;
            foreach (var a in agents) model.Agents.Add(a);
            db.AllocateRules.Add(model);
            await db.SaveChangesAsync();
            return model.Id;
        }
        var e = await db.AllocateRules.Include(x => x.Agents).FirstOrDefaultAsync(x => x.Id == model.Id)
            ?? throw new KeyNotFoundException();
        e.DepartmentCode = model.DepartmentCode; e.AllocateEven = model.AllocateEven;
        e.AssignAgent = model.AssignAgent; e.AllMissedCall = model.AllMissedCall;
        e.IsActive = model.IsActive; e.Remark = model.Remark; e.UpdatedAt = DateTime.Now;
        // Thay toàn bộ danh sách agent nhận phiếu (thiết lập là cấu hình, không giữ lịch sử).
        db.AllocateAgents.RemoveRange(e.Agents);
        e.Agents.Clear();
        foreach (var a in agents) e.Agents.Add(a);
        await db.SaveChangesAsync();
        return e.Id;
    }

    public async Task AllocateRuleToggleAsync(int id)
    {
        var r = await db.AllocateRules.FirstOrDefaultAsync(x => x.Id == id) ?? throw new KeyNotFoundException();
        r.IsActive = !r.IsActive;
        r.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task<AllocateStats> AllocateStatsAsync()
    {
        var list = await db.AllocateRules.Include(r => r.Agents).ToListAsync();
        return new AllocateStats(
            list.Count,
            list.Count(r => r.IsActive),
            list.Count(r => r.AssignAgent),
            list.Count(r => r.AllMissedCall),
            list.Sum(r => r.Agents.Count));
    }

    // ── Thiết lập nhắc nhở phiếu (Mst_EstablishRemindETicket) ────────
    public async Task<List<ReminderRule>> ReminderRulesAsync(bool? active, RemindChannel? channel, string? q)
    {
        var query = db.ReminderRules.AsQueryable();
        if (active.HasValue) query = query.Where(r => r.IsActive == active.Value);
        if (channel.HasValue) query = query.Where(r =>
            (channel.Value == RemindChannel.System && r.NotifySystem) ||
            (channel.Value == RemindChannel.Email && r.NotifyEmail) ||
            (channel.Value == RemindChannel.Sms && r.NotifySms) ||
            (channel.Value == RemindChannel.Zalo && r.NotifyZalo));
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(r => r.EstablishId.Contains(q) || (r.Remark ?? "").Contains(q));
        var list = await query.ToListAsync();
        return list.OrderByDescending(r => r.UpdatedAt).ToList();
    }

    public Task<ReminderRule?> ReminderRuleGetAsync(int id) =>
        db.ReminderRules.FirstOrDefaultAsync(r => r.Id == id);

    public async Task<int> ReminderRuleSaveAsync(ReminderRule model)
    {
        if (model.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(model.EstablishId)) model.EstablishId = "RM-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            model.CreatedAt = model.UpdatedAt = DateTime.Now;
            db.ReminderRules.Add(model);
            await db.SaveChangesAsync();
            return model.Id;
        }
        var e = await db.ReminderRules.FirstOrDefaultAsync(x => x.Id == model.Id) ?? throw new KeyNotFoundException();
        e.EstablishId = model.EstablishId;
        e.NotifySystem = model.NotifySystem; e.NotifyEmail = model.NotifyEmail;
        e.NotifySms = model.NotifySms; e.NotifyZalo = model.NotifyZalo;
        e.SubFormCodeEmail = model.SubFormCodeEmail; e.SubFormCodeSms = model.SubFormCodeSms;
        e.SubFormCodeZalo = model.SubFormCodeZalo;
        e.IsActive = model.IsActive; e.Remark = model.Remark; e.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return e.Id;
    }

    public async Task ReminderRuleToggleAsync(int id)
    {
        var r = await db.ReminderRules.FirstOrDefaultAsync(x => x.Id == id) ?? throw new KeyNotFoundException();
        r.IsActive = !r.IsActive;
        r.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task<ReminderStats> ReminderStatsAsync()
    {
        var list = await db.ReminderRules.ToListAsync();
        return new ReminderStats(
            list.Count,
            list.Count(r => r.IsActive),
            list.Count(r => r.NotifySystem),
            list.Count(r => r.NotifyEmail),
            list.Count(r => r.NotifySms),
            list.Count(r => r.NotifyZalo));
    }

    // ── Danh mục phiếu (Mst_TicketStatus/TicketPriority/TicketSource/ReceptionChannel) ──
    public async Task<List<TicketCatalog>> TicketCatalogsAsync(TicketCatalogKind? kind, bool? active, string? q)
    {
        var query = db.TicketCatalogs.AsQueryable();
        if (kind.HasValue) query = query.Where(c => c.Kind == kind.Value);
        if (active.HasValue) query = query.Where(c => c.IsActive == active.Value);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c => c.Code.Contains(q) || c.AgentName.Contains(q) || c.CustomerName.Contains(q));
        var list = await query.ToListAsync();
        return list.OrderBy(c => c.Kind).ThenBy(c => c.Code).ToList();
    }

    public Task<TicketCatalog?> TicketCatalogGetAsync(int id) =>
        db.TicketCatalogs.FirstOrDefaultAsync(c => c.Id == id);

    public async Task<int> TicketCatalogSaveAsync(TicketCatalog model)
    {
        if (model.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(model.Code)) model.Code = "CAT-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            model.CreatedAt = model.UpdatedAt = DateTime.Now;
            db.TicketCatalogs.Add(model);
            await db.SaveChangesAsync();
            return model.Id;
        }
        var e = await db.TicketCatalogs.FirstOrDefaultAsync(x => x.Id == model.Id) ?? throw new KeyNotFoundException();
        e.Kind = model.Kind; e.Code = model.Code; e.AgentName = model.AgentName; e.CustomerName = model.CustomerName;
        e.UseType = model.UseType; e.IsActive = model.IsActive; e.Remark = model.Remark; e.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return e.Id;
    }

    public async Task TicketCatalogToggleAsync(int id)
    {
        var c = await db.TicketCatalogs.FirstOrDefaultAsync(x => x.Id == id) ?? throw new KeyNotFoundException();
        c.IsActive = !c.IsActive;
        c.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task<TicketCatalogStats> TicketCatalogStatsAsync()
    {
        var list = await db.TicketCatalogs.ToListAsync();
        return new TicketCatalogStats(
            list.Count,
            list.Count(c => c.IsActive),
            list.Count(c => c.Kind == TicketCatalogKind.Status),
            list.Count(c => c.Kind == TicketCatalogKind.Priority),
            list.Count(c => c.Kind == TicketCatalogKind.Source),
            list.Count(c => c.Kind == TicketCatalogKind.ReceptionChannel));
    }

    // ── Phòng ban (Mst_Department) ───────────────────────────────────
    public async Task<List<Department>> DepartmentsAsync(bool? active, string? q)
    {
        var query = db.Departments.Include(d => d.Members).AsQueryable();
        if (active.HasValue) query = query.Where(d => d.IsActive == active.Value);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(d => d.Code.Contains(q) || d.Name.Contains(q) || (d.Description ?? "").Contains(q));
        var list = await query.ToListAsync();
        return list.OrderBy(d => d.Level).ThenBy(d => d.Order).ThenBy(d => d.Code).ToList();
    }

    public Task<Department?> DepartmentGetAsync(int id) =>
        db.Departments.Include(d => d.Members).FirstOrDefaultAsync(d => d.Id == id);

    public async Task<int> DepartmentSaveAsync(Department model, List<DepartmentMember> members)
    {
        if (model.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(model.Code)) model.Code = "PB-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            model.CreatedAt = model.UpdatedAt = DateTime.Now;
            foreach (var m in members) model.Members.Add(m);
            db.Departments.Add(model);
            await db.SaveChangesAsync();
            return model.Id;
        }
        var e = await db.Departments.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == model.Id)
            ?? throw new KeyNotFoundException();
        e.Code = model.Code; e.ParentCode = model.ParentCode; e.Name = model.Name;
        e.Description = model.Description; e.Level = model.Level; e.TaxCode = model.TaxCode;
        e.AutoDiv = model.AutoDiv; e.IsActive = model.IsActive; e.Order = model.Order;
        e.UpdatedAt = DateTime.Now;
        // Thay toàn bộ danh sách thành viên (phòng ban là cấu hình, không giữ lịch sử thành viên).
        db.DepartmentMembers.RemoveRange(e.Members);
        e.Members.Clear();
        foreach (var m in members) e.Members.Add(m);
        await db.SaveChangesAsync();
        return e.Id;
    }

    public async Task DepartmentToggleAsync(int id)
    {
        var d = await db.Departments.FirstOrDefaultAsync(x => x.Id == id) ?? throw new KeyNotFoundException();
        d.IsActive = !d.IsActive;
        d.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task<DepartmentStats> DepartmentStatsAsync()
    {
        var list = await db.Departments.Include(d => d.Members).ToListAsync();
        return new DepartmentStats(
            list.Count,
            list.Count(d => d.IsActive),
            list.Count(d => d.IsRoot),
            list.Count(d => d.AutoDiv),
            list.Sum(d => d.Members.Count));
    }

    // ── Điều khoản thanh toán (Mst_PaymentTerm) ──────────────────────
    public async Task<List<PaymentTerm>> PaymentTermsAsync(bool? active, PTType? type, string? q)
    {
        var query = db.PaymentTerms.AsQueryable();
        if (active.HasValue) query = query.Where(p => p.IsActive == active.Value);
        if (type.HasValue) query = query.Where(p => p.Type == type.Value);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Code.Contains(q) || p.Name.Contains(q) || (p.Description ?? "").Contains(q));
        var list = await query.ToListAsync();
        return list.OrderBy(p => p.Type).ThenBy(p => p.Code).ToList();
    }

    public Task<PaymentTerm?> PaymentTermGetAsync(int id) =>
        db.PaymentTerms.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<int> PaymentTermSaveAsync(PaymentTerm model)
    {
        if (model.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(model.Code)) model.Code = "PT-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            model.CreatedAt = model.UpdatedAt = DateTime.Now;
            db.PaymentTerms.Add(model);
            await db.SaveChangesAsync();
            return model.Id;
        }
        var e = await db.PaymentTerms.FirstOrDefaultAsync(x => x.Id == model.Id) ?? throw new KeyNotFoundException();
        e.Code = model.Code; e.Name = model.Name; e.Type = model.Type; e.Description = model.Description;
        e.OwedDay = model.OwedDay; e.CreditLimit = model.CreditLimit; e.DepositPercent = model.DepositPercent;
        e.IsActive = model.IsActive; e.Remark = model.Remark; e.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return e.Id;
    }

    public async Task PaymentTermToggleAsync(int id)
    {
        var p = await db.PaymentTerms.FirstOrDefaultAsync(x => x.Id == id) ?? throw new KeyNotFoundException();
        p.IsActive = !p.IsActive;
        p.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task<PaymentTermStats> PaymentTermStatsAsync()
    {
        var list = await db.PaymentTerms.ToListAsync();
        return new PaymentTermStats(
            list.Count,
            list.Count(p => p.IsActive),
            list.Count(p => p.Type == PTType.Sale),
            list.Count(p => p.Type == PTType.Purchase),
            list.Count(p => p.CreditLimit > 0));
    }

    // ── Vùng thị trường (Mst_Area) ───────────────────
    public async Task<List<Area>> AreasAsync(bool? active, string? q)
    {
        var query = db.Areas.AsQueryable();
        if (active.HasValue) query = query.Where(a => a.IsActive == active.Value);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(a => a.Code.Contains(q) || a.Name.Contains(q) || (a.Description ?? "").Contains(q));
        var list = await query.ToListAsync();
        return list.OrderBy(a => a.Level).ThenBy(a => a.Code).ToList();
    }

    public Task<Area?> AreaGetAsync(int id) =>
        db.Areas.FirstOrDefaultAsync(a => a.Id == id);

    public async Task<int> AreaSaveAsync(Area model)
    {
        if (model.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(model.Code)) model.Code = "AR-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            model.CreatedAt = model.UpdatedAt = DateTime.Now;
            db.Areas.Add(model);
            await db.SaveChangesAsync();
            return model.Id;
        }
        var e = await db.Areas.FirstOrDefaultAsync(x => x.Id == model.Id) ?? throw new KeyNotFoundException();
        e.Code = model.Code; e.ParentCode = model.ParentCode; e.Name = model.Name;
        e.Description = model.Description; e.Level = model.Level;
        e.BUCode = model.BUCode; e.BUPattern = model.BUPattern;
        e.IsActive = model.IsActive; e.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return e.Id;
    }

    public async Task AreaToggleAsync(int id)
    {
        var a = await db.Areas.FirstOrDefaultAsync(x => x.Id == id) ?? throw new KeyNotFoundException();
        a.IsActive = !a.IsActive;
        a.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task<AreaStats> AreaStatsAsync()
    {
        var list = await db.Areas.ToListAsync();
        return new AreaStats(
            list.Count,
            list.Count(a => a.IsActive),
            list.Count(a => a.IsRoot),
            list.Count(a => !a.IsRoot),
            list.Count == 0 ? 0 : list.Max(a => a.Level));
    }

    // ── Thẻ (Mst_Tag) ────────────────────────────────────────────────
    public async Task<List<Tag>> TagsAsync(bool? active, string? q)
    {
        var query = db.Tags.AsQueryable();
        if (active.HasValue) query = query.Where(t => t.IsActive == active.Value);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(t => t.Code.Contains(q) || t.Name.Contains(q) || (t.Description ?? "").Contains(q) || (t.Slug ?? "").Contains(q));
        var list = await query.ToListAsync();
        return list.OrderBy(t => t.Name).ThenBy(t => t.Code).ToList();
    }

    public Task<Tag?> TagGetAsync(int id) =>
        db.Tags.FirstOrDefaultAsync(t => t.Id == id);

    public async Task<int> TagSaveAsync(Tag model)
    {
        if (model.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(model.Code)) model.Code = "TAG" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(model.Slug)) model.Slug = Tag.MakeSlug(model.Name);
            model.CreatedAt = model.UpdatedAt = DateTime.Now;
            db.Tags.Add(model);
            await db.SaveChangesAsync();
            return model.Id;
        }
        var e = await db.Tags.FirstOrDefaultAsync(x => x.Id == model.Id) ?? throw new KeyNotFoundException();
        e.Code = model.Code; e.Name = model.Name; e.Description = model.Description;
        e.Slug = string.IsNullOrWhiteSpace(model.Slug) ? Tag.MakeSlug(model.Name) : model.Slug;
        e.IsActive = model.IsActive; e.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return e.Id;
    }

    public async Task TagToggleAsync(int id)
    {
        var t = await db.Tags.FirstOrDefaultAsync(x => x.Id == id) ?? throw new KeyNotFoundException();
        t.IsActive = !t.IsActive;
        t.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task<TagStats> TagStatsAsync()
    {
        var list = await db.Tags.ToListAsync();
        return new TagStats(
            list.Count,
            list.Count(t => t.IsActive),
            list.Count(t => !string.IsNullOrWhiteSpace(t.Slug)),
            list.Count(t => !t.IsActive));
    }

    // ── Người nhận thông báo phiếu (Mst_EstablishReceiveNotifyETicket) ──
    public async Task<List<ReceiveNotify>> ReceiveNotifiesAsync(string? q)
    {
        var query = db.ReceiveNotifies.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(r => r.AgentCode.Contains(q) || (r.AgentName ?? "").Contains(q) || (r.Remark ?? "").Contains(q));
        var list = await query.ToListAsync();
        return list.OrderBy(r => r.AgentCode).ToList();
    }

    public Task<ReceiveNotify?> ReceiveNotifyGetAsync(int id) =>
        db.ReceiveNotifies.FirstOrDefaultAsync(r => r.Id == id);

    public async Task<int> ReceiveNotifySaveAsync(ReceiveNotify model)
    {
        if (model.Id == 0)
        {
            model.CreatedAt = model.UpdatedAt = DateTime.Now;
            db.ReceiveNotifies.Add(model);
            await db.SaveChangesAsync();
            return model.Id;
        }
        var e = await db.ReceiveNotifies.FirstOrDefaultAsync(x => x.Id == model.Id) ?? throw new KeyNotFoundException();
        e.AgentCode = model.AgentCode; e.AgentName = model.AgentName; e.Remark = model.Remark;
        e.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return e.Id;
    }

    public async Task ReceiveNotifyDeleteAsync(int id)
    {
        var r = await db.ReceiveNotifies.FirstOrDefaultAsync(x => x.Id == id) ?? throw new KeyNotFoundException();
        db.ReceiveNotifies.Remove(r);
        await db.SaveChangesAsync();
    }

    public async Task<ReceiveNotifyStats> ReceiveNotifyStatsAsync()
    {
        var list = await db.ReceiveNotifies.ToListAsync();
        return new ReceiveNotifyStats(
            list.Count,
            list.Count(r => !string.IsNullOrWhiteSpace(r.AgentName)),
            list.Count(r => !string.IsNullOrWhiteSpace(r.Remark)),
            list.Select(r => r.AgentCode).Distinct().Count());
    }

    // ── Danh mục địa chỉ (Mst_Province/Mst_District/Mst_Ward) ────────
    public async Task<List<Address>> AddressesAsync(AddressLevel? level, bool? active, string? parentCode, string? q)
    {
        var query = db.Addresses.AsQueryable();
        if (level.HasValue) query = query.Where(a => a.Level == level.Value);
        if (active.HasValue) query = query.Where(a => a.IsActive == active.Value);
        if (!string.IsNullOrWhiteSpace(parentCode)) query = query.Where(a => a.ParentCode == parentCode);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(a => a.Code.Contains(q) || a.Name.Contains(q) || (a.ParentCode ?? "").Contains(q));
        var list = await query.ToListAsync();
        return list.OrderBy(a => a.Level).ThenBy(a => a.Code).ToList();
    }

    public Task<Address?> AddressGetAsync(int id) =>
        db.Addresses.FirstOrDefaultAsync(a => a.Id == id);

    public async Task<int> AddressSaveAsync(Address model)
    {
        if (model.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(model.Code)) model.Code = "AD-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            model.CreatedAt = model.UpdatedAt = DateTime.Now;
            db.Addresses.Add(model);
            await db.SaveChangesAsync();
            return model.Id;
        }
        var e = await db.Addresses.FirstOrDefaultAsync(x => x.Id == model.Id) ?? throw new KeyNotFoundException();
        e.Level = model.Level; e.Code = model.Code; e.Name = model.Name; e.ParentCode = model.ParentCode;
        e.PostCode = model.PostCode; e.CountryCode = model.CountryCode;
        e.IsActive = model.IsActive; e.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return e.Id;
    }

    public async Task AddressToggleAsync(int id)
    {
        var a = await db.Addresses.FirstOrDefaultAsync(x => x.Id == id) ?? throw new KeyNotFoundException();
        a.IsActive = !a.IsActive;
        a.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task<AddressStats> AddressStatsAsync()
    {
        var list = await db.Addresses.ToListAsync();
        return new AddressStats(
            list.Count,
            list.Count(a => a.IsActive),
            list.Count(a => a.Level == AddressLevel.Province),
            list.Count(a => a.Level == AddressLevel.District),
            list.Count(a => a.Level == AddressLevel.Ward));
    }

    // ── Lịch làm việc SLA (Mst_SLAWorkingDay / Mst_SLAHoliday) ───────
    public async Task<List<SlaPolicy>> SlaCalendarsAsync(string? q)
    {
        var query = db.SlaPolicies.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Code.Contains(q) || p.Level.Contains(q));
        var list = await query.ToListAsync();
        return list.OrderBy(p => p.FirstResMinutes).ToList();
    }

    public Task<SlaPolicy?> SlaCalendarGetAsync(int id) =>
        db.SlaPolicies.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<List<SlaWorkingDay>> SlaWorkingDaysAsync(int slaPolicyId)
    {
        var list = await db.SlaWorkingDays.Where(x => x.SlaPolicyId == slaPolicyId).ToListAsync();
        return list.OrderBy(x => x.WeekdayCode).ThenBy(x => x.Shift).ToList();
    }

    public async Task<List<SlaHoliday>> SlaHolidaysAsync(int slaPolicyId)
    {
        var list = await db.SlaHolidays.Where(x => x.SlaPolicyId == slaPolicyId).ToListAsync();
        return list.OrderBy(x => x.Holiday).ToList();
    }

    public async Task<int> SlaCalendarSaveAsync(SlaPolicy model, List<SlaWorkingDay> workingDays, List<SlaHoliday> holidays)
    {
        var e = await db.SlaPolicies.FirstOrDefaultAsync(x => x.Id == model.Id) ?? throw new KeyNotFoundException();
        // Thay toàn bộ lịch làm việc + ngày nghỉ (lịch là cấu hình, không giữ lịch sử).
        var oldWd = await db.SlaWorkingDays.Where(x => x.SlaPolicyId == e.Id).ToListAsync();
        var oldHd = await db.SlaHolidays.Where(x => x.SlaPolicyId == e.Id).ToListAsync();
        db.SlaWorkingDays.RemoveRange(oldWd);
        db.SlaHolidays.RemoveRange(oldHd);
        foreach (var w in workingDays)
        {
            w.Id = 0; w.SlaPolicyId = e.Id; w.CreatedAt = DateTime.Now;
            db.SlaWorkingDays.Add(w);
        }
        foreach (var h in holidays)
        {
            h.Id = 0; h.SlaPolicyId = e.Id; h.CreatedAt = DateTime.Now;
            db.SlaHolidays.Add(h);
        }
        await db.SaveChangesAsync();
        return e.Id;
    }

    public async Task<SlaCalendarStats> SlaCalendarStatsAsync()
    {
        var policies = await db.SlaPolicies.ToListAsync();
        var wd = await db.SlaWorkingDays.ToListAsync();
        var hd = await db.SlaHolidays.ToListAsync();
        var wdPolicyIds = wd.Select(x => x.SlaPolicyId).Distinct().ToHashSet();
        var hdPolicyIds = hd.Select(x => x.SlaPolicyId).Distinct().ToHashSet();
        return new SlaCalendarStats(
            policies.Count,
            policies.Count(p => wdPolicyIds.Contains(p.Id)),
            policies.Count(p => hdPolicyIds.Contains(p.Id)),
            wd.Count,
            hd.Count);
    }

    /// <summary>
    /// Tính hạn (deadline) theo lịch làm việc của chính sách SLA — port từ
    /// Mst_SLA_CalcFirstResDTimeX / Mst_SLA_CalcDeadlineX (Master.1.cs).
    /// Cộng dồn số phút cam kết (FirstResMinutes hoặc ResolutionMinutes), chỉ tính
    /// trong các ca làm việc, bỏ qua ngày nghỉ (SLAHoliday "dd-MM") và ngày không có ca.
    /// Không có lịch làm việc → cộng thẳng theo giờ đồng hồ (24/7).
    /// </summary>
    public async Task<DateTime?> SlaCalcDeadlineAsync(int slaPolicyId, DateTime reception, bool firstResponse)
    {
        var sla = await db.SlaPolicies.FirstOrDefaultAsync(p => p.Id == slaPolicyId);
        if (sla == null) return null;
        var minutes = firstResponse ? sla.FirstResMinutes : sla.ResolutionMinutes;

        var wd = await db.SlaWorkingDays.Where(x => x.SlaPolicyId == slaPolicyId).ToListAsync();
        var holidays = await db.SlaHolidays.Where(x => x.SlaPolicyId == slaPolicyId).ToListAsync();
        var holidaySet = holidays.Select(h => h.Holiday.Trim()).ToHashSet();

        // Không có lịch làm việc → cộng thẳng (24/7), chỉ né ngày nghỉ nếu có.
        if (wd.Count == 0)
        {
            var dt = reception;
            for (int guard = 0; guard < 366; guard++)
            {
                if (holidaySet.Contains(dt.ToString("dd-MM"))) { dt = dt.AddDays(1); continue; }
                return dt.AddMinutes(minutes);
            }
            return null;
        }

        // Gộp ca theo thứ: WeekdayCode (1=CN..7=Thứ bảy) → danh sách ca (sáng/chiều).
        var byDay = wd.GroupBy(x => x.WeekdayCode)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Shift).ToList());

        // Quy đổi DayOfWeek (.NET: Sunday=0) sang SLAWorkingDayCode (1=CN..7=Thứ bảy).
        static int Code(DayOfWeek d) => d == DayOfWeek.Sunday ? 1 : (int)d + 1;

        // Bước 1: cộng phần thời gian đã trôi qua trong ngày nhận (nếu đang trong ca).
        var day = reception.Date;
        var dayShifts = byDay.TryGetValue(Code(reception.DayOfWeek), out var s0) ? s0 : new List<SlaWorkingDay>();
        int remaining = minutes;
        foreach (var sh in dayShifts)
        {
            var start = day.AddMinutes(sh.FromMinutes);
            var end = day.AddMinutes(sh.ToMinutes);
            if (reception >= end) remaining += sh.Minutes;                 // đã qua cả ca
            else if (reception >= start) remaining += (int)(reception - start).TotalMinutes; // đang trong ca
        }

        // Bước 2: dò từng ngày, trừ dần số phút còn lại theo ca làm việc.
        for (int guard = 0; guard < 366 && remaining > 0; guard++)
        {
            if (holidaySet.Contains(day.ToString("dd-MM"))) { day = day.AddDays(1); continue; }
            if (!byDay.TryGetValue(Code(day.DayOfWeek), out var shifts) || shifts.Count == 0)
            {
                day = day.AddDays(1);
                continue;
            }
            foreach (var sh in shifts)
            {
                if (remaining <= 0) break;
                if (remaining > sh.Minutes)
                {
                    remaining -= sh.Minutes;
                }
                else
                {
                    return day.AddMinutes(sh.FromMinutes + remaining);
                }
            }
            day = day.AddDays(1);
        }
        return null;
    }
}
