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
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? DueAt { get; set; }          // hạn SLA
    public DateTime? FirstResponseAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public TicketCategory? Category { get; set; }
    public Agent? AssignedAgent { get; set; }
    public SlaPolicy? SlaPolicy { get; set; }
    public List<TicketComment> Comments { get; set; } = [];

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
