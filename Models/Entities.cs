namespace MiniCSKH.Models;

public enum TicketStatus { New = 0, InProgress = 1, WaitingCustomer = 2, Resolved = 3, Closed = 4, Cancelled = 5 }
public enum TicketPriority { Low = 0, Normal = 1, High = 2, Urgent = 3 }
public enum Channel { Web = 0, Email = 1, Zalo = 2, Phone = 3, Facebook = 4 }

/// <summary>Nhân viên hỗ trợ (agent).</summary>
public class Agent : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

/// <summary>Nhóm/loại phiếu — gắn SLA (giờ) mặc định.</summary>
public class TicketCategory : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Name { get; set; } = "";
    public int SlaHours { get; set; } = 24;
}

// ── Phân loại nghiệp vụ (Mst_TicketType) ─────────────────────────────
// Theo SkyCS: mỗi "phân loại nghiệp vụ" (TicketType) là một mã nghiệp vụ
// dùng để phân loại eTicket, gắn mẫu bố cục màn hình tạo/chi tiết và loại
// nghiệp vụ (ETICKET/CAMPAIGN). Có tên riêng cho agent và cho khách hàng.

/// <summary>Loại nghiệp vụ áp dụng cho phân loại (Mst_TicketType.BusinessType).</summary>
public enum BusinessType { ETicket = 0, Campaign = 1 }

/// <summary>Phân loại nghiệp vụ eTicket (Mst_TicketType).</summary>
public class TicketType : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";              // TicketType — mã phân loại nghiệp vụ
    public string AgentName { get; set; } = "";          // AgentTicketTypeName — tên cho agent
    public string CustomerName { get; set; } = "";       // CustomerTicketTypeName — tên cho khách
    public string? CreateTemplate { get; set; }          // ScrTplCreateCodeSys — mẫu bố cục tạo
    public string? DetailTemplate { get; set; }          // ScrTplDetailCodeSys — mẫu bố cục chi tiết
    public string? HoCode { get; set; }                  // TicketTypeHO — mã phân loại HO
    public BusinessType BusinessType { get; set; } = BusinessType.ETicket; // BusinessType
    public bool IsActive { get; set; } = true;           // FlagActive
    public int Order { get; set; }                       // Idx — thứ tự hiển thị
    public string? Remark { get; set; }                  // Remark
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";          // LogLUBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public string BusinessTypeName => BusinessType == BusinessType.Campaign ? "Chiến dịch" : "eTicket";
}

/// <summary>
/// Chính sách SLA (Mst_SLA bên SkyCS): mức cam kết theo 2 mốc thời gian —
/// phản hồi đầu tiên (FirstResMinutes) và xử lý xong (ResolutionMinutes).
/// </summary>
public class SlaPolicy : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";          // SLAID
    public string Level { get; set; } = "";         // SLALevel — tên mức SLA
    public string? Description { get; set; }         // SLADesc
    public int FirstResMinutes { get; set; } = 60;   // FirstResTime (phút)
    public int ResolutionMinutes { get; set; } = 480; // ResolutionTime (phút)
    public bool IsActive { get; set; } = true;       // SLAStatus
    // ── cờ "áp dụng cho TẤT CẢ" (Mst_SLA.FlagAll*) ───────────────────
    public bool AllTicketType { get; set; }          // FlagAllTicketType — mọi loại phiếu
    public bool AllTicketCustomType { get; set; }    // FlagAllTicketCustomType — mọi loại tùy chỉnh
    public bool AllCustomerCN { get; set; }          // FlagAllCustomerCN — mọi khách cá nhân
    public bool AllCustomerGroupCN { get; set; }     // FlagAllCustomerGrpCN — mọi nhóm khách cá nhân
    public bool AllCustomerDN { get; set; }          // FlagAllCustomerDN — mọi khách doanh nghiệp
    public bool AllCustomerGroupDN { get; set; }     // FlagAllCustomerGrpDN — mọi nhóm khách doanh nghiệp
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<SlaScope> Scopes { get; set; } = [];

    public string FirstResText => FormatMinutes(FirstResMinutes);
    public string ResolutionText => FormatMinutes(ResolutionMinutes);

    // ── tính toán ────────────────────────────────────────────────────
    /// <summary>Số đối tượng áp dụng chi tiết (không tính cờ "tất cả").</summary>
    public int ScopeCount => Scopes.Count;
    /// <summary>Số nhóm đối tượng đang bật cờ "áp dụng cho tất cả".</summary>
    public int AllFlagCount => (AllTicketType ? 1 : 0) + (AllTicketCustomType ? 1 : 0)
        + (AllCustomerCN ? 1 : 0) + (AllCustomerGroupCN ? 1 : 0)
        + (AllCustomerDN ? 1 : 0) + (AllCustomerGroupDN ? 1 : 0);
    /// <summary>Mô tả ngắn phạm vi áp dụng (dùng cho view).</summary>
    public string ScopeText => AllFlagCount == 6 ? "Áp dụng cho tất cả"
        : AllFlagCount > 0 ? $"{AllFlagCount} nhóm \"tất cả\" + {ScopeCount} đối tượng"
        : ScopeCount > 0 ? $"{ScopeCount} đối tượng cụ thể"
        : "Chưa gán đối tượng";

    public static string FormatMinutes(int m) => m <= 0 ? "—" : m < 60 ? $"{m} phút" : m % 60 == 0 ? $"{m / 60} giờ" : $"{m / 60}g{m % 60}p";
}

/// <summary>Phiếu hỗ trợ (eTicket).</summary>
public class Ticket : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";
    public string Subject { get; set; } = "";
    public string? Description { get; set; }

    public string CustomerName { get; set; } = "";
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }

    public Channel Channel { get; set; } = Channel.Web;
    public TicketPriority Priority { get; set; } = TicketPriority.Normal;
    public TicketStatus Status { get; set; } = TicketStatus.New;

    public int? CategoryId { get; set; }
    public int? AssignedAgentId { get; set; }
    public int? SlaPolicyId { get; set; }         // chính sách SLA áp dụng
    public bool FlagRated { get; set; }           // cờ đã đánh giá (ET_Ticket.FlagRated)
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? DueAt { get; set; }          // hạn SLA
    public DateTime? FirstResponseAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public TicketCategory? Category { get; set; }
    public Agent? AssignedAgent { get; set; }
    public SlaPolicy? SlaPolicy { get; set; }
    public List<TicketComment> Comments { get; set; } = [];
    public List<TicketRating> Ratings { get; set; } = [];

    // ── tính toán ────────────────────────────────────────────────────
    public bool IsOpen => Status is not (TicketStatus.Resolved or TicketStatus.Closed or TicketStatus.Cancelled);
    public bool IsOverdue => IsOpen && DueAt.HasValue && DateTime.Now > DueAt.Value;

    // ── SLA: thời gian thực tế (phút) so với cam kết ─────────────────
    /// <summary>Thời gian phản hồi đầu tiên thực tế (phút). Chưa phản hồi → tính tới hiện tại.</summary>
    public int ActualFirstResMinutes => (int)Math.Max(0, ((FirstResponseAt ?? DateTime.Now) - CreatedAt).TotalMinutes);
    /// <summary>Thời gian xử lý thực tế (phút). Chưa xong → tính tới hiện tại.</summary>
    public int ActualResolutionMinutes => (int)Math.Max(0, ((ResolvedAt ?? ClosedAt ?? DateTime.Now) - CreatedAt).TotalMinutes);
    /// <summary>Vi phạm SLA phản hồi đầu tiên (chỉ xét khi đã có cam kết).</summary>
    public bool ViolatesFirstResponse => SlaPolicy != null && ActualFirstResMinutes > SlaPolicy.FirstResMinutes;
    /// <summary>Vi phạm SLA thời gian xử lý.</summary>
    public bool ViolatesResolution => SlaPolicy != null && ActualResolutionMinutes > SlaPolicy.ResolutionMinutes;
    public bool ViolatesSla => ViolatesFirstResponse || ViolatesResolution;
}

/// <summary>Dòng trao đổi trên phiếu (timeline). IsInternal = ghi chú nội bộ, KH không thấy.</summary>
public class TicketComment : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int TicketId { get; set; }
    public string Author { get; set; } = "";
    public string Body { get; set; } = "";
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Ticket Ticket { get; set; } = null!;
}

public enum CallDirection { Inbound = 0, Outbound = 1 }
public enum CallOutcome { Answered = 0, Missed = 1, Voicemail = 2, Busy = 3 }

/// <summary>Nhật ký cuộc gọi (call center) — có thể gắn với 1 phiếu.</summary>
public class CallLog : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public CallDirection Direction { get; set; }
    public string PhoneNumber { get; set; } = "";
    public string? CustomerName { get; set; }
    public int? AgentId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.Now;
    public int DurationSeconds { get; set; }
    public CallOutcome Outcome { get; set; } = CallOutcome.Answered;
    public string? Note { get; set; }
    public int? TicketId { get; set; }

    public Agent? Agent { get; set; }
    public Ticket? Ticket { get; set; }

    public string DurationText => DurationSeconds <= 0 ? "—" : $"{DurationSeconds / 60}:{DurationSeconds % 60:D2}";
}

