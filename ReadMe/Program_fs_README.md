<!-- Role: Documentation Lead -->
<!-- Developer: Omar Salama -->
# Program.fs - Complete Code Explanation

This document explains every block of code in `Program.fs`, the main entry point of our F# web application.

## Table of Contents
1. [Namespace and Imports]
2. [Module Declaration]
3. [WebApplication Builder Setup]
4. [JSON Serialization Configuration]
5. [Service Registration]
6. [Database Configuration]
7. [CORS Configuration]
8. [Application Building]
9. [Middleware Pipeline]
10. [Endpoint Definitions]
11. [Application Execution]

---

## 1. Namespace and Imports

```fsharp
namespace BackEnd

open System
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.EntityFrameworkCore
open BackEnd.Data
open BackEnd.Handlers
open Microsoft.AspNetCore.Http
open Microsoft.OpenApi.Models
```

**What it does:**
- `namespace BackEnd` - Declares this file belongs to the `BackEnd` namespace
- `open` statements - Import namespaces/modules so we can use their types and functions without fully qualifying them

**Why we use it:**
- Organizes code into logical groups
- Reduces verbosity (can write `WebApplication` instead of `Microsoft.AspNetCore.Builder.WebApplication`)

---

## 2. Module Declaration

```fsharp
module Program =
```

**What it does:**
- Declares a module named `Program` that contains all our application setup code

**Why we use it:**
- Groups related code together
- Makes functions accessible from other modules via `Program.functionName`

---

## 3. WebApplication Builder Setup

```fsharp
let builder = WebApplication.CreateBuilder()
```

**What it does:**
- Creates a `WebApplicationBuilder` instance
- This is the modern .NET way to configure ASP.NET Core applications (replaces `Startup.cs`)

**Why we use it:**
- Provides a fluent API for configuring services, middleware, and endpoints
- Simplifies application setup compared to older ASP.NET Core patterns

**What it contains:**
- Configuration system (appsettings.json, environment variables)
- Dependency injection container
- Logging infrastructure
- Hosting environment information

---

## 4. JSON Serialization Configuration

```fsharp
builder.Services.ConfigureHttpJsonOptions(fun options ->
    options.SerializerOptions.ReferenceHandler <- ReferenceHandler.IgnoreCycles
    options.SerializerOptions.WriteIndented <- true
) |> ignore
```

**What it does:**
- Configures how JSON is serialized/deserialized for HTTP requests and responses

