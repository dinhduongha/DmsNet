using Hano.Core.Import.Helpers;
using Hano.Core.Import.Parsers;
using Volo.Abp.Application.Services;

namespace Hano.Core.Import.Plan;

/// <summary>
/// Builds an <see cref="ImportPlan"/> entirely in memory from parsed Excel data.
/// No database access — all username generation and collision resolution is done
/// against an in-memory set that is seeded only from the current Excel file.
/// DB-level conflicts are resolved later in ImportAppService (Step 4).
/// </summary>
public class ImportPlanBuilder : ApplicationService
{
    // Region display name → short code used as username prefix for GSBH/NVBH.
    private static readonly IReadOnlyDictionary<string, string> RegionCodeMap =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Hà Nội",            "hn"   },
            { "Tây Bắc",           "tb"   },
            { "Đông Bắc 1",        "db1"  },
            { "Đông Bắc 2",        "db2"  },
            { "Duyên Hải",         "dh"   },
            { "Bắc Miền Trung",    "bmt"  },
            { "Tây Nguyên",        "tn"   },
            { "Miền Đông",         "md"   },
            { "Sữa Bộ Miền Bắc",   "sbmb" },
            { "Sữa Bộ Miền Trung", "sbmt" },
            { "Siêu Thị",          "st"   },
            { "Trường Học",        "th"   },
            { "Công Nghiệp",       "cn"   },
        };

    public ImportPlan Build(ParsedMasterData data)
    {
        // Single shared set — mutated by usernameGen so each call sees prior allocations.
        var usedUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var admins = BuildAdmins(data.Admins, usedUsernames);
        var regions = BuildRegions(data.Regions, usedUsernames);
        var teams = BuildTeams(data.Supervisors, regions, usedUsernames);
        var salesUsers = BuildSalesUsers(data.Salespeople, usedUsernames);
        var distributors = BuildDistributors(data.Distributors);

        return new ImportPlan(admins, regions, teams, salesUsers, distributors);
    }

    // ── Step 1: Admin users ───────────────────────────────────────────────────

    private static List<AdminSpec> BuildAdmins(List<PersonRow> rows, HashSet<string> used)
    {
        var result = new List<AdminSpec>();
        foreach (var row in rows)
        {
            var name = VietnameseSlugHelper.NormalizeDisplay(row.Name);
            var (username, password) = UsernamePasswordGenerator.Generate(name, row.Index, used);
            result.Add(new AdminSpec(row.Index, name, row.Region, username, password));
        }
        return result;
    }

    // ── Step 2: Region OUs + ASM users ───────────────────────────────────────

    private static List<RegionSpec> BuildRegions(List<RegionRow> rows, HashSet<string> used)
    {
        var result = new List<RegionSpec>();
        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.Region)) continue;

            var regionName = VietnameseSlugHelper.NormalizeDisplay(row.Region);
            RegionCodeMap.TryGetValue(regionName, out var regionCode);

            AsmSpec? asm = null;
            if (!string.IsNullOrWhiteSpace(row.AsmName))
            {
                var asmName = VietnameseSlugHelper.NormalizeDisplay(row.AsmName);
                var (username, password) = UsernamePasswordGenerator.Generate(asmName, row.Index, used);
                asm = new AsmSpec(row.Index, asmName, username, password);
            }

            result.Add(new RegionSpec(row.Index, regionName, regionCode, asm));
        }
        return result;
    }

    // ── Step 3: GSBH team OUs + supervisor users ──────────────────────────────

    private static List<TeamSpec> BuildTeams(
        List<PersonRow> rows,
        List<RegionSpec> regions,
        HashSet<string> used)
    {
        var result = new List<TeamSpec>();
        var seenTeamOuNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var regionByName = regions.ToDictionary(r => r.RegionName, StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var displayName = VietnameseSlugHelper.NormalizeDisplay(row.Name);
            var regionName = VietnameseSlugHelper.NormalizeDisplay(row.Region);

            // Team OU name: display name; append region suffix on collision.
            var teamOuName = seenTeamOuNames.Contains(displayName)
                ? $"{displayName} ({regionName})"
                : displayName;
            seenTeamOuNames.Add(teamOuName);

            var (username, password) = UsernamePasswordGenerator.Generate(displayName, row.Index, used);
            result.Add(new TeamSpec(row.Index, displayName, regionName, teamOuName, username, password));
        }
        return result;
    }

    // ── Step 4: NVBH sales users ─────────────────────────────────────────────

    private static List<SalesUserSpec> BuildSalesUsers(
        List<PersonRow> rows,
        HashSet<string> used)
    {
        var result = new List<SalesUserSpec>();

        foreach (var row in rows)
        {
            var displayName = VietnameseSlugHelper.NormalizeDisplay(row.Name);
            var regionName = VietnameseSlugHelper.NormalizeDisplay(row.Region);
            var (username, password) = UsernamePasswordGenerator.Generate(displayName, row.Index, used);
            result.Add(new SalesUserSpec(row.Index, displayName, regionName, username, password));
        }
        return result;
    }

    // ── Step 5: Distributors ──────────────────────────────────────────────────

    private static List<DistributorSpec> BuildDistributors(List<DistributorRow> rows)
    {
        var result = new List<DistributorSpec>();
        foreach (var row in rows)
        {
            result.Add(new DistributorSpec(
                row.Index,
                VietnameseSlugHelper.NormalizeDisplay(row.Region),
                row.CustomerCode,
                VietnameseSlugHelper.NormalizeDisplay(row.Name),
                row.Province,
                VietnameseSlugHelper.NormalizeDisplay(row.Address)));
        }
        return result;
    }
}
