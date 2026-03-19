using Hano.Core.Application;
using Hano.Core.Domain.MasterData;
using Hano.Core.Domain.Organizations;
using Hano.Core.Domain.Outlets;
using Hano.Core.Domain.Shared.Enums;
using Hano.Core.Import.Dtos;
using Hano.Core.Import.Excel;
using Hano.Core.Import.Helpers;
using Hano.Core.Import.Parsers;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;

using Bamboo.Shared.Common;
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
    IRepository<DmsOrganization, Guid> dmsOrgRepository,
    IRepository<DmsTeam, Guid> dmsTeamRepository,
    IRepository<Sku, Guid> skuRepository,
    IUnitOfWorkManager uowManager,
    UsernamePasswordGenerator usernameGen
) : HanoCoreAppServiceBase, IImportAppService
{
    // ── Region name → username prefix (Vietnamese diacritics normalised externally) ──
    // Keys are the NFC-normalised region display names as they appear after NormalizeDisplay().
    private static readonly IReadOnlyDictionary<string, string> RegionCodeMap =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Hà Nội",             "hn"   },
            { "Tây Bắc",            "tb"   },
            { "Đông Bắc 1",         "db1"  },
            { "Đông Bắc 2",         "db2"  },
            { "Duyên Hải",          "dh"   },
            { "Bắc Miền Trung",     "bmt"  },
            { "Tây Nguyên",         "tn"   },
            { "Miền Đông",          "md"   },
            { "Sữa Bộ Miền Bắc",    "sbmb" },
            { "Sữa Bộ Miền Trung",  "sbmt" },
            { "Siêu Thị",           "st"   },
            { "Trường Học",         "th"   },
            { "Công Nghiệp",        "cn"   },
        };

    // ─────────────────────────────────────────────────────────────────────────
    // ImportMasterDataAsync
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<ImportMasterDataResult> ImportMasterDataAsync(
        Stream fileStream, ImportMasterDataInput input)
    {
        var result = new ImportMasterDataResult { IsDryRun = input.DryRun };

        var reader = readerFactory.Create(input.ReaderType);
        var parser = new MasterDataExcelParser(reader);
        var data = parser.Parse(fileStream);

        using var uow = uowManager.Begin(requiresNew: true, isTransactional: true);
        try
        {
            // ── Pre-load existing data into memory ────────────────────────────
            var usedUsernames = new HashSet<string>(
                userManager.Users.Select(u => u.UserName!),
                StringComparer.OrdinalIgnoreCase);

            var ouByName = (await ouRepository.GetListAsync())
                .ToDictionary(ou => ou.DisplayName, StringComparer.OrdinalIgnoreCase);

            var existingDistCodes = (await distributorRepository.GetListAsync())
                .Where(d => d.OdsDistributorId != null)
                .Select(d => d.OdsDistributorId!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var dmsOrgByOuId = (await dmsOrgRepository.GetListAsync())
                .ToDictionary(d => d.OrganizationUnitId);

            var dmsTeamByOuId = (await dmsTeamRepository.GetListAsync())
                .ToDictionary(d => d.OrganizationUnitId);

            // ── Step 1: DS Admin → dms_admin users ────────────────────────────
            var adminByRegion = await ImportAdminUsersAsync(data.Admins, usedUsernames, result);

            // ── Step 2: Vùng & ASM → Region OUs + dms_sale_manager + DmsOrganization
            var (ouMap, asmByRegion) = await ImportRegionsAndAsmAsync(
                data.Regions, ouByName, usedUsernames, adminByRegion, dmsOrgByOuId, result);

            // ── Step 3: DS GSBH → Child OUs + dms_sale_supervisor + DmsTeam ──
            await ImportSupervisorsAsync(
                data.Supervisors, ouMap, asmByRegion, usedUsernames, dmsTeamByOuId, result);

            // ── Step 4: DS NVBH → dms_sale_user users ────────────────────────
            await ImportSalesUsersAsync(data.Salespeople, ouMap, usedUsernames, result);

            // ── Step 5: DS NPP → Tenant + Distributor ────────────────────────
            await ImportDistributorsAsync(data.Distributors, ouMap, existingDistCodes, result);

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

        var existingCodes = (await skuRepository.GetListAsync())
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

    // ─────────────────────────────────────────────────────────────────────────
    // Private step helpers
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<Dictionary<string, IdentityUser>> ImportAdminUsersAsync(
        List<PersonRow> admins,
        HashSet<string> usedUsernames,
        ImportMasterDataResult result)
    {
        var adminByRegion = new Dictionary<string, IdentityUser>(StringComparer.OrdinalIgnoreCase);

        foreach (var person in admins)
        {
            try
            {
                var displayName = VietnameseSlugHelper.NormalizeDisplay(person.Name);
                var (username, password) = usernameGen.Generate(displayName, null, usedUsernames);

                var user = new IdentityUser(
                    GuidGenerator.Create(), username, $"{username}@hanoimilk.vn", tenantId: null)
                { Name = displayName };

                var r = await userManager.CreateAsync(user, password);
                if (!r.Succeeded)
                {
                    result.UsersSkipped++;
                    result.Errors.Add(new ImportErrorDto
                    {
                        Sheet = "DS ADMIN",
                        Message = $"Cannot create '{displayName}': {string.Join(", ", r.Errors.Select(e => e.Description))}"
                    });
                    continue;
                }

                await userManager.AddToRoleAsync(user, DmsRoles.Admin);
                if (!string.IsNullOrWhiteSpace(person.Region))
                    adminByRegion[person.Region] = user;

                result.UsersCreated++;
                result.CreatedUsers.Add(new ImportedUserRecord
                {
                    DisplayName = displayName,
                    Username = username,
                    Password = password,
                    Role = DmsRoles.Admin,
                    Region = person.Region,
                });
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportErrorDto { Sheet = "DS ADMIN", Message = $"'{person.Name}': {ex.Message}" });
            }
        }

        return adminByRegion;
    }

    private async Task<(Dictionary<string, OrganizationUnit> OuMap, Dictionary<string, IdentityUser> AsmByRegion)>
        ImportRegionsAndAsmAsync(
            List<RegionRow> regions,
            Dictionary<string, OrganizationUnit> existingOuByName,
            HashSet<string> usedUsernames,
            Dictionary<string, IdentityUser> adminByRegion,
            Dictionary<Guid, DmsOrganization> dmsOrgByOuId,
            ImportMasterDataResult result)
    {
        var ouMap = new Dictionary<string, OrganizationUnit>(existingOuByName, StringComparer.OrdinalIgnoreCase);
        var asmByRegion = new Dictionary<string, IdentityUser>(StringComparer.OrdinalIgnoreCase);

        foreach (var region in regions)
        {
            if (string.IsNullOrWhiteSpace(region.Region)) continue;
            var regionName = VietnameseSlugHelper.NormalizeDisplay(region.Region);

            OrganizationUnit ou;
            if (ouMap.TryGetValue(regionName, out var existingOu))
            {
                ou = existingOu;
                result.RegionsSkipped++;
            }
            else
            {
                ou = new OrganizationUnit(GuidGenerator.Create(), regionName, parentId: null, tenantId: null);
                await ouManager.CreateAsync(ou);
                ouMap[regionName] = ou;
                result.RegionsCreated++;
            }

            IdentityUser? asmUser = null;
            if (!string.IsNullOrWhiteSpace(region.AsmName))
            {
                var asmName = VietnameseSlugHelper.NormalizeDisplay(region.AsmName);
                var (username, password) = usernameGen.Generate(asmName, null, usedUsernames);

                var user = new IdentityUser(
                    GuidGenerator.Create(), username, $"{username}@hanoimilk.vn", tenantId: null)
                { Name = asmName };

                var r = await userManager.CreateAsync(user, password);
                if (r.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, DmsRoles.SaleManager);
                    await userManager.SetOrganizationUnitsAsync(user, ou.Id);
                    asmByRegion[regionName] = user;
                    asmUser = user;
                    result.UsersCreated++;
                    result.CreatedUsers.Add(new ImportedUserRecord
                    {
                        DisplayName = asmName,
                        Username = username,
                        Password = password,
                        Role = DmsRoles.SaleManager,
                        Region = regionName,
                    });
                }
                else
                {
                    result.UsersSkipped++;
                    result.Errors.Add(new ImportErrorDto
                    {
                        Sheet = "VÙNG & ASM",
                        Message = $"ASM '{asmName}': {string.Join(", ", r.Errors.Select(e => e.Description))}"
                    });
                }
            }

            if (!dmsOrgByOuId.ContainsKey(ou.Id))
            {
                adminByRegion.TryGetValue(regionName, out var adminUser);
                var dmsOrg = new DmsOrganization
                {
                    Id = GuidGenerator.Create(),
                    OrganizationUnitId = ou.Id,
                    AdminUserId = adminUser?.Id,
                    SaleManagerUserId = asmUser?.Id,
                };
                await dmsOrgRepository.InsertAsync(dmsOrg, autoSave: false);
                dmsOrgByOuId[ou.Id] = dmsOrg;
            }
        }

        return (ouMap, asmByRegion);
    }

    private async Task ImportSupervisorsAsync(
        List<PersonRow> supervisors,
        Dictionary<string, OrganizationUnit> ouMap,
        Dictionary<string, IdentityUser> asmByRegion,
        HashSet<string> usedUsernames,
        Dictionary<Guid, DmsTeam> dmsTeamByOuId,
        ImportMasterDataResult result)
    {
        var childOuNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var person in supervisors)
        {
            try
            {
                var displayName = VietnameseSlugHelper.NormalizeDisplay(person.Name);
                var regionName = VietnameseSlugHelper.NormalizeDisplay(person.Region);

                if (!ouMap.TryGetValue(regionName, out var regionOu))
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Sheet = "DS GSBH",
                        Message = $"Region OU not found for '{regionName}' (GSBH: {displayName})"
                    });
                    continue;
                }

                var teamOuName = childOuNames.Contains(displayName)
                    ? $"{displayName} ({regionName})"
                    : displayName;
                childOuNames.Add(teamOuName);

                var teamOu = new OrganizationUnit(
                    GuidGenerator.Create(), teamOuName, parentId: regionOu.Id, tenantId: null);
                await ouManager.CreateAsync(teamOu);

                var regionCode = RegionCodeMap.TryGetValue(regionName, out var rc) ? rc : null;
                var (username, password) = usernameGen.Generate(displayName, regionCode, usedUsernames);

                var user = new IdentityUser(
                    GuidGenerator.Create(), username, $"{username}@hanoimilk.vn", tenantId: null)
                { Name = displayName };

                var r = await userManager.CreateAsync(user, password);
                if (!r.Succeeded)
                {
                    result.UsersSkipped++;
                    result.Errors.Add(new ImportErrorDto
                    {
                        Sheet = "DS GSBH",
                        Message = $"Cannot create '{displayName}': {string.Join(", ", r.Errors.Select(e => e.Description))}"
                    });
                    continue;
                }

                await userManager.AddToRoleAsync(user, DmsRoles.SalesSupervisor);
                await userManager.SetOrganizationUnitsAsync(user, teamOu.Id);

                if (!dmsTeamByOuId.ContainsKey(teamOu.Id))
                {
                    asmByRegion.TryGetValue(regionName, out var asmUser);
                    var dmsTeam = new DmsTeam
                    {
                        Id = GuidGenerator.Create(),
                        OrganizationUnitId = teamOu.Id,
                        ManagerUserId = asmUser?.Id,
                        SupervisorUserId = user.Id,
                    };
                    await dmsTeamRepository.InsertAsync(dmsTeam, autoSave: false);
                    dmsTeamByOuId[teamOu.Id] = dmsTeam;
                }

                result.UsersCreated++;
                result.CreatedUsers.Add(new ImportedUserRecord
                {
                    DisplayName = displayName,
                    Username = username,
                    Password = password,
                    Role = DmsRoles.SalesSupervisor,
                    Region = regionName,
                });
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportErrorDto { Sheet = "DS GSBH", Message = $"'{person.Name}': {ex.Message}" });
            }
        }
    }

    private async Task ImportSalesUsersAsync(
        List<PersonRow> salespeople,
        Dictionary<string, OrganizationUnit> ouMap,
        HashSet<string> usedUsernames,
        ImportMasterDataResult result)
    {
        foreach (var person in salespeople)
        {
            try
            {
                var displayName = VietnameseSlugHelper.NormalizeDisplay(person.Name);
                var regionName = VietnameseSlugHelper.NormalizeDisplay(person.Region);
                var regionCode = RegionCodeMap.TryGetValue(regionName, out var rc) ? rc : null;

                var (username, password) = usernameGen.Generate(displayName, regionCode, usedUsernames);

                var user = new IdentityUser(
                    GuidGenerator.Create(), username, $"{username}@hanoimilk.vn", tenantId: null)
                { Name = displayName };

                var r = await userManager.CreateAsync(user, password);
                if (!r.Succeeded)
                {
                    result.UsersSkipped++;
                    result.Errors.Add(new ImportErrorDto
                    {
                        Sheet = "DS NVBH",
                        Message = $"Cannot create '{displayName}': {string.Join(", ", r.Errors.Select(e => e.Description))}"
                    });
                    continue;
                }

                await userManager.AddToRoleAsync(user, DmsRoles.SalesUser);
                if (ouMap.TryGetValue(regionName, out var regionOu))
                    await userManager.SetOrganizationUnitsAsync(user, regionOu.Id);

                result.UsersCreated++;
                result.CreatedUsers.Add(new ImportedUserRecord
                {
                    DisplayName = displayName,
                    Username = username,
                    Password = password,
                    Role = DmsRoles.SalesUser,
                    Region = regionName,
                });
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportErrorDto { Sheet = "DS NVBH", Message = $"'{person.Name}': {ex.Message}" });
            }
        }
    }

    private async Task ImportDistributorsAsync(
        List<DistributorRow> distributors,
        Dictionary<string, OrganizationUnit> ouMap,
        HashSet<string> existingDistCodes,
        ImportMasterDataResult result)
    {
        foreach (var dist in distributors)
        {
            try
            {
                var distName = VietnameseSlugHelper.NormalizeDisplay(dist.Name);
                var regionName = VietnameseSlugHelper.NormalizeDisplay(dist.Region);

                Guid tenantId;
                using (currentTenant.Change(null))
                {
                    var existing = await tenantRepository.FindByNameAsync(dist.CustomerCode);
                    if (existing != null)
                    {
                        tenantId = existing.Id;
                    }
                    else
                    {
                        var tenant = await tenantManager.CreateAsync(dist.CustomerCode);
                        await tenantRepository.InsertAsync(tenant, autoSave: true);
                        tenantId = tenant.Id;
                        result.TenantsCreated++;
                    }
                }

                if (existingDistCodes.Contains(dist.CustomerCode)) continue;

                ouMap.TryGetValue(regionName, out var ou);

                var distributor = new Distributor
                {
                    Id = GuidGenerator.Create(),
                    TenantId = tenantId,
                    OrganizationUnitId = ou?.Id,
                    Name = distName,
                    Address = VietnameseSlugHelper.NormalizeDisplay(dist.Address),
                    Region = regionName,
                    OdsDistributorId = dist.CustomerCode,
                    IsActive = true,
                };

                await distributorRepository.InsertAsync(distributor, autoSave: false);
                existingDistCodes.Add(dist.CustomerCode);
                result.DistributorsCreated++;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportErrorDto
                {
                    Sheet = "DS NPP",
                    Message = $"'{dist.Name}' ({dist.CustomerCode}): {ex.Message}"
                });
            }
        }
    }
}
