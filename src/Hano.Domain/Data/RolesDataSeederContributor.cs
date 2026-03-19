using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bamboo.Shared.Common;
using Microsoft.Extensions.Configuration;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
/* 
    * Creates initial roles/users that is needed to property run the application    
    */
public class RolesDataSeederContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IConfiguration _configuration;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IdentityRoleManager _identityRoleManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<IdentityRole, Guid> _identityRoleRepository;
    public RolesDataSeederContributor(
        IConfiguration configuration,
        IdentityUserManager identityUserManager,
        IdentityRoleManager identityRoleManager,
        IRepository<IdentityRole, Guid> identityRoleRepository,
        ICurrentTenant currentTenant, IGuidGenerator guidGenerator)
    {
        _configuration = configuration;
        _identityUserManager = identityUserManager;
        _identityRoleManager = identityRoleManager;
        _currentTenant = currentTenant;
        _guidGenerator = guidGenerator;
        _identityRoleRepository = identityRoleRepository;
    }

    [UnitOfWork]
    public async Task SeedAsync(DataSeedContext context)
    {
        var configurationSection = _configuration.GetSection("App");
        var domain = configurationSection["Domain"] ?? "ainativeglobal.com";
        var rolesName = configurationSection.GetSection("StaticRole").Get<List<string>>();
        rolesName = new List<string>();
        rolesName = new List<string>()
        {
            //"superadmin",
            "admin",
            //"owner",
            //"staff",
            //"guest",
        };
        if (_currentTenant.Id == null)
        {
            rolesName.AddRange([DmsRoles.Admin, DmsRoles.SaleManager, DmsRoles.SalesSupervisor, DmsRoles.SalesUser]);
        }
        else
        {
            rolesName.AddRange(["owner",]);
        }
        //rolesName.AddRange(["admin", "owner", "staff", "guest"]);
        using (_currentTenant.Change(context.TenantId))
        {
            if (rolesName != null)
            {
                foreach (var r in rolesName)
                {
                    if (r != null && !r.IsNullOrEmpty())
                    {
                        IdentityRole? role = await _identityRoleRepository.FirstOrDefaultAsync(x => x.Name == r && x.TenantId == _currentTenant.Id);
                        if (role == null)
                        {
                            role = new IdentityRole(_guidGenerator.Create(), r, _currentTenant.Id)
                            {
                                IsStatic = true,
                                IsPublic = true
                            };
                            try
                            {
                                await _identityRoleRepository.InsertAsync(role);
                            }
                            catch (Exception e)
                            {
                                var str = e.Message;
                            }
                        }
                    }
                }
            }
        }
    }
}