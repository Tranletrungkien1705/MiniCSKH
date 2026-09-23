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

    // ── Phân loại nghiệp vụ (Mst_TicketType) ──
    public static string BusinessTypeName(BusinessType b) => b switch
    {
        BusinessType.ETicket => "eTicket", BusinessType.Campaign => "Chiến dịch", _ => b.ToString()
    };
    public static string BusinessTypeColor(BusinessType b) => b switch
    {
        BusinessType.ETicket => "primary", BusinessType.Campaign => "info", _ => "secondary"
    };
    public static string BusinessTypeIcon(BusinessType b) => b switch
    {
        BusinessType.ETicket => "bi-ticket-detailed", BusinessType.Campaign => "bi-megaphone", _ => "bi-tag"
    };
    public static string BusinessTypeBadge(BusinessType b) =>
        $"<span class='badge bg-{BusinessTypeColor(b)}-subtle text-{BusinessTypeColor(b)} border-{BusinessTypeColor(b)}-subtle'>{BusinessTypeName(b)}</span>";

    // ── Trung tâm khách hàng (Mst_Customer) ──
    public static string CustomerTypeName(CustomerType t) => t switch
    { CustomerType.Individual => "Cá nhân", CustomerType.Business => "Doanh nghiệp", _ => t.ToString() };
    public static string CustomerTypeColor(CustomerType t) => t switch
    { CustomerType.Individual => "info", CustomerType.Business => "primary", _ => "secondary" };
    public static string CustomerTypeIcon(CustomerType t) => t switch
    { CustomerType.Individual => "bi-person", CustomerType.Business => "bi-building", _ => "bi-tag" };
    public static string CustomerTypeBadge(CustomerType t) =>
        $"<span class='badge bg-{CustomerTypeColor(t)}-subtle text-{CustomerTypeColor(t)} border-{CustomerTypeColor(t)}-subtle'>{CustomerTypeName(t)}</span>";

    public static string PartnerTypeName(PartnerType p) => p switch
    { PartnerType.Customer => "Khách hàng", PartnerType.Supplier => "Nhà cung cấp", PartnerType.Both => "Cả hai", _ => p.ToString() };

    // ── Thiết lập phân bổ phiếu tự động (Mst_EstablishAllocateETicket) ──
    public static string AllocateModeName(AllocateRule r) => r switch
    {
        { AssignAgent: false } => "Phân về phòng ban",
        { AllocateEven: true } => "Chia đều cho agent",
        _ => "Gán agent theo thứ tự"
    };
    public static string AllocateModeColor(AllocateRule r) => r switch
    {
        { AssignAgent: false } => "secondary",
        { AllocateEven: true } => "success",
        _ => "info"
    };
    public static string AllocateModeBadge(AllocateRule r) =>
        $"<span class='badge bg-{AllocateModeColor(r)}-subtle text-{AllocateModeColor(r)} border-{AllocateModeColor(r)}-subtle'>{AllocateModeName(r)}</span>";

    // ── Thiết lập nhắc nhở phiếu (Mst_EstablishRemindETicket) ──
    public static string RemindChannelName(RemindChannel c) => c switch
    {
        RemindChannel.System => "Hệ thống", RemindChannel.Email => "Email",
        RemindChannel.Sms => "SMS", RemindChannel.Zalo => "Zalo", _ => c.ToString()
    };
    public static string RemindChannelColor(RemindChannel c) => c switch
    {
        RemindChannel.System => "primary", RemindChannel.Email => "info",
        RemindChannel.Sms => "warning", RemindChannel.Zalo => "success", _ => "secondary"
    };
    public static string RemindChannelIcon(RemindChannel c) => c switch
    {
        RemindChannel.System => "bi-bell", RemindChannel.Email => "bi-envelope",
        RemindChannel.Sms => "bi-chat-left-text", RemindChannel.Zalo => "bi-chat-dots", _ => "bi-tag"
    };
    public static string RemindChannelBadge(RemindChannel c) =>
        $"<span class='badge bg-{RemindChannelColor(c)}-subtle text-{RemindChannelColor(c)} border-{RemindChannelColor(c)}-subtle'><i class='bi {RemindChannelIcon(c)} me-1'></i>{RemindChannelName(c)}</span>";

    // ── Danh mục phiếu (Mst_TicketStatus/TicketPriority/TicketSource/ReceptionChannel) ──
    public static string CatalogKindName(TicketCatalogKind k) => k switch
    {
        TicketCatalogKind.Status => "Trạng thái phiếu", TicketCatalogKind.Priority => "Mức ưu tiên",
        TicketCatalogKind.Source => "Nguồn phiếu", TicketCatalogKind.ReceptionChannel => "Kênh tiếp nhận", _ => k.ToString()
    };
    public static string CatalogKindColor(TicketCatalogKind k) => k switch
    {
        TicketCatalogKind.Status => "primary", TicketCatalogKind.Priority => "warning",
        TicketCatalogKind.Source => "info", TicketCatalogKind.ReceptionChannel => "success", _ => "secondary"
    };
    public static string CatalogKindIcon(TicketCatalogKind k) => k switch
    {
        TicketCatalogKind.Status => "bi-flag", TicketCatalogKind.Priority => "bi-exclamation-triangle",
        TicketCatalogKind.Source => "bi-signpost-split", TicketCatalogKind.ReceptionChannel => "bi-inboxes", _ => "bi-tag"
    };
    public static string CatalogKindBadge(TicketCatalogKind k) =>
        $"<span class='badge bg-{CatalogKindColor(k)}-subtle text-{CatalogKindColor(k)} border-{CatalogKindColor(k)}-subtle'><i class='bi {CatalogKindIcon(k)} me-1'></i>{CatalogKindName(k)}</span>";

    public static string CatalogUseTypeName(CatalogUseType t) => t switch
    {
        CatalogUseType.Type1 => "Loại 1 (TYPE1)", CatalogUseType.Type2 => "Loại 2 (TYPE2)",
        CatalogUseType.Type3 => "Loại 3 (TYPE3)", _ => t.ToString()
    };

    // ── Phòng ban (Mst_Department) ──
    public static string DepartmentLevelName(int level) => level <= 1 ? "Cấp 1" : $"Cấp {level}";
    public static string DepartmentLevelColor(int level) => level <= 1 ? "primary" : level == 2 ? "info" : "secondary";
    public static string DepartmentLevelBadge(int level) =>
        $"<span class='badge bg-{DepartmentLevelColor(level)}-subtle text-{DepartmentLevelColor(level)} border-{DepartmentLevelColor(level)}-subtle'>{DepartmentLevelName(level)}</span>";

    // ── Điều khoản thanh toán (Mst_PaymentTerm) ──
    public static string PTTypeName(PTType t) => t switch
    { PTType.Sale => "Bán ra", PTType.Purchase => "Mua vào", _ => t.ToString() };
    public static string PTTypeColor(PTType t) => t switch
    { PTType.Sale => "success", PTType.Purchase => "info", _ => "secondary" };
    public static string PTTypeIcon(PTType t) => t switch
    { PTType.Sale => "bi-cart-check", PTType.Purchase => "bi-bag-check", _ => "bi-tag" };
    public static string PTTypeBadge(PTType t) =>
        $"<span class='badge bg-{PTTypeColor(t)}-subtle text-{PTTypeColor(t)} border-{PTTypeColor(t)}-subtle'><i class='bi {PTTypeIcon(t)} me-1'></i>{PTTypeName(t)}</span>";

    // ── Vùng thị trường (Mst_Area) ──
    public static string AreaLevelName(int level) => level <= 1 ? "Cấp 1" : $"Cấp {level}";
    public static string AreaLevelColor(int level) => level <= 1 ? "primary" : level == 2 ? "info" : "secondary";
    public static string AreaLevelBadge(int level) =>
        $"<span class='badge bg-{AreaLevelColor(level)}-subtle text-{AreaLevelColor(level)} border-{AreaLevelColor(level)}-subtle'>{AreaLevelName(level)}</span>";

    // ── Thẻ (Mst_Tag) ──
    public static string TagActiveBadge(bool active) => active
        ? "<span class='badge bg-success-subtle text-success border-success-subtle'>Đang dùng</span>"
        : "<span class='badge bg-secondary-subtle text-secondary border-secondary-subtle'>Ngừng dùng</span>";

    // ── Người nhận thông báo phiếu (Mst_EstablishReceiveNotifyETicket) ──
    public static string AgentBadge(string code, string? name) =>
        $"<span class='badge bg-primary-subtle text-primary border-primary-subtle'><i class='bi bi-person-badge me-1'></i>{(string.IsNullOrWhiteSpace(name) ? code : name)}</span>";

    // ── Danh mục địa chỉ (Mst_Province/Mst_District/Mst_Ward) ──
    public static string AddressLevelName(AddressLevel l) => l switch
    {
        AddressLevel.Province => "Tỉnh / Thành phố", AddressLevel.District => "Quận / Huyện",
        AddressLevel.Ward => "Phường / Xã", _ => l.ToString()
    };
    public static string AddressLevelColor(AddressLevel l) => l switch
    {
        AddressLevel.Province => "primary", AddressLevel.District => "info", AddressLevel.Ward => "success", _ => "secondary"
    };
    public static string AddressLevelIcon(AddressLevel l) => l switch
    {
        AddressLevel.Province => "bi-building", AddressLevel.District => "bi-geo-alt", AddressLevel.Ward => "bi-pin-map", _ => "bi-tag"
    };
    public static string AddressLevelBadge(AddressLevel l) =>
        $"<span class='badge bg-{AddressLevelColor(l)}-subtle text-{AddressLevelColor(l)} border-{AddressLevelColor(l)}-subtle'><i class='bi {AddressLevelIcon(l)} me-1'></i>{AddressLevelName(l)}</span>";
}
