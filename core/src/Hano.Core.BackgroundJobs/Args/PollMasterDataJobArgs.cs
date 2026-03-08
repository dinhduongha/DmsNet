using System;

namespace Hano.Core.BackgroundJobs.Args;

[Serializable]
public class PollMasterDataJobArgs
{
    public DateTime? LastPollTimestamp { get; set; }
}