/// <summary>Bài viết Knowledge Base.</summary>
public class KbArticle : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Title { get; set; } = "";
    public string Category { get; set; } = "Chung";
    public string Body { get; set; } = "";
    public int Views { get; set; }
    public bool IsPublished { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

// ── Chiến dịch (Campaign / Cpn) ──────────────────────
// Vòng đời (theo SkyCS Cpn_Campaign_*): Pending → Approve → Started ⇄ Paused → Finish.
public enum CampaignStatus { Pending = 0, Approved = 1, Started = 2, Paused = 3, Finished = 4, Cancelled = 5 }

/// <summary>Kết quả gọi từng khách trong chiến dịch (SkyCS CampaignCustomerCallStatus).</summary>
public enum CampaignCustomerStatus { Pending = 0, Done = 1, Failed = 2, NoAnswer = 3, CallAgain = 4, DoNotCall = 5 }

/// <summary>Chiến dịch gọi ra (outbound campaign) — gom danh sách khách để agent gọi.</summary>
public class Campaign : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string CampaignType { get; set; } = "Telesales";   // loại chiến dịch (Mst_CampaignType)
    public CampaignStatus Status { get; set; } = CampaignStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? StartAt { get; set; }
    public DateTime? FinishAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public List<CampaignCustomer> Customers { get; set; } = [];

    // ── tính toán ────────────────────
    public int TotalCustomers => Customers.Count;
    public int DoneCustomers => Customers.Count(c => c.Status == CampaignCustomerStatus.Done);
    public int ProgressPercent => TotalCustomers == 0 ? 0 : (int)Math.Round(DoneCustomers * 100.0 / TotalCustomers);
    public bool IsRunning => Status == CampaignStatus.Started;
}

/// <summary>Khách hàng trong 1 chiến dịch + kết quả gọi (agent phụ trách).</summary>
public class CampaignCustomer : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int CampaignId { get; set; }
    public string CustomerName { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string? Company { get; set; }
    public int? AgentId { get; set; }
    public CampaignCustomerStatus Status { get; set; } = CampaignCustomerStatus.Pending;
    public string? Feedback { get; set; }        // phản hồi của khách
    public string? Remark { get; set; }          // ghi chú của agent
    public DateTime? LastCallAt { get; set; }
    public int CallCount { get; set; }

    public Campaign Campaign { get; set; } = null!;
    public Agent? Agent { get; set; }
}

// ── Đánh giá phiếu (eTicket Rating / ET_TicketRated) ─────────────────
// Khách đánh giá phiếu đã xử lý (RATE); agent kiểm soát lại đánh giá (REVIEW).
// Theo SkyCS: ET_TicketRated (RateType RATE/REVIEW, RatedJsonInfo JSON chứa RATESTATUS/RATERESULT)
// và cờ ET_Ticket.FlagRated (0/null = chưa đánh giá, 1 = đã đánh giá).

/// <summary>Loại bản ghi đánh giá (SkyCS RateType).</summary>
public enum RateType { Rate = 0, Review = 1 }

/// <summary>Trạng thái đánh giá (RATESTATUS).</summary>
public enum RateStatus { None = 0, Rated = 1, Reviewed = 2 }

/// <summary>Kết quả đánh giá (RATERESULT).</summary>
public enum RateResult { Satisfied = 0, Neutral = 1, Unsatisfied = 2 }

/// <summary>Một lần đánh giá phiếu (ET_TicketRated). Mỗi phiếu lưu lịch sử nhiều lần đánh giá.</summary>
public class TicketRating : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int TicketId { get; set; }
    public string FormCode { get; set; } = "SAT-STD";   // HstIdx — mã mẫu đánh giá
    public int RateRound { get; set; } = 1;             // HstRate — lần đánh giá
    public RateType RateType { get; set; } = RateType.Rate;
    public RateStatus Status { get; set; } = RateStatus.Rated;   // RATESTATUS
    public RateResult Result { get; set; } = RateResult.Satisfied; // RATERESULT
    public int Score { get; set; } = 5;                 // điểm 1..5 (sao)
    public string? Comment { get; set; }                // nội dung đánh giá
    public string RatedBy { get; set; } = "";           // người đánh giá (RATEUSER)
    public string? ReviewedBy { get; set; }             // agent kiểm soát (REVIEW)
    public string? ReviewNote { get; set; }             // ghi chú kiểm soát
    public DateTime RatedAt { get; set; } = DateTime.Now;

    public Ticket Ticket { get; set; } = null!;

    public string Stars => new string('★', Math.Clamp(Score, 0, 5)) + new string('☆', 5 - Math.Clamp(Score, 0, 5));
}

// ── Mẫu khảo sát hài lòng (St_SurveyForm / St_SurveyFormDetail) ──────
// Theo SkyCS: mẫu khảo sát (St_SurveyForm) gồm nhiều trường cấu hình
// (St_SurveyFormDetail → Mst_CampaignColumnConfig). Mẫu được dùng để
// đánh giá eTicket sau khi đóng phiếu (ET_TicketRated.HstIdx = FrmSurveyCode).

/// <summary>Kiểu dữ liệu của trường khảo sát (CampaignColCfgDataType).</summary>
public enum SurveyFieldType { Text = 0, Number = 1, Rating = 2, SingleChoice = 3, MultiChoice = 4, Date = 5 }

/// <summary>Mẫu khảo sát hài lòng (St_SurveyForm).</summary>
public class SurveyForm : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";            // FrmSurveyCode
    public string Name { get; set; } = "";            // FrmSurveyName
    public string? Description { get; set; }           // FrmSurveyDesc
    public string? Remark { get; set; }                // Remark
    public bool IsActive { get; set; } = true;         // FlagActive
    public DateTime? UsedAt { get; set; }              // DTimeUsed — thời điểm mẫu được dùng
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // CreateDTimeUTC
    public string CreatedBy { get; set; } = "";        // CreateBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;   // LUDTimeUTC

    public List<SurveyFormField> Fields { get; set; } = [];

    // ── tính toán ────────────────────────────────────────────────────
    public int FieldCount => Fields.Count;
    public int RequiredCount => Fields.Count(f => f.IsRequired);
    public bool IsUsed => UsedAt.HasValue;
}

/// <summary>Một trường (câu hỏi) trong mẫu khảo sát (St_SurveyFormDetail).</summary>
public class SurveyFormField : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int SurveyFormId { get; set; }
    public string Code { get; set; } = "";            // CampaignColCfgCodeSys
    public string Name { get; set; } = "";            // CampaignColCfgName
    public SurveyFieldType FieldType { get; set; } = SurveyFieldType.Text;  // CampaignColCfgDataType
    public int Order { get; set; }                     // Idx — số thứ tự hiển thị
    public int Width { get; set; } = 12;               // ColWidth (theo cột lưới 12)
    public bool IsRequired { get; set; }               // FlagRequired
    public string? Options { get; set; }               // JsonListOption — danh sách lựa chọn (phân tách bằng |)

    public SurveyForm SurveyForm { get; set; } = null!;

    public List<string> OptionList => string.IsNullOrWhiteSpace(Options)
        ? []
        : Options.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
}

// ── Cải tiến chất lượng dịch vụ (Service Improvement / SvImp_SvImprv) ──
// Theo SkyCS: bộ tiêu chí đánh giá chất lượng cuộc gọi (ServiceImprovement.cs,
// models SvImp_SvImprv + các bảng con Honorific/DenyWord/CallTalkTime/Audio).
// Mỗi "bộ cải tiến" (SvImprv) gom nhiều tiêu chí con; dùng để chấm điểm cuộc gọi.

/// <summary>Loại tiêu chí cải tiến (Mst_SvImprvItemType.SvImprvItType).</summary>
public enum SvImprvItemType { Honorific = 0, DenyWord = 1, CallTalkTime = 2, Audio = 3 }

/// <summary>Bộ tiêu chí cải tiến chất lượng dịch vụ (SvImp_SvImprv).</summary>
public class ServiceImprovement : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";            // SvImprvCode
    public string Name { get; set; } = "";            // SvImprvName
    public SvImprvItemType ItemType { get; set; } = SvImprvItemType.Honorific; // SvImprvItType
    public string? Remark { get; set; }               // Remark
    public bool IsActive { get; set; } = true;        // FlagActive
    public DateTime? UsedAt { get; set; }             // DtimeUsed
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // CreateDTimeUTC
    public string CreatedBy { get; set; } = "";       // CreateBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;   // LUDTimeUTC

    public List<SvImprvCriterion> Criteria { get; set; } = [];

    // ── tính toán ────────────────────
    public int CriterionCount => Criteria.Count;
    public int RequiredCount => Criteria.Count(c => c.IsRequired);
    public bool IsUsed => UsedAt.HasValue;
}

/// <summary>
/// Một tiêu chí con trong bộ cải tiến — gộp chung Honorific/DenyWord/CallTalkTime/Audio
/// (SkyCS tách 4 bảng con; ở đây dùng 1 bảng với Kind để dễ nhìn).
/// </summary>
public class SvImprvCriterion : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int ServiceImprovementId { get; set; }
    public SvImprvItemType Kind { get; set; } = SvImprvItemType.Honorific; // loại tiêu chí
    public string Word { get; set; } = "";            // WordDesc — từ khóa / mô tả tiêu chí
    public int QtyStd { get; set; }                   // QtyStd — số lần chuẩn (Honorific/DenyWord)
    public bool IsRequired { get; set; }              // FlagIsRequire (Honorific)
    public int MinValue { get; set; }                 // TalkTimeMinValue / MinValue
    public int MaxValue { get; set; }                 // TalkTimeMaxValue / MaxValue
    public int QtyAllow { get; set; }                 // QtyAllow (Audio)
    public bool IsActive { get; set; } = true;        // FlagActive

    public ServiceImprovement ServiceImprovement { get; set; } = null!;

    /// <summary>Mô tả ngưỡng giá trị theo loại tiêu chí (dùng cho view).</summary>
    public string RangeText => Kind switch
    {
        SvImprvItemType.CallTalkTime => $"{MinValue}–{MaxValue} giây",
        SvImprvItemType.Audio => $"min {MinValue} · max {MaxValue} · cho phép {QtyAllow}",
        _ => QtyStd > 0 ? $"chuẩn {QtyStd} lần" : "—"
    };
}