**Breaking it down:**
- `ConfigureHttpJsonOptions` - Registers a configuration action for JSON options
- `fun options -> ...` - Lambda function that receives the options object
- `ReferenceHandler.IgnoreCycles` - Prevents infinite loops when serializing objects with circular references (e.g., User -> Borrowing -> Book -> Borrowing)
- `WriteIndented <- true` - Makes JSON output pretty-printed (formatted with indentation) for easier debugging
- `|> ignore` - Discards the return value (we don't need it)

**Why we use it:**
- **IgnoreCycles**: Entity Framework creates navigation properties that can reference each other. Without this, serialization would fail or loop infinitely.
- **WriteIndented**: Makes API responses human-readable during development

**Example problem it solves:**
```fsharp
// Without IgnoreCycles, this would cause an error:
// User -> Borrowings -> Book -> Borrowings -> User -> ...
```

---

## 5. Service Registration

### 5.1 Swagger/OpenAPI

```fsharp
builder.Services.AddEndpointsApiExplorer() |> ignore
builder.Services.AddSwaggerGen(fun options ->
    options.SwaggerDoc("v1", OpenApiInfo(
        Title = "Library Management API",
        Version = "v1",
        Description = "A RESTful API for managing library resources, users, and authentication",
        Contact = OpenApiContact(
            Name = "API Support",
            Email = "support@example.com"
        )
    ))
) |> ignore
```

**What it does:**
- `AddEndpointsApiExplorer()` - Enables API discovery for Swagger
- `AddSwaggerGen` - Configures Swagger/OpenAPI documentation generation
- `SwaggerDoc` - Defines API metadata (title, version, description, contact info)

**Why we use it:**
- Automatically generates interactive API documentation
- Allows testing endpoints directly from the browser
- Provides API contract documentation for frontend developers

**Result:** When you run the app, visit `/swagger` to see interactive API docs

---

## 6. Database Configuration

```fsharp
builder.Services.AddDbContext<AppDbContext>(fun options ->
    let connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    options.UseSqlServer(connectionString) |> ignore
) |> ignore
```

**What it does:**
- Registers `AppDbContext` with the dependency injection container
- Configures Entity Framework Core to use SQL Server
- Reads connection string from `appsettings.json` via `builder.Configuration`

**Breaking it down:**
- `AddDbContext<AppDbContext>` - Registers the database context as a scoped service
- `fun options -> ...` - Lambda that configures EF Core options
- `GetConnectionString("DefaultConnection")` - Reads connection string from config
- `UseSqlServer(connectionString)` - Tells EF Core to use SQL Server provider
- `|> ignore` - Discards return values

**Why we use it:**
- Dependency injection: Other parts of the app can request `AppDbContext` and get it automatically
- Scoped lifetime: One context per HTTP request (ensures data consistency)
- Configuration: Connection string stored in config, not hardcoded

**Where connection string comes from:**
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;..."
  }
}
```

---

## 7. CORS Configuration

```fsharp
builder.Services.AddCors(fun options ->
    options.AddDefaultPolicy(fun policy ->
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader() |> ignore
    ) |> ignore
) |> ignore
```

**What it does:**
- Configures Cross-Origin Resource Sharing (CORS)
- Allows requests from any origin, method, and header

**Breaking it down:**
- `AddCors` - Registers CORS services
- `AddDefaultPolicy` - Creates a default CORS policy
- `AllowAnyOrigin()` - Allows requests from any domain (e.g., localhost:3000, example.com)
- `AllowAnyMethod()` - Allows GET, POST, PUT, DELETE, etc.
- `AllowAnyHeader()` - Allows any HTTP headers

**Why we use it:**
- Frontend (Next.js) runs on different port (e.g., localhost:3000)
- Backend runs on different port (e.g., localhost:5000)
- Browsers block cross-origin requests by default (same-origin policy)
- CORS tells browser: "It's OK to allow this request"

**Security note:** `AllowAnyOrigin()` is permissive. In production, specify exact origins:
```fsharp
policy.WithOrigins("https://myapp.com", "https://www.myapp.com")
```

---

## 8. Application Building

```fsharp
let app = builder.Build()
```

**What it does:**
- Builds the `WebApplication` from the configured `WebApplicationBuilder`
- At this point, services are registered and the app is ready for middleware/endpoints

**Why we use it:**
- Separates configuration (builder) from runtime (app)
- Once built, you can't add more services, but you can add middleware and endpoints

---

## 9. Middleware Pipeline

```fsharp
if app.Environment.IsDevelopment() then
    app.UseSwagger() |> ignore
    app.UseSwaggerUI() |> ignore

app.UseCors() |> ignore
app.UseHttpsRedirection() |> ignore
```

**What it does:**
- Configures middleware that processes HTTP requests in order

**Breaking it down:**

**Swagger (Development only):**
- `UseSwagger()` - Serves OpenAPI JSON specification
- `UseSwaggerUI()` - Serves Swagger UI (interactive documentation)
- Only enabled in Development environment

**CORS:**
- `UseCors()` - Applies CORS policy to requests
- Must be called before endpoints that need CORS

**HTTPS Redirection:**
- `UseHttpsRedirection()` - Redirects HTTP requests to HTTPS
- Security best practice

**Middleware order matters:**
1. Swagger (if dev)
2. CORS (before endpoints)
3. HTTPS Redirection
4. Endpoints (defined next)

**Why this order:**
- CORS must be early so preflight requests work
- HTTPS redirection happens before endpoints
- Swagger is just documentation, can be anywhere

---

## 10. Endpoint Definitions

Endpoints define the API routes and their handlers. Each endpoint follows this pattern:

```fsharp
app.Map[Method]("/path", handler)
    .WithName("EndpointName")
    .WithTags("Category")
    .WithSummary("Brief description")
    .WithDescription("Detailed description")
    |> ignore
