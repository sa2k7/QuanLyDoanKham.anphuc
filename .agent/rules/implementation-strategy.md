# Logic Analysis & Implementation Strategy

Whenever the user requests a new feature or a complex change, the Agent must follow this 2-step process:

## Phase 1: Deep Analysis (Logic First)
Before writing any code, the Agent must provide a brief but comprehensive analysis:
1. **Impact Assessment**: What files need to be created or modified? (Backend Entities, DTOs, Controllers, Frontend Views, Stores, etc.)
2. **Logic Flow**: How will the data flow? (e.g., User clicks button -> Vue calls ApiService -> Controller validates DTO -> Repository saves to DB).
3. **Task Breakdown**: A bulleted list of technical steps.

## Phase 2: Autonomous Implementation (Action Second)
Immediately after the analysis (in the same or next turn), the Agent must:
1. **Execute**: Start implementing the steps starting from the foundation (usually Backend/Database) up to the UI.
2. **Sync**: Ensure consistency between C# types and Vue/JS interfaces.
3. **Verification**: If possible, check for syntax errors or build status.

## Usage Trigger
This rule is triggered by keywords like: "làm bài mới", "thêm tính năng", "phân tích và làm", "giải quyết logic".
