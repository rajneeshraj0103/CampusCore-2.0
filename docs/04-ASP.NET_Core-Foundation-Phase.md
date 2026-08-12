# CampusCore — ASP.NET Core Foundation

## 1. Phase Objective

The objective of this phase is to establish the ASP.NET Core foundation of CampusCore and understand how the application works internally before moving into more advanced features.

The phase focuses on:

- ASP.NET Core application structure
- Clean Architecture project separation
- Dependency Injection
- Constructor Injection
- Controllers and endpoints
- Request lifecycle
- Middleware
- Middleware short-circuiting
- Entity Framework Core
- `DbContext`
- Domain entities
- Entity relationships
- PostgreSQL integration
- EF Core + Npgsql
- Database connection through DI

---

# 2. Git Branch

Feature branch created:

```text
feature/aspnet-core-foundation
```

This branch is being used for the ASP.NET Core foundation work.

---

# 3. Solution Structure

The CampusCore solution currently contains four projects:

```text
CampusCore
│
├── CampusCore.API
├── CampusCore.Application
├── CampusCore.Domain
└── CampusCore.Infrastructure
```

Their responsibilities are:

```text
CampusCore.Domain
        ↑
        │
CampusCore.Application
        ↑
        │
CampusCore.Infrastructure
        ↑
        │
CampusCore.API
```

The important architectural rule is that the Domain layer remains independent of infrastructure concerns such as EF Core, PostgreSQL, or ASP.NET Core.

---

# 4. CampusCore.Domain

The Domain project contains business/domain entities.

Current entities:

```text
Role
User
```

## User

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public Role Role { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

## Role

```csharp
public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ICollection<User> Users { get; set; }
}
```

---

# 5. Understanding Navigation Properties

The following property:

```csharp
public Role Role { get; set; }
```

means:

```text
Type/Data Type = Role
Property Name  = Role
```

It represents the related Role object for a User.

For example:

```text
User
│
├── Id = 1
├── Name = "System Administrator"
├── RoleId = 1
└── Role
      ├── Id = 1
      └── Name = "Administrator"
```

The `Role` here is not the database table name.

It is the C# type representing a related Role entity.

---

The following property:

```csharp
public ICollection<User> Users { get; set; }
```

means:

```text
Type/Data Type = ICollection<User>
Property Name  = Users
```

It represents a collection of User objects related to a Role.

Therefore:

```text
Role
│
└── Users
      ├── User 1
      ├── User 2
      └── User 3
```

This represents the:

```text
Role 1 ─────── * User
```

relationship.

---

# 6. Entity Framework Core DbContext

`CampusCoreDbContext` was created in:

```text
CampusCore.Infrastructure
└── Data
    └── CampusCoreDbContext.cs
```

The current context is:

```csharp
public class CampusCoreDbContext : DbContext
{
    public CampusCoreDbContext(
        DbContextOptions<CampusCoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>()
            .ToTable("Role");

        modelBuilder.Entity<User>()
            .ToTable("User");

        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

---

# 7. Why DbContext Is in Infrastructure

`CampusCoreDbContext` belongs in Infrastructure because it is an implementation detail related to data access.

It depends on:

```text
Entity Framework Core
PostgreSQL provider
Database configuration
```

The Domain project should not depend on these infrastructure technologies.

Therefore:

```text
Domain
  ↓
Business entities

Infrastructure
  ↓
Database implementation
```

This preserves the architectural separation.

---

# 8. DbSet

We added:

```csharp
public DbSet<Role> Roles { get; set; }

public DbSet<User> Users { get; set; }
```

`DbSet<Role>` represents the collection through which EF Core works with `Role` entities.

Conceptually:

```text
_context.Roles
      ↓
Role entities
      ↓
"Role" database table
```

Similarly:

```text
_context.Users
      ↓
User entities
      ↓
"User" database table
```

---

# 9. Explicit Table Mapping

We explicitly configured:

```csharp
modelBuilder.Entity<Role>()
    .ToTable("Role");

modelBuilder.Entity<User>()
    .ToTable("User");
```

Therefore:

```text
C# Entity             PostgreSQL Table

Role          ─────→  "Role"

User          ─────→  "User"
```

This was important because CampusCore already had the PostgreSQL schema created.

---

# 10. User → Role Relationship

The relationship was configured as:

```csharp
modelBuilder.Entity<User>()
    .HasOne(u => u.Role)
    .WithMany(r => r.Users)
    .HasForeignKey(u => u.RoleId)
    .OnDelete(DeleteBehavior.Restrict);
```

This means:

```text
One Role
   │
   │
   └──────────────< Many Users
```

More specifically:

```text
User
 │
 ├── RoleId
 │
 └── Role
```

and:

```text
Role
 │
 └── Users
```

`RoleId` is the foreign key connecting the two entities.

---

# 11. Delete Behavior

We configured:

```csharp
.OnDelete(DeleteBehavior.Restrict);
```

This means a Role cannot simply be deleted while Users are still referencing it.

Conceptually:

```text
Role 1
  ↑
  │
User 1
User 2
User 3
```

Attempting to delete Role 1 while those Users still reference it should be restricted.

---

# 12. Dependency Injection

ASP.NET Core has a built-in Dependency Injection container.

The basic idea learned in this phase is:

> A class should receive the dependencies it needs instead of creating those dependencies itself.

For example, instead of:

```csharp
public class UserService
{
    public UserService()
    {
        _repository = new UserRepository();
    }
}
```

we use dependency injection.

The class receives the dependency from outside.

---

# 13. Why Tight Coupling Is a Problem

If `UserService` directly does:

```csharp
_repository = new UserRepository();
```

then `UserService` knows exactly which concrete implementation it must create.

The dependency becomes:

```text
UserService
     ↓
new UserRepository()
```

Changing the repository implementation now requires changing `UserService`.

This makes the classes tightly coupled.

With DI:

```text
UserService
     ↓
IUserRepository
     ↑
     │
DI Container
     │
     ↓
UserRepository
```

`UserService` depends on the abstraction.

The DI container decides which implementation to provide.

---

# 14. Constructor Injection

The DI technique used in CampusCore is Constructor Injection.

Example:

```csharp
public UserController(IUserService userService)
{
    _userService = userService;
}
```

The dependency is supplied through the constructor.

Similarly, our database controller used:

```csharp
public DatabaseController(CampusCoreDbContext context)
{
    _context = context;
}
```

Therefore:

```text
Constructor Injection
        ↓
Dependency supplied by DI
        ↓
Class can use dependency
```

---

# 15. Service Registration

We registered:

```csharp
builder.Services.AddScoped<IUserService, UserService>();
```

This tells ASP.NET Core:

```text
When something asks for IUserService
              ↓
provide UserService
```

We also registered:

```csharp
builder.Services.AddDbContext<CampusCoreDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("CampusCoreDb")));
```

This tells ASP.NET Core:

```text
When something asks for CampusCoreDbContext
              ↓
