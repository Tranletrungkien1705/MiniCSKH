using Microsoft.EntityFrameworkCore;
using MiniCSKH.Models;

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
        };
        foreach (var t in tables)
            sql.Add($"ALTER TABLE minicskh.\"{t}\" ADD COLUMN IF NOT EXISTS \"OrgId\" uuid NOT NULL DEFAULT '{def}'");
        foreach (var s in sql)
            try { await db.Database.ExecuteSqlRawAsync(s); } catch { }
    }
}
