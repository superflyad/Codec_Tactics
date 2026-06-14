# Codex Instructions

Codex should treat this repository as a testable game project foundation.

## Working Rules

- Inspect the repository before changing files.
- Understand existing structure and docs before adding new patterns.
- Keep commits small, focused, and easy to review.
- Run available validation before committing.
- Debug failures instead of abandoning validation.
- Prefer deterministic tests over timing-sensitive or random tests.
- Never hide, skip, or relabel failing tests to make a task appear complete.
- Document decisions that affect architecture, workflow, or future tasks.
- Avoid real gameplay implementation unless the task explicitly asks for it.
- Summarize changes after every task, including validation results and known limitations.

## Expected Validation

Use this command from the repository root:

```powershell
.\scripts\validate.ps1
```

If Godot CLI is missing, say so clearly. Continue with structure checks and .NET build/tests when possible.

## Review Checklist

- Does the change match the requested scope?
- Are tests deterministic and meaningful?
- Did validation run from a clean command?
- Are docs updated when project behavior or workflow changes?
- Are any limitations or blockers clearly reported?
