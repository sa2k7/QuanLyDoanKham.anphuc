# Changelog

All notable changes to this project will be documented in this file.

## [4.3.5] - 2026-04-10
### Added
- **Medical Record State Machine**: Triển khai cơ chế quản lý trạng thái hồ sơ nghiêm ngặt (Ready -> CheckedIn -> WaitingForQC -> Completed).
- **Strict State Validation**: Chặn đứng việc Finalize hồ sơ khi chưa hoàn tất quy trình (Bypass protection).

### Changed
- **ServiceResult Pattern**: Chuyển đổi toàn bộ `MedicalRecordStateMachine` sang trả về `ServiceResult` thay vì dùng raw Exceptions, giúp API ổn định và trả về HTTP 400 chuẩn.
- **EF Synchronization**: Tối ưu hóa thứ tự `SaveChangesAsync` để đảm bảo dữ liệu luôn nhất quán khi kiểm đếm trạng thái đồng bộ.

### Fixed
- **Foreign Key Constraint**: Sửa lỗi sập API khi `TaskId` bị gán giá trị 0.
- **AnyAsync Stale Read Bug**: Sửa lỗi hồ sơ bị kẹt trạng thái do đọc dữ liệu cũ từ Database.

---

## [QuanLyDoanKham - 1.0.0-beta] - 2026-04-01
### Added
- **Centralized Error Handling**: Tích hợp cơ chế chiết xuất lỗi từ API tập trung trong `authStore.js` và `useToast.js`, giúp hiển thị thông báo lỗi thân thiện thay vì đối tượng JSON.
- **BCrypt Security**: Chuyển đổi toàn hệ thống sang chuẩn mã hóa mật khẩu BCrypt.
- **Brutalist/Modern UI**: Cập nhật giao diện Đăng nhập theo phong cách hiện đại, tối ưu trải nghiệm người dùng.

### Changed
- **API Naming Policy**: Chuyển đổi toàn bộ phản hồi API sang chuẩn `camelCase` để đồng bộ hoàn toàn với Frontend.
- **Auth Service Optimization**: Tối ưu hóa logic đăng nhập và xử lý Token để tăng độ ổn định.

### Fixed
- **Admin Authentication**: Sửa lỗi tài khoản Admin không thể đăng nhập do sai biệt mã băm (Legacy vs BCrypt) và lỗi rò rỉ biến môi trường trong lệnh SQL.
- **Toast Notifications**: Sửa lỗi hiển thị `[object Object]` trong các thông báo Toast.

---


## [4.2.0] - 2026-02-14
### Optimized
- **Instant Mode**: Reduced bloat by replacing `maker` with `maker-lite`. (Skills reduced by ~60%).
- **SME Mode**: Professionalized by replacing startup skills with `growth-enterprise`.
- **New Categories**: Added `maker-lite` (Indie/MVP focus) and `growth-enterprise` (Scale/Ops focus).

## [4.1.30] - 2026-02-14
### Changed
- Refined scale-rules logic.

## [4.1.0] - 2026-02-11
### Added
- **Mega Skill Expansion**: Massively increased Core Skills from **74** to **573**, covering almost every modern development and research domain.
- **Fractal Skill Architecture**: Implemented a "Core + Sub-skills" model for all 573 skills, allowing for modular knowledge retrieval and precise execution.
- **AI Patterns**: Verified **2977** specific AI Patterns/Tactics extracted from the expanded skill system.
- **Shared DNA**: Expanded shared logic modules to **135** reusable libraries in `.agent/.shared`.
- **SME & Creative Modes**: Fully verified the scale-adaptive filtering logic. SME mode now provides a curated set of 317 professional-grade skills.

### Changed
- **Branding Standardization**: Rebranded all skills with the `agent-*` prefix for a consistent, professional, and AI-agnostic appearance.
- **CLI Logic**: Refactored `cli/prompts.js` to ensure the `-y` / `--skip-prompts` flag defaults to the full "Creative" experience with all categories enabled.

### Fixed
- **Skill Routing Gap**: Updated `cli/logic/skill-definitions.js` to correctly map all 573 skills to their respective installation categories (webdev, ai, security, etc.).

## [4.0.13] - 2026-02-11
### Added
- **Killer Feature**: New `testing-automation-mcp` skill allowing Agents to autonomously **WRITE & RUN** Playwright E2E tests.
- **Safety**: New `fabric-compliance` skill integrating "Ultimate Law of AI Safety" patterns from Daniel Miessler.
- **Resources**: Updated `RESOURCES.md` with links to Official MCP Servers (Testing, Search, Finance) and AutoGen v0.4.

