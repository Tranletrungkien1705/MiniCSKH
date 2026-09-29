using Microsoft.EntityFrameworkCore;
using MiniCSKH.Data;
using MiniCSKH.Models;

namespace MiniCSKH.Services;

/// <summary>
/// Tầng nghiệp vụ cho cụm port 1:1 từ 2023.8A.SkyCS.
/// Đường trace nguồn: view .cshtml → Controllers/*Controller.cs → Services/*Manager.cs (biz).
/// Mỗi hàm dưới đây ghi rõ hàm nguồn tương ứng; guard nào bỏ thì nói thẳng ở chỗ đó.
/// </summary>
public interface ISkyCsService
{
    // ── SLA (nguồn Views/WorkSetting/SLAs.cshtml + EditSLA.cshtml) ──────────────────
    Task<List<Sla>> SlaListAsync();
    Task<Sla?> SlaGetAsync(int id);
    Task<(bool ok, string msg, int id)> SlaSaveAsync(Sla model);
    Task<(bool ok, string msg)> SlaDeleteAsync(int id);
    Task<List<BizHour>> BizHoursAsync();

    // ── Hội thoại (nguồn Views/Conversation/ConvEdit.cshtml) ────────────────────────
    Task<List<Conversation>> ConvListAsync(int? statusId, int? priorityId, int? picId, string? q);
    Task<Conversation?> ConvGetAsync(int id);
    Task<Conversation> ConvNewAsync(int currentUserId);
    Task<(bool ok, string msg, int id)> ConvSaveAsync(Conversation model, int currentUserId);
    Task<ConvEditLists> ConvEditListsAsync();

    // ── Công việc (nguồn Views/Work/AddWork.cshtml) ─────────────────────────────────
    Task<List<Work>> WorkListAsync(WorkStatuses? status, int? userId, string? q);
    Work WorkNew();
    Task<List<Work>> WorkOverlappedAsync(Work model);
    Task<(bool ok, string msg, int id, List<Work> overlapped)> WorkAddAsync(Work model, List<WorkFollow> followers, int currentUserId, bool confirm);
    Task<WorkAddLists> WorkAddListsAsync();
}

/// <summary>Các danh sách ViewBag mà ConvEdit.cshtml:3-17 đòi. Gom 1 record để controller khỏi 8 ViewBag rời.</summary>
public record ConvEditLists(List<ConvLookup> TypeList, List<ConvLookup> StatusList, List<ConvLookup> SourceList,
    List<ConvLookup> PriorityList, List<ConvLookup> ChannelList, List<Company> Companies, List<Team> TeamList,
    List<Product> ProductList, List<UserInTeam> UserInTeams, List<Sla> SlaList, List<Agent> UserList);

/// <summary>Danh sách ViewBag mà AddWork.cshtml:4-5 đòi (+ WorkUnit cho autocomplete AddWork.cshtml:282).</summary>
public record WorkAddLists(List<WorkType> WorkTypes, List<Product> ProductList, List<WorkUnit> WorkUnits, List<Agent> AllOrgUsers);

public class SkyCsService(AppDbContext db) : ISkyCsService
{
    /// <summary>
    /// Dựng mẫu LIKE cho ô tìm kiếm. PHẢI dùng <c>EF.Functions.Like</c>, KHÔNG dùng
    /// <c>string.Contains</c>: trên SQLite EF dịch Contains thành <c>instr()</c> — PHÂN BIỆT hoa/thường,
    /// còn nguồn chạy SQL Server với collation KHÔNG phân biệt (Sql_Latin1_General_CP1_CI_AS).
    /// Hậu quả nếu dùng Contains: build vẫn 0 Errors mà truy vấn trả 0 dòng — sai câm.
    /// GIỚI HẠN CÒN LẠI (khai báo rõ): LIKE của SQLite chỉ bỏ qua hoa/thường với ký tự ASCII,
    /// nên gõ "HÓA" vẫn không khớp "hóa". Muốn khớp trọn tiếng Việt phải bật ICU hoặc chuẩn hóa
    /// thêm 1 cột không dấu — là bước riêng, chưa làm.
    /// </summary>
    private static string MauLike(string q) => "%" + q.Trim().Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%";

