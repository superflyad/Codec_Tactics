# Codec_Tactics

Codec_Tactics is an early-stage C# Godot project for a turn-based network tactics game.

The player builds a growing network across layered, cube-inspired spaces. Each expansion opens useful paths, but poor choices can expose routes for enemy corruption. Complexity increases as the player descends into deeper network layers.

This repository is currently at Milestone 1.5: visible Godot prototype. It contains a small deterministic core loop in pure C# plus a minimal Godot scene that renders and interacts with that core model. It still intentionally avoids layers, cube visualization, advanced AI, save/load, resources, and polish.

## Current Foundation

- Godot project metadata in `project.godot`
- Launchable Godot main scene in `scenes/Main.tscn`
- C# solution for deterministic non-Godot domain code
- Console-based automated test runner with no third-party test dependency
- Validation script for repeatable local checks
- Documentation for architecture, milestones, contribution workflow, and Codex usage
- Pure C# 2D network prototype with fixed grid nodes, adjacent connections, ownership, player actions, deterministic enemy spread, turn counter, and placeholder outcomes
- Minimal Godot C# presentation that draws the board, handles node claims, exposes an End Turn button, and updates HUD text

## Requirements

- .NET SDK 8.0 or newer
- Godot with .NET support for future editor/game work

Godot CLI is optional for validation. If it is not available, the validation script records that and still checks repository structure plus .NET build/tests.

## Launch Prototype

Open the repository in Godot .NET 4.x or newer and run the configured main scene:

```text
res://scenes/Main.tscn
```

The prototype displays the current 4x4 2D network board. Click an adjacent neutral node to claim it for the player. Use End Turn to resolve the current placeholder pass action through the core turn flow. Enemy/corruption expansion happens through `CodecTactics.Core` after successful player actions.

## Validate

Run from the repository root:

```powershell
.\scripts\validate.ps1
```

The script checks required repository files, reports Godot CLI availability, builds the .NET solution, and runs tests.

## Project Layout

```text
docs/                         Design and architecture notes
scenes/                       Godot scene files
scripts/                      Developer automation
src/CodecTactics.Core/        Pure C# game-domain foundation
src/CodecTactics.Godot/       Thin Godot presentation scripts
tests/CodecTactics.Core.Tests/ Console test runner
project.godot                 Godot project metadata
Codec_Tactics.csproj          Godot C# project
Codec_Tactics.sln             .NET validation solution
```

## Milestone 1 Mechanics

- The default board is a fixed 4x4 orthogonal node grid.
- Nodes can be neutral, player-owned, or enemy-owned.
- The player starts at `(0,0)` and enemy corruption starts at `(3,3)`.
- One successful player action resolves one deterministic enemy expansion and advances the turn.
- Player actions currently include claiming adjacent neutral nodes, reinforcing owned nodes, and weakening reachable enemy connections.
- Enemy corruption expands into the first adjacent neutral node by deterministic row-major node order.

## Milestone 1.5 Visible Prototype

- The default board renders as temporary 2D circles and connection lines.
- Neutral, player, enemy/corruption, and reinforced nodes have distinct visual treatments.
- Valid adjacent neutral clicks claim nodes through `NetworkGame.ClaimNode`.
- Invalid clicks do not mutate core state and update the HUD status text.
- End Turn uses the current core reinforcement action as a simple pass action and triggers deterministic enemy expansion.

## Current Limitations

- Godot visuals are temporary debug-style UI, not final art.
- End Turn is a placeholder pass action implemented by reinforcing the start node because the core does not yet expose a dedicated pass command.
- No layers, cube visualization, advanced AI, save/load, balancing, resources, or production UI.
- Win/loss rules are placeholders for future scenario design.
