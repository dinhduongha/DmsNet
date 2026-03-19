using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Hano.Core.Domain.Entities;

/// <summary>
/// Region-level OrganizationUnit metadata.
/// One record per Region OU — stores which admin and ASM are responsible for the region.
/// </summary>
[Table("regions")]
public class Organization : AuditedEntity<Guid>, IMultiTenant
{
    [Key]
    [Column("id")]
    public new Guid Id { get => base.Id; set => base.Id = value; }

    [Column("tenant_id")]
    public Guid? TenantId { get; set; }

    [Column("organization_unit_id")]
    public Guid OrganizationUnitId { get; set; }

    /// <summary>dms_admin user responsible for this region.</summary>
    [Column("admin_user_id")]
    public Guid? AdminUserId { get; set; }

    /// <summary>dms_sale_manager (ASM) who leads this region.</summary>
    [Column("sale_manager_user_id")]
    public Guid? SaleManagerUserId { get; set; }
}
