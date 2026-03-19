using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace Hano.Core.Domain.Entities;

/// <summary>
/// GSBH team-level OrganizationUnit metadata.
/// One record per GSBH Team OU (child of Region OU) — stores the ASM manager and GSBH supervisor.
/// </summary>
[Table("dms_teams")]
public class DmsTeam : AuditedEntity<Guid>
{
    [Key]
    [Column("id")]
    public new Guid Id { get => base.Id; set => base.Id = value; }

    [Column("organization_unit_id")]
    public Guid OrganizationUnitId { get; set; }

    /// <summary>dms_sale_manager (ASM) who manages this team's region.</summary>
    [Column("manager_user_id")]
    public Guid? ManagerUserId { get; set; }

    /// <summary>dms_sale_supervisor (GSBH) who directly supervises this team.</summary>
    [Column("supervisor_user_id")]
    public Guid? SupervisorUserId { get; set; }
}
