namespace Hano.Core.Import.Plan;

/// <summary>An admin user to create (dms_admin role).</summary>
public record AdminSpec(int Index, string DisplayName, string Region, string Username, string Password);

/// <summary>An ASM user to create (dms_sale_manager role) — embedded in RegionSpec.</summary>
public record AsmSpec(string DisplayName, string Username, string Password);

/// <summary>A region OU to create/reuse, with its optional ASM.</summary>
public record RegionSpec(int Index, string RegionName, string? RegionCode, AsmSpec? Asm);

/// <summary>A GSBH team: a child OU under a region + the supervisor user (dms_sale_supervisor).</summary>
public record TeamSpec(int Index, string DisplayName, string Region, string TeamOuName, string Username, string Password);

/// <summary>An NVBH user to create (dms_sale_user role).</summary>
public record SalesUserSpec(int Index, string DisplayName, string Region, string Username, string Password);

/// <summary>A distributor (NPP) to create, with its tenant.</summary>
public record DistributorSpec(int Index, string Region, string CustomerCode, string Name, string Province, string Address);

/// <summary>
/// The full import plan built from parsed Excel data.
/// All objects are decided in-memory before any DB operation.
/// </summary>
public record ImportPlan(
    List<AdminSpec> Admins,
    List<RegionSpec> Regions,
    List<TeamSpec> Teams,
    List<SalesUserSpec> SalesUsers,
    List<DistributorSpec> Distributors);
