using System;

namespace Hano.Core.BackgroundJobs.Args;

/// <summary>
/// Args cho job đồng bộ entity sang ODS.
/// Enqueue khi có Order/Visit/Report mới.
/// </summary>
[Serializable]
public class SyncToOdsJobArgs
{
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = null!; // Visit, Order, OsaReport, OosReport, PosmReport, FeedbackReport
    public string Action { get; set; } = "CREATE";  // CREATE | UPDATE
    public int RetryCount { get; set; }
}
