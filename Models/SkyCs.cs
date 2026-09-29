namespace MiniCSKH.Models;

// ════════════════════════════════════════════════════════════════════════════════════════════
//  PORT 1:1 tu du an nguon 2023.8A.SkyCS (idn.igoss) — cum HOI THOAI (Conversation) / CONG VIEC
//  (Work) / SLA. Moi lop duoi day doi chieu tung cot voi model nguon, co dan nguon file:dong.
//
//  BA QUY UOC DOI CHIEU dung cho CA BA lop (khai bao 1 lan, khong nhac lai o tung cot):
//   1) Khoa: nguon `long Id` + thuoc tinh [LongGuid] (sinh id kieu guid-long o tang mbiz).
//      Dich dung `int Id` tu tang (identity) — theo mau nha MiniCSKH (Models/Entities.cs:10).
//      Day la DON GIAN HOA CO CHU DICH, khong phai thieu cot.
//   2) Tenant: nguon co CA `long OrgId` (cot DB) LAN `NetworkId` (ke thua NetworkEntityBase,
//      tang mang/he thong). Dich chi co MOT tang tenant `Guid OrgId` qua IOrgOwned
//      (Models/Org.cs:15) => `NetworkId` CO Y BO.
//   3) Nguoi dung: nguon tro tai `SysUser`/`OrgUser` (PicId, UserId, CreatorId,
//      WorkAssignUserId). Dich KHONG co bang SysUser; cac truong nay tro tai `Agent`
//      (Models/Entities.cs:8). Giu NGUYEN TEN cot de doi chieu duoc.
// ════════════════════════════════════════════════════════════════════════════════════════════

/// <summary>Trang thai kha dung. Port 1:1 enum nguon Models/SysUser.cs:10-15.</summary>
public enum AvailableStatuses { Inactive = 0, Active = 1, Deleted = 2 }

/// <summary>
/// Cam ket chat luong dich vu. Port 1:1 <c>Models/SLA.cs:10</c> (bang DB <c>SLA</c>).
/// Ba moc thoi gian luu bang PHUT (int) dung nhu nguon; ba thuoc tinh <c>*Desc</c> quy doi
/// "gio:phut" — port 1:1 SLA.cs:119-193, la thu ma man EditSLA.cshtml:79/100/118 thuc su post len.
/// </summary>
public class Sla : IOrgOwned
{
    public int Id { get; set; }                                  // nguon SLA.cs:15 (long)
    public Guid OrgId { get; set; }                              // nguon SLA.cs:17 (long)
    public string Level { get; set; } = "";                      // nguon SLA.cs:19 — EditSLA.cshtml:63 chi cho A/B/C
    public string Description { get; set; } = "";                // nguon SLA.cs:21
    public int FirstResponseTime { get; set; }                   // nguon SLA.cs:23 — don vi PHUT
    public int EveryResponseTime { get; set; }                   // nguon SLA.cs:25 — don vi PHUT
    public int ResolutionTime { get; set; }                      // nguon SLA.cs:27 — don vi PHUT
    public int BusinessHourId { get; set; }                      // nguon SLA.cs:29 (long) -> bang BizHour
    public string? ANDConditionDetails { get; set; }             // nguon SLA.cs:31 — JSON List<RoutingCondition>
    public string? ORConditionDetails { get; set; }              // nguon SLA.cs:33 — JSON List<RoutingCondition>
    public AvailableStatuses AvailableStatus { get; set; }       // nguon SLA.cs:35

    /// <summary>Port 1:1 SLA.cs:38-49 — IsActive KHONG phai cot rieng, no la anh xa cua AvailableStatus.</summary>
    public bool IsActive
    {
        get => AvailableStatus == AvailableStatuses.Active;
        set => AvailableStatus = value ? AvailableStatuses.Active : AvailableStatuses.Inactive;
    }

    /// <summary>Port 1:1 SLA.cs:119-142 — doc ra "HH:mm", ghi vao thi tach ':' roi h*60+m.</summary>
    public string FirstResponseTimeDesc
    {
        get => PhutSangGioPhut(FirstResponseTime);
        set => FirstResponseTime = GioPhutSangPhut(value);
    }
    /// <summary>Port 1:1 SLA.cs:144-167.</summary>
    public string EveryResponseTimeDesc
    {
        get => PhutSangGioPhut(EveryResponseTime);
        set => EveryResponseTime = GioPhutSangPhut(value);
    }
    /// <summary>Port 1:1 SLA.cs:170-193.</summary>
    public string ResolutionTimeDesc
    {
        get => PhutSangGioPhut(ResolutionTime);
        set => ResolutionTime = GioPhutSangPhut(value);
    }

