# Codec_Tactics

Codec_Tactics is an early-stage C# Godot project for a turn-based network tactics game.

The player builds a growing network across layered, cube-inspired spaces. Each expansion opens useful paths, but poor choices can expose routes for enemy corruption. Complexity increases as the player descends into deeper network layers.

This repository is currently at Milestone 0: repo foundation. It intentionally does not contain real gameplay yet.

## Current Foundation

- Godot project metadata in `project.godot`
- C# solution for deterministic non-Godot domain code
- Console-based automated test runner with no third-party test dependency
- Validation script for repeatable local checks
- Documentation for architecture, milestones, contribution workflow, and Codex usage

## Requirements

- .NET SDK 8.0 or newer
- Godot with .NET support for future editor/game work

Godot CLI is optional for current validation. If it is not available, the validation script records that and still checks repository structure plus .NET build/tests.

## Validate

Run from the repository root:

```powershell
.\scripts\validate.ps1
```

The script checks required repository files, reports Godot CLI availability, builds the .NET solution, and runs tests.

## Project Layout

```text
docs/                         Design and architecture notes
scenes/                       Future Godot scene files
scripts/                      Developer automation
src/CodecTactics.Core/        Pure C# game-domain foundation
tests/CodecTactics.Core.Tests/ Console test runner
project.godot                 Godot project metadata
Codec_Tactics.sln             .NET validation solution
```

## Status

No gameplay is implemented yet. The next target is Milestone 1: a playable 2D network prototype.
