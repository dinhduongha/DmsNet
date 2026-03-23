using Bamboo.Shared.Common;
using Hano.Core.Application;
using Hano.Core.Domain.Shared.Enums;
using Hano.Core.Import.Dtos;
using Hano.Core.Import.Excel;
using Hano.Core.Import.Helpers;
using Hano.Core.Import.Parsers;
using Hano.Core.Import.Plan;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;

namespace Hano.Core.Import;

[Authorize(Roles = "admin")]
public class ImportAppService(
    ExcelReaderFactory readerFactory,
    IOrganizationUnitRepository ouRepository,
    IdentityUserManager userManager,
    ITenantManager tenantManager,
    ITenantRepository tenantRepository,
    IRepository<Distributor, Guid> distributorRepository,
    IRepository<Outlet, Guid> outletRepository,
    IRepository<Organization, Guid> dmsOrgRepository,
    IRepository<Team, Guid> dmsTeamRepository,
    IRepository<Sku, Guid> skuRepository,
    IUnitOfWorkManager uowManager,
    ImportPlanBuilder planBuilder
) : HanoCoreAppServiceBase, IImportAppService
{
    // OrganizationUnit.Code has a protected setter; use reflection to set it from outside.
    // Cached once — thread-safe for read after initialization.
    private static readonly System.Reflection.PropertyInfo OuCodeProperty =
        typeof(OrganizationUnit).GetProperty(nameof(OrganizationUnit.Code))!;

    private static void SetOuCode(OrganizationUnit ou, string code) =>
        OuCodeProperty.SetValue(ou, code);

    // ─────────────────────────────────────────────────────────────────────────
    // Step 1 — ImportAbpEntitiesAsync
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<ImportAbpEntitiesResult> ImportAbpEntitiesAsync(
        Stream fileStream, ImportMasterDataInput input)
    {
        var result = new ImportAbpEntitiesResult { IsDryRun = input.DryRun };

        // Phase A: Parse
        var reader = readerFactory.Create(input.ReaderType);
        var data = new MasterDataExcelParser(reader).Parse(fileStream);

        // Phase B: Build plan (in-memory, no DB)
        var plan = planBuilder.Build(data);

        // Phase C: Resolve conflicts against DB (FindByNameAsync per username)
        var (resolvedPlan, existingUsers) = await ResolveConflictsAsync(plan);
        plan = resolvedPlan;

        // DryRun: populate preview and return early
        if (input.DryRun)
        {
            PopulateAbpPreview(plan, result);
            return result;
        }

        // D1-1 to D1-4: Users + OUs (in one transaction)
        using (var uow = uowManager.Begin(requiresNew: true, isTransactional: true))
        {
            try
            {
                // D1-1: Create all users (Admin, ASM, GSBH, NVBH)
                var userIdByUsername = await CreateAllUsersAsync(plan, existingUsers, result);

                // D1-2: Create Region OUs (code = CreateCode(region.Index))
                var (ouByRegion, regionIndexByName) = await CreateRegionOusAsync(plan.Regions, result);

                // D1-3: Create Team OUs (code = CreateCode(regionIndex, team.Index))
                var teamOuByName = await CreateTeamOusAsync(plan.Teams, ouByRegion, regionIndexByName, result);

                // D1-4: Assign users to OUs
                await AssignUsersToOusAsync(plan, ouByRegion, teamOuByName, userIdByUsername);

                await uow.CompleteAsync();
            }
            catch (Exception ex)
            {
                await uow.RollbackAsync();
                Logger.LogError(ex, "ImportAbpEntitiesAsync (users+OUs) failed fatally — transaction rolled back");
                result.Errors.Add(new ImportErrorDto
                {
                    Message = $"Fatal (users/OUs): {ex.GetType().Name}: {ex.Message}",
                    IsFatal = true
                });
                return result;
            }
        }

        // D1-5: Create ABP Tenants — each in its own UoW because tenantManager.CreateAsync
        // fires distributed events and seeds per-tenant Identity resources internally,
        // which requires opening nested UoWs. Running inside an outer transaction causes conflicts.
        await CreateTenantsAsync(plan.Distributors, result);

        return result;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Step 2 — ImportDomainRecordsAsync
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<ImportDomainRecordsResult> ImportDomainRecordsAsync(
        Stream fileStream, ImportMasterDataInput input)
    {
        var result = new ImportDomainRecordsResult { IsDryRun = input.DryRun };

        // Phase A: Parse
        var reader = readerFactory.Create(input.ReaderType);
        var data = new MasterDataExcelParser(reader).Parse(fileStream);

        // Phase B: Build plan (same deterministic plan as Step 1)
        var plan = planBuilder.Build(data);

        // Phase C2: Load existing ABP entities from DB (independent of Step 1 result)
        var ouByRegion = await LoadRegionOusAsync(plan.Regions);
        var teamOuByName = await LoadTeamOusAsync(plan.Teams, ouByRegion);
        var userIdByUsername = await LoadUsersFromDbAsync(plan);
        var tenantIdByCode = await LoadTenantsFromDbAsync(plan.Distributors);

        // Build adminUsernameByRegion: region name → admin username (first admin per region)
        var adminUsernameByRegion = plan.Admins
            .GroupBy(a => a.Region, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Username, StringComparer.OrdinalIgnoreCase);

        // Build asmIdByRegionName: region name → ASM user ID
        var asmIdByRegionName = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        foreach (var s in plan.Regions)
        {
            if (s.Asm == null) continue;
            if (userIdByUsername.TryGetValue(s.Asm.Username, out var asmId))
                asmIdByRegionName[s.RegionName] = asmId;
        }

        if (input.DryRun)
        {
            PopulateDomainPreview(plan, result);
            return result;
        }

        using var uow = uowManager.Begin(requiresNew: true, isTransactional: true);
        try
        {
            // D2-1: Insert Region records (regions table)
            await InsertRegionRecordsAsync(plan.Regions, ouByRegion, userIdByUsername, adminUsernameByRegion, result);

            // D2-2: Insert Team records (teams table)
            await InsertTeamRecordsAsync(plan.Teams, teamOuByName, userIdByUsername, asmIdByRegionName, result);

            // D2-3: Insert Distributor records
            await InsertDistributorRecordsAsync(plan.Distributors, ouByRegion, tenantIdByCode, result);

            await uow.CompleteAsync();
        }
        catch (Exception ex)
        {
            await uow.RollbackAsync();
            Logger.LogError(ex, "ImportDomainRecordsAsync failed fatally — transaction rolled back");
            result.Errors.Add(new ImportErrorDto
            {
                Message = $"Fatal: {ex.GetType().Name}: {ex.Message}",
                IsFatal = true
            });
        }

        return result;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Phase C — Conflict resolution against DB
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<(ImportPlan Plan, Dictionary<string, IdentityUser> ExistingUsers)>
        ResolveConflictsAsync(ImportPlan plan)
    {
        // Collect all planned usernames
        var plannedUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var s in plan.Admins) plannedUsernames.Add(s.Username);
        foreach (var s in plan.Regions) { if (s.Asm != null) plannedUsernames.Add(s.Asm.Username); }
        foreach (var s in plan.Teams) plannedUsernames.Add(s.Username);
        foreach (var s in plan.SalesUsers) plannedUsernames.Add(s.Username);

        // FindByNameAsync per username — UserManager.Users.Where(Contains) doesn't translate
        var existingUsers = new Dictionary<string, IdentityUser>(StringComparer.OrdinalIgnoreCase);
        foreach (var username in plannedUsernames)
        {
            var dbUser = await userManager.FindByNameAsync(username);
            if (dbUser != null) existingUsers[username] = dbUser;
        }

        if (existingUsers.Count == 0) return (plan, existingUsers);

        // Re-resolve: rename only when DB user is a DIFFERENT person (genuine collision).
        // If DB user's Name matches planned DisplayName → same person re-imported → keep username.
        var allUsed = new HashSet<string>(plannedUsernames, StringComparer.OrdinalIgnoreCase);

        var fixedAdmins = new List<AdminSpec>();
        foreach (var s in plan.Admins)
        {
            if (!existingUsers.TryGetValue(s.Username, out var dbUser) ||
                string.Equals(dbUser.Name, s.DisplayName, StringComparison.OrdinalIgnoreCase))
            {
                fixedAdmins.Add(s);
                continue;
            }
            allUsed.Remove(s.Username);
            var (u, p) = UsernamePasswordGenerator.Generate(s.DisplayName, s.Index, allUsed);
            fixedAdmins.Add(s with { Username = u, Password = p });
        }

        var fixedRegions = new List<RegionSpec>();
        foreach (var s in plan.Regions)
        {
            if (s.Asm == null ||
                !existingUsers.TryGetValue(s.Asm.Username, out var dbUser) ||
                string.Equals(dbUser.Name, s.Asm.DisplayName, StringComparison.OrdinalIgnoreCase))
            {
                fixedRegions.Add(s);
                continue;
            }
            allUsed.Remove(s.Asm.Username);
            var (u, p) = UsernamePasswordGenerator.Generate(s.Asm.DisplayName, s.Asm.Index, allUsed);
            fixedRegions.Add(s with { Asm = s.Asm with { Username = u, Password = p } });
        }

        var fixedTeams = new List<TeamSpec>();
        foreach (var s in plan.Teams)
        {
            if (!existingUsers.TryGetValue(s.Username, out var dbUser) ||
                string.Equals(dbUser.Name, s.DisplayName, StringComparison.OrdinalIgnoreCase))
            {
                fixedTeams.Add(s);
                continue;
            }
            allUsed.Remove(s.Username);
            var (u, p) = UsernamePasswordGenerator.Generate(s.DisplayName, s.Index, allUsed);
            fixedTeams.Add(s with { Username = u, Password = p });
        }

        var fixedSalesUsers = new List<SalesUserSpec>();
        foreach (var s in plan.SalesUsers)
        {
            if (!existingUsers.TryGetValue(s.Username, out var dbUser) ||
                string.Equals(dbUser.Name, s.DisplayName, StringComparison.OrdinalIgnoreCase))
            {
                fixedSalesUsers.Add(s);
                continue;
            }
            allUsed.Remove(s.Username);
            var (u, p) = UsernamePasswordGenerator.Generate(s.DisplayName, s.Index, allUsed);
            fixedSalesUsers.Add(s with { Username = u, Password = p });
        }

        return (new ImportPlan(fixedAdmins, fixedRegions, fixedTeams, fixedSalesUsers, plan.Distributors),
                existingUsers);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DryRun preview helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static void PopulateAbpPreview(ImportPlan plan, ImportAbpEntitiesResult result)
    {
        foreach (var s in plan.Admins)
            result.CreatedUsers.Add(new ImportedUserRecord
            {
                DisplayName = s.DisplayName,
                Username = s.Username,
                Password = s.Password,
                Role = DmsRoles.Admin,
                Region = s.Region
            });

        foreach (var s in plan.Regions)
        {
            result.OusCreated++;
            if (s.Asm != null)
                result.CreatedUsers.Add(new ImportedUserRecord
                {
                    DisplayName = s.Asm.DisplayName,
                    Username = s.Asm.Username,
                    Password = s.Asm.Password,
                    Role = DmsRoles.SaleManager,
                    Region = s.RegionName
                });
        }

        foreach (var s in plan.Teams)
        {
            result.OusCreated++;
            result.CreatedUsers.Add(new ImportedUserRecord
            {
                DisplayName = s.DisplayName,
                Username = s.Username,
                Password = s.Password,
                Role = DmsRoles.SalesSupervisor,
                Region = s.Region
            });
        }

        foreach (var s in plan.SalesUsers)
            result.CreatedUsers.Add(new ImportedUserRecord
            {
                DisplayName = s.DisplayName,
                Username = s.Username,
                Password = s.Password,
                Role = DmsRoles.SalesUser,
                Region = s.Region
            });

        result.UsersCreated = result.CreatedUsers.Count;
        result.TenantsCreated = plan.Distributors.Select(d => d.CustomerCode)
            .Distinct(StringComparer.OrdinalIgnoreCase).Count();
    }

    private static void PopulateDomainPreview(ImportPlan plan, ImportDomainRecordsResult result)
    {
        result.RegionsCreated = plan.Regions.Count;
        result.TeamsCreated = plan.Teams.Count;
        result.DistributorsCreated = plan.Distributors.Count;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // D1 helpers — ABP entity creation
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<Dictionary<string, Guid>> CreateAllUsersAsync(
        ImportPlan plan,
        Dictionary<string, IdentityUser> existingUsers,
        ImportAbpEntitiesResult result)
    {
        var userIdByUsername = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        async Task<Guid?> CreateOrSkip(
            string displayName, string username, string password,
            string role, string region, string sheet, int rowIndex,
            string? ouName = null)
        {
            if (existingUsers.TryGetValue(username, out var existing))
            {
                Logger.LogInformation("[Import] User '{Username}' already exists — skipping (role={Role} ou={OuName})",
                    username, role, ouName ?? "none");
                userIdByUsername[username] = existing.Id;
                result.UsersSkipped++;
                return existing.Id;
            }

            Logger.LogInformation("[Import] Creating user '{Username}' ({DisplayName}) role={Role} region={Region} ou={OuName}",
                username, displayName, role, region, ouName ?? "none");

            var user = new IdentityUser(GuidGenerator.Create(), username,
                $"{username}@hanoimilk.vn", tenantId: null)
            { Name = displayName };

            var r = await userManager.CreateAsync(user, password);
            if (!r.Succeeded)
            {
                var errors = string.Join(", ", r.Errors.Select(e => e.Description));
                Logger.LogWarning("[Import] Failed to create user '{Username}' ({DisplayName}) role={Role}: {Errors}",
                    username, displayName, role, errors);
                result.UsersSkipped++;
                result.Errors.Add(new ImportErrorDto
                {
                    Sheet = sheet,
                    RowNumber = rowIndex,
                    Message = $"'{displayName}': {errors}"
                });
                return null;
            }

            await userManager.AddToRoleAsync(user, role);
            Logger.LogInformation("[Import] Created user '{Username}' ({DisplayName}) Id={UserId} role={Role} ou={OuName}",
                username, displayName, user.Id, role, ouName ?? "none");
            userIdByUsername[username] = user.Id;
            result.UsersCreated++;
            result.CreatedUsers.Add(new ImportedUserRecord
            {
                DisplayName = displayName,
                Username = username,
                Password = password,
                Role = role,
                Region = region
            });
            return user.Id;
        }

        foreach (var s in plan.Admins)
        {
            try { await CreateOrSkip(s.DisplayName, s.Username, s.Password, DmsRoles.Admin, s.Region, "DS ADMIN", s.Index, ouName: null); }
            catch (Exception ex) { result.Errors.Add(new ImportErrorDto { Sheet = "DS ADMIN", RowNumber = s.Index, Message = $"'{s.DisplayName}': {ex.Message}" }); }
        }

        foreach (var s in plan.Regions)
        {
            if (s.Asm == null) continue;
            try { await CreateOrSkip(s.Asm.DisplayName, s.Asm.Username, s.Asm.Password, DmsRoles.SaleManager, s.RegionName, "REGIONS", s.Index, ouName: s.RegionName); }
            catch (Exception ex) { result.Errors.Add(new ImportErrorDto { Sheet = "REGIONS", RowNumber = s.Index, Message = $"ASM '{s.Asm.DisplayName}': {ex.Message}" }); }
        }

        foreach (var s in plan.Teams)
        {
            try { await CreateOrSkip(s.DisplayName, s.Username, s.Password, DmsRoles.SalesSupervisor, s.Region, "DS GSBH", s.Index, ouName: s.TeamOuName); }
            catch (Exception ex) { result.Errors.Add(new ImportErrorDto { Sheet = "DS GSBH", RowNumber = s.Index, Message = $"'{s.DisplayName}': {ex.Message}" }); }
        }

        foreach (var s in plan.SalesUsers)
        {
            try { await CreateOrSkip(s.DisplayName, s.Username, s.Password, DmsRoles.SalesUser, s.Region, "DS NVBH", s.Index, ouName: s.Region); }
            catch (Exception ex) { result.Errors.Add(new ImportErrorDto { Sheet = "DS NVBH", RowNumber = s.Index, Message = $"'{s.DisplayName}': {ex.Message}" }); }
        }

        return userIdByUsername;
    }

    private async Task<(Dictionary<string, OrganizationUnit> OuByRegion, Dictionary<string, int> RegionIndexByName)>
        CreateRegionOusAsync(List<RegionSpec> specs, ImportAbpEntitiesResult result)
    {
        var ouByRegion = new Dictionary<string, OrganizationUnit>(StringComparer.OrdinalIgnoreCase);
        var regionIndexByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        var plannedNames = specs.Select(s => s.RegionName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existingByName = (await ouRepository.GetListAsync())
            .Where(ou => plannedNames.Contains(ou.DisplayName) && ou.ParentId == null)
            .ToDictionary(ou => ou.DisplayName, StringComparer.OrdinalIgnoreCase);

        foreach (var s in specs)
        {
            try
            {
                if (existingByName.TryGetValue(s.RegionName, out var existing))
                {
                    Logger.LogInformation("[Import] Region OU '{RegionName}' already exists — skipping", s.RegionName);
                    ouByRegion[s.RegionName] = existing;
                    regionIndexByName[s.RegionName] = s.Index;
                    result.OusSkipped++;
                    continue;
                }

                Logger.LogInformation("[Import] Creating Region OU '{RegionName}'", s.RegionName);
                var ou = new OrganizationUnit(GuidGenerator.Create(), s.RegionName,
                    parentId: null, tenantId: null);
                SetOuCode(ou, OrganizationUnit.CreateCode(s.Index));
                await ouRepository.InsertAsync(ou, autoSave: true);
                Logger.LogInformation("[Import] Created Region OU '{RegionName}' Id={OuId}", s.RegionName, ou.Id);

                ouByRegion[s.RegionName] = ou;
                regionIndexByName[s.RegionName] = s.Index;
                result.OusCreated++;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[Import] Failed to create Region OU '{RegionName}' (row {Row})", s.RegionName, s.Index);
                result.Errors.Add(new ImportErrorDto { Sheet = "REGIONS", RowNumber = s.Index, Message = $"OU '{s.RegionName}': {ex.Message}" });
            }
        }

        return (ouByRegion, regionIndexByName);
    }

    private async Task<Dictionary<string, OrganizationUnit>> CreateTeamOusAsync(
        List<TeamSpec> specs,
        Dictionary<string, OrganizationUnit> ouByRegion,
        Dictionary<string, int> regionIndexByName,
        ImportAbpEntitiesResult result)
    {
        var teamOuByName = new Dictionary<string, OrganizationUnit>(StringComparer.OrdinalIgnoreCase);

        // Cache children per region OU to avoid repeated DB calls
        var childrenByRegionOuId = new Dictionary<Guid, List<OrganizationUnit>>();

        foreach (var s in specs)
        {
            try
            {
                if (!ouByRegion.TryGetValue(s.Region, out var regionOu))
                {
                    result.Errors.Add(new ImportErrorDto { Sheet = "DS GSBH", RowNumber = s.Index, Message = $"Region OU not found for '{s.Region}' (GSBH: {s.DisplayName})" });
                    continue;
                }

                if (!childrenByRegionOuId.TryGetValue(regionOu.Id, out var children))
                {
                    children = await ouRepository.GetChildrenAsync(regionOu.Id);
                    childrenByRegionOuId[regionOu.Id] = children;
                }

                var existing = children.FirstOrDefault(
                    c => string.Equals(c.DisplayName, s.TeamOuName, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    Logger.LogInformation("[Import] Team OU '{TeamOuName}' already exists — skipping", s.TeamOuName);
                    teamOuByName[s.TeamOuName] = existing;
                    result.OusSkipped++;
                    continue;
                }

                Logger.LogInformation("[Import] Creating Team OU '{TeamOuName}' under Region '{Region}'", s.TeamOuName, s.Region);
                var teamOu = new OrganizationUnit(GuidGenerator.Create(), s.TeamOuName,
                    parentId: regionOu.Id, tenantId: null);

                regionIndexByName.TryGetValue(s.Region, out var regionIdx);
                SetOuCode(teamOu, OrganizationUnit.CreateCode(regionIdx, s.Index));
                await ouRepository.InsertAsync(teamOu, autoSave: true);
                Logger.LogInformation("[Import] Created Team OU '{TeamOuName}' Id={OuId}", s.TeamOuName, teamOu.Id);

                children.Add(teamOu);
                teamOuByName[s.TeamOuName] = teamOu;
                result.OusCreated++;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[Import] Failed to create Team OU '{TeamOuName}' (row {Row})", s.TeamOuName, s.Index);
                result.Errors.Add(new ImportErrorDto { Sheet = "DS GSBH", RowNumber = s.Index, Message = $"Team OU '{s.TeamOuName}': {ex.Message}" });
            }
        }

        return teamOuByName;
    }

    private async Task AssignUsersToOusAsync(
        ImportPlan plan,
        Dictionary<string, OrganizationUnit> ouByRegion,
        Dictionary<string, OrganizationUnit> teamOuByName,
        Dictionary<string, Guid> userIdByUsername)
    {
        // Admin → no OU (dms_admin role is not scoped to any OU)
        Logger.LogInformation("[Import] Skipping OU assignment for {Count} admin user(s) — dms_admin has no OU", plan.Admins.Count);

        // ASM → Region OU
        foreach (var s in plan.Regions)
        {
            if (s.Asm == null) continue;
            if (!userIdByUsername.TryGetValue(s.Asm.Username, out var userId)) continue;
            if (!ouByRegion.TryGetValue(s.RegionName, out var ou)) continue;
            try
            {
                Logger.LogInformation("[Import] Assigning ASM '{Username}' to Region OU '{OuName}' OuId={OuId}", s.Asm.Username, s.RegionName, ou.Id);
                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user != null)
                {
                    await userManager.SetOrganizationUnitsAsync(user, ou.Id);
                    Logger.LogInformation("[Import] Assigned ASM '{Username}' UserId={UserId} to Region OU '{OuName}' OuId={OuId}", s.Asm.Username, userId, s.RegionName, ou.Id);
                }
                else
                {
                    Logger.LogWarning("[Import] ASM user '{Username}' UserId={UserId} not found in DB — skipping OU assignment", s.Asm.Username, userId);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "[Import] Failed to assign ASM '{Username}' to OU '{OuName}'", s.Asm.Username, s.RegionName);
            }
        }

        // GSBH → Team OU
        foreach (var s in plan.Teams)
        {
            if (!userIdByUsername.TryGetValue(s.Username, out var userId)) continue;
            if (!teamOuByName.TryGetValue(s.TeamOuName, out var ou)) continue;
            try
            {
                Logger.LogInformation("[Import] Assigning GSBH '{Username}' to Team OU '{OuName}' OuId={OuId}", s.Username, s.TeamOuName, ou.Id);
                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user != null)
                {
                    await userManager.SetOrganizationUnitsAsync(user, ou.Id);
                    Logger.LogInformation("[Import] Assigned GSBH '{Username}' UserId={UserId} to Team OU '{OuName}' OuId={OuId}", s.Username, userId, s.TeamOuName, ou.Id);
                }
                else
                {
                    Logger.LogWarning("[Import] GSBH user '{Username}' UserId={UserId} not found in DB — skipping OU assignment", s.Username, userId);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "[Import] Failed to assign GSBH '{Username}' to OU '{OuName}'", s.Username, s.TeamOuName);
            }
        }

        // NVBH → Region OU (Team OU assignment requires parser extension for team column)
        foreach (var s in plan.SalesUsers)
        {
            if (!userIdByUsername.TryGetValue(s.Username, out var userId)) continue;
            if (!ouByRegion.TryGetValue(s.Region, out var regionOu)) continue;
            try
            {
                Logger.LogInformation("[Import] Assigning NVBH '{Username}' to Region OU '{OuName}' OuId={OuId}", s.Username, s.Region, regionOu.Id);
                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user != null)
                {
                    await userManager.SetOrganizationUnitsAsync(user, regionOu.Id);
                    Logger.LogInformation("[Import] Assigned NVBH '{Username}' UserId={UserId} to Region OU '{OuName}' OuId={OuId}", s.Username, userId, s.Region, regionOu.Id);
                }
                else
                {
                    Logger.LogWarning("[Import] NVBH user '{Username}' UserId={UserId} not found in DB — skipping OU assignment", s.Username, userId);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "[Import] Failed to assign NVBH '{Username}' to OU '{OuName}'", s.Username, s.Region);
            }
        }
    }

    private async Task CreateTenantsAsync(List<DistributorSpec> specs, ImportAbpEntitiesResult result)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var s in specs)
        {
            if (!seen.Add(s.CustomerCode)) continue;

            try
            {
                using var tenantUow = uowManager.Begin(requiresNew: true, isTransactional: true);
                using (DataFilter.Disable<IMultiTenant>())
                {
                    var tenantName = $"d-{s.CustomerCode}";
                    var existing = await tenantRepository.FindByNameAsync(tenantName);
                    if (existing != null)
                    {
                        Logger.LogInformation("[Import] Tenant '{TenantName}' already exists — skipping (NPP={CustomerCode} region={Region})", tenantName, s.CustomerCode, s.Region);
                        result.TenantsSkipped++;
                        continue;
                    }

                    Logger.LogInformation("[Import] Creating tenant '{TenantName}' for NPP '{CustomerCode}' (region={Region})", tenantName, s.CustomerCode, s.Region);
                    var tenant = await tenantManager.CreateAsync(tenantName);
                    await tenantRepository.InsertAsync(tenant, autoSave: true);
                    await tenantUow.CompleteAsync();
                    Logger.LogInformation("[Import] Created tenant '{TenantName}' Id={TenantId} for NPP '{CustomerCode}' region={Region}", tenantName, tenant.Id, s.CustomerCode, s.Region);
                    result.TenantsCreated++;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[Import] Failed to create tenant 'd-{CustomerCode}' region={Region} (row {Row})", s.CustomerCode, s.Region, s.Index);
                result.Errors.Add(new ImportErrorDto { Sheet = "DS NPP", RowNumber = s.Index, Message = $"Tenant 'd-{s.CustomerCode}': {ex.GetType().Name}: {ex.Message}" });
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // D2 helpers — Load ABP entities from DB (Step 2 independent queries)
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<Dictionary<string, OrganizationUnit>> LoadRegionOusAsync(List<RegionSpec> specs)
    {
        var names = specs.Select(s => s.RegionName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return (await ouRepository.GetListAsync())
            .Where(ou => ou.ParentId == null && names.Contains(ou.DisplayName))
            .ToDictionary(ou => ou.DisplayName, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<Dictionary<string, OrganizationUnit>> LoadTeamOusAsync(
        List<TeamSpec> specs,
        Dictionary<string, OrganizationUnit> ouByRegion)
    {
        var result = new Dictionary<string, OrganizationUnit>(StringComparer.OrdinalIgnoreCase);

        var regionOuIds = ouByRegion.Values.Select(ou => ou.Id).ToHashSet();
        var allChildren = regionOuIds.Count == 0
            ? []
            : (await ouRepository.GetListAsync()).Where(ou => ou.ParentId.HasValue && regionOuIds.Contains(ou.ParentId!.Value)).ToList();

        var childByName = allChildren.ToDictionary(ou => ou.DisplayName, StringComparer.OrdinalIgnoreCase);

        foreach (var s in specs)
        {
            if (childByName.TryGetValue(s.TeamOuName, out var ou))
                result[s.TeamOuName] = ou;
        }

        return result;
    }

    private async Task<Dictionary<string, Guid>> LoadUsersFromDbAsync(ImportPlan plan)
    {
        var usernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var s in plan.Admins) usernames.Add(s.Username);
        foreach (var s in plan.Regions) { if (s.Asm != null) usernames.Add(s.Asm.Username); }
        foreach (var s in plan.Teams) usernames.Add(s.Username);
        foreach (var s in plan.SalesUsers) usernames.Add(s.Username);

        var result = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        foreach (var username in usernames)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user != null) result[username] = user.Id;
        }
        return result;
    }

    private async Task<Dictionary<string, Guid>> LoadTenantsFromDbAsync(List<DistributorSpec> specs)
    {
        var result = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var s in specs)
        {
            if (!seen.Add(s.CustomerCode)) continue;
            using (DataFilter.Disable<IMultiTenant>())
            {
                var tenant = await tenantRepository.FindByNameAsync($"d-{s.CustomerCode}");
                if (tenant != null) result[s.CustomerCode] = tenant.Id;
            }
        }
        return result;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // D2 helpers — Domain record insertion
    // ─────────────────────────────────────────────────────────────────────────

    private async Task InsertRegionRecordsAsync(
        List<RegionSpec> specs,
        Dictionary<string, OrganizationUnit> ouByRegion,
        Dictionary<string, Guid> userIdByUsername,
        Dictionary<string, string> adminUsernameByRegion,
        ImportDomainRecordsResult result)
    {
        var existingOuIds = ouByRegion.Values.Select(ou => ou.Id).ToList();
        var existingOrgOuIds = existingOuIds.Count == 0
            ? new HashSet<Guid>()
            : (await dmsOrgRepository.GetListAsync(d => existingOuIds.Contains(d.OrganizationUnitId)))
              .Select(d => d.OrganizationUnitId).ToHashSet();

        foreach (var s in specs)
        {
            try
            {
                if (!ouByRegion.TryGetValue(s.RegionName, out var ou))
                {
                    result.Errors.Add(new ImportErrorDto { Sheet = "REGIONS", RowNumber = s.Index, Message = $"Region OU not found for '{s.RegionName}'" });
                    continue;
                }

                if (existingOrgOuIds.Contains(ou.Id))
                {
                    result.RegionsSkipped++;
                    continue;
                }

                Guid? adminId = null;
                Guid? asmId = null;

                if (adminUsernameByRegion.TryGetValue(s.RegionName, out var adminUsername) &&
                    userIdByUsername.TryGetValue(adminUsername, out var adminIdVal))
                    adminId = adminIdVal;

                if (s.Asm != null && userIdByUsername.TryGetValue(s.Asm.Username, out var asmIdVal))
                    asmId = asmIdVal;

                await dmsOrgRepository.InsertAsync(new Organization
                {
                    Id = GuidGenerator.Create(),
                    OrganizationUnitId = ou.Id,
                    AdminUserId = adminId,
                    SaleManagerUserId = asmId,
                }, autoSave: false);

                existingOrgOuIds.Add(ou.Id);
                result.RegionsCreated++;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportErrorDto { Sheet = "REGIONS", RowNumber = s.Index, Message = $"'{s.RegionName}': {ex.Message}" });
            }
        }
    }

    private async Task InsertTeamRecordsAsync(
        List<TeamSpec> specs,
        Dictionary<string, OrganizationUnit> teamOuByName,
        Dictionary<string, Guid> userIdByUsername,
        Dictionary<string, Guid> asmIdByRegionName,
        ImportDomainRecordsResult result)
    {
        foreach (var s in specs)
        {
            try
            {
                if (!teamOuByName.TryGetValue(s.TeamOuName, out var teamOu))
                {
                    result.Errors.Add(new ImportErrorDto { Sheet = "DS GSBH", RowNumber = s.Index, Message = $"Team OU not found for '{s.TeamOuName}'" });
                    continue;
                }

                var existingTeam = await dmsTeamRepository.FindAsync(t => t.OrganizationUnitId == teamOu.Id);
                if (existingTeam != null)
                {
                    result.TeamsSkipped++;
                    continue;
                }

                userIdByUsername.TryGetValue(s.Username, out var supervisorId);
                asmIdByRegionName.TryGetValue(s.Region, out var managerId);

                await dmsTeamRepository.InsertAsync(new Team
                {
                    Id = GuidGenerator.Create(),
                    OrganizationUnitId = teamOu.Id,
                    ManagerUserId = managerId == Guid.Empty ? null : managerId,
                    SupervisorUserId = supervisorId == Guid.Empty ? null : supervisorId,
                }, autoSave: false);

                result.TeamsCreated++;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportErrorDto { Sheet = "DS GSBH", RowNumber = s.Index, Message = $"'{s.DisplayName}': {ex.Message}" });
            }
        }
    }

    private async Task InsertDistributorRecordsAsync(
        List<DistributorSpec> specs,
        Dictionary<string, OrganizationUnit> ouByRegion,
        Dictionary<string, Guid> tenantIdByCode,
        ImportDomainRecordsResult result)
    {
        var plannedCodes = specs.Select(s => s.CustomerCode).ToList();

        // Disable IMultiTenant filter: distributors have TenantId set (non-null),
        // so the default host-level filter (WHERE tenant_id IS NULL) would hide them all.
        HashSet<string> existingCodes;
        using (DataFilter.Disable<IMultiTenant>())
        {
            existingCodes = plannedCodes.Count == 0
                ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                : (await distributorRepository.GetListAsync(
                    d => d.OdsDistributorId != null && plannedCodes.Contains(d.OdsDistributorId)))
                  .Select(d => d.OdsDistributorId!)
                  .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        foreach (var s in specs)
        {
            try
            {
                if (existingCodes.Contains(s.CustomerCode))
                {
                    result.DistributorsSkipped++;
                    continue;
                }

                if (!tenantIdByCode.TryGetValue(s.CustomerCode, out var tenantId))
                {
                    result.Errors.Add(new ImportErrorDto { Sheet = "DS NPP", RowNumber = s.Index, Message = $"Tenant not found for '{s.CustomerCode}' — run Step 1 first" });
                    continue;
                }

                ouByRegion.TryGetValue(s.Region, out var ou);
                using (DataFilter.Disable<IMultiTenant>())
                {
                    await distributorRepository.InsertAsync(new Distributor
                    {
                        Id = GuidGenerator.Create(),
                        TenantId = tenantId,
                        OrganizationUnitId = ou?.Id,
                        Name = s.Name,
                        Address = s.Address,
                        Region = s.Region,
                        OdsDistributorId = s.CustomerCode,
                        IsActive = true,
                    }, autoSave: false);
                }

                existingCodes.Add(s.CustomerCode);
                result.DistributorsCreated++;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportErrorDto { Sheet = "DS NPP", RowNumber = s.Index, Message = $"'{s.Name}' ({s.CustomerCode}): {ex.Message}" });
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ImportCustomersAsync
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<ImportCustomersResult> ImportCustomersAsync(
        Stream fileStream, ImportCustomersInput input)
    {
        var result = new ImportCustomersResult { IsDryRun = input.DryRun };

        var allOus = await ouRepository.GetListAsync();
        var ouMap = allOus.ToDictionary(ou => ou.DisplayName, ou => ou.Id, StringComparer.OrdinalIgnoreCase);

        var reader = readerFactory.Create(input.ReaderType);
        var parser = new CustomerExcelParser(reader);
        var customers = parser.Parse(fileStream).ToList();

        var existingOutlets = (await outletRepository.GetListAsync())
            .Select(o => (o.Name, o.Address))
            .ToHashSet();

        using var uow = uowManager.Begin(requiresNew: true, isTransactional: true);
        try
        {
            foreach (var customer in customers)
            {
                try
                {
                    var name = VietnameseSlugHelper.NormalizeDisplay(customer.ShopName);
                    var address = VietnameseSlugHelper.NormalizeDisplay(customer.Address);

                    if (existingOutlets.Contains((name, address)))
                    {
                        result.OutletsSkipped++;
                        continue;
                    }

                    var regionName = CustomerExcelParser.SheetToRegion.TryGetValue(
                        customer.RegionSheet, out var rn) ? rn : customer.RegionSheet;
                    ouMap.TryGetValue(regionName, out var ouId);

                    var outlet = new Outlet
                    {
                        Id = GuidGenerator.Create(),
                        TenantId = null,
                        OrganizationUnitId = ouId == Guid.Empty ? null : ouId,
                        Name = name,
                        Address = address,
                        Phone = customer.Phone,
                        OutletType = OutletType.Other,
                        Status = OutletStatus.Approved,
                        Latitude = 0,
                        Longitude = 0,
                        CreatedByUserId = CurrentUserId,
                        ApprovedByUserId = CurrentUserId,
                        ApprovedAt = DateTimeOffset.UtcNow,
                    };

                    await outletRepository.InsertAsync(outlet, autoSave: false);
                    existingOutlets.Add((name, address));
                    result.OutletsCreated++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Sheet = customer.RegionSheet,
                        RowNumber = customer.Seq,
                        Message = ex.Message,
                    });
                }
            }

            if (input.DryRun) await uow.RollbackAsync();
            else await uow.CompleteAsync();
        }
        catch (Exception ex)
        {
            await uow.RollbackAsync();
            result.Errors.Add(new ImportErrorDto { Message = $"Fatal: {ex.Message}", IsFatal = true });
        }

        return result;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ImportSkusAsync
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<ImportSkusResult> ImportSkusAsync(Stream fileStream, ImportSkusInput input)
    {
        var result = new ImportSkusResult { IsDryRun = input.DryRun };

        var reader = readerFactory.Create(input.ReaderType);
        var parser = new SkuExcelParser(reader);
        var rows = parser.Parse(fileStream).ToList();

        var plannedCodes = rows.Where(r => !string.IsNullOrWhiteSpace(r.Code))
            .Select(r => r.Code).ToList();
        var existingCodes = plannedCodes.Count == 0
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : (await skuRepository.GetListAsync(s => plannedCodes.Contains(s.Code)))
              .Select(s => s.Code)
              .ToHashSet(StringComparer.OrdinalIgnoreCase);

        using var uow = uowManager.Begin(requiresNew: true, isTransactional: true);
        try
        {
            foreach (var row in rows)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(row.Code)) continue;

                    if (existingCodes.Contains(row.Code))
                    {
                        result.SkusSkipped++;
                        continue;
                    }

                    var name = VietnameseSlugHelper.NormalizeDisplay(
                        $"{row.ProductName} {row.VolumeVal}{row.VolumeUnit}".Trim());

                    var sku = new Sku
                    {
                        Id = GuidGenerator.Create(),
                        TenantId = null,
                        Code = row.Code.Trim(),
                        Name = name.Length > 200 ? name[..200] : name,
                        Category = VietnameseSlugHelper.NormalizeDisplay(row.Category),
                        Unit = $"{row.VolumeVal}{row.VolumeUnit}".Trim(),
                        Brand = VietnameseSlugHelper.NormalizeDisplay(row.Brand),
                        IsActive = true,
                    };

                    await skuRepository.InsertAsync(sku, autoSave: false);
                    existingCodes.Add(row.Code);
                    result.SkusCreated++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Sheet = "Sheet1",
                        Message = $"SKU '{row.Code}': {ex.Message}"
                    });
                }
            }

            if (input.DryRun) await uow.RollbackAsync();
            else await uow.CompleteAsync();
        }
        catch (Exception ex)
        {
            await uow.RollbackAsync();
            result.Errors.Add(new ImportErrorDto { Message = $"Fatal: {ex.Message}", IsFatal = true });
        }

        return result;
    }
}
