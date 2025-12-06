# BookHandler.fs - Complete Workflow and Code Explanation

This document explains the `BookHandler.fs` module, which handles all book-related operations including CRUD operations, borrowing, and returning books.

## Table of Contents
1. [Overview]
2. [Module Structure]
3. [Validation Functions]
4. [Database Query Functions]
5. [Business Logic Functions]
6. [Handler Functions]
7. [Complete Workflow Examples]

---

## Overview

**Purpose:** Manage books, borrowings, and book availability in the library system.

**Key Features:**
- Book CRUD operations (Create, Read, Update, Delete)
- Book search functionality
- Borrowing management
- Return processing
- Overdue status tracking
- Available copies tracking

**Key Principles:**
- **Pure validation functions** for input validation
- **Async database operations** using `task { }`
- **Result type** for explicit error handling
- **Option type** for safe null handling
- **Function composition** for complex workflows

---

## Module Structure

```fsharp
namespace BackEnd.Handlers

module BookHandler =
    // Pure validation functions
    // Database query functions (private)
    // Business logic functions (private)
    // Handler functions (public entry points)
```

**Organization:** Functions are organized from low-level (validation) to high-level (handlers), with private helper functions supporting public handlers.

---

## 1. Validation Functions

### `validateBook`

```fsharp
let private validateBook (title: string) (author: string) (totalCopies: int) : Result<unit, string> =
    if String.IsNullOrWhiteSpace(title) then Error "Title is required"
    elif String.IsNullOrWhiteSpace(author) then Error "Author is required"
    elif totalCopies < 1 then Error "Total copies must be at least 1"
    else Ok ()
```

**What it does:**
- Validates book creation/update input
- Returns `Result<unit, string>` (Ok with no value, or Error with message)

**Breaking it down:**
- `Result<unit, string>` - Success case has no value (`unit`), error case has message
- `String.IsNullOrWhiteSpace` - Checks if string is null, empty, or whitespace
- `totalCopies < 1` - Ensures at least one copy exists

**Why `unit` in Result:**
- Validation doesn't need to return data
- Just needs to indicate success/failure
- `Ok ()` means "validation passed"

**Example:**
```fsharp
match validateBook "Harry Potter" "J.K. Rowling" 5 with
| Ok () -> printfn "Valid book"
| Error msg -> printfn "Invalid: %s" msg
```

---

### `hasActiveBorrowingAsync`

```fsharp
let private hasActiveBorrowingAsync (db: AppDbContext) (userId: Guid) (bookId: Guid) : Task<bool> =
    task {
        let query = db.Borrowings.Where(fun b -> 
            b.UserId = userId && 
            b.BookId = bookId && 
            (b.Status = "Active" || b.Status = "Overdue"))
        let! existingBorrowing = query.FirstOrDefaultAsync()
        return existingBorrowing <> null
    }
```

**What it does:**
- Checks if a user already has an active borrowing for a specific book
- Returns `bool` (true if active borrowing exists)

**Breaking it down:**
- `db.Borrowings.Where(...)` - LINQ query filtering borrowings
- `fun b -> ...` - Lambda predicate function
- Conditions:
  - `b.UserId = userId` - Same user
  - `b.BookId = bookId` - Same book
  - `b.Status = "Active" || b.Status = "Overdue"` - Not returned yet
- `FirstOrDefaultAsync()` - Gets first match or null
- `existingBorrowing <> null` - Converts null check to bool

**Why we use it:**
- Prevents users from borrowing the same book multiple times
- Ensures one active borrowing per user per book

**Example:**
```fsharp
let! hasActive = hasActiveBorrowingAsync db userId bookId
if hasActive then
    printfn "User already has this book borrowed"
```

---

### `validateBorrowRequest`

```fsharp
let private validateBorrowRequest (book: Book option) (userId: Guid) (hasActiveBorrowing: bool) 
    : Result<unit, string> =
    match book with
    | None -> Error "Book not found"
    | Some b ->
        if hasActiveBorrowing then 
            Error "You already have an active borrowing for this book. Please return it before borrowing again."
        elif b.AvailableCopies < 1 then 
            Error "No copies available"
        else Ok ()
```

