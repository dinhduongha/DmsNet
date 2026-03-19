//namespace Hano.Core.Domain.Shared.Identity;
namespace Bamboo.Shared.Common;
/// <summary>
/// DMS role name constants.
/// Import processing order defines priority: higher index = lower priority for username conflict resolution.
/// </summary>
public static class DmsRoles
{
    public const string Admin = "dms_admin";
    public const string SaleManager = "dms_sale_manager";    // ASM (Trưởng vùng)
    public const string SalesSupervisor = "dms_sale_supervisor"; // GSBH (Giám sát bán hàng)
    public const string SalesUser = "dms_sale_user";       // NVBH (Nhân viên bán hàng)

    /// <summary>
    /// Priority order for username allocation: lower index = higher priority.
    /// Higher-priority roles claim base usernames; lower-priority roles get a numeric suffix on conflict.
    /// </summary>
    public static readonly string[] PriorityOrder =
        [Admin, SaleManager, SalesSupervisor, SalesUser];
}
