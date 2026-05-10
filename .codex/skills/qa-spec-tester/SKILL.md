---
name: qa-spec-tester
description: Run manual and functional QA for this Kelas workspace using QA-AGENT.md. Use when the user asks to test, validate, review, approve, reject, or run QA for one Kiro/specs task or a spec file in this repository, especially prompts like "Tomando en cuenta QA-AGENT.md, hace el test de la siguiente spec".
---

# QA Spec Tester

## Core Rule

Act as the QA agent, not the implementation agent. Validate one spec at a time against the running project, report reproducible findings, and update `specs/_qa-status.md` before finishing.

## Workflow

1. Read `QA-AGENT.md` completely and follow it as the source of truth for role, criteria, reporting format, and status updates.
2. Read `README.md` to confirm current commands, URLs, credentials, and service expectations.
3. Identify the target spec:
   - If the user named a file, use that exact spec.
   - If the user says "siguiente spec" or does not name one, read `specs/_qa-status.md` and pick the next suggested/pending spec.
   - If there is ambiguity between `.kiro/specs/...` and `specs/...`, prefer the path explicitly provided by the user; otherwise use `specs/_qa-status.md` continuity.
4. Before running browser/API validation, ask the user whether Docker is already ready and updated.
   - Ask a concise question in Spanish, for example: `Docker ya esta levantado y actualizado en localhost:3000?`
   - If the user answers yes, verify the app at `http://localhost:3000` and continue.
   - If the user answers no, or asks you to prepare it, run `docker compose up --build -d` from the repository root, then verify `http://localhost:3000` and `http://localhost:5000/api/health`.
   - If Docker commands fail due to permissions or sandboxing, request escalation for the same command instead of using an unrelated workaround.
5. Inspect only the code needed to understand and test the target spec. Do not fix implementation bugs unless the user explicitly changes the task from QA to implementation.
6. Use Browser/Chrome MCP for UI specs and real web flows:
   - Open `http://localhost:3000`.
   - Navigate the full happy path and basic invalid/error states from the spec.
   - Check visible UI state, navigation, form behavior, notifications, loading states, and console errors that affect the task.
7. Use API calls, tests, logs, or database inspection only when they materially validate an acceptance criterion or explain a finding.
8. Update `specs/_qa-status.md` with the reviewed task, status, date, short summary, blockers, and suggested next task.
9. Final response must follow the format required by `QA-AGENT.md`:
   - `### Tarea revisada`
   - `### Estado`
   - `### Hallazgos`
   - `### Cobertura validada`
   - `### Riesgos o vacios`

## Status Rules

Use these outcomes consistently:

- `aprobada`: all relevant acceptance criteria were validated with no findings.
- `aprobada con observaciones`: the feature works, but there are non-blocking issues or clear residual risks.
- `rechazada`: one or more acceptance criteria fail.
- `bloqueada`: the task cannot be meaningfully validated because the environment, data, auth, build, or navigation is blocked.

Never mark a task as approved only because code exists, builds, or tests pass. If a criterion was not verified, state it explicitly under risks or blockers.

## Finding Format

For each real issue, include:

- Title
- Severity: `critica`, `alta`, `media`, or `baja`
- Affected spec
- Reproduction steps
- Expected result
- Actual result
- Evidence
- Brief technical note when useful

Keep findings concrete and reproducible. Prefer functional defects, data errors, broken navigation, missing validations, and blocking UX over cosmetic details.
