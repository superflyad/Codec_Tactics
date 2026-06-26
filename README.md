# Codec_Tactics

Codec_Tactics is an early-stage C# Godot project for a turn-based network tactics game.

The player builds a growing network across layered, cube-inspired spaces. Each expansion opens useful paths, but poor choices can expose routes for enemy corruption. Complexity increases as the player descends into deeper network layers.

This repository is currently at Milestone 3.25: engine hardening and board generalization. It contains a small deterministic core loop in pure C# plus a minimal Godot scene that renders and interacts with that core model. It still intentionally avoids layers, cube visualization, advanced AI, save/load, real art, and polish.

## Current Foundation

- Godot project metadata in `project.godot`
- Launchable Godot main scene in `scenes/Main.tscn`
- C# solution for deterministic non-Godot domain code
- Console-based automated test runner with no third-party test dependency
- Validation script for repeatable local checks
- Documentation for architecture, milestones, contribution workflow, and Codex usage
- Pure C# 2D network prototype with configurable board definitions, adjacent connections, ownership, node types, energy costs, network integrity, threat, instability, deterministic corruption pressure, collapse, turn counter, and placeholder outcomes
- `BoardDefinition` and `GameConfiguration` models that keep the default 4x4 prototype behavior data-driven and prepare the core for later layers and cube faces
- Minimal Godot C# presentation that draws the board, handles node claims, exposes a real End Turn action, and updates HUD text

## Requirements

- .NET SDK 8.0 or newer
- Godot with .NET support for future editor/game work

Godot CLI is optional for validation. If it is not available, the validation script records that and still checks repository structure plus .NET build/tests.

## Launch Prototype

Open the repository in Godot .NET 4.x or newer and run the configured main scene:

```text
res://scenes/Main.tscn
```

The prototype displays the current 4x4 2D network board. Click a reachable neutral node to claim it for the player. Use End Turn to resolve a real no-cost player pass through the core turn flow. Enemy/corruption pressure and expansion happen through `CodecTactics.Core` after successful player actions and end turns.

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

## Milestone 3.25 Mechanics

- The default prototype board is a 4x4 orthogonal node grid defined by `BoardDefinition.CreateDefaultPrototype()`.
- The core engine can initialize alternate board widths, heights, starting positions, and node type placement from `BoardDefinition`.
- Default costs, corruption growth, integrity values, threat values, and collapse timing are provided by `GameConfiguration`.
- Nodes can be neutral, player-owned, or enemy-owned.
- The player starts at `(0,0)` and enemy corruption starts at `(3,3)`.
- Nodes have types: Standard, Resource, Relay, and Firewall.
- The player has energy. Claiming costs 2 energy; reinforcing costs 1 energy.
- Player-owned Resource nodes generate 2 energy at the start of each new player turn.
- Player-owned Relay nodes extend claim range to neutral nodes within two active connections.
- Player-owned nodes calculate integrity from core connection, distance, owned neighbors, Relay support, Firewall support, dense structure, reinforcement, and isolation.
- Player-owned nodes calculate threat from nearby corruption, corruption pressure, weak owned connections, frontier exposure, and isolation.
- Nodes become unstable when threat exceeds integrity. If instability persists for 2 enemy turns, the node collapses to corruption.
- Corruption pressure increases by 1 each enemy turn and expands deterministically by prioritizing unstable, weak, and exposed targets when pressure meets the target node's resistance.
- Firewall nodes require 2 corruption pressure before corruption can claim them.
- Player actions include claiming reachable neutral nodes, reinforcing owned nodes, weakening reachable enemy connections, and ending the turn.
- See `docs/network-integrity.md` for formulas, examples, collapse behavior, and strategy implications.
- See `docs/board-definition.md` for board definition and configuration details.

## Milestone 1.5 Visible Prototype

- The default board renders as temporary 2D circles and connection lines.
- Neutral, player, enemy/corruption, and reinforced nodes have distinct visual treatments.
- Node types are labeled and outlined in distinct colors.
- Valid reachable neutral clicks claim nodes through `CodecTactics.Core`.
- Invalid clicks do not mutate core state and update the HUD status text.
- End Turn uses the real core corruption turn rather than a placeholder reinforcement.
- HUD text shows turn, phase, energy, corruption pressure, status, and result state.

## Current Limitations

- Godot visuals are temporary debug-style UI, not final art.
- The default prototype still uses a single authored 4x4 scenario.
- Network integrity and threat formulas are prototype balance values.
- Alternate board definitions are engine-supported but not yet balanced scenarios.
- No layers, cube visualization, advanced AI, save/load, balancing, real art, or production UI.
- Win/loss rules are placeholders for future scenario design.
