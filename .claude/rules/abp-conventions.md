# ABP Framework Conventions

When creating or modifying code in this project:

## Entities
- Aggregate roots: inherit `FullAuditedAggregateRoot<Guid>`
- Child entities: inherit `AuditedEntity<Guid>` or `Entity<Guid>`
- Primary keys are `Guid` (UUIDv7, auto-generated via `GuidGenerator.Create()`)
- Use `[Table("snake_case_name")]` for table names
- Use `[Column("snake_case")]` for column names
- Use parameterized constructors with `Check.NotNullOrWhiteSpace()` for required fields
- Include `SyncStatus` property for entities synced with ODS

## AppServices
- Inherit from `HanoCoreAppServiceBase` (in Hano.Core.Application)
- Implement `I{Name}AppService` interface (in Application.Contracts)
- Keep thin — delegate complex logic to Domain Services
- Always use `[Authorize(CorePermissions.Xxx)]` for permission checks
- Use `CurrentUserId` from base class (not `CurrentUser.Id`)
- Return DTOs, never entities
- Implement idempotency checks for create operations

## Controllers
- Inherit from `HanoCoreController` (NOT auto-generated)
- Use `[Route("api/v1/{resource}")]` with explicit routing
- Use `[Authorize]` at class level
- Inject AppService interfaces, delegate all logic
- Manual response wrapping: `Ok(new { ... })`

## DTOs
- Input DTOs: `CreateXxxInput`, `UpdateXxxInput`, `XxxInputDto`
- Output DTOs: `XxxDto`, `XxxListDto`
- Filter DTOs: `XxxFilterDto` inheriting `PagedAndSortedResultRequestDto`
- Validate inputs with FluentValidation validators
- Place in Hano.Core.Application.Contracts

## Mapping (Riok.Mapperly — NOT AutoMapper)
- Create `[Mapper]` static partial classes in Application project
- Use extension methods: `public static partial XxxDto ToDto(this Xxx source)`
- Place in `Mappers/` folder
- Mix generated and manual mappings as needed

## Entity Configuration (EF Core)
- Implement `IEntityTypeConfiguration<T>` in EntityFrameworkCore project
- Use `b.ToTable("snake_case")` for table names
- Always call `b.ConfigureByConvention()` (ABP convention)
- Place in `EntityConfigurations/` folder

## Repositories
- Define interfaces in Domain: `IXxxRepository : IRepository<Xxx, Guid>`
- Implement in EntityFrameworkCore project
- Use `IQueryable` via `GetQueryableAsync()`, avoid raw SQL

## Module separation
- Business logic → `core/src/Hano.Core.*`
- Infrastructure/ABP wiring → `src/Hano.*`
- Never reference Shell from Core module
