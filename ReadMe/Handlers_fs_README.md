<!-- Role: Documentation Lead -->
<!-- Developer: Omar Salama -->
# Handlers.fs - Complete Workflow and Code Explanation

This document explains the `Handlers.fs` module, which provides basic user management operations (CRUD) using functional programming principles.

## Table of Contents
1. [Overview]
2. [Module Structure]
3. [Configuration and Utilities]
4. [Validation Functions]
5. [Database Query Functions]
6. [Business Logic Functions]
7. [Handler Functions]
8. [Complete Workflow Examples]

---

## Overview

**Purpose:** Handle basic user management operations (Get All, Get By ID, Create).

**Key Features:**
- Get all users
- Get user by ID
- Create new user
- Input validation
- JSON serialization configuration

**Key Principles:**
- **Pure validation functions** for input validation
- **Async database operations** using `task { }`
- **Result type** for explicit error handling
- **Option type** for safe null handling
- **Function composition** for complex workflows

**Note:** This module handles basic user operations. For authentication (register/login), see `AuthHandler.fs`.

---

## Module Structure

```fsharp
namespace BackEnd.Handlers

module Handlers =
    // JSON configuration
    // Utility functions
    // Validation functions
    // Database query functions
    // Business logic functions
    // Handler functions (entry points)
```

**Organization:** Functions are organized from configuration/utilities to handlers, following the same pattern as other handler modules.

---

## 1. Configuration and Utilities

### JSON Options Configuration

```fsharp
let jsonOptions = 
    let opts = JsonSerializerOptions()
    opts.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
    opts.Converters.Add(JsonFSharpConverter())
    opts
```

**What it does:**
- Configures JSON serialization options
- Used when returning JSON responses

**Breaking it down:**
- `JsonSerializerOptions()` - Creates default JSON options
- `PropertyNamingPolicy <- JsonNamingPolicy.CamelCase` - Converts property names to camelCase
  - C#: `FirstName` → JSON: `firstName`
  - F#: `FirstName` → JSON: `firstName`
- `Converters.Add(JsonFSharpConverter())` - Adds F# type converter
  - Handles F# types like `Option`, `Result`, discriminated unions
  - Makes F# types serializable to JSON

**Why we use it:**
- **CamelCase**: JavaScript/frontend convention
- **F# Converter**: Enables serialization of F#-specific types

**Example:**
```fsharp
// Without converter, Option types might not serialize correctly
// With converter, Some value → JSON value, None → null
```

**Usage:**
```fsharp
Results.Json(user, jsonOptions)  // Uses configured options
```

---

### `toOption` Utility Function

```fsharp
let toOption (value: 'a when 'a : null) : 'a option =
    if isNull value then None else Some value
```

**What it does:**
- Converts nullable C# values to F# Option type
- Generic function that works with any nullable type

**Breaking it down:**
- `'a when 'a : null` - Generic type parameter with constraint (must be nullable)
- `isNull value` - Checks if value is null
- Returns `None` if null, `Some value` if not null

**Why we use it:**
- Entity Framework returns `null` when not found
- F# Option type is safer and more idiomatic
- This utility converts between the two

**Example:**
```fsharp
let user = db.Users.FirstOrDefaultAsync(...)  // Returns User or null
let userOption = toOption user  // Converts to Option<User>
match userOption with
| None -> // Not found
| Some u -> // Found
```

**Alternative approach:**
In `AuthHandler.fs`, we do this inline:
```fsharp
return if isNull (box user) then None else Some user
```
This utility function makes it cleaner and reusable.

---

## 2. Validation Functions

### `validateUser`

```fsharp
let validateUser (user: User) : Microsoft.FSharp.Core.Result<User, string> =
    if System.String.IsNullOrWhiteSpace(user.Name) then
        Microsoft.FSharp.Core.Error "Name is required and cannot be empty"
    elif System.String.IsNullOrWhiteSpace(user.Email) then
        Microsoft.FSharp.Core.Error "Email is required and cannot be empty"
    elif not (user.Email.Contains("@")) then
        Microsoft.FSharp.Core.Error "Email must be a valid email address"
    else
        Microsoft.FSharp.Core.Ok user
```

**What it does:**
- Validates user input for creation
- Returns `Result<User, string>`

**Breaking it down:**
- **Return type**: `Result<User, string>`
  - Success: Returns the validated user
  - Error: Returns error message
