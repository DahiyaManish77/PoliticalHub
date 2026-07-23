# Architecture

# PoliticalLeaderPortal System Architecture

## Overview

PoliticalLeaderPortal is an enterprise-grade ASP.NET MVC 5 web application built using a layered architecture. The application combines a public-facing website with a powerful administrative Content Management System (CMS), campaign management features, and election operations.

The architecture emphasizes:

- Separation of Concerns
- Reusability
- Maintainability
- Scalability
- Consistent User Experience

---

# Technology Stack

## Backend

- ASP.NET MVC 5
- .NET Framework 4.8
- C#
- Entity Framework 6 (Database First)
- SQL Server

## Frontend

- Bootstrap 5
- jQuery
- AJAX
- HTML5
- CSS3
- Razor Views

## Authentication

- Forms Authentication
- Role-Based Authorization
- Session Management

---

# High-Level Architecture

```
Browser
    │
    ▼
Controllers
    │
    ▼
Services
    │
    ▼
Entity Framework (EDMX)
    │
    ▼
SQL Server Database
```

---

# Layers

## Presentation Layer

Responsible for:

- Razor Views
- Layouts
- Partial Views
- Forms
- Validation
- User Interface

Folders:

- Views
- Areas/Admin/Views

---

## Controller Layer

Responsibilities:

- Receive HTTP Requests
- Validate Input
- Call Services
- Return Views or JSON

Controllers should remain lightweight and avoid business logic.

---

## Service Layer

The Service Layer contains business logic.

Responsibilities include:

- Database Operations
- Business Rules
- Validation
- ViewModel Preparation
- File Handling
- Report Generation

Controllers communicate with Services rather than directly accessing the database.

---

## Data Layer

Database access is performed through:

- Entity Framework Database First
- EDMX Model
- SQL Server

Generated entity classes should never be edited manually.

---

## ViewModel Layer

ViewModels isolate the UI from database entities.

Separate ViewModels should be used for:

- Create
- Edit
- Details
- List
- Search
- Dashboard

---

# Application Areas

## Public Website

Provides public-facing functionality:

- Home
- News
- Events
- Gallery
- Videos
- Downloads
- About Leader
- Search
- Citizen Connect

---

## Admin Area

Provides management functionality:

- Website Settings
- Header/Footer
- Hero Slider
- News
- Gallery
- Videos
- Downloads
- Polls
- Events
- Voters
- Election War Room
- Reports
- Menu Management
- Role Permissions

---

# File Storage

Uploaded files are stored under the Uploads directory.

Examples include:

- Images
- Videos
- Documents
- Hero Slider Images
- Gallery Images
- Media Files

Database stores only the file paths.

---

# Security Architecture

Current security includes:

- Forms Authentication
- Role-Based Authorization
- Session Management
- Anti-Forgery Tokens
- Password Hashing

---

# Development Principles

Every new module should:

- Follow existing folder structure.
- Use Services for business logic.
- Use ViewModels for presentation.
- Keep Controllers lightweight.
- Reuse existing components whenever possible.

---

# Future Expansion

The architecture is designed to support additional modules without major structural changes.

New features should integrate with the existing MVC + Service Layer pattern to maintain consistency.