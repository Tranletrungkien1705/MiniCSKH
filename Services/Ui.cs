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

    // ── Lịch làm việc SLA (Mst_SLAWorkingDay / Mst_SLAHoliday) ──
    public static string ShiftName(SlaShift s) => s == SlaShift.Morning ? "Buổi sáng" : "Buổi chiều";
    public static string ShiftColor(SlaShift s) => s == SlaShift.Morning ? "warning" : "info";
    public static string ShiftIcon(SlaShift s) => s == SlaShift.Morning ? "bi-sunrise" : "bi-sunset";
    public static string ShiftBadge(SlaShift s) =>
        $"<span class='badge bg-{ShiftColor(s)}-subtle text-{ShiftColor(s)} border-{ShiftColor(s)}-subtle'><i class='bi {ShiftIcon(s)} me-1'></i>{ShiftName(s)}</span>";
    public static string WeekdayName(int code) => code switch
    {
        1 => "Chủ nhật", 2 => "Thứ hai", 3 => "Thứ ba", 4 => "Thứ tư",
        5 => "Thứ năm", 6 => "Thứ sáu", 7 => "Thứ bảy", _ => $"Thứ {code}"
    };

    // ── Kênh liên hệ (Mst_ContactChannel) ──
    public static string ContactUseTypeName(CatalogUseType t) => t switch
    {
        CatalogUseType.Type1 => "Chỉ agent", CatalogUseType.Type2 => "Agent & khách",
        CatalogUseType.Type3 => "Chỉ khách", _ => t.ToString()
    };
    public static string ContactUseTypeColor(CatalogUseType t) => t switch
    {
        CatalogUseType.Type1 => "info", CatalogUseType.Type2 => "success",
        CatalogUseType.Type3 => "warning", _ => "secondary"
    };
    public static string ContactUseTypeBadge(CatalogUseType t) =>
        $"<span class='badge bg-{ContactUseTypeColor(t)}-subtle text-{ContactUseTypeColor(t)} border-{ContactUseTypeColor(t)}-subtle'>{ContactUseTypeName(t)}</span>";

    // ── Loại phiếu tùy chỉnh (Mst_TicketCustomType) ──
    public static string CustomTypeUseTypeName(CatalogUseType t) => t switch
    {
        CatalogUseType.Type1 => "Chỉ agent", CatalogUseType.Type2 => "Agent & khách",
        CatalogUseType.Type3 => "Chỉ khách", _ => t.ToString()
    };
    public static string CustomTypeUseTypeColor(CatalogUseType t) => t switch
    {
        CatalogUseType.Type1 => "info", CatalogUseType.Type2 => "success",
        CatalogUseType.Type3 => "warning", _ => "secondary"
    };
    public static string CustomTypeUseTypeBadge(CatalogUseType t) =>
        $"<span class='badge bg-{CustomTypeUseTypeColor(t)}-subtle text-{CustomTypeUseTypeColor(t)} border-{CustomTypeUseTypeColor(t)}-subtle'>{CustomTypeUseTypeName(t)}</span>";

    // ── Điều kiện áp dụng SLA (Mst_SLATicketType/SLACustomerCN/…) ──
    public static string SlaScopeKindName(SlaScopeKind k) => k switch
    {
        SlaScopeKind.TicketType => "Loại phiếu",
        SlaScopeKind.TicketCustomType => "Loại phiếu tùy chỉnh",
        SlaScopeKind.CustomerCN => "Khách hàng cá nhân",
        SlaScopeKind.CustomerGroupCN => "Nhóm khách cá nhân",
        SlaScopeKind.CustomerDN => "Khách hàng doanh nghiệp",
        SlaScopeKind.CustomerGroupDN => "Nhóm khách doanh nghiệp",
        _ => k.ToString()
    };
    public static string SlaScopeKindColor(SlaScopeKind k) => k switch
    {
        SlaScopeKind.TicketType => "primary",
        SlaScopeKind.TicketCustomType => "info",
        SlaScopeKind.CustomerCN => "success",
        SlaScopeKind.CustomerGroupCN => "warning",
        SlaScopeKind.CustomerDN => "success",
        SlaScopeKind.CustomerGroupDN => "warning",
        _ => "secondary"
    };
    public static string SlaScopeKindIcon(SlaScopeKind k) => k switch
    {
        SlaScopeKind.TicketType => "bi-diagram-3",
        SlaScopeKind.TicketCustomType => "bi-ticket-perforated",
        SlaScopeKind.CustomerCN => "bi-person",
        SlaScopeKind.CustomerGroupCN => "bi-people",
        SlaScopeKind.CustomerDN => "bi-building",
        SlaScopeKind.CustomerGroupDN => "bi-people-fill",
        _ => "bi-tag"
    };
    public static string SlaScopeKindBadge(SlaScopeKind k) =>
        $"<span class='badge bg-{SlaScopeKindColor(k)}-subtle text-{SlaScopeKindColor(k)} border-{SlaScopeKindColor(k)}-subtle'><i class='bi {SlaScopeKindIcon(k)} me-1'></i>{SlaScopeKindName(k)}</span>";

    // ── Người nộp thuế (Mst_NNT) ──
    public static string TctStatusName(TctStatus s) => s switch
    {
        TctStatus.Registered => "TCT xác nhận đăng ký", TctStatus.Cancelled => "TCT xác nhận ngừng",
        _ => "Chưa đăng ký"
    };
    public static string TctStatusColor(TctStatus s) => s switch
    {
        TctStatus.Registered => "success", TctStatus.Cancelled => "danger", _ => "secondary"
    };
    public static string TctStatusIcon(TctStatus s) => s switch
    {
        TctStatus.Registered => "bi-patch-check", TctStatus.Cancelled => "bi-x-octagon", _ => "bi-hourglass"
    };
    public static string TctStatusBadge(TctStatus s) =>
        $"<span class='badge bg-{TctStatusColor(s)}-subtle text-{TctStatusColor(s)} border-{TctStatusColor(s)}-subtle'><i class='bi {TctStatusIcon(s)} me-1'></i>{TctStatusName(s)}</span>";

    // ── Loại chiến dịch (Mst_CampaignType) ──
    public static string CampaignTypeActiveBadge(bool active) => active
        ? "<span class='badge bg-success-subtle text-success border-success-subtle'>Đang dùng</span>"
        : "<span class='badge bg-secondary-subtle text-secondary border-secondary-subtle'>Ngừng dùng</span>";

    // ── Loại giấy tờ định danh (Mst_GovIDType) ──
    public static string GovIDTypeActiveBadge(bool active) => active
        ? "<span class='badge bg-success-subtle text-success border-success-subtle'>Đang dùng</span>"
        : "<span class='badge bg-secondary-subtle text-secondary border-secondary-subtle'>Ngừng dùng</span>";
    public static string GovIDTypeIcon(string code) => code.ToUpperInvariant() switch
    {
        "CMTND_THECANCUOC" => "bi-person-vcard",
        "HOCHIEU" => "bi-passport",
        "BANGLAIXE" => "bi-car-front",
        _ => "bi-card-text"
    };

    // ── Quốc gia (Mst_Country) ──
    public static string CountryActiveBadge(bool active) => active
        ? "<span class='badge bg-success-subtle text-success border-success-subtle'>Đang dùng</span>"
        : "<span class='badge bg-secondary-subtle text-secondary border-secondary-subtle'>Ngừng dùng</span>";
    public static string CountryIcon(string code) => code.ToUpperInvariant() switch
    {
        "VN" => "bi-flag",
        "US" => "bi-globe-americas",
        "JP" => "bi-globe-asia-australia",
        "KR" => "bi-globe-asia-australia",
        "CN" => "bi-globe-asia-australia",
        "SG" => "bi-globe-asia-australia",
        _ => "bi-globe"
    };

    // ── Mức đánh giá hài lòng (Mst_SatisfactionRating) ──
    public static string SatRatingActiveBadge(bool active) => active
        ? "<span class='badge bg-success-subtle text-success border-success-subtle'>Đang dùng</span>"
        : "<span class='badge bg-secondary-subtle text-secondary border-secondary-subtle'>Ngừng dùng</span>";
    /// <summary>Biểu tượng theo mức đánh giá (dựa vào mã).</summary>
    public static string SatRatingIcon(string code) => code.ToUpperInvariant() switch
    {
        "SAT5" or "RAT5" => "bi-emoji-laughing",
        "SAT4" or "RAT4" => "bi-emoji-smile",
        "SAT3" or "RAT3" => "bi-emoji-neutral",
        "SAT2" or "RAT2" => "bi-emoji-frown",
        "SAT1" or "RAT1" => "bi-emoji-angry",
        _ => "bi-star"
    };
    /// <summary>Màu badge theo mức đánh giá (dựa vào mã).</summary>
    public static string SatRatingBadge(string code) => code.ToUpperInvariant() switch
    {
        "SAT5" or "RAT5" => "<span class='badge bg-success-subtle text-success border-success-subtle'>Rất hài lòng</span>",
        "SAT4" or "RAT4" => "<span class='badge bg-info-subtle text-info border-info-subtle'>Hài lòng</span>",
        "SAT3" or "RAT3" => "<span class='badge bg-warning-subtle text-warning border-warning-subtle'>Bình thường</span>",
        "SAT2" or "RAT2" => "<span class='badge bg-danger-subtle text-danger border-danger-subtle'>Không hài lòng</span>",
        "SAT1" or "RAT1" => "<span class='badge bg-danger text-white'>Rất không hài lòng</span>",
        _ => "<span class='badge bg-light text-dark border'>Khác</span>"
    };

    // ── Loại kênh (Mst_ChannelType) ──
    public static string ChannelTypeActiveBadge(bool active) => active
        ? "<span class='badge bg-success-subtle text-success border-success-subtle'>Đang dùng</span>"
        : "<span class='badge bg-secondary-subtle text-secondary border-secondary-subtle'>Ngừng dùng</span>";
    /// <summary>Biểu tượng theo loại kênh (dựa vào mã).</summary>
    public static string ChannelTypeIcon(string code) => code.ToUpperInvariant() switch
    {
        "EMAIL" => "bi-envelope",
        "SMS" => "bi-chat-left-text",
        "ZALO" => "bi-chat-dots",
        "CALL" => "bi-telephone",
        "FACEBOOK" => "bi-facebook",
        _ => "bi-broadcast"
    };

    // ── Quản lý thông báo (Mst_NotifyType / Mst_ManageNotify / Map_UserInNotifyType) ──
    public static string NotifyActiveBadge(bool active) => active
        ? "<span class='badge bg-success-subtle text-success border-success-subtle'>Bật mặc định</span>"
        : "<span class='badge bg-secondary-subtle text-secondary border-secondary-subtle'>Tắt mặc định</span>";
    public static string NotifyFlagBadge(bool on) => on
        ? "<span class='badge bg-success-subtle text-success border-success-subtle'><i class='bi bi-bell-fill me-1'></i>Nhận</span>"
        : "<span class='badge bg-secondary-subtle text-secondary border-secondary-subtle'><i class='bi bi-bell-slash me-1'></i>Không nhận</span>";
    /// <summary>Biểu tượng theo loại thông báo (dựa vào mã).</summary>
    public static string NotifyTypeIcon(string code) => code.ToUpperInvariant() switch
    {
        "INVOICE_CREATE" => "bi-receipt",
        "INVOICEISSUED" => "bi-file-earmark-check",
        "DLORDERAPPROVED" => "bi-bag-check",
        "SOORDERCREATED" => "bi-cart-plus",
        "SOORDERAPPROVED" => "bi-cart-check",
        "SRORDERCREATED" => "bi-basket",
        "SRORDERAPPROVED" => "bi-basket-fill",
        "CONTRACTSENT" => "bi-file-earmark-text",
        "OTHERS" => "bi-bell",
        _ => "bi-bell"
    };

    // ── Gán loại phiếu cho phòng ban (Map_TicketTypeDepartment) ──
    public static string TicketTypeDeptActiveBadge(bool active) => active
        ? "<span class='badge bg-success-subtle text-success border-success-subtle'>Đang dùng</span>"
        : "<span class='badge bg-secondary-subtle text-secondary border-secondary-subtle'>Ngừng dùng</span>";
    /// <summary>Badge mã phân loại nghiệp vụ (TicketType).</summary>
    public static string TicketTypeCodeBadge(string code) =>
        $"<span class='badge bg-primary-subtle text-primary border-primary-subtle'><i class='bi bi-diagram-3 me-1'></i>{code}</span>";
    /// <summary>Badge mã phòng ban (DepartmentCode).</summary>
    public static string DepartmentCodeBadge(string code) =>
        $"<span class='badge bg-info-subtle text-info border-info-subtle'><i class='bi bi-diagram-2 me-1'></i>{code}</span>";

    // ── Danh mục bài viết Knowledge Base (KB_Category) ──
    public static string KbCategoryActiveBadge(bool active) => active
        ? "<span class='badge bg-success-subtle text-success border-success-subtle'>Đang dùng</span>"
        : "<span class='badge bg-secondary-subtle text-secondary border-secondary-subtle'>Ngừng dùng</span>";
    public static string KbShareTypeName(KbShareType t) => t switch
    {
        KbShareType.Private => "Riêng tư", KbShareType.Branch => "Chi nhánh",
        KbShareType.CreditFund => "Quỹ TDND", KbShareType.Ho => "Nội bộ đơn vị",
        KbShareType.Kbtt => "Trụ sở chính", _ => t.ToString()
    };
    public static string KbShareTypeColor(KbShareType t) => t switch
    {
        KbShareType.Private => "secondary", KbShareType.Branch => "info",
        KbShareType.CreditFund => "warning", KbShareType.Ho => "primary",
        KbShareType.Kbtt => "success", _ => "secondary"
    };
    public static string KbShareTypeIcon(KbShareType t) => t switch
    {
        KbShareType.Private => "bi-lock", KbShareType.Branch => "bi-diagram-2",
        KbShareType.CreditFund => "bi-bank", KbShareType.Ho => "bi-building",
        KbShareType.Kbtt => "bi-buildings", _ => "bi-share"
    };
    public static string KbShareTypeBadge(KbShareType t) =>
        $"<span class='badge bg-{KbShareTypeColor(t)}-subtle text-{KbShareTypeColor(t)} border-{KbShareTypeColor(t)}-subtle'><i class='bi {KbShareTypeIcon(t)} me-1'></i>{KbShareTypeName(t)}</span>";
}
