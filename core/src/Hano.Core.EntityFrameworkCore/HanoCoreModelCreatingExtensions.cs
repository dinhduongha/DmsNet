using System;
using System.Linq;
using Hano.Core.Domain.Notifications;
using Hano.Core.EntityFrameworkCore.EntityConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Volo.Abp.Domain.Entities;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Hano.Core.EntityFrameworkCore;

/// <summary>
/// Call in OnModelCreating: builder.ConfigureHanoCore();
/// </summary>
public static partial class HanoCoreModelCreatingExtensions
{
    public static void ConfigureHanoCore(this ModelBuilder builder)
    {
        builder.UseUuidV7();
        builder.ConfigureDevice();
        builder.ConfigureOsaReport();
        builder.ConfigureOsaReportItem();
        builder.ConfigureOosReport();
        builder.ConfigureOosReportItem();
        builder.ConfigurePosmReport();
        builder.ConfigurePosmReportItem();
        builder.ConfigureDailyReport();
        builder.ConfigureFeedbackReport();
        builder.ConfigureSku();
        builder.ConfigurePriceList();
        builder.ConfigureDistributor();
        builder.ConfigureNotification();
        builder.ConfigureOrder();
        builder.ConfigureOrderLine();
        builder.ConfigureVehicleStock();
        builder.ConfigureReconciliation();
        builder.ConfigureOutlet();
        builder.ConfigurePhoto();
        builder.ConfigureRoute();
        builder.ConfigureRouteOutlet();
        builder.ConfigureWorkSession();
        builder.ConfigureGpsBreadcrumb();
        builder.ConfigureSyncQueue();
        builder.ConfigureVisit();
        builder.ConfigureDmsOrganization();
        builder.ConfigureDmsTeam();

        //builder.UseUuidV7();
        builder.SnakeCase();

        // builder.ApplyConfiguration(new DeviceConfiguration());
        // builder.ApplyConfiguration(new WorkSessionConfiguration());
        // builder.ApplyConfiguration(new GpsBreadcrumbConfiguration());
        // builder.ApplyConfiguration(new VisitConfiguration());
        // builder.ApplyConfiguration(new OrderConfiguration());
        // builder.ApplyConfiguration(new OrderLineConfiguration());
        // builder.ApplyConfiguration(new VehicleStockConfiguration());
        // builder.ApplyConfiguration(new ReconciliationConfiguration());
        // builder.ApplyConfiguration(new OsaReportConfiguration());
        // builder.ApplyConfiguration(new OsaReportItemConfiguration());
        // //builder.ApplyConfiguration(new OosReportConfiguration());
        // builder.ApplyConfiguration(new OosReportItemConfiguration());
        // builder.ApplyConfiguration(new PosmReportConfiguration());
        // builder.ApplyConfiguration(new PosmReportItemConfiguration());
        // builder.ApplyConfiguration(new FeedbackReportConfiguration());
        // builder.ApplyConfiguration(new NotificationConfiguration());
        // builder.ApplyConfiguration(new RouteConfiguration());
        // builder.ApplyConfiguration(new RouteOutletConfiguration());
        // builder.ApplyConfiguration(new OutletConfiguration());
        // builder.ApplyConfiguration(new SkuConfiguration());
        // builder.ApplyConfiguration(new PriceListConfiguration());
        // builder.ApplyConfiguration(new DistributorConfiguration());
        // builder.ApplyConfiguration(new SyncQueueConfiguration());
        // builder.ApplyConfiguration(new PhotoConfiguration());
        // builder.ApplyConfiguration(new DailyReportConfiguration());
    }