create/configure it using EF Core
              ↓
use PostgreSQL
```

---

# 16. Controller Pattern

We created `UserController`.

The basic controller structure is:

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
}
```

The thought process is:

```text
1. What resource does the controller represent?
       ↓
   User

2. What should the route be?
       ↓
   api/user

3. What dependency does the controller need?
       ↓
   IUserService

4. What endpoint do we expose?
       ↓
   HTTP GET
```

---

# 17. UserController

Our test controller used:

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        var message = _userService.GetMessage();

        return Ok(message);
    }
}
```

The endpoint became:

```text
GET /api/user/test
```

and returned:

```text
Hello from UserService
```

---

# 18. ASP.NET Core Request Lifecycle

When the browser requested:

```text
https://localhost:7210/api/user/test
```

the conceptual flow was:

```text
Browser
   ↓
Kestrel
   ↓
Middleware Pipeline
   ↓
Routing / Endpoint Selection
   ↓
Controller
   ↓
UserService
   ↓
Response
   ↓
Middleware Pipeline
   ↓
Kestrel
   ↓
Browser
```

Kestrel receives the network request and hands it into the ASP.NET Core application.

---

# 19. Middleware

We created:

```text
CampusCore.API
└── Middleware
    ├── RequestLoggingMiddleware
    └── BlockRequestMiddleware
```

A middleware receives an `HttpContext` and can perform work before and/or after the next middleware.

Basic structure:

```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Before

        await _next(context);

        // After
    }
}
```

---

# 20. What Is `_next`?

`_next` represents the next component in the middleware pipeline.

Conceptually:

```text
Middleware A
     ↓
Middleware B
     ↓
