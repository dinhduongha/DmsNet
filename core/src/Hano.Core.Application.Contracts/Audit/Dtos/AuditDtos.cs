using System;
using System.Collections.Generic;
using Hano.Core.Domain.Shared.Enums;
namespace Hano.Core.Application.Contracts.Audit.Dtos;

public class OsaReportInputDto
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public List<OsaItemDto> Items { get; set; } = new();
    public List<Guid> PhotoIds { get; set; } = new();
    public string? Notes { get; set; }
    public DateTime Timestamp { get; set; }
}
public class OsaItemDto
{
    public Guid SkuId { get; set; }
    public bool IsPresent { get; set; }
    public int? FacingCount { get; set; }
    public ShelfPosition? ShelfPosition { get; set; }
    public SkuCondition? Condition { get; set; }
}
public class OosReportInputDto
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public List<OosItemDto> Items { get; set; } = new();
    public string? Notes { get; set; }
    public DateTime Timestamp { get; set; }
}
public class OosItemDto { public Guid SkuId { get; set; } public string? Reason { get; set; } public string? DaysMissing { get; set; } }
public class PosmReportInputDto
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public List<PosmItemDto> Items { get; set; } = new();
    public List<Guid> PhotoIds { get; set; } = new();
    public string? Notes { get; set; }
    public DateTime Timestamp { get; set; }
}
public class PosmItemDto { public Guid PosmItemId { get; set; } public bool IsPresent { get; set; } public PosmCondition? Condition { get; set; } public bool CorrectPosition { get; set; } public bool NeedsReplacement { get; set; } }
