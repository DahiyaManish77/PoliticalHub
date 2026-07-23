# PoliticalLeaderPortal Completion Roadmap

This roadmap follows the attached master prompt and keeps the existing ASP.NET MVC 5, C# 5, SQL Server, EF Database First, Bootstrap, jQuery and AdminLTE-style architecture.

## Current Execution Order

1. Project setup and alignment
   - Keep one centralized design system for public and admin UI.
   - Keep homepage sections as reusable partial views.
   - Keep admin modules grouped under the final sidebar structure.
   - Keep all new work inside the existing MVC 5 architecture.

2. Public website homepage
   - Use the final homepage order: marquee, hero, leader introduction, statistics, events, campaign activity/news, gallery, videos, media coverage, downloads, volunteer CTA, contact CTA and footer.
   - Make every section compact, professional, responsive and consistent.
   - Preserve CMS-driven action methods wherever available.

3. Admin dashboard and shell
   - Match the provided dashboard reference: dark blue sidebar, clean white topbar, compact metric cards, quick actions, progress panels, recent activity and tables.
   - Final sidebar groups: Dashboard, Website CMS, Constituency, People, Organization, Campaign, Events, Communication, Social Media, Documents, Reports, Administration and My Account.

4. Module-by-module completion
   - Website CMS
   - Constituency
   - People and volunteers
   - Organization
   - Campaign
   - Events
   - Communication
   - Social Media
   - Documents
   - Reports
   - Administration

## Non-Negotiable Standards

- No random pages or duplicate menus.
- No new framework migration.
- No manual edits to EF-generated entity files.
- Every page uses the same typography, buttons, cards, tables, spacing and statuses.
- Forms must be grouped, responsive and validation-friendly.
- Listings must be searchable, compact and mobile-safe.
- Sensitive data must be masked or permission-controlled.
- Dashboard pages must show summaries and recent items only, not heavy full-table loads.

## Immediate Slice

The active slice is public homepage completion. After this slice builds successfully, the next slice is admin dashboard/sidebar redesign to match the provided reference image.
