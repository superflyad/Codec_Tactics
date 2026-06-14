# Codec_Tactics

Codec_Tactics is an early-stage C# Godot project for a turn-based network tactics game.

The player builds a growing network across layered, cube-inspired spaces. Each expansion opens useful paths, but poor choices can expose routes for enemy corruption. Complexity increases as the player descends into deeper network layers.

This repository is currently at Milestone 1: playable 2D network prototype. It contains a small deterministic core loop in pure C# and still intentionally avoids layers, cube visualization, advanced AI, save/load, and polish.

## Current Foundation

- Godot project metadata in `project.godot`
- C# solution for deterministic non-Godot domain code
- Console-based automated test runner with no third-party test dependency
- Validation script for repeatable local checks
- Documentation for architecture, milestones, contribution workflow, and Codex usage
- Pure C# 2D network prototype with fixed grid nodes, adjacent connections, ownership, player actions, deterministic enemy spread, turn counter, and placeholder outcomes

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

## Milestone 1 Mechanics

- The default board is a fixed 4x4 orthogonal node grid.
- Nodes can be neutral, player-owned, or enemy-owned.
- The player starts at `(0,0)` and enemy corruption starts at `(3,3)`.
- One successful player action resolves one deterministic enemy expansion and advances the turn.
- Player actions currently include claiming adjacent neutral nodes, reinforcing owned nodes, and weakening reachable enemy connections.
- Enemy corruption expands into the first adjacent neutral node by deterministic row-major node order.

## Current Limitations

- No Godot gameplay scene is wired to the core model yet.
- No layers, cube visualization, advanced AI, save/load, balancing, resources, or production UI.
- Win/loss rules are placeholders for future scenario design.
