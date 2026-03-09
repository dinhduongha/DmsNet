# Testing Rules

## Run tests
```bash
dotnet test                                   # All tests
dotnet test --filter "FullyQualifiedName~ClassName"  # Specific class
dotnet test --collect:"XPlat Code Coverage"   # With coverage
```

## Conventions
- Test naming: `MethodName_Scenario_ExpectedResult`
- Unit tests: xUnit + NSubstitute for mocking
- Integration tests: TestContainers with PostgreSQL
- Target: ≥ 70% code coverage

## Test project mapping
| Source project | Test project |
|---|---|
| Hano.Core.Domain | Hano.Domain.Tests |
| Hano.Core.Application | Hano.Application.Tests |
| Hano.Core.EntityFrameworkCore | Hano.EntityFrameworkCore.Tests |

## Always
- Run `dotnet test` before suggesting a commit
- Write tests for new AppServices and Domain Services
- Mock external dependencies (ODS, S3, Redis)
