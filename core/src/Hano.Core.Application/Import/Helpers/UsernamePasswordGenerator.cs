namespace Hano.Core.Import.Helpers;

/// <summary>
/// Generates unique usernames and matching passwords for bulk import.
///
/// Username rules (Vietnamese name convention — family name first, given name last):
///   - "Tạ Quyết Chiến" → firstName="Chiến", familyName="Tạ"
///   - NVBH / GSBH with region : "{regionCode}.{slug(firstName)}.{slug(familyName)}"  e.g. "hn.chien.ta"
///   - ASM  / Admin (no region) : "{slug(firstName)}.{slug(familyName)}"               e.g. "chien.ta"
///   - On conflict              : append ".{XXXX}" 4-digit suffix                       e.g. "hn.chien.ta.5678"
///
/// Password: "{username}$$${XXXX}" where XXXX = same digits as suffix on conflict,
///           or fresh random digits when no conflict. Always has digits (Identity compliance).
///
/// Priority guaranteed by call order (Admin → ASM → GSBH → NVBH): higher-priority roles
/// are processed first and claim base usernames; lower-priority clashes get a numeric suffix.
/// </summary>
public class UsernamePasswordGenerator
{
    private readonly Random _rng = new();

    /// <param name="displayName">Full display name (Vietnamese, e.g. "Tạ Quyết Chiến").</param>
    /// <param name="regionCode">Region prefix for NVBH/GSBH, or null for ASM/Admin.</param>
    /// <param name="usedUsernames">
    ///   In-memory set of already-allocated usernames. Mutated on success so subsequent
    ///   calls within the same import batch see previous allocations.
    /// </param>
    public (string Username, string Password) Generate(
        string displayName,
        string? regionCode,
        HashSet<string> usedUsernames)
    {
        var (firstName, familyName) = VietnameseSlugHelper.ExtractNameParts(displayName);
        var firstSlug = VietnameseSlugHelper.ToSlug(firstName);
        var familySlug = VietnameseSlugHelper.ToSlug(familyName);

        string baseUsername;
        if (!string.IsNullOrEmpty(familySlug))
            baseUsername = string.IsNullOrEmpty(regionCode)
                ? $"{firstSlug}.{familySlug}"
                : $"{regionCode.ToLowerInvariant()}.{firstSlug}.{familySlug}";
        else
            // single-word name fallback
            baseUsername = string.IsNullOrEmpty(regionCode)
                ? firstSlug
                : $"{regionCode.ToLowerInvariant()}.{firstSlug}";

        if (!usedUsernames.Contains(baseUsername, StringComparer.OrdinalIgnoreCase))
        {
            usedUsernames.Add(baseUsername);
            var pwdDigits = _rng.Next(1000, 9999).ToString();
            return (baseUsername, $"{baseUsername}$$${pwdDigits}");
        }

        // Resolve conflict — same digits in both username suffix and password
        for (int attempt = 0; attempt < 30; attempt++)
        {
            var digits = _rng.Next(1000, 9999).ToString();
            var candidate = $"{baseUsername}.{digits}";
            if (!usedUsernames.Contains(candidate, StringComparer.OrdinalIgnoreCase))
            {
                usedUsernames.Add(candidate);
                return (candidate, $"{candidate}$$${digits}");
            }
        }

        // Fallback: timestamp-based suffix (extremely unlikely)
        var ts = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 10000).ToString("D4");
        var fallback = $"{baseUsername}.{ts}";
        usedUsernames.Add(fallback);
        return (fallback, $"{fallback}$$${ts}");
    }
}
