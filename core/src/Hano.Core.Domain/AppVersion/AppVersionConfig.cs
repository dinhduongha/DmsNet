using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Hano.Core.Domain.Entities;

[Table("app_version_configs")]
public class AppVersionConfig : AuditedEntity<Guid>
{
    [Key]
    [Column("id")]
    public Guid Id { get => base.Id; set => base.Id = value; }

    [Column("platform")]
    public string Platform { get; set; } = null!;

    [Column("latest_version")]
    public string LatestVersion { get; set; } = null!;

    [Column("update_type")]
    public UpdateType UpdateType { get; set; }

    [Column("download_url")]
    public string? DownloadUrl { get; set; }

    [Column("release_notes")]
    public string? ReleaseNotes { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;
}
