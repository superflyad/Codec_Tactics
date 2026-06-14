# Decisions

## 2026-06-14: Keep Milestone 0 Gameplay-Free

The initial repository foundation includes Godot metadata, C# project structure, documentation, validation, and a deterministic test harness. It does not implement gameplay systems yet.

Reason: future gameplay tasks should start from a small, reviewable foundation with repeatable validation.

## 2026-06-14: Use Pure C# Core Tests First

The first automated tests run through a console test project instead of a Godot-specific test framework.

Reason: Godot CLI may not be installed in every workspace. Pure C# tests give future Codex tasks a reliable validation path for domain logic while Godot scenes and editor tests are introduced later.

## 2026-06-14: Keep Milestone 1 Domain Logic Godot-Free

Milestone 1 implements the playable 2D network loop in `CodecTactics.Core` rather than Godot scenes.

Reason: deterministic board, ownership, action, turn, and enemy-spread rules should remain easy to test and evolve before presentation code is introduced.
