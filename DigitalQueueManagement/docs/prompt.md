I’m preparing the “Digital Queue & Counter Management” .NET MAUI use-case sample for an AI cloud harness. Before implementing the application, create four separate harness-planning documents in the DigitalQueueManagement project root:

1. indent.md
2. specs.md
3. plan.md
4. task.md

Use the filename “indent.md” exactly as listed. Do not implement application code or modify existing project files or source documentation. Do not create additional planning files.

SOURCE REVIEW — REQUIRED

Review the complete docs/ folder recursively before drafting the four documents. Do not assume that the tagged overview is the only source.

The current docs folder contains approximately 742 files:
- docs/Design Flow/: overview.md and Mobile-desktop-screens.md
- docs/Desktop Designs/: screen and bottom-sheet DESIGN.md files, corresponding code.html design references, interaction_navigation_mappings.md, and visual assets
- docs/Mobile Designs/: screen and bottom-sheet DESIGN.md files, corresponding code.html design references, interaction_navigation_mappings.md, and visual assets
- docs/skills/.agents/skills/: bundled Syncfusion MAUI skill definitions and their reference documents
- Additional image and font assets under docs/

First inventory the files and use that inventory to verify coverage. Read the flow documents and all screen, modal/bottom-sheet, and interaction-mapping documents. Inspect the HTML design references and visual assets as design sources. Review the bundled Syncfusion skill definitions and use the relevant references to inform component and implementation requirements. Account for the remaining skill files in the inventory; do not apply unrelated skills to this app. Do not claim to have reviewed a file unless you actually inspected it.

Use these sources according to their purpose:
- The overview and flow documents define business scope, queue behavior, and application goals.
- The desktop/mobile DESIGN.md, code.html, images, and interaction mappings define screen layout, visual behavior, and navigation.
- The Syncfusion skills and references inform feasible MAUI control usage and implementation constraints.

If any source documents conflict or leave behavior unclear, record the conflict or open question in the appropriate document. Do not silently invent a resolution. Distinguish documented requirements from proposed assumptions.

APPLICATION CONTEXT FROM THE DOCUMENTATION

The application is a showcase service-center queue manager using local sample data. It is not a production queue-management system.

Business scope:
- One service center with three counters.
- Service categories: General Enquiry, Payments, and Customer Support.
- Priorities: Normal and Priority.
- Approximately 20–30 predefined local tokens.
- Token statuses: Waiting, Serving, Skipped, and Completed. A token has only one status at a time.
- Call Next selects the first available Waiting token and assigns it to the selected counter as Serving.
- Skip moves a Serving token to Skipped and makes its counter available.
- Recall moves a Skipped token back to Waiting.
- Transfer moves a currently Serving token to another counter.
- Complete marks a Serving token Completed and frees its counter.
- Estimated wait is a simple estimate: Waiting token count × 4 minutes. It is not an intelligent prediction.
- Reset restores the original demonstration data.

Data and architecture:
- Use local sample data only. No authentication, backend, database, web API, real-time synchronization, multiple branches, appointment scheduling, AI prediction, complex routing, or emergency queue handling.
- The documented recommended pattern is MVVM, with Models, Services, ViewModels, Views, and Resources.
- QueueService owns token allocation, queue ordering, status updates, and analytics calculations.
- DemoDataService creates predefined tokens and counters and restores the sample data.
- Keep architecture and implementation scope appropriate for a .NET MAUI showcase.

UI and documented component mapping:
- Support desktop and mobile layouts.
- The documentation maps DataGrid to the token directory, ListView to counter cards and token lists, Radial Gauge to queue load, charts to analytics, Badge View to statuses, Segmented Control to filtering/counter selection, and Popup to token details and confirmations.
- The visual direction is a clean, professional service-center dashboard using blue and teal with clear status colors, readable token numbers, touch-friendly mobile layouts, and restrained animation. Avoid glassmorphism and futuristic styling.
- Review the design docs for the exact desktop/mobile screens, modal and bottom-sheet contents, and interactions. Do not substitute a desktop-style grid for mobile layouts where the designs call for stacked cards.

IMPORTANT: DOCUMENTED AMBIGUITIES TO CHECK
The overview, screen descriptions, and navigation mappings may describe different numbers or groupings of screens—for example, a two-screen acceptance statement versus separate Overview, Tokens, Counters, and Insights areas, and mobile bottom navigation versus a Manage screen with segmented tabs. Compare all relevant sources, explain the mismatch, and record a recommended assumption or an explicit open decision. Do not pretend these descriptions are already consistent.

DOCUMENT REQUIREMENTS

indent.md
- State the use-case intent, problem, intended users, goals, scope, non-goals, constraints, and design principles.
- Explain that this is a local-data showcase and an AI-harness development exercise.
- Include important assumptions, unresolved decisions, and traceability to the source documentation.

specs.md
- Define functional and non-functional requirements.
- Specify the token and counter data fields, status rules, queue workflows, wait-time calculation, sample-data/reset behavior, and dashboard/analytics update expectations.
- Specify desktop and mobile navigation, screens, interactions, popups/bottom sheets, and responsive behavior based on the design sources.
- Map documented Syncfusion controls to their intended uses; use the relevant bundled skill references to identify any implementation considerations.
- Provide measurable acceptance criteria and trace each significant requirement to its source document.
- Separate documented facts from assumptions and open questions.

plan.md
- Provide a phased implementation plan with dependencies, expected outcomes, and verification for each phase.
- Cover project setup and Syncfusion integration, domain models/services/sample data, navigation, overview, token directory, counter operations, insights, and desktop/mobile validation, consistent with the reviewed documentation.
- Keep the scope suitable for the documented showcase and avoid adding unrequested backend or production features.

task.md
- Turn the plan into an actionable, ordered checklist with stable task IDs.
- Show actual dependencies between tasks; do not imply dependencies solely through ordering.
- Give each task a clear outcome and completion/verification criteria.
- Include tasks for resolving or confirming the documented ambiguities before dependent implementation work.

GENERAL QUALITY RULES
- Keep the four documents separate and mutually consistent.
- Do not invent requirements. Mark assumptions and unresolved questions clearly.
- Use relative paths when citing source documents.
- Ensure every acceptance criterion in specs.md is represented by one or more tasks in task.md.
- Before finishing, check the four files against the source inventory for coverage and consistency, and report any items that could not be verified.