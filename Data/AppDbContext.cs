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
