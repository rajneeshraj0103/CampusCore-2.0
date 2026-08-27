# CampusCore – User CRUD, DTOs, Validation and Exception Handling

## Phase Summary

This document records the current implementation progress of the CampusCore backend after completing the User API CRUD flow and adding DTOs, validation, role validation, and centralized exception handling.

The implementation was built incrementally and tested through the API before moving to the next step.

---

# 1. Implementation Progression

This section records how the implementation progressed step by step. Each step either established a foundation, solved a problem discovered during testing, or improved the architecture.

### Step 1 — Set up the User CRUD implementation

The User module was implemented with the basic CRUD operations:

- Get all users
- Get user by ID
- Create user
- Update user
- Partially update user
- Delete user

This established the initial working API flow.

### Step 2 — Added Repository Pattern

Database access was separated from the Service layer by introducing:

- `IUserRepository`
- `UserRepository`

The Service layer no longer needed to directly use `CampusCoreDbContext` for User operations.

### Step 3 — Tested CRUD APIs

The CRUD endpoints were tested through Postman to verify that the basic operations worked correctly before introducing additional layers and validation.

### Step 4 — Introduced DTOs

DTOs were added to separate API request and response contracts from the `User` entity.

The following DTOs were introduced:

- `CreateUserDto`
- `UpdateUserDto`
- `PatchUserDto`
- `UserResponseDto`

### Step 5 — Updated CRUD to use DTOs and response DTOs

The CRUD flow was updated so that:

- POST accepts `CreateUserDto`
- PUT accepts `UpdateUserDto`
- PATCH uses `PatchUserDto`
- API responses return `UserResponseDto`

The API no longer exposes the User entity directly through the response.

### Step 6 — Implemented `CreatedAtAction`

The POST endpoint was improved to return:

`201 Created`

using `CreatedAtAction`, including a Location header pointing to the `GetUserById` endpoint for the newly created user.

### Step 7 — Added Data Annotation validation for POST

Validation attributes were added to `CreateUserDto`.

Examples included:

- Required fields
- Name length validation
- Email format validation
- Password length validation
- RoleId range validation

Invalid POST requests were then automatically rejected with `400 Bad Request`.

### Step 8 — Added validation for PUT

Similar validation rules were added to `UpdateUserDto`.

This ensured invalid full-update requests were rejected before reaching the Service layer.

### Step 9 — Discovered PATCH validation was not automatically working

Testing showed that applying a JSON Patch document did not automatically validate the resulting `PatchUserDto`.

For example, invalid values such as a one-character name could still pass through after the patch operation.

This exposed an important difference between normal model binding validation and JSON Patch processing.

### Step 10 — Added `TryValidateModel(dto)` after applying JSON Patch

The PATCH endpoint was updated to validate the DTO after applying the patch:

`patchDoc.ApplyTo(dto, ModelState);`

followed by:

`TryValidateModel(dto);`

This ensured invalid patched values were rejected with `400 Bad Request`.

### Step 11 — Discovered that `RoleId = 999` passed model validation but failed at the database level

The `[Range]` validation correctly rejected values less than 1.

However, a value such as:

`RoleId = 999`

could still pass Data Annotation validation because it is greater than 0.

The database then rejected the update because the referenced Role did not exist, resulting in a foreign key exception.

This demonstrated the difference between input validation and business validation.

### Step 12 — Added `IRoleRepository` and `RoleRepository`

A separate repository abstraction and implementation were introduced:

- `IRoleRepository`
- `RoleRepository`

Their responsibility is to look up Roles by ID.

### Step 13 — Added business validation for RoleId

`UserService` was updated to verify that the supplied Role exists before assigning the RoleId.

This validation was applied during:

- Create
- Update
- Patch, when RoleId is supplied

If the Role does not exist, the Service throws `BadRequestException`.

### Step 14 — Encountered raw `500` exceptions

Initially, application exceptions and database-related errors could result in raw exception output and `500 Internal Server Error` responses.

This exposed the need for centralized exception handling.

### Step 15 — Added `GlobalExceptionMiddleware`

A centralized middleware was added to catch exceptions from downstream components and convert them into consistent HTTP responses.

This removed exception handling responsibilities from individual controllers for application exceptions.

### Step 16 — Added `BadRequestException`

`BadRequestException` was introduced in the Application layer.

It is used when a request is structurally valid but violates an application business rule.

Current example:

`Invalid RoleId.`

The global exception middleware maps it to:

`400 Bad Request`

### Step 17 — Added `NotFoundException`

`NotFoundException` was introduced for resources that do not exist.

Current example:

`User with Id 999 was not found.`

The global exception middleware maps it to:

`404 Not Found`

### Step 18 — Gradually moved 404 handling from controllers into the centralized exception flow

