
1. Application Overview
Purpose
The Digital Queue & Counter Manager is a service-center queue monitoring and token-processing application that demonstrates queue management workflows using local data.
The application helps customers and service operators understand:

- Current token being served
- Number of waiting customers
- Estimated waiting time
- Active counters
- Token status progression
This is a showcase use-case application, not a production queue-management system.


2. Problem Statement
Customers visiting service centers often do not know:

- Their position in the queue
- Which token is currently being served
- Which counter is handling services
- Approximate waiting time
Employees also need a simple visual interface to manage queue progression.
This application provides a transparent queue view while demonstrating state management, dashboards, analytics, and workflow operations using Syncfusion controls.


3. Project Goals
Primary Goals

- Demonstrate a complete queue workflow
- Showcase multiple Syncfusion controls
- Provide attractive dashboard visualizations
- Support both Desktop and Mobile layouts
- Use only local data
- Be suitable for AI-assisted development using a Harness workflow
Non-Goals
The application will NOT support:

- Authentication
- Real-time synchronization
- Database storage
- Web APIs
- Multiple service branches
- Appointment scheduling
- AI waiting-time prediction
- Complex queue routing
- Emergency queue handling


4. Application Scope
Service Center Setup
Service Center

- Single Service Center
Counters

- Counter 1
- Counter 2
- Counter 3
Service Categories

- General Enquiry
- Payments
- Customer Support
Priority Types

- Normal
- Priority
Token Count

- 20–30 predefined local tokens


5. Queue Statuses
Each token can have only one status.

Status	Description
Waiting	Customer waiting in queue
Serving	Customer currently being served
Skipped	Token temporarily skipped
Completed	Service completed



6. Queue Workflow
Call Next
Action:

- Select first available waiting token
- Assign token to selected counter
- Change status to Serving


Skip Token
Action:

- Move Serving token to Skipped state
- Counter becomes available


Recall Token
Action:

- Move Skipped token back to Waiting


Transfer Token
Action:

- Move currently Serving token to another counter


Complete Token
Action:

- Mark Serving token as Completed
- Free the counter


7. Waiting Time Logic
This application should not provide intelligent predictions.
Simple Formula

Estimated Wait =
Waiting Tokens × Average Service Duration

Assumptions:

- Average Service Duration = 4 Minutes
- Value is displayed as an estimate only
Example:

8 Waiting Tokens

Estimated Wait:
8 × 4 = 32 Minutes



8. Sample Data Structure
Token

Field	Type
Token Number	String
Service Type	Enum
Priority	Enum
Assigned Counter	String
Status	Enum
Estimated Wait	Integer



Counter

Field	Type
Counter ID	Integer
Counter Name	String
Current Token	String
Active Status	Boolean



9. Application Screens

Refer Mobile-desktop-screens.md file.

10. Syncfusion Control Mapping

Control	Usage
DataGrid	Token Directory
ListView	Counter Cards & Token Lists
Radial Gauge	Queue Load
Charts	Analytics
Badge View	Status Indicators
Segmented Control	Filtering & Counter Selection
Popup	Token Details & Confirmations



11. Navigation Structure
Desktop

Dashboard
│
├── Queue Overview
├── Token Directory
├── Counter Operations
└── Queue Insights

Navigation Style:

- Left Navigation Rail


Mobile

Bottom Navigation

Overview
Tokens
Counters
Insights

Navigation Style:

- Bottom Tab Navigation


12. Recommended Architecture
Pattern

MVVM



Layers

Models
Services
ViewModels
Views
Resources



Core Services
QueueService
Responsible for:

- Token allocation
- Queue ordering
- Status updates
- Analytics calculations


DemoDataService
Responsible for:

- Creating predefined tokens
- Generating counters
- Resetting sample data


13. Harness Development Requirements
The application must be developed through an AI Harness workflow.
Development Phases
Phase 1
Project Setup
Deliverables:

- MAUI Solution
- Syncfusion Integration
- Navigation Structure


Phase 2
Domain Layer
Deliverables:

- Models
- Queue Service
- Sample Data


Phase 3
Queue Overview Screen
Deliverables:

- Dashboard Metrics
- Gauge
- Counter Cards


Phase 4
Token Directory
Deliverables:

- DataGrid
- Search
- Filters


Phase 5
Counter Operations
Deliverables:

- Queue Actions
- State Management


Phase 6
Queue Insights
Deliverables:

- Charts
- Analytics


Phase 7
Responsive Validation
Deliverables:

- Desktop UI Verification
- Mobile UI Verification


14. Stitch AI Design Requirements
Visual Theme
Style:

- Modern Service Center Dashboard
- Clean Enterprise UI
- Professional Appearance
Colors

- Primary: Blue
- Secondary: Teal
- Success: Green
- Warning: Orange
- Error: Red


Card Types
Metric Card
Used for:

- Waiting Tokens
- Active Counters
- Completed Tokens
Token Card
Used for:

- Upcoming Tokens
- Counter Assignments
Analytics Card
Used for chart visualizations


Important Design Rules

- Large token numbers for visibility
- Clear queue-status badges
- Consistent card layout
- Responsive desktop and mobile layouts
- No glassmorphism
- No excessive animations
- No futuristic styling
- Focus on readability


15. Acceptance Criteria
The application is complete when:
✅ Two screens are implemented along with taps and popup and bottom sheet
✅ Queue actions function correctly
✅ Dashboard updates after every action
✅ Charts refresh automatically
✅ Desktop and Mobile layouts work properly
✅ Syncfusion controls are integrated
✅ No backend or server dependency exists
✅ Uses local sample data only
✅ Reset functionality restores original tokens
✅ Can be completed within 1–2 development days


Final Verdict
Digital Queue & Counter Manager is the recommended option for the AI Harness demonstration.
It provides:

- Clear business workflow
- Strong visual appeal
- Meaningful Syncfusion control usage
- Simple state management
- Predictable implementation effort
- Excellent suitability for Stitch AI screen generation
- Ideal scope for a 1–2 day .NET MAUI showcase application using AI-assisted development and harness-based workflows.
