<!-- Role: Documentation Lead -->
<!-- Developer: Omar Salama -->
# GitHub Push Sequence Guide

This document outlines the recommended sequence for pushing code to GitHub, organized by role and dependencies.

## Push Sequence Overview

The project should be pushed in the following order to ensure dependencies are met:

1. **Storage Layer** (Foundation)
2. **Backend Models** (Data Structures)
3. **Backend Handlers** (Business Logic)
4. **Backend Configuration** (Application Setup)
5. **Frontend API Client** (Integration Layer)
6. **Frontend UI Components** (Reusable Components)
7. **Frontend Pages** (Application Pages)
8. **Tests** (Quality Assurance)
9. **Documentation** (Can be pushed anytime)

---

## Phase 1: Storage & Data Layer

### 👤 **Role: Storage Developer**
### 👨‍💻 **Developer: Youssef Amr**

**Files to Push:**
- `BackEnd.Data/AppDbContext.cs`
- `BackEnd.Data/Models/User.cs`
- `BackEnd.Data/Models/Book.cs`
- `BackEnd.Data/Models/Borrowing.cs`
- `BackEnd.Data/Models/RegisterRequest.cs`
- `BackEnd.Data/Models/LoginRequest.cs`
- `BackEnd.Data/Models/BorrowRequest.cs`
- `BackEnd.Data/Models/ReturnRequest.cs`
- `BackEnd.Data/Migrations/` (all migration files)
- `BackEnd.Data/BackEnd.Data.csproj`

**Commit Message Example:**
```
feat: Add database context and data models

- Add AppDbContext with User, Book, and Borrowing entities
- Add request models for authentication and borrowing
- Add initial migrations
```

**Why First?** This is the foundation layer that all other components depend on.

---

## Phase 2: Backend Models & Authentication

### 👤 **Role: Authentication & Models**
### 👨‍💻 **Developer: Mohamed Emad**

**Files to Push:**
- `BackEnd/Handlers/AuthHandler.fs`
- Any authentication-related utilities

**Commit Message Example:**
```
feat: Add authentication handler with registration and login

- Implement password hashing and verification
- Add user registration and login endpoints
- Include input validation
```

**Why Second?** Authentication is needed before other handlers can use user context.

---

## Phase 3: Backend CRUD Operations

### 👤 **Role: CRUD Developer**
### 👨‍💻 **Developer: Youssef Amr**

**Files to Push:**
- `BackEnd/Handlers/Handlers.fs` (User CRUD)
- `BackEnd/Handlers/BookHandler.fs` (Book CRUD operations only)
  - `getBookById`
  - `createBook`
  - `updateBook`
  - `deleteBook`
  - `getAllBooksAsync` (without search)

**Commit Message Example:**
```
feat: Add CRUD operations for users and books

- Implement user CRUD endpoints
- Implement book CRUD endpoints (create, read, update, delete)
- Add validation and error handling
```

**Why Third?** CRUD operations are the core functionality that other features build upon.

---

## Phase 4: Search Functionality

### 👤 **Role: Search Developer**
### 👨‍💻 **Developer: Alkady**

**Files to Push:**
- `BackEnd/Handlers/BookHandler.fs` (Search functionality)
  - `getAllBooksAsync` (with search parameter)
  - `getBooks` (public handler with search)

**Commit Message Example:**
```
feat: Add book search functionality

- Implement search by title, author, and ISBN
- Add search parameter to getBooks endpoint
- Support case-insensitive search
```

**Why Fourth?** Search builds on top of the CRUD operations.

---

## Phase 5: Borrow/Return Logic

### 👤 **Role: Borrow/Return Logic Developer**
### 👨‍💻 **Developer: Mohamed Seif**

**Files to Push:**
- `BackEnd/Handlers/BookHandler.fs` (Borrow/Return functions)
  - `borrowBook`
  - `returnBook`
  - `createBorrowingAsync`
  - `returnBorrowingAsync`
  - `getUserBorrowings`
  - `getAllBorrowings`
  - `hasActiveBorrowingAsync`
  - `validateBorrowRequest`
  - `updateOverdueStatusAsync`

