<!-- Role: Documentation Lead -->
<!-- Developer: Omar Salama -->
# Dependency Injection Explained

## Where Does the `db` Object Come From?

The `db` parameter (of type `AppDbContext`) is **automatically injected** by ASP.NET Core's Dependency Injection (DI) system. Here's how it works:

## Step-by-Step Flow

### 1. **Registration** (Program.fs - Line 21-24)

```fsharp
builder.Services.AddDbContext<AppDbContext>(fun options ->
    let connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    options.UseSqlServer(connectionString) |> ignore
) |> ignore
```

**What happens here:**
- Registers `AppDbContext` in the DI container
- Configures it to use SQL Server
- Gets the connection string from `appsettings.json`
- Sets the lifetime (scoped by default - one per HTTP request)

### 2. **Connection String** (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=DESKTOP-6FLDD19\\SQLEXPRESS14;Database=LibraryDB;..."
  }
}
```

The connection string is read from configuration and used to configure the DbContext.

### 3. **Endpoint Registration** (Program.fs - Line 45-47)

```fsharp
app.MapGet("/api/users", 
    System.Func<AppDbContext, System.Threading.Tasks.Task<IResult>>(
        fun db -> Handlers.getUsers db))
```

**What happens here:**
- `MapGet` sees `AppDbContext` as a parameter type
- ASP.NET Core's DI system automatically resolves it
- Creates a new `AppDbContext` instance (scoped to the request)
- Passes it to your handler function as `db`

### 4. **Handler Receives It** (Handlers.fs - Line 21)

```fsharp
let getUsers (db: AppDbContext) =
    task {
        let! users = db.Users.ToListAsync()
        // ... use db here
    }
```

The `db` parameter is automatically provided by the framework!

## Visual Flow

```
┌─────────────────────────────────────────────────────────┐
│  1. Application Startup (Program.fs)                    │
│                                                           │
│  builder.Services.AddDbContext<AppDbContext>(...)       │
│  └─> Registers AppDbContext in DI Container              │
│      └─> Reads connection string from appsettings.json   │
│          └─> Configures SQL Server connection            │
└─────────────────────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│  2. HTTP Request Arrives                                 │
│     GET /api/users                                       │
└─────────────────────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│  3. Minimal API Routing (Program.fs)                     │
│                                                           │
│  app.MapGet("/api/users", fun db -> ...)                 │
│  └─> ASP.NET Core sees AppDbContext parameter           │
│      └─> Looks up in DI Container                        │
│          └─> Creates new AppDbContext instance         │
│              └─> Opens database connection              │
└─────────────────────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│  4. Handler Execution (Handlers.fs)                      │
│                                                           │
│  let getUsers (db: AppDbContext) =                       │
│      // db is already provided here!                     │
│      db.Users.ToListAsync()                              │
└─────────────────────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│  5. Request Completes                                    │
│                                                           │
│  └─> AppDbContext is disposed (scoped lifetime)          │
│      └─> Database connection is closed                    │
└─────────────────────────────────────────────────────────┘
```

## Key Concepts

### Dependency Injection (DI)

ASP.NET Core has a built-in DI container that:
- **Resolves** dependencies automatically
- **Manages lifetimes** (when to create/dispose)
- **Injects** dependencies into your functions

### Scoped Lifetime

`AddDbContext` registers `AppDbContext` with a **scoped lifetime**, which means:
- ✅ One instance per HTTP request
- ✅ Automatically disposed when request completes
- ✅ Thread-safe (each request gets its own instance)
- ✅ Database connection is properly closed

### Automatic Resolution

In Minimal APIs, when you have a parameter in your handler:
```fsharp
fun db -> Handlers.getUsers db
```

ASP.NET Core automatically:
1. Checks the parameter type (`AppDbContext`)
2. Looks it up in the DI container
3. Creates/resolves an instance
4. Passes it to your function

## What You DON'T Need to Do

❌ **You don't manually create AppDbContext:**
```fsharp
// ❌ DON'T DO THIS
let db = new AppDbContext(...)  // Wrong!
```

❌ **You don't pass it explicitly:**
```fsharp
// ❌ DON'T DO THIS
let db = getDbContext()
Handlers.getUsers(db)  // Wrong!
```

✅ **You just declare it as a parameter:**
```fsharp
// ✅ DO THIS - Framework handles it!
let getUsers (db: AppDbContext) =  // db is automatically provided
    // use db here
```

## Other Services You Can Inject

You can inject **any registered service** the same way:

### Example: Injecting ILogger

**1. Register it (if not already):**
```fsharp
// Usually already registered, but you can configure it:
builder.Services.AddLogging()
```

**2. Use it in handler:**
```fsharp
let getUsers (db: AppDbContext) (logger: ILogger<Handlers>) =
    logger.LogInformation("Getting users")
    // ... rest of code
```

**3. Map endpoint:**
```fsharp
app.MapGet("/api/users", 
    fun (db: AppDbContext) (logger: ILogger<Handlers>) -> 
        Handlers.getUsers db logger)
```

### Example: Injecting IConfiguration

```fsharp
let getUsers (db: AppDbContext) (config: IConfiguration) =
    let maxUsers = config.GetValue<int>("MaxUsers", 100)
    // ... use config
```

## How to See What's Registered

You can check what's registered in the DI container:

```fsharp
// In Program.fs, after builder.Build()
let app = builder.Build()

// Log all registered services (for debugging)
let serviceProvider = app.Services
for service in serviceProvider.GetServices<AppDbContext>() do
    printfn "Service: %A" service
```

## Common Questions

### Q: Can I inject multiple services?
**A:** Yes! Just add more parameters:
```fsharp
let getUsers (db: AppDbContext) (logger: ILogger) (config: IConfiguration) =
    // All are automatically injected
```

### Q: What if the service isn't registered?
**A:** You'll get a runtime error:
```
System.InvalidOperationException: Unable to resolve service for type 'AppDbContext'
```

### Q: Can I use a different lifetime?
**A:** Yes, but for DbContext, scoped is recommended:
```fsharp
// Scoped (default - one per request) ✅ Recommended
builder.Services.AddDbContext<AppDbContext>(...)

// Singleton (one for entire app) ❌ Not recommended for DbContext
builder.Services.AddSingleton<AppDbContext>(...)

// Transient (new instance every time) ❌ Not recommended for DbContext
builder.Services.AddTransient<AppDbContext>(...)
```

### Q: How does it know which connection string?
**A:** From `appsettings.json`:
```fsharp
builder.Configuration.GetConnectionString("DefaultConnection")
// Reads from: "ConnectionStrings": { "DefaultConnection": "..." }
```

## Summary

The `db` object comes from:

1. **Registration**: `AddDbContext<AppDbContext>()` in `Program.fs`
2. **Configuration**: Connection string from `appsettings.json`
3. **Automatic Injection**: ASP.NET Core DI resolves it automatically
4. **Scoped Lifetime**: One instance per HTTP request
5. **Automatic Disposal**: Disposed when request completes

**You don't create it manually - the framework does it for you!** 🎉

