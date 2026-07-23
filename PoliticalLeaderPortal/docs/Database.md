# Database

# PoliticalLeaderPortal Database Documentation

## Overview

PoliticalLeaderPortal uses Microsoft SQL Server as its primary database and Entity Framework 6 Database First (EDMX) as the Object Relational Mapping (ORM) framework.

The database is considered the single source of truth for application data.

All Entity Framework models are generated from the database schema.

---

# Database Technology

| Item | Value |
|------|-------|
| Database Engine | Microsoft SQL Server |
| ORM | Entity Framework 6 |
| Model Type | Database First (EDMX) |
| Connection | SQL Server Connection String |

---

# Database Design Principles

The database is designed around the following principles:

- Normalized data structure
- Primary and Foreign Key relationships
- Business entities separated into logical modules
- Lookup tables for reusable values
- Audit-friendly design where applicable
- Scalable structure for future expansion

---

# Entity Framework

The project uses:

- Entity Framework 6
- Database First
- EDMX Designer

### Rules

- Never manually edit generated Entity classes.
- Never manually edit the generated DbContext.
- Always update the EDMX after database schema changes.
- Regenerate models after modifying the database.

---

# Connection Management

Database connections are configured through **Web.config**.

Connection strings should be environment-specific:

- Development
- Testing
- Staging
- Production

Sensitive credentials should never be committed to source control.

---

# Major Functional Areas

The database supports several business domains including:

## Website CMS

- Website Settings
- Header
- Footer
- Navigation
- Hero Slider
- Home Sections

---

## Content Management

- Latest News
- Gallery
- Video Gallery
- Media Coverage
- Downloads
- Events

---

## Campaign Management

- Campaigns
- Election War Room
- Booths
- Teams
- Attendance
- Vehicles
- Guests
- Expenses
- Alerts
- Tasks

---

## Citizen Engagement

- Citizen Connect
- Campaign Polls
- Public Feedback

---

## Voter Management

- Voter Records
- Voter Roll
- Backup
- Backup Settings

---

## Membership

- Digital Member Cards
- Member Information

---

# Data Integrity

Every new table should include appropriate:

- Primary Keys
- Foreign Keys
- Constraints
- Indexes where required

Avoid duplicate data whenever possible.

---

# Naming Standards

Recommended naming conventions:

Tables

- Singular or consistent project naming convention

Columns

- PascalCase

Primary Key

- Id

Foreign Key

- EntityNameId

Boolean Fields

- IsActive
- IsDeleted
- IsPublished

Date Fields

- CreatedDate
- ModifiedDate

---

# Database Change Workflow

When adding new database functionality:

1. Create or modify SQL tables.
2. Test relationships and constraints.
3. Update the EDMX model.
4. Regenerate Entity classes.
5. Update Services.
6. Update ViewModels.
7. Update Controllers.
8. Update Views.

---

# Performance Guidelines

To maintain performance:

- Retrieve only required columns.
- Use filtering whenever possible.
- Implement paging for large datasets.
- Avoid unnecessary database calls.
- Minimize repeated queries inside loops.

---

# Transactions

Business operations affecting multiple tables should execute within a transaction to ensure consistency.

---

# File Storage

Large binary files should not be stored directly in SQL Server unless there is a specific requirement.

Preferred approach:

- Store files in the Uploads directory.
- Store only the relative file path in the database.

---

# Backup Strategy

Recommended:

- Daily SQL backups
- Weekly full backups
- Off-site backup copies
- Test restoration periodically

---

# Future Expansion

The database should remain modular.

New modules should:

- Reuse existing lookup tables where practical.
- Maintain naming consistency.
- Preserve backward compatibility.
- Avoid unnecessary duplication of entities.

---

# AI Guidance

When generating database-related code:

- Reuse existing entities whenever possible.
- Respect existing relationships.
- Avoid schema redesign unless explicitly requested.
- Keep compatibility with Entity Framework Database First.