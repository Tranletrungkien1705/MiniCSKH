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
        if (!await db.TicketTypes.AnyAsync())
        {
            db.TicketTypes.AddRange(
                new TicketType { Code = "TT-SUPPORT", AgentName = "Yêu cầu hỗ trợ", CustomerName = "Hỗ trợ kỹ thuật", BusinessType = BusinessType.ETicket, Order = 1, IsActive = true, Remark = "Phân loại mặc định cho phiếu hỗ trợ kỹ thuật.", CreateTemplate = "SCR-ET-CREATE", DetailTemplate = "SCR-ET-DETAIL", CreatedBy = "Hệ thống" },
                new TicketType { Code = "TT-COMPLAINT", AgentName = "Khiếu nại dịch vụ", CustomerName = "Phản ánh / Khiếu nại", BusinessType = BusinessType.ETicket, Order = 2, IsActive = true, Remark = "Tiếp nhận phản ánh, khiếu nại của khách hàng.", CreateTemplate = "SCR-ET-CREATE", DetailTemplate = "SCR-ET-DETAIL", CreatedBy = "Hệ thống" },
                new TicketType { Code = "TT-SURVEY", AgentName = "Khảo sát sau bán", CustomerName = "Khảo sát hài lòng", BusinessType = BusinessType.Campaign, Order = 3, IsActive = true, Remark = "Dùng cho chiến dịch gọi khảo sát hài lòng.", CreatedBy = "Hệ thống" },
                new TicketType { Code = "TT-OLD", AgentName = "Phân loại cũ (ngừng dùng)", CustomerName = "Phân loại cũ", BusinessType = BusinessType.ETicket, Order = 9, IsActive = false, Remark = "Đã ngừng sử dụng.", CreatedBy = "Hệ thống" });
            await db.SaveChangesAsync();
        }
        if (!await db.SlaPolicies.AnyAsync())
        {
            db.SlaPolicies.AddRange(
                new SlaPolicy { Code = "SLA-VIP", Level = "VIP / Khẩn cấp", Description = "Khách VIP, sự cố nghiêm trọng — phản hồi nhanh, xử lý trong ngày.", FirstResMinutes = 15, ResolutionMinutes = 240 },
                new SlaPolicy { Code = "SLA-STD", Level = "Tiêu chuẩn", Description = "Mức mặc định cho hầu hết phiếu hỗ trợ.", FirstResMinutes = 60, ResolutionMinutes = 480 },
                new SlaPolicy { Code = "SLA-LOW", Level = "Thấp / Tư vấn", Description = "Câu hỏi tư vấn, không gấp.", FirstResMinutes = 240, ResolutionMinutes = 2880 });
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
            var slas = await db.SlaPolicies.ToListAsync();
            int n = 0;
            Ticket T(string subj, string cust, Channel ch, TicketPriority pri, TicketStatus st, int catIdx, int? agentIdx, int ageHours, int slaIdx, int? firstResMin)
            {
                n++;
                var created = DateTime.Now.AddHours(-ageHours);
                var cat = cats[catIdx];
                var sla = slas[slaIdx];
                var done = st is TicketStatus.Resolved or TicketStatus.Closed;
                return new Ticket
                {
                    Code = $"TK{DateTime.Now:yyMM}{n:D4}",
                    Subject = subj, CustomerName = cust, Channel = ch, Priority = pri, Status = st,
                    CategoryId = cat.Id, AssignedAgentId = agentIdx is { } ai ? agents[ai].Id : null,
                    SlaPolicyId = sla.Id,
                    CreatedAt = created, DueAt = created.AddHours(cat.SlaHours),
                    FirstResponseAt = firstResMin is { } fm ? created.AddMinutes(fm) : null,
                    ResolvedAt = done ? created.AddMinutes(sla.ResolutionMinutes + 30) : null,
                    Description = "Nội dung yêu cầu từ khách hàng.",
                    Comments = [ new TicketComment { Author = "Hệ thống", Body = "Phiếu được tạo.", CreatedAt = created } ]
                };
            }
            db.Tickets.AddRange(
                T("Không tải được hóa đơn PDF", "Cửa hàng Minh Anh", Channel.Email, TicketPriority.High, TicketStatus.New, 1, null, 1, 1, null),
                T("Sản phẩm giao bị lỗi", "Shop thời trang Hà", Channel.Zalo, TicketPriority.Urgent, TicketStatus.InProgress, 2, 0, 6, 0, 10),
                T("Hỏi chính sách bảo hành", "Đại lý Phương Nam", Channel.Phone, TicketPriority.Normal, TicketStatus.WaitingCustomer, 3, 1, 30, 2, 90),
                T("Cần xuất lại hóa đơn sai MST", "Công ty ABC", Channel.Web, TicketPriority.High, TicketStatus.Resolved, 1, 2, 50, 1, 45),
                T("Tư vấn chọn size vợt", "Nguyễn Văn A", Channel.Web, TicketPriority.Low, TicketStatus.Closed, 3, 0, 72, 2, 30)
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

        if (!await db.Campaigns.AnyAsync())
        {
            var agents = await db.Agents.ToListAsync();
            db.Campaigns.AddRange(
                new Campaign
                {
                    Code = $"CP{DateTime.Now:yyMM}0001", Name = "Chăm sóc khách VIP quý này",
                    CampaignType = "Telesales", Status = CampaignStatus.Started,
                    Description = "Gọi hỏi thăm + upsell cho nhóm khách VIP.",
                    CreatedAt = DateTime.Now.AddDays(-5), ApprovedAt = DateTime.Now.AddDays(-4), StartAt = DateTime.Now.AddDays(-3),
                    Customers =
                    [
                        new CampaignCustomer { CustomerName = "Cửa hàng Minh Anh", PhoneNumber = "0901234567", Company = "Minh Anh", AgentId = agents[0].Id, Status = CampaignCustomerStatus.Done, Feedback = "Hài lòng, muốn mua thêm.", LastCallAt = DateTime.Now.AddDays(-2), CallCount = 1 },
                        new CampaignCustomer { CustomerName = "Shop thời trang Hà", PhoneNumber = "0912345678", Company = "Hà Fashion", AgentId = agents[1].Id, Status = CampaignCustomerStatus.CallAgain, Remark = "Hẹn gọi lại sau 17h.", LastCallAt = DateTime.Now.AddDays(-1), CallCount = 2 },
                        new CampaignCustomer { CustomerName = "Đại lý Phương Nam", PhoneNumber = "0934567890", Company = "Phương Nam", Status = CampaignCustomerStatus.Pending }
                    ]
                },
                new Campaign
                {
                    Code = $"CP{DateTime.Now:yyMM}0002", Name = "Khảo sát hài lòng sau bán",
                    CampaignType = "Survey", Status = CampaignStatus.Pending,
                    Description = "Gọi khảo sát mức độ hài lòng sau khi đóng phiếu.",
                    CreatedAt = DateTime.Now.AddDays(-1),
                    Customers =
                    [
                        new CampaignCustomer { CustomerName = "Công ty ABC", PhoneNumber = "0945678901", Company = "ABC", Status = CampaignCustomerStatus.Pending },
                        new CampaignCustomer { CustomerName = "Nguyễn Văn A", PhoneNumber = "0956789012", Status = CampaignCustomerStatus.Pending }
                    ]
                }
            );
            await db.SaveChangesAsync();
        }

        if (!await db.Ratings.AnyAsync())
        {
            var tickets = await db.Tickets.OrderBy(t => t.Id).ToListAsync();
            if (tickets.Count >= 2)
            {
                // Phiếu đã giải quyết/đóng được khách đánh giá (RATE); một số đã được agent kiểm soát (REVIEW).
                var t1 = tickets.FirstOrDefault(t => t.Status == TicketStatus.Resolved) ?? tickets[0];
                var t2 = tickets.FirstOrDefault(t => t.Status == TicketStatus.Closed) ?? tickets[^1];
                t1.FlagRated = true;
                t2.FlagRated = true;
                db.Ratings.AddRange(
                    new TicketRating { TicketId = t1.Id, FormCode = "SAT-STD", RateRound = 1, RateType = RateType.Rate, Status = RateStatus.Rated, Result = RateResult.Satisfied, Score = 5, Comment = "Xử lý nhanh, nhân viên nhiệt tình.", RatedBy = t1.CustomerName, RatedAt = DateTime.Now.AddDays(-1) },
                    new TicketRating { TicketId = t2.Id, FormCode = "SAT-STD", RateRound = 1, RateType = RateType.Review, Status = RateStatus.Reviewed, Result = RateResult.Neutral, Score = 3, Comment = "Tạm ổn nhưng chờ hơi lâu.", RatedBy = t2.CustomerName, ReviewedBy = "Trần Văn Minh", ReviewNote = "Đã nhắc nhở agent về thời gian phản hồi.", RatedAt = DateTime.Now.AddDays(-2) }
                );
                await db.SaveChangesAsync();
            }
        }

        if (!await db.SurveyForms.AnyAsync())
        {
            db.SurveyForms.AddRange(
                new SurveyForm
                {
                    Code = "SAT-STD", Name = "Khảo sát hài lòng tiêu chuẩn",
                    Description = "Mẫu mặc định đánh giá phiếu hỗ trợ sau khi đóng.",
                    IsActive = true, CreatedAt = DateTime.Now.AddDays(-10), UpdatedAt = DateTime.Now.AddDays(-10),
                    CreatedBy = "Hệ thống", UsedAt = DateTime.Now.AddDays(-1),
                    Fields =
                    [
                        new SurveyFormField { Code = "F1", Name = "Mức độ hài lòng chung", FieldType = SurveyFieldType.Rating, Order = 1, IsRequired = true },
                        new SurveyFormField { Code = "F2", Name = "Thái độ nhân viên", FieldType = SurveyFieldType.SingleChoice, Order = 2, IsRequired = true, Options = "Rất tốt|Tốt|Bình thường|Kém" },
                        new SurveyFormField { Code = "F3", Name = "Thời gian xử lý", FieldType = SurveyFieldType.SingleChoice, Order = 3, Options = "Nhanh|Chấp nhận được|Chậm" },
                        new SurveyFormField { Code = "F4", Name = "Góp ý thêm", FieldType = SurveyFieldType.Text, Order = 4 }
                    ]
                },
                new SurveyForm
                {
                    Code = "SAT-VIP", Name = "Khảo sát khách VIP",
                    Description = "Mẫu khảo sát chi tiết dành cho khách hàng VIP.",
                    IsActive = true, CreatedAt = DateTime.Now.AddDays(-5), UpdatedAt = DateTime.Now.AddDays(-5),
                    CreatedBy = "Hệ thống",
                    Fields =
                    [
                        new SurveyFormField { Code = "F1", Name = "Mức độ hài lòng chung", FieldType = SurveyFieldType.Rating, Order = 1, IsRequired = true },
                        new SurveyFormField { Code = "F2", Name = "Chất lượng tư vấn", FieldType = SurveyFieldType.Rating, Order = 2, IsRequired = true },
                        new SurveyFormField { Code = "F3", Name = "Khả năng quay lại / giới thiệu", FieldType = SurveyFieldType.SingleChoice, Order = 3, Options = "Chắc chắn|Có thể|Không" },
                        new SurveyFormField { Code = "F4", Name = "Điểm cần cải thiện", FieldType = SurveyFieldType.MultiChoice, Order = 4, Options = "Thời gian chờ|Thái độ|Chuyên môn|Kênh liên hệ" }
                    ]
                }
            );
            await db.SaveChangesAsync();
        }

        if (!await db.ServiceImprovements.AnyAsync())
        {
            db.ServiceImprovements.AddRange(
                new ServiceImprovement
                {
                    Code = "SI-CALL", Name = "Đánh giá chất lượng gọi ra",
                    ItemType = SvImprvItemType.Honorific,
                    Remark = "Bộ tiêu chí chấm điểm cuộc gọi telesales.",
                    IsActive = true, CreatedAt = DateTime.Now.AddDays(-8), UpdatedAt = DateTime.Now.AddDays(-8),
                    CreatedBy = "Hệ thống", UsedAt = DateTime.Now.AddDays(-2),
                    Criteria =
                    [
                        new SvImprvCriterion { Kind = SvImprvItemType.Honorific, Word = "Chào hỏi đúng kính ngữ (Anh/Chị)", QtyStd = 1, IsRequired = true },
                        new SvImprvCriterion { Kind = SvImprvItemType.Honorific, Word = "Cảm ơn khách khi kết thúc", QtyStd = 1, IsRequired = true },
                        new SvImprvCriterion { Kind = SvImprvItemType.DenyWord, Word = "Từ cấm: 'không biết', 'tùy bạn'", QtyStd = 0 },
                        new SvImprvCriterion { Kind = SvImprvItemType.CallTalkTime, Word = "Thời lượng gọi hợp lý", MinValue = 60, MaxValue = 600 }
                    ]
                },
                new ServiceImprovement
                {
                    Code = "SI-AUDIO", Name = "Phân tích audio cuộc gọi",
                    ItemType = SvImprvItemType.Audio,
                    Remark = "Tiêu chí phân tích tự động từ file ghi âm.",
                    IsActive = true, CreatedAt = DateTime.Now.AddDays(-3), UpdatedAt = DateTime.Now.AddDays(-3),
                    CreatedBy = "Hệ thống",
                    Criteria =
                    [
                        new SvImprvCriterion { Kind = SvImprvItemType.Audio, Word = "Tỷ lệ im lặng", MinValue = 0, MaxValue = 30, QtyAllow = 2 },
                        new SvImprvCriterion { Kind = SvImprvItemType.Audio, Word = "Tốc độ nói (từ/phút)", MinValue = 100, MaxValue = 180, QtyAllow = 1 }
                    ]
                }
            );
            await db.SaveChangesAsync();
        }
    }

    /// <summary>DB Postgres cloud cũ: tạo Orgs + thêm cột OrgId nếu thiếu, backfill về org mặc định. Idempotent.</summary>
    private static async Task MigratePostgresAsync(AppDbContext db)
    {
        if (!db.Database.IsNpgsql()) return;
        var def = TenantContext.DefaultOrgId;
        var tables = new[] { "Agents", "Categories", "TicketTypes", "SlaPolicies", "Tickets", "Comments", "KbArticles", "Calls", "Campaigns", "CampaignCustomers", "Ratings", "SurveyForms", "SurveyFormFields", "ServiceImprovements", "SvImprvCriteria" };
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
