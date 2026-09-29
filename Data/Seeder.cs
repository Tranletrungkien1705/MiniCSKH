using Microsoft.EntityFrameworkCore;
using MiniCSKH.Models;
using MiniCSKH.Services;

namespace MiniCSKH.Data;

public static class Seeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        await MigratePostgresAsync(db);   // DB cloud cũ: thêm Orgs + cột OrgId nếu thiếu

        if (!await db.Orgs.AnyAsync(o => o.Id == TenantContext.DefaultOrgId))
        {
            db.Orgs.Add(new Org { Id = TenantContext.DefaultOrgId, Name = "Demo CSKH", ApiKey = TenantContext.DefaultApiKey });
            await db.SaveChangesAsync();
        }

        if (!await db.Agents.AnyAsync())
        {
            db.Agents.AddRange(
                new Agent { Name = "Nguyễn Thu Hà", Email = "ha@cskh.vn" },
                new Agent { Name = "Trần Văn Minh", Email = "minh@cskh.vn" },
                new Agent { Name = "Lê Thị Lan", Email = "lan@cskh.vn" });
            await db.SaveChangesAsync();
        }
        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(
                new TicketCategory { Name = "Kỹ thuật", SlaHours = 8 },
                new TicketCategory { Name = "Thanh toán / Hóa đơn", SlaHours = 24 },
                new TicketCategory { Name = "Khiếu nại", SlaHours = 4 },
                new TicketCategory { Name = "Tư vấn chung", SlaHours = 48 });
            await db.SaveChangesAsync();
        }
        if (!await db.KbArticles.AnyAsync())
        {
            db.KbArticles.AddRange(
                new KbArticle { Title = "Cách tra cứu hóa đơn điện tử", Category = "Hóa đơn", Body = "Vào mục Hóa đơn → nhập mã tra cứu (MCCQThue) → tải PDF/XML.", Views = 128 },
                new KbArticle { Title = "Đổi trả sản phẩm trong 7 ngày", Category = "Chính sách", Body = "Sản phẩm còn nguyên tem, hóa đơn hợp lệ được đổi trả trong 7 ngày.", Views = 342 },
                new KbArticle { Title = "Thời gian giao hàng COD toàn quốc", Category = "Vận chuyển", Body = "Nội thành 1-2 ngày, tỉnh 3-5 ngày. COD toàn quốc.", Views = 210 });
            await db.SaveChangesAsync();
        }
        if (!await db.Tickets.AnyAsync())
        {
            var cats = await db.Categories.ToListAsync();
            var agents = await db.Agents.ToListAsync();
            int n = 0;
            Ticket T(string subj, string cust, Channel ch, TicketPriority pri, TicketStatus st, int catIdx, int? agentIdx, int ageHours)
            {
                n++;
                var created = DateTime.Now.AddHours(-ageHours);
                var cat = cats[catIdx];
                return new Ticket
                {
                    Code = $"TK{DateTime.Now:yyMM}{n:D4}",
                    Subject = subj, CustomerName = cust, Channel = ch, Priority = pri, Status = st,
                    CategoryId = cat.Id, AssignedAgentId = agentIdx is { } ai ? agents[ai].Id : null,
                    CreatedAt = created, DueAt = created.AddHours(cat.SlaHours),
                    ResolvedAt = st is TicketStatus.Resolved or TicketStatus.Closed ? created.AddHours(2) : null,
                    Description = "Nội dung yêu cầu từ khách hàng.",
                    Comments = [ new TicketComment { Author = "Hệ thống", Body = "Phiếu được tạo.", CreatedAt = created } ]
                };
            }
            db.Tickets.AddRange(
                T("Không tải được hóa đơn PDF", "Cửa hàng Minh Anh", Channel.Email, TicketPriority.High, TicketStatus.New, 1, null, 1),
                T("Sản phẩm giao bị lỗi", "Shop thời trang Hà", Channel.Zalo, TicketPriority.Urgent, TicketStatus.InProgress, 2, 0, 6),
                T("Hỏi chính sách bảo hành", "Đại lý Phương Nam", Channel.Phone, TicketPriority.Normal, TicketStatus.WaitingCustomer, 3, 1, 30),
                T("Cần xuất lại hóa đơn sai MST", "Công ty ABC", Channel.Web, TicketPriority.High, TicketStatus.Resolved, 1, 2, 50),
                T("Tư vấn chọn size vợt", "Nguyễn Văn A", Channel.Web, TicketPriority.Low, TicketStatus.Closed, 3, 0, 72)
            );
            await db.SaveChangesAsync();
        }

        if (!await db.Calls.AnyAsync())
        {
            var agents = await db.Agents.ToListAsync();
            db.Calls.AddRange(
                new CallLog { Direction = CallDirection.Inbound, PhoneNumber = "0901234567", CustomerName = "Cửa hàng Minh Anh", AgentId = agents[0].Id, StartedAt = DateTime.Now.AddHours(-2), DurationSeconds = 245, Outcome = CallOutcome.Answered, Note = "Hỏi về hóa đơn." },
                new CallLog { Direction = CallDirection.Outbound, PhoneNumber = "0912345678", CustomerName = "Shop thời trang Hà", AgentId = agents[1].Id, StartedAt = DateTime.Now.AddHours(-5), DurationSeconds = 132, Outcome = CallOutcome.Answered, Note = "Gọi lại xác nhận đơn." },
                new CallLog { Direction = CallDirection.Inbound, PhoneNumber = "0923456789", CustomerName = "Khách vãng lai", StartedAt = DateTime.Now.AddHours(-1), DurationSeconds = 0, Outcome = CallOutcome.Missed, Note = "Nhỡ, cần gọi lại." },
                new CallLog { Direction = CallDirection.Inbound, PhoneNumber = "0934567890", CustomerName = "Đại lý Phương Nam", AgentId = agents[2].Id, StartedAt = DateTime.Now.AddHours(-26), DurationSeconds = 410, Outcome = CallOutcome.Answered, Note = "Khiếu nại giao hàng chậm." }
            );
            await db.SaveChangesAsync();
        }

        await SeedSkyCsAsync(db);
    }

    /// <summary>
    /// Dữ liệu mồi cho cụm port 1:1 từ 2023.8A.SkyCS (SLA / Conversation / Work).
    /// Giá trị mã của 5 bảng tra cứu lấy đúng hằng số nguồn (Models/ConvStatus.cs:12-19,
    /// ConvPriority.cs:12-16, ConvSource.cs:12-15, ConvChannel.cs:12-20) — xem ConvConst.
    /// Idempotent: mỗi khối chỉ chạy khi bảng còn rỗng.
    /// </summary>
    private static async Task SeedSkyCsAsync(AppDbContext db)
    {
        if (!await db.ConvLookups.AnyAsync())
        {
            void Them(ConvLookupKind kind, params (int code, string ten)[] ds)
            {
                foreach (var (code, ten) in ds)
                    db.ConvLookups.Add(new ConvLookup { Kind = kind, Code = code, Name = ten, NameCustomer = ten, IsSystem = true, IsActive = true });
            }
            Them(ConvLookupKind.Status,
                (ConvConst.STATUS_NEW, "Mới"), (ConvConst.STATUS_OPEN, "Đang mở"),
                (ConvConst.STATUS_PROCESSING, "Đang xử lý"), (ConvConst.STATUS_RESOLVED, "Đã giải quyết"),
                (ConvConst.STATUS_CLOSED, "Đã đóng"), (ConvConst.STATUS_WAITING_ON_CUSTOMER, "Chờ khách hàng"),
                (ConvConst.STATUS_WAITING_ON_3RD, "Chờ bên thứ ba"));
            Them(ConvLookupKind.Priority,
                (ConvConst.PRIORITY_LOW, "Thấp"), (ConvConst.PRIORITY_NORMAL, "Bình thường"),
                (ConvConst.PRIORITY_HIGH, "Cao"), (ConvConst.PRIORITY_URGENT, "Khẩn"));
            Them(ConvLookupKind.Source,
                (ConvConst.SOURCE_FACEBOOK, "Facebook"), (ConvConst.SOURCE_CALL, "Cuộc gọi"), (ConvConst.SOURCE_OTHER, "Khác"));
            Them(ConvLookupKind.Channel,
                (ConvConst.CHANNEL_EMAIL, "Email"), (ConvConst.CHANNEL_CALL, "Điện thoại"), (ConvConst.CHANNEL_SMS, "SMS"),
                (ConvConst.CHANNEL_CHAT, "Chat"), (ConvConst.CHANNEL_FACEBOOK, "Facebook"), (ConvConst.CHANNEL_ZALO, "Zalo"),
                (ConvConst.CHANNEL_YOUTUBE, "YouTube"), (ConvConst.CHANNEL_PORTAL, "Cổng khách hàng"));
            // ConvType KHÔNG có hằng số ở nguồn (Models/ConvType.cs chỉ có cột) ⇒ đây là dữ liệu mồi, không phải hằng nghiệp vụ.
            Them(ConvLookupKind.Type, (1, "Yêu cầu hỗ trợ"), (2, "Khiếu nại"), (3, "Tư vấn"), (4, "Bảo hành"));
            await db.SaveChangesAsync();
        }

        if (!await db.BizHours.AnyAsync())
        {
            db.BizHours.AddRange(
                new BizHour { Name = "Giờ hành chính (8h-17h30)", IsDefault = true },
                new BizHour { Name = "Trực 24/7", Is247 = true });
            await db.SaveChangesAsync();
        }

        if (!await db.Slas.AnyAsync())
        {
            var gio = await db.BizHours.FirstOrDefaultAsync();
            // FirstResponseTime/EveryResponseTime/ResolutionTime lưu bằng PHÚT (nguồn Models/SLA.cs:23-27)
            db.Slas.AddRange(
                new Sla { Level = "A", Description = "SLA hạng A — phản hồi 30 phút", FirstResponseTime = 30, EveryResponseTime = 60, ResolutionTime = 240, BusinessHourId = gio?.Id ?? 0, IsActive = true },
                new Sla { Level = "B", Description = "SLA hạng B — phản hồi 2 giờ", FirstResponseTime = 120, EveryResponseTime = 240, ResolutionTime = 1440, BusinessHourId = gio?.Id ?? 0, IsActive = true },
                new Sla { Level = "C", Description = "SLA hạng C — phản hồi 8 giờ", FirstResponseTime = 480, EveryResponseTime = 960, ResolutionTime = 2880, BusinessHourId = gio?.Id ?? 0, IsActive = true });
            await db.SaveChangesAsync();
        }

        if (!await db.Teams.AnyAsync())
        {
            var agents = await db.Agents.OrderBy(a => a.Id).ToListAsync();
            var t1 = new Team { Name = "Tổng đài", IsAgent = true, Description = "Tiếp nhận đa kênh", WorkAssignUserId = agents.Count > 0 ? agents[0].Id : 0 };
            var t2 = new Team { Name = "Kỹ thuật", IsTechnician = true, Description = "Xử lý sự cố", WorkAssignUserId = agents.Count > 1 ? agents[1].Id : 0 };
            db.Teams.AddRange(t1, t2);
            await db.SaveChangesAsync();
            // UserInTeam: ô PIC của ConvEdit.cshtml:236 lọc agent theo Team qua bảng này (nguồn ConvEdit.cshtml:400)
            if (agents.Count >= 3)
                db.UserInTeams.AddRange(
                    new UserInTeam { TeamId = t1.Id, UserId = agents[0].Id, Rank = "1" },
                    new UserInTeam { TeamId = t1.Id, UserId = agents[2].Id, Rank = "2" },
                    new UserInTeam { TeamId = t2.Id, UserId = agents[1].Id, Rank = "1" });
            await db.SaveChangesAsync();
        }

        if (!await db.Companies.AnyAsync())
        {
            db.Companies.AddRange(
                new Company { Name = "Công ty ABC", TaxCode = "0101234567" },
                new Company { Name = "Cửa hàng Minh Anh", TaxCode = "0107654321" });
            await db.SaveChangesAsync();
        }

        if (!await db.Products.AnyAsync())
        {
            db.Products.AddRange(
                new Product { Code = "SP01", Name = "Hóa đơn điện tử", GroupCode = "HDDT", BrandName = "idocNet" },
                new Product { Code = "SP02", Name = "Tổng đài ảo", GroupCode = "CC", BrandName = "idocNet" },
                new Product { Code = "SP03", Name = "Chăm sóc khách hàng", GroupCode = "CSKH", BrandName = "idocNet" });
            await db.SaveChangesAsync();
        }

        if (!await db.WorkTypes.AnyAsync())
        {
            var team = await db.Teams.FirstOrDefaultAsync();
            db.WorkTypes.AddRange(
                new WorkType { Name = "Gọi ra cho khách", TeamId = team?.Id ?? 0, Description = "Liên hệ chủ động" },
                new WorkType { Name = "Kiểm tra hệ thống", TeamId = team?.Id ?? 0, Description = "Việc kỹ thuật" });
            await db.SaveChangesAsync();
        }

        if (!await db.WorkUnits.AnyAsync())
        {
            var wt = await db.WorkTypes.OrderBy(x => x.Id).ToListAsync();
            var team = await db.Teams.FirstOrDefaultAsync();
            db.WorkUnits.AddRange(
                new WorkUnit { Name = "Gọi xác nhận thông tin", TypeId = wt.Count > 0 ? wt[0].Id : 0, TeamId = team?.Id ?? 0, StdTime = 15 },
                new WorkUnit { Name = "Khảo sát hài lòng qua điện thoại", TypeId = wt.Count > 0 ? wt[0].Id : 0, TeamId = team?.Id ?? 0, StdTime = 30 },
                new WorkUnit { Name = "Kiểm tra log hệ thống", TypeId = wt.Count > 1 ? wt[1].Id : 0, TeamId = team?.Id ?? 0, StdTime = 60 });
            await db.SaveChangesAsync();
        }

        if (!await db.Conversations.AnyAsync())
        {
            var cty = await db.Companies.OrderBy(c => c.Id).FirstOrDefaultAsync();
            var kh = new CsCustomer
            {
                Code = "KH0001", Name = "Cửa hàng Minh Anh", PhoneNo = "0901234567",
                Email = "minhanh@shop.vn", Address = "12 Lê Lợi, Hà Nội", CompanyId = cty?.Id ?? 0
            };
            db.CsCustomers.Add(kh);
            await db.SaveChangesAsync();

            var team = await db.Teams.OrderBy(t => t.Id).FirstOrDefaultAsync();
            var sla = await db.Slas.OrderBy(s => s.Id).FirstOrDefaultAsync();
            var sp = await db.Products.OrderBy(p => p.Id).FirstOrDefaultAsync();
            var pic = await db.Agents.OrderBy(a => a.Id).Select(a => a.Id).FirstOrDefaultAsync();
            db.Conversations.Add(new Conversation
            {
                Title = "Không tải được hóa đơn PDF", CustomerId = kh.Id, CustomerName = kh.Name,
                ChannelId = ConvConst.CHANNEL_EMAIL, SourceId = ConvConst.SOURCE_OTHER, TypeId = 1,
                StatusId = ConvConst.STATUS_OPEN, PriorityId = ConvConst.PRIORITY_HIGH,
                ProductId = sp?.Id ?? 0, SLAId = sla?.Id ?? 0, TeamId = team?.Id ?? 0, PicId = pic,
                CreatorId = pic, Tags = "hóa đơn,pdf",
                Deadline = DateTime.Now.AddHours(4).ToString(SkyCsService.DangThoiGian),
                FirstResponseTime = 20, MaxResponseTime = 45
            });
            await db.SaveChangesAsync();
            kh.ConversationCount = await db.Conversations.CountAsync(c => c.CustomerId == kh.Id);
            await db.SaveChangesAsync();
        }

        if (!await db.Works.AnyAsync())
        {
            var conv = await db.Conversations.OrderBy(c => c.Id).FirstOrDefaultAsync();
            var wt = await db.WorkTypes.OrderBy(x => x.Id).FirstOrDefaultAsync();
            var wu = await db.WorkUnits.OrderBy(x => x.Id).FirstOrDefaultAsync();
            var agent = await db.Agents.OrderBy(a => a.Id).Select(a => a.Id).FirstOrDefaultAsync();
            var moc = DateTime.Now.AddMinutes(-DateTime.Now.Minute);
            db.Works.Add(new Work
            {
                Name = "Gọi xác nhận thông tin hóa đơn", ConvId = conv?.Id ?? 0, TypeId = wt?.Id ?? 0,
                WUId = wu?.Id ?? 0, UserId = agent, CreatorId = agent, StdTime = 15, Duration = 15,
                Start = moc.AddHours(1).ToString(SkyCsService.DangThoiGian),
                End = moc.AddHours(1).AddMinutes(15).ToString(SkyCsService.DangThoiGian),
                Deadline = moc.AddHours(2).ToString(SkyCsService.DangThoiGian),
                Status = WorkStatuses.Open, Priority = WorkPriorites.Normal, WorkPlace = "Tại chỗ"
            });
            await db.SaveChangesAsync();
        }
    }

    /// <summary>DB Postgres cloud cũ: tạo Orgs + thêm cột OrgId nếu thiếu, backfill về org mặc định. Idempotent.</summary>
    private static async Task MigratePostgresAsync(AppDbContext db)
    {
        if (!db.Database.IsNpgsql()) return;
        var def = TenantContext.DefaultOrgId;
        var tables = new[] { "Agents", "Categories", "Tickets", "Comments", "KbArticles", "Calls" };
        var sql = new List<string>
        {
            "CREATE TABLE IF NOT EXISTS minicskh.\"Orgs\" (\"Id\" uuid PRIMARY KEY, \"Name\" text NOT NULL DEFAULT '', \"ApiKey\" text NOT NULL DEFAULT '', \"CreatedAt\" timestamp NOT NULL DEFAULT now())",
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Orgs_ApiKey\" ON minicskh.\"Orgs\" (\"ApiKey\")",
            // Bảng khảo sát CSAT (đợt bổ sung HCC Survey)
            "CREATE TABLE IF NOT EXISTS minicskh.\"Surveys\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL DEFAULT '" + def + "', \"Code\" text NOT NULL DEFAULT '', \"TicketId\" integer NOT NULL DEFAULT 0, \"CustomerName\" text, \"CustomerPhone\" text, \"Score\" integer NOT NULL DEFAULT 0, \"Comment\" text, \"CreatedAt\" timestamp NOT NULL DEFAULT now(), \"RespondedAt\" timestamp)",
            "CREATE TABLE IF NOT EXISTS minicskh.\"Campaigns\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL DEFAULT '" + def + "', \"Name\" text NOT NULL DEFAULT '', \"Channel\" integer NOT NULL DEFAULT 3, \"Message\" text, \"Status\" integer NOT NULL DEFAULT 0, \"CreatedAt\" timestamp NOT NULL DEFAULT now())",
            "CREATE TABLE IF NOT EXISTS minicskh.\"CampaignTargets\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL DEFAULT '" + def + "', \"CampaignId\" integer NOT NULL DEFAULT 0, \"Phone\" text NOT NULL DEFAULT '', \"Name\" text, \"SentAt\" timestamp)",

            // ── cụm port 1:1 từ 2023.8A.SkyCS ─────────────────────────────────────────────
            // EnsureCreated KHÔNG thêm bảng vào DB đã tồn tại ⇒ phải DDL tay, đúng mẫu nhà ở trên.
            // Tên cột giữ ĐÚNG tên thuộc tính entity (xem Models/SkyCs.cs) để khớp 1:1 với nguồn.
            "CREATE TABLE IF NOT EXISTS minicskh.\"Slas\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL DEFAULT '" + def + "', \"Level\" varchar(10) NOT NULL DEFAULT '', \"Description\" text NOT NULL DEFAULT '', \"FirstResponseTime\" integer NOT NULL DEFAULT 0, \"EveryResponseTime\" integer NOT NULL DEFAULT 0, \"ResolutionTime\" integer NOT NULL DEFAULT 0, \"BusinessHourId\" integer NOT NULL DEFAULT 0, \"ANDConditionDetails\" text, \"ORConditionDetails\" text, \"AvailableStatus\" integer NOT NULL DEFAULT 1)",
            "CREATE TABLE IF NOT EXISTS minicskh.\"BizHours\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL DEFAULT '" + def + "', \"Name\" text NOT NULL DEFAULT '', \"Is247\" boolean NOT NULL DEFAULT false, \"IsDefault\" boolean NOT NULL DEFAULT false, \"IsActive\" boolean NOT NULL DEFAULT true)",
            "CREATE TABLE IF NOT EXISTS minicskh.\"Companies\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL DEFAULT '" + def + "', \"Name\" text NOT NULL DEFAULT '', \"TaxCode\" text)",
            "CREATE TABLE IF NOT EXISTS minicskh.\"CsCustomers\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL DEFAULT '" + def + "', \"Code\" text, \"Name\" text NOT NULL DEFAULT '', \"PhoneNo\" text, \"Address\" text, \"Email\" text, \"CustomerType\" integer NOT NULL DEFAULT 0, \"CompanyId\" integer NOT NULL DEFAULT 0, \"Avatar\" text, \"TimeZone\" integer NOT NULL DEFAULT 0, \"LastActivityDTime\" timestamp NOT NULL DEFAULT now(), \"ConversationCount\" integer NOT NULL DEFAULT 0)",
            "CREATE TABLE IF NOT EXISTS minicskh.\"Teams\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL DEFAULT '" + def + "', \"Name\" text NOT NULL DEFAULT '', \"Image\" text, \"IsAgent\" boolean NOT NULL DEFAULT false, \"IsTechnician\" boolean NOT NULL DEFAULT false, \"Description\" text, \"WorkAssignUserId\" integer NOT NULL DEFAULT 0, \"RankDetails\" text, \"AvailableStatus\" integer NOT NULL DEFAULT 1)",
            "CREATE TABLE IF NOT EXISTS minicskh.\"UserInTeams\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL DEFAULT '" + def + "', \"TeamId\" integer NOT NULL DEFAULT 0, \"UserId\" integer NOT NULL DEFAULT 0, \"Rank\" text)",
            "CREATE TABLE IF NOT EXISTS minicskh.\"Products\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL DEFAULT '" + def + "', \"Code\" text, \"Name\" text NOT NULL DEFAULT '', \"GroupCode\" text, \"BrandName\" text, \"IsActive\" boolean NOT NULL DEFAULT true)",
            "CREATE TABLE IF NOT EXISTS minicskh.\"ConvLookups\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL DEFAULT '" + def + "', \"Kind\" integer NOT NULL DEFAULT 0, \"Code\" integer NOT NULL DEFAULT 0, \"Name\" text NOT NULL DEFAULT '', \"NameCustomer\" text, \"IsSystem\" boolean NOT NULL DEFAULT false, \"IsRequired\" boolean NOT NULL DEFAULT false, \"IsActive\" boolean NOT NULL DEFAULT true)",
            "CREATE INDEX IF NOT EXISTS \"IX_ConvLookups_Kind_Code\" ON minicskh.\"ConvLookups\" (\"Kind\", \"Code\")",
            "CREATE TABLE IF NOT EXISTS minicskh.\"Conversations\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL DEFAULT '" + def + "', \"CustomerId\" integer NOT NULL DEFAULT 0, \"Title\" varchar(300), \"CustomerName\" text, \"CustomerAvatar\" text, \"LastMessageId\" integer NOT NULL DEFAULT 0, \"LastMessageSumary\" text, \"ChannelId\" integer NOT NULL DEFAULT 0, \"SourceId\" integer NOT NULL DEFAULT 0, \"TypeId\" integer NOT NULL DEFAULT 0, \"StatusId\" integer NOT NULL DEFAULT 0, \"PriorityId\" integer NOT NULL DEFAULT 0, \"ProductId\" integer NOT NULL DEFAULT 0, \"BizHourId\" integer NOT NULL DEFAULT 0, \"SLAId\" integer NOT NULL DEFAULT 0, \"Tags\" text, \"Deadline\" text, \"TeamId\" integer NOT NULL DEFAULT 0, \"PicId\" integer NOT NULL DEFAULT 0, \"WorkflowId\" integer NOT NULL DEFAULT 0, \"CreatorId\" integer NOT NULL DEFAULT 0, \"UnreadCount\" integer NOT NULL DEFAULT 0, \"FirstResponseTime\" integer NOT NULL DEFAULT 0, \"MaxResponseTime\" integer NOT NULL DEFAULT 0, \"ResolveDTime\" text, \"IsLastOfCustomer\" boolean NOT NULL DEFAULT false)",
            "CREATE TABLE IF NOT EXISTS minicskh.\"WorkTypes\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL DEFAULT '" + def + "', \"TeamId\" integer NOT NULL DEFAULT 0, \"Name\" text NOT NULL DEFAULT '', \"Description\" text)",
            "CREATE TABLE IF NOT EXISTS minicskh.\"WorkUnits\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL DEFAULT '" + def + "', \"TeamId\" integer NOT NULL DEFAULT 0, \"GroupId\" integer NOT NULL DEFAULT 0, \"TypeId\" integer NOT NULL DEFAULT 0, \"Name\" text NOT NULL DEFAULT '', \"Description\" text, \"StdTime\" integer NOT NULL DEFAULT 0, \"RankDetails\" text, \"RankList\" text)",
            "CREATE TABLE IF NOT EXISTS minicskh.\"Works\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL DEFAULT '" + def + "', \"ConvId\" integer NOT NULL DEFAULT 0, \"ParentId\" integer NOT NULL DEFAULT 0, \"RootId\" integer NOT NULL DEFAULT 0, \"OriginalId\" integer NOT NULL DEFAULT 0, \"TypeId\" integer NOT NULL DEFAULT 0, \"ProductId\" integer NOT NULL DEFAULT 0, \"GroupId\" integer NOT NULL DEFAULT 0, \"MessengerId\" integer NOT NULL DEFAULT 0, \"Idx\" integer NOT NULL DEFAULT 0, \"IsGroup\" boolean NOT NULL DEFAULT false, \"Name\" text NOT NULL DEFAULT '', \"Description\" text, \"WUId\" integer NOT NULL DEFAULT 0, \"TeamId\" integer NOT NULL DEFAULT 0, \"UserId\" integer NOT NULL DEFAULT 0, \"UserName\" text, \"StdTime\" integer NOT NULL DEFAULT 0, \"ActualTime\" integer NOT NULL DEFAULT 0, \"Duration\" integer NOT NULL DEFAULT 0, \"Start\" text, \"End\" text, \"StartActual\" text, \"EndActual\" text, \"Deadline\" text, \"Status\" integer NOT NULL DEFAULT 0, \"Priority\" integer NOT NULL DEFAULT 0, \"WorkDetails\" text, \"Price\" numeric(18,2) NOT NULL DEFAULT 0, \"RefNo\" text, \"RefType\" text, \"Remark\" text, \"WorkPlace\" text, \"CreatorId\" integer NOT NULL DEFAULT 0, \"DeadlineNotifyDTime\" text)",
            "CREATE TABLE IF NOT EXISTS minicskh.\"WorkFollows\" (\"Id\" serial PRIMARY KEY, \"OrgId\" uuid NOT NULL DEFAULT '" + def + "', \"WorkId\" integer NOT NULL DEFAULT 0, \"UserId\" integer NOT NULL DEFAULT 0, \"RootId\" integer NOT NULL DEFAULT 0, \"Role\" integer NOT NULL DEFAULT 0)",
        };
        foreach (var t in tables)
            sql.Add($"ALTER TABLE minicskh.\"{t}\" ADD COLUMN IF NOT EXISTS \"OrgId\" uuid NOT NULL DEFAULT '{def}'");
        foreach (var s in sql)
            try { await db.Database.ExecuteSqlRawAsync(s); } catch { }
    }
}
