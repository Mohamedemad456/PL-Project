<!-- Role: Documentation Lead -->
<!-- Developer: Omar Salama -->
# Migration Guide

This guide explains how to create and manage EF Core migrations in the hybrid F#/C# setup.

## Prerequisites

1. Ensure you have the EF Core tools installed:
   ```bash
   dotnet tool install --global dotnet-ef
   ```

2. Verify your connection string is correct in `BackEnd/appsettings.json`

## Creating a New Migration

### Step 1: Make Changes to Models or DbContext

Edit your models in `BackEnd.Data/Models/` or update `BackEnd.Data/AppDbContext.cs`:

**Example - Adding a new property to User:**
```csharp
// BackEnd.Data/Models/User.cs
public class User
{
    // ... existing properties ...
    public string PhoneNumber { get; set; } = string.Empty;  // New property
}
```

**Example - Adding a new entity:**
```csharp
// BackEnd.Data/Models/Order.cs (new file)
public class Order
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

Then add the DbSet to `AppDbContext.cs`:
```csharp
public DbSet<Order> Orders { get; set; }
```

### Step 2: Navigate to the Data Project

```bash
cd BackEnd.Data
```

### Step 3: Create the Migration

```bash
dotnet ef migrations add MigrationName --startup-project ..\BackEnd\BackEnd.fsproj
```

**Replace `MigrationName` with a descriptive name**, for example:
- `AddPhoneNumberToUser`
- `CreateOrderTable`
- `AddIndexToEmail`
- `UpdateProductDescription`

**Full example:**
```bash
dotnet ef migrations add AddPhoneNumberToUser --startup-project ..\BackEnd\BackEnd.fsproj
```

### Step 4: Review the Generated Migration

The migration will be created in `BackEnd.Data/Migrations/` with a timestamp prefix:
- `20241129230000_AddPhoneNumberToUser.cs`
- `20241129230000_AddPhoneNumberToUser.Designer.cs`

Review the generated code to ensure it matches your expectations.

### Step 5: Apply the Migration to Database

```bash
dotnet ef database update --startup-project ..\BackEnd\BackEnd.fsproj
```

This will:
- Apply any pending migrations to your database
- Update the database schema
- Update the `AppDbContextModelSnapshot.cs` file

## Common Migration Commands

### List All Migrations
```bash
cd BackEnd.Data
dotnet ef migrations list --startup-project ..\BackEnd\BackEnd.fsproj
```

### Remove Last Migration (if not applied)
```bash
cd BackEnd.Data
dotnet ef migrations remove --startup-project ..\BackEnd\BackEnd.fsproj
```

### Generate SQL Script (without applying)
```bash
cd BackEnd.Data
dotnet ef migrations script --startup-project ..\BackEnd\BackEnd.fsproj --output migration.sql
```

### Apply Specific Migration
```bash
cd BackEnd.Data
dotnet ef database update MigrationName --startup-project ..\BackEnd\BackEnd.fsproj
```

### Rollback to Previous Migration
```bash
cd BackEnd.Data
dotnet ef database update PreviousMigrationName --startup-project ..\BackEnd\BackEnd.fsproj
```

## Quick Reference

**From solution root:**
```bash
# Create migration
dotnet ef migrations add MigrationName --project BackEnd.Data --startup-project BackEnd

# Apply migration
dotnet ef database update --project BackEnd.Data --startup-project BackEnd

# Remove migration
dotnet ef migrations remove --project BackEnd.Data --startup-project BackEnd
```

**From BackEnd.Data directory:**
```bash
# Create migration
dotnet ef migrations add MigrationName --startup-project ..\BackEnd\BackEnd.fsproj

# Apply migration
dotnet ef database update --startup-project ..\BackEnd\BackEnd.fsproj

# Remove migration
dotnet ef migrations remove --startup-project ..\BackEnd\BackEnd.fsproj
```

## Troubleshooting

### Error: "Unable to create a 'DbContext'"
- Ensure your connection string in `appsettings.json` is correct
- Make sure SQL Server is running and accessible

### Error: "No parameterless constructor"
- This shouldn't happen with the current setup, but if it does, ensure `AppDbContext` has the proper constructor

### Migration conflicts
- If you have conflicts, you may need to resolve them manually in the migration file
- Always review generated migrations before applying

## Best Practices

1. **Name migrations descriptively**: Use clear names like `AddEmailIndex` or `CreateOrderTable`
2. **Review before applying**: Always check the generated migration code
3. **Test locally first**: Apply migrations to a development database before production
4. **One change per migration**: Keep migrations focused on a single change when possible
5. **Commit migrations**: Include migration files in version control

