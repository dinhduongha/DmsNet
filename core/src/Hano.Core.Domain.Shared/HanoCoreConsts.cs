namespace Hano.Core.Domain.Shared;

public static class HanoCoreConsts
{
    public const double GpsOkMeters = 50;
    public const double GpsWarningMeters = 200;
    public const int MasterDataCacheTtlHours = 24;
    public const int OdsPollIntervalHours = 4;
    public const int SyncRetryMax = 3;
    public const int NotifRetentionDays = 30;
    public const int NotifMaxRecords = 200;
    public const int BreadcrumbIntervalSec = 300;
    public const int PresignedUrlExpiryMin = 15;
    public const int FeedbackMaxLen = 500;
}