- **Validation checks:**
  1. Name not empty
  2. Email not empty
  3. Email contains "@" (basic email validation)

**Why Result type:**
- Explicit error handling
- Type-safe validation
- Composable with other functions

**Example:**
```fsharp
match validateUser user with
| Error msg -> return Results.BadRequest(msg)
| Ok validUser -> // Continue with validated user
```

**Note:** This is simpler than `AuthHandler.validateRegistration` because:
- No password validation (password handled in AuthHandler)
- No username length check
- Basic email validation only

---

## 3. Database Query Functions

### `findUserByIdAsync`

```fsharp
let findUserByIdAsync (db: AppDbContext) (id: System.Guid) =
    task {
        let! user = db.Users.FirstOrDefaultAsync(fun u -> u.Id = id)
        return toOption user
    }
```

**What it does:**
- Finds a user by their ID
- Returns `Option<User>` (Some user or None)

**Breaking it down:**
- `task { }` - Async computation expression
- `let! user = ...` - Awaits database query
- `FirstOrDefaultAsync(fun u -> u.Id = id)` - LINQ query
  - `fun u -> u.Id = id` - Lambda predicate
  - Returns first match or null
- `toOption user` - Converts null to Option type

**Why Option type:**
- Safe null handling
- Forces explicit "not found" handling
- More idiomatic F#

**Example:**
```fsharp
let! userOption = findUserByIdAsync db userId
match userOption with
| None -> Results.NotFound()
| Some user -> Results.Ok(user)
```

---

### `getAllUsersAsync`

```fsharp
let getAllUsersAsync (db: AppDbContext) =
    db.Users.ToListAsync()
```

**What it does:**
- Gets all users from database
- Returns `Task<List<User>>`

**Breaking it down:**
- `db.Users` - Entity Framework DbSet
- `ToListAsync()` - Executes query and returns list
- No filtering or ordering (returns all users)

**Why simple:**
- No complex query needed
- Just returns all users
- Can be extended with filtering/ordering if needed

**Example:**
```fsharp
let! users = getAllUsersAsync db
// users is List<User>
```

---

## 4. Business Logic Functions

### `prepareUserForCreation`

```fsharp
let prepareUserForCreation (user: User) : User =
    // Note: We still need to mutate C# objects for EF Core,
    // but we isolate this in a single function
    user.CreatedAt <- System.DateTime.UtcNow
    user
```

**What it does:**
- Prepares a user object for database insertion
- Sets the `CreatedAt` timestamp
- Returns the modified user

**Breaking it down:**
- `user.CreatedAt <- System.DateTime.UtcNow` - Sets creation timestamp
- `user` - Returns the user (same object, mutated)

**Why we use it:**
- Encapsulates preparation logic
- Ensures `CreatedAt` is always set
- Single place to modify if logic changes

**Note on mutability:**
- C# objects (like EF Core entities) are mutable
- F# prefers immutability, but we need to work with EF Core
- Isolating mutation in one function is a compromise

**Example:**
```fsharp
let preparedUser = prepareUserForCreation user
// preparedUser.CreatedAt is now set to current UTC time
```

---

### `saveUserAsync`

```fsharp
let saveUserAsync (db: AppDbContext) (user: User) =
    task {
        try
            db.Users.Add(user) |> ignore
            let! _ = db.SaveChangesAsync()
            return Microsoft.FSharp.Core.Ok user
        with
        | :? DbUpdateException as ex ->
            return Microsoft.FSharp.Core.Error $"Failed to save user: {ex.Message}"
        | ex ->
            return Microsoft.FSharp.Core.Error $"Unexpected error: {ex.Message}"
    }
```

**What it does:**
- Saves a user to the database
- Returns `Result<User, string>`

**Breaking it down:**
- `db.Users.Add(user)` - Adds user to EF Core context
- `|> ignore` - Discards return value (EntityEntry)
- `db.SaveChangesAsync()` - Persists to database
- `let! _ = ...` - Awaits async operation, discards result
- Exception handling converts to Result type

**Exception types:**
- `DbUpdateException` - Database errors (constraint violations, etc.)
- `ex` - Any other unexpected error

**Why Result type:**
- Explicit error handling
- No hidden exceptions
- Caller must handle errors