```

### 10.1 User Endpoints

#### Get All Users
```fsharp
app.MapGet("/api/users", 
    System.Func<AppDbContext, System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>>(
        fun db -> Handlers.getUsers db))
    .WithName("GetAllUsers")
    .WithTags("Users")
    .WithSummary("Get all users")
    .WithDescription("Retrieves a list of all users in the system") |> ignore
```

**What it does:**
- Maps GET requests to `/api/users`
- Handler: `Handlers.getUsers` (from `Handlers.fs`)
- Dependency injection: ASP.NET Core automatically provides `AppDbContext`

**Breaking it down:**
- `MapGet` - Creates a GET endpoint
- `System.Func<...>` - Wraps F# function in .NET delegate (required for ASP.NET Core)
- `fun db -> Handlers.getUsers db` - Lambda that calls our handler with injected `db`
- Metadata methods (`.WithName`, `.WithTags`, etc.) - Used by Swagger for documentation

**Why the System.Func wrapper:**
- ASP.NET Core expects .NET delegates
- F# functions are different from .NET delegates
- This wrapper makes them compatible

---

#### Get User By ID
```fsharp
app.MapGet("/api/users/{id:guid}", 
    System.Func<AppDbContext, System.Guid, System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>>(
        fun db id -> Handlers.getUserById db id))
    .WithName("GetUserById")
    .WithTags("Users")
    .WithSummary("Get user by ID")
    .WithDescription("Retrieves a specific user by their unique identifier") |> ignore
```

**What it does:**
- Maps GET requests to `/api/users/{id}` where `{id}` is a GUID
- Extracts `id` from URL and passes it to handler
- Example: `GET /api/users/123e4567-e89b-12d3-a456-426614174000`

**Breaking it down:**
- `{id:guid}` - Route parameter with type constraint (must be valid GUID)
- `System.Guid` in Func signature - Parameter type
- `fun db id -> ...` - Lambda receives both `db` (injected) and `id` (from route)

---

#### Create User
```fsharp
app.MapPost("/api/users", 
    System.Func<AppDbContext, BackEnd.Data.Models.User, System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>>(
        fun db user -> Handlers.createUser db user))
    .WithName("CreateUser")
    .WithTags("Users")
    .WithSummary("Create a new user")
    .WithDescription("Creates a new user in the system") |> ignore
```

**What it does:**
- Maps POST requests to `/api/users`
- Deserializes request body to `User` object
- Passes `db` and `user` to handler

**Breaking it down:**
- `MapPost` - Creates POST endpoint
- `BackEnd.Data.Models.User` - Request body type (automatically deserialized from JSON)
- ASP.NET Core handles JSON deserialization automatically

---

### 10.2 Authentication Endpoints

#### Register User
```fsharp
app.MapPost("/api/auth/register", 
    System.Func<AppDbContext, BackEnd.Data.Models.RegisterRequest, System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>>(
        fun db request -> AuthHandler.register db request.Email request.Username request.Password))
    .WithName("RegisterUser")
    .WithTags("Authentication")
    .WithSummary("Register a new user")
    .WithDescription("Registers a new user account with email, username, and password. The password will be hashed before storage.")
    .Accepts<BackEnd.Data.Models.RegisterRequest>("application/json")
    .Produces<{| id: System.Guid; email: string; name: string; message: string |}>(201)
    .Produces(400)
    .Produces(500) |> ignore
```

**What it does:**
- Maps POST to `/api/auth/register`
- Uses `AuthHandler.register` (from `AuthHandler.fs`)
- Extracts `Email`, `Username`, `Password` from `RegisterRequest` object

**Breaking it down:**
- `request.Email`, `request.Username`, `request.Password` - Accessing properties of request object
- `.Accepts<...>` - Documents what request body type is expected
- `.Produces<...>(201)` - Documents successful response (201 Created)
- `.Produces(400)`, `.Produces(500)` - Documents error responses

**Why separate AuthHandler:**
- Authentication logic is complex (validation, hashing, etc.)
- Separates concerns: user management vs authentication

---

#### Login User
```fsharp
app.MapPost("/api/auth/login", 
    System.Func<AppDbContext, BackEnd.Data.Models.LoginRequest, System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>>(
        fun db request -> AuthHandler.login db request.Email request.Password))
    .WithName("LoginUser")
    .WithTags("Authentication")
    .WithSummary("Login user")
    .WithDescription("Authenticates a user with email and password. Returns user information on successful login.")
    .Accepts<BackEnd.Data.Models.LoginRequest>("application/json")
    .Produces<{| id: System.Guid; email: string; name: string; message: string |}>(200)
    .Produces(400)
    .Produces(401) |> ignore