// ── Trung tâm khách hàng (Customer Center / Mst_Customer) ────────────
// Theo SkyCS: hồ sơ khách hàng (Mst_Customer) là trung tâm dữ liệu CSKH —
// gắn với nhóm khách hàng (Mst_CustomerGroup), người liên hệ
// (Mst_CustomerContact) và lịch sử thay đổi (Mst_CustomerHist).
// (11.BackEnd/V10/idn.SkyCS.Biz/CustomerCenter/Customer.cs)

/// <summary>Loại khách hàng (Mst_Customer.CustomerType).</summary>
public enum CustomerType { Individual = 0, Business = 1 }

/// <summary>Đối tượng khách hàng (Mst_Customer.PartnerType).</summary>
public enum PartnerType { Customer = 0, Supplier = 1, Both = 2 }

/// <summary>Nhóm khách hàng (Mst_CustomerGroup) — phân nhóm phục vụ CSKH.</summary>
public class CustomerGroup : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";          // CustomerGrpCode
    public string Name { get; set; } = "";          // CustomerGrpName
    public string? Description { get; set; }          // CustomerGrpDesc
    public bool IsActive { get; set; } = true;        // FlagActive
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<Customer> Customers { get; set; } = [];

    public int CustomerCount => Customers.Count;
}

/// <summary>Hồ sơ khách hàng (Mst_Customer) — trung tâm dữ liệu CSKH.</summary>
public class Customer : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";            // CustomerCodeSys — mã hệ thống
    public string? CodeInvoice { get; set; }           // CustomerCodeInvoice — mã xuất hóa đơn
    public string Name { get; set; } = "";            // CustomerName
    public string? NameEN { get; set; }                // CustomerNameEN
    public CustomerType Type { get; set; } = CustomerType.Individual;  // CustomerType
    public PartnerType Partner { get; set; } = PartnerType.Customer;   // PartnerType
    public string? TaxCode { get; set; }               // MST — mã số thuế
    public string? GroupCode { get; set; }             // CustomerGrpCode — nhóm khách hàng
    // Liên hệ
    public string? Phone { get; set; }                 // Mst_CustomerPhone
    public string? Email { get; set; }                 // Mst_CustomerEmail
    public string? Address { get; set; }               // địa chỉ liên hệ
    public string? Province { get; set; }              // ProvinceCodeContact
    public string? District { get; set; }              // DistrictCodeContact
    public bool IsActive { get; set; } = true;         // FlagActive
    public string? Remark { get; set; }                // ghi chú
    public DateTime? UsedAt { get; set; }              // DTimeUsed
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // CreateDTimeUTC
    public string CreatedBy { get; set; } = "";        // CreateBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;   // LUDTimeUTC
    public List<CustomerContact> Contacts { get; set; } = [];
    public List<CustomerHistory> Histories { get; set; } = [];

    // ── tính toán ────────────────────
    public int ContactCount => Contacts.Count;
    public string TypeName => Type == CustomerType.Business ? "Doanh nghiệp" : "Cá nhân";
    public string Initials => string.IsNullOrWhiteSpace(Name) ? "?" : Name.Trim()[0].ToString().ToUpperInvariant();
}

/// <summary>Người liên hệ của khách hàng (Mst_CustomerContact).</summary>
public class CustomerContact : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int CustomerId { get; set; }
    public string Name { get; set; } = "";            // tên người liên hệ
    public string? Title { get; set; }                 // chức danh
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;         // FlagActive
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Customer Owner { get; set; } = null!;
}

/// <summary>Lịch sử thay đổi hồ sơ khách hàng (Mst_CustomerHist).</summary>
public class CustomerHistory : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int CustomerId { get; set; }
    public string Action { get; set; } = "Cập nhật";   // hành động
    public string? Detail { get; set; }                // JsonCustomerInfoHist — mô tả thay đổi
    public string ChangedBy { get; set; } = "";        // LUBy
    public DateTime ChangedAt { get; set; } = DateTime.Now;   // LUDTimeUTC
    public Customer Owner { get; set; } = null!;
}

// ── Thiết lập phân bổ phiếu tự động (Mst_EstablishAllocateETicket) ────
// Theo SkyCS: cấu hình cách hệ thống tự động phân bổ eTicket mới về phòng ban
// và gán cho agent. Gồm 1 bản ghi cấu hình (Mst_EstablishAllocateETicket) +
// danh sách agent nhận phiếu (Mst_EstablishAllocateETAssignAgent).
// (11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_EstablishAllocateETicket_Get`/`_SaveX`)

/// <summary>Thiết lập phân bổ phiếu tự động (Mst_EstablishAllocateETicket).</summary>
public class AllocateRule : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string DepartmentCode { get; set; } = "";   // DepartmentCode — phòng ban nhận phiếu
    public bool AllocateEven { get; set; } = true;      // FlagAllocateEven — chia đều cho các agent
    public bool AssignAgent { get; set; } = true;       // FlagAssignAgent — tự gán agent
    public bool AllMissedCall { get; set; }             // FlagAllMissedCall — gán cả cuộc gọi nhỡ
    public bool IsActive { get; set; } = true;          // FlagActive
    public string? Remark { get; set; }                 // Remark
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";        // LogLUBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public List<AllocateAgent> Agents { get; set; } = [];

    // ── tính toán ────────────────────
    public int AgentCount => Agents.Count;
    public string ModeText => AssignAgent
        ? (AllocateEven ? "Chia đều cho agent" : "Gán agent theo thứ tự")
        : "Chỉ phân về phòng ban";
}

/// <summary>Agent nhận phiếu trong thiết lập phân bổ (Mst_EstablishAllocateETAssignAgent).</summary>
public class AllocateAgent : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int AllocateRuleId { get; set; }
    public string AgentCode { get; set; } = "";        // AgentCode — mã agent (Sys_User.UserCode)
    public string? Remark { get; set; }                 // Remark
    public bool IsActive { get; set; } = true;          // FlagActive
    public AllocateRule Rule { get; set; } = null!;
}

// ── Thiết lập nhắc nhở phiếu (Mst_EstablishRemindETicket) ────────────
// Theo SkyCS: cấu hình kênh thông báo khi phiếu tới hạn / quá hạn xử lý.
// Mỗi bản ghi bật/tắt 4 kênh (Hệ thống/Email/SMS/Zalo) và gắn mẫu nội dung
// (SubFormCode) cho từng kênh. (11.BackEnd/V10/idn.SkyCS.Biz/Master.cs,
// `Mst_EstablishRemindETicket_Get`/`_SaveX`)

/// <summary>Kênh nhắc nhở phiếu (Mst_EstablishRemindETicket.FlagNotify*).</summary>
public enum RemindChannel { System = 0, Email = 1, Sms = 2, Zalo = 3 }

/// <summary>Thiết lập nhắc nhở phiếu (Mst_EstablishRemindETicket).</summary>
public class ReminderRule : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string EstablishId { get; set; } = "";       // EstablishID — mã thiết lập
    public bool NotifySystem { get; set; } = true;       // FlagNotifySystem — thông báo trên hệ thống
    public bool NotifyEmail { get; set; }                // FlagNotifyEmail
    public bool NotifySms { get; set; }                  // FlagNotifySMS
    public bool NotifyZalo { get; set; }                 // FlagNotifyZalo
    public string? SubFormCodeEmail { get; set; }        // SubFormCodeEmail — mẫu nội dung email
    public string? SubFormCodeSms { get; set; }          // SubFormCodeSMS — mẫu nội dung SMS
    public string? SubFormCodeZalo { get; set; }         // SubFormCodeZalo — mẫu nội dung Zalo
    public bool IsActive { get; set; } = true;           // FlagActive
    public string? Remark { get; set; }                  // Remark
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";          // LogLUBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // ── tính toán ────────────────────
    /// <summary>Số kênh nhắc nhở đang bật.</summary>
    public int ChannelCount => (NotifySystem ? 1 : 0) + (NotifyEmail ? 1 : 0) + (NotifySms ? 1 : 0) + (NotifyZalo ? 1 : 0);
    /// <summary>Danh sách kênh đang bật (dùng cho view).</summary>
    public List<RemindChannel> Channels
    {
        get
        {
            var list = new List<RemindChannel>();
            if (NotifySystem) list.Add(RemindChannel.System);
            if (NotifyEmail) list.Add(RemindChannel.Email);
            if (NotifySms) list.Add(RemindChannel.Sms);
            if (NotifyZalo) list.Add(RemindChannel.Zalo);
            return list;
        }
    }
}

// ── Danh mục phiếu (Mst_TicketStatus / Mst_TicketPriority / ──────────
//    Mst_TicketSource / Mst_ReceptionChannel) ────────────────────────
// Theo SkyCS: các bảng danh mục dùng chung cho eTicket đều có cùng bộ cột
// (mã, tên cho agent, tên cho khách, FlagUseType, FlagActive, Remark).
// Gộp 4 danh mục vào 1 bảng với Kind để dễ nhìn và quản trị tập trung.
// (11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_TicketStatus_Save`/`_Get`,
//  `Mst_TicketPriority_Save`, `Mst_TicketSource_Save`, `Mst_ReceptionChannel_Save`)

