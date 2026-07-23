# Development Workflow

# PoliticalLeaderPortal Development Workflow

## Purpose

This document defines the official development workflow for PoliticalLeaderPortal.

Every developer and AI coding assistant should follow this process to ensure code quality, architectural consistency, and maintainability.

The objective is to build features in a predictable, reviewable, and production-ready manner.

---

# Development Philosophy

Every feature should be:

- Planned
- Reviewed
- Designed
- Developed
- Tested
- Approved
- Committed
- Deployed

Never skip steps for convenience.

---

# Technology Stack

Development must remain consistent with:

- ASP.NET MVC 5
- .NET Framework 4.8
- Entity Framework Database First (EDMX)
- SQL Server
- Bootstrap 5
- jQuery
- AJAX
- Razor Views

---

# Standard Development Lifecycle

Every new feature should follow this sequence.

## Step 1 – Requirement Analysis

Understand:

- Business objective
- End users
- Required functionality
- Existing related modules
- Dependencies

Do not start coding immediately.

---

## Step 2 – Existing Code Review

Before writing code:

Review:

- Controllers
- Services
- ViewModels
- Views
- JavaScript
- CSS
- Database

Reuse existing code whenever possible.

Avoid duplicate implementations.

---

## Step 3 – Solution Design

Prepare:

- Architecture
- Database impact
- UI changes
- Security impact
- Performance considerations

Explain the implementation plan before coding.

---

## Step 4 – User Approval

AI should:

- Explain the approach.
- List files to be created or modified.
- Wait for approval.

Do not modify files until approval is received.

---

## Step 5 – Database Changes

If database changes are required:

- Design tables
- Review relationships
- Review indexes
- Update SQL scripts
- Update EDMX
- Verify generated entities

Never edit generated Entity Framework classes manually.

---

## Step 6 – Backend Development

Create or update:

- Services
- ViewModels
- Controllers

Business logic belongs inside Services.

Controllers remain lightweight.

---

## Step 7 – Frontend Development

Create or update:

- Razor Views
- Partial Views
- JavaScript
- CSS

Follow UIStandards.md.

Ensure complete responsiveness.

---

## Step 8 – Validation

Implement:

- Client-side validation
- Server-side validation
- Security validation
- Permission validation

Never rely only on client-side validation.

---

## Step 9 – Testing

Verify:

- Functional testing
- Responsive testing
- Permission testing
- Validation testing
- Error handling
- Performance

Test on:

- Mobile
- Tablet
- Laptop
- Desktop

---

## Step 10 – Documentation

If architecture changes:

Update:

- README.md
- AGENTS.md
- AI_CONTEXT.md
- Relevant files inside docs/

Documentation should evolve with the project.

---

## Step 11 – Git

Check repository status:

git status

Review changes.

Commit only related files.

Use meaningful commit messages.

Example:

Add Survey Management Module

Fix Hero Slider Mobile Layout

Improve Election Dashboard Performance

---

## Step 12 – Push

Push to GitHub after successful testing.

Keep main branch stable.

---

# AI Development Workflow

When AI receives a request it should follow this sequence.

1. Understand the requirement.
2. Analyze the repository.
3. Reuse existing architecture.
4. Explain the implementation.
5. List affected files.
6. Wait for approval.
7. Generate code.
8. Explain changes.
9. Recommend testing.
10. Update documentation if necessary.

---

# Code Review Checklist

Before approving code ensure:

✓ Controllers remain lightweight

✓ Business logic is inside Services

✓ ViewModels are used

✓ UI is responsive

✓ Validation exists

✓ Security is maintained

✓ Naming conventions are followed

✓ CSS is centralized

✓ JavaScript is centralized

✓ Documentation updated if required

---

# Module Creation Workflow

Every new module should include:

Controller

Service

ViewModels

Views

JavaScript

CSS

Menu (if applicable)

Permissions

Documentation

Testing

---

# Bug Fix Workflow

1. Reproduce the issue.
2. Identify root cause.
3. Fix only the required code.
4. Avoid unrelated refactoring.
5. Test regression scenarios.
6. Commit with clear message.

---

# Performance Review

Before completing any feature:

Review:

- SQL queries
- ViewModels
- AJAX requests
- Images
- CSS
- JavaScript
- Response time

---

# Deployment Readiness

Before deployment verify:

- debug="false"
- Connection strings
- Upload permissions
- IIS configuration
- Database backup
- Documentation updated

---

# Long-Term Goal

PoliticalLeaderPortal should continue growing as a stable, enterprise-quality platform.

Every new feature must integrate seamlessly with the existing architecture while maintaining code quality, performance, security, and a consistent user experience.