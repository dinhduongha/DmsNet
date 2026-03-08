using Microsoft.EntityFrameworkCore;
using Hano.Core.Domain.Identity;
using Hano.Core.Domain.Sessions;
using Hano.Core.Domain.Visits;
using Hano.Core.Domain.Orders;
using Hano.Core.Domain.Audit;
using Hano.Core.Domain.Feedback;
using Hano.Core.Domain.Notifications;
using Hano.Core.Domain.Routes;
using Hano.Core.Domain.Outlets;
using Hano.Core.Domain.MasterData;
using Hano.Core.Domain.Sync;
using Hano.Core.Domain.Photos;
using Hano.Core.Domain.AppVersion;
using Hano.Core.Domain.Reports;

namespace Hano.Core.EntityFrameworkCore;

// Copy these DbSet properties into your existing HanoCoreDbContext
public partial class CoreDbContext
{
    public DbSet<Device> Devices { get; set; } = null!;
    public DbSet<WorkSession> WorkSessions { get; set; } = null!;
    public DbSet<GpsBreadcrumb> GpsBreadcrumbs { get; set; } = null!;
    public DbSet<Visit> Visits { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderLine> OrderLines { get; set; } = null!;
    public DbSet<VehicleStock> VehicleStocks { get; set; } = null!;
    public DbSet<Reconciliation> Reconciliations { get; set; } = null!;
    public DbSet<ReconciliationItem> ReconciliationItems { get; set; } = null!;
    public DbSet<OsaReport> OsaReports { get; set; } = null!;
    public DbSet<OsaReportItem> OsaReportItems { get; set; } = null!;
    public DbSet<OosReport> OosReports { get; set; } = null!;
    public DbSet<OosReportItem> OosReportItems { get; set; } = null!;
    public DbSet<PosmReport> PosmReports { get; set; } = null!;
    public DbSet<PosmReportItem> PosmReportItems { get; set; } = null!;
    public DbSet<FeedbackReport> FeedbackReports { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<Route> Routes { get; set; } = null!;
    public DbSet<RouteOutlet> RouteOutlets { get; set; } = null!;
    public DbSet<Outlet> Outlets { get; set; } = null!;
    public DbSet<Sku> Skus { get; set; } = null!;
    public DbSet<PriceList> PriceLists { get; set; } = null!;
    public DbSet<Promotion> Promotions { get; set; } = null!;
    public DbSet<PosmItem> PosmItems { get; set; } = null!;
    public DbSet<Distributor> Distributors { get; set; } = null!;
    public DbSet<SyncQueue> SyncQueues { get; set; } = null!;
    public DbSet<Photo> Photos { get; set; } = null!;
    public DbSet<AppVersionConfig> AppVersionConfigs { get; set; } = null!;
    public DbSet<DailyReport> DailyReports { get; set; } = null!;
}
