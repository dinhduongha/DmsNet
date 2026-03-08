namespace Hano.Core.Domain.Shared.Enums;

public enum SyncQueueStatus
{
    Pending = 1,
    Processing = 2,
    SyncedToOds = 3,
    Failed = 4,
}