    // dạng chuỗi thời gian của nguồn — Work.Start/End/Deadline và Conversation.Deadline đều là chuỗi
    // (nguồn Models/Work.cs:104-113, Conversation.cs:95); WorkController.cs:363-365 dùng "yyyy-MM-dd HH:mm".
    public const string DangThoiGian = "yyyy-MM-dd HH:mm";

    // ════════════════════════════════════════ SLA ════════════════════════════════════════

    /// <summary>Port 1:1 <c>WorkSettingController.cs:556</c> <c>SLAs()</c> — SLAManager.GetAll(OrgId), KHÔNG lọc IsActive.</summary>
    public Task<List<Sla>> SlaListAsync() => db.Slas.OrderBy(s => s.Level).ThenBy(s => s.Id).ToListAsync();

    /// <summary>
    /// Port 1:1 <c>WorkSettingController.cs:577</c> <c>EditSLA(long id)</c>.
    /// Guard nguồn: <c>if (sla.OrgId != OrgId) throw new Exception("invalid network")</c>
    /// (WorkSettingController.cs:582). Ở đích guard này do query filter theo tenant lo
    /// (Data/AppDbContext.cs) ⇒ bản ghi khác org trả về null, không phải ném lỗi.
    /// </summary>
    public Task<Sla?> SlaGetAsync(int id) => db.Slas.FirstOrDefaultAsync(s => s.Id == id);

    public Task<List<BizHour>> BizHoursAsync() => db.BizHours.Where(h => h.IsActive).ToListAsync();

    /// <summary>
    /// Port 1:1 <c>WorkSettingController.cs:602</c> <c>EditSLA(SLA model)</c> [HttpPost].
    /// Sửa: nguồn nạp bản cũ rồi gán ĐÚNG 9 trường (WorkSettingController.cs:612-620) —
    /// KHÔNG ghi đè cả bản ghi. Giữ nguyên danh sách 9 trường đó.
    /// Nguồn có 2 dòng bị comment (<c>old.Ranks</c>, <c>old.UpdateDetailJson()</c> :621-622) ⇒ KHÔNG port.
    /// </summary>
    public async Task<(bool ok, string msg, int id)> SlaSaveAsync(Sla model)
    {
        // Nguồn không validate ở server (chỉ data-val-required phía JS: EditSLA.cshtml:61/79/88/100/109/118).
        // Đích thêm 1 chốt tối thiểu ở server vì form đích post thẳng, không qua tvalidate().
        if (string.IsNullOrWhiteSpace(model.Level)) return (false, "Cần chọn Level.", 0);
        if (string.IsNullOrWhiteSpace(model.Description)) return (false, "Cần nhập Mô tả.", 0);

        if (model.Id > 0)
        {
            var old = await db.Slas.FirstOrDefaultAsync(s => s.Id == model.Id);
            if (old == null) return (false, "Không tìm thấy SLA (hoặc thuộc tổ chức khác).", 0);
            old.Level = model.Level;
            old.Description = model.Description;
            old.IsActive = model.IsActive;
            old.ANDConditionDetails = model.ANDConditionDetails;
            old.ORConditionDetails = model.ORConditionDetails;
            old.FirstResponseTimeDesc = model.FirstResponseTimeDesc;
            old.ResolutionTimeDesc = model.ResolutionTimeDesc;
            old.EveryResponseTimeDesc = model.EveryResponseTimeDesc;
            old.BusinessHourId = model.BusinessHourId;
            await db.SaveChangesAsync();
            return (true, "Đã lưu SLA.", old.Id);
        }
        db.Slas.Add(model);                      // OrgId do StampOrg đóng dấu (Data/AppDbContext.cs:75)
        await db.SaveChangesAsync();
        return (true, "Đã thêm SLA.", model.Id);
    }

