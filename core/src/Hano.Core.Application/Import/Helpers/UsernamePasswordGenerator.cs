namespace Hano.Core.Import.Helpers;

/// <summary>
/// Generates unique usernames and matching passwords for bulk import.
///
/// Username rules (Vietnamese name convention — family name first, given name last):
///   - "Tran Thi Tho"   → base = "trantho"
///   - "Tạ Quyết Chiến" → base = "tachien"
///   - No region prefix, no dots — pure family+given concatenation.
///   - On conflict: append the person's own STT index → "trantho14"
///   - Very rare double conflict: "trantho14_2", "trantho14_3", …
///
/// Password: always "{FamilyCased}{GivenCased}$$${index}"
///   - "Tran Thi Tho" at STT 14 → password = "TranTho$$$14"
///   - Password does NOT change on username conflict — stays tied to the name + row index.
///
/// Priority guaranteed by call order (Admin → ASM → GSBH → NVBH): higher-priority roles
/// are processed first and claim base usernames; lower-priority clashes get the index suffix.
/// </summary>
public class UsernamePasswordGenerator
{
    /// <param name="displayName">Full display name (Vietnamese, e.g. "Tạ Quyết Chiến").</param>
    /// <param name="index">STT row index from Excel — used for conflict suffix and password.</param>
    /// <param name="usedUsernames">
    ///   In-memory set of already-allocated usernames. Mutated on success so subsequent
    ///   calls within the same import batch see previous allocations.
    /// </param>
    public static (string Username, string Password) Generate(
        string displayName,
        int index,
        HashSet<string> usedUsernames)
    {
        var (firstName, familyName) = VietnameseSlugHelper.ExtractNameParts(displayName);
        var givenSlug = VietnameseSlugHelper.ToSlug(firstName);
        var familySlug = VietnameseSlugHelper.ToSlug(familyName);

        // Username: family+given, no separator
        var baseUsername = string.IsNullOrEmpty(familySlug)
            ? givenSlug
            : $"{familySlug}{givenSlug}";

        // Password: CamelCase family+given + $$$ + index (constant regardless of conflict)
        var givenCased = CapitalizeFirst(givenSlug);
        var familyCased = CapitalizeFirst(familySlug);
        var password = string.IsNullOrEmpty(familyCased)
            ? $"{givenCased}$$${index}"
            : $"{familyCased}{givenCased}$$${index}";

        if (!usedUsernames.Contains(baseUsername, StringComparer.OrdinalIgnoreCase))
        {
            usedUsernames.Add(baseUsername);
            return (baseUsername, password);
        }

        // Conflict: append the person's own STT index
        var withIndex = $"{baseUsername}{index}";
        if (!usedUsernames.Contains(withIndex, StringComparer.OrdinalIgnoreCase))
        {
            usedUsernames.Add(withIndex);
            return (withIndex, password);
        }

        // Very rare — same name AND same index in same batch; append counter
        for (var n = 2; n < 200; n++)
        {
            var candidate = $"{baseUsername}{index}_{n}";
            if (!usedUsernames.Contains(candidate, StringComparer.OrdinalIgnoreCase))
            {
                usedUsernames.Add(candidate);
                return (candidate, password);
            }
        }

        // Absolute fallback (practically impossible)
        var fallback = $"{baseUsername}{index}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 10000}";
        usedUsernames.Add(fallback);
        return (fallback, password);
    }

    private static string CapitalizeFirst(string? s) =>
        string.IsNullOrEmpty(s) ? string.Empty : char.ToUpper(s[0]) + s[1..];
}
