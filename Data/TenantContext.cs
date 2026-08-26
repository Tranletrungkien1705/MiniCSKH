namespace MiniCSKH.Data;

/// <summary>Ngữ cảnh tenant của request. Middleware set OrgId (cookie org_key hoặc header X-Api-Key), DbContext lọc.</summary>
public interface ITenantContext
{
    Guid OrgId { get; set; }
}

public sealed class TenantContext : ITenantContext
{
    /// <summary>Org mặc định (dữ liệu seed + khi chưa chọn tổ chức). Cố định để ổn định qua các lần khởi động.</summary>
    public static readonly Guid DefaultOrgId = new("33333333-3333-3333-3333-333333333333");
    public const string DefaultApiKey = "demo-cskh";
    public const string CookieName = "org_key";

    public Guid OrgId { get; set; } = DefaultOrgId;
}
