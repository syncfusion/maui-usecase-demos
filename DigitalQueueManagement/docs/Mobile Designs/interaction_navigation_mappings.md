# Interaction & Modal Navigation Mappings

### Overview
This document specifies the exact interaction triggers and navigation mapping rules across both the **Mobile** and **Desktop** views of the **Digital Queue & Counter Manager** suite.

---

### 1. Token Details Modal / Bottom Sheet
- **Target Modal:** `Token Details Bottom Sheet` (Mobile) / `Desktop Popup - Token Details` (Desktop)
- **Triggers:**
  - Tapping/clicking any token card in the **Tokens** tab (`Token Directory`).
  - Tapping/clicking any token in the **Upcoming Tokens** section (`Queue Overview`).
- **Context Passed:** Active Token ID (e.g. `A-105`), Service Category, Priority Level, Wait Time.

---

### 2. Counter Operations Modal / Bottom Sheet
- **Target Modal:** `Counter Operations Bottom Sheet` (Mobile) / `Desktop Popup - Counter Operations` (Desktop)
- **Triggers:**
  - Tapping/clicking any counter card in **Queue Overview** (`Counter Status` list).
  - Tapping/clicking any counter terminal card or switcher in the **Counters** tab (`Counter Operations`).
- **Context Passed:** Counter Terminal ID (e.g. `Counter 1 - Express Desk`), Operator Name, Status, Currently Serving Token.

---

### 3. Transfer Token Modal / Bottom Sheet
- **Target Modal:** `Transfer Token Bottom Sheet` (Mobile) / `Desktop Popup - Transfer Counter` (Desktop)
- **Triggers:**
  - Tapping/clicking the **Transfer** / **Transfer Token** action button inside the **Counter Operations** workspace / modal.
  - Tapping/clicking the secondary **Transfer Token** button inside the **Token Details** bottom sheet/popup.
- **Context Passed:** Origin Counter / Token ID (e.g. `A-104`), List of available destination counters.

---

### 4. Reset Confirmation Modal / Bottom Sheet
- **Target Modal:** `Reset Confirmation Bottom Sheet` (Mobile) / `Desktop Popup - Reset Queue` (Desktop)
- **Triggers:**
  - Selecting the **Reset Queue** option from the **App Bar overflow menu** (`⋮` three-dot menu) in the top header on any screen.
- **Action Behavior:** Prompts operator/supervisor confirmation before purging transient queue state and restoring default demonstration data.