```

**What it does:**
- Maps POST to `/api/auth/login`
- Authenticates user and returns user info on success

**Breaking it down:**
- Similar structure to register endpoint
- `.Produces(401)` - Unauthorized response (invalid credentials)

---

### 10.3 Book Endpoints

#### Get All Books (with Search)
```fsharp
app.MapGet("/api/books", 
    System.Func<AppDbContext, Microsoft.AspNetCore.Http.HttpContext, System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>>(
        fun db ctx -> 
            let searchTerm = match ctx.Request.Query.TryGetValue("search") with
                             | true, values when values.Count > 0 -> Some (values.[0].ToString())
                             | _ -> None
            BookHandler.getBooks db searchTerm))
    .WithName("GetBooks")
    .WithTags("Books")
    .WithSummary("Get all books")
    .WithDescription("Retrieves a list of all books. Optionally search by title, author, or ISBN using the 'search' query parameter.") |> ignore
```

**What it does:**
- Maps GET to `/api/books`
- Extracts optional `search` query parameter
- Example: `GET /api/books?search=harry`

**Breaking it down:**
- `HttpContext` parameter - Gives access to request details (query strings, headers, etc.)
- `ctx.Request.Query.TryGetValue("search")` - Tries to get query parameter
- Pattern matching on result:
  - `true, values when values.Count > 0` - Parameter exists and has value
  - `_` - Parameter doesn't exist or is empty
- `Some (values.[0].ToString())` - Wraps value in Option type
- `None` - No search term

**Why HttpContext:**
- Query parameters aren't automatically extracted like route/body parameters
- Need to manually read from `Request.Query`

---

#### Get Book By ID
```fsharp
app.MapGet("/api/books/{id:guid}", 
    System.Func<AppDbContext, System.Guid, System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>>(
        fun db id -> BookHandler.getBookById db id))
    .WithName("GetBookById")
    .WithTags("Books")
    .WithSummary("Get book by ID")
    .WithDescription("Retrieves a specific book by its unique identifier") |> ignore
```

**What it does:**
- Standard GET by ID endpoint for books
- Similar pattern to user endpoints

---

#### Create Book
```fsharp
app.MapPost("/api/books", 
    System.Func<AppDbContext, BackEnd.Data.Models.Book, System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>>(
        fun db book -> BookHandler.createBook db book))
    .WithName("CreateBook")
    .WithTags("Books")
    .WithSummary("Create a new book")
    .WithDescription("Creates a new book in the library") |> ignore
```

**What it does:**
- Creates a new book
- Request body is deserialized to `Book` object

---

#### Update Book
```fsharp
app.MapPut("/api/books/{id:guid}", 
    System.Func<AppDbContext, System.Guid, BackEnd.Data.Models.Book, System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>>(
        fun db id book -> BookHandler.updateBook db id book))
    .WithName("UpdateBook")
    .WithTags("Books")
    .WithSummary("Update a book")
    .WithDescription("Updates an existing book") |> ignore
```

**What it does:**
- Updates existing book
- `id` from route, `book` from request body

---

#### Delete Book
```fsharp
app.MapDelete("/api/books/{id:guid}", 
    System.Func<AppDbContext, System.Guid, System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>>(
        fun db id -> BookHandler.deleteBook db id))
    .WithName("DeleteBook")
    .WithTags("Books")
    .WithSummary("Delete a book")
    .WithDescription("Deletes a book from the library") |> ignore
```

**What it does:**
- Deletes a book by ID
- Only needs `id`, no request body

---

### 10.4 Borrowing Endpoints

#### Get All Borrowings
```fsharp
app.MapGet("/api/borrowings", 
    System.Func<AppDbContext, System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>>(
        fun db -> BookHandler.getAllBorrowings db))
    .WithName("GetAllBorrowings")
    .WithTags("Borrowings")
    .WithSummary("Get all borrowings")
    .WithDescription("Retrieves all borrowing records, including active, returned, and overdue books") |> ignore
