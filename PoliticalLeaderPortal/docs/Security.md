# Security Guidelines

# PoliticalLeaderPortal Security Standards

## Purpose

This document defines the official security standards for PoliticalLeaderPortal.

All developers and AI coding assistants must follow these guidelines when creating or modifying code.

Security must never be sacrificed for convenience.

---

# Security Philosophy

The application should protect:

- User Accounts
- Administrative Functions
- Personal Information
- Uploaded Files
- Database
- Configuration
- API Keys
- Reports

---

# Authentication

Current implementation:

- ASP.NET Forms Authentication
- Session-based authentication
- Password hashing

Requirements

- Never store plain text passwords.
- Always hash passwords.
- Logout must destroy authentication session.
- Login pages must use Anti-Forgery validation.

---

# Authorization

All administrative pages must require authorization.

Use the existing authorization mechanism already implemented in the project.

Every new module must verify permissions before allowing:

- View
- Create
- Edit
- Delete
- Export
- Print

---

# Role-Based Access

Features must only be available to authorized roles.

Do not rely solely on hidden buttons in the UI.

Authorization must always be enforced on the server.

---

# Session Security

Store only required information inside Session.

Avoid storing sensitive business information in Session.

Always check Session values before use.

Handle expired sessions gracefully.

---

# Password Security

Passwords must:

- Be hashed
- Never be reversible
- Never be logged
- Never be displayed

Password reset functionality should generate secure temporary tokens.

---

# Input Validation

Every input must be validated.

Validate:

- Required fields
- Length
- Data type
- Numeric ranges
- Dates
- Email
- Phone numbers

Never trust client-side validation alone.

---

# SQL Security

Always use Entity Framework or parameterized queries.

Never concatenate SQL strings using user input.

Avoid dynamic SQL unless absolutely necessary.

---

# Cross-Site Request Forgery (CSRF)

Every POST action should use:

- ValidateAntiForgeryToken

Every form should include:

- AntiForgeryToken

---

# Cross-Site Scripting (XSS)

Always HTML encode user-generated content before rendering.

Avoid rendering raw HTML unless it has been explicitly sanitized.

---

# File Upload Security

Validate:

- File extension
- MIME type
- Maximum size
- File name
- Duplicate names

Generate unique file names.

Store uploads outside executable paths where practical.

Never execute uploaded files.

---

# Download Security

Downloads should verify:

- User permissions (where applicable)
- File existence
- Valid path

Prevent path traversal attacks.

---

# API Keys

Never hardcode:

- API Keys
- Access Tokens
- Passwords
- Connection Strings

Production secrets should come from secure configuration or environment-specific settings.

---

# Database Security

Grant the application only the permissions it requires.

Avoid using highly privileged SQL accounts.

Back up the database regularly and test restore procedures.

---

# Error Handling

Do not expose:

- Stack traces
- SQL errors
- Internal file paths
- Connection strings

Show friendly messages to users and log detailed errors internally.

---

# Logging

Log:

- Authentication failures
- Authorization failures
- Critical exceptions
- Security-related events

Do not log passwords or other sensitive credentials.

---

# Auditing

Important administrative actions should be auditable, such as:

- User management
- Permission changes
- Content publication
- File deletion
- Configuration updates

---

# Deployment Security

Before production deployment:

- Set debug="false"
- Review Web.config
- Remove development secrets
- Verify HTTPS configuration
- Restrict server access
- Validate file permissions

---

# AI Rules

When generating code:

- Preserve existing security patterns.
- Do not bypass authorization.
- Do not disable validation.
- Do not weaken authentication.
- Prefer secure defaults.