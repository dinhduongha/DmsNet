using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Hano.Core.Import.Helpers;

public static partial class VietnameseSlugHelper
{
    // "đ/Đ" does not decompose via FormD, handle separately
    private static readonly (string From, string To)[] SpecialReplacements =
    [
        ("đ", "d"), ("Đ", "d"),
    ];

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultiSpaceRegex();

    /// <summary>
    /// Normalizes display text: NFC form, trimmed, collapsed internal spaces.
    /// Use for storing DisplayName, Outlet.Name, Distributor.Name, Sku.Name.
    /// </summary>
    public static string NormalizeDisplay(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var collapsed = MultiSpaceRegex().Replace(input.Trim(), " ");
        return collapsed.Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Splits a Vietnamese full name into (FirstName, FamilyName).
    /// Vietnamese convention: family name first, given name last.
    /// "Tạ Quyết Chiến" → FirstName="Chiến", FamilyName="Tạ"
    /// "Nguyễn Văn A"   → FirstName="A",     FamilyName="Nguyễn"
    /// Single word       → FirstName=word,    FamilyName=""
    /// </summary>
    public static (string FirstName, string FamilyName) ExtractNameParts(string fullName)
    {
        var parts = NormalizeDisplay(fullName)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return parts.Length switch
        {
            0 => (string.Empty, string.Empty),
            1 => (parts[0], string.Empty),
            _ => (parts[^1], parts[0]),
        };
    }

    /// <summary>
    /// Converts a Vietnamese full name to a URL-safe slug.
    /// Example: "Nguyễn Văn A" → "nguyen-van-a"
    /// </summary>
    public static string ToSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var sb = new StringBuilder(input.Trim());

        foreach (var (from, to) in SpecialReplacements)
            sb.Replace(from, to);

        // Decompose Unicode (e.g., "ê" → "e" + combining circumflex) then strip marks
        var decomposed = sb.ToString().Normalize(NormalizationForm.FormD);
        var stripped = new StringBuilder(decomposed.Length);
        foreach (char c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                stripped.Append(c);
        }

        var slug = stripped.ToString().ToLowerInvariant();

        // Replace any non-alphanumeric sequence with a single hyphen
        slug = NonAlphanumericRegex().Replace(slug, "-").Trim('-');

        return slug;
    }
}
