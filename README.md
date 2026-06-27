# Codec_Tactics

Codec_Tactics is an early-stage C# MonoGame project for a turn-based network tactics game.

The player builds a growing network across layered, cube-inspired spaces. Each expansion opens useful paths, but poor choices can expose routes for enemy corruption. Complexity increases as the player descends into deeper network layers.

This repository is currently at Milestone 3.75: playable vertical slice mission. It contains a small deterministic core loop in pure C# plus a minimal MonoGame frontend that renders and interacts with that core model. It still intentionally avoids layers, cube visualization, advanced AI, save/load, real art, and polish.

## Current Foundation

- C# solution for deterministic domain code, tests, and the MonoGame frontend
- Console-based automated test runner with no third-party test dependency
- Validation script for repeatable local checks
- Documentation for architecture, milestones, contribution workflow, and Codex usage
- Pure C# 2D network prototype with configurable board definitions, adjacent connections, ownership, node types, energy costs, network integrity, threat, instability, deterministic corruption pressure, collapse, turn counter, and placeholder outcomes
- `BoardDefinition` and `GameConfiguration` models that keep the default 4x4 prototype behavior data-driven and prepare the core for later layers and cube faces
- Minimal MonoGame presentation that draws the authored mission board and routes input through `CodecTactics.Core`
- One authored vertical-slice mission with a fixed board, objective hold win condition, loss states, player feedback, and restartable game loop
- Legacy Godot files remain for reference only and are not part of validation or the active frontend workflow

## Requirements

- .NET SDK 8.0 or newer
- Visual Studio 2022 or newer with .NET desktop development tools
- MonoGame templates:

```powershell
dotnet new install MonoGame.Templates.CSharp
```

- MonoGame Content Builder editor:

```powershell
dotnet tool install --global dotnet-mgcb-editor
```

If the MGCB editor is already installed, update it instead:

```powershell
dotnet tool update --global dotnet-mgcb-editor
```

## Play Mission

Open `Codec_Tactics.sln` in Visual Studio and run the MonoGame project.

To set the startup project:

1. Right-click `CodecTactics.MonoGame` in Solution Explorer.
2. Select `Set as Startup Project`.
3. Press `F5` to run with the debugger, or `Ctrl+F5` to run without debugging.

The current MonoGame input surface is intentionally small:

```text
1 = Claim
2 = Reinforce
3 = Weaken
Space = End Turn
R = Restart Mission
Esc = Exit
Left click = apply the selected action to a node
```

The app launches the vertical-slice mission, `Secure the Uplink`.

- Start: player core at `(0,2)`.
- Corruption start: `(4,4)`.
- Objective: secure the Firewall node at `(3,2)` for 2 turns.
- Win: claim and hold the objective for the required hold turns.
- Lose: the player core collapses or corruption captures the objective.

Use the action buttons to choose Claim, Reinforce, or Weaken. Click a node to apply the selected action, or use End Turn to let corruption act without spending energy. Hover nodes to inspect ownership, type, integrity, threat, instability, and danger reason. Restart Mission resets the authored board deterministically.

## Validate

Run from the repository root:

```powershell
.\scripts\validate.ps1
```

The script checks required repository files, confirms Godot validation is retired, builds the .NET solution, and runs tests.

## Project Layout

```text
docs/                         Design and architecture notes
scripts/                      Developer automation
src/CodecTactics.Core/        Pure C# game-domain foundation
src/CodecTactics.MonoGame/    Active MonoGame frontend
src/CodecTactics.Godot/       Legacy Godot presentation scripts retained for reference
tests/CodecTactics.Core.Tests/ Console test runner
project.godot                 Legacy Godot project metadata
Codec_Tactics.csproj          Legacy Godot C# project
Codec_Tactics.sln             .NET validation solution
```

## Milestone 3.75 Mechanics

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
- The vertical-slice mission uses `MissionDefinition.CreateVerticalSlice()` with a fixed 5x5 board and objective-hold win/loss rules.
- Objective hold progress increases after successful player actions or end turns that leave the objective player-owned after corruption resolves.
- The mission ends in loss if the player core is no longer player-owned or the objective becomes corrupted.
- See `docs/network-integrity.md` for formulas, examples, collapse behavior, and strategy implications.
- See `docs/board-definition.md` for board definition and configuration details.
- See `docs/playable-vertical-slice.md` for the current mission route, feedback, tests, and limitations.

## Milestone 1.5 Visible Prototype

- The original Milestone 1.5 Godot prototype is now retired from the active workflow.
- The active MonoGame frontend renders the current authored mission board with temporary 2D nodes and connection lines.
- Neutral, player, enemy/corruption, and reinforced nodes have distinct visual treatments.
- Node types are labeled and outlined in distinct colors.
- Valid reachable neutral clicks claim nodes through `CodecTactics.Core`.
- Invalid clicks do not mutate core state and update the HUD status text.
- End Turn uses the real core corruption turn.
- The window title shows selected action, turn, energy, result, and last action status.
- Restart Mission starts a fresh deterministic copy of the authored mission.

## Current Limitations

- MonoGame visuals are temporary debug-style UI, not final art.
- Legacy Godot files remain in the repository but are not active workflow targets.
- The playable slice is still one authored 5x5 scenario.
- Network integrity and threat formulas are prototype balance values.
- Alternate board definitions are engine-supported but not yet balanced scenarios.
- No layers, cube visualization, advanced AI, save/load, broader balancing, real art, or production UI.
