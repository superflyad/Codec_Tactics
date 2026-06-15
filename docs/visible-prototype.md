# Visible Prototype

Milestone 1.5 adds the first launchable Godot scene for the 2D network prototype.

## Scope

The scene is intentionally temporary and visual-only. It renders the existing `CodecTactics.Core.Network.NetworkGame` state and sends player input back through core methods.

Included:

- 4x4 network board rendered as simple connected nodes.
- Neutral, player-owned, and enemy/corruption nodes use distinct colors.
- Reinforced nodes show an extra ring when `NodeState.Integrity` is greater than one.
- Clicking a valid adjacent neutral node calls `NetworkGame.ClaimNode`.
- Invalid clicks leave the core state unchanged and update the status text.
- The End Turn button calls `NetworkGame.ReinforceNode` on the player start node as a placeholder pass action, which still uses the current tested core turn flow.
- HUD labels show turn, phase, status, and placeholder result state.

Excluded:

- Layers.
- Cube visualization.
- Advanced AI.
- Save/load.
- Resource systems.
- Production art or animation.

## Launch

Open this repository with Godot .NET 4.x or newer and run the configured main scene:

```text
res://scenes/Main.tscn
```

The scene script is:

```text
res://src/CodecTactics.Godot/PrototypeScene.cs
```

The root Godot C# project references `src/CodecTactics.Core` so scene scripts can call the tested domain model directly.

## Validation

Run the standard repository validation:

```powershell
.\scripts\validate.ps1
```

If `godot` or `godot4` is on `PATH`, validation runs Godot in headless mode and builds the Godot C# solution. If Godot CLI is not available, the script reports the skip clearly and continues with structure checks plus .NET build/tests.
