# AI System Source of Truth (SSoT)

This file is the single source of truth for all AI assistants used in this repository.

## 1) Authority and Precedence

When instructions conflict, use this order:
1. `AI_SYSTEM.md` (this file)
2. Active context files (`.cursor/active-context.md`, `.agent-mem/active-context.md`, `.agent-mem/gotchas.md`)
3. Agent adapter files (`CLAUDE.md`, `GEMINI.md`, `AGENT.md`, `AGENTS.md`)

Adapter files are thin wrappers only. They must not redefine project scope, architecture, or stack truth.

## 2) Product Truth (OMS)

- System type: OMS (Operation Management System) for enterprise health check operations.
- Out of scope: CRM, Lead, Sales, Quotation.
- Contract exists before data enters this system.
- Core flow: `Contract -> MedicalGroup -> MedicalRecord -> Check-in -> Payroll -> Report`.
- Scale target: 1000+ patients per campaign/day.
- Modeling preference: MedicalRecord-first.

## 3) Tech Truth

- Frontend: Vue 3 + Vite + Tailwind (`QuanLyDoanKham.Web`).
- Backend: ASP.NET Core Web API (`QuanLyDoanKham.API`, target `net10.0`).
- ORM/DB access: EF Core.
- Primary DB config in codebase: SQL Server connection strings in `QuanLyDoanKham.API/appsettings*.json`.
- Reporting libs in backend: QuestPDF and ClosedXML.

If older docs mention Express/React/SQLite, treat them as legacy/outdated unless code is updated to match.

## 4) Global Safety Rules

- Never run `git clean -fd` or `git reset --hard` without explicit approval and commit safety checks.
- Never bulk-delete untracked files/folders without backup/stash.
- Before schema migration on shared/prod DB, require backup and rollback plan.

## 5) Runtime Context Protocol

Before implementing changes:
1. Read `AI_SYSTEM.md`.
2. Read the active-context file for the current toolchain:
   - Cursor/Windsurf/Cline: `.cursor/active-context.md`
   - Agent memory workflow: `.agent-mem/active-context.md` and `.agent-mem/gotchas.md`
3. Use memory/tools on-demand only.

## 6) Shared Engineering Conventions

### Language Policy
- **Communication artifacts** (responses, plans, reports): Vietnamese preferred.
- **Source code** (identifiers, comments, commit messages, API names): English only.
- Do NOT mix languages within the same source file.

### Engineering Standards
- UI conventions: premium style, consistent responsive classes, extract repeated patterns.
- API conventions: async-safe error handling, input validation, rate limiting, security headers.
- Vue conventions: never mutate props directly, always use `:key` with `v-for`, avoid mixed styling patterns.

## 7) Adapter Contract (Mandatory)

`CLAUDE.md`, `GEMINI.md`, `AGENT.md`, `AGENTS.md` may include only:
- Link/reference to `AI_SYSTEM.md`.
- Agent-specific runtime behavior (trigger mode, identity, command policy).
- Agent-specific tool alias mapping.

They must not duplicate product truth and tech truth from this file.

## 8) Legacy Mapping

| Legacy file | New role |
|---|---|
| `AGENTS.md` | Cursor/Windsurf/Cline adapter |
| `AGENT.md` | Generic memory-agent adapter |
| `CLAUDE.md` | Claude adapter |
| `GEMINI.md` | Gemini adapter |

## 9) Change Management

When updating AI rules:
1. Update `AI_SYSTEM.md` first.
2. Update adapters only if runtime-specific behavior changed.
3. Keep adapters short and conflict-free.
