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
    public DbSet<TicketType> TicketTypes => Set<TicketType>();
    public DbSet<SlaPolicy> SlaPolicies => Set<SlaPolicy>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> Comments => Set<TicketComment>();
    public DbSet<KbArticle> KbArticles => Set<KbArticle>();
    public DbSet<CallLog> Calls => Set<CallLog>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignCustomer> CampaignCustomers => Set<CampaignCustomer>();
    public DbSet<TicketRating> Ratings => Set<TicketRating>();
    public DbSet<SurveyForm> SurveyForms => Set<SurveyForm>();
    public DbSet<SurveyFormField> SurveyFormFields => Set<SurveyFormField>();
    public DbSet<ServiceImprovement> ServiceImprovements => Set<ServiceImprovement>();
    public DbSet<SvImprvCriterion> SvImprvCriteria => Set<SvImprvCriterion>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerContact> CustomerContacts => Set<CustomerContact>();
    public DbSet<CustomerHistory> CustomerHistories => Set<CustomerHistory>();
    public DbSet<CustomerGroup> CustomerGroups => Set<CustomerGroup>();
    public DbSet<AllocateRule> AllocateRules => Set<AllocateRule>();
    public DbSet<AllocateAgent> AllocateAgents => Set<AllocateAgent>();
    public DbSet<ReminderRule> ReminderRules => Set<ReminderRule>();
    public DbSet<TicketCatalog> TicketCatalogs => Set<TicketCatalog>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<DepartmentMember> DepartmentMembers => Set<DepartmentMember>();
    public DbSet<PaymentTerm> PaymentTerms => Set<PaymentTerm>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        if (Database.IsNpgsql()) b.HasDefaultSchema("minicskh");
        b.Entity<Org>().HasIndex(x => x.ApiKey).IsUnique();
        b.Entity<Agent>().HasQueryFilter(x => x.OrgId == _orgId);
        b.Entity<TicketCategory>().HasQueryFilter(x => x.OrgId == _orgId);
        b.Entity<TicketType>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(30);
            e.Property(x => x.AgentName).HasMaxLength(200);
            e.Property(x => x.CustomerName).HasMaxLength(200);
            e.HasIndex(x => x.Code);
            e.Ignore(x => x.BusinessTypeName);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<SlaPolicy>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(30);
            e.Property(x => x.Level).HasMaxLength(120);
            e.HasIndex(x => x.Code);
            e.Ignore(x => x.FirstResText);
            e.Ignore(x => x.ResolutionText);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
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
            e.Ignore(x => x.ActualFirstResMinutes);
            e.Ignore(x => x.ActualResolutionMinutes);
            e.Ignore(x => x.ViolatesFirstResponse);
            e.Ignore(x => x.ViolatesResolution);
            e.Ignore(x => x.ViolatesSla);
            e.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId);
            e.HasOne(x => x.AssignedAgent).WithMany().HasForeignKey(x => x.AssignedAgentId);
            e.HasOne(x => x.SlaPolicy).WithMany().HasForeignKey(x => x.SlaPolicyId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<TicketComment>(e =>
        {
            e.HasOne(x => x.Ticket).WithMany(x => x.Comments).HasForeignKey(x => x.TicketId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Campaign>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(20);
            e.Property(x => x.Name).HasMaxLength(200);
            e.HasIndex(x => x.Code);
            e.Ignore(x => x.TotalCustomers);
            e.Ignore(x => x.DoneCustomers);
            e.Ignore(x => x.ProgressPercent);
            e.Ignore(x => x.IsRunning);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<CampaignCustomer>(e =>
        {
            e.HasOne(x => x.Campaign).WithMany(x => x.Customers).HasForeignKey(x => x.CampaignId);
            e.HasOne(x => x.Agent).WithMany().HasForeignKey(x => x.AgentId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<TicketRating>(e =>
        {
            e.Property(x => x.FormCode).HasMaxLength(30);
            e.Ignore(x => x.Stars);
            e.HasOne(x => x.Ticket).WithMany(x => x.Ratings).HasForeignKey(x => x.TicketId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<SurveyForm>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(30);
            e.Property(x => x.Name).HasMaxLength(200);
            e.HasIndex(x => x.Code);
            e.Ignore(x => x.FieldCount);
            e.Ignore(x => x.RequiredCount);
            e.Ignore(x => x.IsUsed);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<SurveyFormField>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(50);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Ignore(x => x.OptionList);
            e.HasOne(x => x.SurveyForm).WithMany(x => x.Fields).HasForeignKey(x => x.SurveyFormId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<ServiceImprovement>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(30);
            e.Property(x => x.Name).HasMaxLength(200);
            e.HasIndex(x => x.Code);
            e.Ignore(x => x.CriterionCount);
            e.Ignore(x => x.RequiredCount);
            e.Ignore(x => x.IsUsed);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<SvImprvCriterion>(e =>
        {
            e.Property(x => x.Word).HasMaxLength(300);
            e.Ignore(x => x.RangeText);
            e.HasOne(x => x.ServiceImprovement).WithMany(x => x.Criteria).HasForeignKey(x => x.ServiceImprovementId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<CustomerGroup>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(30);
            e.Property(x => x.Name).HasMaxLength(200);
            e.HasIndex(x => x.Code);
            e.Ignore(x => x.CustomerCount);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Customer>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(30);
            e.Property(x => x.Name).HasMaxLength(300);
            e.HasIndex(x => x.Code);
            e.Ignore(x => x.ContactCount);
            e.Ignore(x => x.TypeName);
            e.Ignore(x => x.Initials);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<CustomerContact>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200);
            e.HasOne(x => x.Owner).WithMany(x => x.Contacts).HasForeignKey(x => x.CustomerId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<CustomerHistory>(e =>
        {
            e.HasOne(x => x.Owner).WithMany(x => x.Histories).HasForeignKey(x => x.CustomerId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<AllocateRule>(e =>
        {
            e.Property(x => x.DepartmentCode).HasMaxLength(30);
            e.Ignore(x => x.AgentCount);
            e.Ignore(x => x.ModeText);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<AllocateAgent>(e =>
        {
            e.Property(x => x.AgentCode).HasMaxLength(30);
            e.HasOne(x => x.Rule).WithMany(x => x.Agents).HasForeignKey(x => x.AllocateRuleId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<ReminderRule>(e =>
        {
            e.Property(x => x.EstablishId).HasMaxLength(30);
            e.HasIndex(x => x.EstablishId);
            e.Ignore(x => x.ChannelCount);
            e.Ignore(x => x.Channels);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<TicketCatalog>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(50);
            e.Property(x => x.AgentName).HasMaxLength(200);
            e.Property(x => x.CustomerName).HasMaxLength(200);
            e.HasIndex(x => new { x.Kind, x.Code });
            e.Ignore(x => x.KindName);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Department>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(50);
            e.Property(x => x.Name).HasMaxLength(200);
            e.HasIndex(x => x.Code);
            e.Ignore(x => x.MemberCount);
            e.Ignore(x => x.IsRoot);
            e.Ignore(x => x.LevelName);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<DepartmentMember>(e =>
        {
            e.Property(x => x.UserCode).HasMaxLength(50);
            e.Property(x => x.FullName).HasMaxLength(200);
            e.HasOne(x => x.Department).WithMany(x => x.Members).HasForeignKey(x => x.DepartmentId);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<PaymentTerm>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(30);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.CreditLimit).HasPrecision(18, 2);
            e.Property(x => x.DepositPercent).HasPrecision(18, 2);
            e.HasIndex(x => x.Code);
            e.Ignore(x => x.TypeName);
            e.Ignore(x => x.OwedDayText);
            e.Ignore(x => x.CreditLimitText);
            e.Ignore(x => x.DepositText);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Area>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(50);
            e.Property(x => x.Name).HasMaxLength(200);
            e.HasIndex(x => x.Code);
            e.Ignore(x => x.IsRoot);
            e.Ignore(x => x.LevelName);
            e.HasQueryFilter(x => x.OrgId == _orgId);
        });
        b.Entity<Tag>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(50);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Slug).HasMaxLength(200);
            e.HasIndex(x => x.Code);
            e.Ignore(x => x.SlugText);
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
