# AGENTS.md

# AI Coding Instructions for PoliticalLeaderPortal

This file contains mandatory development rules for all AI coding assistants working on this repository.

Failure to follow these rules may introduce architectural inconsistencies.

---

# Project Overview

PoliticalLeaderPortal is an enterprise-level ASP.NET MVC 5 web application combining:

- Public Website
- Content Management System (CMS)
- Political Campaign Management
- Election War Room
- Voter Management
- Digital Member Card System
- Media Management
- Poll & Survey System
- Role-Based Administration

This is a monolithic MVC application designed for long-term enterprise maintenance.

---

# Technology Stack

Framework

- ASP.NET MVC 5
- .NET Framework 4.8

Language

- C#
- Razor

Database

- SQL Server
- Entity Framework Database First (EDMX)

Frontend

- Bootstrap 5
- jQuery
- AJAX
- HTML5
- CSS3

Authentication

- Forms Authentication

---

# Architecture Rules

Always follow the existing architecture.

Do NOT introduce new architectural patterns unless explicitly requested.

The application uses:

- MVC
- Service Layer
- ViewModels
- Database First (EDMX)

---

# Controller Rules

Controllers should remain thin.

Controllers should:

- Receive requests
- Validate input
- Call Services
- Return Views or JSON

Controllers should NOT contain:

- SQL
- Complex business logic
- Large helper methods
- Data access logic

---

# Service Rules

Business logic belongs inside Service classes.

Services should:

- Query the database
- Apply business rules
- Perform calculations
- Prepare ViewModels
- Handle transactions when required

Do not move business logic into Controllers.

---

# ViewModel Rules

Always use ViewModels.

Do NOT expose Entity Framework entities directly to Razor Views unless specifically required.

Create dedicated ViewModels for:

- Create
- Edit
- Details
- List
- Search
- Dashboard

---

# Database Rules

The project uses Entity Framework Database First.

Rules:

- Never manually edit generated EDMX classes.
- Never modify generated entity files.
- Update the EDMX after database schema changes.
- Keep database changes backward compatible whenever possible.

---

# UI / UX Rules

All UI must be:

- Responsive
- Mobile First
- Bootstrap 5 compatible
- Professional
- Compact
- Enterprise-grade
- Consistent across modules

Avoid inconsistent spacing, typography, or component styles.

---

# JavaScript Rules

Prefer:

- jQuery
- AJAX
- Modular JavaScript files

Avoid large inline scripts inside Razor Views.

---

# CSS Rules

Use centralized CSS files.

Avoid inline styles.

Reuse existing classes before creating new ones.

---

# Security Rules

Always:

- Validate user input.
- Use Anti-Forgery Tokens for POST requests.
- Validate uploaded files.
- Prevent unauthorized access.
- Respect Role-Based Authorization.

Never hardcode secrets or credentials.

---

# Performance Rules

Minimize:

- Duplicate database queries
- Large ViewModels
- Repeated service calls

Prefer efficient queries and paging for large datasets.

---

# Error Handling

Handle exceptions gracefully.

Return meaningful validation messages.

Avoid exposing internal exception details to end users.

---

# File Organization

Follow the existing folder structure.

New modules should include:

- Controller
- Service
- ViewModels
- Views
- JavaScript
- CSS

Keep module assets grouped together whenever practical.

---

# Coding Style

Use:

- PascalCase for classes, methods, and properties.
- Meaningful variable names.
- Clear method names.
- Small reusable methods.

Keep formatting consistent with the existing project.

---

# AI Behaviour Rules

Before modifying code:

1. Analyze existing implementation.
2. Reuse existing architecture.
3. Explain the proposed changes.
4. List affected files.
5. Wait for user approval before making changes.

Never rewrite unrelated modules.

---

# Git Rules

Do not modify unrelated files.

Keep commits focused on a single feature or bug fix.

Avoid unnecessary formatting-only changes.

---

# Documentation

Whenever a new module is created, update relevant documentation if needed.

---

# Final Rule

When uncertain, prefer consistency with the existing repository over introducing new frameworks, libraries, or patterns.