```

**What it does:**
- Returns all borrowing records in the system

---

#### Borrow Book
```fsharp
app.MapPost("/api/books/{bookId:guid}/borrow", 
    System.Func<AppDbContext, System.Guid, BackEnd.Data.Models.BorrowRequest, System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>>(
        fun db bookId request -> BookHandler.borrowBook db bookId request.UserId))
    .WithName("BorrowBook")
    .WithTags("Borrowings")
    .WithSummary("Borrow a book")
    .WithDescription("Borrows a book for a user. The book must be available.") |> ignore
```

**What it does:**
- Borrows a book for a user
- `bookId` from route, `UserId` from request body

**Example request:**
```
POST /api/books/123e4567-e89b-12d3-a456-426614174000/borrow
Body: { "userId": "456e7890-e89b-12d3-a456-426614174000" }
```

---

#### Return Book
```fsharp
app.MapPost("/api/borrowings/{borrowingId:guid}/return", 
    System.Func<AppDbContext, System.Guid, BackEnd.Data.Models.ReturnRequest, System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>>(
        fun db borrowingId request -> BookHandler.returnBook db borrowingId request.UserId))
    .WithName("ReturnBook")
    .WithTags("Borrowings")
    .WithSummary("Return a borrowed book")
    .WithDescription("Returns a borrowed book and makes it available again") |> ignore
```

**What it does:**
- Returns a borrowed book
- Marks borrowing as returned and increments available copies

---

#### Get User's Borrowings
```fsharp
app.MapGet("/api/users/{userId:guid}/borrowings", 
    System.Func<AppDbContext, System.Guid, System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>>(
        fun db userId -> BookHandler.getUserBorrowings db userId))
    .WithName("GetUserBorrowings")
    .WithTags("Borrowings")
    .WithSummary("Get user's borrowings")
    .WithDescription("Retrieves all borrowing records for a specific user, including active, returned, and overdue books") |> ignore
```

**What it does:**
- Returns all borrowings for a specific user

---

### 10.5 Health Check Endpoint

```fsharp
app.MapGet("/health", 
    System.Func<Microsoft.AspNetCore.Http.IResult>(
        fun () -> Microsoft.AspNetCore.Http.Results.Ok({| status = "healthy" |}) :> Microsoft.AspNetCore.Http.IResult))
    .WithName("HealthCheck")
    .WithTags("Health")
    .WithSummary("Health check")
    .WithDescription("Returns the health status of the API") |> ignore
```

**What it does:**
- Simple endpoint to check if API is running
- Returns `{ "status": "healthy" }`

**Breaking it down:**
- `{| status = "healthy" |}` - Anonymous record (inline object creation)
- `:> Microsoft.AspNetCore.Http.IResult` - Type cast (required for type system)

**Why we use it:**
- Monitoring systems can ping this endpoint
- Load balancers can check if service is up
- Simple way to verify API is responding

---

## 11. Application Execution

```fsharp
app.Run()
```

**What it does:**
- Starts the web server
- Begins listening for HTTP requests
- Blocks until application shuts down

**Why we use it:**
- This is the final step that actually runs the application
- Without this, the app would configure everything but never start

---

## Summary

**Program.fs Flow:**
1. **Configure** services** (JSON, Swagger, Database, CORS)
2. **Build** the application
3. **Add** middleware (Swagger, CORS, HTTPS)
4. **Map** endpoints (routes to handlers)
5. **Run** the application

**Key Concepts:**
- **Dependency Injection**: Services registered in `builder.Services` are automatically injected into handlers
- **Minimal APIs**: Modern ASP.NET Core pattern using `MapGet`, `MapPost`, etc. instead of controllers
- **Type Safety**: F# ensures handlers match expected signatures
- **Functional Style**: Handlers are pure functions composed together

**Request Flow:**
1. HTTP request arrives
2. Middleware processes it (CORS, HTTPS)
3. Route matched to endpoint
4. Dependencies injected (e.g., `AppDbContext`)
5. Handler function called
6. Response returned

This architecture is clean, testable, and follows functional programming principles while leveraging ASP.NET Core's powerful features.

