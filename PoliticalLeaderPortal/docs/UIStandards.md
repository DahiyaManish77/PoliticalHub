# UI Standards

# PoliticalLeaderPortal UI & UX Standards

## Purpose

This document defines the official UI/UX standards for PoliticalLeaderPortal.

Every page, partial view, popup, modal, form, table, card, and dashboard must follow these standards.

The objective is to maintain a modern, premium, enterprise-level user experience across the entire application.

---

# Design Philosophy

The application should look:

- Professional
- Premium
- Enterprise-grade
- Clean
- Modern
- Fast
- Mobile Friendly
- Easy to Understand
- Consistent

Avoid outdated designs.

Avoid clutter.

Avoid oversized controls.

---

# Responsive Design

Every page must work perfectly on:

- Mobile Phones
- Tablets
- Laptops
- Desktop
- Large Monitors
- 4K Displays

The UI should automatically adapt.

No horizontal scrolling.

No broken layouts.

No overlapping controls.

No cropped text.

---

# Mobile First

Design should start from mobile.

Then scale naturally for:

- Tablet
- Laptop
- Desktop

Never create desktop-only pages.

---

# Bootstrap Standards

Use Bootstrap 5 only.

Prefer Bootstrap utility classes before creating custom CSS.

Use Bootstrap Grid consistently.

Avoid nested grids unless necessary.

---

# Cards

All cards should:

- Rounded corners
- Soft shadow
- Small padding
- Consistent spacing
- Equal height where practical

Avoid oversized cards.

---

# Buttons

Buttons should have:

- Consistent height
- Bootstrap styling
- Icons where appropriate
- Loading state
- Disabled state

Primary buttons should stand out.

Danger buttons should require confirmation.

---

# Forms

Forms should:

- Compact layout
- Equal spacing
- Responsive columns
- Floating labels or consistent labels
- Client + Server validation
- Required field indicator

Avoid extremely long forms.

Split into sections when needed.

---

# Tables

Tables should support:

- Mobile responsiveness
- Search
- Pagination
- Sorting
- Export
- Fixed action buttons

Avoid horizontal scrolling whenever possible.

---

# Typography

Use one consistent font family throughout the application.

Recommended:

- Poppins
- Inter

Maintain consistent:

- Font sizes
- Font weights
- Line height

---

# Icons

Use Font Awesome.

Avoid mixing multiple icon libraries.

Icons should always match the action.

---

# Images

Images should:

- Load lazily where practical.
- Maintain aspect ratio.
- Be responsive.
- Support WebP when appropriate.

---

# Color Standards

Maintain a consistent brand color palette.

Primary

Secondary

Success

Warning

Danger

Info

Do not introduce random colors.

---

# Animations

Use subtle animations only.

Avoid excessive motion.

Prefer:

- Fade
- Slide
- Smooth hover

---

# Navigation

Menus should:

- Collapse correctly on mobile
- Highlight active page
- Support nested menus
- Load quickly

---

# Dashboard

Dashboard widgets should:

- Align properly
- Maintain equal heights
- Display responsive charts
- Support compact mobile layout

---

# Modals

Use Bootstrap Modals.

Support:

- Keyboard close
- Mobile responsiveness
- Scrollable content
- Large forms

---

# Toasts & Alerts

Use SweetAlert2.

Avoid browser alert().

Use toast notifications for success messages.

---

# Loading Indicators

Every AJAX request should display:

- Loading spinner
- Progress indicator where appropriate

Avoid blank screens.

---

# Google Translator

The website must include a fully responsive Google Translate integration.

Requirements:

- Use Google Translate API/widget without displaying the default Google floating popup/banner.
- Hide the standard Google Translate toolbar/banner using approved CSS techniques where appropriate.
- Display a custom language selector that matches the website's branding.
- Apply custom CSS so the translator appears as a professional part of the website.
- Match the application's typography, colors, spacing, border radius, and button styles.
- Support desktop, tablet, and mobile layouts.
- The language selector should integrate naturally into the website header or navigation.
- Translation should not break the responsive layout.
- The translator should not overlap menus, modals, or other UI components.
- Avoid abrupt layout shifts when switching languages.

---

# Accessibility

Every page should:

- Be keyboard accessible
- Use proper labels
- Have sufficient color contrast
- Support screen readers where practical

---

# AI UI Rules

When generating UI:

- Follow existing design language.
- Reuse existing CSS.
- Keep spacing consistent.
- Prefer reusable partial views.
- Avoid inline CSS.
- Avoid inline JavaScript.
- Ensure every page is fully responsive before completion.