    public static void ConfigureDevice(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>(b =>
        {
            //b.ToTable("devices");
            //b.ConfigureByConvention();
            b.HasKey(x => x.Id);

            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");

            b.Property(x => x.DeviceId).HasMaxLength(200).IsRequired();
            b.Property(x => x.Platform).HasMaxLength(20).IsRequired();
            b.Property(x => x.Model).HasMaxLength(100);
            b.Property(x => x.FcmToken).HasMaxLength(500);
            b.HasIndex(x => x.DeviceId).IsUnique();
            b.HasIndex(x => x.UserId);

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }

    public static void ConfigureOsaReport(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OsaReport>(b =>
        {
            b.HasIndex(x => x.VisitId);

            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.ReportId);

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }

    public static void ConfigureOsaReportItem(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OsaReportItem>(b =>
        {
            // Chỉ định rõ ràng thuộc tính Id này chính là Primary Key và map vào cột "id"
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }

    public static void ConfigureOosReport(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OosReport>(b =>
        {
            // Chỉ định rõ ràng thuộc tính Id này chính là Primary Key và map vào cột "id"
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }

    public static void ConfigureOosReportItem(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OosReportItem>(b =>
        {
            // Chỉ định rõ ràng thuộc tính Id này chính là Primary Key và map vào cột "id"
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }

    public static void ConfigurePosmReport(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PosmReport>(b =>
        {
            b.HasIndex(x => x.VisitId);
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.ReportId);

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();

        });
    }

    public static void ConfigurePosmReportItem(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PosmReportItem>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }


    public static void ConfigureDailyReport(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DailyReport>(b =>
        {
            b.Property(x => x.ReportType).HasMaxLength(10).IsRequired();
            b.HasIndex(x => new { x.UserId, x.Date }).IsUnique();
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }

    public static void ConfigureFeedbackReport(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FeedbackReport>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            b.Property(x => x.Category).HasMaxLength(50).IsRequired();
            b.Property(x => x.Content).HasMaxLength(500).IsRequired();
            b.HasIndex(x => x.VisitId);
            b.HasIndex(x => x.Type);

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }

    public static void ConfigureSku(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sku>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            b.Property(x => x.Category).HasMaxLength(50).IsRequired();
            b.Property(x => x.Code).HasMaxLength(50).IsRequired();
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.HasIndex(x => x.Code).IsUnique();

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }

    public static void ConfigurePriceList(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PriceList>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");

            b.Property(x => x.UnitPrice).HasColumnType("decimal(12,2)");
            b.Property(x => x.PromoPrice).HasColumnType("decimal(12,2)");
            b.HasIndex(x => new { x.SkuId, x.DistributorId, x.EffectiveFrom });

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }

    public static void ConfigureDistributor(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Distributor>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");

            b.Property(x => x.Name).HasMaxLength(200).IsRequired();

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }

    public static void ConfigureNotification(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");

            b.Property(x => x.Type).HasMaxLength(20).IsRequired();
            b.Property(x => x.Title).HasMaxLength(200).IsRequired();
            b.HasIndex(x => new { x.TargetUserId, x.IsRead });

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }

    public static void ConfigureOrder(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");

            b.Property(x => x.OrderCode).HasMaxLength(50);
            b.Property(x => x.TotalAmount).HasColumnType("decimal(15,2)");
            b.Property(x => x.DiscountAmount).HasColumnType("decimal(15,2)");
            b.Property(x => x.AmountCollected).HasColumnType("decimal(15,2)");
            b.HasIndex(x => x.VisitId);
            b.HasIndex(x => new { x.UserId, x.CreationTime });
            b.HasIndex(x => x.OrderCode).IsUnique().HasFilter("\"order_code\" IS NOT NULL");
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.OrderId);

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }

    public static void ConfigureOrderLine(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderLine>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");

            b.Property(x => x.SkuCode).HasMaxLength(50);
            b.Property(x => x.SkuName).HasMaxLength(200);
            b.Property(x => x.Unit).HasMaxLength(20);
            b.Property(x => x.UnitPrice).HasColumnType("decimal(12,2)");
            b.Property(x => x.LineTotal).HasColumnType("decimal(15,2)");
            b.Property(x => x.Discount).HasColumnType("decimal(12,2)");

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }

    public static void ConfigureVehicleStock(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VehicleStock>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");

            b.HasIndex(x => new { x.SessionId, x.SkuId }).IsUnique();

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }

    public static void ConfigureReconciliation(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Reconciliation>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");

            b.Property(x => x.TotalSoldAmount).HasColumnType("decimal(15,2)");
            b.Property(x => x.TotalCollected).HasColumnType("decimal(15,2)");
            b.HasIndex(x => new { x.UserId, x.Date }).IsUnique();
            b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.ReconciliationId);

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }

    public static void ConfigureOutlet(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Outlet>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");

            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Address).HasMaxLength(500).IsRequired();
            b.Property(x => x.Latitude).HasColumnType("decimal(10,7)");
            b.Property(x => x.Longitude).HasColumnType("decimal(10,7)");
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.OdsOutletId);

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }

    public static void ConfigurePhoto(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Photo>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");

            b.Property(x => x.S3Key).HasMaxLength(500).IsRequired();
            b.Property(x => x.Latitude).HasColumnType("decimal(10,7)");
            b.Property(x => x.Longitude).HasColumnType("decimal(10,7)");
            b.HasIndex(x => x.VisitId);

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }
    public static void ConfigureRoute(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Route>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");

            b.Property(x => x.Name).HasMaxLength(100).IsRequired();
            b.HasIndex(x => new { x.AssignedUserId, x.DayOfWeek });
            b.HasMany(x => x.Outlets).WithOne().HasForeignKey(x => x.RouteId);

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }
    public static void ConfigureRouteOutlet(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RouteOutlet>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");

            b.HasIndex(x => new { x.RouteId, x.OutletId }).IsUnique();
            b.HasIndex(x => new { x.RouteId, x.SequenceOrder }).IsUnique();

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }
    public static void ConfigureWorkSession(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkSession>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");

            b.Property(x => x.SodLatitude).HasColumnType("decimal(10,7)");
            b.Property(x => x.SodLongitude).HasColumnType("decimal(10,7)");
            b.Property(x => x.EodLatitude).HasColumnType("decimal(10,7)");
            b.Property(x => x.EodLongitude).HasColumnType("decimal(10,7)");
            b.Property(x => x.TotalDistanceKm).HasColumnType("decimal(8,2)");
            b.Property(x => x.TotalRevenue).HasColumnType("decimal(15,2)");
            b.HasIndex(x => new { x.UserId, x.Date }).IsUnique();
            b.HasMany(x => x.Breadcrumbs).WithOne().HasForeignKey(x => x.SessionId);

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }
    public static void ConfigureGpsBreadcrumb(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GpsBreadcrumb>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");

            b.Property(x => x.Latitude).HasColumnType("decimal(10,7)");
            b.Property(x => x.Longitude).HasColumnType("decimal(10,7)");
            b.HasIndex(x => x.SessionId);

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }
    public static void ConfigureSyncQueue(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SyncQueue>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");

            b.Property(x => x.EntityType).HasMaxLength(50).IsRequired();
            b.Property(x => x.Action).HasMaxLength(20).IsRequired();
            b.HasIndex(x => x.Status);
            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }

    public static void ConfigureVisit(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Visit>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");

            b.Property(x => x.CheckinLatitude).HasColumnType("decimal(10,7)");
            b.Property(x => x.CheckinLongitude).HasColumnType("decimal(10,7)");
            b.Property(x => x.CheckoutLatitude).HasColumnType("decimal(10,7)");
            b.Property(x => x.CheckoutLongitude).HasColumnType("decimal(10,7)");
            b.Property(x => x.GpsDistanceM).HasColumnType("decimal(8,2)");
            b.Property(x => x.SkipReason).HasMaxLength(200);
            b.HasIndex(x => x.SessionId);
            b.HasIndex(x => new { x.UserId, x.CheckinAt });
            b.HasIndex(x => x.OutletId);

            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }

    public static void ConfigureDmsOrganization(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>(b =>
        {
            b.ToTable("dms_organizations");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            b.Property(x => x.OrganizationUnitId).HasColumnName("organization_unit_id").IsRequired();
            b.Property(x => x.AdminUserId).HasColumnName("admin_user_id");
            b.Property(x => x.SaleManagerUserId).HasColumnName("sale_manager_user_id");
            b.HasIndex(x => x.OrganizationUnitId).IsUnique();
            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }

    public static void ConfigureDmsTeam(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>(b =>
        {
            b.ToTable("dms_teams");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            b.Property(x => x.OrganizationUnitId).HasColumnName("organization_unit_id").IsRequired();
            b.Property(x => x.ManagerUserId).HasColumnName("manager_user_id");
            b.Property(x => x.SupervisorUserId).HasColumnName("supervisor_user_id");
            b.HasIndex(x => x.OrganizationUnitId).IsUnique();
            b.TryConfigureConcurrencyStamp();
            b.TryConfigureExtraProperties();
        });
    }

    public static ModelBuilder UseUuidV7(this ModelBuilder builder)
    {
        // For Npgsql
        var entityTypes = builder.Model.GetEntityTypes()
            // Lọc ra các entity class kế thừa IEntity<Guid> và không phải abstract
            .Where(t => typeof(IEntity<Guid>).IsAssignableFrom(t.ClrType) && !t.ClrType.IsAbstract);

        foreach (var entityType in entityTypes)
        {
            builder.Entity(entityType.ClrType, b =>
            {
                b.Property("Id")
                    .HasDefaultValueSql("uuidv7()");
            });
        }
        return builder;
    }
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
            if (entity.ClrType.Namespace?.StartsWith("Volo.Abp") == true)
                continue;
            var tblName = entity.GetTableName();
            if (tblName == null || tblName.StartsWith("Abp"))
                continue;
            if (entity.BaseType == null && !entity.IsOwned())
            {
                entity.SetTableName(entity.GetTableName()?.ToSnakeCase());
            }
        }

        foreach (var entity in builder.Model.GetEntityTypes())
        {
            if (entity.ClrType.Namespace?.StartsWith("Volo.Abp") == true)
                continue;
            if (entity.IsOwned())
                continue;
            var tblName = entity.GetTableName();
            if (tblName == null || tblName.StartsWith("Abp"))
                continue;
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
