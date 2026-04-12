# AGENTS Adapter (Cursor/Windsurf/Cline)

Canonical instructions live in `AI_SYSTEM.md`.

## Startup Checklist

1. Read `AI_SYSTEM.md`.
2. Read `.cursor/active-context.md`.
3. Use project-memory tools on-demand only.

## Cursor-Style Tool Names

- `save(title, content, category)`
- `batch_save(items[])`
- `query(text)`
- `search(text)`
- `check_errors()`

## Must-Keep Guardrails

- Do not mix contradictory stack assumptions from legacy memory.
- Do not bypass active-context instructions for the file being edited.

## Conflict Rule

If this file conflicts with `AI_SYSTEM.md`, always follow `AI_SYSTEM.md`.