**Commit Message Example:**
```
feat: Add book borrowing and return functionality

- Implement borrow book endpoint
- Implement return book endpoint
- Add borrowing validation and status tracking
- Add overdue status management
- Add user borrowings retrieval
```

**Why Fifth?** Borrow/Return logic depends on books and users being available.

---

## Phase 6: Backend Application Setup

### 👤 **Role: Application Setup & Configuration**
### 👨‍💻 **Developer: Mohamed Emad**

**Files to Push:**
- `BackEnd/Program.fs`
- `BackEnd/appsettings.json`
- `BackEnd/appsettings.Development.json`
- `BackEnd/BackEnd.fsproj`
- `BackEnd/Properties/launchSettings.json`

**Commit Message Example:**
```
feat: Add application configuration and endpoint registration

- Configure dependency injection
- Register all API endpoints
- Add CORS, Swagger, and JSON serialization
- Set up database connection
```

**Why Sixth?** This ties all the handlers together and exposes them as API endpoints.

---

## Phase 7: Frontend API Client

### 👤 **Role: API Client & Integration**
### 👨‍💻 **Developer: Mohamed Emad**

**Files to Push:**
- `frontend/src/lib/api.ts`
- `frontend/src/lib/token.ts`
- `frontend/src/lib/auth.ts`
- `frontend/src/lib/token-client.ts`
- `frontend/src/app/api/auth/actions.ts`
- `frontend/src/lib/utils.ts`

**Commit Message Example:**
```
feat: Add frontend API client and authentication utilities

- Create API client with all endpoints
- Add token management (server and client)
- Add authentication helpers
- Add utility functions
```

**Why Seventh?** Frontend pages need the API client to communicate with the backend.

---

## Phase 8: Frontend UI Components

### 👤 **Role: UI Developer**
### 👨‍💻 **Developer: Mohamed Emad**

**Files to Push:**
- `frontend/src/components/ui/button.tsx`
- `frontend/src/components/ui/card.tsx`
- `frontend/src/components/ui/input.tsx`
- `frontend/src/components/ui/label.tsx`
- `frontend/src/components/ui/select.tsx`
- `frontend/src/components/ui/dialog.tsx`
- `frontend/src/components/ui/textarea.tsx`
- `frontend/src/components/ui/sonner.tsx`
- `frontend/src/components/nav.tsx`
- `frontend/src/app/layout.tsx`

**Commit Message Example:**
```
feat: Add reusable UI components and navigation

- Add shadcn/ui components (button, card, input, etc.)
- Add navigation component
- Add root layout with global styles
- Configure toast notifications
```

**Why Eighth?** Pages need these reusable components to build the UI.

---

## Phase 9: Frontend Pages

### 👤 **Role: UI Developer**
### 👨‍💻 **Developer: Mohamed Emad**

**Files to Push (in this order):**

1. **Authentication Pages:**
   - `frontend/src/app/login/page.tsx`
   - `frontend/src/app/register/page.tsx`

2. **Public Pages:**
   - `frontend/src/app/page.tsx` (Home)
   - `frontend/src/app/about/page.tsx`
   - `frontend/src/app/contact/page.tsx`

3. **Feature Pages:**
   - `frontend/src/app/books/page.tsx`
   - `frontend/src/app/profile/page.tsx`
   - `frontend/src/app/admin/page.tsx`

**Commit Message Examples:**

```
feat: Add authentication pages (login and register)

- Create login page with form validation
- Create register page with password confirmation
- Add error handling and user feedback
```

```
feat: Add public pages (home, about, contact)

- Create home page with hero section
- Add about page with team information
- Add contact page with form
```

```
feat: Add feature pages (books, profile, admin)

- Create books browsing page with search
- Add user profile page with borrowings
- Add admin dashboard for book management
```

**Why Ninth?** Pages use the API client and UI components to create the full application.

---

## Phase 10: Tests

### 👤 **Role: Tester**
### 👨‍💻 **Developer: Amr**

**Files to Push:**
- `BackEnd.Tests/Tests.fs`
- `BackEnd.Tests/SeleniumTests/LoginTests.fs`
- `BackEnd.Tests/BackEnd.Tests.fsproj`

