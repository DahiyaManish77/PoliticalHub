# AI_CONTEXT.md

# PoliticalLeaderPortal - AI Project Context

> This document provides AI coding assistants with a complete understanding of the project architecture, business domain, development philosophy, workflows, and implementation guidelines.

---

# Project Overview

PoliticalLeaderPortal is an enterprise-grade ASP.NET MVC 5 web application designed to manage a political leader's complete digital ecosystem.

The application combines:

- Public Website
- Enterprise CMS
- Election Campaign Management
- Election War Room
- Media Management
- News Management
- Events Management
- Voter Management
- Public Engagement
- Digital Membership
- Reports
- Administration

This is a long-term commercial application intended to evolve through multiple releases while maintaining a consistent architecture.

---

# Development Philosophy

Every new feature should:

- Follow the existing MVC architecture.
- Reuse existing Services whenever practical.
- Avoid duplicate business logic.
- Maintain a consistent UI.
- Preserve backward compatibility.
- Keep Controllers lightweight.
- Keep business logic inside Services.
- Use ViewModels for presentation.

---

# Technology Stack

Backend

- ASP.NET MVC 5
- .NET Framework 4.8
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

Reports

- PDF generation infrastructure

---

# High-Level Architecture

Presentation Layer

- Controllers
- Views
- Partial Views
- Shared Layouts

Business Layer

- Services

Presentation Models

- ViewModels

Persistence Layer

- Entity Framework Database First

Database

- SQL Server

File Storage

- Uploads folder

Configuration

- Web.config
- App_Start

---

# Business Modules

The application currently includes the following major modules.

## Website CMS

- Website Settings
- Header
- Footer
- Navigation
- Home Sections
- Hero Slider

---

## Content Management

- Latest News
- News Ticker
- Gallery
- Video Gallery
- Media Coverage
- Downloads

---

## Public Website

- Home
- About Leader
- Search
- Events
- News
- Gallery
- Videos
- Downloads

---

## Citizen Engagement

- Citizen Connect
- Campaign Poll
- Contact Forms

---

## Campaign Management

- Election War Room
- Campaign Dashboard
- Campaign Activities
- Alerts
- Social Media
- Teams
- Booths
- Vehicles
- Attendance
- Expenses
- Guests
- Tasks
- Jan Sampark

---

## Administration

- Menu Management
- Role Permissions
- User Management
- Dashboard

---

## Voter Management

- Voter Records
- Voter Roll
- Backup
- Backup Settings

---

## Member Services

- Digital Member Card
- PDF Generation

---

# Folder Structure

Typical project folders include:

Areas/

Controllers/

Models/

Services/

ViewModels/

Views/

Content/

Scripts/

Infrastructure/

Uploads/

App_Start/

App_Data/

---

# Coding Standards

The project follows:

- MVC Pattern
- Service Layer
- ViewModel Pattern
- Separation of Concerns
- Database First Architecture

---

# UI Philosophy

Every page should:

- Be fully responsive.
- Follow Bootstrap 5.
- Maintain visual consistency.
- Use compact layouts.
- Be optimized for desktop, tablet, and mobile devices.

---

# Database Philosophy

The database is the source of truth.

Entity Framework Database First is used.

Database schema changes should be reflected by updating the EDMX model.

Avoid manually editing generated entity classes.

---

# Security Model

Security includes:

- Forms Authentication
- Session-based user information
- Role-based authorization
- Anti-forgery validation
- Password hashing
- Input validation

Future enhancements should strengthen security without breaking compatibility.

---

# File Upload Strategy

Uploaded files are stored under the Uploads directory.

Every upload should include:

- Extension validation
- File size validation
- MIME type validation
- Secure file naming
- Proper folder organization

---

# Performance Goals

The application should remain responsive while supporting large datasets.

Recommended practices include:

- Pagination
- Efficient database queries
- Reuse of Services
- Minimized duplicate queries
- Optimized ViewModels

---

# Error Handling

Exceptions should be logged.

Users should receive friendly error messages.

Internal implementation details should never be exposed publicly.

---

# Future Development Principles

When creating new modules:

- Follow the existing architecture.
- Reuse existing patterns.
- Maintain naming consistency.
- Keep Controllers lightweight.
- Place business logic inside Services.
- Create dedicated ViewModels.
- Keep JavaScript modular.
- Keep CSS centralized.

---

# AI Instructions

Before generating code:

1. Understand the existing implementation.
2. Reuse current architecture.
3. Minimize unnecessary changes.
4. Explain the proposed solution.
5. Wait for user approval before editing files.

---

# Repository Goal

The long-term objective is to build a scalable, maintainable, enterprise-quality platform while preserving architectural consistency across all future modules.