/// <summary>Loại danh mục phiếu (bảng gốc bên SkyCS).</summary>
public enum TicketCatalogKind { Status = 0, Priority = 1, Source = 2, ReceptionChannel = 3 }

/// <summary>Phạm vi sử dụng (Mst_*.FlagUseType — TYPE1/TYPE2/TYPE3).</summary>
public enum CatalogUseType { Type1 = 0, Type2 = 1, Type3 = 2 }

/// <summary>
/// Một dòng danh mục phiếu (gộp Mst_TicketStatus/TicketPriority/TicketSource/ReceptionChannel).
/// Mỗi danh mục có mã (Code), tên hiển thị cho agent và cho khách hàng.
/// </summary>
public class TicketCatalog : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public TicketCatalogKind Kind { get; set; } = TicketCatalogKind.Status;  // bảng gốc
    public string Code { get; set; } = "";            // TicketStatus/TicketPriority/TicketSource/ReceptionChannel
    public string AgentName { get; set; } = "";        // Agent*Name — tên cho agent
    public string CustomerName { get; set; } = "";     // Customer*Name — tên cho khách
    public CatalogUseType UseType { get; set; } = CatalogUseType.Type2;  // FlagUseType
    public bool IsActive { get; set; } = true;         // FlagActive
    public string? Remark { get; set; }                // Remark
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";        // LogLUBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // ── tính toán ────────────────────
    public string KindName => Kind switch
    {
        TicketCatalogKind.Status => "Trạng thái phiếu",
        TicketCatalogKind.Priority => "Mức ưu tiên",
        TicketCatalogKind.Source => "Nguồn phiếu",
        TicketCatalogKind.ReceptionChannel => "Kênh tiếp nhận",
        _ => Kind.ToString()
    };
}

// ── Phòng ban (Mst_Department) ───────────────────────────────────────
// Theo SkyCS: phòng ban là đơn vị tổ chức nhận và xử lý eTicket, có phân cấp
// (DepartmentCodeParent) và cờ phân chia tự động đều cho thành viên (FlagAutoDiv).
// Là master data nền cho phân bổ phiếu (Mst_EstablishAllocateETicket.DepartmentCode)
// và gán agent theo phòng ban (Sys_UserMapDepartment).
// (11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_Department_Get`/`_Update`;
//  model 12.Dev.Common/idn.SkyCS.Common/Models/Mst_Department.cs)

/// <summary>Phòng ban (Mst_Department) — đơn vị tổ chức nhận/xử lý phiếu, có phân cấp.</summary>
public class Department : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";              // DepartmentCode — mã phòng ban
    public string? ParentCode { get; set; }              // DepartmentCodeParent — mã phòng ban cấp trên
    public string Name { get; set; } = "";              // DepartmentName — tên phòng ban
    public string? Description { get; set; }             // DepartmentDesc — mô tả
    public int Level { get; set; } = 1;                  // DepartmentLevel — cấp phòng ban
    public string? TaxCode { get; set; }                 // MST — mã số thuế
    public bool AutoDiv { get; set; }                    // FlagAutoDiv — chia đều tự động cho thành viên
    public bool IsActive { get; set; } = true;           // FlagActive
    public int Order { get; set; }                       // OrderIdx — thứ tự hiển thị
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";          // LogLUBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public List<DepartmentMember> Members { get; set; } = [];

    // ── tính toán ────────────────────
    public int MemberCount => Members.Count;
    public bool IsRoot => string.IsNullOrWhiteSpace(ParentCode);
    public string LevelName => Level <= 1 ? "Cấp 1" : $"Cấp {Level}";
}

/// <summary>Thành viên (agent) thuộc phòng ban (Sys_UserMapDepartment).</summary>
public class DepartmentMember : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int DepartmentId { get; set; }
    public string UserCode { get; set; } = "";          // UserCode — mã nhân viên (Sys_User.UserCode)
    public string FullName { get; set; } = "";          // FullName — tên hiển thị
    public string? Email { get; set; }                   // Email
    public string? Phone { get; set; }                   // PhoneNo
    public bool IsActive { get; set; } = true;           // FlagActive

    public Department Department { get; set; } = null!;
}

// ── Điều khoản thanh toán (Mst_PaymentTerm) ──────────────────────────
// Theo SkyCS: điều khoản thanh toán quy định cách khách hàng thanh toán —
// loại (bán/mua), số ngày được nợ, hạn mức công nợ tối đa và % đặt cọc.
// Là master data nền cho hồ sơ khách hàng / đơn hàng.
// (11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_PaymentTerm_Get`/`_SaveX`;
//  model 12.Dev.Common/idn.SkyCS.Common/Models/Mst_PaymentTerm.cs;
//  hằng số TConst.PTType = SALE/PURCHASE)

/// <summary>Loại điều khoản thanh toán (Mst_PaymentTerm.PTType — TConst.PTType).</summary>
public enum PTType { Sale = 0, Purchase = 1 }

/// <summary>Điều khoản thanh toán (Mst_PaymentTerm).</summary>
public class PaymentTerm : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";              // PaymentTermCode — mã điều khoản
    public string Name { get; set; } = "";              // PaymentTermName — tên điều khoản
    public PTType Type { get; set; } = PTType.Sale;     // PTType — loại (SALE/PURCHASE)
    public string? Description { get; set; }            // PTDesc — mô tả
    public int OwedDay { get; set; }                    // OwedDay — số ngày được nợ
    public decimal CreditLimit { get; set; }            // CreditLimit — hạn mức công nợ tối đa
    public decimal DepositPercent { get; set; }         // DepositPercent — % thanh toán đặt cọc
    public bool IsActive { get; set; } = true;          // FlagActive
    public string? Remark { get; set; }                 // Remark
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";         // LogLUBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // ── tính toán ────────────────────
    public string TypeName => Type == PTType.Purchase ? "Mua vào" : "Bán ra";
    public string OwedDayText => OwedDay <= 0 ? "Thanh toán ngay" : $"Nợ {OwedDay} ngày";
    public string CreditLimitText => CreditLimit <= 0 ? "Không giới hạn" : CreditLimit.ToString("#,##0");
    public string DepositText => DepositPercent <= 0 ? "—" : $"{DepositPercent:0.##}%";
}

// ── Vùng thị trường (Mst_Area) ───────────────────────────────────────
// Theo SkyCS: vùng thị trường (Mst_Area) là master data của Trung tâm khách hàng,
// phân cấp theo AreaCodeParent (vùng → tỉnh → khu vực) và gắn khách hàng vào vùng
// qua Mst_CustomerInArea. Dùng để phân vùng phục vụ, báo cáo theo khu vực.
// (11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_Area_Get`/`_SaveX`;
//  model 12.Dev.Common/idn.SkyCS.Common/Models/CustomerCentrer/Mst_Area.cs)

/// <summary>Vùng thị trường (Mst_Area) — master data phân vùng khách hàng, có phân cấp.</summary>
public class Area : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";              // AreaCode — mã vùng
    public string? ParentCode { get; set; }              // AreaCodeParent — mã vùng cấp trên
    public string Name { get; set; } = "";              // AreaName — tên vùng
    public string? Description { get; set; }             // AreaDesc — mô tả
    public int Level { get; set; } = 1;                  // AreaLevel — cấp vùng
    public string? BUCode { get; set; }                  // AreaBUCode — mã đơn vị kinh doanh
    public string? BUPattern { get; set; }               // AreaBUPattern — mẫu đơn vị kinh doanh
    public bool IsActive { get; set; } = true;           // FlagActive
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";          // LogLUBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // ── tính toán ────────────────────
    public bool IsRoot => string.IsNullOrWhiteSpace(ParentCode);
    public string LevelName => Level <= 1 ? "Cấp 1" : $"Cấp {Level}";
}

// ── Thẻ (Tag) — Mst_Tag ──────────────────────────────────────────────
// Theo SkyCS: thẻ (Mst_Tag) là nhãn dùng chung để gắn/phân loại nội dung
// CSKH (bài viết Knowledge Base, phiếu hỗ trợ). Mỗi thẻ có mã (TagID),
// tên hiển thị (TagName), mô tả (TagDesc) và slug (Slug — dùng cho URL/tìm kiếm).
// (11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_Tag_Get`/`Mst_Tag_Create`;
//  model 12.Dev.Common/idn.SkyCS.Common/Models/Mst_Tag.cs;
//  controller 13.ClientGate/V20/idn.SkyCS.WebAPI/Controllers/MstTagController.cs)

/// <summary>Thẻ (Mst_Tag) — nhãn dùng chung để gắn/phân loại nội dung CSKH.</summary>
public class Tag : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";              // TagID — mã thẻ
    public string Name { get; set; } = "";              // TagName — tên thẻ
    public string? Description { get; set; }             // TagDesc — mô tả
    public string? Slug { get; set; }                    // Slug — đường dẫn rút gọn
    public bool IsActive { get; set; } = true;           // FlagActive
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";          // LogLUBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // ── tính toán ────────────────────
    /// <summary>Slug hiển thị: dùng Slug nếu có, ngược lại sinh từ tên.</summary>
    public string SlugText => !string.IsNullOrWhiteSpace(Slug) ? Slug! : MakeSlug(Name);

    /// <summary>Sinh slug từ tên (bỏ dấu tiếng Việt, thay khoảng trắng bằng '-').</summary>
    public static string MakeSlug(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        var norm = s.Trim().ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder();
        foreach (var ch in norm)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch) == System.Globalization.UnicodeCategory.NonSpacingMark) continue;
            if (ch == 'đ') { sb.Append('d'); continue; }
            if (char.IsLetterOrDigit(ch)) sb.Append(ch);
            else if (ch is ' ' or '-' or '_' or '/') sb.Append('-');
        }
        return sb.ToString().Trim('-');
    }
}

