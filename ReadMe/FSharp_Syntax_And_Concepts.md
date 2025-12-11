<!-- Role: Documentation Lead -->
<!-- Developer: Omar Salama -->
# F# Syntax and Concepts Guide

This document explains the F# syntax and functional programming concepts used throughout this project.

## Table of Contents
1. [Pipe Operator (`|>`)]
2. [Lambda Functions (`->`)]
3. [Pattern Matching (`match`)]
4. [Option Type (`Option<'T>`)]
5. [Result Type (`Result<'T, 'E>`)]
6. [Task Computation Expression (`task { }`)]
7. [Modules and Namespaces]
8. [Type Annotations]
9. [Immutable by Default]
10. [Function Composition]

---

## 1. Pipe Operator (`|>`)

**What it is:** The pipe operator passes the result of the left expression as the last argument to the function on the right.

**Syntax:**
```fsharp
value |> function
```

**Why we use it:** It makes code more readable by allowing you to write operations in the order they execute, similar to Unix pipes.

**Examples from our code:**

```fsharp
// Instead of: ignore(db.Users.Add(user))
db.Users.Add(user) |> ignore

// Instead of: ignore(app.UseSwagger())
app.UseSwagger() |> ignore

// Instead of: ignore(options.UseSqlServer(connectionString))
options.UseSqlServer(connectionString) |> ignore
```

**Chaining pipes:**
```fsharp
// You can chain multiple operations
let result = 
    data
    |> processData
    |> validateData
    |> saveToDatabase
```

---

## 2. Lambda Functions (`->`)

**What it is:** Anonymous functions (lambdas) that take parameters and return a value.

**Syntax:**
```fsharp
fun parameter -> expression
```

**Why we use it:** Allows inline function definitions without naming them, especially useful for callbacks and LINQ-style queries.

**Examples from our code:**

```fsharp
// Lambda in Entity Framework query
db.Users.FirstOrDefaultAsync(fun u -> u.Email = email)

// Lambda in Where clause
db.Borrowings.Where(fun b -> 
    b.UserId = userId && 
    b.BookId = bookId && 
    b.Status = "Active")

// Lambda in configuration
builder.Services.ConfigureHttpJsonOptions(fun options ->
    options.SerializerOptions.ReferenceHandler <- ReferenceHandler.IgnoreCycles
)
```

**Multiple parameters:**
```fsharp
fun x y -> x + y  // Takes two parameters
```

---

## 3. Pattern Matching (`match`)

**What it is:** A powerful control flow construct that matches a value against patterns and executes code based on the match.

**Syntax:**
```fsharp
match expression with
| pattern1 -> result1
| pattern2 -> result2
| _ -> defaultResult  // _ is a wildcard
```

**Why we use it:** It's safer than if-else chains and works beautifully with Option and Result types. The compiler ensures all cases are handled.

**Examples from our code:**

```fsharp
// Pattern matching on Result type
match validateRegistration email username password with
| Error validationError ->
    return Results.BadRequest(validationError)
| Ok (validEmail, validUsername, validPassword) ->
    // Continue with valid data
    ...

// Pattern matching on Option type
match userOption with
| None ->
    Results.NotFound("User not found")
| Some user ->
    Results.Ok(user)

// Pattern matching with tuple deconstruction
match validateLogin email password with
| Error msg -> return Results.BadRequest(msg)
| Ok (validEmail, validPassword) ->
    // Use validEmail and validPassword
    ...
```

**Pattern matching with guards:**
```fsharp
match value with
| x when x > 0 -> "Positive"
| x when x < 0 -> "Negative"
| _ -> "Zero"
```

---

## 4. Option Type (`Option<'T>`)

**What it is:** A type that represents a value that might or might not exist. Similar to nullable types in C#, but safer.

**Syntax:**
```fsharp
Some value  // Value exists
None        // No value
```

**Why we use it:** Prevents null reference exceptions. Forces you to handle the "no value" case explicitly.

**Examples from our code:**

