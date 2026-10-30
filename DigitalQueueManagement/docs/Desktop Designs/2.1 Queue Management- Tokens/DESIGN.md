---
name: Enterprise Service Operations
colors:
  surface: '#f8f9ff'
  surface-dim: '#cbdbf5'
  surface-bright: '#f8f9ff'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#eff4ff'
  surface-container: '#e5eeff'
  surface-container-high: '#dce9ff'
  surface-container-highest: '#d3e4fe'
  on-surface: '#0b1c30'
  on-surface-variant: '#444653'
  inverse-surface: '#213145'
  inverse-on-surface: '#eaf1ff'
  outline: '#757684'
  outline-variant: '#c4c5d5'
  surface-tint: '#3755c3'
  primary: '#00288e'
  on-primary: '#ffffff'
  primary-container: '#1e40af'
  on-primary-container: '#a8b8ff'
  inverse-primary: '#b8c4ff'
  secondary: '#006a61'
  on-secondary: '#ffffff'
  secondary-container: '#86f2e4'
  on-secondary-container: '#006f66'
  tertiary: '#532a00'
  on-tertiary: '#ffffff'
  tertiary-container: '#743d00'
  on-tertiary-container: '#ffa85d'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#dde1ff'
  primary-fixed-dim: '#b8c4ff'
  on-primary-fixed: '#001453'
  on-primary-fixed-variant: '#173bab'
  secondary-fixed: '#89f5e7'
  secondary-fixed-dim: '#6bd8cb'
  on-secondary-fixed: '#00201d'
  on-secondary-fixed-variant: '#005049'
  tertiary-fixed: '#ffdcc3'
  tertiary-fixed-dim: '#ffb77d'
  on-tertiary-fixed: '#2f1500'
  on-tertiary-fixed-variant: '#6e3900'
  background: '#f8f9ff'
  on-background: '#0b1c30'
  surface-variant: '#d3e4fe'
typography:
  display-ticket:
    fontFamily: Inter
    fontSize: 48px
    fontWeight: '700'
    lineHeight: 56px
    letterSpacing: -0.02em
  display-ticket-mobile:
    fontFamily: Inter
    fontSize: 36px
    fontWeight: '700'
    lineHeight: 44px
    letterSpacing: -0.01em
  headline-lg:
    fontFamily: Inter
    fontSize: 32px
    fontWeight: '600'
    lineHeight: 40px
    letterSpacing: -0.01em
  headline-lg-mobile:
    fontFamily: Inter
    fontSize: 26px
    fontWeight: '600'
    lineHeight: 32px
    letterSpacing: 0em
  headline-md:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
    letterSpacing: -0.01em
  headline-sm:
    fontFamily: Inter
    fontSize: 20px
    fontWeight: '600'
    lineHeight: 28px
  body-lg:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  body-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
  body-sm:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '400'
    lineHeight: 16px
  label-lg:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '600'
    lineHeight: 20px
    letterSpacing: 0.01em
  label-md:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '600'
    lineHeight: 16px
    letterSpacing: 0.02em
  label-sm:
    fontFamily: Inter
    fontSize: 11px
    fontWeight: '700'
    lineHeight: 14px
    letterSpacing: 0.04em
  code-tabular:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '500'
    lineHeight: 20px
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  gutter: 1rem
  gutter-desktop: 1.5rem
  margin: 1rem
  margin-desktop: 2rem
  space-xs: 0.25rem
  space-sm: 0.5rem
  space-md: 0.75rem
  space-lg: 1rem
  space-xl: 1.5rem
  space-2xl: 2rem
---

## Brand & Style

This design system is engineered for mission-critical enterprise service centers, counter management hubs, and customer routing operations. The aesthetic draws direct inspiration from high-density, high-reliability enterprise component suites like Syncfusion—prioritizing deterministic layouts, precise information density, immediate legibility, and high situational awareness.

The brand archetype is authoritative, vigilant, and structured. In high-pressure operational environments where staff and floor managers process hundreds of tickets per hour, visual ambiguity represents real-world service delays. The design eliminates decorative clutter, extreme gradients, and glassmorphic blurs in favor of structural clarity, distinct status-driven signaling, and accessible contrast.