// ── Người nhận thông báo phiếu (Mst_EstablishReceiveNotifyETicket) ────
// Theo SkyCS: danh sách agent (Sys_User.UserCode) sẽ nhận thông báo khi có
// eTicket mới. Là cấu hình dạng danh sách theo tổ chức (OrgID) — khi lưu sẽ
// ClearAll/InsertAll (theo `Mst_EstablishReceiveNotifyETicket_SaveX`).
// (11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_EstablishReceiveNotifyETicket_Get`/`_SaveX`;
//  model 12.Dev.Common/idn.SkyCS.Common/Models/Mst_EstablishReceiveNotifyETicket.cs;
//  controller 13.ClientGate/V20/idn.SkyCS.WebAPI/Controllers/MstEstablishReceiveNotifyETicketController.cs)

/// <summary>Người nhận thông báo phiếu (Mst_EstablishReceiveNotifyETicket) — 1 agent nhận thông báo.</summary>
public class ReceiveNotify : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string AgentCode { get; set; } = "";        // AgentCode — mã agent (Sys_User.UserCode)
    public string? AgentName { get; set; }              // su_UserName — tên agent (join Sys_User)
    public string? Remark { get; set; }                 // Remark — ghi chú
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";        // LogLUBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // ── tính toán ────────────────────
    public string DisplayName => string.IsNullOrWhiteSpace(AgentName) ? AgentCode : AgentName!;
}

// ── Danh mục địa chỉ (Mst_Province / Mst_District / Mst_Ward) ────────
// Theo SkyCS: danh mục địa chỉ hành chính 3 cấp — Tỉnh/Thành (Mst_Province),
// Quận/Huyện (Mst_District, thuộc 1 tỉnh) và Phường/Xã (Mst_Ward, thuộc 1
// quận/huyện). Là master data nền cho địa chỉ khách hàng (Mst_Customer.
// ProvinceCode/DistrictCode) và giao hàng.
// (11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_Province_Get`/`Mst_District_Get`/
//  `Mst_Ward_Get`; models 12.Dev.Common/idn.SkyCS.Common/Models/Mst_Province.cs,
//  Mst_District.cs, CustomerCentrer/Mst_Ward.cs)

/// <summary>Cấp địa chỉ hành chính (bảng gốc bên SkyCS).</summary>
public enum AddressLevel { Province = 0, District = 1, Ward = 2 }

/// <summary>
/// Một dòng danh mục địa chỉ (gộp Mst_Province/Mst_District/Mst_Ward).
/// Mỗi cấp có mã riêng; District/Ward gắn mã cấp trên (ParentCode) để tạo phân cấp.
/// </summary>
public class Address : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public AddressLevel Level { get; set; } = AddressLevel.Province;  // bảng gốc
    public string Code { get; set; } = "";              // ProvinceCode/DistrictCode/WardCode
    public string Name { get; set; } = "";              // ProvinceName/DistrictName/WardName
    public string? ParentCode { get; set; }             // ProvinceCode (District) / DistrictCode (Ward)
    public string? PostCode { get; set; }               // PostCode (Province/District)
    public string? CountryCode { get; set; }            // CountryCode (Province)
    public bool IsActive { get; set; } = true;          // FlagActive
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";         // LogLUBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // ── tính toán ────────────────────
    public string LevelName => Level switch
    {
        AddressLevel.Province => "Tỉnh / Thành phố",
        AddressLevel.District => "Quận / Huyện",
        AddressLevel.Ward => "Phường / Xã",
        _ => Level.ToString()
    };
    public bool IsRoot => Level == AddressLevel.Province;
}
// ── Lịch làm việc SLA (Mst_SLAWorkingDay / Mst_SLAHoliday) ────────────
// Theo SkyCS: mỗi chính sách SLA (Mst_SLA) có thể gắn một LỊCH LÀM VIỆC để
// tính hạn xử lý (deadline) theo giờ hành chính thay vì 24/7. Lịch gồm:
//  • Mst_SLAWorkingDay — giờ làm việc theo từng thứ trong tuần, mỗi ngày có
//    2 ca (Idx=1 buổi sáng, Idx=2 buổi chiều); WorkingDTimeFrom/To là số PHÚT
//    tính từ 00:00 (ví dụ 480 = 08:00, 720 = 12:00).
//  • Mst_SLAHoliday — danh sách ngày nghỉ (SLAHoliday dạng "dd-MM") không tính
//    vào thời gian xử lý.
// Thuật toán tính deadline: Mst_SLA_CalcDeadlineX / Mst_SLA_CalcFirstResDTimeX
// (11.BackEnd/V10/idn.SkyCS.Biz/Master.1.cs) — cộng dồn số phút cam kết, chỉ
// tính trong các ca làm việc, bỏ qua ngày nghỉ và ngày không có ca.
// (models 12.Dev.Common/idn.SkyCS.Common/Models/Mst_SLAWorkingDay.cs, Mst_SLAHoliday.cs)

/// <summary>Ca làm việc trong ngày (Mst_SLAWorkingDay.Idx).</summary>
public enum SlaShift { Morning = 1, Afternoon = 2 }

/// <summary>
/// Giờ làm việc của 1 thứ trong tuần cho 1 chính sách SLA (Mst_SLAWorkingDay).
/// SLAWorkingDayCode: 1=Chủ nhật … 7=Thứ bảy (theo SkyCS). Mỗi (thứ, ca) là 1 dòng.
/// </summary>
public class SlaWorkingDay : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int SlaPolicyId { get; set; }                 // SLAID — chính sách SLA
    public int WeekdayCode { get; set; } = 2;            // SLAWorkingDayCode — 1=CN … 7=Thứ bảy
    public SlaShift Shift { get; set; } = SlaShift.Morning; // Idx — 1 sáng / 2 chiều
    public int FromMinutes { get; set; }                 // WorkingDTimeFrom — phút từ 00:00
    public int ToMinutes { get; set; }                   // WorkingDTimeTo — phút từ 00:00
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";          // LogLUBy

    public SlaPolicy SlaPolicy { get; set; } = null!;

    // ── tính toán ────────────────────────────────────────────────────
    /// <summary>Số phút làm việc của ca này.</summary>
    public int Minutes => Math.Max(0, ToMinutes - FromMinutes);
    /// <summary>Tên thứ (theo SLAWorkingDayCode).</summary>
    public string WeekdayName => WeekdayCode switch
    {
        1 => "Chủ nhật", 2 => "Thứ hai", 3 => "Thứ ba", 4 => "Thứ tư",
        5 => "Thứ năm", 6 => "Thứ sáu", 7 => "Thứ bảy", _ => $"Thứ {WeekdayCode}"
    };
    /// <summary>Khoảng giờ dạng HH:mm–HH:mm.</summary>
    public string RangeText => $"{Fmt(FromMinutes)}–{Fmt(ToMinutes)}";
    public static string Fmt(int minutes) => $"{minutes / 60:D2}:{minutes % 60:D2}";
}

/// <summary>Ngày nghỉ của 1 chính sách SLA (Mst_SLAHoliday) — không tính vào thời gian xử lý.</summary>
public class SlaHoliday : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int SlaPolicyId { get; set; }                 // SLAID — chính sách SLA
    public string Holiday { get; set; } = "";            // SLAHoliday — "dd-MM" (lặp hằng năm)
    public string Name { get; set; } = "";               // SLAHolidayName — tên ngày nghỉ
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";          // LogLUBy

    public SlaPolicy SlaPolicy { get; set; } = null!;
}

// ── Điều kiện áp dụng SLA (Mst_SLATicketType / Mst_SLATicketCustomType / ──
//    Mst_SLACustomerCN / Mst_SLACustomerGroupCN / Mst_SLACustomerDN / ──
//    Mst_SLACustomerGroupDN) ────────────────────────────────────────────
// Theo SkyCS: mỗi chính sách SLA (Mst_SLA) được gán cho một tập đối tượng
// áp dụng — loại phiếu (TicketType), loại phiếu tùy chỉnh (TicketCustomType),
// khách hàng cá nhân (CN), nhóm khách cá nhân, khách doanh nghiệp (DN) và
// nhóm khách doanh nghiệp. Ngoài ra Mst_SLA có các cờ "áp dụng cho TẤT CẢ"
// (FlagAllTicketType/FlagAllTicketCustomType/FlagAllCustomerCN/…): khi bật cờ
// thì không cần liệt kê chi tiết. Khi tạo eTicket, hệ thống dò các điều kiện
// này để chọn đúng chính sách SLA (Master.1.cs, `Mst_SLA_Get`/`Mst_SLA_SaveX`).
// Ở đây gộp 6 bảng con vào 1 bảng `SlaScope` với Kind để dễ nhìn.

