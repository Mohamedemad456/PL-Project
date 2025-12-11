<!-- Role: Documentation Lead -->
<!-- Developer: Omar Salama -->
# AuthHandler.fs - Complete Workflow and Code Explanation

This document explains the `AuthHandler.fs` module, which handles user authentication (registration and login) using functional programming principles.

## Table of Contents
1. [Overview]
2. [Module Structure]
3. [Password Hashing Functions]
4. [Validation Functions]
5. [Database Query Functions]
6. [Business Logic Functions]
7. [Handler Functions]
8. [Complete Workflow Examples]

---

## Overview

**Purpose:** Handle user registration and login with secure password hashing and validation.

**Key Principles:**
- **Pure Functions**: Validation and hashing functions have no side effects
- **Result Type**: Explicit error handling instead of exceptions
- **Option Type**: Safe handling of potentially missing data
- **Function Composition**: Build complex operations from simple functions
- **Async Operations**: Non-blocking database operations using `task { }`

---

## Module Structure

```fsharp
namespace BackEnd.Handlers

module AuthHandler =
    // Password hashing functions
    // Validation functions
    // Database query functions
    // Business logic functions
    // Handler functions (entry points)
```

**Organization:** Functions are grouped by responsibility, from low-level (hashing) to high-level (handlers).

---

## 1. Password Hashing Functions

### `hashPassword`

```fsharp
let hashPassword (password: string) : string =
    use sha256 = SHA256.Create()
    let bytes = Encoding.UTF8.GetBytes(password)
    let hash = sha256.ComputeHash(bytes)
    Convert.ToBase64String(hash)
```

**What it does:**
- Takes a plain text password
- Hashes it using SHA256 algorithm
- Returns Base64-encoded hash string

**Breaking it down:**
- `use sha256 = SHA256.Create()` - Creates SHA256 hasher, automatically disposed when done
- `Encoding.UTF8.GetBytes(password)` - Converts string to byte array
- `sha256.ComputeHash(bytes)` - Computes hash
- `Convert.ToBase64String(hash)` - Converts byte array to Base64 string

**Why we use it:**
- **Security**: Never store plain text passwords
- **One-way**: Can't reverse hash to get original password
- **Verification**: Can compare hashes to verify passwords

**Example:**
```fsharp
let hash = hashPassword "mypassword123"
// Returns: "XohImNooBHFR0OVvjcYpJ3NgPQ1qq73WKhHvch0VQtg="
```

**Note:** SHA256 is used here for simplicity. In production, use BCrypt or Argon2 for better security.

---

### `verifyPassword`

```fsharp
let verifyPassword (password: string) (hash: string) : bool =
    let passwordHash = hashPassword password
    passwordHash = hash
```

**What it does:**
- Verifies if a password matches a stored hash
- Returns `true` if they match, `false` otherwise

