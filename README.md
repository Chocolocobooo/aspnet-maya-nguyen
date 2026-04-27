# CoreFitness Club

ASP.NET Core 8 MVC web application for a fitness club, built with Clean Architecture, DDD, and ASP.NET Identity.

## Architecture

```
CoreFitnessClub/
├── CoreFitnessClub.Domain/         # Entities, domain interfaces
├── CoreFitnessClub.Application/    # Services, DTOs, application interfaces
├── CoreFitnessClub.Infrastructure/ # EF Core DbContext, repository implementations
├── CoreFitnessClub.Web/            # MVC Controllers, Views, Partial Views
└── CoreFitnessClub.Tests/          # xUnit unit tests (Moq + FluentAssertions)
```

## Design Patterns Used
- **Repository Pattern** — abstracts data access behind interfaces
- **Service Pattern** — business logic in application services
- **Clean Architecture** — domain has no external dependencies
- **Dependency Injection** — all services registered in Program.cs

## Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB is fine — included with Visual Studio)
- Visual Studio 2022

## Getting Started

### 1. Clone / open the solution
Open `CoreFitnessClub.sln` in Visual Studio 2026.

### 2. Set up the database
The connection string in `CoreFitnessClub.Web/appsettings.json` uses LocalDB by default:
```
Server=(localdb)\mssqllocaldb;Database=CoreFitnessClub;Trusted_Connection=True
```
Change this if you use a different SQL Server instance.

### 3. Apply migrations
In the **Package Manager Console**, with `CoreFitnessClub.Web` as the startup project:
```powershell
# First migration (creates all tables + seeds data)
Add-Migration InitialCreate -Project CoreFitnessClub.Infrastructure -StartupProject CoreFitnessClub.Web
Update-Database -Project CoreFitnessClub.Infrastructure -StartupProject CoreFitnessClub.Web
```
Or via CLI:
```bash
cd CoreFitnessClub.Web
dotnet ef migrations add InitialCreate --project ../CoreFitnessClub.Infrastructure
dotnet ef database update --project ../CoreFitnessClub.Infrastructure
```

### 4. Run
Press **F5** in Visual Studio, or:
```bash
cd CoreFitnessClub.Web
dotnet run
```

### 5. Run Tests
```bash
cd CoreFitnessClub.Tests
dotnet test
```
- ✅ EF Core Code First with migrations + seed data
- ✅ Repository pattern with generic base
- ✅ 20 unit tests covering services and DTO validation
