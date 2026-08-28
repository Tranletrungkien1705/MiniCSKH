using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MiniCSKH.Data;
using MiniCSKH.Models;
using MiniCSKH.Services;
using Xunit;

namespace MiniCSKH.Tests;

/// <summary>Test CSKH: tạo ticket (mã + New), gán agent, chuyển trạng thái Resolved (đóng), thêm comment, dashboard.</summary>
public class TicketServiceTests
{
    private static async Task<(AppDbContext db, ITicketService svc, SqliteConnection conn)> NewSvc()
    {
        var conn = new SqliteConnection("DataSource=:memory:"); conn.Open();
        var opt = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conn).Options;
        var db = new AppDbContext(opt, new TenantContext { OrgId = TenantContext.DefaultOrgId });
        db.Database.EnsureCreated();
        await Seeder.SeedAsync(db);   // nạp agents/categories/KB mẫu
        return (db, new TicketService(db), conn);
    }

    private static Ticket New(string subject = "Không đăng nhập được")
        => new() { Subject = subject, CustomerName = "KH A", CustomerPhone = "0900", Priority = TicketPriority.High };

    [Fact]
    public async Task Create_StartsNew_WithCode()
    {
        var (db, svc, conn) = await NewSvc(); using (conn)
        {
            var id = await svc.CreateAsync(New());
            var t = await svc.GetAsync(id);
            Assert.Equal(TicketStatus.New, t!.Status);
            Assert.False(string.IsNullOrEmpty(t.Code));
        }
    }

    [Fact]
    public async Task Assign_SetsAgent()
    {
        var (db, svc, conn) = await NewSvc(); using (conn)
        {
            var agent = await db.Agents.FirstAsync();
            var id = await svc.CreateAsync(New());
            await svc.AssignAsync(id, agent.Id);
            Assert.Equal(agent.Id, (await svc.GetAsync(id))!.AssignedAgentId);
        }
    }

    [Fact]
    public async Task ChangeStatus_Resolved_ClosesTicket()
    {
        var (db, svc, conn) = await NewSvc(); using (conn)
        {
            var id = await svc.CreateAsync(New());
            await svc.ChangeStatusAsync(id, TicketStatus.Resolved);
            var t = await svc.GetAsync(id);
            Assert.Equal(TicketStatus.Resolved, t!.Status);
            Assert.False(t.IsOpen);
        }
    }

    [Fact]
    public async Task AddComment_AppendsComment()
    {
        var (db, svc, conn) = await NewSvc(); using (conn)
        {
            var id = await svc.CreateAsync(New());
            await svc.AddCommentAsync(id, "Agent", "Đã kiểm tra", true);
            var cmts = (await svc.GetAsync(id))!.Comments;
            Assert.Contains(cmts, c => c.Body == "Đã kiểm tra");   // ngoài comment hệ thống lúc tạo
        }
    }

    [Fact]
    public async Task Dashboard_CountsOpen()
    {
        var (db, svc, conn) = await NewSvc(); using (conn)
        {
            var openBefore = (await svc.DashboardAsync()).Open;
            await svc.CreateAsync(New());
            Assert.Equal(openBefore + 1, (await svc.DashboardAsync()).Open);
        }
    }

    [Fact]
    public async Task LogCall_And_Stats()
    {
        var (db, svc, conn) = await NewSvc(); using (conn)
        {
            await svc.LogCallAsync(new CallLog { Direction = CallDirection.Inbound, Outcome = CallOutcome.Missed, PhoneNumber = "0900", DurationSeconds = 0 });
            var (today, missed, _) = await svc.CallStatsAsync();
            Assert.True(today >= 1);
            Assert.True(missed >= 1);
        }
    }
}