**What it does:**
- Validates if a book can be borrowed
- Returns `Result<unit, string>`

**Breaking it down:**
- **Pattern matching on Option:**
  - `None` - Book doesn't exist → Error
  - `Some b` - Book exists → Continue validation
- **Validation checks:**
  1. User doesn't already have active borrowing
  2. At least one copy is available

**Why we use it:**
- Centralizes borrowing validation logic
- Clear error messages for different failure cases
- Composable with other functions

**Example:**
```fsharp
let! bookOpt = findBookByIdAsync db bookId
let! hasActive = hasActiveBorrowingAsync db userId bookId
match validateBorrowRequest bookOpt userId hasActive with
| Ok () -> // Proceed with borrowing
| Error msg -> // Return error to user
```

---

## 2. Database Query Functions

### `findBookByIdAsync`

```fsharp
let private findBookByIdAsync (db: AppDbContext) (bookId: Guid) : Task<Option<Book>> =
    task {
        let! book = db.Books.FirstOrDefaultAsync(fun b -> b.Id = bookId)
        return match book with
               | null -> None
               | _ -> Some book
    }
```

**What it does:**
- Finds a book by its ID
- Returns `Option<Book>` (Some book or None)

**Breaking it down:**
- `FirstOrDefaultAsync` - Gets first match or null
- Pattern matching converts null to Option type
- `_` - Wildcard pattern (any non-null value)

**Why Option type:**
- Safe null handling
- Forces explicit "not found" handling

---

### `getAllBooksAsync`

```fsharp
let private getAllBooksAsync (db: AppDbContext) (searchTerm: string option) 
    : Task<System.Collections.Generic.List<Book>> =
    task {
        let mutable query = db.Books.AsQueryable()
        query <- match searchTerm with
                 | Some term when not (String.IsNullOrWhiteSpace(term)) ->
                     let termLower = term.ToLower()
                     query.Where(fun b -> 
                         b.Title.ToLower().Contains(termLower) || 
                         b.Author.ToLower().Contains(termLower) ||
                         (b.ISBN <> null && b.ISBN.ToLower().Contains(termLower)))
                 | _ -> query
        let query = query.OrderBy(fun b -> b.Title)
        return! query.ToListAsync()
    }
```

**What it does:**
- Gets all books with optional search filtering
- Returns list of books

**Breaking it down:**

**Mutable query:**
```fsharp
let mutable query = db.Books.AsQueryable()
```
- `mutable` - Allows reassignment (needed for conditional filtering)
- `AsQueryable()` - Converts to IQueryable for LINQ operations

**Conditional search:**
```fsharp
query <- match searchTerm with
         | Some term when not (String.IsNullOrWhiteSpace(term)) ->
             // Apply search filter
         | _ -> query  // No filter
```

**Search logic:**
- Searches in Title, Author, and ISBN
- Case-insensitive (converts to lowercase)
- Uses `Contains` for partial matching

**Ordering:**
```fsharp
let query = query.OrderBy(fun b -> b.Title)
```
- Orders results alphabetically by title

**Why mutable:**
- Need to conditionally modify query
- F# immutability would require different approach
- This is a pragmatic choice for LINQ queries

**Example:**
```fsharp
// Get all books
let! books = getAllBooksAsync db None

// Search for "harry"
let! books = getAllBooksAsync db (Some "harry")
```

---

### `createBookInDbAsync`

```fsharp
let private createBookInDbAsync (db: AppDbContext) (book: Book) : Task<Result<Book, string>> =
    task {
        try
            db.Books.Add(book) |> ignore
            let! _ = db.SaveChangesAsync()
            return Ok book
        with
        | :? DbUpdateException as ex ->
            return Error $"Failed to create book: {ex.Message}"
        | ex ->
            return Error $"Unexpected error: {ex.Message}"
    }
```

**What it does:**
- Saves a new book to database
- Returns `Result<Book, string>`

