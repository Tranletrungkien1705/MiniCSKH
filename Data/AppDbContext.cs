using Microsoft.EntityFrameworkCore;
using MiniCSKH.Models;

namespace MiniCSKH.Data;

public class AppDbContext : DbContext
{
    private readonly Guid _orgId;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenant) : base(options)
        => _orgId = tenant.OrgId;

    public DbSet<Org> Orgs => Set<Org>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<TicketCategory> Categories => Set<TicketCategory>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> Comments => Set<TicketComment>();
    public DbSet<KbArticle> KbArticles => Set<KbArticle>();
    public DbSet<CallLog> Calls => Set<CallLog>();
    public DbSet<SurveyResponse> Surveys => Set<SurveyResponse>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignTarget> CampaignTargets => Set<CampaignTarget>();

    // ── cụm port 1:1 từ 2023.8A.SkyCS — xem Models/SkyCs.cs ─────────────────────────
    public DbSet<Sla> Slas => Set<Sla>();
    public DbSet<BizHour> BizHours => Set<BizHour>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CsCustomer> CsCustomers => Set<CsCustomer>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<UserInTeam> UserInTeams => Set<UserInTeam>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ConvLookup> ConvLookups => Set<ConvLookup>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<WorkType> WorkTypes => Set<WorkType>();
    public DbSet<WorkUnit> WorkUnits => Set<WorkUnit>();
    public DbSet<Work> Works => Set<Work>();
    public DbSet<WorkFollow> WorkFollows => Set<WorkFollow>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        if (Database.IsNpgsql()) b.HasDefaultSchema("minicskh");
        b.Entity<Org>().HasIndex(x => x.ApiKey).IsUnique();
        b.Entity<Agent>().HasQueryFilter(x => x.OrgId == _orgId);
        b.Entity<TicketCategory>().HasQueryFilter(x => x.OrgId == _orgId);
        b.Entity<KbArticle>().HasQueryFilter(x => x.OrgId == _orgId);
        b.Entity<CallLog>(e =>
        {
            e.Ignore(x => x.DurationText);
            e.HasOne(x => x.Agent).WithMany().HasForeignKey(x => x.AgentId);
            e.HasOne(x => x.Ticket).WithMany().HasForeignKey(x => x.TicketId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Ticket>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(20);
            e.Property(x => x.Subject).HasMaxLength(300);
            e.HasIndex(x => x.Code);
            e.Ignore(x => x.IsOpen);
            e.Ignore(x => x.IsOverdue);
            e.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId);
            e.HasOne(x => x.AssignedAgent).WithMany().HasForeignKey(x => x.AssignedAgentId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<TicketComment>(e =>
        {
            e.HasOne(x => x.Ticket).WithMany(x => x.Comments).HasForeignKey(x => x.TicketId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<SurveyResponse>(e =>
        {
            e.Ignore(x => x.Responded);
            e.HasIndex(x => x.Code);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Campaign>(e =>
        {
            e.Ignore(x => x.Total); e.Ignore(x => x.Reached);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<CampaignTarget>(e =>
        {
            e.HasOne(x => x.Campaign).WithMany(x => x.Targets).HasForeignKey(x => x.CampaignId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });

        CauHinhSkyCs(b);
    }

    /// <summary>
    /// Cấu hình cho cụm port 1:1 từ 2023.8A.SkyCS. Mọi bảng đều lọc theo tenant giống mẫu nhà.
    /// Các thuộc tính CHỈ-TÍNH (không phải cột nguồn) phải Ignore, nếu không EF sinh cột thừa
    /// ⇒ lệch 1:1 về cột. Danh sách: Sla.IsActive/*Desc (ánh xạ của AvailableStatus + phút),
    /// Conversation.TagList, WorkUnit.Duration, Work.StatusDesc.
    ///
    /// KHÔNG dựng khóa ngoại thật cho các cột *Id của cụm này, và các thuộc tính điều hướng
    /// (Customer/Team/Product/SLA/Pic/WorkType/WorkUnit) đều Ignore — có LÝ DO 1:1, không phải cho tiện:
    ///  • Nguồn coi giá trị <c>0</c> là "không chọn" (ConvEdit.cshtml:96/130 và AddWork.cshtml:118/204
    ///    đều có &lt;option value="0"&gt; rỗng). Dựng ràng buộc FK thì 0 vi phạm ngay ⇒ lưu là lỗi.
    ///    Đã sập thật: SQLite Error 19 'FOREIGN KEY constraint failed' khi tạo công việc không chọn loại.
    ///  • Thuộc tính <c>[ForeignKey]</c> ở nguồn (vd Models/Work.cs:64, Conversation.cs:57) là siêu dữ liệu
    ///    của tầng mbiz để join, KHÔNG sinh ràng buộc trong DB.
    ///  • Nguồn cũng nạp đối tượng liên quan bằng lệnh Get RIÊNG, không join sẵn:
    ///    <c>WorkManager.cs:GetDetail</c> gọi WorkTypeManager.Get / WorkUnitManager.Get / ProductManager.Get,
    ///    <c>ConversationController.cs:1101</c> gọi CustomerManager.Get.
    ///    Vì vậy Services/SkyCsService.cs cũng nạp tay — đúng đường đi của nguồn.
    /// </summary>
    private void CauHinhSkyCs(ModelBuilder b)
    {
        b.Entity<Sla>(e =>
        {
            e.Ignore(x => x.IsActive);
            e.Ignore(x => x.FirstResponseTimeDesc);
            e.Ignore(x => x.EveryResponseTimeDesc);
            e.Ignore(x => x.ResolutionTimeDesc);
            e.Property(x => x.Level).HasMaxLength(10);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<BizHour>().HasQueryFilter(x => x.OrgId == _orgId);
        b.Entity<Company>().HasQueryFilter(x => x.OrgId == _orgId);
        b.Entity<CsCustomer>(e =>
        {
            e.Ignore(x => x.Company);            // xem ghi chú "KHÔNG dựng khóa ngoại" ở dưới
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Team>().HasQueryFilter(x => x.OrgId == _orgId);
        b.Entity<UserInTeam>().HasQueryFilter(x => x.OrgId == _orgId);
        b.Entity<Product>().HasQueryFilter(x => x.OrgId == _orgId);
        b.Entity<ConvLookup>(e =>
        {
            e.HasIndex(x => new { x.Kind, x.Code });
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Conversation>(e =>
        {
            e.Ignore(x => x.TagList);
            e.Ignore(x => x.IsSlaSatisfied);       // nguồn Conversation.cs:217 — tính bởi CalcSlaSatisfaction, không lưu
            e.Property(x => x.Title).HasMaxLength(300);
            // KHÔNG dựng khóa ngoại cho CustomerId/TeamId/ProductId/SLAId/PicId — xem ghi chú ở dưới.
            e.Ignore(x => x.Customer);
            e.Ignore(x => x.Team);
            e.Ignore(x => x.Product);
            e.Ignore(x => x.SLA);
            e.Ignore(x => x.Pic);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<WorkType>().HasQueryFilter(x => x.OrgId == _orgId);
        b.Entity<WorkUnit>(e =>
        {
            e.Ignore(x => x.Duration);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Work>(e =>
        {
            e.Ignore(x => x.StatusDesc);
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.Ignore(x => x.WorkType);           // xem ghi chú "KHÔNG dựng khóa ngoại" ở dưới
            e.Ignore(x => x.WorkUnit);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<WorkFollow>(e =>
        {
            e.HasOne(x => x.Work).WithMany(x => x.Followers).HasForeignKey(x => x.WorkId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
    }

    public override int SaveChanges() { StampOrg(); return base.SaveChanges(); }
    public override Task<int> SaveChangesAsync(CancellationToken ct = default) { StampOrg(); return base.SaveChangesAsync(ct); }

    private void StampOrg()
    {
        foreach (var entry in ChangeTracker.Entries<IOrgOwned>())
            if (entry.State == EntityState.Added && entry.Entity.OrgId == Guid.Empty)
                entry.Entity.OrgId = _orgId;
    }
}
