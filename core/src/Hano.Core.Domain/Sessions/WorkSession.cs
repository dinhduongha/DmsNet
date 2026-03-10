using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Hano.Core.Domain.Sessions;

[Table("work_sessions")]
public class WorkSession : FullAuditedAggregateRoot<Guid>
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

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("device_id")]
    public Guid DeviceId { get; set; }

    [Column("date")]
    public DateOnly Date { get; set; }

    [Column("status")]
    public SessionStatus Status { get; set; } = SessionStatus.Active;

    [Column("sod_timestamp")]
    public DateTimeOffset? SodTimestamp { get; set; }

    [Column("sod_latitude")]
    public decimal SodLatitude { get; set; }

    [Column("sod_longitude")]
    public decimal SodLongitude { get; set; }

    [Column("sod_selfie_photo_id")]
    public Guid? SodSelfiePhotoId { get; set; }

    [Column("eod_timestamp")]
    public DateTimeOffset? EodTimestamp { get; set; }

    [Column("eod_latitude")]
    public decimal? EodLatitude { get; set; }

    [Column("eod_longitude")]
    public decimal? EodLongitude { get; set; }

    [Column("total_distance_km")]
    public decimal? TotalDistanceKm { get; set; }

    [Column("total_visits")]
    public int TotalVisits { get; set; }

    [Column("total_orders")]
    public int TotalOrders { get; set; }

    [Column("total_revenue")]
    public decimal TotalRevenue { get; set; }

    public ICollection<GpsBreadcrumb> Breadcrumbs { get; set; } = new List<GpsBreadcrumb>();
}
