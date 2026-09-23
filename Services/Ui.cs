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

    // ── Campaign ─────────────────────
    public static string CampaignStatusName(CampaignStatus s) => s switch
    {
        CampaignStatus.Pending => "Chờ duyệt", CampaignStatus.Approved => "Đã duyệt", CampaignStatus.Started => "Đang chạy",
        CampaignStatus.Paused => "Tạm dừng", CampaignStatus.Finished => "Hoàn thành", CampaignStatus.Cancelled => "Đã hủy", _ => s.ToString()
    };
    public static string CampaignStatusColor(CampaignStatus s) => s switch
    {
        CampaignStatus.Pending => "secondary", CampaignStatus.Approved => "info", CampaignStatus.Started => "success",
        CampaignStatus.Paused => "warning", CampaignStatus.Finished => "primary", CampaignStatus.Cancelled => "dark", _ => "secondary"
    };
    public static string CampaignStatusBadge(CampaignStatus s) =>
        $"<span class='badge bg-{CampaignStatusColor(s)}-subtle text-{CampaignStatusColor(s)} border-{CampaignStatusColor(s)}-subtle'>{CampaignStatusName(s)}</span>";

    public static string CustStatusName(CampaignCustomerStatus s) => s switch
    {
        CampaignCustomerStatus.Pending => "Chưa gọi", CampaignCustomerStatus.Done => "Thành công", CampaignCustomerStatus.Failed => "Lỗi",
        CampaignCustomerStatus.NoAnswer => "Không nghe", CampaignCustomerStatus.CallAgain => "Hẹn gọi lại", CampaignCustomerStatus.DoNotCall => "Không liên hệ", _ => s.ToString()
    };
    public static string CustStatusColor(CampaignCustomerStatus s) => s switch
    {
        CampaignCustomerStatus.Pending => "secondary", CampaignCustomerStatus.Done => "success", CampaignCustomerStatus.Failed => "danger",
        CampaignCustomerStatus.NoAnswer => "warning", CampaignCustomerStatus.CallAgain => "info", CampaignCustomerStatus.DoNotCall => "dark", _ => "secondary"
    };
    public static string CustStatusBadge(CampaignCustomerStatus s) =>
        $"<span class='badge bg-{CustStatusColor(s)}-subtle text-{CustStatusColor(s)} border-{CustStatusColor(s)}-subtle'>{CustStatusName(s)}</span>";

    // ── Đánh giá phiếu (Rating) ──────
    public static string RateTypeName(RateType t) => t == RateType.Rate ? "Đánh giá" : "Kiểm soát";
    public static string RateStatusName(RateStatus s) => s switch
    { RateStatus.None => "Chưa đánh giá", RateStatus.Rated => "Đã đánh giá", RateStatus.Reviewed => "Đã kiểm soát", _ => s.ToString() };
    public static string RateResultName(RateResult r) => r switch
    { RateResult.Satisfied => "Hài lòng", RateResult.Neutral => "Bình thường", RateResult.Unsatisfied => "Không hài lòng", _ => r.ToString() };
    public static string RateResultColor(RateResult r) => r switch
    { RateResult.Satisfied => "success", RateResult.Neutral => "warning", RateResult.Unsatisfied => "danger", _ => "secondary" };
    public static string RateResultBadge(RateResult r) =>
        $"<span class='badge bg-{RateResultColor(r)}-subtle text-{RateResultColor(r)} border-{RateResultColor(r)}-subtle'>{RateResultName(r)}</span>";

    // ── Mẫu khảo sát hài lòng (SurveyForm) ──
    public static string SurveyFieldTypeName(SurveyFieldType t) => t switch
    {
        SurveyFieldType.Text => "Văn bản", SurveyFieldType.Number => "Số", SurveyFieldType.Rating => "Chấm điểm",
        SurveyFieldType.SingleChoice => "Chọn 1", SurveyFieldType.MultiChoice => "Chọn nhiều", SurveyFieldType.Date => "Ngày", _ => t.ToString()
    };
    public static string SurveyFieldTypeIcon(SurveyFieldType t) => t switch
    {
        SurveyFieldType.Text => "bi-textarea-t", SurveyFieldType.Number => "bi-123", SurveyFieldType.Rating => "bi-star",
        SurveyFieldType.SingleChoice => "bi-ui-radios", SurveyFieldType.MultiChoice => "bi-ui-checks", SurveyFieldType.Date => "bi-calendar3", _ => "bi-input-cursor"
    };

    // ── Cải tiến chất lượng dịch vụ (SvImprv) ──
    public static string SvImprvTypeName(SvImprvItemType t) => t switch
    {
        SvImprvItemType.Honorific => "Lời chào / kính ngữ", SvImprvItemType.DenyWord => "Từ cấm",
        SvImprvItemType.CallTalkTime => "Thời lượng gọi", SvImprvItemType.Audio => "Phân tích audio", _ => t.ToString()
    };
    public static string SvImprvTypeColor(SvImprvItemType t) => t switch
    {
        SvImprvItemType.Honorific => "success", SvImprvItemType.DenyWord => "danger",
        SvImprvItemType.CallTalkTime => "info", SvImprvItemType.Audio => "warning", _ => "secondary"
    };
    public static string SvImprvTypeIcon(SvImprvItemType t) => t switch
    {
        SvImprvItemType.Honorific => "bi-chat-heart", SvImprvItemType.DenyWord => "bi-slash-circle",
        SvImprvItemType.CallTalkTime => "bi-stopwatch", SvImprvItemType.Audio => "bi-soundwave", _ => "bi-tag"
    };
    public static string SvImprvTypeBadge(SvImprvItemType t) =>
        $"<span class='badge bg-{SvImprvTypeColor(t)}-subtle text-{SvImprvTypeColor(t)} border-{SvImprvTypeColor(t)}-subtle'>{SvImprvTypeName(t)}</span>";
}