    // Nguon: string.Format("{0}:{1}", (p/60).ToString("00"), (p%60).ToString("00")) — SLA.cs:124-127.
    private static string PhutSangGioPhut(int phut) => $"{phut / 60:00}:{phut % 60:00}";

    // Nguon: int.Parse(arr[0]) * 60 + int.Parse(arr[1]) — SLA.cs:131-139. Nguon NEM Exception khi
    // chuoi sai dinh dang (int.Parse, khong TryParse); dich dung TryParse va coi phan sai la 0 —
    // DON GIAN HOA CO CHU DICH: form web dich khong co widget html-duration-picker bao dam dinh dang.
    private static int GioPhutSangPhut(string? value)
    {
        if (string.IsNullOrEmpty(value)) return 0;
        var arr = value.Split(':');
        _ = int.TryParse(arr[0], out var h);
        var m = 0;
        if (arr.Length > 1) _ = int.TryParse(arr[1], out m);
        return h * 60 + m;
    }
}

/// <summary>Khung gio lam viec. Port 1:1 <c>Models/BizHour.cs</c> — chi giu phan EditSLA.cshtml:88 dung.</summary>
public class BizHour : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public string Name { get; set; } = "";
    public bool Is247 { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>Cong ty cua khach hang. Port 1:1 <c>Models/Company.cs:10</c> (bang <c>Company</c>).</summary>
public class Company : IOrgOwned
{
    public int Id { get; set; }                   // nguon Company.cs:15
    public Guid OrgId { get; set; }               // nguon Company.cs:17
    public string Name { get; set; } = "";        // nguon Company.cs:19
    public string? TaxCode { get; set; }          // nguon Company.cs:21
}

/// <summary>
/// Khach hang cua hoi thoai. Port 1:1 <c>Models/Customer.cs:10</c> (bang <c>Customer</c>).
/// Dat ten <c>CsCustomer</c> vi mau nha da co khai niem "khach hang" nhung nam INLINE tren
/// <c>Ticket.CustomerName/Phone/Email</c> (Models/Entities.cs:35-37) — khong co bang rieng.
/// </summary>
public class CsCustomer : IOrgOwned
{
    public int Id { get; set; }                        // nguon Customer.cs:15
    public Guid OrgId { get; set; }                    // nguon Customer.cs:17
    public string? Code { get; set; }                  // nguon Customer.cs:19 — ConvEdit.cshtml:80
    public string Name { get; set; } = "";             // nguon Customer.cs:21 — ConvEdit.cshtml:58/65
    public string? PhoneNo { get; set; }               // nguon Customer.cs:23 — ConvEdit.cshtml:115
    public string? Address { get; set; }               // nguon Customer.cs:28 — ConvEdit.cshtml:86
    public string? Email { get; set; }                 // nguon Customer.cs:31 — ConvEdit.cshtml:122
    public int CustomerType { get; set; }              // nguon Customer.cs:33
    public int CompanyId { get; set; }                 // nguon Customer.cs:36 — ConvEdit.cshtml:95
    public string? Avatar { get; set; }                // nguon Customer.cs:38
    public int TimeZone { get; set; }                  // nguon Customer.cs:40
    public DateTime LastActivityDTime { get; set; } = DateTime.Now;  // nguon Customer.cs:42
    public int ConversationCount { get; set; }         // nguon Customer.cs:44

    public Company? Company { get; set; }              // nguon Customer.cs:49 (khong phai cot)
}

/// <summary>Nhom xu ly. Port 1:1 <c>Models/Team.cs:14</c> (bang <c>Team</c>).</summary>
public class Team : IOrgOwned
{
    public int Id { get; set; }                        // nguon Team.cs:19
    public Guid OrgId { get; set; }                    // nguon Team.cs:21
    public string Name { get; set; } = "";             // nguon Team.cs:23 — ConvEdit.cshtml:229
    public string? Image { get; set; }                 // nguon Team.cs:25
    public bool IsAgent { get; set; }                  // nguon Team.cs:27
    public bool IsTechnician { get; set; }             // nguon Team.cs:29
    public string? Description { get; set; }           // nguon Team.cs:31
    public int WorkAssignUserId { get; set; }          // nguon Team.cs:33 — ConvEdit.cshtml:229 default-id
    public string? RankDetails { get; set; }           // nguon Team.cs:36
    public AvailableStatuses AvailableStatus { get; set; } = AvailableStatuses.Active;  // nguon Team.cs:39
}

/// <summary>Agent thuoc Team. Port 1:1 <c>Models/UserInTeam.cs</c> — ConvEdit.cshtml:400 nap de loc PIC theo Team.</summary>
public class UserInTeam : IOrgOwned
{
    public int Id { get; set; }
    public Guid OrgId { get; set; }
    public int TeamId { get; set; }
    public int UserId { get; set; }        // -> Agent.Id (quy uoc 3)
    public string? Rank { get; set; }
}

/// <summary>San pham/dich vu. Port 1:1 <c>Models/Product.cs:10</c> (bang <c>Product</c>).</summary>
public class Product : IOrgOwned
{
    public int Id { get; set; }                     // nguon Product.cs:15
    public Guid OrgId { get; set; }                 // nguon Product.cs:17
    public string? Code { get; set; }               // nguon Product.cs:19
    public string Name { get; set; } = "";          // nguon Product.cs:21 — ConvEdit.cshtml:133
    public string? GroupCode { get; set; }          // nguon Product.cs:23
    public string? BrandName { get; set; }          // nguon Product.cs:26
    public bool IsActive { get; set; } = true;      // nguon Product.cs:29 — loc o ConversationController.cs:1127
}

/// <summary>
/// Nam bang tra cuu cua hoi thoai. GOP CO CHU DICH: nguon co 5 bang RIENG
/// <c>ConvStatus</c>/<c>ConvType</c>/<c>ConvSource</c>/<c>ConvPriority</c>/<c>ConvChannel</c>
/// (Models/ConvStatus.cs:10, ConvType.cs:10, ConvSource.cs:10, ConvPriority.cs:10, ConvChannel.cs:10)
/// nhung DA DO: ca 5 co CHINH XAC cung bo 7 cot (OrgId, Id, Name, NameCustomer, IsSystem,
/// IsRequired, IsActive) => gop 1 bang + cot phan loai <c>Kind</c>. Khong cot nao bi bo.
/// </summary>
public class ConvLookup : IOrgOwned
{
    public int Id { get; set; }                          // khoa tu tang cua bang gop
    public Guid OrgId { get; set; }                      // nguon ConvStatus.cs:24
    public ConvLookupKind Kind { get; set; }             // THEM: phan biet 5 bang nguon
    public int Code { get; set; }                        // nguon ConvStatus.cs:27 `Id` — hang so nghiep vu (OPEN=2...)
    public string Name { get; set; } = "";               // nguon ConvStatus.cs:29
    public string? NameCustomer { get; set; }            // nguon ConvStatus.cs:31
    public bool IsSystem { get; set; }                   // nguon ConvStatus.cs:33
    public bool IsRequired { get; set; }                 // nguon ConvStatus.cs:35
    public bool IsActive { get; set; } = true;           // nguon ConvStatus.cs:37 — loc o ConversationController.cs:1114-1118
}
public enum ConvLookupKind { Status = 1, Type = 2, Source = 3, Priority = 4, Channel = 5 }

/// <summary>Hang so nghiep vu — port 1:1 cac `public const int` trong 5 lop tra cuu nguon.</summary>
public static class ConvConst
{
    // Models/ConvStatus.cs:12-19
    public const int STATUS_NONE = 0, STATUS_NEW = 1, STATUS_OPEN = 2, STATUS_PROCESSING = 3,
        STATUS_RESOLVED = 4, STATUS_CLOSED = 5, STATUS_WAITING_ON_CUSTOMER = 6, STATUS_WAITING_ON_3RD = 7;
    // Models/ConvPriority.cs:12-16
    public const int PRIORITY_NONE = 0, PRIORITY_LOW = 1, PRIORITY_NORMAL = 2, PRIORITY_HIGH = 3, PRIORITY_URGENT = 4;
    // Models/ConvSource.cs:12-15
    public const int SOURCE_NONE = 0, SOURCE_FACEBOOK = 1, SOURCE_CALL = 2, SOURCE_OTHER = 3;
    // Models/ConvChannel.cs:12-20
    public const int CHANNEL_NONE = 0, CHANNEL_EMAIL = 1, CHANNEL_CALL = 2, CHANNEL_SMS = 3, CHANNEL_CHAT = 4,
        CHANNEL_FACEBOOK = 5, CHANNEL_ZALO = 6, CHANNEL_YOUTUBE = 7, CHANNEL_PORTAL = 8;
}

/// <summary>
/// Hoi thoai / eTicket. Port 1:1 <c>Models/Conversation.cs:46</c> (bang DB <c>Conversation</c>).
/// Day la bang lon nhat cua cum CSKH nguon: 9 man dung no (NGUON_VIEW_BANG_COT.tsv).
/// </summary>
public class Conversation : IOrgOwned
{
    public int Id { get; set; }                           // nguon Conversation.cs:51
    public Guid OrgId { get; set; }                       // nguon Conversation.cs:54
    public int CustomerId { get; set; }                   // nguon Conversation.cs:58 — ConvEdit.cshtml:36
    public string? Title { get; set; }                    // nguon Conversation.cs:60 — ConvEdit.cshtml:42
    public string? CustomerName { get; set; }             // nguon Conversation.cs:62 — set trong Add, ConversationManager.cs:930
    public string? CustomerAvatar { get; set; }           // nguon Conversation.cs:64
    public int LastMessageId { get; set; }                // nguon Conversation.cs:68
    public string? LastMessageSumary { get; set; }        // nguon Conversation.cs:70 (nguon viet sai chinh ta "Sumary" — GIU NGUYEN)
    public int ChannelId { get; set; }                    // nguon Conversation.cs:72 — ConvEdit.cshtml:209
    public int SourceId { get; set; }                     // nguon Conversation.cs:74 — ConvEdit.cshtml:185
    public int TypeId { get; set; }                       // nguon Conversation.cs:76 — ConvEdit.cshtml:156
    public int StatusId { get; set; }                     // nguon Conversation.cs:78 — ConvEdit.cshtml:143
    public int PriorityId { get; set; }                   // nguon Conversation.cs:80 — ConvEdit.cshtml:170
    public int ProductId { get; set; }                    // nguon Conversation.cs:83 — ConvEdit.cshtml:129
    public int BizHourId { get; set; }                    // nguon Conversation.cs:85
    public int SLAId { get; set; }                        // nguon Conversation.cs:89 — ConvEdit.cshtml:247
    public string? Tags { get; set; }                     // nguon Conversation.cs:92 — ConvEdit.cshtml:49
    public string? Deadline { get; set; }                 // nguon Conversation.cs:95 — CHUOI "yyyy-MM-dd HH:mm", khong phai DateTime
    public int TeamId { get; set; }                       // nguon Conversation.cs:99 — ConvEdit.cshtml:225
    public int PicId { get; set; }                        // nguon Conversation.cs:102 — ConvEdit.cshtml:236 (-> Agent.Id)
    public int WorkflowId { get; set; }                   // nguon Conversation.cs:105
    public int CreatorId { get; set; }                    // nguon Conversation.cs:109 — set o ConversationController.cs:1170
    public int UnreadCount { get; set; }                  // nguon Conversation.cs:111
    public int FirstResponseTime { get; set; }            // nguon Conversation.cs:113 — PHUT, dung cho CalcSlaSatisfaction
    public int MaxResponseTime { get; set; }              // nguon Conversation.cs:115
    public string? ResolveDTime { get; set; }             // nguon Conversation.cs:118
    public bool IsLastOfCustomer { get; set; }            // nguon Conversation.cs:121

    public CsCustomer? Customer { get; set; }             // nguon Conversation.cs:140
    public Team? Team { get; set; }                       // nguon Conversation.cs:132
    public Product? Product { get; set; }                 // nguon Conversation.cs:131
    public Sla? SLA { get; set; }                         // nguon Conversation.cs:150
    public Agent? Pic { get; set; }                       // nguon Conversation.cs:123 `OrgUser Pic`

    /// <summary>Port 1:1 Conversation.cs:124-135 — tach Tags theo ',', trim + ha chu; RONG => null.</summary>
    public List<string>? TagList
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Tags)) return null;
            return Tags.Split(',').Select(s => s.Trim().ToLower()).ToList();
        }
    }

    public bool IsSlaSatisfied { get; set; }              // nguon Conversation.cs:217

    /// <summary>
    /// Port 1:1 Conversation.cs:223-236. Khong co SLA => coi la DAT.
    /// Chu y ranh gioi KHAC NHAU giua hai ve: FirstResponseTime dung &lt;= con MaxResponseTime dung &lt;
    /// (nguon Conversation.cs:231). Sai dau nay ra ket qua khac ma build van xanh.
    /// Phan so sanh ResolutionTime o nguon DANG BI COMMENT (Conversation.cs:233-234) => KHONG port.
    /// </summary>
    public void CalcSlaSatisfaction()
    {
        if (SLA == null) { IsSlaSatisfied = true; return; }
        IsSlaSatisfied = FirstResponseTime <= SLA.FirstResponseTime && MaxResponseTime < SLA.EveryResponseTime;
    }
}

