# BackEnd - F# ASP.NET Core Minimal API

This is an F# backend project using ASP.NET Core Minimal APIs with Entity Framework Core and MS SQL Server.

## Architecture

The project uses a hybrid approach:
- **F#** for the main application, handlers, and business logic
- **C#** for the data layer (DbContext and Models) to enable EF Core migrations

This workaround allows us to use EF Core migrations (which don't support F#) while keeping the rest of the application in F#.

📖 **For detailed architecture and project structure, see [ARCHITECTURE.md](./ARCHITECTURE.md)**

## Prerequisites

- .NET 8.0 SDK or later
- MS SQL Server (local or remote)
- Visual Studio 2022, VS Code with Ionide, or Rider
- Entity Framework Core Tools: `dotnet tool install --global dotnet-ef`

## Setup

1. **Update Connection String**

   Edit `appsettings.json` or `appsettings.Development.json` with your SQL Server connection details:

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=DESKTOP-6FLDD19\\SQLEXPRESS14;Database=LibraryDB;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```

2. **Create Database and Run Migrations**

   From the `BackEnd.Data` project directory:

   ```bash
   cd BackEnd.Data
   dotnet ef database update --startup-project ..\BackEnd\BackEnd.fsproj
   ```

   Or from the solution root:

   ```bash
   dotnet ef database update --project BackEnd.Data --startup-project BackEnd
   ```

3. **Run the Application**

   ```bash
   cd BackEnd
   dotnet run
   ```

   The API will be available at:
   - HTTP: `http://localhost:5000`
   - HTTPS: `https://localhost:5001`
   - Swagger UI: `https://localhost:5001/swagger`

## API Endpoints

### Users
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create a new user

### Products
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create a new product

### Health
- `GET /health` - Health check endpoint

## Project Structure

```
BackEnd/
├── Handlers/
│   └── Handlers.fs            # API endpoint handlers (F#)
├── Program.fs                 # Application entry point and configuration (F#)
├── appsettings.json          # Configuration file
└── BackEnd.fsproj            # F# project file

BackEnd.Data/                 # C# class library for data layer
├── Models/
│   ├── User.cs               # User entity (C#)
│   └── Product.cs            # Product entity (C#)
├── Migrations/               # EF Core migrations (auto-generated)
│   └── ...
├── AppDbContext.cs           # Entity Framework DbContext (C#)
└── BackEnd.Data.csproj       # C# project file
```

## Development

### Adding New Models

1. Add the model class to `BackEnd.Data/Models/` (C#)
2. Add a `DbSet` property to `AppDbContext` in `BackEnd.Data/AppDbContext.cs`
3. Configure the entity in `OnModelCreating` if needed
4. Create handlers in `BackEnd/Handlers/Handlers.fs` (F#)
5. Register endpoints in `BackEnd/Program.fs` (F#)
6. Create and run migrations (see below)

### Running Migrations

All migrations are managed in the `BackEnd.Data` project:

```bash
# Navigate to the data project
cd BackEnd.Data

# Add a new migration
dotnet ef migrations add MigrationName --startup-project ..\BackEnd\BackEnd.fsproj

# Update database
dotnet ef database update --startup-project ..\BackEnd\BackEnd.fsproj

# Remove last migration (if not applied)
dotnet ef migrations remove --startup-project ..\BackEnd\BackEnd.fsproj
```

### Building the Solution

```bash
# From solution root
dotnet build

# Or build individual projects
dotnet build BackEnd
dotnet build BackEnd.Data
```

## Notes

- The data layer (DbContext and Models) is in C# to enable EF Core migrations
- The application layer (handlers, endpoints) remains in F#
- JSON serialization uses camelCase naming
- CORS is enabled for all origins (configure appropriately for production)
- Swagger/OpenAPI is enabled in development mode
- The F# project references the C# data project
