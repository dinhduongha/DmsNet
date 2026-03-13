# EF Core Migrations

## Core module migrations (most common)
```bash
dotnet ef migrations add <Name> \
  -p core/src/Hano.Core.EntityFrameworkCore \
  -s src/Hano.HttpApi.Host

dotnet ef database update \
  -p core/src/Hano.Core.EntityFrameworkCore \
  -s src/Hano.HttpApi.Host
```

## Shell migrations (Identity, OpenIddict, etc.)
```bash
dotnet ef migrations add <Name> \
  -p src/Hano.EntityFrameworkCore \
  -s src/Hano.HttpApi.Host

dotnet ef database update \
  -p src/Hano.EntityFrameworkCore \
  -s src/Hano.HttpApi.Host
```

## Rules
- Always build successfully before creating a migration
- Migration name should be descriptive: `Added_Xxx_Entity`, `Updated_Xxx_AddedYyyField`
- After adding entity to DbContext, configure it in the `OnModelCreating` method using `builder.ConfigureCore()`
- Never manually edit migration files unless fixing a known issue
- Run `dotnet ef database update` or `dotnet run --project src/Hano.DbMigrator` to apply