Key Principles:
- **Operational Clarity:** High-contrast tokens, large queue ticket indicators, and explicit status colors ensure counter agents and visitors can read states instantly at a distance.
- **Enterprise Utility:** Tight spatial discipline, rigid structural alignments, and subtle contrast boundaries accommodate large volumes of tabular data and real-time counter metrics.
- **Controlled Affordance:** Buttons, segmented controls, and interactive queue items use crisp borders, flat fills, and subtle elevation rather than exaggerated motion or sensory gimmicks.

## Colors

The palette establishes an unambiguous hierarchy centered around a Corporate Royal Blue primary, a Vibrant Teal secondary for service actions, and a comprehensive status palette mapped directly to operational states:

- **Primary (`#1E40AF` base, `#2563EB` active/interactive):** Anchors navigation, active counter selections, primary CTA buttons, and high-priority administrative flows.
- **Secondary (`#0D9488` base, `#14B8A6` hover):** Identifies auxiliary workflows, digital check-ins, department routing, and customer confirmation steps.
- **Tertiary / Warning (`#D97706` base, `#F59E0B` light):** Reserved for "Serving" status, counter recalls, wait-time SLA warnings, and pending counter assignments.
- **Success (`#16A34A`):** Designates "Completed" tickets, operational desks online, and verified transactions.
- **Danger (`#DC2626`):** Identifies "Skipped" tickets, counter escalations, system errors, and emergency no-shows.
- **Neutral Canvas (`#F8FAFC` base background, `#F1F5F9` sub-surfaces):** Ensures maximum eye comfort during extended screen shifts while keeping cards and panels distinctly defined with `#E2E8F0` borders.
- **Surface Cards (`#FFFFFF`):** Pure white elevates ticket detail cards, modal forms, and live queue panels against the slate background.

## Typography

The typography strategy leverages `Inter` across all tiers to deliver standard-setting legibility in data-dense user interfaces. Special attention is given to tabular figures (`font-variant-numeric: tabular-nums`) so that ticket identifiers (e.g., `A-104`, `C-089`), wait timers (`14:22`), and counter statistics stay aligned without jitter during real-time streaming updates.

Display Ticket styles are specialized for queue announcements and large counter screens, pairing semi-condensed letter tracking with a structural 700 weight for high visibility across service lobbies. Body text emphasizes clean x-height and consistent vertical metrics, while labels maintain uppercase or title-case clarity for high-contrast status badges.

## Layout & Spacing

This design system employs a deterministic 12-column fluid grid system engineered for enterprise multi-monitor setups, service agent terminals, and touch kiosk screens.

- **Breakpoints:**
  - **Mobile / Compact Kiosk (< 768px):** 4-column grid, `margin: 1rem`, `gutter: 1rem`. Cards and queues stack vertically. The active ticket sticky drawer anchors to the bottom.
  - **Tablet / Counter Terminal (768px - 1199px):** 8-column grid, `margin: 1.5rem`, `gutter: 1.25rem`. Split-view allows queue list on the left (3 cols) and ticket servicing actions on the right (5 cols).
  - **Desktop / Service Operations Center (>= 1200px):** 12-column grid, `margin: 2rem`, `gutter: 1.5rem`. Enables 3-pane layout: Department Overview (2 cols), Live Ticket Grid (6 cols), Counter Diagnostics & Queue Controls (4 cols).

Spacing rhythm relies strictly on a 4px/8px modular scale. Spacing tokens (`space-xs` through `space-2xl`) govern internal component padding and element gaps, maintaining compact density without sacrificing touch and click accuracy.

## Elevation & Depth

Visual depth follows the Syncfusion enterprise paradigm: structural, low-elevation layers that utilize micro-borders combined with delicate, directional ambient shadows. Depth signifies state change and user focus rather than decorative separation.

