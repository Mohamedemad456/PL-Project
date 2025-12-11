<!-- Role: Documentation Lead -->
<!-- Developer: Omar Salama -->
# Project Architecture & Structure Guide

## Overview

This is a hybrid F#/C# ASP.NET Core Minimal API project that uses:
- **F#** for the main application, business logic, and API handlers
- **C#** for the data layer (Entity Framework Core) to enable migrations

This architecture allows us to leverage F#'s strengths for application logic while using C# for EF Core migrations (which don't support F#).

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    HTTP Request                             │
│              (GET /api/users, POST /api/products, etc.)     │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│              BackEnd (F# Web API Project)                  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Program.fs                                           │  │
│  │  - Application entry point                          │  │
│  │  - Service configuration (DI, EF, CORS, Swagger)     │  │
│  │  - Endpoint registration                             │  │
│  └──────────────────────────────────────────────────────┘  │
│                       │                                      │
│                       ▼                                      │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Handlers/Handlers.fs                                 │  │
│  │  - Business logic                                     │  │
│  │  - Request/Response handling                          │  │
│  │  - JSON serialization                                 │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       │ Uses DbContext
                       ▼
┌─────────────────────────────────────────────────────────────┐
│         BackEnd.Data (C# Class Library)                    │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  AppDbContext.cs                                      │  │
│  │  - Entity Framework DbContext                          │  │
│  │  - Database configuration                              │  │
│  │  - Entity relationships                                │  │
│  └──────────────────────────────────────────────────────┘  │
│                       │                                      │
│                       ▼                                      │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Models/                                               │  │
│  │  - User.cs (C# entity)                                │  │
│  │  - Product.cs (C# entity)                             │  │
│  └──────────────────────────────────────────────────────┘  │
│                       │                                      │
│                       ▼                                      │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Migrations/                                           │  │
│  │  - Database schema migrations (auto-generated)        │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│              MS SQL Server Database                         │
│              (LibraryDB)                                     │
└─────────────────────────────────────────────────────────────┘
```

## Project Structure

```
Pl-3/
│
├── BackEnd/                          # F# Web API Project
│   ├── Handlers/
│   │   └── Handlers.fs              # API endpoint handlers (F#)
│   │
│   ├── Properties/
│   │   └── launchSettings.json       # Development launch configuration
│   │
│   ├── Program.fs                    # Application entry point (F#)
│   ├── appsettings.json              # Configuration (connection strings, etc.)
│   ├── appsettings.Development.json  # Development-specific config
│   ├── BackEnd.fsproj                # F# project file
│   ├── README.md                     # Project documentation
│   ├── ARCHITECTURE.md               # This file
│   └── MIGRATION_GUIDE.md            # Migration instructions
│
└── BackEnd.Data/                     # C# Class Library (Data Layer)
    ├── Models/
    │   ├── User.cs                   # User entity (C#)
    │   └── Product.cs                # Product entity (C#)
    │
    ├── Migrations/
    │   ├── [timestamp]_InitialCreate.cs
    │   ├── [timestamp]_InitialCreate.Designer.cs
    │   └── AppDbContextModelSnapshot.cs
    │
    ├── AppDbContext.cs               # EF Core DbContext (C#)
    └── BackEnd.Data.csproj           # C# project file
```

## File Locations & Responsibilities

### BackEnd (F# Project)

#### `Program.fs`
**Location:** `BackEnd/Program.fs`  
**Language:** F#  
**Purpose:** Application entry point and configuration

**Responsibilities:**
- Creates and configures the web application builder
- Registers services (EF Core, Swagger, CORS)
- Configures the HTTP request pipeline
- Maps API endpoints to handlers
- Starts the application

**Key Sections:**
```fsharp
// Service registration
builder.Services.AddDbContext<AppDbContext>(...)
builder.Services.AddSwaggerGen()
builder.Services.AddCors(...)

// Endpoint mapping
app.MapGet("/api/users", ...)
app.MapPost("/api/products", ...)
```

#### `Handlers/Handlers.fs`
**Location:** `BackEnd/Handlers/Handlers.fs`  
**Language:** F#  
**Purpose:** Business logic and request/response handling

**Responsibilities:**
- Handles HTTP requests
- Interacts with the database through DbContext
- Performs business logic operations
- Serializes responses to JSON
- Returns appropriate HTTP status codes

**Key Functions:**
- `getUsers` - Retrieves all users
- `getUserById` - Retrieves a user by ID
- `createUser` - Creates a new user
- `getProducts` - Retrieves all products
- `getProductById` - Retrieves a product by ID
- `createProduct` - Creates a new product

#### `appsettings.json`
**Location:** `BackEnd/appsettings.json`  
**Purpose:** Application configuration

**Contains:**
- Connection strings for SQL Server
- Logging configuration
- CORS settings
- Environment-specific settings

### BackEnd.Data (C# Project)

#### `AppDbContext.cs`
**Location:** `BackEnd.Data/AppDbContext.cs`  
**Language:** C#  
**Purpose:** Entity Framework Core database context

**Responsibilities:**
- Defines DbSet properties for each entity
- Configures entity relationships and constraints
- Maps entities to database tables
- Handles database operations

**Key Components:**
```csharp
public DbSet<User> Users { get; set; }
public DbSet<Product> Products { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Entity configuration
}
```

#### `Models/User.cs`
**Location:** `BackEnd.Data/Models/User.cs`  
**Language:** C#  
**Purpose:** User entity definition

**Properties:**
- `Id` (int, primary key)
- `Name` (string, required, max 100 chars)
- `Email` (string, required, max 255 chars)
- `CreatedAt` (DateTime)

#### `Models/Product.cs`
**Location:** `BackEnd.Data/Models/Product.cs`  
**Language:** C#  
**Purpose:** Product entity definition

**Properties:**
- `Id` (int, primary key)
- `Name` (string, required, max 200 chars)
- `Description` (string, nullable)
- `Price` (decimal, precision 18,2)
- `CreatedAt` (DateTime)

#### `Migrations/`
**Location:** `BackEnd.Data/Migrations/`  
**Language:** C# (auto-generated)  
**Purpose:** Database schema migrations

**Files:**
- `[timestamp]_[MigrationName].cs` - Migration code
- `[timestamp]_[MigrationName].Designer.cs` - Migration metadata
- `AppDbContextModelSnapshot.cs` - Current model snapshot

## Request Flow

### Example: GET /api/users

```
1. HTTP Request arrives
   └─> GET https://localhost:5001/api/users

2. Program.fs routes to handler
   └─> app.MapGet("/api/users", Handlers.getUsers)

3. Handlers.fs processes request
   └─> getUsers(db: AppDbContext)
       ├─> Queries database: db.Users.ToListAsync()
       ├─> Serializes to JSON
       └─> Returns Results.Json(users)

4. AppDbContext (C#) executes query
   └─> Translates LINQ to SQL
       └─> SELECT * FROM Users

5. SQL Server returns data
   └─> Returns User entities

6. Response sent to client
   └─> JSON array of users
```

### Example: POST /api/users

```
1. HTTP Request arrives
   └─> POST https://localhost:5001/api/users
   └─> Body: { "name": "John", "email": "john@example.com" }

2. Program.fs routes to handler
   └─> app.MapPost("/api/users", Handlers.createUser)

3. Handlers.fs processes request
   └─> createUser(db: AppDbContext, user: User)
       ├─> Sets CreatedAt = DateTime.UtcNow
       ├─> Adds to context: db.Users.Add(user)
       ├─> Saves changes: db.SaveChangesAsync()
       └─> Returns Results.Created(...)

4. AppDbContext (C#) tracks entity
   └─> EF Core change tracking

5. SQL Server inserts record
   └─> INSERT INTO Users (Name, Email, CreatedAt) VALUES (...)

6. Response sent to client
   └─> 201 Created with location header
```

## Technology Stack

### Backend Framework
- **ASP.NET Core 8.0** - Web framework
- **Minimal APIs** - Lightweight API endpoints
- **F#** - Primary application language

### Data Access
- **Entity Framework Core 8.0** - ORM
- **MS SQL Server** - Database
- **C#** - Data layer language (for migrations)

### Additional Libraries
- **Swashbuckle.AspNetCore** - Swagger/OpenAPI documentation
- **FSharp.SystemTextJson** - F# JSON serialization support
- **Microsoft.AspNetCore.OpenApi** - OpenAPI support

## Design Patterns

### 1. **Layered Architecture**
- **Presentation Layer:** `Handlers/Handlers.fs` (F#)
- **Data Access Layer:** `BackEnd.Data` (C#)
- **Database Layer:** SQL Server

### 2. **Dependency Injection**
- Services registered in `Program.fs`
- DbContext injected into handlers
- Configuration injected from `appsettings.json`

### 3. **Repository Pattern (Implicit)**
- DbContext acts as repository
- Handlers use DbContext directly
- Can be extended with explicit repositories if needed

### 4. **Minimal API Pattern**
- Endpoints defined directly in `Program.fs`
- No controllers needed
- Lightweight and fast

## Why This Architecture?

### Why F# for Application Layer?
- **Type Safety:** Strong type system prevents many bugs
- **Immutability:** Default immutability reduces side effects
- **Pattern Matching:** Powerful for handling different cases
- **Functional Style:** Clean, expressive code
- **Domain Modeling:** Great for business logic

### Why C# for Data Layer?
- **EF Core Support:** Migrations only work with C#
- **Tooling:** Better IDE support for EF Core in C#
- **Ecosystem:** More examples and documentation
- **Compatibility:** Seamless integration with EF Core

### Benefits of Hybrid Approach
- ✅ Best of both worlds
- ✅ EF Core migrations work perfectly
- ✅ F# for business logic where it shines
- ✅ C# only where necessary (data layer)
- ✅ Clean separation of concerns

## Development Workflow

### Adding a New Feature

1. **Define Entity (C#)**
   - Create model in `BackEnd.Data/Models/`
   - Add DbSet to `AppDbContext.cs`
   - Configure in `OnModelCreating` if needed

2. **Create Migration (C#)**
   ```bash
   cd BackEnd.Data
   dotnet ef migrations add AddNewEntity --startup-project ..\BackEnd\BackEnd.fsproj
   dotnet ef database update --startup-project ..\BackEnd\BackEnd.fsproj
   ```

3. **Create Handler (F#)**
   - Add handler functions in `Handlers/Handlers.fs`
   - Use DbContext to interact with database

4. **Register Endpoint (F#)**
   - Map endpoint in `Program.fs`
   - Connect to handler function

5. **Test**
   - Run application: `dotnet run`
   - Test via Swagger UI or Postman

## Configuration

### Connection String
Located in `BackEnd/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=LibraryDB;..."
  }
}
```

### Environment-Specific Settings
- **Development:** `appsettings.Development.json`
- **Production:** `appsettings.Production.json` (create as needed)

### Launch Settings
Located in `BackEnd/Properties/launchSettings.json`:
- HTTP/HTTPS ports
- Environment variables
- Launch profiles

## Key Concepts

### Dependency Injection
Services are registered in `Program.fs` and automatically injected:
```fsharp
builder.Services.AddDbContext<AppDbContext>(...)
// DbContext is now available in handlers
```

### Task-Based Async
All database operations use `task { }` computation expression:
```fsharp
let getUsers (db: AppDbContext) =
    task {
        let! users = db.Users.ToListAsync()
        return Results.Json(users)
    }
```

### JSON Serialization
Configured with camelCase naming:
```fsharp
let jsonOptions = 
    let opts = JsonSerializerOptions()
    opts.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
    opts
```

## Common Tasks

### Adding a New Endpoint
1. Add handler function in `Handlers/Handlers.fs`
2. Map endpoint in `Program.fs`
3. Test via Swagger or Postman

### Modifying Database Schema
1. Update model in `BackEnd.Data/Models/`
2. Create migration: `dotnet ef migrations add ...`
3. Apply migration: `dotnet ef database update ...`

### Adding a New Service
1. Register in `Program.fs`: `builder.Services.Add...`
2. Inject where needed (handlers, etc.)

## Best Practices

1. **Keep C# minimal:** Only use for data layer
2. **F# for logic:** All business logic in F#
3. **Type safety:** Leverage F#'s type system
4. **Immutable by default:** Use F# immutability
5. **Async everywhere:** Use `task { }` for I/O
6. **Error handling:** Use F# Result types or exceptions appropriately
7. **Configuration:** Keep in `appsettings.json`
8. **Migrations:** Always review before applying

## Troubleshooting

### Build Errors
- Check file compilation order in `.fsproj`
- Ensure C# project builds first
- Verify project references

### Migration Issues
- Ensure connection string is correct
- Check SQL Server is running
- Verify EF Core tools are installed

### Runtime Errors
- Check connection string
- Verify database exists
- Check entity configurations

## Next Steps

- Review `README.md` for setup instructions
- Check `MIGRATION_GUIDE.md` for database changes
- Explore Swagger UI at `/swagger` when running
- Add more entities and endpoints as needed