Service methods were updated to throw `NotFoundException` instead of returning `null` or `false` for missing resources.

This simplified the controller layer.

The flow became:

Service detects missing resource
↓
Throws `NotFoundException`
↓
Global Exception Middleware catches it
↓
Returns consistent `404 Not Found`

### Step 19 — Tested POST, PUT, PATCH, GET and DELETE error scenarios again

After the validation and exception-handling changes, the API was tested again to verify:

- Valid requests
- Invalid input
- Invalid RoleId
- Non-existing users
- PATCH validation
- Successful deletion
- Deleting non-existing users

This completed the current CRUD, validation, and centralized exception-handling milestone.

---

# 2. What Was Built

The current User module supports:

- Get all users
- Get user by ID
- Create user
- Update user
- Partially update user
- Delete user
- Repository pattern
- Service layer
- DTO-based API contracts
- Manual entity-to-DTO mapping
- Data Annotation validation
- JSON Patch support
- Business validation for RoleId
- Centralized exception handling
- Consistent 400, 404, and 500 responses

Current request flow:

Client  
↓  
ASP.NET Core Middleware Pipeline  
↓  
Controller  
↓  
Service  
↓  
Repository  
↓  
EF Core  
↓  
Database  
↓  
Response

---

# 3. Repository Pattern

The repository layer was introduced to separate database access from business logic.

## User Repository

### Interface

`IUserRepository`

Current responsibilities:

- Get all users
- Get user by ID
- Add user
- Update user
- Delete user

### Implementation

`UserRepository`

The implementation uses `CampusCoreDbContext` and Entity Framework Core for database operations.

This keeps the Service layer independent from direct DbContext usage.

Flow:

Controller  
↓  
UserService  
↓  
IUserRepository  
↓  
UserRepository  
↓  
CampusCoreDbContext  
↓  
Database

---

# 4. Role Repository

A separate repository was added for Role lookup.

### Interface

`IRoleRepository`

Current responsibility:

- Get role by ID

### Implementation

`RoleRepository`

This repository is used by `UserService` to validate that a RoleId exists before assigning it to a user.

This prevents invalid role references from reaching the database.

---

# 5. User CRUD APIs

## GET – Get All Users

Endpoint:

`GET /api/user`

Flow:

Controller  
↓  
UserService.GetUsersAsync()  
↓  
UserRepository.GetAllAsync()  
↓  
Database

The returned User entities are manually mapped to `UserResponseDto`.

---

## GET – Get User By ID

Endpoint:

`GET /api/user/{id}`

If the user exists:

Returns `200 OK`

If the user does not exist:

`UserService` throws `NotFoundException`.

Global exception middleware returns `404 Not Found`.

Example error:

```json
{
  "message": "User with Id 999 was not found."
}
```

---

## POST – Create User

Endpoint:

`POST /api/user`

The API accepts `CreateUserDto`.

Before creating the user:

- ASP.NET Core validates the incoming DTO.
- `UserService` checks whether the supplied RoleId exists.

If the RoleId is invalid, `BadRequestException` is thrown.

The user is created and saved through the repository.

The controller uses `CreatedAtAction`.

Successful response:

`201 Created`

with a Location header pointing to `GetUserById`.

Example:

```json
{
  "id": 9,
  "name": "Created At Action User1234",
  "email": "createdataction1234@campuscore.com",
  "roleId": 1,
  "createdAt": "2026-08-26T07:32:12.4133842Z"
}
```

---

## PUT – Update User

Endpoint:

`PUT /api/user/{id}`

The API accepts `UpdateUserDto`.

Processing:

1. Find the user.
2. Throw `NotFoundException` if the user does not exist.
3. Validate that the supplied RoleId exists.
4. Update the entity.
5. Save through the repository.
6. Return `UserResponseDto`.

---

## PATCH – Partial Update User

Endpoint:

`PATCH /api/user/{id}`

The API uses:

`JsonPatchDocument<PatchUserDto>`

Flow:

1. Create a `PatchUserDto`.
2. Apply the JSON Patch document.
3. Run `TryValidateModel(dto)`.
4. Return `400 Bad Request` if validation fails.
5. Call the service.
6. Update only the supplied fields.

Supported fields currently include:

- Name
- Email
- RoleId

Example:

```json
[
  {
    "op": "replace",
    "path": "/name",
    "value": "Patch Validation User"
  }
]
```

The PATCH implementation exposed an important learning point:

Applying a JSON Patch document does not automatically guarantee Data Annotation validation of the resulting DTO.

Therefore:

`TryValidateModel(dto)`

was added after:

`patchDoc.ApplyTo(dto, ModelState);`

This ensures invalid patched values are rejected before reaching the Service layer.

---

## DELETE – Delete User

