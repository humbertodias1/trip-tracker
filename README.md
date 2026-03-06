# TripTracker

TripTracker is an ASP.NET Core 8 MVC web app for tracking trips, itinerary items, expenses, packing lists, and document links.  
Each authenticated user sees only their own trips.

## Tech Stack

- C# + ASP.NET Core 8 MVC
- Entity Framework Core
- MySQL 8 (Pomelo EF Core provider)
- ASP.NET Core Identity (email/password)
- Bootstrap 5

## Features

- Authentication (Register/Login/Logout)
- Trips CRUD
- Trip dashboard with tabs:
  - Itinerary: day-by-day grouping, filters, "What's next?"
  - Expenses: add/edit/delete, totals by category and total spend
  - Expenses: rough auto-currency conversion using static rates (not live exchange rates)
  - Packing: add/edit/delete, toggle packed, template apply, completion %
  - Documents: URL links with title/notes
  - Settings: edit trip details
- Seeded packing templates:
  - Carry-on
  - International
  - Winter
  - Beach
- Optional demo user + sample trip seeded automatically

## Prerequisites

- .NET 8 SDK
- MySQL Server 8.x (MySQL Workbench is optional client tooling)
- (Optional) EF Core CLI:
  - `dotnet tool install --global dotnet-ef`

## Setup

1. Restore dependencies:

```bash
dotnet restore
```

2. Configure local secrets (recommended for DB password):

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=127.0.0.1;Port=3306;Database=triptracker;User=root;Password=YOUR_PASSWORD;"
```

3. Apply migrations (creates database/tables if needed):

```bash
dotnet ef database update
```

4. Run app:

```bash
dotnet run
```

## MySQL Connection

`appsettings.json` contains a safe base connection string (no password):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=127.0.0.1;Port=3306;Database=triptracker;User=root;"
}
```

The full connection string (with password) should be stored in user-secrets for local development.

## Migrations Workflow

When models change:

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## Default URLs

- `http://localhost:5234`
- `https://localhost:7095`

## Demo Account (Seeded)

- Email: `demo@triptracker.local`
- Password: `Demo123!`

## Currency Conversion Notes

- Conversion is intentionally rough and static (no external API).
- Supported currencies currently include: `USD, EUR, GBP, CAD, MXN, JPY, BRL, AUD, CHF, CNY, INR`.
- Dropdown labels show approximate value relative to USD.

## Project Structure

- `Data/` - DbContext and seeding
- `Migrations/` - EF Core migrations
- `Models/` - domain entities and enums
- `Services/` - helper services (e.g., currency estimator)
- `ViewModels/` - UI view models
- `Controllers/` - MVC controllers
- `Views/` - Razor views
- `wwwroot/` - static files

## Notes

- Server-side ownership checks are enforced for every trip-related read/write action.
- Trip date validation enforces `StartDate <= EndDate`.