    /// <summary>
    /// Nguồn có <c>DeleteSLA</c> (SLAs.cshtml:86 gọi tới). Đích chặn xóa SLA đang được hội thoại dùng —
    /// nguồn KHÔNG có chốt này; đây là phần ĐÍCH THÊM, khai báo rõ chứ không im lặng.
    /// </summary>
    public async Task<(bool ok, string msg)> SlaDeleteAsync(int id)
    {
        var sla = await db.Slas.FirstOrDefaultAsync(s => s.Id == id);
        if (sla == null) return (false, "Không tìm thấy SLA.");
        if (await db.Conversations.AnyAsync(c => c.SLAId == id)) return (false, "SLA đang được hội thoại sử dụng.");
        db.Slas.Remove(sla);
        await db.SaveChangesAsync();
        return (true, "Đã xóa SLA.");
    }

    // ════════════════════════════════════ HỘI THOẠI ══════════════════════════════════════

    public async Task<List<Conversation>> ConvListAsync(int? statusId, int? priorityId, int? picId, string? q)
    {
        var query = db.Conversations.AsQueryable();
        if (statusId is > 0) query = query.Where(c => c.StatusId == statusId);
        if (priorityId is > 0) query = query.Where(c => c.PriorityId == priorityId);
        if (picId is > 0) query = query.Where(c => c.PicId == picId);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var mau = MauLike(q);
            query = query.Where(c => EF.Functions.Like(c.Title ?? "", mau)
                                  || EF.Functions.Like(c.CustomerName ?? "", mau)
                                  || EF.Functions.Like(c.Tags ?? "", mau));
        }
        var list = await query.ToListAsync();