- **Level 0 (Flat Surface):** Default `#FFFFFF` cards and `#F8FAFC` canvas. No shadow. Boundaries are marked strictly by a 1px border (`#E2E8F0`).
- **Level 1 (Card & Row Rest):** Subtle lift for standard operational cards and interactive data grid rows: `0 1px 3px 0 rgba(15, 23, 42, 0.08), 0 1px 2px -1px rgba(15, 23, 42, 0.04)`.
- **Level 2 (Active Ticket / Hover):** Elevated state when hovering over incoming customer tickets or focusing a counter lane: `0 4px 6px -1px rgba(15, 23, 42, 0.10), 0 2px 4px -2px rgba(15, 23, 42, 0.06)`. Border color sharpens to `#CBD5E1`.
- **Level 3 (Modals, Overlays, Floating Counter Actions):** Top-level operational focus: `0 10px 15px -3px rgba(15, 23, 42, 0.12), 0 4px 6px -4px rgba(15, 23, 42, 0.08)`. Accompanied by a 1px border `#94A3B8` on modal containers.

## Shapes

The design system adopts a Soft (`1`) roundedness tier (4px / `0.25rem` standard radius). This conservative geometry communicates enterprise precision, aligns with structured data grids, and maximizes content density inside complex dashboards.

- **Base Radius (`0.25rem` / 4px):** Standard buttons, text inputs, table row highlights, checkboxes, radio tiles, and compact badges.
- **Large Radius (`0.5rem` / 8px):** Service desk cards, metrics summary panels, queue grouping containers, and dialog modal windows.
- **Pill Radius (`9999px`):** High-contrast status indicator badges (Waiting, Serving, Skipped, Completed) and counter lane number tags, ensuring status tags are visually distinct from actionable rectangular buttons.

## Components

### Buttons
- **Primary:** Solid `#1E40AF` with white text, font weight 600. Hover: `#2563EB`. Active: `#1D4ED8`. 1px border `#1E3A8A`. Padding: `0.5rem 1rem`.
- **Secondary / Action:** Solid `#0D9488` with white text. Hover: `#14B8A6`. Used for "Call Next", "Transfer Desk", and customer intake actions.
- **Danger:** Solid `#DC2626` or outlined `#DC2626` over `#FEF2F2`. Used strictly for "Skip", "Cancel", and emergency halt.
- **Ghost / Neutral:** Transparent with `#1E293B` text, 1px border `#E2E8F0`. Hover: `#F1F5F9`.

### Status Badges & Chips
- Status badges use pill shapes with strong semantic color coding and distinct background tints to ensure WCAG AAA contrast:
  - **Waiting:** Background `#EFF6FF`, border `#BFDBFE`, text `#1E40AF` (Royal Blue).
  - **Serving:** Background `#FFFBEB`, border `#FDE68A`, text `#B45309` (Amber Orange).
  - **Completed:** Background `#F0FDF4`, border `#BBF7D0`, text `#15803D` (Emerald Green).
  - **Skipped / Absent:** Background `#FEF2F2`, border `#FECACA`, text `#B91C1C` (Crimson Red).
- Chips include optional monospace ticket IDs (e.g., `#B-201`) placed inside an inset container.

### Input Fields & Controls
- **Text Inputs & Dropdowns:** 36px standard enterprise height. White background, 1px `#CBD5E1` border, 4px border radius. Focus state invokes a crisp 1px `#2563EB` ring with 0 0 0 2px `rgba(37, 99, 235, 0.20)`. No offset blur.
- **Checkboxes & Radios:** 16px square/circle with a rigid 1.5px `#94A3B8` border. When selected, solid `#1E40AF` fill with white checkmark icon.

### Cards & Service Panels
- Constructed with `#FFFFFF` background, 1px `#E2E8F0` border, and Level 1 ambient elevation.
- Header bands are strictly segmented by a bottom 1px `#F1F5F9` divider.
- Counter Callout Cards feature a 4px left semantic border indicating the current lane status (e.g., Amber left-edge for currently serving, Green for ready).

### Enterprise Data Grid / Queue Table
- High-density table styling with alternating or subtle hover rows (`#F8FAFC`).
- Header cells: `#F1F5F9` background, 11px uppercase bold labels (`#475569`), vertical alignment centered with 1px border dividers.
- Ticket numbers displayed using `code-tabular` Inter 500 weight to prevent layout shift during countdowns and queue shifts.