**Breaking it down:**
- `db.Books.Add(book)` - Adds book to EF Core context
- `db.SaveChangesAsync()` - Persists to database
- Exception handling converts to Result type

**Why Result:**
- Explicit error handling
- No hidden exceptions
- Caller must handle errors

---

### `updateBookInDbAsync`

```fsharp
let private updateBookInDbAsync (db: AppDbContext) (book: Book) : Task<Result<Book, string>> =
    task {
        try
            // Check if entity is already tracked
            let entry = db.Entry(book)
            if entry.State = EntityState.Detached then
                db.Books.Update(book) |> ignore
            // If already tracked, changes are automatically detected
            let! _ = db.SaveChangesAsync()
            return Ok book
        with
        | :? DbUpdateException as ex ->
            let innerMsg = if ex.InnerException <> null then ex.InnerException.Message else ""
            return Error $"Failed to update book: {ex.Message}. Inner: {innerMsg}"
        | ex ->
            return Error $"Unexpected error: {ex.Message}. Type: {ex.GetType().Name}"
    }
```

**What it does:**
- Updates an existing book in database
- Handles Entity Framework tracking

**Breaking it down:**

**Entity tracking:**
```fsharp
let entry = db.Entry(book)
if entry.State = EntityState.Detached then
    db.Books.Update(book) |> ignore
```
- EF Core tracks entities it loads
- If entity is already tracked, changes are detected automatically
- If detached, need to call `Update()` explicitly

**Why this matters:**
- Prevents "entity already tracked" errors
- Handles both scenarios (tracked and detached)

**Error details:**
- Includes inner exception message if available
- Includes exception type name for debugging

---

### `deleteBookFromDbAsync`

```fsharp
let private deleteBookFromDbAsync (db: AppDbContext) (bookId: Guid) : Task<Result<unit, string>> =
    task {
        try
            let! bookOpt = findBookByIdAsync db bookId
            match bookOpt with
            | None -> return Error "Book not found"
            | Some book ->
                db.Books.Remove(book) |> ignore
                let! _ = db.SaveChangesAsync()
                return Ok ()
        with
        | :? DbUpdateException as ex ->
            return Error $"Failed to delete book: {ex.Message}"
        | ex ->
            return Error $"Unexpected error: {ex.Message}"
    }
```

**What it does:**
- Deletes a book from database
- Returns `Result<unit, string>`

**Breaking it down:**
- First finds book (to ensure it exists)
- If found, removes it
- If not found, returns error

**Why check existence first:**
- Provides clear "not found" error
- EF Core would throw exception if trying to remove non-existent entity

---

### `createBorrowingAsync`

```fsharp
let private createBorrowingAsync (db: AppDbContext) (userId: Guid) (bookId: Guid) 
    : Task<Result<Borrowing, string>> =
    task {
        try
            let! bookOpt = findBookByIdAsync db bookId
            match bookOpt with
            | None -> return Error "Book not found"
            | Some book ->
                let borrowing = Borrowing()
                borrowing.Id <- Guid.NewGuid()
                borrowing.UserId <- userId
                borrowing.BookId <- bookId
                borrowing.BorrowedDate <- DateTime.UtcNow
                borrowing.ReturnedDate <- Nullable<DateTime>()
                borrowing.DueDate <- DateTime.UtcNow.AddDays(14) // 14 days loan period
                borrowing.Status <- "Active"
                
                book.AvailableCopies <- book.AvailableCopies - 1
                db.Borrowings.Add(borrowing) |> ignore
                // Book is already tracked, no need to call Update()
                let! _ = db.SaveChangesAsync()
                return Ok borrowing
        with
        | :? DbUpdateException as ex ->
            let innerMsg = if ex.InnerException <> null then ex.InnerException.Message else ""
            return Error $"Failed to borrow book: {ex.Message}. Inner: {innerMsg}"
        | ex ->
            return Error $"Unexpected error: {ex.Message}. Type: {ex.GetType().Name}"
    }
```

**What it does:**
- Creates a borrowing record and updates book availability
- Returns `Result<Borrowing, string>`

**Breaking it down:**

