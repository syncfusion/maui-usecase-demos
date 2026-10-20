# Diagram Builder — Mock Data Strategy

No backend. All data originates from:

1. **Embedded JSON template files** (`Resources/Raw/Templates/*.json`, `MauiAsset`) loaded by
   `TemplateService` via `FileSystem.OpenAppPackageFileAsync`. DTOs (`TemplateNode`,
   `TemplateConnector`) map to `DiagramTemplate`.

2. **In-code sample catalogs** for UI metadata:
   - `StencilCatalog` — stencil groups (flow shapes, basic shapes, connectors) with
     display names and search tags.
   - `MenuCatalog` / `ToolbarCatalog` — menu & toolbar command models (built in
     `MenuBarViewModel`/`ToolbarViewModel` constructors via constants).
   - `RecentDocumentCatalog` — mock "Open" recent list (in-memory only).

3. **In-memory document store** (`DiagramService`) — current `DiagramDocument`, dirty flag,
   undo of document-level operations; `Save`/`Save As` update the in-memory record and raise
   status messages.

## Template payloads

### flow-credit-card-processing.json
11-node payment flow (mirrors the control's GettingStarted sample): Start → Enter Card →
Validate (decision, Yes/No ports) → declined path → Process Payment → Store → Receipt → End,
orthogonal connectors, labeled branches.

### mindmap-product-planning.json
Central "Product Planning" ellipse with 5 branches (Research, Design, Development, Marketing,
Launch), each with 2–3 child ellipses; left/right balanced manual positioning, bezier
connectors, branch accent colors.

### orgchart-company-hierarchy.json
CEO → 3 directors → managers/teams tree, `OrganizationalChart` auto-layout, TopToBottom,
rounded rectangles, manager nodes filled PrimaryColor.

Blank templates emit a single starter node annotated "Double-click to rename" plus the
category-appropriate default connector tool.

## Sample data guarantees
- `CreateDiagramPage` shows all 6 template cards immediately (templates preloaded on app start
  by `TemplateService.PreloadAsync()`; JSON parse failures fall back to built-in C# seed
  templates so the UI is never empty).
- Every designer region has initial state: stencil groups expanded (Flow Shapes), property
  panel empty-state, zoom 100 %, grid on, snap on.
- Export dialog pre-fills file name from document title.