/// <summary>Loại đối tượng áp dụng SLA (bảng con gốc bên SkyCS).</summary>
public enum SlaScopeKind
{
    TicketType = 0,          // Mst_SLATicketType — loại phiếu
    TicketCustomType = 1,    // Mst_SLATicketCustomType — loại phiếu tùy chỉnh
    CustomerCN = 2,          // Mst_SLACustomerCN — khách hàng cá nhân
    CustomerGroupCN = 3,     // Mst_SLACustomerGroupCN — nhóm khách cá nhân
    CustomerDN = 4,          // Mst_SLACustomerDN — khách hàng doanh nghiệp
    CustomerGroupDN = 5      // Mst_SLACustomerGroupDN — nhóm khách doanh nghiệp
}

/// <summary>
/// Một đối tượng áp dụng của chính sách SLA (gộp 6 bảng con Mst_SLA*).
/// Mỗi dòng = 1 cặp (SLA ↔ mã đối tượng) theo Kind.
/// </summary>
public class SlaScope : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int SlaPolicyId { get; set; }                 // SLAID — chính sách SLA
    public SlaScopeKind Kind { get; set; } = SlaScopeKind.TicketType;  // bảng con gốc
    public string RefCode { get; set; } = "";            // TicketType/TicketCustomType/CustomerCodeSys/CustomerGrpCode
    public string? Remark { get; set; }                  // ghi chú
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";          // LogLUBy

    public SlaPolicy SlaPolicy { get; set; } = null!;

    // ── tính toán ────────────────────────────────────────────────────
    public string KindName => Kind switch
    {
        SlaScopeKind.TicketType => "Loại phiếu",
        SlaScopeKind.TicketCustomType => "Loại phiếu tùy chỉnh",
        SlaScopeKind.CustomerCN => "Khách hàng cá nhân",
        SlaScopeKind.CustomerGroupCN => "Nhóm khách cá nhân",
        SlaScopeKind.CustomerDN => "Khách hàng doanh nghiệp",
        SlaScopeKind.CustomerGroupDN => "Nhóm khách doanh nghiệp",
        _ => Kind.ToString()
    };
}

// ── Kênh liên hệ (Mst_ContactChannel) ────────────────────────────────
// Theo SkyCS: kênh liên hệ (Mst_ContactChannel) là master data quy định
// CÁCH liên hệ với khách hàng (gọi điện, email, Zalo, SMS, gặp trực tiếp…),
// khác với "kênh tiếp nhận" (Mst_ReceptionChannel — nơi phiếu được tạo đến).
// Mỗi kênh có mã (ContactChannel), tên hiển thị cho agent và cho khách hàng,
// phạm vi sử dụng (FlagUseType TYPE1/2/3) và trạng thái (FlagActive).
// eTicket dùng kênh liên hệ để xác định cách thức liên lạc với khách.
// (11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_ContactChannel_Get`/`_Save`;
//  model 12.Dev.Common/idn.SkyCS.Common/Models/Mst_ContactChannel.cs)

/// <summary>Kênh liên hệ (Mst_ContactChannel) — cách liên hệ với khách hàng.</summary>
public class ContactChannel : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";              // ContactChannel — mã kênh liên hệ
    public string AgentName { get; set; } = "";         // AgentContactChannelName — tên cho agent
    public string CustomerName { get; set; } = "";      // CustomerContactChannelName — tên cho khách
    public CatalogUseType UseType { get; set; } = CatalogUseType.Type2;  // FlagUseType (TYPE1/2/3)
    public bool IsActive { get; set; } = true;          // FlagActive
    public string? Remark { get; set; }                 // Remark
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";         // LogLUBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // ── tính toán ────────────────────────────────────────────────────
    public string UseTypeName => UseType switch
    {
        CatalogUseType.Type1 => "Chỉ agent",
        CatalogUseType.Type2 => "Agent & khách",
        CatalogUseType.Type3 => "Chỉ khách",
        _ => UseType.ToString()
    };
}// ── Loại phiếu tùy chỉnh (Mst_TicketCustomType) ──────────────────────
// Theo SkyCS: "loại phiếu tùy chỉnh" (Mst_TicketCustomType) là phân loại
// con của eTicket — mỗi loại có mã (TicketCustomType), tên hiển thị cho
// agent và cho khách, phạm vi sử dụng (FlagUseType TYPE1/2/3) và trạng thái.
// eTicket gắn loại tùy chỉnh qua ET_Ticket.TicketCustomType; khi hiển thị sẽ
// join sang bảng này để lấy tên cho agent/khách (ETicket.cs).
// Loại tùy chỉnh được gán cho từng "phân loại nghiệp vụ" (Mst_TicketType) qua
// bảng map Mst_TicketTypeMapCustom (TicketType ↔ TicketCustomType).
// (11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_TicketCustomType_Get`/
//  `Mst_TicketCustomType_GetByTicketType`/`Mst_TicketCustomType_Save`;
//  model 12.Dev.Common/idn.SkyCS.Common/Models/Mst_TicketCustomType.cs,
//  Mst_TicketTypeMapCustom.cs; cột xác nhận qua TblMst_TicketCustomType)

/// <summary>Loại phiếu tùy chỉnh (Mst_TicketCustomType) — phân loại con của eTicket.</summary>
public class TicketCustomType : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";              // TicketCustomType — mã loại tùy chỉnh
    public string AgentName { get; set; } = "";         // AgentTicketCustomTypeName — tên cho agent
    public string CustomerName { get; set; } = "";      // CustomerTicketCustomTypeName — tên cho khách
    public CatalogUseType UseType { get; set; } = CatalogUseType.Type2;  // FlagUseType (TYPE1/2/3)
    public bool IsActive { get; set; } = true;          // FlagActive
    public string? Remark { get; set; }                 // Remark
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";         // LogLUBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public List<TicketCustomTypeMap> Maps { get; set; } = [];

    // ── tính toán ────────────────────────────────────────────────────
    public int MappedTypeCount => Maps.Count;
    public string UseTypeName => UseType switch
    {
        CatalogUseType.Type1 => "Chỉ agent",
        CatalogUseType.Type2 => "Agent & khách",
        CatalogUseType.Type3 => "Chỉ khách",
        _ => UseType.ToString()
    };
}

/// <summary>
/// Gán loại phiếu tùy chỉnh cho 1 phân loại nghiệp vụ (Mst_TicketTypeMapCustom).
/// Mỗi dòng = 1 cặp (TicketType ↔ TicketCustomType).
/// </summary>
public class TicketCustomTypeMap : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int TicketCustomTypeId { get; set; }
    public string TicketTypeCode { get; set; } = "";    // TicketType — mã phân loại nghiệp vụ (Mst_TicketType)
    public string? Remark { get; set; }                 // ghi chú
    public bool IsActive { get; set; } = true;          // FlagActive

    public TicketCustomType CustomType { get; set; } = null!;
}// ── Người nộp thuế (Mst_NNT) ─────────────────────────────────────────
// Theo SkyCS: "người nộp thuế" (Mst_NNT) là hồ sơ doanh nghiệp/tổ chức nộp
// thuế trong Trung tâm khách hàng — gắn với mã số thuế (MST), thông tin pháp
// lý (giấy phép KD, người đại diện, chữ ký số), địa chỉ (tỉnh/huyện), ngân
// hàng và người liên hệ. Là master data nền cho hồ sơ khách hàng doanh nghiệp
// và đăng ký dịch vụ TVAN (TCTStatus = kết quả Tổng cục Thuế).
// (11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_NNT_Get`/`Mst_NNT_Update`/
//  `Mst_NNT_CreateForNetwork`; model 12.Dev.Common/idn.SkyCS.Common/Models/Mst_NNT.cs;
//  controller 13.ClientGate/V20/idn.SkyCS.WebAPI/Controllers/MstNNTController.cs;
//  hằng số MstNNT_TCTStatus trong Const.Main.BE.cs)

/// <summary>Trạng thái đăng ký dịch vụ TVAN với Tổng cục Thuế (MstNNT_TCTStatus).</summary>
public enum TctStatus { None = 0, Registered = 1, Cancelled = 2 }

