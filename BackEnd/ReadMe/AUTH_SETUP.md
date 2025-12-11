<!-- Role: Documentation Lead -->
<!-- Developer: Omar Salama -->
# Authentication Setup Guide

## Overview

Authentication has been added to the backend with the following features:
- User registration with password hashing
- User login with password verification
- User ID changed from `int` to `Guid`
- Password field added to User model

## Changes Made

### 1. User Model Updates (`BackEnd.Data/Models/User.cs`)

**Changes:**
- `Id` changed from `int` to `Guid`
- Added `PasswordHash` field (string, required)

**Updated Model:**
```csharp
public class User
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
}
```

### 2. Database Context Updates (`BackEnd.Data/AppDbContext.cs`)

**Changes:**
- Added `PasswordHash` property configuration
- Added unique index on `Email` to prevent duplicate registrations

### 3. New AuthHandler (`BackEnd/Handlers/AuthHandler.fs`)

Created a new handler module with:

#### Functions:

1. **Password Hashing**
   - `hashPassword` - Hashes passwords using SHA256
   - `verifyPassword` - Verifies password against hash

2. **Validation**
   - `validateRegistration` - Validates email, username, and password
   - `validateLogin` - Validates email and password

3. **Database Queries**
   - `findUserByEmailAsync` - Finds user by email (returns Option)
   - `emailExistsAsync` - Checks if email already exists

4. **Business Logic**
   - `createUser` - Creates user with hashed password (takes email, username, password)
   - `saveUserAsync` - Saves user to database (returns Result)

5. **Handlers**
   - `register` - Registration endpoint handler
   - `login` - Login endpoint handler

### 4. API Endpoints (`BackEnd/Program.fs`)

**New Endpoints:**
- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login user

**Updated Endpoints:**
- `GET /api/users/{id:guid}` - Changed from `{id:int}` to `{id:guid}`

## API Usage

### Register User

**Endpoint:** `POST /api/auth/register`

**Parameters:**
- `email` (string) - User's email address
- `username` (string) - User's username (at least 3 characters)
- `password` (string) - User's password (at least 6 characters)

**Example Request:**
```json
{
  "email": "user@example.com",
  "username": "johndoe",
  "password": "securepassword123"
}
```

**Success Response (201 Created):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "name": "johndoe",
  "message": "User registered successfully"
}
```

**Error Responses:**
- `400 Bad Request` - Validation errors (missing fields, invalid email, etc.)
- `500 Internal Server Error` - Database errors

### Login User

**Endpoint:** `POST /api/auth/login`

**Parameters:**
- `email` (string) - User's email address
- `password` (string) - User's password

**Example Request:**
```json
{
  "email": "user@example.com",
  "password": "securepassword123"
}
```

**Success Response (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "name": "johndoe",
  "message": "Login successful"
}
```

**Error Responses:**
- `400 Bad Request` - Missing email or password
- `401 Unauthorized` - Invalid email or password

## Password Security

### Current Implementation
- Uses SHA256 hashing (suitable for development)
- Passwords are hashed before storing in database
- Original passwords are never stored

### Production Recommendations
For production, consider using:
- **BCrypt** - More secure, slower (prevents brute force)
- **Argon2** - Modern, secure password hashing
- **PBKDF2** - NIST recommended

**To upgrade to BCrypt:**
1. Install package: `dotnet add package BCrypt.Net-Next`
2. Update `hashPassword` and `verifyPassword` functions

## Database Migration

Since the User model has changed, you need to create a new migration:

```bash
cd BackEnd.Data
dotnet ef migrations add AddPasswordHashAndGuidId --startup-project ..\BackEnd\BackEnd.fsproj
dotnet ef database update --startup-project ..\BackEnd\BackEnd.fsproj
```

**Note:** This migration will:
- Change `Id` from `int` to `Guid`
- Add `PasswordHash` column
- Add unique index on `Email`
- **⚠️ This will require data migration if you have existing users**

## Functional Programming Features

The AuthHandler follows F# functional programming best practices:

1. **Option Types** - Safe handling of nullable values
   ```fsharp
   let! userOption = findUserByEmailAsync db email
   match userOption with
   | Some user -> ...
   | None -> ...
   ```

2. **Result Types** - Explicit error handling
   ```fsharp
   match validateRegistration email username password with
   | Error msg -> Results.BadRequest(msg)
   | Ok (email, username, password) -> ...
   ```

3. **Pure Functions** - Validation functions with no side effects
   ```fsharp
   let validateRegistration (email: string) (username: string) (password: string) = ...
   ```

4. **Function Composition** - Building complex from simple
   ```fsharp
   validateRegistration → checkEmailExists → createUser → saveUserAsync
   ```

5. **Pattern Matching** - Expressive control flow
   ```fsharp
   match result with
   | Ok value -> success
   | Error msg -> failure
   ```

## Testing the Endpoints

### Using Swagger UI
1. Run the application: `dotnet run`
2. Navigate to: `https://localhost:5001/swagger`
3. Test the `/api/auth/register` and `/api/auth/login` endpoints

### Using cURL

**Register:**
```bash
curl -X POST "https://localhost:5001/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","username":"testuser","password":"password123"}'
```

**Login:**
```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"password123"}'
```

## Next Steps

1. **Create Migration** - Run the migration commands above
2. **Test Registration** - Register a new user
3. **Test Login** - Login with registered credentials
4. **Add JWT Tokens** - For production, add JWT token generation after successful login
5. **Add Authorization** - Protect endpoints with authentication middleware

## Security Considerations

1. **Password Hashing** - Currently using SHA256 (upgrade to BCrypt for production)
2. **HTTPS** - Always use HTTPS in production
3. **Rate Limiting** - Add rate limiting to prevent brute force attacks
4. **JWT Tokens** - Add token-based authentication for stateless sessions
5. **Password Requirements** - Consider stronger password requirements
6. **Email Verification** - Add email verification for new registrations

## File Structure

```
BackEnd/
├── Handlers/
│   ├── Handlers.fs          # User/product handlers
│   └── AuthHandler.fs       # Authentication handlers (NEW)
├── Program.fs               # Updated with auth endpoints
└── BackEnd.fsproj           # Updated to include AuthHandler.fs

BackEnd.Data/
├── Models/
│   └── User.cs              # Updated with Guid Id and PasswordHash
└── AppDbContext.cs          # Updated with PasswordHash config
```

## Summary

✅ User model updated with `Guid` ID and `PasswordHash`  
✅ AuthHandler created with register and login functions  
✅ Password hashing implemented (SHA256)  
✅ Validation functions for registration and login  
✅ API endpoints added for authentication  
✅ Functional programming patterns used throughout  
✅ Email uniqueness enforced in database  

The authentication system is ready to use! Remember to create the database migration before testing.