**Breaking it down:**
- `hashPassword password` - Hashes the provided password
- `passwordHash = hash` - Compares hashes (F# equality operator)

**Why we use it:**
- During login, we need to verify the user's password
- We can't decrypt the stored hash, so we hash the input and compare

**Example:**
```fsharp
let storedHash = "XohImNooBHFR0OVvjcYpJ3NgPQ1qq73WKhHvch0VQtg="
let isValid = verifyPassword "mypassword123" storedHash  // true
let isInvalid = verifyPassword "wrongpassword" storedHash  // false
```

**Workflow:**
1. User provides password
2. Hash the provided password
3. Compare with stored hash
4. Return match result

---

## 2. Validation Functions

### `validateRegistration`

```fsharp
let validateRegistration (email: string) (username: string) (password: string) 
    : Result<string * string * string, string> =
    if System.String.IsNullOrWhiteSpace(email) then
        Microsoft.FSharp.Core.Error "Email is required"
    elif not (email.Contains("@")) then
        Microsoft.FSharp.Core.Error "Email must be a valid email address"
    elif System.String.IsNullOrWhiteSpace(username) then
        Microsoft.FSharp.Core.Error "Username is required"
    elif username.Length < 3 then
        Microsoft.FSharp.Core.Error "Username must be at least 3 characters"
    elif System.String.IsNullOrWhiteSpace(password) then
        Microsoft.FSharp.Core.Error "Password is required"
    elif password.Length < 6 then
        Microsoft.FSharp.Core.Error "Password must be at least 6 characters"
    else
        Microsoft.FSharp.Core.Ok (email, username, password)
```

**What it does:**
- Validates registration input
- Returns `Ok` with validated data, or `Error` with message

**Breaking it down:**
- **Return type**: `Result<string * string * string, string>`
  - Success: Tuple of (email, username, password)
  - Error: Error message string
- **Validation checks**:
  1. Email not empty
  2. Email contains "@"
  3. Username not empty
  4. Username at least 3 characters
  5. Password not empty
  6. Password at least 6 characters
- **`elif`**: F# equivalent of `else if`

**Why we use Result type:**
- **Explicit errors**: No hidden exceptions
- **Type safety**: Compiler forces error handling
- **Composable**: Can chain validations

**Example:**
```fsharp
// Valid input
match validateRegistration "user@example.com" "john" "password123" with
| Ok (email, username, password) -> 
    printfn "Valid: %s, %s, %s" email username password
| Error msg -> 
    printfn "Error: %s" msg

// Invalid input
match validateRegistration "" "ab" "123" with
| Ok _ -> printfn "Valid"
| Error msg -> printfn "Error: %s" msg  // "Email is required"
```

**Workflow:**
1. Check each validation rule in order
2. Return `Error` on first failure
3. Return `Ok` with validated data if all pass

---

### `validateLogin`

```fsharp
let validateLogin (email: string) (password: string) : Result<string * string, string> =
    if System.String.IsNullOrWhiteSpace(email) then
        Microsoft.FSharp.Core.Error "Email is required"
    elif System.String.IsNullOrWhiteSpace(password) then
        Microsoft.FSharp.Core.Error "Password is required"
    else
        Microsoft.FSharp.Core.Ok (email, password)
```

**What it does:**
- Validates login input (simpler than registration)
- Only checks that email and password are provided

**Why simpler:**
- Login doesn't need complex validation
- Just need to ensure fields aren't empty
- Actual authentication happens in database lookup

**Example:**
```fsharp
match validateLogin "user@example.com" "password123" with
| Ok (email, password) -> // Proceed with login
| Error msg -> // Return error to user
```

---

## 3. Database Query Functions

### `findUserByEmailAsync`

```fsharp
let findUserByEmailAsync (db: AppDbContext) (email: string) =
    task {
        let! user = db.Users.FirstOrDefaultAsync(fun u -> u.Email = email)
        return if isNull (box user) then None else Some user
    }
```

**What it does:**
- Finds a user by email address
- Returns `Option<User>` (Some user or None)

**Breaking it down:**
- `task { }` - Async computation expression
- `let! user = ...` - Awaits async database query
- `FirstOrDefaultAsync(fun u -> u.Email = email)` - LINQ query, returns first match or null
- `isNull (box user)` - Checks if user is null (C# objects can be null)
- `Some user` / `None` - Wraps in Option type

**Why Option type:**
- C# returns `null` when not found
- F# Option type is safer (explicitly handles "no value")
- Forces pattern matching (can't forget to check for null)

**Example:**
```fsharp
let! userOption = findUserByEmailAsync db "user@example.com"
match userOption with
| Some user -> printfn "Found: %s" user.Name
| None -> printfn "User not found"
```

**Workflow:**
1. Query database for user with matching email
2. If found, wrap in `Some`
3. If not found (null), return `None`

---

### `emailExistsAsync`

```fsharp
let emailExistsAsync (db: AppDbContext) (email: string) =
    task {
        let! exists = db.Users.AnyAsync(fun u -> u.Email = email)
        return exists
    }
```

**What it does:**
- Checks if an email already exists in database
- Returns `bool` (true if exists, false otherwise)

**Breaking it down:**
- `AnyAsync` - Efficient LINQ method, returns true if any match exists
- More efficient than `FirstOrDefaultAsync` because it stops at first match

**Why we use it:**
- During registration, we need to check if email is already taken
- `AnyAsync` is optimized for existence checks

**Example:**
```fsharp
let! exists = emailExistsAsync db "user@example.com"
if exists then
    printfn "Email already registered"
else
    printfn "Email available"
```

---

## 4. Business Logic Functions

### `createUser`

```fsharp
let createUser (email: string) (username: string) (password: string) : User =
    let user = User()
    user.Id <- Guid.NewGuid()
    user.Email <- email
    user.Name <- username
    user.PasswordHash <- hashPassword password
    user.CreatedAt <- DateTime.UtcNow
    user
```

**What it does:**
- Creates a new `User` object with all required fields
- Hashes password before storing
- Returns configured user object

**Breaking it down:**
- `User()` - Creates new C# object (mutable)
- `user.Id <- Guid.NewGuid()` - Sets unique ID
- `user.PasswordHash <- hashPassword password` - **Important**: Hashes password, doesn't store plain text
- `user.CreatedAt <- DateTime.UtcNow` - Sets creation timestamp
- `user` - Returns the configured user

**Why we use it:**
- Encapsulates user creation logic
- Ensures password is always hashed
- Sets required fields consistently

**Security note:**
- Password is hashed here, not in handler
- Ensures password is never stored in plain text

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
- Saves user to database
- Returns `Result<User, string>`

**Breaking it down:**
- `try ... with` - Exception handling
- `db.Users.Add(user)` - Adds user to context
- `db.SaveChangesAsync()` - Persists to database
- `|> ignore` - Discards return value
- `:? DbUpdateException` - Pattern matches specific exception type
- `ex` - Catches any other exception

**Why Result type:**
- Converts exceptions to explicit errors
- Makes error handling predictable
- Caller must handle errors explicitly

**Exception types:**
- `DbUpdateException` - Database constraint violations (e.g., duplicate key)
- `ex` - Any other unexpected error

**Example:**
```fsharp
let! result = saveUserAsync db newUser
match result with
| Ok savedUser -> printfn "User saved: %s" savedUser.Id
| Error msg -> printfn "Error: %s" msg
```

---

## 5. Handler Functions

Handlers are the entry points called by ASP.NET Core. They compose the smaller functions together.

### `register`

```fsharp
let register (db: AppDbContext) (email: string) (username: string) (password: string) =
    task {
        // Step 1: Validate input
        match validateRegistration email username password with
        | Microsoft.FSharp.Core.Error validationError ->
            return Results.BadRequest(validationError)
        | Microsoft.FSharp.Core.Ok (validEmail, validUsername, validPassword) ->
            // Step 2: Check if email already exists
            let! emailExists = emailExistsAsync db validEmail
            if emailExists then
                return Results.BadRequest("Email is already registered")
            else
                // Step 3: Create user with hashed password
                let newUser = createUser validEmail validUsername validPassword
                
                // Step 4: Save to database
                let! saveResult = saveUserAsync db newUser
                
                // Step 5: Return result
                return match saveResult with
                       | Microsoft.FSharp.Core.Ok savedUser ->
                           Results.Created(
                               $"/api/users/{savedUser.Id}",
                               {| 
                                   id = savedUser.Id
                                   email = savedUser.Email
                                   name = savedUser.Name
                                   message = "User registered successfully"
                               |}
                           )
                       | Microsoft.FSharp.Core.Error errorMsg ->
                           Results.Problem(
                               title = "Failed to register user",
                               detail = errorMsg,
                               statusCode = 500
                           )
    }
```

**What it does:**
- Complete registration workflow
- Returns HTTP result (IResult)

**Workflow breakdown:**

**Step 1: Validate Input**
```fsharp
match validateRegistration email username password with
| Error validationError -> return Results.BadRequest(validationError)
| Ok (validEmail, validUsername, validPassword) -> // Continue
```
- Validates all input fields
- Returns 400 Bad Request if validation fails

**Step 2: Check Email Exists**
```fsharp
let! emailExists = emailExistsAsync db validEmail
if emailExists then
    return Results.BadRequest("Email is already registered")
```
- Checks database for existing email
- Returns 400 if email already registered

**Step 3: Create User**
```fsharp
let newUser = createUser validEmail validUsername validPassword
```
- Creates user object with hashed password
- All fields properly initialized

**Step 4: Save to Database**
```fsharp
let! saveResult = saveUserAsync db newUser
```
- Attempts to save user
- Returns Result type (Ok or Error)

**Step 5: Return Result**
```fsharp
match saveResult with
| Ok savedUser -> Results.Created(...)  // 201 Created
| Error errorMsg -> Results.Problem(...)  // 500 Internal Server Error
```
- Success: Returns 201 Created with user data
- Error: Returns 500 with error message

**Response types:**
- `Results.BadRequest` - 400 (validation/duplicate email)
- `Results.Created` - 201 (success, includes location header)
- `Results.Problem` - 500 (database error)

**Anonymous record:**
```fsharp
{| 
    id = savedUser.Id
    email = savedUser.Email
    name = savedUser.Name
    message = "User registered successfully"
|}
```
- Inline object creation
- Returns only necessary fields (not password hash!)

---

### `login`

```fsharp
let login (db: AppDbContext) (email: string) (password: string) =
    task {
        // Step 1: Validate input
        match validateLogin email password with
        | Microsoft.FSharp.Core.Error validationError ->
            return Results.BadRequest(validationError)
        | Microsoft.FSharp.Core.Ok (validEmail, validPassword) ->
            // Step 2: Find user by email
            let! userOption = findUserByEmailAsync db validEmail
            
            // Step 3: Verify user exists and password matches
            return match userOption with
                   | None ->
                       Results.Json(
                           {| message = "Invalid email or password" |},
                           statusCode = 401
                       )
                   | Some user ->
                       if verifyPassword validPassword user.PasswordHash then
                           // Login successful
                           Results.Ok({|
                               userId = user.Id
                               id = user.Id
                               email = user.Email
                               name = user.Name
                               message = "Login successful"
                           |})
                       else
                           // Invalid password
                           Results.Json(
                               {| message = "Invalid email or password" |},
                               statusCode = 401
                           )
    }
```

**What it does:**
- Authenticates user and returns user info on success

**Workflow breakdown:**

**Step 1: Validate Input**
```fsharp
match validateLogin email password with
| Error validationError -> return Results.BadRequest(validationError)
| Ok (validEmail, validPassword) -> // Continue
```
- Ensures email and password are provided

**Step 2: Find User**
```fsharp
let! userOption = findUserByEmailAsync db validEmail
```
- Looks up user by email
- Returns Option type (Some user or None)

**Step 3: Verify Credentials**
```fsharp
match userOption with
| None -> Results.Json(..., 401)  // User not found
| Some user ->
    if verifyPassword validPassword user.PasswordHash then
        Results.Ok(...)  // Success
    else
        Results.Json(..., 401)  // Wrong password
```

**Security considerations:**
- Same error message for "user not found" and "wrong password"
- Prevents user enumeration attacks
- Attacker can't tell if email exists in system

**Response types:**
- `Results.BadRequest` - 400 (validation error)
- `Results.Ok` - 200 (successful login)
- `Results.Json(..., 401)` - 401 Unauthorized (invalid credentials)

---

## Complete Workflow Examples

### Registration Flow

```
1. User sends POST /api/auth/register
   Body: { "email": "user@example.com", "username": "john", "password": "password123" }

2. Program.fs calls AuthHandler.register(db, email, username, password)

3. validateRegistration(email, username, password)
   → Returns: Ok (email, username, password)

4. emailExistsAsync(db, email)
   → Returns: false (email not taken)

5. createUser(email, username, password)
   → Creates User object
   → Hashes password: "password123" → "XohImNooBHFR0OVvjcYpJ3NgPQ1qq73WKhHvch0VQtg="
   → Returns: User { Id = Guid, Email = "user@example.com", ... }

6. saveUserAsync(db, user)
   → Adds user to database
   → Saves changes
   → Returns: Ok user

7. Results.Created(...)
   → HTTP 201 Created
   → Response: { "id": "...", "email": "user@example.com", "name": "john", "message": "..." }
```

### Login Flow

```
1. User sends POST /api/auth/login
   Body: { "email": "user@example.com", "password": "password123" }

2. Program.fs calls AuthHandler.login(db, email, password)

3. validateLogin(email, password)
   → Returns: Ok (email, password)

4. findUserByEmailAsync(db, email)
   → Queries database
   → Returns: Some user (or None if not found)

5. verifyPassword(password, user.PasswordHash)
   → Hashes "password123" → "XohImNooBHFR0OVvjcYpJ3NgPQ1qq73WKhHvch0VQtg="
   → Compares with stored hash
   → Returns: true (or false)

6. Results.Ok(...)
   → HTTP 200 OK
   → Response: { "userId": "...", "id": "...", "email": "user@example.com", "name": "john", "message": "Login successful" }
```

---

## Key Design Patterns

### 1. Function Composition
Small, focused functions composed into larger operations:
```
validateRegistration → emailExistsAsync → createUser → saveUserAsync → Results
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
```

### 5. Separation of Concerns
- **Pure functions**: Validation, hashing (no side effects)
- **Database functions**: Data access (async)
- **Business logic**: User creation (composes pure functions)
- **Handlers**: HTTP layer (composes everything)

---

## Summary

**AuthHandler** demonstrates:
- ✅ Functional programming principles
- ✅ Type-safe error handling (Result)
- ✅ Null-safe data access (Option)
- ✅ Secure password handling
- ✅ Clear separation of concerns
- ✅ Composable, testable functions

Each function has a single responsibility, making the code easy to understand, test, and maintain.