**Commit Message Example:**
```
test: Add unit and integration tests

- Add unit tests for authentication handlers
- Add Selenium tests for login functionality
- Test invalid credentials and validation errors
```

**Why Tenth?** Tests verify that all the implemented features work correctly.

---

## Phase 11: Documentation

### 👤 **Role: Documentation Lead**
### 👨‍💻 **Developer: Omar Salama**

**Files to Push:**
- `BackEnd/README.md`
- `BackEnd/ARCHITECTURE.md`
- `BackEnd/ReadMe/` (all documentation files)
- `ReadMe/` (all documentation files)
- `frontend/README.md`
- `GITHUB_PUSH_SEQUENCE.md` (this file)

**Commit Message Example:**
```
docs: Add comprehensive project documentation

- Add architecture documentation
- Add handler documentation
- Add setup and migration guides
- Add GitHub push sequence guide
```

**Why Last?** Documentation describes the completed system, but can be updated anytime.

---

## Quick Reference: Push Order Summary

| Phase | Role | Developer | Key Files |
|-------|------|-----------|-----------|
| 1 | Storage Developer | Youssef Amr | AppDbContext, Models, Migrations |
| 2 | Authentication | Mohamed Emad | AuthHandler.fs |
| 3 | CRUD Developer | Youssef Amr | Handlers.fs, BookHandler.fs (CRUD) |
| 4 | Search Developer | Alkady | BookHandler.fs (Search) |
| 5 | Borrow/Return Logic | Mohamed Seif | BookHandler.fs (Borrow/Return) |
| 6 | Application Setup | Mohamed Emad | Program.fs, Configuration |
| 7 | API Client | Mohamed Emad | api.ts, token utilities |
| 8 | UI Components | Mohamed Emad | UI components, nav, layout |
| 9 | UI Pages | Mohamed Emad | All page.tsx files |
| 10 | Tester | Amr | Test files |
| 11 | Documentation | Omar Salama | All .md files |

---

## Branch Strategy Recommendation

### Option 1: Feature Branches (Recommended)
Each developer works on their own feature branch and merges in sequence:
```
main
├── feature/storage-layer (Youssef Amr)
├── feature/authentication (Mohamed Emad)
├── feature/crud-operations (Youssef Amr)
├── feature/search (Alkady)
├── feature/borrow-return (Mohamed Seif)
├── feature/app-setup (Mohamed Emad)
├── feature/api-client (Mohamed Emad)
├── feature/ui-components (Mohamed Emad)
├── feature/ui-pages (Mohamed Emad)
├── feature/tests (Amr)
└── feature/documentation (Omar Salama)
```

### Option 2: Role-Based Branches
Each role has their own branch:
```
main
├── storage-developer (Youssef Amr)
├── crud-developer (Youssef Amr)
├── search-developer (Alkady)
├── borrow-return-developer (Mohamed Seif)
├── ui-developer (Mohamed Emad)
├── tester (Amr)
└── documentation (Omar Salama)
```

---

## Important Notes

1. **Dependencies**: Always ensure dependencies are pushed before dependent code
2. **Testing**: Run tests after each phase before pushing
3. **Communication**: Coordinate with team members when pushing interdependent code
4. **Pull Before Push**: Always pull latest changes before pushing
5. **Commit Messages**: Use clear, descriptive commit messages
6. **Review**: Consider code reviews before merging to main branch

---

## Emergency Push Order (If Needed)

If you need to push everything at once, use this minimal order:

1. **BackEnd.Data** (all files) - Youssef Amr
2. **BackEnd** (all files) - Multiple developers (coordinate)
3. **frontend** (all files) - Mohamed Emad
4. **BackEnd.Tests** - Amr
5. **Documentation** - Omar Salama

---

## Questions or Issues?

If you encounter merge conflicts or dependency issues:
1. Check this guide to ensure you're pushing in the correct order
2. Communicate with team members whose code you depend on
3. Resolve conflicts by following the dependency chain
4. Update this document if the sequence needs adjustment

---

**Last Updated:** December 2024  
**Maintained By:** Omar Salama (Documentation Lead)