### Changed
- **Architecture**: Refactored `ai-engineer` skill to support Microsoft AutoGen v0.4 (Async/Event-driven) and DSPy prompting.
- **Documentation**: Updated total Master Skills count to **74**.
- **System**: Enhanced `SKILLS_GUIDE` with new Testing and Compliance capabilities.
## [4.0.12] - 2026-02-08
### Fixed
- **Critical Fix**: Resolved circular dependency in `cli/repair.js` causing `generateGeminiMd` to be undefined during project repair/initialization.

## [4.0.11] - 2026-02-08
### Changed
- **NPM Package**: Enhanced package description to list ALL project assets (Rules, Agents, Master Skills, Patterns, Workflows, DNA Libraries) for complete transparency.

### Changed
- **NPM Optimization**: Updated `package.json` description to include key statistics (72 Skills, 22 Agents) for better discoverability on npmjs.com.

### Added
- **Marketing**: New "One Command" slogan and high-contrast NPM badges.
- **Documentation**: Synchronized all guides with 72 Master Skills and 600+ AI Patterns.
- **Stats**: Added project statistics bar to READMEs.
- **Reference**: Updated `GEMINI_GUIDE.md`, `SKILLS.md`, and `MASTER_GUIDE.md`.

### Changed
- Refined `README.md` and `README.vi.md` for maximum marketing impact.
- Standardized badge colors: Green (Version), Purple (Downloads), Orange (License).
- Clarified distinction between Master Skills (72) and AI Patterns (600+).

### Added
- **Feature**: Auto-Update Documentation System - Never forget to update docs again
- **Workflow**: New `/update-docs` workflow for systematic docs synchronization
- **Rule**: New `docs-update.md` rule with checklist for all doc types
- **Script**: `update-docs.js` for automatic statistics collection
- **Automation**: AI now auto-checks and updates docs when adding Skills/Workflows/Rules

### Changed
- Updated workflow count from 19 to 21 (added update-docs and plan-auto-update-chat)
- Updated all documentation with new feature descriptions
- Enhanced `RULES_GUIDE.vi.md` with docs-update rule
- Enhanced `WORKFLOW_GUIDE.vi.md` with /update-docs workflow

### Improved
- Documentation consistency across all files
- Automatic statistics tracking (27 Skills, 21 Workflows, 12 Rules)
- Reduced manual effort in maintaining docs

## [4.0.2] - 2026-02-02
### Added
- **Feature**: Automatic Error Logging System - AI tracks all errors to `ERRORS.md` for learning
- **Rule**: New `error-logging.md` rule that auto-captures errors during development
- **Workflow**: New `/log-error` workflow for systematic error tracking
- **File**: `ERRORS.md` central error repository with statistics and prevention tips
- **Test**: Error logging test suite to verify the tracking system

### Changed
- Updated `RULES_GUIDE.vi.md` with error-logging rule
- Updated `WORKFLOW_GUIDE.vi.md` with /log-error workflow
- Enhanced AI's ability to learn from mistakes and prevent recurring errors

## [4.0.1] - 2026-02-02
### Added
- **Security**: New `malware-analyst` skill for threat intelligence and malicious URL scanning.
- **Security**: New `malware-protection.md` rule to prevent malware infiltration and link safety.
- **Feature**: Auto-update functionality via chat - AI can check NPM version and offer to upgrade.
- **Workflow**: New `/update` workflow for checking and updating Antigravity IDE.
- **Rule**: New `system-update.md` rule that triggers on version-related queries.
- **Tool**: `link_checker.py` script for automated URL security scanning.

### Changed
- Updated all documentation to reflect new security and update features.
- Enhanced `README.vi.md` with security and auto-update capabilities.
- Updated skill and workflow counts (27 Skills, 18 Workflows).

## [3.5.54] - 2026-01-31
- **Optimization**: Significantly reduced NPM package size (excluded `docs/`, `tests/`).
- **Automation**: Implemented GitHub Actions for auto-publishing with Provenance.
- **Fixes**: Sync `package-lock.json` and `.npmignore` for stable CI builds.
- **Docs**: Updated internal guides and verified asset counts.

## [3.5.30] - 2026-01-30
- **Feature**: Added "Copy-Paste Prompts" for AI Delegation in Setup Wizard.
- **Feature**: Smart Python detection and installation guidance.
- **Docs**: Renamed `GEMINI.md` to `GEMINI_GUIDE.md` for clarity.

## [3.5.29] - 2026-01-29
### Added
- **Workflow**: `/audit` for comprehensive quality checks.
- **Workflow**: `/onboard` for team integration.
- **Workflow**: `/document` for automated documentation generation.
- **Workflow**: `/monitor` for system health tracking.
- **Workflow**: `/security` and `/seo` for domain-specific tasks.

### Changed
- **CLI**: Enhanced `prompts.js` with dynamic workflow mapping based on industry.
- **Setup**: Improved "Engine Mode" selection (Standard vs Advanced) with language support.