Endpoint:

`DELETE /api/user/{id}`

Processing:

1. Find the user.
2. Throw `NotFoundException` if the user does not exist.
3. Delete through the repository.
4. Return `204 No Content`.

Example:

`DELETE /api/user/7`

Response:

`204 No Content`

Deleting the same user again returns:

`404 Not Found`

through the centralized exception handling flow.

---

# 6. DTO Pattern

The API no longer returns the User entity directly for User API responses.

The following DTOs were introduced.

## CreateUserDto

Used for creating users.

Contains:

- Name
- Email
- Password
- RoleId

## UpdateUserDto

Used for full user updates.

Contains:

- Name
- Email
- RoleId

## PatchUserDto

Used for partial updates.

Properties are nullable because PATCH should allow only selected fields to be updated.

Contains:

- Name
- Email
- RoleId

## UserResponseDto

Used for API responses.

Contains:

- Id
- Name
- Email
- RoleId
- CreatedAt

The password and password hash are not exposed through this response DTO.

---

# 7. Manual Mapping

Manual mapping is currently used between `User` entities and `UserResponseDto`.

Example responsibilities:

- Entity → Response DTO
- Create DTO → Entity
- Update DTO → Existing Entity

AutoMapper has intentionally not been introduced yet.

Current decision:

Keep manual mapping while the project is small.

Introduce AutoMapper later when repeated mappings become significant enough to justify it.

This avoids introducing unnecessary abstraction early.

---

# 8. Model Validation

Data Annotation validation was added to the request DTOs.

## CreateUserDto

Current validation includes:

- Name is required.
- Name length: 3 to 100 characters.
- Email is required.
- Email must have a valid email format.
- Email maximum length: 255 characters.
- Password is required.
- Password minimum length: 8 characters.
- RoleId must be greater than or equal to 1.

## UpdateUserDto

Current validation includes:

- Name is required.
- Name length: 3 to 100 characters.
- Email is required.
- Email format validation.
- Email maximum length: 255 characters.
- RoleId must be greater than or equal to 1.

## PatchUserDto

PATCH fields are optional, but supplied values are validated.

Current validation includes:

- Name length validation.
- Email format validation.
- Email maximum length validation.
- RoleId must be greater than or equal to 1.

---

# 9. Input Validation vs Business Validation

Two different types of validation are now present.

## Input Validation

Handled through Data Annotations.

Examples:

- Invalid email format
- Name shorter than 3 characters
- Password shorter than 8 characters
- RoleId equal to 0

This prevents invalid request data from reaching business logic.

## Business Validation

Handled in `UserService`.

Example:

A RoleId may be greater than 0 and still not exist in the database.

For example:

`RoleId = 999`

This passes the `[Range]` validation but fails the business rule.

The Service layer checks:

Does this role exist?

If not, throw `BadRequestException`.

This distinction is important:

Model Validation  
↓  
Input Validation

Service Validation  
↓  
Business Validation

---

# 10. Custom Exceptions

Custom exceptions were introduced in the Application layer.

## BadRequestException

Used when a request is structurally valid but violates an application business rule.

Current example:

`Invalid RoleId.`

Mapped to:

`400 Bad Request`

## NotFoundException

Used when a requested resource does not exist.

Current example:

`User with Id 999 was not found.`

Mapped to:

`404 Not Found`

---

# 11. Global Exception Middleware

A centralized `GlobalExceptionMiddleware` was added.

Location:

`CampusCore.API/Middleware/GlobalExceptionMiddleware.cs`

The middleware catches unhandled exceptions from downstream components.

Flow:

Request  
↓  
GlobalExceptionMiddleware  
↓  
Controller  
↓  
Service  
↓  
Repository  
↓  
Exception  
↓  
GlobalExceptionMiddleware catches exception  
↓  
Standardized HTTP response

Current handling:

| Exception | HTTP Status |
|---|---|
| BadRequestException | 400 Bad Request |
| NotFoundException | 404 Not Found |
| Unexpected Exception | 500 Internal Server Error |

Unexpected errors return a generic message:

```json
{
  "message": "An unexpected error occurred."
}
```

This prevents API clients from receiving internal implementation details.

---

# 12. Request Logging Middleware

The existing request logging middleware logs:

- HTTP method
- Request path
- Response status code

This provides a simple view of the request/response flow during development.

The current logging middleware is intentionally simple and will later evolve toward structured logging and production-ready logging.

---

# 13. Dependency Injection

The following services are registered as scoped services:

`IUserService` → `UserService`

`IUserRepository` → `UserRepository`

`IRoleRepository` → `RoleRepository`

Scoped lifetime is appropriate because these services participate in the request flow and use EF Core DbContext, which is also normally scoped per request.

---