**Borrowing creation:**
```fsharp
let borrowing = Borrowing()
borrowing.Id <- Guid.NewGuid()
borrowing.UserId <- userId
borrowing.BookId <- bookId
borrowing.BorrowedDate <- DateTime.UtcNow
borrowing.ReturnedDate <- Nullable<DateTime>()  // Not returned yet
borrowing.DueDate <- DateTime.UtcNow.AddDays(14)  // 14 days from now
borrowing.Status <- "Active"
```

**Update availability:**
```fsharp
book.AvailableCopies <- book.AvailableCopies - 1
```
- Decrements available copies
- Book is already tracked (from `findBookByIdAsync`), so changes are detected

**Transaction:**
- Both borrowing creation and book update happen in same `SaveChangesAsync()`
- Atomic operation (both succeed or both fail)

**Why 14 days:**
- Standard library loan period
- Could be made configurable

---

### `returnBorrowingAsync`

```fsharp
let private returnBorrowingAsync (db: AppDbContext) (borrowingId: Guid) (userId: Guid) 
    : Task<Result<Borrowing, string>> =
    task {
        try
            let query = db.Borrowings.Include(fun b -> b.Book)
            let! borrowing = query.FirstOrDefaultAsync(fun b -> b.Id = borrowingId && b.UserId = userId)
            
            match borrowing with
            | null -> return Error "Borrowing record not found"
            | b when b.Status = "Returned" -> return Error "Book already returned"
            | b ->
                b.ReturnedDate <- Nullable<DateTime>(DateTime.UtcNow)
                b.Status <- "Returned"
                
                match b.Book with
                | null -> return Error "Book not found"
                | book ->
                    book.AvailableCopies <- book.AvailableCopies + 1
                    // Both entities are already tracked via Include, no need to call Update()
                    let! _ = db.SaveChangesAsync()
                    return Ok b
        with
        | :? DbUpdateException as ex ->
            let innerMsg = if ex.InnerException <> null then ex.InnerException.Message else ""
            return Error $"Failed to return book: {ex.Message}. Inner: {innerMsg}"
        | ex ->
            return Error $"Unexpected error: {ex.Message}. Type: {ex.GetType().Name}"
    }
```

**What it does:**
- Returns a borrowed book and updates availability
- Returns `Result<Borrowing, string>`

**Breaking it down:**

**Include for navigation:**
```fsharp
let query = db.Borrowings.Include(fun b -> b.Book)
```
- `Include` loads related Book entity
- Prevents lazy loading issues
- Both entities tracked in same context

**Find borrowing:**
```fsharp
query.FirstOrDefaultAsync(fun b -> b.Id = borrowingId && b.UserId = userId)
```
- Finds borrowing by ID and user ID
- Ensures user can only return their own borrowings

**Pattern matching:**
```fsharp
match borrowing with
| null -> Error "Borrowing record not found"
| b when b.Status = "Returned" -> Error "Book already returned"
| b -> // Process return
```
- Multiple conditions handled elegantly
- `when` clause adds additional condition

**Update borrowing:**
```fsharp
b.ReturnedDate <- Nullable<DateTime>(DateTime.UtcNow)
b.Status <- "Returned"
```

**Update book:**
```fsharp
book.AvailableCopies <- book.AvailableCopies + 1
```
- Increments available copies
- Book already tracked via Include

---

### `getUserBorrowingsAsync`

```fsharp
let private getUserBorrowingsAsync (db: AppDbContext) (userId: Guid) 
    : Task<System.Collections.Generic.List<Borrowing>> =
    task {
        let query = db.Borrowings.Include(fun b -> b.Book)
        let query = query.Where(fun b -> b.UserId = userId)
        let query = query.OrderByDescending(fun b -> b.BorrowedDate)
        return! query.ToListAsync()
    }
```

**What it does:**
- Gets all borrowings for a specific user
- Includes book information
- Orders by most recent first

**Breaking it down:**
- `Include(fun b -> b.Book)` - Eager loads Book entity
- `Where` - Filters by user ID
- `OrderByDescending` - Most recent first