**Example:**
```fsharp
let! result = saveUserAsync db user
match result with
| Ok savedUser -> printfn "Saved: %s" savedUser.Id
| Error msg -> printfn "Error: %s" msg
```

---

## 5. Handler Functions

Handlers are public entry points called by ASP.NET Core. They compose validation, database operations, and business logic.

### `getUsers`

```fsharp
let getUsers (db: AppDbContext) =
    task {
        try
            let! users = getAllUsersAsync db
            return Results.Json(users, jsonOptions)
        with
        | ex ->
            return Results.Problem(
                title = "Error retrieving users",
                detail = ex.Message,
                statusCode = 500
            )
    }
```

**What it does:**
- Gets all users and returns as JSON
- Handles exceptions gracefully

**Breaking it down:**
- `let! users = getAllUsersAsync db` - Awaits database query
- `Results.Json(users, jsonOptions)` - Returns JSON response
  - Uses configured `jsonOptions` (camelCase, F# converter)
- Exception handling returns 500 error

**Response:**
- Success: HTTP 200 OK with JSON array of users
- Error: HTTP 500 Internal Server Error

**Example response:**
```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "email": "user@example.com",
    "name": "John Doe",
    "createdAt": "2024-01-01T00:00:00Z"
  },
  ...
]
```

---

### `getUserById`

```fsharp
let getUserById (db: AppDbContext) (id: System.Guid) =
    task {
        try
            let! userOption = findUserByIdAsync db id
            
            // Pattern matching on Option type (functional approach)
            return match userOption with
                   | Some user -> Results.Json(user, jsonOptions)
                   | None -> Results.NotFound()
        with
        | ex ->
            return Results.Problem(
                title = "Error retrieving user",
                detail = ex.Message,
                statusCode = 500
            )
    }
```

**What it does:**
- Gets a single user by ID
- Returns 404 if not found, 200 with user if found

**Breaking it down:**
- `let! userOption = findUserByIdAsync db id` - Finds user (Option type)
- Pattern matching:
  - `Some user` - User found → Return JSON
  - `None` - User not found → Return 404

**Why pattern matching:**
- Exhaustive: Compiler ensures all cases handled
- Type-safe: Can't forget to handle None case
- Readable: Clear intent

**Response:**
- Success: HTTP 200 OK with user JSON
- Not found: HTTP 404 Not Found
- Error: HTTP 500 Internal Server Error

**Example response (success):**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "email": "user@example.com",
  "name": "John Doe",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

---

### `createUser`

```fsharp
let createUser (db: AppDbContext) (user: User) =
    task {
        // Function composition: validate -> prepare -> save
        match validateUser user with
        | Microsoft.FSharp.Core.Error validationError ->
            return Results.BadRequest(validationError)
        | Microsoft.FSharp.Core.Ok validUser ->
            let preparedUser = prepareUserForCreation validUser
            let! saveResult = saveUserAsync db preparedUser
            
            // Pattern matching on Result type
            return match saveResult with
                   | Microsoft.FSharp.Core.Ok savedUser ->
                       Results.Created($"/api/users/{savedUser.Id}", savedUser)
                   | Microsoft.FSharp.Core.Error errorMsg ->
                       Results.Problem(
                           title = "Failed to create user",
                           detail = errorMsg,
                           statusCode = 500
                       )
    }
```

**What it does:**
- Creates a new user
- Validates input, prepares user, saves to database
- Returns appropriate HTTP response

**Workflow breakdown:**

**Step 1: Validate Input**
```fsharp
match validateUser user with
| Error validationError -> return Results.BadRequest(validationError)
| Ok validUser -> // Continue
```
- Validates user input
- Returns 400 Bad Request if validation fails

**Step 2: Prepare User**
```fsharp
let preparedUser = prepareUserForCreation validUser
```
- Sets `CreatedAt` timestamp
- Prepares user for database insertion

**Step 3: Save to Database**
```fsharp
let! saveResult = saveUserAsync db preparedUser
```
- Attempts to save user
- Returns Result type (Ok or Error)

**Step 4: Return Result**
```fsharp
match saveResult with
| Ok savedUser -> Results.Created(...)  // 201 Created
| Error errorMsg -> Results.Problem(...)  // 500 Internal Server Error
```
- Success: Returns 201 Created with location header
- Error: Returns 500 with error message

**Response types:**
- `Results.BadRequest` - 400 (validation error)
- `Results.Created` - 201 (success, includes location header)
- `Results.Problem` - 500 (database error)

**Example request:**
```json
POST /api/users
{
  "email": "user@example.com",
  "name": "John Doe"
}
```

**Example response (success):**
```json
HTTP 201 Created
Location: /api/users/123e4567-e89b-12d3-a456-426614174000
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "email": "user@example.com",
  "name": "John Doe",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

---

## Complete Workflow Examples

### Get All Users

```
1. User sends GET /api/users

2. Program.fs calls Handlers.getUsers(db)

3. getAllUsersAsync(db)
   → Queries database: SELECT * FROM Users
   → Returns: List<User>

4. Results.Json(users, jsonOptions)
   → Serializes to JSON (camelCase)
   → HTTP 200 OK
   → Response: JSON array of users
```

### Get User By ID

```
1. User sends GET /api/users/{id}

2. Program.fs calls Handlers.getUserById(db, id)

3. findUserByIdAsync(db, id)
   → Queries database: SELECT * FROM Users WHERE Id = {id}
   → Returns: Option<User> (Some user or None)

4. Pattern matching:
   - Some user → Results.Json(user, jsonOptions) → HTTP 200 OK
   - None → Results.NotFound() → HTTP 404 Not Found
```

### Create User

```
1. User sends POST /api/users
   Body: { "email": "user@example.com", "name": "John Doe" }

2. Program.fs calls Handlers.createUser(db, user)

3. validateUser(user)
   → Checks: name not empty, email not empty, email contains "@"
   → Returns: Ok user (or Error message)

4. If validation passed:
   prepareUserForCreation(user)
   → Sets: user.CreatedAt = DateTime.UtcNow
   → Returns: preparedUser

5. saveUserAsync(db, preparedUser)
   → db.Users.Add(preparedUser)
   → db.SaveChangesAsync()
   → Returns: Ok savedUser (or Error message)

6. Results.Created(...)
   → HTTP 201 Created
   → Location: /api/users/{id}
   → Response: Created user object
```

---

## Key Design Patterns

### 1. Function Composition
Small, focused functions composed into larger operations:
```
validateUser → prepareUserForCreation → saveUserAsync → Results
```

### 2. Result Type for Error Handling
Explicit errors instead of exceptions:
```fsharp
Result<User, string>  // Success or Error
```

### 3. Option Type for Null Safety
Safe handling of potentially missing data:
```fsharp
Option<User>  // Some user or None
```

### 4. Pattern Matching
Exhaustive case handling:
```fsharp
match result with
| Ok value -> // Handle success
| Error msg -> // Handle error

match option with
| Some value -> // Handle value
| None -> // Handle absence
```

### 5. JSON Configuration
Centralized JSON serialization settings:
- CamelCase naming
- F# type support
- Consistent across all responses

### 6. Exception to Result Conversion
Database operations convert exceptions to Result:
```fsharp
try
    // Database operation
    return Ok result
with
| ex -> return Error ex.Message
```

---

## Comparison with AuthHandler

**Similarities:**
- Both use Result type for error handling
- Both use Option type for null safety
- Both follow function composition pattern
- Both use async database operations

**Differences:**

| Aspect | Handlers.fs | AuthHandler.fs |
|--------|-------------|----------------|
| **Purpose** | Basic user CRUD | Authentication (register/login) |
| **Password** | Not handled | Hashed and verified |
| **Validation** | Basic (name, email) | Comprehensive (email, username, password) |
| **Security** | No security concerns | Password hashing, validation |

**Why separate:**
- **Separation of concerns**: User management vs authentication
- **Different requirements**: CRUD doesn't need password handling
- **Reusability**: Handlers can be used without authentication logic

---

## Summary

**Handlers.fs** demonstrates:
- ✅ Basic CRUD operations
- ✅ Input validation
- ✅ Type-safe error handling
- ✅ JSON serialization configuration
- ✅ Function composition
- ✅ Pattern matching
- ✅ Async operations

The module provides a clean, functional approach to user management while working seamlessly with Entity Framework Core and ASP.NET Core.

**Key Takeaways:**
- Small, focused functions are easier to test and maintain
- Result and Option types make error handling explicit
- Function composition builds complex operations from simple ones
- Pattern matching ensures all cases are handled
- JSON configuration ensures consistent API responses