Middleware C
     ↓
Controller
```

When Middleware A executes:

```csharp
await _next(context);
```

it passes the request to Middleware B.

After Middleware B and everything downstream completes, execution can return to Middleware A.

Therefore middleware can execute code:

```text
Before _next()
       ↓
   next middleware
       ↓
   controller
       ↓
   response
       ↓
After _next()
```

---

# 21. Why `async Task`?

Middleware operations can involve asynchronous work.

The method:

```csharp
public async Task InvokeAsync(HttpContext context)
```

means:

```text
async
  ↓
method can await asynchronous operations

Task
  ↓
represents the asynchronous operation
```

We used:

```csharp
await _next(context);
```

because the next middleware component is asynchronous.

---

# 22. HttpContext

ASP.NET Core provides the current HTTP request/response information through:

```csharp
HttpContext
```

The middleware receives it:

```csharp
public async Task InvokeAsync(HttpContext context)
```

The `context` variable represents the current HTTP request and response context.

For example:

```csharp
context.Request.Method
```

gives the HTTP method.

```csharp
context.Request.Path
```

gives the request path.

```csharp
context.Response.StatusCode
```

gives the response status code.

---

# 23. Request Logging Middleware

We implemented:

```csharp
Console.WriteLine(
    $"Request: {context.Request.Method} {context.Request.Path}");

await _next(context);

Console.WriteLine(
    $"Response: {context.Response.StatusCode}");
```

For:

```text
GET /api/user/test
```

we observed:

```text
Request: GET /api/user/test
Response: 200
```

This demonstrated the "before and after `_next`" behavior.

---

# 24. Middleware Short-Circuiting

We also created a temporary blocking middleware.

The important logic was:

```csharp
if (request == "/api/user/test")
{
    context.Response.StatusCode = 403;

    await context.Response.WriteAsync(
        "Request blocked by middleware.");
}
else
{
    await _next(context);
}
```

The important concept is:

```text
Request
   ↓
BlockRequestMiddleware
   │
   ├── blocked
   │     ↓
   │   response
   │
   └── allowed
         ↓
      _next()
```

If the middleware does not call:

```csharp
await _next(context);
```

the request does not continue to the downstream pipeline.

This is called **short-circuiting**.

We verified this by receiving:

```text
403
Request blocked by middleware.
```

---

# 25. PostgreSQL Integration

CampusCore uses PostgreSQL.

The EF Core PostgreSQL provider is:

```text
Npgsql.EntityFrameworkCore.PostgreSQL
```

The relationship is:

```text
EF Core
   ↓
Npgsql
   ↓
PostgreSQL
```

---

# 26. Connection String

The database connection configuration is kept in:

```text
CampusCore.API
└── appsettings.json
```

Structure:

```json
{
  "ConnectionStrings": {
    "CampusCoreDb": "Host=localhost;Port=5432;Database=CampusCoreDb;Username=postgres;Password=..."
  }
}
```

The application retrieves it using:

```csharp
builder.Configuration.GetConnectionString("CampusCoreDb")
```

---

# 27. Registering DbContext

The API registers the database context:

```csharp
builder.Services.AddDbContext<CampusCoreDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("CampusCoreDb")));
```

This combines:

```text
Configuration
      ↓
Connection String
      ↓
UseNpgsql
      ↓
CampusCoreDbContext
      ↓
DI Container
```

---

# 28. Actual Database Verification

We did not stop after building the application.

We created a temporary `DatabaseController` to verify the real database integration.

First we tested:

```csharp
await _context.Database.CanConnectAsync();
```

The result was:

```json
{
  "databaseConnected": true
}
```

This proved that the application could actually reach PostgreSQL.

---

# 29. Role Query Verification

We then queried:

```csharp
var roles = await _context.Roles.ToListAsync();
```

The existing database returned:

```text
Administrator
Teacher
Student
```

This proved:

```text
Role entity
   ↓
DbSet<Role>
   ↓
EF Core mapping
   ↓
"Role" table
   ↓
PostgreSQL
```

was working correctly.

---

# 30. User Query Verification

We also queried:

```csharp
var users = await _context.Users.ToListAsync();
```

The existing User record was successfully returned.

This proved:

```text
User entity
   ↓
DbSet<User>
   ↓
EF Core mapping
   ↓
"User" table
   ↓