/// <summary>Trang thai cong viec. Port 1:1 <c>Models/Work.cs:10-20</c>.</summary>
public enum WorkStatuses { None = 0, Pending = 1, Open = 2, Processing = 3, Cancelled = 4, Finished = 5 }
/// <summary>Do uu tien cong viec. Port 1:1 <c>Models/Work.cs:22-29</c>. KHAC enum TicketPriority cua mau nha (bat dau tu 1, co None).</summary>
public enum WorkPriorites { None = 0, Low = 1, Normal = 2, High = 3, Urgent = 4 }
/// <summary>Vai tro nguoi theo viec. Port 1:1 <c>Models/WorkFollow.cs:9-15</c>.</summary>
public enum WorkUserRoles { None = 0, Incharge = 1, Normal = 2, Manage = 3 }

/// <summary>Loai cong viec. Port 1:1 <c>Models/WorkType.cs:10</c> (bang <c>WorkType</c>).</summary>
public class WorkType : IOrgOwned
{
    public int Id { get; set; }                       // nguon WorkType.cs:15
    public Guid OrgId { get; set; }                   // nguon WorkType.cs:17
    public int TeamId { get; set; }                   // nguon WorkType.cs:19
    public string Name { get; set; } = "";            // nguon WorkType.cs:21 — AddWork.cshtml:121
    public string? Description { get; set; }          // nguon WorkType.cs:23
}

