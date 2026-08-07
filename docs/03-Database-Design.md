# Phase 1 – Database Design

---

# 1. Document Information

| Property      | Value           |
| ------------- | --------------- |
| Document Name | Database Design |
| Project       | CampusCore 2.0  |
| Phase         | Phase 1         |
| Version       | 1.0             |
| Status        | In Progress     |
| Prepared By   | Rajneesh Raj    |
| Last Updated  | August 2026     |

---

# 2. Revision History

| Version | Date        | Author       | Description                                                      |
| ------- | ----------- | ------------ | ---------------------------------------------------------------- |
| 1.0     | August 2026 | Rajneesh Raj | Initial database design based on Administrator Login requirement |

---

# 3. Purpose

The purpose of this document is to design the database incrementally based on business requirements.

CampusCore 2.0 does not follow a "design everything first" approach.

Instead, every table, column, relationship, and constraint is introduced only when the current business requirement justifies it.

This document will evolve throughout the project.

---

# 4. Database Design Philosophy

The database will grow together with the product.

Every design decision must satisfy the following questions:

- What are we building?
- Why do we need it?
- Why do we need it now?
- How will it work?
- What trade-offs were considered?

No table or column will be added without a clear business requirement.

---

# 5. Current Business Requirement

The first business requirement is:

**Administrator Login**

The current implementation milestone focuses on enabling Administrator Login.

To support this requirement, the system must first identify:

- Who is trying to log in?
- What role does the user have?
- Whether the user is authorized to access protected resources.

This requirement introduces the first two business concepts:

- Role
- User

---

# 6. Business Concepts

## Role

A Role represents a responsibility within CampusCore.

The initial system-defined roles are:

- Administrator
- Teacher
- Student

A Role is a business concept, not a person.

Additional roles may be introduced in future versions if business requirements evolve.

---

## User

A User represents a person who can access CampusCore.

Every User belongs to exactly one Role.

---

# 7. Design Decisions

## Decision 1

### Administrator is a User

An Administrator is not a separate entity.

It is simply a User whose assigned Role is "Administrator".

**Status:** ✅ Accepted

---

## Decision 2

### Roles are created before Users

Every User must belong to a valid Role.

Therefore, Roles must exist before Users can be created.

**Status:** ✅ Accepted

---

## Decision 3

### Initial Role Structure

Current fields:

- Id
- Name
- CreatedAt

Additional fields will be introduced only when required.

**Status:** ✅ Accepted

---

## Decision 4

### Initial User Structure

Current fields:

- Id
- Name
- Email
- PasswordHash
- RoleId
- CreatedAt

Only the fields required to authenticate an Administrator and determine the user's role have been included.

Additional fields will be introduced only when new business requirements justify them.

**Status:** ✅ Accepted

---

## Decision 5

### Audit Strategy

Audit information will be introduced incrementally.

Current audit field:

- CreatedAt

Future audit fields:

- UpdatedAt
- CreatedBy
- UpdatedBy
- DeletedAt
- DeletedBy

These fields are intentionally deferred to avoid introducing unused columns during the early stages of development.

**Status:** ✅ Accepted

---

## Decision 6

### Role Seeding Strategy

Roles are considered system data rather than business data.

Therefore, CampusCore will automatically seed the following roles during database initialization:

- Administrator
- Teacher
- Student

This ensures every installation starts with the same predefined roles required by the application.

**Status:** ✅ Accepted

---

## Decision 7

### Relationship Between Role and User

Business Rule:

- One Role can have many Users.
- Every User must belong to exactly one Role.
- A User cannot exist without a Role.

Relationship:

Role (1) -------- (Many) User

**Status:** ✅ Accepted

---

# 8. Current Domain Model

```
Role
------------------
Id
Name
CreatedAt

        1
        │
        │
        │
        │
        *
User
------------------
Id
Name
Email
PasswordHash
RoleId
CreatedAt
```

---

# 9. Current Database Model

## Role

| Column    | Description               |
| --------- | ------------------------- |
| Id        | Unique identifier         |
| Name      | Role name                 |
| CreatedAt | Record creation timestamp |

---

## User

| Column       | Description               |
| ------------ | ------------------------- |
| Id           | Unique identifier         |
| Name         | User name                 |
| Email        | Login email               |
| PasswordHash | Hashed password           |
| RoleId       | Foreign Key referencing Role         |
| CreatedAt    | Record creation timestamp |

---

# 10. Business Rules

1. Every User must belong to exactly one Role.

2. A Role may be assigned to multiple Users.

3. A User cannot exist without a valid Role.

4. Email addresses must be unique.

5. Passwords are never stored in plain text.

6. System-defined Roles are seeded during database initialization.

7. New business entities are introduced only when required by a business feature.

---

# 11. Database Evolution Strategy

CampusCore 2.0 follows an incremental database design approach.

The database will evolve alongside the business requirements.

Future entities such as:

- Teacher
- Student
- Course
- Enrollment
- Attendance
- Examination
- Result
- Fee

will only be introduced when their corresponding business requirements are implemented.
This approach ensures that every table, relationship, and column has a clear business purpose before implementation begins.

---

# 12. Current Progress

Completed:

- ✅ Business Concepts Identified
- ✅ Role Design
- ✅ User Design
- ✅ Audit Strategy
- ✅ Role Seeding Strategy
- ✅ Role–User Relationship
- ✅ Business Rules Defined

In Progress:

- ⏳ PostgreSQL Schema Design
- ⏳ Entity Framework Core Mapping

---

# 13. Next Step

The next step is to translate the approved business model into a PostgreSQL database schema.

This will include:

- Table definitions
- Primary Keys
- Foreign Keys
- Constraints
- Naming conventions
- Entity Framework Core mappings

The schema will be implemented only after the design decisions documented above have been finalized.
