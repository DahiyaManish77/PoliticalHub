# Coding Standards

# PoliticalLeaderPortal Development Standards

## Purpose

This document defines the official coding standards for PoliticalLeaderPortal.

Every developer and AI coding assistant must follow these standards when creating or modifying code.

The objective is to keep the project:

- Maintainable
- Consistent
- Enterprise-grade
- Easy to extend
- Easy to debug

---

# Technology Standards

## Backend

- ASP.NET MVC 5
- .NET Framework 4.8
- C#
- Razor Views

---

## Database

- SQL Server
- Entity Framework 6
- Database First (EDMX)

---

## Frontend

- Bootstrap 5
- jQuery
- AJAX
- HTML5
- CSS3

---

# Architecture Standards

The project follows:

- MVC Architecture
- Service Layer
- ViewModel Pattern
- Database First
- Separation of Concerns

New code must follow the same architecture.

Do not introduce:

- Repository Pattern
- CQRS
- Domain Driven Design (DDD)
- Minimal APIs
- ASP.NET Core patterns
- Microservices
- Third-party frameworks without approval

---

# Folder Standards

Every new business module should follow this structure.

Areas/Admin/

- Controllers
- Services
- ViewModels
- Views

Public modules should use:

Controllers/

Services/

ViewModels/

Views/

Scripts/

Content/

---

# Controller Standards

Controllers should only:

- Receive requests
- Validate models
- Call Services
- Return Views
- Return JSON

Controllers should NOT:

- Execute SQL
- Perform complex calculations
- Contain business rules
- Handle file processing logic

Controllers should remain small and easy to read.

---

# Service Standards

Business logic belongs inside Services.

Services should:

- Access Entity Framework
- Apply business rules
- Prepare ViewModels
- Perform validation
- Handle file operations
- Generate reports

Services should avoid:

- UI code
- HTML generation
- JavaScript generation

---

# ViewModel Standards

Always use ViewModels between Controllers and Views.

Create dedicated ViewModels for:

- Create
- Edit
- Details
- List
- Search
- Dashboard
- Reports

Do not expose Entity Framework entities directly to Razor Views unless absolutely necessary.

---

# Entity Framework Standards

The project uses Database First.

Rules:

- Never manually edit generated entity classes.
- Never manually edit DbContext.
- Always update EDMX after database changes.
- Keep generated files untouched.

---

# Database Standards

Database changes should:

- Preserve backward compatibility where possible.
- Use Primary Keys.
- Use Foreign Keys.
- Use proper indexing.
- Avoid duplicate tables.
- Follow consistent naming conventions.

---

# Naming Conventions

Classes

PascalCase

Example:

HeroSliderService

WebsiteSettingService

Methods

PascalCase

Example:

GetLatestNews()

SaveWebsiteSettings()

Properties

PascalCase

Variables

camelCase

Constants

PascalCase or UPPER_CASE where appropriate.

---

# Razor Standards

Views should remain clean.

Move business logic to Services.

Use Partial Views for reusable UI.

Avoid large Razor files whenever practical.

---

# JavaScript Standards

Use:

- jQuery
- AJAX

Keep JavaScript in separate files.

Avoid inline JavaScript inside Razor Views.

Use event delegation where appropriate.

---

# CSS Standards

Use centralized CSS files.

Avoid inline CSS.

Reuse existing classes.

Maintain consistent spacing, typography, and component styling.

---

# Responsive Design Standards

Every page must support:

- Desktop
- Laptop
- Tablet
- Mobile

Bootstrap Grid should be used consistently.

---

# Validation Standards

Use:

- Data Annotations
- Server-side validation
- Client-side validation

Never rely only on client-side validation.

---

# Security Standards

Always:

- Validate user input.
- Validate uploaded files.
- Use Anti-Forgery Tokens.
- Respect Role Permissions.
- Hash passwords.
- Protect sensitive information.

Never expose:

- SQL credentials
- API keys
- Internal exception details

---

# File Upload Standards

Validate:

- Extension
- MIME Type
- File Size
- File Name

Store uploads inside the Uploads folder.

Store only relative paths in the database.

---

# Error Handling

Handle exceptions gracefully.

Provide user-friendly messages.

Log unexpected errors.

Avoid empty catch blocks.

---

# Performance Standards

Prefer:

- Efficient LINQ queries
- Paging
- Filtering
- Reuse of Services

Avoid:

- N+1 queries
- Duplicate queries
- Large ViewModels
- Repeated database calls

---

# Documentation Standards

New modules should include:

- Controller
- Service
- ViewModels
- Views
- JavaScript
- CSS

Update documentation whenever architecture changes.

---

# Git Standards

One feature per commit.

Use meaningful commit messages.

Do not commit:

- bin
- obj
- .vs
- temporary files
- secrets
- generated backups

---

# AI Coding Standards

Before generating code:

1. Analyze the existing implementation.
2. Reuse current architecture.
3. Follow existing naming conventions.
4. Explain planned changes.
5. List affected files.
6. Wait for user approval.

Never rewrite unrelated code.

Never introduce new frameworks unless explicitly requested.

---

# Long-Term Goal

Maintain a clean, modular, enterprise-quality codebase that can evolve for many years while preserving consistency, readability, and maintainability.