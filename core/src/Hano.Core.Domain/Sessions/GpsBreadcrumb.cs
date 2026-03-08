using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Hano.Core.Domain.Sessions;

[Table("gps_breadcrumbs")]
public class GpsBreadcrumb : Entity<Guid>
{
    [Key]
    [Column("id")]
    public Guid Id { get => base.Id; set => base.Id = value; }

    [Column("tenant_id")]
    public Guid? TenantId { get; set; }

    [Column("organization_id")]
    public Guid? OrganizationUnitId { get; set; }

    [Column("team_id")]
    public Guid? TeamId { get; set; }

    [Column("session_id")]
    public Guid SessionId { get; set; }

    [Column("latitude")]
    public decimal Latitude { get; set; }

    [Column("longitude")]
    public decimal Longitude { get; set; }

    [Column("accuracy")]
    public float Accuracy { get; set; }

    [Column("timestamp")]
    public DateTime Timestamp { get; set; }

    [Column("battery_level")]
    public float? BatteryLevel { get; set; }
}
