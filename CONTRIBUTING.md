# Contributing

Codec_Tactics is intentionally small at this stage. Prefer focused changes that can be validated quickly.

## Workflow

1. Inspect the current repository state before editing.
2. Keep each task scoped to its requested outcome.
3. Run `.\scripts\validate.ps1` before committing.
4. Fix validation failures when they are related to your change.
5. Document meaningful design decisions in `docs/`.
6. Keep commits small and descriptive.

## Testing

Tests should be deterministic and runnable from a clean checkout. Prefer pure C# tests for domain logic. Godot-specific behavior should be isolated so future editor or scene tests can be added without making the whole suite fragile.

## Documentation

Update documentation when a change affects project structure, architecture, milestones, or development workflow.
