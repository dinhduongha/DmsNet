using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
//using Hano.Domain.Models;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.Identity;
using Volo.Abp.OpenIddict;
using Volo.Abp.OpenIddict.Applications;
using Volo.Abp.OpenIddict.Authorizations;
using Volo.Abp.OpenIddict.Scopes;
using Volo.Abp.OpenIddict.Tokens;
using Volo.Abp.TenantManagement;

namespace Hano.EntityFrameworkCore;

public static partial class ModelBuilderExtensions
{
    public static ModelBuilder SnakeCase(this ModelBuilder builder)
    {
        //var entityTypes = builder.Model.GetEntityTypes().ToList();
        //foreach (var entityType in entityTypes)
        //{
        //    if (entityType.BaseType != null)
        //    {
        //        builder.Ignore(entityType.ClrType);
        //    }
        //}

        foreach (var entity in builder.Model.GetEntityTypes())
        {
            if (entity.BaseType == null)
            {
                if (entity.GetTableName().StartsWith("tbl_"))
                {
                    entity.SetTableName(entity.GetTableName().ToSnakeCase());
                }
            }
        }

        foreach (var entity in builder.Model.GetEntityTypes())
        {
            if (!entity.GetTableName().StartsWith("tbl_"))
            {
                continue;
            }

            // Replace column names
            foreach (var property in entity.GetProperties())
            {
                var columnName = property.GetColumnName(StoreObjectIdentifier.Table(property.DeclaringEntityType.GetTableName(), null));
                property.SetColumnName(columnName.ToSnakeCase());
            }
            foreach (var key in entity.GetKeys())
            {
                key.SetName(key.GetName().ToSnakeCase());
            }

            foreach (var key in entity.GetForeignKeys())
            {
                key.SetConstraintName(key.GetConstraintName().ToSnakeCase());
            }

            foreach (var index in entity.GetIndexes())
            {
                //index.SetName(index.Name.ToSnakeCase());
                index.SetDatabaseName(index.Name.ToSnakeCase());

            }
        }
        return builder;
    }
}

public static class HanoContextModelCreatingExtensions
{
    public static void ConfigureHano(
        this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        /* Configure all entities here. Example:

        builder.Entity<Question>(b =>
        {
            //Configure table & schema name
            b.ToTable(CoreDbProperties.DbTablePrefix + "Questions", CoreDbProperties.DbSchema);

            b.ConfigureByConvention();

            //Properties
            b.Property(q => q.Title).IsRequired().HasMaxLength(QuestionConsts.MaxTitleLength);

            //Relations
            b.HasMany(question => question.Tags).WithOne().HasForeignKey(qt => qt.QuestionId);

            //Indexes
            b.HasIndex(q => q.CreationTime);
        });
        */

        // builder.Entity<AppOpenIddictApplication>(b =>
        // {
        //     b.ToTable("OpenIddictApplications"); // Sử dụng lại table cũ hoặc mới
        //     b.Property(x => x.TenantId);
        // });

        /* Configure your own tables/entities inside here */

        //builder.Entity<YourEntity>(b =>using Microsoft.Extensions.DependencyInjection;

        builder.Entity<IdentityUser>(b =>
        {
            b.HasIndex(u => u.NormalizedUserName)
            .IsUnique()
            .HasDatabaseName("IX_IdentityUser_NormalizedUserName_Unique_Global");

            b.HasIndex(u => u.NormalizedEmail)
                .IsUnique()
                .HasDatabaseName("IX_IdentityUser_NormalizedEmail_Unique_Global");

            // b.HasDiscriminator<string>("Discriminator")
            // .HasValue<Tenant>("Tenant")
            // .HasValue<TenantInfo>("TenantInfo");
            // b.HasKey(e => e.Id);
            // b.HasOne<Tenant>().WithOne();
            //.HasForeignKey<TenantInfo>(x => x.Id);
            //b.HasQueryFilter(e => true);
            //b.ConfigureByConvention();

        });

        builder.Entity<Tenant>(b =>
        {
            // b.HasDiscriminator<string>("Discriminator")
            // .HasValue<Tenant>("Tenant")
            // .HasValue<TenantInfo>("TenantInfo");
            // b.HasKey(e => e.Id);
            // b.HasOne<Tenant>().WithOne();
            //.HasForeignKey<TenantInfo>(x => x.Id);
            //b.HasQueryFilter(e => true);
            b.ConfigureByConvention();

        });

        // builder.Entity<UserInfo>(b =>
        // {
        //     b.ConfigureByConvention();
        //     b.HasKey(e => e.Id);
        //     b.HasOne<IdentityUser>().WithOne();
        //     //.HasForeignKey<UserInfo>(x => x.Id);            
        // });


    }

}