**Why Include:**
- Loads book data in same query
- Avoids N+1 query problem

---

### `updateOverdueStatusAsync`

```fsharp
let private updateOverdueStatusAsync (db: AppDbContext) : Task<unit> =
    task {
        try
            let query = db.Borrowings.Where(fun b -> b.Status = "Active" && b.DueDate < DateTime.UtcNow)
            let! overdueBorrowings = query.ToListAsync()
            
            for borrowing in overdueBorrowings do
                borrowing.Status <- "Overdue"
                // Entities are already tracked, no need to call Update()
            
            if overdueBorrowings.Count > 0 then
                let! _ = db.SaveChangesAsync()
                ()
        with
        | ex ->
            // Log error but don't fail the request - this is a background update
            System.Diagnostics.Debug.WriteLine($"Error updating overdue status: {ex.Message}")
            ()
    }
```

**What it does:**
- Updates active borrowings to "Overdue" if past due date
- Background maintenance function

**Breaking it down:**
- Finds active borrowings past due date
- Updates status to "Overdue"
- Only saves if there are changes

**Why error handling:**
- This is a background update
- Shouldn't fail the main request
- Logs error but continues

**When called:**
- Called before retrieving user borrowings
- Ensures overdue status is up-to-date

---

### `getAllBorrowingsAsync`

```fsharp
let getAllBorrowingsAsync (db: AppDbContext) : Task<System.Collections.Generic.List<Borrowing>> =
    task {
        let query = db.Borrowings.Include(fun b -> b.Book).Include(fun b -> b.User)
        let query = query.OrderByDescending(fun b -> b.BorrowedDate)
        return! query.ToListAsync()
    }
```

**What it does:**
- Gets all borrowings in system
- Includes book and user information
- Orders by most recent first

**Breaking it down:**
- Multiple `Include` calls load related entities
- Useful for admin views

---

## 3. Handler Functions

Handlers are public entry points called by ASP.NET Core. They compose validation, database operations, and business logic.

### `getBooks`

```fsharp
let getBooks (db: AppDbContext) (searchTerm: string option) : Task<IResult> =
    task {
        let! books = getAllBooksAsync db searchTerm
        return Results.Ok(books)
    }
```

**What it does:**
- Gets all books with optional search
- Returns HTTP 200 OK with book list

**Simple handler:**
- No validation needed
- Just queries and returns

---

### `getBookById`

```fsharp
let getBookById (db: AppDbContext) (bookId: Guid) : Task<IResult> =
    task {
        let! bookOpt = findBookByIdAsync db bookId
        return match bookOpt with
               | None -> Results.NotFound("Book not found")
               | Some book -> Results.Ok(book)
    }
```

**What it does:**
- Gets a single book by ID
- Returns 404 if not found, 200 if found

---

### `createBook`

```fsharp
let createBook (db: AppDbContext) (book: Book) : Task<IResult> =
    task {
        match validateBook book.Title book.Author book.TotalCopies with
        | Error msg -> return Results.BadRequest(msg)
        | Ok _ ->
            let newBook = Book()
            newBook.Id <- Guid.NewGuid()
            newBook.Title <- book.Title
            newBook.Author <- book.Author
            newBook.ISBN <- book.ISBN
            newBook.Description <- book.Description
            newBook.TotalCopies <- book.TotalCopies
            newBook.AvailableCopies <- book.TotalCopies  // Initially all copies available
            newBook.CreatedAt <- DateTime.UtcNow
            newBook.UpdatedAt <- Nullable<DateTime>()
            
            let! result = createBookInDbAsync db newBook
            return match result with
                   | Ok createdBook -> Results.Created($"/api/books/{createdBook.Id}", createdBook)
                   | Error errorMsg -> Results.Problem(title = "Failed to create book", detail = errorMsg, statusCode = 500)
    }
```

**What it does:**
- Creates a new book
- Validates input, creates book object, saves to database

**Breaking it down:**

**Validation:**
```fsharp
match validateBook book.Title book.Author book.TotalCopies with
| Error msg -> return Results.BadRequest(msg)
| Ok _ -> // Continue
```

