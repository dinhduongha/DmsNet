using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Hano.Core.Domain.Entities;

[Table("photos")]
public class Photo : CreationAuditedEntity<Guid>, IMultiTenant
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

    [Column("s3_key")]
    public string S3Key { get; set; } = null!;

    [Column("context")]
    public PhotoContext Context { get; set; }

    [Column("content_type")]
    public string ContentType { get; set; } = null!;

    [Column("file_size_bytes")]
    public long? FileSizeBytes { get; set; }

    [Column("latitude")]
    public decimal? Latitude { get; set; }

    [Column("longitude")]
    public decimal? Longitude { get; set; }

    [Column("captured_at")]
    public DateTimeOffset? CapturedAt { get; set; }

    [Column("visit_id")]
    public Guid? VisitId { get; set; }

    [Column("outlet_id")]
    public Guid? OutletId { get; set; }

    [Column("uploaded_by")]
    public Guid UploadedBy { get; set; }

    [Column("is_uploaded")]
    public bool IsUploaded { get; set; }

    [Column("sync_status")]
    public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
}