/// <summary>Người nộp thuế (Mst_NNT) — hồ sơ doanh nghiệp/tổ chức nộp thuế.</summary>
public class Taxpayer : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string TaxCode { get; set; } = "";            // MST — mã số thuế (khóa nghiệp vụ)
    public string FullName { get; set; } = "";           // NNTFullName — tên doanh nghiệp
    public string? ShortName { get; set; }               // NNTShortName — tên viết tắt
    public string? ParentTaxCode { get; set; }           // MSTParent — đơn vị trực thuộc
    public int Level { get; set; } = 1;                  // MSTLevel — cấp đơn vị
    public string? BUCode { get; set; }                  // MSTBUCode — mã đơn vị kinh doanh
    public string? BUPattern { get; set; }               // MSTBUPattern — mẫu đơn vị kinh doanh
    // Địa chỉ
    public string? Address { get; set; }                 // NNTAddress — địa chỉ người nộp thuế
    public string? ProvinceCode { get; set; }            // ProvinceCode — mã tỉnh
    public string? DistrictCode { get; set; }            // DistrictCode — mã huyện
    // Liên hệ
    public string? Mobile { get; set; }                  // NNTMobile — ĐT di động
    public string? Phone { get; set; }                   // NNTPhone — ĐT cố định
    public string? Fax { get; set; }                     // NNTFax
    public string? Website { get; set; }                 // Website
    // Pháp lý / người đại diện
    public string? PresentBy { get; set; }               // PresentBy — người đại diện
    public string? Position { get; set; }                // NNTPosition — chức vụ
    public string? BusinessRegNo { get; set; }           // BusinessRegNo — giấy phép KD
    public string? PresentIDNo { get; set; }             // PresentIDNo — số giấy tờ
    public string? PresentIDType { get; set; }           // PresentIDType — loại giấy tờ tùy thân
    public string? GovTaxID { get; set; }                // GovTaxID — CQT quản lý
    // Người liên hệ
    public string? ContactName { get; set; }             // ContactName — tên người liên lạc
    public string? ContactPhone { get; set; }            // ContactPhone — ĐT người liên hệ
    public string? ContactEmail { get; set; }            // ContactEmail — email người liên hệ
    // Chữ ký số (CA)
    public string? CANumber { get; set; }                // CANumber — chứng thư số
    public string? CAOrg { get; set; }                   // CAOrg — tổ chức cấp CTS
    public DateTime? CAEffStart { get; set; }            // CAEffDTimeUTCStart — hiệu lực từ
    public DateTime? CAEffEnd { get; set; }              // CAEffDTimeUTCEnd — hiệu lực đến
    // Ngân hàng
    public string? AccNo { get; set; }                   // AccNo — số tài khoản
    public string? AccHolder { get; set; }               // AccHolder — chủ tài khoản
    public string? BankName { get; set; }                // BankName — ngân hàng
    // Phân loại / trạng thái
    public string? BizType { get; set; }                 // BizType — loại hình tổ chức
    public string? BizFieldCode { get; set; }            // BizFieldCode — lĩnh vực hoạt động
    public string? BizSizeCode { get; set; }             // BizSizeCode — quy mô tổ chức
    public string? AreaCode { get; set; }                // AreaCode — vùng thị trường
    public TctStatus TctStatus { get; set; } = TctStatus.None;  // TCTStatus — kết quả TCT
    public bool IsActive { get; set; } = true;           // FlagActive
    public string? Remark { get; set; }                  // Remark
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";          // LogLUBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // ── tính toán ────────────────────────────────────────────────────
    public string TctStatusName => TctStatus switch
    {
        TctStatus.Registered => "TCT xác nhận đăng ký",
        TctStatus.Cancelled => "TCT xác nhận ngừng",
        _ => "Chưa đăng ký"
    };
    public bool IsRoot => string.IsNullOrWhiteSpace(ParentTaxCode);
    public string LevelName => Level <= 1 ? "Cấp 1" : $"Cấp {Level}";
    public string Initials => string.IsNullOrWhiteSpace(FullName) ? "?" : FullName.Trim()[0].ToString().ToUpperInvariant();
    /// <summary>Địa chỉ đầy đủ (địa chỉ + tỉnh/huyện) dùng cho view.</summary>
    public string AddressText => string.Join(", ", new[] { Address, DistrictCode, ProvinceCode }
        .Where(s => !string.IsNullOrWhiteSpace(s)));
}// ── Loại chiến dịch (Mst_CampaignType) ───────────────────────────────
// Theo SkyCS: "loại chiến dịch" (Mst_CampaignType) là master data phân loại
// chiến dịch gọi ra (Cpn_Campaign.CampaignTypeCode). Mỗi loại gắn:
//  • danh sách trường tùy chỉnh (Mst_CustomColumnCampaignType → Mst_CampaignColumnConfig)
//    để cấu hình bố cục nhập liệu cho chiến dịch thuộc loại đó;
//  • danh sách phản hồi khách hàng (Mst_CustomerFeedBack) — các lựa chọn phản hồi
//    chuẩn khi agent ghi nhận kết quả gọi.
// Khi xóa loại chiến dịch, SkyCS chặn nếu đang có chiến dịch dùng loại đó.
// (11.BackEnd/V10/idn.SkyCS.Biz/Campaign.cs, `Mst_CampaignType_Get`/`_SaveX`;
//  models 12.Dev.Common/idn.SkyCS.Common/Models/Mst_CampaignType.cs,
//  Mst_CustomColumnCampaignType.cs, Mst_CustomerFeedBack.cs)

/// <summary>Loại chiến dịch (Mst_CampaignType) — phân loại chiến dịch gọi ra.</summary>
public class CampaignType : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";              // CampaignTypeCode — mã loại chiến dịch
    public string Name { get; set; } = "";              // CampaignTypeName — tên loại chiến dịch
    public string? Description { get; set; }             // CampaignTypeDesc — mô tả
    public string? Remark { get; set; }                  // Remark
    public bool IsActive { get; set; } = true;           // FlagActive
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";          // LogLUBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public List<CampaignTypeColumn> Columns { get; set; } = [];
    public List<CampaignFeedback> Feedbacks { get; set; } = [];

    // ── tính toán ────────────────────────────────────────────────────
    public int ColumnCount => Columns.Count;
    public int RequiredColumnCount => Columns.Count(c => c.IsRequired);
    public int FeedbackCount => Feedbacks.Count;
}

/// <summary>
/// Trường tùy chỉnh của loại chiến dịch (Mst_CustomColumnCampaignType).
/// Mỗi dòng gắn 1 trường cấu hình (Mst_CampaignColumnConfig) vào loại chiến dịch,
/// kèm thứ tự hiển thị (Idx) và cờ bắt buộc (FlagRequired).
/// </summary>
public class CampaignTypeColumn : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int CampaignTypeId { get; set; }
    public string Code { get; set; } = "";              // CampaignColCfgCodeSys — mã trường cấu hình
    public string Name { get; set; } = "";              // CampaignColCfgName — tên trường
    public SurveyFieldType FieldType { get; set; } = SurveyFieldType.Text;  // CampaignColCfgDataType
    public int Order { get; set; }                       // Idx — thứ tự hiển thị
    public bool IsRequired { get; set; }                 // FlagRequired
    public bool IsActive { get; set; } = true;           // FlagActive

    public CampaignType CampaignType { get; set; } = null!;
}

/// <summary>
/// Phản hồi khách hàng chuẩn của loại chiến dịch (Mst_CustomerFeedBack).
/// Là các lựa chọn phản hồi agent chọn khi ghi nhận kết quả gọi.
/// </summary>
public class CampaignFeedback : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int CampaignTypeId { get; set; }
    public string Code { get; set; } = "";              // CusFBCode — mã phản hồi
    public string Name { get; set; } = "";              // CusFBName — tên phản hồi
    public bool IsActive { get; set; } = true;           // FlagActive

    public CampaignType CampaignType { get; set; } = null!;
}
// ── Loại giấy tờ định danh (Mst_GovIDType) ───────────────────────────
// Theo SkyCS: "loại giấy tờ định danh" (Mst_GovIDType) là master data quy
// định các loại giấy tờ tùy thân/định danh dùng khi ghi nhận thông tin
// khách hàng (CMTND/Thẻ căn cước, Hộ chiếu, Bằng lái xe, Giấy tờ khác…).
// Mỗi loại có mã (GovIDType), tên hiển thị (GovIDTypeName), ghi chú và
// trạng thái hoạt động (FlagActive). Hồ sơ khách hàng/người nộp thuế tham
// chiếu loại này qua trường PresentIDType.
// (11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_GovIDType_Get`;
//  model 12.Dev.Common/idn.SkyCS.Common/Models/Mst_GovIDType.cs;
//  controller 13.ClientGate/V20/idn.SkyCS.WebAPI/Controllers/MstGovIDTypeController.cs;
//  cột xác nhận qua TblMst_GovIDType trong Const.Main.cs)

/// <summary>Loại giấy tờ định danh (Mst_GovIDType) — dùng cho hồ sơ khách hàng.</summary>
public class GovIDType : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";              // GovIDType — mã loại giấy tờ
    public string Name { get; set; } = "";              // GovIDTypeName — tên loại giấy tờ
    public string? Remark { get; set; }                 // Remark
    public bool IsActive { get; set; } = true;          // FlagActive
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";         // LogLUBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

// ── Quốc gia (Mst_Country) ───────────────────────────
// Theo SkyCS: "quốc gia" (Mst_Country) là master data nền của Trung tâm
// khách hàng — danh mục quốc gia/vùng lãnh thổ kèm mã bưu chính mặc định.
// Được tham chiếu bởi địa chỉ hành chính (Mst_Province.CountryCode) và
// hồ sơ khách hàng. Mỗi quốc gia có mã (CountryCode), tên (CountryName),
// mã bưu chính (PostCode) và trạng thái hoạt động (FlagActive).
// (11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_Country_Get`;
//  model 12.Dev.Common/idn.SkyCS.Common/Models/Mst_Country.cs;
//  controller 13.ClientGate/V20/idn.SkyCS.WebAPI/Controllers/MstCountryController.cs;
//  cột xác nhận qua TblMst_Country trong Const.Main.cs)

/// <summary>Quốc gia (Mst_Country) — danh mục quốc gia/vùng lãnh thổ, có mã bưu chính.</summary>
public class Country : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";              // CountryCode — mã quốc gia
    public string Name { get; set; } = "";              // CountryName — tên quốc gia
    public string? PostCode { get; set; }               // PostCode — mã bưu chính mặc định
    public bool IsActive { get; set; } = true;          // FlagActive
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";         // LogLUBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // ── tính toán ────────────────────
    /// <summary>Mã bưu chính hiển thị (dùng cho view).</summary>
    public string PostCodeText => string.IsNullOrWhiteSpace(PostCode) ? "—" : PostCode!;
}