PostgreSQL
```

was working correctly.

The response contained:

```text
Id
Name
Email
PasswordHash
RoleId
CreatedAt
```

and:

```text
Role = null
```

because we did not explicitly load the related Role.

The `RoleId` was still present, proving that the foreign-key value was loaded.

---

# 31. Temporary DatabaseController

`DatabaseController` was created only for verification.

It tested:

```text
Database connection
Role query
User query
```

After successful verification, it was deleted.

Therefore it is **not part of the final CampusCore architecture**.

Current API controller retained:

```text
UserController
```

---

# 32. Important EF Core Concept: Navigation Loading

We observed:

```json
"role": null
```

when querying Users.

This happened because:

```csharp
_context.Users.ToListAsync()
```

loads Users but does not automatically load the related Role in this scenario.

Later, EF Core's explicit eager-loading mechanism can be used:

```csharp
.Include(u => u.Role)
```

This topic will be learned when we reach actual repository/data-access implementation.

---

# 33. Current Architecture

The current CampusCore flow is:

```text
                    HTTP Request
                         │
                         ▼
                 CampusCore.API
                         │
                    Controller
                         │
                         ▼
                Application Service
                         │
                         ▼
                  Infrastructure
                         │
                  DbContext / EF Core
                         │
                         ▼
                      Npgsql
                         │
                         ▼
                    PostgreSQL
```

The Domain remains at the center of the model:

```text
                   Domain
                ┌──────────┐
                │ Role     │
                │ User     │
                └──────────┘
                     ▲
                     │
              Application
                     ▲
                     │
             Infrastructure
                     ▲
                     │
                   API
```

---

# 34. What We Have NOT Done Yet

The following are intentionally not part of the completed work yet:

- Repository implementation
- Service-to-repository database flow
- DTOs
- AutoMapper
- FluentValidation
- Pagination
- Exception middleware
- JWT authentication
- Role-based authorization
- Redis
- RabbitMQ
- Unit tests
- Integration tests
- EF Core migrations

These belong to later stages of the CampusCore roadmap.

---

# 35. Phase Verification Checklist

```text
[x] Created feature/aspnet-core-foundation branch
[x] Created CampusCore solution
[x] Created Domain project
[x] Created Application project
[x] Created Infrastructure project
[x] Created API project
[x] Added project references
[x] Created Role entity
[x] Created User entity
[x] Understood navigation properties
[x] Created CampusCoreDbContext
[x] Added DbSet<Role>
[x] Added DbSet<User>
[x] Configured Role/User relationship
[x] Configured DeleteBehavior.Restrict
[x] Configured PostgreSQL provider
[x] Added connection string
[x] Registered DbContext with DI
[x] Created UserController
[x] Created test endpoint
[x] Verified DI through UserService
[x] Created RequestLoggingMiddleware
[x] Verified middleware before/after behavior
[x] Created short-circuiting middleware
[x] Verified 403 response
[x] Verified PostgreSQL connection
[x] Verified Role query
[x] Verified User query
[x] Removed temporary DatabaseController
[x] Solution builds successfully
```

---

# 36. Key Memory Rules

### DI

> **A class should receive dependencies instead of creating them itself.**

### Constructor Injection

> **The dependency is supplied through the constructor.**

### Middleware

> **Middleware can work before `_next`, pass the request downstream with `_next`, and work again after `_next` returns.**

### Short-circuiting

> **If middleware does not call `_next`, the request does not continue downstream.**

### DbContext

> **`DbContext` is EF Core's main bridge between C# entities and the database.**

### DbSet

> **A `DbSet<TEntity>` represents the EF Core entry point for querying and working with an entity type.**

### Navigation Property

> **A navigation property represents a relationship to another entity object or collection of objects.**

### PostgreSQL Provider

> **Npgsql allows EF Core to communicate with PostgreSQL.**

### `AddDbContext`

> **Registers and configures the DbContext with ASP.NET Core DI.**

---

# 37. Phase Status

```text
ASP.NET Core Foundation
        │
        ├── Project foundation       ✅
        ├── DI                       ✅
        ├── Controllers              ✅
        ├── Middleware               ✅
        ├── EF Core                  ✅
        ├── PostgreSQL integration   ✅
        └── Database verification    ✅

              STATUS

             COMPLETE
```

The next implementation stage should continue from this state rather than recreating any of the foundation work.