```fsharp
// Function that returns Option
let findUserByEmailAsync (db: AppDbContext) (email: string) =
    task {
        let! user = db.Users.FirstOrDefaultAsync(fun u -> u.Email = email)
        return if isNull (box user) then None else Some user
    }

// Using Option with pattern matching
let! userOption = findUserByEmailAsync db email
match userOption with
| None -> return Results.NotFound()
| Some user -> return Results.Ok(user)
```

**Common Option operations:**
```fsharp
let value = Some 42
let noValue = None

// Check if has value
match value with
| Some x -> printfn "Value is %d" x
| None -> printfn "No value"

// Map over Option
let doubled = value |> Option.map (fun x -> x * 2)  // Some 84
```

---

## 5. Result Type (`Result<'T, 'E>`)

**What it is:** A type that represents either success (`Ok`) or failure (`Error`). Similar to exceptions, but explicit and type-safe.

**Syntax:**
```fsharp
Ok value      // Success case
Error message // Failure case
```

**Why we use it:** Makes error handling explicit and forces you to handle errors. No hidden exceptions.

**Examples from our code:**

```fsharp
// Function that returns Result
let validateRegistration (email: string) (username: string) (password: string) =
    if String.IsNullOrWhiteSpace(email) then
        Error "Email is required"
    elif not (email.Contains("@")) then
        Error "Email must be a valid email address"
    else
        Ok (email, username, password)

// Using Result with pattern matching
match validateRegistration email username password with
| Error validationError ->
    return Results.BadRequest(validationError)
| Ok (validEmail, validUsername, validPassword) ->
    // Continue with validated data
    ...

// Database operation returning Result
let saveUserAsync (db: AppDbContext) (user: User) =
    task {
        try
            db.Users.Add(user) |> ignore
            let! _ = db.SaveChangesAsync()
            return Ok user
        with
        | :? DbUpdateException as ex ->
            return Error $"Failed to save user: {ex.Message}"
    }
```

**Result vs Exception:**
- **Result**: Explicit, type-safe, forces error handling
- **Exception**: Hidden, can be forgotten, harder to track

---

## 6. Task Computation Expression (`task { }`)

