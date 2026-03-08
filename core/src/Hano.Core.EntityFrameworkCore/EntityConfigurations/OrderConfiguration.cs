using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Hano.Core.Domain.Orders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Hano.Core.EntityFrameworkCore.EntityConfigurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("orders");
        b.ConfigureByConvention();
        b.Property(x => x.OrderCode).HasMaxLength(50);
        b.Property(x => x.TotalAmount).HasColumnType("decimal(15,2)");
        b.Property(x => x.DiscountAmount).HasColumnType("decimal(15,2)");
        b.Property(x => x.AmountCollected).HasColumnType("decimal(15,2)");
        b.HasIndex(x => x.VisitId);
        b.HasIndex(x => new { x.UserId, x.CreationTime });
        b.HasIndex(x => x.OrderCode).IsUnique().HasFilter("\"order_code\" IS NOT NULL");
        b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.OrderId);
    }
}

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> b)
    {
        b.ToTable("order_lines");
        b.ConfigureByConvention();
        b.Property(x => x.SkuCode).HasMaxLength(50);
        b.Property(x => x.SkuName).HasMaxLength(200);
        b.Property(x => x.Unit).HasMaxLength(20);
        b.Property(x => x.UnitPrice).HasColumnType("decimal(12,2)");
        b.Property(x => x.LineTotal).HasColumnType("decimal(15,2)");
        b.Property(x => x.Discount).HasColumnType("decimal(12,2)");
    }
}

public class VehicleStockConfiguration : IEntityTypeConfiguration<VehicleStock>
{
    public void Configure(EntityTypeBuilder<VehicleStock> b)
    {
        b.ToTable("vehicle_stocks");
        b.ConfigureByConvention();
        b.HasIndex(x => new { x.SessionId, x.SkuId }).IsUnique();
    }
}

public class ReconciliationConfiguration : IEntityTypeConfiguration<Reconciliation>
{
    public void Configure(EntityTypeBuilder<Reconciliation> b)
    {
        b.ToTable("reconciliations");
        b.ConfigureByConvention();
        b.Property(x => x.TotalSoldAmount).HasColumnType("decimal(15,2)");
        b.Property(x => x.TotalCollected).HasColumnType("decimal(15,2)");
        b.HasIndex(x => new { x.UserId, x.Date }).IsUnique();
        b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.ReconciliationId);
    }
}
