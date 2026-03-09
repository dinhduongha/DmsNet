# CLAUDE.md

This file provides context for Claude Code when working on the **Hano Backend** — the server-side component of the DX & AI Hanoimilk system.

## Project Overview

A **modular monolith** REST API built with **ABP Framework Community Edition** on **.NET 10** and **PostgreSQL**. It serves a Blazor web UI and future mobile clients for field sales operations (NVBH), supervisors (GSBH), and area managers (ASM) in the dairy distribution industry (Hanoimilk).

The solution follows [ABP Modular Monolith architecture](https://abp.io/architecture/modular-monolith):

- **`Hano.Core`** — the business DDD module (inside `core/`), containing all domain logic for Hanoimilk operations.
- **`Hano`** — the main application shell (inside `src/`), hosting ABP infrastructure (Identity, Tenancy, OpenIddict) and wiring everything together.

## Tech Stack

- **Framework:** ABP Framework Community 10.2.x (DDD, modular monolith)
- **Language:** C# / .NET 10, nullable reference types enabled
- **Database:** PostgreSQL (EF Core ORM), UUIDv7 primary keys, snake_case column naming
- **Mapping:** Riok.Mapperly (compile-time code generation, NOT AutoMapper)
- **Auth:** OpenIddict (separate AuthServer process), OAuth 2.0 + PKCE
- **Cache / Lock:** Redis (StackExchange.Redis, ABP Distributed Locking)
- **Background Jobs:** Hangfire (PostgreSQL / Redis storage)
- **UI:** Blazor WebAssembly (LeptonX Lite theme, hosted on Blazor server)
- **Logging:** Serilog + ABP Serilog integration
- **CI/CD:** GitHub Actions

## Solution Layout

```
Hano.slnx                             # Main solution file

core/                                 # Hano.Core — business DDD module
  src/
    Hano.Core.Domain.Shared/          # Enums, constants, shared DTOs, error codes
    Hano.Core.Domain/                 # Entities, domain services, repository interfaces
      AppVersion/ Audit/ Feedback/ Identity/ MasterData/
      Notifications/ Orders/ Outlets/ Photos/ Reports/
      Routes/ Sessions/ Settings/ Sync/ Visits/
    Hano.Core.Application.Contracts/  # IAppService interfaces, input/output DTOs
    Hano.Core.Application/            # AppService implementations, Mapperly mappers
    Hano.Core.EntityFrameworkCore/    # CoreDbContext, migrations, repository impl
    Hano.Core.HttpApi/                # Controllers (manual routing, inherit HanoCoreController)
    Hano.Core.HttpApi.Client/         # HTTP client proxy for the module
    Hano.Core.BackgroundJobs/         # Hangfire background jobs & workers
    Hano.Core.Blazor/                 # Blazor server-side UI components (module)
    Hano.Core.Blazor.WebAssembly/     # Blazor WASM UI components (module)
    Hano.Core.Blazor.WebAssembly.Bundling/
    Hano.Core.Installer/              # Module installer/seeder

src/                                  # Hano — main app shell
  Hano.Domain.Shared/                 # ABP shared constants, multi-tenancy config
  Hano.Domain/                        # ABP domain (Identity, OpenIddict, settings, etc.)
  Hano.Application.Contracts/         # Shell app contracts
  Hano.Application/                   # Shell app services
  Hano.EntityFrameworkCore/           # Shell DbContext, migrations
  Hano.HttpApi/                        # Shell HTTP API controllers
  Hano.HttpApi.Client/                 # Shell HTTP client proxy
  Hano.HttpApi.Host/                   # Main API entry point (startup, DI, middleware)
  Hano.AuthServer/                     # OpenIddict AuthServer (separate deployment)
  Hano.Blazor/                         # Blazor server host (renders WASM)
  Hano.Blazor.Client/                  # Blazor WASM client
  Hano.DbMigrator/                     # CLI tool to run EF migrations

shared/
  Bamboo.Shared.Common/                # Shared utilities across modules

test/
  Hano.Domain.Tests/
  Hano.Application.Tests/
  Hano.EntityFrameworkCore.Tests/
  Hano.TestBase/
  Hano.HttpApi.Client.ConsoleTestApp/
```

## Build & Run Commands

```bash
dotnet build                                                    # Build entire solution

# Run main API host
dotnet run --project src/Hano.HttpApi.Host

# Run AuthServer (separate process)
dotnet run --project src/Hano.AuthServer

# Run Blazor UI
dotnet run --project src/Hano.Blazor

# EF Migrations (target: Core module DbContext, startup: HttpApi.Host)
dotnet ef migrations add <Name> \
  -p core/src/Hano.Core.EntityFrameworkCore \
  -s src/Hano.HttpApi.Host

dotnet ef database update \
  -p core/src/Hano.Core.EntityFrameworkCore \
  -s src/Hano.HttpApi.Host

# Run DB Migrator tool
dotnet run --project src/Hano.DbMigrator

dotnet test                                   # Run all tests
dotnet test --collect:"XPlat Code Coverage"   # With coverage
```

## Domain Modules (inside Hano.Core.Domain)

| Domain | Purpose | Key Entities |
|--------|---------|--------------|
| **Identity** | Users, roles, device binding | AppUser, Device |
| **Sessions** | SOD/EOD work sessions, GPS tracking | WorkSession, GpsBreadcrumb |
| **Visits** | Check-in/out at outlets | Visit |
| **Orders** | DSR/DSD orders, vehicle stock | Order, OrderLine, VehicleStock, Reconciliation |
| **Audit** | OSA/OOS/POSM store audits | OsaReport, OosReport, PosmReport, AuditPhoto |
| **Reports** | Aggregated reports | (various report entities) |
| **Feedback** | Issues & feedback at outlets | FeedbackReport |
| **Notifications** | Push notifications (FCM/APNs) | Notification, DeviceToken |
| **Routes** | Sales route management | Route, RouteOutlet |
| **Outlets** | Point-of-sale management | Outlet, OutletPhoto |
| **MasterData** | SKUs, prices, promotions, delta sync | Sku, PriceList, Promotion, Distributor |
| **Sync** | Offline queue, conflict resolution, ODS sync | SyncQueue, SyncLog |
| **Photos** | S3 upload/download, metadata | Photo, PhotoMetadata |
| **AppVersion** | Mobile/web app version check | AppVersionConfig |
| **Settings** | Module-level settings | — |

## API Conventions

- **Auth:** Bearer JWT from AuthServer (all endpoints require auth)
- **Content-Type:** `application/json`
- **Pagination:** `?skipCount=0&maxResultCount=20`
- **List response:** `{ items: [], totalCount: N }` (ABP standard)
- **Error response:** `{ error: { code, message, details, validationErrors } }`
- **Timestamps:** ISO 8601 UTC
- **Primary Keys:** UUIDv7 (custom `UuidV7GuidGenerator` registered in `HanoDomainModule`)

## Roles & Permissions (ABP Permission System)

- **NVBH** (field sales): Creates visits, orders, reports, feedbacks. Sees only own data.
- **GSBH** (supervisor): Manages routes, approves outlets, sends notifications to team. Sees team data.
- **ASM** (area manager): Approves routes, views regional dashboards. Sees regional data.

Data isolation is enforced at the Domain layer via ABP Global Query Filters.

## Key Business Rules

- One active device per user (device binding).
- One work session per NVBH per day (`UNIQUE(user_id, date)`).
- GPS check-in validation: >200m → `VIOLATION` flag, requires reason.
- DSD orders deduct from vehicle stock; DSR orders go to distributor via ODS.
- Offline sync order: Session → Visit → Order → Reports → Feedback → Photos.
- Conflict resolution: ODS is System of Record (server wins by default).
- Master data delta sync via `lastSyncTimestamp`; no timestamp = full sync.
- Photo upload via S3 presigned URLs (never through the API body).

## ODS Integration

Backend syncs with ODS/TRAIDA (external system) via internal REST APIs:

- **ODS → Backend:** Master data (webhook + poll every 4h), order status updates.
- **Backend → ODS:** Visits, orders, reports, feedbacks (near real-time via background jobs).
- **Retry policy:** 3 attempts with backoff 30s → 2min → 10min. After 3 fails → `FAILED` + admin alert.
- **Queue:** All outbound sync goes through `sync_queue` table.

## Coding Standards

- Follow ABP Framework conventions for entities, AppServices, DTOs, and repositories.
- Use ABP `FluentValidation` for all input DTOs.
- Use `async/await` throughout — no synchronous DB calls.
- Entities inherit from ABP base classes (`FullAuditedAggregateRoot<Guid>`, `AuditedEntity<Guid>`, etc.).
- Soft-delete (`IsDeleted`) for critical entities.
- Use ABP's `IRepository<T>` and `IQueryable` — avoid raw SQL unless performance-critical.
- Keep AppServices thin; complex logic goes into Domain Services.
- All background jobs use Hangfire via ABP's `IBackgroundJobManager`.
- Log with Serilog; include correlation IDs in all API requests.
- **Module separation:** Business logic belongs in `Hano.Core.*`. Infrastructure/ABP wiring belongs in `Hano.*` (shell).
- **Mapping:** Use Riok.Mapperly (`[Mapper]` static partial classes), NOT AutoMapper.
- **DB naming:** snake_case for table and column names (`[Table("xxx")]`, `[Column("xxx")]`).
- **Controllers:** Manual routing with `[Route("api/v1/...")]`, inherit `HanoCoreController`.
- **AppServices:** Inherit `HanoCoreAppServiceBase`, implement `I{Name}AppService`.
- **Idempotency:** Check existing records before insert in create operations.
- **File-scoped namespaces:** Use `namespace Xxx;` (not block-scoped).

## Testing

- Unit tests: xUnit + NSubstitute for mocking.
- Integration tests: TestContainers with PostgreSQL.
- Always run `dotnet test` before committing.
- Target ≥ 70% code coverage.
- Name tests: `MethodName_Scenario_ExpectedResult`.

## Security Checklist

- All endpoints require Bearer JWT (`[Authorize]`).
- Role-based permissions via `[Authorize(CorePermissions.Xxx)]` (Core module) or `[Authorize(HanoPermissions.Xxx)]` (Shell).
- Rate limiting: 100 req/min/user.
- HTTPS only (TLS 1.2+).
- Never log sensitive data (tokens, passwords).
- Use EF Core parameterized queries (no string concatenation in SQL).

## Terminology

| Term | Meaning |
|------|---------|
| **NVBH** | Nhân viên bán hàng (field salesperson) |
| **GSBH** | Giám sát bán hàng (sales supervisor) |
| **ASM** | Area Sales Manager |
| **SOD/EOD** | Start of Day / End of Day work session |
| **DSR** | Daily Sales Report — order placed, distributor delivers later |
| **DSD** | Direct Store Delivery — salesperson delivers from vehicle stock |
| **OSA** | On-Shelf Availability audit |
| **OOS** | Out-of-Stock report |
| **POSM** | Point of Sale Materials audit |
| **ODS/TRAIDA** | External system of record (another team) |
| **NPP** | Nhà phân phối (distributor) |
| **Delta Sync** | Sync only data changed since a timestamp |
| **Presigned URL** | Temporary signed S3 URL for direct upload/download |
| **PKCE** | Proof Key for Code Exchange (OAuth security for mobile/WASM) |
| **Shell app** | `Hano.*` in `src/` — hosts ABP infrastructure, wires modules together |
| **Core module** | `Hano.Core.*` in `core/` — the business DDD module |
