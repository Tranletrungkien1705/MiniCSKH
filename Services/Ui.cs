using MiniCSKH.Models;

namespace MiniCSKH.Services;

/// <summary>Nhãn + badge dùng chung cho view.</summary>
public static class Ui
{
    public static string StatusName(TicketStatus s) => s switch
    {
        TicketStatus.New => "Mới", TicketStatus.InProgress => "Đang xử lý", TicketStatus.WaitingCustomer => "Chờ khách",
        TicketStatus.Resolved => "Đã giải quyết", TicketStatus.Closed => "Đã đóng", TicketStatus.Cancelled => "Đã hủy", _ => s.ToString()
    };
    public static string StatusColor(TicketStatus s) => s switch
    {
        TicketStatus.New => "primary", TicketStatus.InProgress => "info", TicketStatus.WaitingCustomer => "warning",
        TicketStatus.Resolved => "success", TicketStatus.Closed => "secondary", TicketStatus.Cancelled => "dark", _ => "secondary"
    };
    public static string StatusBadge(TicketStatus s) =>
        $"<span class='badge bg-{StatusColor(s)}-subtle text-{StatusColor(s)} border border-{StatusColor(s)}-subtle'>{StatusName(s)}</span>";

    public static string PriorityName(TicketPriority p) => p switch
    { TicketPriority.Low => "Thấp", TicketPriority.Normal => "Bình thường", TicketPriority.High => "Cao", TicketPriority.Urgent => "Khẩn", _ => p.ToString() };
    public static string PriorityColor(TicketPriority p) => p switch
    { TicketPriority.Low => "secondary", TicketPriority.Normal => "info", TicketPriority.High => "warning", TicketPriority.Urgent => "danger", _ => "secondary" };
    public static string PriorityBadge(TicketPriority p) =>
        $"<span class='badge bg-{PriorityColor(p)}-subtle text-{PriorityColor(p)} border border-{PriorityColor(p)}-subtle'>{PriorityName(p)}</span>";

    public static string ChannelName(Channel c) => c switch
    { Channel.Web => "Web", Channel.Email => "Email", Channel.Zalo => "Zalo", Channel.Phone => "Điện thoại", Channel.Facebook => "Facebook", _ => c.ToString() };
    public static string ChannelIcon(Channel c) => c switch
    { Channel.Web => "bi-globe", Channel.Email => "bi-envelope", Channel.Zalo => "bi-chat-dots", Channel.Phone => "bi-telephone", Channel.Facebook => "bi-facebook", _ => "bi-tag" };

    public static string OutcomeName(CallOutcome o) => o switch
    { CallOutcome.Answered => "Đã nghe", CallOutcome.Missed => "Nhỡ", CallOutcome.Voicemail => "Hộp thư", CallOutcome.Busy => "Bận", _ => o.ToString() };
    public static string OutcomeColor(CallOutcome o) => o switch
    { CallOutcome.Answered => "success", CallOutcome.Missed => "danger", CallOutcome.Voicemail => "warning", CallOutcome.Busy => "secondary", _ => "secondary" };
    public static string OutcomeBadge(CallOutcome o) =>
        $"<span class='badge bg-{OutcomeColor(o)}-subtle text-{OutcomeColor(o)} border border-{OutcomeColor(o)}-subtle'>{OutcomeName(o)}</span>";
    public static string DirName(CallDirection d) => d == CallDirection.Inbound ? "Gọi đến" : "Gọi đi";
    public static string DirIcon(CallDirection d) => d == CallDirection.Inbound ? "bi-telephone-inbound" : "bi-telephone-outbound";
}