/// <summary>Don vi cong viec (dinh muc). Port 1:1 <c>Models/WorkUnit.cs:17</c> (bang <c>WorkUnit</c>).</summary>
public class WorkUnit : IOrgOwned
{
    public int Id { get; set; }                       // nguon WorkUnit.cs:22
    public Guid OrgId { get; set; }                   // nguon WorkUnit.cs:24
    public int TeamId { get; set; }                   // nguon WorkUnit.cs:27
    public int GroupId { get; set; }                  // nguon WorkUnit.cs:30
    public int TypeId { get; set; }                   // nguon WorkUnit.cs:34 — AddWork.cshtml:291 set TypeId cho form
    public string Name { get; set; } = "";            // nguon WorkUnit.cs:37 — AddWork.cshtml:286
    public string? Description { get; set; }          // nguon WorkUnit.cs:39
    public int StdTime { get; set; }                  // nguon WorkUnit.cs:41
    public string? RankDetails { get; set; }          // nguon WorkUnit.cs:43
    public string? RankList { get; set; }             // nguon WorkUnit.cs:46

    /// <summary>
    /// AddWork.cshtml:287-288 lay <c>item.Duration</c> tu ket qua /loadworkunits de dien StdTime VA Duration.
    /// Nguon WorkUnit.cs KHONG co cot `Duration` — no den tu <c>WorkUnitRank.Duration</c> (WorkUnit.cs:12,
    /// nam trong JSON RankDetails) theo rank cua nguoi dung. Dich CHUA co bang Rank/UserInRank =>
    /// dung StdTime lam Duration mac dinh. KHAI BAO GAP, khong im lang.
    /// </summary>
    public int Duration => StdTime;
}