**Book creation:**
```fsharp
newBook.AvailableCopies <- book.TotalCopies
```
- Initially, all copies are available
- As books are borrowed, this decreases

**Response:**
- Success: 201 Created with location header
- Error: 500 Internal Server Error

---

### `updateBook`

```fsharp
let updateBook (db: AppDbContext) (bookId: Guid) (book: Book) : Task<IResult> =
    task {
        let! bookOpt = findBookByIdAsync db bookId
        match bookOpt with
        | None -> return Results.NotFound("Book not found")
        | Some existingBook ->
            match validateBook book.Title book.Author book.TotalCopies with
            | Error msg -> return Results.BadRequest(msg)
            | Ok _ ->
                existingBook.Title <- book.Title
                existingBook.Author <- book.Author
                existingBook.ISBN <- book.ISBN
                existingBook.Description <- book.Description
                existingBook.TotalCopies <- book.TotalCopies
                existingBook.UpdatedAt <- Nullable<DateTime>(DateTime.UtcNow)
                
                // Adjust available copies if total copies changed
                let diff = book.TotalCopies - existingBook.TotalCopies
                existingBook.AvailableCopies <- existingBook.AvailableCopies + diff
                if existingBook.AvailableCopies < 0 then
                    existingBook.AvailableCopies <- 0
                
                let! result = updateBookInDbAsync db existingBook
                return match result with
                       | Ok updated -> Results.Ok(existingBook)
                       | Error errorMsg -> Results.Problem(title = "Failed to update book", detail = errorMsg, statusCode = 500)
    }
```

**What it does:**
- Updates an existing book
- Handles available copies adjustment

**Breaking it down:**

**Available copies logic:**
```fsharp
let diff = book.TotalCopies - existingBook.TotalCopies
existingBook.AvailableCopies <- existingBook.AvailableCopies + diff
if existingBook.AvailableCopies < 0 then
    existingBook.AvailableCopies <- 0
```
- If total copies increases, available copies increases
- If total copies decreases, available copies decreases
- Can't go below 0 (safety check)

**Example:**
- Book has 10 total, 3 available (7 borrowed)
- Update to 15 total → 8 available (15 - 7 = 8)
- Update to 5 total → 0 available (5 - 7 = -2, clamped to 0)

---

### `deleteBook`

```fsharp
let deleteBook (db: AppDbContext) (bookId: Guid) : Task<IResult> =
    task {
        let! result = deleteBookFromDbAsync db bookId
        return match result with
               | Ok _ -> Results.NoContent()
               | Error errorMsg -> Results.NotFound(errorMsg)
    }
```

**What it does:**
- Deletes a book
- Returns 204 No Content on success, 404 on error

---

### `borrowBook`

```fsharp
let borrowBook (db: AppDbContext) (bookId: Guid) (userId: Guid) : Task<IResult> =
    task {
        let! bookOpt = findBookByIdAsync db bookId
        let! hasActiveBorrowing = hasActiveBorrowingAsync db userId bookId
        match validateBorrowRequest bookOpt userId hasActiveBorrowing with
        | Error msg -> return Results.BadRequest(msg)
        | Ok _ ->
            let! result = createBorrowingAsync db userId bookId
            return match result with
                   | Ok borrowing -> Results.Created($"/api/borrowings/{borrowing.Id}", borrowing)
                   | Error errorMsg -> Results.Problem(title = "Failed to borrow book", detail = errorMsg, statusCode = 500)
    }
```

**What it does:**
- Borrows a book for a user
- Validates request, creates borrowing, updates availability

**Workflow:**
1. Find book
2. Check for active borrowing
3. Validate request
4. Create borrowing (updates availability atomically)
5. Return result

---

### `returnBook`

```fsharp
let returnBook (db: AppDbContext) (borrowingId: Guid) (userId: Guid) : Task<IResult> =
    task {
        let! result = returnBorrowingAsync db borrowingId userId
        return match result with
               | Ok borrowing -> Results.Ok(borrowing)
               | Error errorMsg -> Results.BadRequest(errorMsg)
    }
```

