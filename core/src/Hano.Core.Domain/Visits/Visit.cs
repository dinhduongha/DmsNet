using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Hano.Core.Domain.Entities;

[Table("visits")]
public class Visit : FullAuditedAggregateRoot<Guid>, IMultiTenant
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

    [Column("outlet_id")]
    public Guid OutletId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("route_id")]
    public Guid? RouteId { get; set; }

    [Column("status")]
    public VisitStatus Status { get; set; } = VisitStatus.Planned;

    [Column("checkin_at")]
    public DateTimeOffset? CheckinAt { get; set; }

    [Column("checkin_latitude")]
    public decimal? CheckinLatitude { get; set; }

    [Column("checkin_longitude")]
    public decimal? CheckinLongitude { get; set; }

    [Column("gps_distance_m")]
    public decimal? GpsDistanceM { get; set; }

    [Column("gps_flag")]
    public GpsFlag? GpsFlag { get; set; }

    [Column("checkout_at")]
    public DateTimeOffset? CheckoutAt { get; set; }

    [Column("checkout_latitude")]
    public decimal? CheckoutLatitude { get; set; }

    [Column("checkout_longitude")]
    public decimal? CheckoutLongitude { get; set; }

    [Column("duration_minutes")]
    public int? DurationMinutes { get; set; }

    [Column("activities_count")]
    public int ActivitiesCount { get; set; }

    [Column("is_off_route")]
    public bool IsOffRoute { get; set; }

    [Column("skip_reason")]
    public string? SkipReason { get; set; }

    [Column("anomaly_flags")]
    public string? AnomalyFlags { get; set; }

    [Column("sync_status")]
    public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;

    [Column("client_created_at")]
    public DateTimeOffset? ClientCreatedAt { get; set; }
}
