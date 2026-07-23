# Learned Project Context

This note captures the project and prompt conventions that should guide future work in `PoliticalLeaderPortal` and related portal-style projects.

## Locked Technology Stack

- ASP.NET MVC 5 on .NET Framework 4.8.
- C# 5 only.
- SQL Server.
- Entity Framework Database First with EDMX.
- Bootstrap 5, jQuery, AJAX.
- SweetAlert2 for user actions such as save, update, delete, status changes and confirmations.
- AdminLTE-style admin panel, modernized with a premium enterprise UI.
- Visual Studio 2022.

Do not introduce ASP.NET Core, Blazor, React, Angular, Vue, Razor Pages, Minimal APIs, CQRS, Mediator, microservices, DDD or complex repository abstractions unless explicitly requested.

## Architecture Rules

- Continue the existing project architecture. Do not restart or redesign it.
- Database First only. Never manually edit EF-generated EDMX entity classes.
- Prefer simple enterprise architecture that one developer can maintain.
- Use ViewModels, Services, Controllers, Razor Views/Partial Views, AJAX, validation, logging and responsive UI.
- Keep module boundaries practical and readable.
- For Election War Room, prefer one `ElectionWarRoomController` and one `ElectionWarRoomService` unless separation is truly needed.
- Everything reusable on the public website should be a partial view and admin-configurable.

## Political Leader Portal Product Vision

The portal is not just a website. It is a white-label political digital ecosystem for Indian leaders, candidates, parties and campaign organizations.

The product combines:

- Premium public website.
- Admin CMS.
- Election War Room.
- Constituency management.
- Citizen engagement.
- Volunteer and team management.
- Media/gallery/video/document management.
- Campaign analytics.
- Public communication and feedback.

The platform should feel sellable as an enterprise product.

## Public Website Standards

- Premium, clean, modern, minimal, fast and mobile-first.
- Mega menu inspired by high-quality Indian political sites, but original in implementation and assets.
- Every homepage section should be a partial view.
- Admin should manage sections, images, videos, documents, hero slider, social links and display flags.
- Sections should be compact, sleek, responsive and not oversized.
- Use consistent typography, spacing, card radius, button styles and color palette.

Common partial/module areas:

- Header, top bar, mega menu, search, language selector.
- Hero slider with image/video support.
- News, media coverage, events, public meetings.
- Image gallery and video gallery.
- Documents/downloads/manifesto.
- Contact, volunteer, suggestions, join party.
- Statistics, timeline, achievements, vision, mission.
- Footer, quick links, social links.

## Admin UI Standard

Admin pages must look like a premium enterprise admin panel, inspired by Microsoft Admin, Metronic, Tabler, JetAdmin and modern AdminLTE.

Locked visual standards:

- Primary color: `#0D6EFD`.
- Background: `#F5F7FB`.
- Cards: white.
- Border: `#E9ECEF`.
- Subtle hover states.
- No heavy gradients or dark shadows.
- Compact, professional spacing.

Every admin page should start with:

- Page title.
- Short description.
- Right-aligned action buttons.

Forms:

- Maximum three fields per row on desktop.
- Two fields on tablet.
- One field on mobile.
- Consistent labels, rounded controls, validation messages and button styling.
- No horizontal scrolling.

Tables:

- Sleek, compact, responsive.
- Avoid DataTables reinitialization.
- Keep columns aligned with data source shape.

Actions:

- SweetAlert2 confirmation and success/error feedback for create, update, delete and status changes.

## Election War Room Scope

Election War Room is an Admin module for real Indian campaign operations.

Core modules:

- Dashboard / live campaign summary.
- Events, rallies, public meetings, road shows, corner meetings.
- Tasks and task activity.
- Volunteers and teams.
- Booth master, booth visits, booth analytics.
- Jan Sampark / public grievances / village visits / citizen issues.
- Vehicles and allocation.
- Attendance.
- Guests.
- Arrangements.
- Expenses.
- Media.
- Polls.
- Campaign alerts.
- Campaign calendar.

Existing/final table set mentioned in prompts:

- `EventMaster`
- `EventVehicle`
- `EventAttendance`
- `EventTeam`
- `EventTeamMember`
- `EventGuest`
- `EventArrangement`
- `EventExpense`
- `EventMedia`
- `EventTask`
- `EventTaskActivity`
- `EventPoll`
- `EventPollOption`
- `EventPollResponse`
- `ElectionBooth`
- `ElectionBoothVisit`
- `JanSampark`
- `CampaignAlert`

Do not redesign these tables unless there is a serious issue.

## Election War Room UX Priorities

- Useful for district, block, village and booth-level workers.
- Mobile responsive.
- Simple, practical screens.
- Dashboard should show today’s events, pending tasks, booth coverage, volunteer status, alerts and daily summary.
- Data-entry screens should support create/edit/update/delete with permissions.
- Track responsible persons, attendance, expected vs actual crowd, village-wise participation, vehicles, food, expenses, media and follow-up.

## Voter / Constituency Concepts

When adding voter or constituency features, use Indian election fields:

- State, District, Block, Assembly Constituency, Parliament Constituency.
- Village, ward, booth, house number.
- Voter name, guardian name, age, gender, mobile where legally available.
- Photo/Aadhaar only where appropriate and legally permitted.
- Duplicate prevention is mandatory.
- Official voter-list handling should rely on lawful uploads/imports from official sources, not scraping unofficial personal data.

## BhartiyaKKUnionPortal Context

The Bhartiya Kisan Union Krantikari project is another ASP.NET MVC 5 / .NET Framework style portal used as reference for:

- YouTube channel/playlist sync patterns.
- Membership/application modules.
- Enterprise admin patterns.
- Existing web.config-style app settings.

Do not copy visual design directly if user asks for a different design, but reuse proven architectural ideas carefully.

## Prompt and Coaching Patterns

User also keeps reusable prompts for:

- English fluency coaching.
- Advocate management.
- Applicant letter module.
- Membership modules.
- Hit counter.
- PDF card generation.
- Admin UI design standards.

For English coaching:

- Speak simply first.
- Correct mistakes politely.
- Explain in Hindi when helpful.
- Ask one question at a time.
- Give better alternatives, vocabulary, daily-use sentences and speaking challenges.

## Default Implementation Behavior

When working on these projects:

- Read existing code before editing.
- Keep changes scoped.
- Follow MVC 5 / C# 5 syntax.
- Use `rg` for searching where available.
- Use `apply_patch` for manual edits.
- Build/verify when practical.
- Do not revert user changes.
- Keep UI compact, professional and responsive.