// ── Mức đánh giá hài lòng (Mst_SatisfactionRating) ───────────────────
// Theo SkyCS: "mức đánh giá hài lòng" (Mst_SatisfactionRating) là master data
// quy định các MỨC độ hài lòng chuẩn (Rất hài lòng / Hài lòng / Bình thường /
// Không hài lòng / Rất không hài lòng…) để agent chọn khi ghi nhận kết quả
// chăm sóc. eTicket gắn mức này qua ET_Ticket.SatRatingCode ("Mã Đánh giá mức
// độ hài lòng") và chiến dịch khảo sát cũng tham chiếu. Mỗi mức có mã
// (SatRatingCode), tên (SatRatingName), thứ tự hiển thị (OrdIdx), ghi chú và
// trạng thái hoạt động (FlagActive).
// (11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Mst_SatisfactionRating_Get`/
//  `Mst_SatisfactionRating_CheckDB`; model 12.Dev.Common/idn.SkyCS.Common/
//  Models/Mst_SatisfactionRating.cs; controller 13.ClientGate/V20/idn.SkyCS.WebAPI/
//  Controllers/MstSatisfactionRatingController.cs)

/// <summary>Mức đánh giá hài lòng (Mst_SatisfactionRating) — mức độ hài lòng chuẩn.</summary>
public class SatisfactionRating : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";              // SatRatingCode — mã mức đánh giá
    public string Name { get; set; } = "";              // SatRatingName — tên mức đánh giá
    public int Order { get; set; }                       // OrdIdx — thứ tự hiển thị
    public string? Remark { get; set; }                  // Remark — ghi chú
    public bool IsActive { get; set; } = true;           // FlagActive
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";          // LogLUBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // ── tính toán ────────────────────
    /// <summary>Mức đánh giá tích cực (Rất hài lòng / Hài lòng) — dùng cho thống kê.</summary>
    public bool IsPositive => Code.ToUpperInvariant() is "SAT5" or "SAT4" or "RAT5" or "RAT4";
}

// ── Loại kênh (Mst_ChannelType) ──────────────────────
// Theo SkyCS: "loại kênh" (Mst_ChannelType) là master data của OmniChannel —
// danh mục các LOẠI kênh liên lạc đa kênh (email, sms, zalo…). Mỗi loại kênh
// có mã (ChannelType), tên hiển thị (ChannelTypeName) và trạng thái hoạt động
// (FlagActive). Mst_Channel gắn loại kênh cho eTicket (ChannelTypeETicket) và
// cho OTP (ChannelTypeOTP); các bảng con Mst_ChannelEmail/SMS/Zalo tham chiếu
// loại kênh để cấu hình gửi/nhận.
// (11.BackEnd/V10/idn.SkyCS.Biz/OmniChannel/OminiChannel.cs, `Mst_ChannelType_Get`;
//  model 12.Dev.Common/idn.SkyCS.Common/Models/OminiChannel/Mst_ChannelType.cs;
//  controller 13.ClientGate/V20/idn.SkyCS.WebAPI/Controllers/MstChannelTypeController.cs;
//  cột xác nhận qua TblMst_ChannelType trong Const.Main.1.cs;
//  seed thật 20230619.z11.UpdDB.80.OmniChannel.sql: EMAIL/SMS/ZALO)

/// <summary>Loại kênh (Mst_ChannelType) — danh mục loại kênh liên lạc đa kênh.</summary>
public class ChannelType : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";              // ChannelType — mã loại kênh
    public string Name { get; set; } = "";              // ChannelTypeName — tên loại kênh
    public bool IsActive { get; set; } = true;          // FlagActive
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";         // LogLUBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // ── tính toán ────────────────────
    /// <summary>Biểu tượng theo loại kênh (dựa vào mã).</summary>
    public string Icon => Code.ToUpperInvariant() switch
    {
        "EMAIL" => "bi-envelope",
        "SMS" => "bi-chat-left-text",
        "ZALO" => "bi-chat-dots",
        "CALL" => "bi-telephone",
        "FACEBOOK" => "bi-facebook",
        _ => "bi-broadcast"
    };
}

// ── Quản lý thông báo (Mst_NotifyType / Mst_ManageNotify / ────────────
//    Map_UserInNotifyType) ────────────────────────────
// Theo SkyCS: hệ thống thông báo nội bộ gồm 3 bảng:
//  • Mst_NotifyType — danh mục LOẠI thông báo (mã + mô tả + cờ bật mặc định),
//    ví dụ "Tạo hóa đơn", "Duyệt đơn hàng", "Thông báo khác".
//  • Mst_ManageNotify — danh sách NGƯỜI QUẢN LÝ nhận thông báo (Sys_User.UserCode
//    + tên hiển thị). Khi tạo user mới, hệ thống tự thêm vào bảng này
//    (`Mst_ManageNotify_CreateX` trong System.cs).
//  • Map_UserInNotifyType — MA TRẬN phân quyền thông báo: mỗi cặp
//    (UserCode × NotifyType) có cờ FlagNotify bật/tắt — quyết định user đó có
//    nhận loại thông báo tương ứng hay không.
// (11.BackEnd/V10/idn.SkyCS.Biz/System.cs, `Mst_ManageNotify_CreateX`/`_DeleteX`;
//  Delete/Delete.Master.Cloud.cs, `Mst_NotifyType_Get`/`Mst_ManageNotify_Get`/
//  `Map_UserInNotifyType_Get`; models 12.Dev.Common/idn.SkyCS.Common/Models/
//  Mst_NotifyType.cs, Mst_ManageNotify.cs, RQ_/RT_Map_UserInNotifyType.cs;
//  schema 11.BackEnd/V10/05.Refs.Biz/Migrate/20200806.z11.CreateTable.MapNotify.sql)

/// <summary>Loại thông báo (Mst_NotifyType) — danh mục loại thông báo nội bộ.</summary>
public class NotifyType : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";              // NotifyType — mã loại thông báo
    public string? Description { get; set; }             // NotifyDesc — mô tả loại thông báo
    public bool DefaultActive { get; set; } = true;      // DefaultActive — bật mặc định cho user mới
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";          // LogLUBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // ── tính toán ────────────────────
    public string DisplayName => string.IsNullOrWhiteSpace(Description) ? Code : Description!;
}

/// <summary>Người quản lý nhận thông báo (Mst_ManageNotify) — 1 user trong danh sách nhận thông báo.</summary>
public class NotifyManager : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string UserCode { get; set; } = "";          // UserCode — mã user (Sys_User.UserCode)
    public string? UserName { get; set; }                // UserName — tên hiển thị
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";          // LogLUBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // ── tính toán ────────────────────
    public string DisplayName => string.IsNullOrWhiteSpace(UserName) ? UserCode : UserName!;
}

/// <summary>
/// Ma trận phân quyền thông báo (Map_UserInNotifyType) — mỗi dòng = 1 cặp
/// (UserCode × NotifyType) với cờ FlagNotify bật/tắt.
/// </summary>
public class NotifySubscription : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string UserCode { get; set; } = "";          // UserCode — mã user (Mst_ManageNotify.UserCode)
    public string NotifyTypeCode { get; set; } = "";    // NotifyType — mã loại thông báo (Mst_NotifyType.NotifyType)
    public bool FlagNotify { get; set; } = true;         // FlagNotify — có nhận loại thông báo này không
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";          // LogLUBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

// ── Gán loại phiếu cho phòng ban (Map_TicketTypeDepartment) ───────────
// Theo SkyCS: bảng map (Map_TicketTypeDepartment) quy định PHÒNG BAN nào
// phụ trách xử lý LOẠI PHIẾU nào. Mỗi dòng = 1 cặp (TicketType ↔ DepartmentCode)
// theo tổ chức (OrgID), kèm thứ tự hiển thị (Idx) và trạng thái (FlagActive).
// Khi tạo eTicket, hệ thống dò bảng này để định tuyến phiếu về đúng phòng ban
// theo phân loại nghiệp vụ (Mst_TicketType) — là nền cho phân bổ phiếu tự động
// (Mst_EstablishAllocateETicket.DepartmentCode).
// (11.BackEnd/V10/idn.SkyCS.Biz/Master.cs, `Map_TicketTypeDepartment_Get`/
//  `Map_TicketTypeDepartment_SaveX`; model 12.Dev.Common/idn.SkyCS.Common/
//  Models/Map_TicketTypeDepartment.cs; controller 13.ClientGate/V20/
//  idn.SkyCS.WebAPI/Controllers/MapTicketTypeDepartmentController.cs)

/// <summary>
/// Gán loại phiếu cho phòng ban (Map_TicketTypeDepartment) — phòng ban phụ trách 1 loại phiếu.
/// </summary>
public class TicketTypeDepartment : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string TicketTypeCode { get; set; } = "";    // TicketType — mã phân loại nghiệp vụ (Mst_TicketType)
    public string DepartmentCode { get; set; } = "";    // DepartmentCode — mã phòng ban (Mst_Department)
    public int Order { get; set; }                       // Idx — thứ tự hiển thị
    public bool IsActive { get; set; } = true;           // FlagActive
    public DateTime CreatedAt { get; set; } = DateTime.Now;   // LogLUDTimeUTC
    public string CreatedBy { get; set; } = "";          // LogLUBy
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // ── tính toán ────────────────────
    /// <summary>Mô tả ngắn cặp gán (dùng cho view).</summary>
    public string PairText => $"{TicketTypeCode} → {DepartmentCode}";
}
