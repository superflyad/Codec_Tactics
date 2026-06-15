# Visible Prototype

Milestone 1.5 added the first launchable Godot scene for the 2D network prototype. Milestones 2 and 3 keep the same temporary scene and connect it to the strategic node, energy, corruption-pressure, integrity, threat, and collapse rules.

## Scope

The scene is intentionally temporary and visual-only. It renders the existing `CodecTactics.Core.Network.NetworkGame` state and sends player input back through core methods.

Included:

- 4x4 network board rendered as simple connected nodes.
- Neutral, player-owned, and enemy/corruption nodes use distinct colors.
- Standard, Resource, Relay, and Firewall nodes use distinct labels and outline colors.
- Reinforced nodes show an extra ring when `NodeState.Integrity` is greater than one.
- Clicking a valid reachable neutral node calls `NetworkGame.ClaimNodeWithResult`.
- Invalid clicks leave the core state unchanged and update the status text.
- The End Turn button calls `NetworkGame.EndPlayerTurnWithResult`, which resolves the real corruption turn without reinforcing a node.
- HUD labels show turn, phase, energy, corruption pressure, status, and result state.
- Player-owned nodes display integrity and threat values.
- Unstable nodes display an extra orange danger ring.
- Clicking owned nodes reports integrity, threat, instability progress, and the core-generated danger reason.
- Collapse events are reported through the status text after enemy-turn resolution.

Excluded:

- Layers.
- Cube visualization.
- Advanced AI.
- Save/load.
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
