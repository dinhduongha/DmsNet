using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Hano.Core.Application.Contracts.Sync.Dtos;

public class SyncUploadDto
{
    public List<SyncItemDto> Items { get; set; } = new();
}
public class SyncItemDto
{
    public Guid Uuid { get; set; }

    [MaxLength(10240)]
    public string EntityType { get; set; } = null!;
    [MaxLength(10240)]
    public string Action { get; set; } = null!;
    [MaxLength(10240)]
    public string Data { get; set; } = null!;
    public DateTimeOffset ClientTimestamp { get; set; }
}
public class SyncUploadResultDto
{
    public int Processed { get; set; }
    public List<SyncFailedDto> Failed { get; set; } = new();
    public List<SyncConflictDto> Conflicts { get; set; } = new();
}
public class SyncFailedDto
{
    public Guid Uuid { get; set; }
    public string Reason { get; set; } = null!;
}
public class SyncConflictDto
{
    public Guid Uuid { get; set; }
    public string ConflictType { get; set; } = null!;
}
public class ResolveConflictDto
{
    public Guid ConflictId { get; set; }
    public string Resolution { get; set; } = null!;
}