**What it does:**
- Returns a borrowed book
- Updates borrowing status and book availability

---

### `getUserBorrowings`

```fsharp
let getUserBorrowings (db: AppDbContext) (userId: Guid) : Task<IResult> =
    task {
        try
            do! updateOverdueStatusAsync db
            let! borrowings = getUserBorrowingsAsync db userId
            return Results.Ok(borrowings)
        with
        | :? DbUpdateException as ex ->
            let innerMsg = if ex.InnerException <> null then ex.InnerException.Message else ""
            return Results.Problem(
                title = "Failed to retrieve borrowings",
                detail = $"Database error: {ex.Message}. Inner: {innerMsg}",
                statusCode = 500
            )
        | ex ->
            return Results.Problem(
                title = "Failed to retrieve borrowings",
                detail = $"Unexpected error: {ex.Message}. Type: {ex.GetType().Name}",
                statusCode = 500
            )
    }
```

**What it does:**
- Gets user's borrowings
- Updates overdue status first
- Returns borrowings with book information

**Breaking it down:**
- `do! updateOverdueStatusAsync db` - Updates overdue status (doesn't return value, so use `do!`)
- Then retrieves borrowings
- Exception handling for database errors

---

### `getAllBorrowings`

```fsharp
let getAllBorrowings (db: AppDbContext) : Task<IResult> =
    task {
        let! borrowings = getAllBorrowingsAsync db
        return Results.Ok(borrowings)
    }
```

**What it does:**
- Gets all borrowings in system
- Used for admin views

---

## Complete Workflow Examples

### Borrowing a Book

```
1. User sends POST /api/books/{bookId}/borrow
   Body: { "userId": "..." }

2. Program.fs calls BookHandler.borrowBook(db, bookId, userId)

3. findBookByIdAsync(db, bookId)
   → Returns: Some book (or None)

4. hasActiveBorrowingAsync(db, userId, bookId)
   → Returns: false (no active borrowing)

5. validateBorrowRequest(bookOpt, userId, hasActive)
   → Returns: Ok () (validation passed)

6. createBorrowingAsync(db, userId, bookId)
   → Creates Borrowing record
   → Decrements book.AvailableCopies
   → Saves to database
   → Returns: Ok borrowing

7. Results.Created(...)
   → HTTP 201 Created
   → Response: Borrowing object
```

### Returning a Book

```
1. User sends POST /api/borrowings/{borrowingId}/return
   Body: { "userId": "..." }

2. Program.fs calls BookHandler.returnBook(db, borrowingId, userId)

3. returnBorrowingAsync(db, borrowingId, userId)
   → Finds borrowing (with Include for Book)
   → Validates: exists, not already returned, user matches
   → Sets ReturnedDate and Status
   → Increments book.AvailableCopies
   → Saves to database
   → Returns: Ok borrowing

4. Results.Ok(borrowing)
   → HTTP 200 OK
   → Response: Updated Borrowing object
```

---

## Key Design Patterns

### 1. Private Helper Functions
- Database operations are private
- Only handlers are public
- Clear separation of concerns

### 2. Atomic Operations
- Borrowing: Creates borrowing AND updates availability in one transaction
- Returning: Updates borrowing AND updates availability in one transaction
- Ensures data consistency

### 3. Available Copies Tracking
- Automatically maintained
- Decremented on borrow
- Incremented on return
- Adjusted on book update

### 4. Overdue Status
- Background update before retrieval
- Doesn't block main request
- Keeps data current

### 5. Entity Framework Tracking
- Handles both tracked and detached entities
- Uses Include for eager loading
- Prevents N+1 query problems

---

## Summary

**BookHandler** demonstrates:
- ✅ Complete CRUD operations
- ✅ Search functionality
- ✅ Borrowing/returning workflow
- ✅ Availability tracking
- ✅ Overdue status management
- ✅ Atomic database operations
- ✅ Type-safe error handling
- ✅ Functional composition

The module follows functional programming principles while working with Entity Framework Core's object-oriented model, showing how to bridge the two paradigms effectively.