# 14. API Status Tested

The following scenarios were manually tested using Postman.

## GET

- Existing user → `200 OK`
- Non-existing user → `404 Not Found`

## POST

- Valid request → `201 Created`
- Invalid input → `400 Bad Request`
- Invalid existing-reference RoleId → `400 Bad Request`

## PUT

- Valid request → `200 OK`
- Invalid input → `400 Bad Request`
- Non-existing user → `404 Not Found`
- Invalid RoleId → `400 Bad Request`

## PATCH

- Valid partial update → `200 OK`
- Invalid name → `400 Bad Request`
- Invalid email → `400 Bad Request`
- RoleId less than 1 → `400 Bad Request`
- Non-existing RoleId → `400 Bad Request`
- Non-existing user → `404 Not Found`

## DELETE

- Existing user → `204 No Content`
- Non-existing user → `404 Not Found`

---

# 15. Important Architecture Decisions

## Application Layer does not depend on Infrastructure

The Application layer defines repository interfaces.

The Infrastructure layer implements those interfaces.

This preserves the dependency direction:

API  
↓  
Application  
↓  
Domain

Infrastructure provides implementations and is registered through dependency injection.

The Service layer depends on abstractions rather than directly depending on `CampusCoreDbContext`.

## Controllers Remain Thin

Controllers currently focus on:

- Receiving HTTP requests
- Model binding
- PATCH model validation
- Returning HTTP responses

Business rules are handled in the Service layer.

Database operations are handled in repositories.

---

# 16. Files Added or Modified

## API

Modified:

- `CampusCore.API.csproj`
- `Controllers/UserController.cs`
- `Program.cs`

Added:

- `Middleware/GlobalExceptionMiddleware.cs`

## Application

Added:

- `DTOs/Users/CreateUserDto.cs`
- `DTOs/Users/UpdateUserDto.cs`
- `DTOs/Users/PatchUserDto.cs`
- `DTOs/Users/UserResponseDto.cs`
- `Exceptions/BadRequestException.cs`
- `Exceptions/NotFoundException.cs`
- `Interfaces/IUserRepository.cs`
- `Interfaces/IRoleRepository.cs`

Modified:

- `Services/IUserService.cs`
- `Services/UserService.cs`

## Infrastructure

Modified:

- `CampusCore.Infrastructure.csproj`
- `Data/CampusCoreDbContext.cs`

Added:

- `Repositories/UserRepository.cs`
- `Repositories/RoleRepository.cs`

EF Core migration files were generated during database changes.

---

# 17. Production Thinking

The current implementation is appropriate for the learning stage, but several improvements will be required as the application grows.

## Current Limitations

### Get All Users

`GetUsersAsync()` currently returns all users.

As the table grows, this will need:

- Pagination
- Filtering
- Sorting

This is an upcoming backend concern.

### Repository Queries

Read operations currently use straightforward Entity Framework queries.

As read volume and data size increase, future optimization should evaluate:

- `AsNoTracking()` for read-only queries
- Projection directly to DTOs where appropriate
- Pagination
- Database indexing
- Query performance

### Error Logging

The global exception middleware currently returns consistent responses.

A later production-ready improvement should also include structured exception logging so unexpected failures can be investigated.

### Password Handling

The current implementation stores the incoming password in `PasswordHash`.

Password hashing and authentication should be properly implemented before production use.

The current CRUD implementation should not be considered production-ready authentication.

---

# 18. Unit Tests

Unit tests have not yet been added for this implementation.

Future testing should include:

- `UserService` tests using xUnit and Moq
- Controller tests
- Validation scenarios
- Exception scenarios
- Repository behavior where appropriate

---

# 19. Current Implementation Status

## Completed

- Database foundation
- Entity setup
- ASP.NET Core project setup
- DbContext and EF Core setup
- Dependency Injection
- Basic middleware
- Async CRUD implementation
- Repository pattern
- User CRUD APIs
- DTOs
- Manual mapping
- Data Annotation validation
- PATCH validation
- Role business validation
- Global exception middleware
- 400 / 404 / 500 handling
- Postman API testing

## Deferred

- AutoMapper
- FluentValidation
- Unit testing
- Controller testing
- Pagination
- Filtering
- Sorting
- JWT authentication
- Role-based authorization
- React UI
- Redis
- RabbitMQ
- PostgreSQL migration
- Azure deployment

---

# 20. What Comes Next

The next backend milestone should continue from the current stable CRUD foundation.

The immediate upcoming work should be chosen according to the project roadmap and kept incremental.

Before moving significantly further, the current milestone should be:

- Documented
- Reviewed
- Committed
- Pushed to GitHub

This document represents the implementation state at the end of the current User CRUD, DTO, validation, repository, and exception-handling milestone.
