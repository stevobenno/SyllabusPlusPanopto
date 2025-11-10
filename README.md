
# SyllabusPlusPanopto

This scaffold matches the structure we discussed.

## Projects
- SyllabusPlusPanopto.Shared (common DI helpers)
- SyllabusPlusPanopto.Domain (entities, hashing)
- SyllabusPlusPanopto.Transform (AutoMapper mapping)
- SyllabusPlusPanopto.Infrastructure (interfaces for I/O)
- SyllabusPlus.ReadApi.Functions (HTTP read model)
- SyllabusPlus.ReadApi.Console (console host)
- SyllabusPlus.Sync.Functions (transform + sync endpoints)
- SyllabusPlus.Sync.Console (console host)
- SyllabusPlus.Notifications.Functions (timer for notifications)
- SyllabusPlus.Notifications.Console (console host)
- SyllabusPlus.Tests.Unit (xUnit)
- SyllabusPlus.Tests.Int (xUnit)

## Next steps
1. Open the folder in VS/VS Code.
2. Create a real .sln: `dotnet new sln -n SyllabusPlusPanopto` then `dotnet sln add **/*.csproj`
3. Restore & build: `dotnet restore && dotnet build`
4. For Functions, copy `local.settings.sample.json` to `local.settings.json` and fill secrets.
