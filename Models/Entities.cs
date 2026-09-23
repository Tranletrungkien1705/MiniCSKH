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
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string FirstResText => FormatMinutes(FirstResMinutes);
    public string ResolutionText => FormatMinutes(ResolutionMinutes);

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
