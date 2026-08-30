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

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? DueAt { get; set; }          // hạn SLA
    public DateTime? FirstResponseAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public TicketCategory? Category { get; set; }
    public Agent? AssignedAgent { get; set; }
    public List<TicketComment> Comments { get; set; } = [];

    // ── tính toán ────────────────────────────────────────────────────
    public bool IsOpen => Status is not (TicketStatus.Resolved or TicketStatus.Closed or TicketStatus.Cancelled);
    public bool IsOverdue => IsOpen && DueAt.HasValue && DateTime.Now > DueAt.Value;
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

/// <summary>Phiếu khảo sát hài lòng (CSAT) sau khi xử lý ticket. Score 1-5; 0 = chưa trả lời.</summary>
public class SurveyResponse : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";      // mã link khảo sát công khai
    public int TicketId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public int Score { get; set; }              // 0 = chưa trả lời, 1-5 sao
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? RespondedAt { get; set; }
    public bool Responded => RespondedAt != null;
}