/// <summary>
/// Cong viec. Port 1:1 <c>Models/Work.cs:40</c> (bang DB <c>Work</c>). 11 man nguon dung bang nay.
/// LUU Y KIEU: <c>Start/End/StartActual/EndActual/Deadline</c> o nguon la CHUOI (Work.cs:104-113),
/// khong phai DateTime — va guard trung gio so sanh bang <c>string.Compare</c> (WorkManager.cs:274).
/// Vi vay dich GIU NGUYEN kieu chuoi dinh dang "yyyy-MM-dd HH:mm" de thu tu chuoi = thu tu thoi gian.
/// </summary>
public class Work : IOrgOwned
{
    public int Id { get; set; }                       // nguon Work.cs:45
    public Guid OrgId { get; set; }                   // nguon Work.cs:48
    public int ConvId { get; set; }                   // nguon Work.cs:51 — noi ve Conversation
    public int ParentId { get; set; }                 // nguon Work.cs:54
    public int RootId { get; set; }                   // nguon Work.cs:58
    public int OriginalId { get; set; }               // nguon Work.cs:61
    public int TypeId { get; set; }                   // nguon Work.cs:65 — AddWork.cshtml:117
    public int ProductId { get; set; }                // nguon Work.cs:70 — AddWork.cshtml:203
    public int GroupId { get; set; }                  // nguon Work.cs:73
    public int MessengerId { get; set; }              // nguon Work.cs:76
    public int Idx { get; set; }                      // nguon Work.cs:79
    public bool IsGroup { get; set; }                 // nguon Work.cs:81 — set o WorkController.cs:419
    public string Name { get; set; } = "";            // nguon Work.cs:83 — AddWork.cshtml:25
    public string? Description { get; set; }          // nguon Work.cs:85 — AddWork.cshtml:107
    public int WUId { get; set; }                     // nguon Work.cs:88 — AddWork.cshtml:149
    public int TeamId { get; set; }                   // nguon Work.cs:90
    public int UserId { get; set; }                   // nguon Work.cs:92 — nguoi phu trach (-> Agent.Id)
    public string? UserName { get; set; }             // nguon Work.cs:94
    public int StdTime { get; set; }                  // nguon Work.cs:96 — AddWork.cshtml:160 (PHUT)
    public int ActualTime { get; set; }               // nguon Work.cs:99
    public int Duration { get; set; }                 // nguon Work.cs:102 — AddWork.cshtml:169 (PHUT)
    public string? Start { get; set; }                // nguon Work.cs:104 — AddWork.cshtml:38
    public string? End { get; set; }                  // nguon Work.cs:106 — AddWork.cshtml:50
    public string? StartActual { get; set; }          // nguon Work.cs:108
    public string? EndActual { get; set; }            // nguon Work.cs:110
    public string? Deadline { get; set; }             // nguon Work.cs:113 — AddWork.cshtml:66
    public WorkStatuses Status { get; set; }          // nguon Work.cs:115 — WorkController.cs:418 dat Open khi tao
    public WorkPriorites Priority { get; set; }       // nguon Work.cs:118 — AddWork.cshtml:133
    public string? WorkDetails { get; set; }          // nguon Work.cs:120
    public decimal Price { get; set; }                // nguon Work.cs:123
    public string? RefNo { get; set; }                // nguon Work.cs:125
    public string? RefType { get; set; }              // nguon Work.cs:127
    public string? Remark { get; set; }               // nguon Work.cs:129 — AddWork.cshtml:193
    public string? WorkPlace { get; set; }            // nguon Work.cs:132 — AddWork.cshtml:179
    public int CreatorId { get; set; }                // nguon Work.cs:134 — WorkController.cs:415
    public string? DeadlineNotifyDTime { get; set; }  // nguon Work.cs:136

    /// <summary>Port 1:1 Work.cs:137 — chi la Status.ToString(), khong phai cot DB.</summary>
    public string StatusDesc => Status.ToString();

    public List<WorkFollow> Followers { get; set; } = [];   // nguon: Work.Followers (nap o WorkManager.cs:GetDetail)
    public WorkType? WorkType { get; set; }
    public WorkUnit? WorkUnit { get; set; }
}

/// <summary>Nguoi theo/duoc giao cong viec. Port 1:1 <c>Models/WorkFollow.cs:18</c> (bang <c>WorkFollow</c>).</summary>
public class WorkFollow : IOrgOwned
{
    public int Id { get; set; }                       // khoa tu tang o dich (nguon dung khoa ghep WorkId+UserId)
    public int WorkId { get; set; }                   // nguon WorkFollow.cs:25
    public int UserId { get; set; }                   // nguon WorkFollow.cs:28 (-> Agent.Id)
    public int RootId { get; set; }                   // nguon WorkFollow.cs:31
    public Guid OrgId { get; set; }                   // nguon WorkFollow.cs:34
    public WorkUserRoles Role { get; set; }           // nguon WorkFollow.cs:37

    public Work? Work { get; set; }                   // nguon WorkFollow.cs:42
}