**What it is:** A computation expression for working with asynchronous operations (similar to async/await in C#).

**Syntax:**
```fsharp
task {
    let! result = asyncOperation()
    return result
}
```

**Why we use it:** F# doesn't have async/await keywords. The `task { }` computation expression provides similar functionality for .NET Tasks.

**Examples from our code:**

```fsharp
// Simple async function
let findUserByEmailAsync (db: AppDbContext) (email: string) =
    task {
        let! user = db.Users.FirstOrDefaultAsync(fun u -> u.Email = email)
        return if isNull (box user) then None else Some user
    }

// Multiple async operations
let register (db: AppDbContext) (email: string) (username: string) (password: string) =
    task {
        // Step 1: Validate
        match validateRegistration email username password with
        | Error validationError ->
            return Results.BadRequest(validationError)
        | Ok (validEmail, validUsername, validPassword) ->
            // Step 2: Check if email exists (async)
            let! emailExists = emailExistsAsync db validEmail
            if emailExists then
                return Results.BadRequest("Email is already registered")
            else
                // Step 3: Create and save (async)
                let newUser = createUser validEmail validUsername validPassword
                let! saveResult = saveUserAsync db newUser
                return match saveResult with
                       | Ok savedUser -> Results.Created(...)
                       | Error errorMsg -> Results.Problem(...)
    }
```

**Key points:**
- `let!` is used to await async operations
- `return` is used to return a value from the task
- `task { }` automatically handles Task unwrapping

---

## 7. Modules and Namespaces

**What they are:** Ways to organize code into logical units.

**Namespace:** Groups related modules together (like packages in Java or namespaces in C#).
```fsharp
namespace BackEnd.Handlers
```

**Module:** Groups related functions and types together.
```fsharp
module AuthHandler =
    let hashPassword (password: string) = ...
    let verifyPassword (password: string) (hash: string) = ...
```

**Why we use them:** 
- **Namespaces**: Prevent naming conflicts across the codebase
- **Modules**: Organize related functions, make code reusable

**Examples from our code:**
```fsharp
namespace BackEnd.Handlers

module AuthHandler =
    // All auth-related functions here

module BookHandler =
    // All book-related functions here
```

---

## 8. Type Annotations

**What it is:** Explicitly specifying the type of a value or function parameter.

**Syntax:**
```fsharp
let functionName (parameter: Type) : ReturnType = ...
```

**Why we use it:** 
- Makes code more readable
- Helps the compiler catch errors
- Documents intent

**Examples from our code:**

```fsharp
// Function with type annotations
let hashPassword (password: string) : string =
    use sha256 = SHA256.Create()
    let bytes = Encoding.UTF8.GetBytes(password)
    let hash = sha256.ComputeHash(bytes)
    Convert.ToBase64String(hash)

// Function with multiple parameters
let validateRegistration (email: string) (username: string) (password: string) 
    : Result<string * string * string, string> =
    // Implementation
```

**Type inference:** F# can often infer types, but explicit annotations improve clarity:
```fsharp
// Type inferred
let add x y = x + y

// Explicit type annotation
let add (x: int) (y: int) : int = x + y
```

---

## 9. Immutable by Default

**What it is:** Values cannot be changed after they're created (unless explicitly marked as mutable).

**Why we use it:** 
- Prevents bugs from accidental mutations
- Makes code easier to reason about
- Enables safe concurrent programming

**Examples:**

```fsharp
// Immutable (default)
let x = 5
// x <- 10  // ERROR: Cannot reassign immutable value

// Mutable (explicit)
let mutable y = 5
y <- 10  // OK: y is mutable

// In our code, we use mutable for C# objects (EF Core entities)
let user = User()
user.Id <- Guid.NewGuid()  // OK: C# objects are mutable
user.Email <- email
```

**Functional approach:**
```fsharp
// Instead of mutating, create new values
let original = [1; 2; 3]
let doubled = original |> List.map (fun x -> x * 2)  // New list, original unchanged
```

---

## 10. Function Composition

**What it is:** Combining small functions to build larger ones.

**Why we use it:** 
- Breaks complex logic into small, testable pieces
- Makes code reusable
- Follows single responsibility principle

**Examples from our code:**

```fsharp
// Small, focused functions
let validateRegistration email username password = ...
let emailExistsAsync db email = ...
let createUser email username password = ...
let saveUserAsync db user = ...

// Composed into larger function
let register db email username password =
    task {
        // Compose: validate -> check -> create -> save
        match validateRegistration email username password with
        | Error e -> return Results.BadRequest(e)
        | Ok (validEmail, validUsername, validPassword) ->
            let! exists = emailExistsAsync db validEmail
            if exists then return Results.BadRequest("Email exists")
            else
                let user = createUser validEmail validUsername validPassword
                let! result = saveUserAsync db user
                return match result with
                       | Ok u -> Results.Created(...)
                       | Error e -> Results.Problem(...)
    }
```

**Function composition operators:**
```fsharp
// Forward composition (>>)
let processData = validate >> transform >> save

// Backward composition (<<)
let processData = save << transform << validate
```

---

## Additional Concepts

### Discard Operator (`|> ignore`)

Used when a function returns a value but we don't need it:

```fsharp
db.Users.Add(user) |> ignore  // Add returns EntityEntry, we don't need it
app.UseSwagger() |> ignore     // Returns IApplicationBuilder, we don't need it
```

### `use` Keyword

Automatically disposes resources when they go out of scope:

```fsharp
use sha256 = SHA256.Create()  // Automatically disposed at end of scope
let hash = sha256.ComputeHash(bytes)
```

### Tuples

Groups multiple values together:

```fsharp
let tuple = (email, username, password)  // Tuple of 3 strings
let (email, username, password) = tuple  // Deconstruct tuple
```

### Active Patterns

Advanced pattern matching (not used in our code, but worth knowing):

```fsharp
let (|Even|Odd|) n = if n % 2 = 0 then Even else Odd

match 5 with
| Even -> "Even"
| Odd -> "Odd"
```

---

## Summary

F# emphasizes:
- **Type safety**: Option and Result types prevent null/exception bugs
- **Immutability**: Values don't change unexpectedly
- **Composition**: Build complex logic from simple functions
- **Expressiveness**: Code reads like the problem it solves
- **Functional style**: Functions are first-class citizens

These concepts work together to create robust, maintainable code that's easier to test and reason about.