        // Nạp đối tượng liên quan bằng lệnh RIÊNG (không Include): cột *Id của nguồn không có
        // khóa ngoại thật và dùng 0 = "không chọn" — xem ghi chú ở Data/AppDbContext.cs.
        var khs = await db.CsCustomers.ToDictionaryAsync(x => x.Id);
        var teams = await db.Teams.ToDictionaryAsync(x => x.Id);
        var slas = await db.Slas.ToDictionaryAsync(x => x.Id);
        var agents = await db.Agents.ToDictionaryAsync(x => x.Id);
        foreach (var c in list)
        {
            if (khs.TryGetValue(c.CustomerId, out var kh)) c.Customer = kh;
            if (teams.TryGetValue(c.TeamId, out var tm)) c.Team = tm;
            if (slas.TryGetValue(c.SLAId, out var sl)) c.SLA = sl;
            if (agents.TryGetValue(c.PicId, out var ag)) c.Pic = ag;
            c.CalcSlaSatisfaction();                      // nguồn Conversation.cs:223
        }
        return list.OrderByDescending(c => c.Id).ToList();
    }

    /// <summary>
    /// Port 1:1 nhánh <c>id &gt; 0</c> của <c>ConversationController.cs:1098-1106</c>:
    /// nạp hội thoại rồi nạp Customer RIÊNG bằng CustomerManager.Get (nguồn 1101-1103).
    /// </summary>
    public async Task<Conversation?> ConvGetAsync(int id)
    {
        var conv = await db.Conversations.FirstOrDefaultAsync(c => c.Id == id);
        if (conv == null) return null;
        conv.Customer = await db.CsCustomers.FirstOrDefaultAsync(x => x.Id == conv.CustomerId) ?? new CsCustomer();
        conv.Team = await db.Teams.FirstOrDefaultAsync(x => x.Id == conv.TeamId);
        conv.Product = await db.Products.FirstOrDefaultAsync(x => x.Id == conv.ProductId);
        conv.SLA = await db.Slas.FirstOrDefaultAsync(x => x.Id == conv.SLAId);
        conv.Pic = await db.Agents.FirstOrDefaultAsync(x => x.Id == conv.PicId);
        return conv;
    }

    /// <summary>
    /// Port 1:1 nhánh tạo mới của <c>ConversationController.cs:1072-1096</c>:
    /// mặc định PriorityId=NORMAL, ChannelId=NONE, StatusId=OPEN, SourceId=OTHER
    /// (nguồn 1077-1080 dùng hằng ConvPriority.NORMAL/ConvChannel.NONE/ConvStatus.OPEN/ConvSource.OTHER),
    /// rồi tìm Team ĐẦU TIÊN mà người đang đăng nhập thuộc về và gán PicId = chính người đó (nguồn 1082-1094).
    /// </summary>
    public async Task<Conversation> ConvNewAsync(int currentUserId)
    {
        var conv = new Conversation
        {
            Id = 0,
            Customer = new CsCustomer(),
            PriorityId = ConvConst.PRIORITY_NORMAL,
            ChannelId = ConvConst.CHANNEL_NONE,
            StatusId = ConvConst.STATUS_OPEN,
            SourceId = ConvConst.SOURCE_OTHER
        };
        var uit = await db.UserInTeams.FirstOrDefaultAsync(u => u.UserId == currentUserId);
        if (uit != null) { conv.TeamId = uit.TeamId; conv.PicId = currentUserId; }
        return conv;
    }

    /// <summary>Port 1:1 ConvEdit.cshtml:3-17 — MỌI danh sách tra cứu đều lọc IsActive, TRỪ TeamList (nguồn ConversationController.cs:1119 gọi GetAll không lọc).</summary>
    public async Task<ConvEditLists> ConvEditListsAsync()
    {
        var lk = await db.ConvLookups.Where(x => x.IsActive).ToListAsync();
        List<ConvLookup> Loai(ConvLookupKind k) => lk.Where(x => x.Kind == k).OrderBy(x => x.Code).ToList();
        return new ConvEditLists(
            Loai(ConvLookupKind.Type), Loai(ConvLookupKind.Status), Loai(ConvLookupKind.Source),
            Loai(ConvLookupKind.Priority), Loai(ConvLookupKind.Channel),
            await db.Companies.OrderBy(c => c.Name).ToListAsync(),
            await db.Teams.OrderBy(t => t.Name).ToListAsync(),
            await db.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(),
            await db.UserInTeams.ToListAsync(),
            await db.Slas.Where(s => s.AvailableStatus == AvailableStatuses.Active).ToListAsync(),
            await db.Agents.Where(a => a.IsActive).OrderBy(a => a.Name).ToListAsync());
    }

    /// <summary>
    /// Port 1:1 <c>ConversationController.cs:1134</c> <c>ConvEdit(Conversation model)</c> [HttpPost]
    /// + <c>ConversationManager.Add</c> (ConversationManager.cs:909) / <c>UpdateDetail</c> (:650).
    ///
    /// Điểm dễ port sai — đã bám đúng nguồn:
    ///  • SỬA: ghi lại khách hàng CHỈ KHI <c>Customer != null &amp;&amp; Customer.Name</c> không rỗng
    ///    (ConversationManager.cs:662) và lấy khách theo <c>old.CustomerId</c> — KHÔNG theo id form gửi lên.
    ///  • SỬA: chỉ gán 13 trường (ConversationManager.cs:673-685). <c>CustomerName</c> KHÔNG nằm trong
    ///    danh sách đó ⇒ đổi tên khách ở màn sửa KHÔNG cập nhật Conversation.CustomerName. Đây là
    ///    hành vi THẬT của nguồn, port y nguyên (không "sửa hộ").
    ///  • THÊM: <c>CustomerName</c> ĐƯỢC set (= cus.Name, ConversationManager.cs:930), CreatorId/OrgId
    ///    set ở controller (ConversationController.cs:1170-1171), khách mới thì Add, khách cũ thì Update
    ///    (ConversationManager.cs:933-936) và <c>LastActivityDTime = DateTime.Now</c> (:922).
    ///
    /// CỐ Ý BỎ: bảng <c>ConvFollow</c> (người theo hội thoại, ConversationManager.cs:940-944, :688-699)
    /// và <c>NotifyConvEvent</c> (thông báo, ConversationController.cs:1155-1157) — đích chưa có 2 hệ đó.
    /// Ghi rõ là GAP, không phải "khớp hoàn toàn".
    /// </summary>
    public async Task<(bool ok, string msg, int id)> ConvSaveAsync(Conversation model, int currentUserId)
    {
        // data-val-required của nguồn: title (ConvEdit.cshtml:42), Customer.Name (:58/65),
        // statusId (:143), deadline (:199), PicId (:236), SLAId (:247).
        if (string.IsNullOrWhiteSpace(model.Title)) return (false, "Cần nhập Tiêu đề.", 0);
        if (string.IsNullOrWhiteSpace(model.Customer?.Name)) return (false, "Cần nhập Tên khách hàng.", 0);
        if (model.StatusId <= 0) return (false, "Cần chọn Trạng thái.", 0);
        if (string.IsNullOrWhiteSpace(model.Deadline)) return (false, "Cần nhập Hạn xử lý.", 0);
        if (model.PicId <= 0) return (false, "Cần chọn Người phụ trách (PIC).", 0);

        if (model.Id > 0)
        {
            var old = await db.Conversations.FirstOrDefaultAsync(c => c.Id == model.Id);
            if (old == null) return (false, "Không tìm thấy hội thoại (hoặc thuộc tổ chức khác).", 0);

            if (model.Customer != null && !string.IsNullOrEmpty(model.Customer.Name))
            {
                var cus = await db.CsCustomers.FirstOrDefaultAsync(x => x.Id == old.CustomerId);
                if (cus != null)
                {
                    cus.Code = model.Customer.Code;
                    cus.CompanyId = model.Customer.CompanyId;
                    cus.Name = model.Customer.Name;
                    cus.Email = model.Customer.Email;
                    cus.Address = model.Customer.Address;
                    cus.PhoneNo = model.Customer.PhoneNo;
                }
            }
            old.Title = model.Title;
            old.PicId = model.PicId;
            old.TeamId = model.TeamId;
            old.Tags = model.Tags;
            old.TypeId = model.TypeId;
            old.SourceId = model.SourceId;
            old.ChannelId = model.ChannelId;
            old.StatusId = model.StatusId;
            old.ProductId = model.ProductId;
            old.PriorityId = model.PriorityId;
            old.Deadline = model.Deadline;
            old.SLAId = model.SLAId;
            await db.SaveChangesAsync();
            return (true, "Đã lưu hội thoại.", old.Id);
        }

        // ── thêm mới ──
        model.CreatorId = currentUserId;
        var kh = model.CustomerId > 0 ? await db.CsCustomers.FirstOrDefaultAsync(x => x.Id == model.CustomerId) : null;
        kh ??= new CsCustomer();
        kh.LastActivityDTime = DateTime.Now;
        kh.Code = model.Customer!.Code;
        kh.CompanyId = model.Customer.CompanyId;
        kh.Name = model.Customer.Name;
        kh.Email = model.Customer.Email;
        kh.Address = model.Customer.Address;
        kh.PhoneNo = model.Customer.PhoneNo;
        model.CustomerName = kh.Name;
        if (kh.Id == 0) db.CsCustomers.Add(kh);
        await db.SaveChangesAsync();          // cần Id của khách trước khi gán vào hội thoại
        model.CustomerId = kh.Id;
        model.Customer = null;                // navigation này đã Ignore; xóa cho khỏi hiểu nhầm là đã lưu kèm
        db.Conversations.Add(model);
        await db.SaveChangesAsync();

        // nguồn UpdateLastCusConv(cus.Id) — ConversationManager.cs:946: đếm lại số hội thoại của khách.
        kh.ConversationCount = await db.Conversations.CountAsync(c => c.CustomerId == kh.Id);
        await db.SaveChangesAsync();
        return (true, "Đã thêm hội thoại.", model.Id);
    }

    // ═════════════════════════════════════ CÔNG VIỆC ═════════════════════════════════════

    public async Task<List<Work>> WorkListAsync(WorkStatuses? status, int? userId, string? q)
    {
        var query = db.Works.Include(w => w.Followers).AsQueryable();
        if (status.HasValue) query = query.Where(w => w.Status == status.Value);
        if (userId is > 0) query = query.Where(w => w.UserId == userId);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var mau = MauLike(q);
            query = query.Where(w => EF.Functions.Like(w.Name, mau) || EF.Functions.Like(w.WorkPlace ?? "", mau));
        }
        var list = await query.ToListAsync();

        // nguồn WorkManager.cs:GetDetail nạp WorkType / WorkUnit bằng lệnh Get riêng, không join sẵn
        var loai = await db.WorkTypes.ToDictionaryAsync(x => x.Id);
        var dinhMuc = await db.WorkUnits.ToDictionaryAsync(x => x.Id);
        foreach (var w in list)
        {
            if (loai.TryGetValue(w.TypeId, out var t)) w.WorkType = t;
            if (dinhMuc.TryGetValue(w.WUId, out var u)) w.WorkUnit = u;
        }
        return list.OrderByDescending(w => w.Id).ToList();
    }

    /// <summary>
    /// Port 1:1 <c>WorkController.cs:359-372</c> <c>AddWork()</c> [HttpGet]:
    /// Start = đầu giờ kế tiếp (bỏ phút hiện tại rồi +1h), End và Deadline = +2h.
    /// </summary>
    public Work WorkNew()
    {
        var moc = DateTime.Now.AddMinutes(-DateTime.Now.Minute);
        return new Work
        {
            Start = moc.AddHours(1).ToString(DangThoiGian),
            End = moc.AddHours(2).ToString(DangThoiGian),
            Deadline = moc.AddHours(2).ToString(DangThoiGian)
        };
    }

    public async Task<WorkAddLists> WorkAddListsAsync() => new(
        await db.WorkTypes.OrderBy(t => t.Name).ToListAsync(),
        await db.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(),
        await db.WorkUnits.OrderBy(u => u.Name).ToListAsync(),
        await db.Agents.Where(a => a.IsActive).OrderBy(a => a.Name).ToListAsync());

    /// <summary>
    /// Port 1:1 <c>WorkManager.cs:237</c> <c>GetOverlappedWorks</c> — tìm việc TRÙNG GIỜ của người phụ trách.
    /// Ba chi tiết PHẢI giữ đúng, sai là ra kết quả khác mà build vẫn xanh:
    ///  1) Người đối chiếu là <c>Followers</c> có Role = Incharge, KHÔNG phải mọi thành viên
    ///     (WorkManager.cs:255-257); không có Incharge thì lấy <c>Work.UserId</c> (:250).
    ///  2) Điều kiện chồng lấn là NGHIÊM NGẶT hai đầu: <c>w.Start &lt; ow.End &amp;&amp; w.End &gt; ow.Start</c>
    ///     (WorkManager.cs:274) ⇒ hai việc chạm mép nhau (End == Start) KHÔNG tính là trùng.
    ///  3) So sánh bằng <c>string.Compare</c> trên chuỗi "yyyy-MM-dd HH:mm" — nguồn không parse ngày.
    ///  Nguồn KHÔNG loại việc đã Hủy/Hoàn thành khỏi phép so ⇒ đích cũng không loại (giữ 1:1).
    /// </summary>
    public async Task<List<Work>> WorkOverlappedAsync(Work model)
    {
        var ret = new List<Work>();
        if (string.IsNullOrEmpty(model.Start) || string.IsNullOrEmpty(model.End)) return ret;

        // WorkManager.cs:249-260 — chốt lại UserId theo người phụ trách
        var userId = model.UserId;
        var pic = model.Followers.FirstOrDefault(f => f.Role == WorkUserRoles.Incharge);
        if (pic != null) userId = pic.UserId;
        model.UserId = userId;
        if (userId <= 0) return ret;

        // WorkManager.cs:265-271 — thu hẹp theo người phụ trách rồi so từng cặp.
        // Phép so khoảng làm ở BỘ NHỚ, không ở SQL: nguồn dùng string.Compare trên chuỗi
        // "yyyy-MM-dd HH:mm", mà cách provider dịch so sánh chuỗi (collation) không chắc
        // trùng thứ tự ordinal ⇒ đưa vào SQL là đổi nghĩa guard một cách âm thầm.
        var ungVien = await db.Works.Where(w => w.UserId == userId && w.Start != null && w.End != null).ToListAsync();

        foreach (var ow in ungVien)
            if (ow.Id != model.Id
                && string.Compare(model.Start, ow.End, StringComparison.Ordinal) < 0
                && string.Compare(model.End, ow.Start, StringComparison.Ordinal) > 0)
                ret.Add(ow);
        return ret;
    }

    /// <summary>
    /// Port 1:1 <c>WorkController.cs:410</c> <c>AddWork(Work model, bool confirm)</c> [HttpPost]
    /// + <c>WorkManager.cs:63</c> <c>AddWithDetail</c>.
    ///
    /// Thứ tự nguồn (giữ nguyên):
    ///  1) CreatorId = người đăng nhập, OrgId = org hiện tại, Status = Open (WorkController.cs:415-418).
    ///  2) <c>Followers.Count &gt; 1</c> ⇒ IsGroup = true (:419) — CHÚ Ý là &gt; 1, không phải &gt; 0.
    ///  3) Không có Follower nào ⇒ UserId = người đăng nhập (:421).
    ///  4) Chưa xác nhận (<c>confirm == false</c>) ⇒ kiểm trùng giờ; có trùng thì TRẢ VỀ danh sách
    ///     để người dùng xác nhận, CHƯA ghi gì (:422-448).
    ///  5) AddWithDetail: nếu có ParentId thì RootId = parent.RootId (hoặc parent.Id nếu parent là gốc)
    ///     và guard <c>parent.OrgId != model.OrgId</c> ⇒ lỗi (WorkManager.cs:71-75);
    ///     UserId = Incharge nếu có, ngược lại = CreatorId (:83-85);
    ///     CHỈ lưu WorkFollow của những người KHÔNG phải Incharge (:90 — <c>if (f.Role != Incharge)</c>),
    ///     người Incharge đã nằm ở <c>Work.UserId</c>.
    ///
    /// CỐ Ý BỎ: <c>NotifyWorkEvent</c> (WorkController.cs:375-406) — đích chưa có hệ thông báo.
    /// </summary>
    public async Task<(bool ok, string msg, int id, List<Work> overlapped)> WorkAddAsync(
        Work model, List<WorkFollow> followers, int currentUserId, bool confirm)
    {
        if (string.IsNullOrWhiteSpace(model.Name)) return (false, "Cần nhập Tên công việc.", 0, []);
        if (string.IsNullOrWhiteSpace(model.Start) || string.IsNullOrWhiteSpace(model.End))
            return (false, "Cần nhập thời gian Bắt đầu và Kết thúc.", 0, []);

        model.Followers = followers ?? [];
        model.CreatorId = currentUserId;
        model.Status = WorkStatuses.Open;
        if (model.Followers.Count > 1) model.IsGroup = true;
        if (model.Followers.Count == 0) model.UserId = currentUserId;

        if (!confirm)
        {
            var trung = await WorkOverlappedAsync(model);
            if (trung.Count > 0) return (false, "Trùng giờ với công việc đã có — cần xác nhận.", 0, trung);
        }

        // WorkManager.cs:71-75
        if (model.ParentId > 0)
        {
            var parent = await db.Works.FirstOrDefaultAsync(w => w.Id == model.ParentId);
            if (parent == null) return (false, "Không tìm thấy công việc cha (hoặc thuộc tổ chức khác).", 0, []);
            model.RootId = parent.RootId > 0 ? parent.RootId : parent.Id;
        }

        // WorkManager.cs:81-85 — nguồn ĐẶT LẠI UserId = 0 rồi mới quyết định
        model.UserId = 0;
        var pic = model.Followers.FirstOrDefault(f => f.Role == WorkUserRoles.Incharge);
        model.UserId = pic != null ? pic.UserId : model.CreatorId;

        var dsFollow = model.Followers;
        model.Followers = [];                 // chèn Work trước, WorkFollow sau (cần WorkId)
        db.Works.Add(model);
        await db.SaveChangesAsync();

        // WorkManager.cs:87-101 — CHỈ lưu người KHÔNG phải Incharge
        foreach (var f in dsFollow.Where(f => f.Role != WorkUserRoles.Incharge))
        {
            f.WorkId = model.Id;
            f.RootId = model.RootId > 0 ? model.RootId : model.Id;
            db.WorkFollows.Add(f);
        }
        if (db.ChangeTracker.HasChanges()) await db.SaveChangesAsync();
        return (true, "Đã tạo công việc.", model.Id, []);
    }
}
