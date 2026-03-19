using Bamboo.Shared.Common;
using Hano.Core.Application;
using Hano.Core.Domain.Shared.Enums;
using Hano.Core.Import.Dtos;
using Hano.Core.Import.Excel;
using Hano.Core.Import.Helpers;
using Hano.Core.Import.Parsers;
using Hano.Core.Import.Plan;
using Microsoft.AspNetCore.Authorization;
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
    OrganizationUnitManager ouManager,
    IdentityUserManager userManager,
    ICurrentTenant currentTenant,
    ITenantManager tenantManager,
    ITenantRepository tenantRepository,
    IRepository<Distributor, Guid> distributorRepository,
    IRepository<Outlet, Guid> outletRepository,
    IRepository<Organization, Guid> dmsOrgRepository,
    IRepository<Team, Guid> dmsTeamRepository,
    IRepository<Sku, Guid> skuRepository,
    IUnitOfWorkManager uowManager,
    ImportPlanBuilder planBuilder,
    UsernamePasswordGenerator usernameGen
) : HanoCoreAppServiceBase, IImportAppService
{
    // ─────────────────────────────────────────────────────────────────────────
    // ImportMasterDataAsync
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<ImportMasterDataResult> ImportMasterDataAsync(
        Stream fileStream, ImportMasterDataInput input)
    {
        var result = new ImportMasterDataResult { IsDryRun = input.DryRun };

        // ── Phase A: Parse ────────────────────────────────────────────────────
        var reader = readerFactory.Create(input.ReaderType);
        var parser = new MasterDataExcelParser(reader);
        var data = parser.Parse(fileStream);

        // ── Phase B: Build plan (no DB) ───────────────────────────────────────
        var plan = planBuilder.Build(data);

        // ── Phase C: Targeted DB conflict check ───────────────────────────────
        var resolveResult = await ResolveConflictsAsync(plan);
        plan = resolveResult.Plan;
        var existingUsers = resolveResult.ExistingUsers;

        // ── Populate result from plan (for DryRun and real run) ───────────────
        PopulateResultFromPlan(plan, result);

        if (input.DryRun) return result;

        // ── Phase D: Persist (single transaction) ─────────────────────────────
        using var uow = uowManager.Begin(requiresNew: true, isTransactional: true);
        try
        {
            // 1. Admin users
            var adminIdByRegion = await PersistAdminsAsync(plan.Admins, existingUsers, result);

            // 2. Region OUs + ASM users + DmsOrganization
            var (ouByRegion, asmIdByRegion) = await PersistRegionsAsync(
                plan.Regions, existingUsers, adminIdByRegion, result);

            // 3. Team OUs + GSBH users + DmsTeam
            await PersistTeamsAsync(plan.Teams, ouByRegion, asmIdByRegion, existingUsers, result);

            // 4. NVBH users
            await PersistSalesUsersAsync(plan.SalesUsers, ouByRegion, existingUsers, result);

            // 5. Tenants + Distributors
            await PersistDistributorsAsync(plan.Distributors, ouByRegion, result);

            await uow.CompleteAsync();
        }
        catch (Exception ex)
        {
            await uow.RollbackAsync();
            result.Errors.Add(new ImportErrorDto { Message = $"Fatal: {ex.Message}", IsFatal = true });
        }

        return result;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Phase C — Conflict resolution against DB
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<(ImportPlan Plan, Dictionary<string, IdentityUser> ExistingUsers)>
        ResolveConflictsAsync(ImportPlan plan)
    {
        // 1. Collect all planned usernames
        var plannedUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var s in plan.Admins) plannedUsernames.Add(s.Username);
        foreach (var s in plan.Regions) { if (s.Asm != null) plannedUsernames.Add(s.Asm.Username); }
        foreach (var s in plan.Teams) plannedUsernames.Add(s.Username);
        foreach (var s in plan.SalesUsers) plannedUsernames.Add(s.Username);

        // 2. Query each planned username individually — UserManager.Users.Where() doesn't translate
        //    with a local HashSet. FindByNameAsync is the safe ABP-compatible approach.
        var existingUsers = new Dictionary<string, IdentityUser>(StringComparer.OrdinalIgnoreCase);
        foreach (var username in plannedUsernames)
        {
            var dbUser = await userManager.FindByNameAsync(username);
            if (dbUser != null) existingUsers[username] = dbUser;
        }

        if (existingUsers.Count == 0) return (plan, existingUsers);

        // 3. Re-resolve: rename only when the existing DB user is a DIFFERENT person
        //    (genuine username collision). If the DB user's Name matches the planned
        //    DisplayName, it's the same person being re-imported → keep the username;
        //    the persist phase will detect it in existingUsers and skip creation.
        var allUsed = new HashSet<string>(plannedUsernames, StringComparer.OrdinalIgnoreCase);

        var fixedAdmins = new List<AdminSpec>();
        foreach (var s in plan.Admins)
        {
            if (!existingUsers.TryGetValue(s.Username, out var dbUser) ||
                string.Equals(dbUser.Name, s.DisplayName, StringComparison.OrdinalIgnoreCase))
            {
                fixedAdmins.Add(s); // new user OR same person re-imported
                continue;
            }
            // Different person — rename the incoming user
            allUsed.Remove(s.Username);
            var (u, p) = usernameGen.Generate(s.DisplayName, null, allUsed);
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
            var (u, p) = usernameGen.Generate(s.Asm.DisplayName, null, allUsed);
            fixedRegions.Add(s with { Asm = s.Asm with { Username = u, Password = p } });
        }

        var regionCodeByName = fixedRegions
            .Where(r => r.RegionCode != null)
            .ToDictionary(r => r.RegionName, r => r.RegionCode!, StringComparer.OrdinalIgnoreCase);

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
            regionCodeByName.TryGetValue(s.Region, out var rc);
            var (u, p) = usernameGen.Generate(s.DisplayName, rc, allUsed);
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
            regionCodeByName.TryGetValue(s.Region, out var rc);
            var (u, p) = usernameGen.Generate(s.DisplayName, rc, allUsed);
            fixedSalesUsers.Add(s with { Username = u, Password = p });
        }

        return (new ImportPlan(fixedAdmins, fixedRegions, fixedTeams, fixedSalesUsers, plan.Distributors),
                existingUsers);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Populate result counters + CreatedUsers from plan (DryRun preview)
    // ─────────────────────────────────────────────────────────────────────────

    private static void PopulateResultFromPlan(ImportPlan plan, ImportMasterDataResult result)
    {
        foreach (var s in plan.Admins)
            result.CreatedUsers.Add(new ImportedUserRecord
            {
                DisplayName = s.DisplayName,
                Username = s.Username,
                Password = s.Password,
                Role = DmsRoles.Admin,
                Region = s.Region,
            });

        foreach (var s in plan.Regions)
        {
            result.RegionsCreated++;
            if (s.Asm != null)
                result.CreatedUsers.Add(new ImportedUserRecord
                {
                    DisplayName = s.Asm.DisplayName,
                    Username = s.Asm.Username,
                    Password = s.Asm.Password,
                    Role = DmsRoles.SaleManager,
                    Region = s.RegionName,
                });
        }

        foreach (var s in plan.Teams)
            result.CreatedUsers.Add(new ImportedUserRecord
            {
                DisplayName = s.DisplayName,
                Username = s.Username,
                Password = s.Password,
                Role = DmsRoles.SalesSupervisor,
                Region = s.Region,
            });

        foreach (var s in plan.SalesUsers)
            result.CreatedUsers.Add(new ImportedUserRecord
            {
                DisplayName = s.DisplayName,
                Username = s.Username,
                Password = s.Password,
                Role = DmsRoles.SalesUser,
                Region = s.Region,
            });

        result.UsersCreated = result.CreatedUsers.Count;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Phase D — Persist helpers
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<Dictionary<string, Guid>> PersistAdminsAsync(
        List<AdminSpec> specs,
        Dictionary<string, IdentityUser> existingUsers,
        ImportMasterDataResult result)
    {
        var adminIdByRegion = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        foreach (var s in specs)
        {
            try
            {
                Guid userId;
                if (existingUsers.TryGetValue(s.Username, out var existing))
                {
                    // Same person already in DB — reuse their ID
                    userId = existing.Id;
                    result.UsersSkipped++;
                }
                else
                {
                    var user = new IdentityUser(GuidGenerator.Create(), s.Username,
                        $"{s.Username}@hanoimilk.vn", tenantId: null)
                    { Name = s.DisplayName };

                    var r = await userManager.CreateAsync(user, s.Password);
                    if (!r.Succeeded)
                    {
                        result.UsersSkipped++;
                        result.Errors.Add(new ImportErrorDto
                        {
                            Sheet = "DS ADMIN",
                            RowNumber = s.Index,
                            Message = $"'{s.DisplayName}': {string.Join(", ", r.Errors.Select(e => e.Description))}"
                        });
                        continue;
                    }

                    await userManager.AddToRoleAsync(user, DmsRoles.Admin);
                    userId = user.Id;
                    result.UsersCreated++;
                }

                if (!string.IsNullOrWhiteSpace(s.Region))
                    adminIdByRegion[s.Region] = userId;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportErrorDto { Sheet = "DS ADMIN", RowNumber = s.Index, Message = $"'{s.DisplayName}': {ex.Message}" });
            }
        }

        return adminIdByRegion;
    }

    private async Task<(Dictionary<string, OrganizationUnit> OuByRegion, Dictionary<string, Guid> AsmIdByRegion)>
        PersistRegionsAsync(
            List<RegionSpec> specs,
            Dictionary<string, IdentityUser> existingUsers,
            Dictionary<string, Guid> adminIdByRegion,
            ImportMasterDataResult result)
    {
        var ouByRegion = new Dictionary<string, OrganizationUnit>(StringComparer.OrdinalIgnoreCase);
        var asmIdByRegion = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        // IOrganizationUnitRepository has no predicate overload — filter in memory.
        var plannedNames = specs.Select(s => s.RegionName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existingOuByName = (await ouRepository.GetListAsync())
            .Where(ou => plannedNames.Contains(ou.DisplayName))
            .ToDictionary(ou => ou.DisplayName, StringComparer.OrdinalIgnoreCase);

        // Load existing DmsOrg for reuse check
        var existingOuIds = existingOuByName.Values.Select(ou => ou.Id).ToList();
        var existingDmsOrgIds = existingOuIds.Count == 0
            ? new HashSet<Guid>()
            : (await dmsOrgRepository.GetListAsync(d => existingOuIds.Contains(d.OrganizationUnitId)))
              .Select(d => d.OrganizationUnitId).ToHashSet();

        foreach (var s in specs)
        {
            try
            {
                // Get or create region OU
                OrganizationUnit ou;
                if (existingOuByName.TryGetValue(s.RegionName, out var existing))
                {
                    ou = existing;
                    result.RegionsSkipped++;
                }
                else
                {
                    ou = new OrganizationUnit(GuidGenerator.Create(), s.RegionName, parentId: null, tenantId: null);
                    await ouManager.CreateAsync(ou);
                    result.RegionsCreated++;
                }
                ouByRegion[s.RegionName] = ou;

                // Get or create ASM user
                Guid? asmUserId = null;
                if (s.Asm != null)
                {
                    if (existingUsers.TryGetValue(s.Asm.Username, out var existingAsm))
                    {
                        // Same person already in DB — reuse ID
                        asmIdByRegion[s.RegionName] = existingAsm.Id;
                        asmUserId = existingAsm.Id;
                        result.UsersSkipped++;
                    }
                    else
                    {
                        var asmUser = new IdentityUser(GuidGenerator.Create(), s.Asm.Username,
                            $"{s.Asm.Username}@hanoimilk.vn", tenantId: null)
                        { Name = s.Asm.DisplayName };

                        var r = await userManager.CreateAsync(asmUser, s.Asm.Password);
                        if (r.Succeeded)
                        {
                            await userManager.AddToRoleAsync(asmUser, DmsRoles.SaleManager);
                            await userManager.SetOrganizationUnitsAsync(asmUser, ou.Id);
                            asmIdByRegion[s.RegionName] = asmUser.Id;
                            asmUserId = asmUser.Id;
                            result.UsersCreated++;
                        }
                        else
                        {
                            result.UsersSkipped++;
                            result.Errors.Add(new ImportErrorDto
                            {
                                Sheet = "VÙNG & ASM",
                                RowNumber = s.Index,
                                Message = $"ASM '{s.Asm.DisplayName}': {string.Join(", ", r.Errors.Select(e => e.Description))}"
                            });
                        }
                    }
                }

                // Create DmsOrganization if not already present
                if (!existingDmsOrgIds.Contains(ou.Id))
                {
                    adminIdByRegion.TryGetValue(s.RegionName, out var adminId);
                    await dmsOrgRepository.InsertAsync(new Organization
                    {
                        Id = GuidGenerator.Create(),
                        OrganizationUnitId = ou.Id,
                        AdminUserId = adminId == Guid.Empty ? null : adminId,
                        SaleManagerUserId = asmUserId,
                    }, autoSave: false);
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportErrorDto { Sheet = "VÙNG & ASM", RowNumber = s.Index, Message = $"'{s.RegionName}': {ex.Message}" });
            }
        }

        return (ouByRegion, asmIdByRegion);
    }

    private async Task PersistTeamsAsync(
        List<TeamSpec> specs,
        Dictionary<string, OrganizationUnit> ouByRegion,
        Dictionary<string, Guid> asmIdByRegion,
        Dictionary<string, IdentityUser> existingUsers,
        ImportMasterDataResult result)
    {
        // Cache of children already loaded per region OU to avoid repeated DB calls.
        var childrenByRegionOuId = new Dictionary<Guid, List<OrganizationUnit>>();

        foreach (var s in specs)
        {
            try
            {
                if (!ouByRegion.TryGetValue(s.Region, out var regionOu))
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Sheet = "DS GSBH",
                        RowNumber = s.Index,
                        Message = $"Region OU not found for '{s.Region}' (GSBH: {s.DisplayName})"
                    });
                    continue;
                }

                // Get or create team OU — check existing children first.
                if (!childrenByRegionOuId.TryGetValue(regionOu.Id, out var children))
                {
                    children = await ouRepository.GetChildrenAsync(regionOu.Id);
                    childrenByRegionOuId[regionOu.Id] = children;
                }

                var existingTeamOu = children.FirstOrDefault(
                    c => string.Equals(c.DisplayName, s.TeamOuName, StringComparison.OrdinalIgnoreCase));

                OrganizationUnit teamOu;
                if (existingTeamOu != null)
                {
                    teamOu = existingTeamOu;
                }
                else
                {
                    teamOu = new OrganizationUnit(GuidGenerator.Create(), s.TeamOuName,
                        parentId: regionOu.Id, tenantId: null);
                    await ouManager.CreateAsync(teamOu);
                    children.Add(teamOu); // keep cache consistent
                }

                // Get or create GSBH user
                Guid supervisorId;
                if (existingUsers.TryGetValue(s.Username, out var existingGsbh))
                {
                    supervisorId = existingGsbh.Id;
                    result.UsersSkipped++;
                }
                else
                {
                    var user = new IdentityUser(GuidGenerator.Create(), s.Username,
                        $"{s.Username}@hanoimilk.vn", tenantId: null)
                    { Name = s.DisplayName };

                    var r = await userManager.CreateAsync(user, s.Password);
                    if (!r.Succeeded)
                    {
                        result.UsersSkipped++;
                        result.Errors.Add(new ImportErrorDto
                        {
                            Sheet = "DS GSBH",
                            RowNumber = s.Index,
                            Message = $"'{s.DisplayName}': {string.Join(", ", r.Errors.Select(e => e.Description))}"
                        });
                        continue;
                    }

                    await userManager.AddToRoleAsync(user, DmsRoles.SalesSupervisor);
                    await userManager.SetOrganizationUnitsAsync(user, teamOu.Id);
                    supervisorId = user.Id;
                    result.UsersCreated++;
                }

                // Create DmsTeam if not already present for this OU
                var existingDmsTeam = await dmsTeamRepository.FindAsync(
                    t => t.OrganizationUnitId == teamOu.Id);
                if (existingDmsTeam == null)
                {
                    asmIdByRegion.TryGetValue(s.Region, out var asmId);
                    await dmsTeamRepository.InsertAsync(new Team
                    {
                        Id = GuidGenerator.Create(),
                        OrganizationUnitId = teamOu.Id,
                        ManagerUserId = asmId == Guid.Empty ? null : asmId,
                        SupervisorUserId = supervisorId,
                    }, autoSave: false);
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportErrorDto { Sheet = "DS GSBH", RowNumber = s.Index, Message = $"'{s.DisplayName}': {ex.Message}" });
            }
        }
    }

    private async Task PersistSalesUsersAsync(
        List<SalesUserSpec> specs,
        Dictionary<string, OrganizationUnit> ouByRegion,
        Dictionary<string, IdentityUser> existingUsers,
        ImportMasterDataResult result)
    {
        foreach (var s in specs)
        {
            try
            {
                if (existingUsers.TryGetValue(s.Username, out _))
                {
                    result.UsersSkipped++;
                    continue;
                }

                var user = new IdentityUser(GuidGenerator.Create(), s.Username,
                    $"{s.Username}@hanoimilk.vn", tenantId: null)
                { Name = s.DisplayName };

                var r = await userManager.CreateAsync(user, s.Password);
                if (!r.Succeeded)
                {
                    result.UsersSkipped++;
                    result.Errors.Add(new ImportErrorDto
                    {
                        Sheet = "DS NVBH",
                        RowNumber = s.Index,
                        Message = $"'{s.DisplayName}': {string.Join(", ", r.Errors.Select(e => e.Description))}"
                    });
                    continue;
                }

                await userManager.AddToRoleAsync(user, DmsRoles.SalesUser);
                if (ouByRegion.TryGetValue(s.Region, out var regionOu))
                    await userManager.SetOrganizationUnitsAsync(user, regionOu.Id);
                result.UsersCreated++;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportErrorDto { Sheet = "DS NVBH", RowNumber = s.Index, Message = $"'{s.DisplayName}': {ex.Message}" });
            }
        }
    }

    private async Task PersistDistributorsAsync(
        List<DistributorSpec> specs,
        Dictionary<string, OrganizationUnit> ouByRegion,
        ImportMasterDataResult result)
    {
        // Targeted query — only planned codes
        var plannedCodes = specs.Select(s => s.CustomerCode).ToList();
        var existingDistCodes = plannedCodes.Count == 0
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : (await distributorRepository.GetListAsync(
                d => d.OdsDistributorId != null && plannedCodes.Contains(d.OdsDistributorId)))
              .Select(d => d.OdsDistributorId!)
              .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var s in specs)
        {
            try
            {
                Guid tenantId;
                using (currentTenant.Change(null))
                {
                    var existing = await tenantRepository.FindByNameAsync(s.CustomerCode);
                    if (existing != null)
                    {
                        tenantId = existing.Id;
                    }
                    else
                    {
                        var tenant = await tenantManager.CreateAsync(s.CustomerCode);
                        await tenantRepository.InsertAsync(tenant, autoSave: true);
                        tenantId = tenant.Id;
                        result.TenantsCreated++;
                    }
                }

                if (existingDistCodes.Contains(s.CustomerCode)) continue;

                ouByRegion.TryGetValue(s.Region, out var ou);
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
                existingDistCodes.Add(s.CustomerCode);
                result.DistributorsCreated++;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportErrorDto
                {
                    Sheet = "DS NPP",
                    RowNumber = s.Index,
                    Message = $"'{s.Name}' ({s.CustomerCode}): {ex.Message}"
                });
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

        // Targeted query — only planned codes
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
