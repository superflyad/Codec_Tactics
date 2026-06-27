# Legacy Godot Prototype

Milestone 1.5 added the first launchable Godot scene for the 2D network prototype. That prototype is now retired from the active workflow. The files remain for reference, but new frontend work should use `src/CodecTactics.MonoGame`.

## Scope

The scene is intentionally temporary and visual-only. It renders the existing `CodecTactics.Core.Network.NetworkGame` state and sends player input back through core methods.

Included:

- Authored 5x5 vertical-slice mission board rendered as simple connected nodes.
- Neutral, player-owned, and enemy/corruption nodes use distinct colors.
- Standard, Resource, Relay, and Firewall nodes use distinct labels and outline colors.
- Reinforced nodes show an extra ring when `NodeState.Integrity` is greater than one.
- Action buttons select Claim, Reinforce, or Weaken and route node clicks through `NetworkGame.ExecutePlayerAction`.
- Invalid clicks leave the core state unchanged and update the status text.
- The End Turn button calls `NetworkGame.EndPlayerTurnWithResult`, which resolves the real corruption turn without reinforcing a node.
- Restart Mission creates a deterministic fresh mission.
- HUD labels show turn, phase, energy, corruption pressure, selected action, objective hold progress, hover status, click status, and result state.
- Player-owned nodes display integrity and threat values.
- Unstable nodes display an extra orange danger ring.
- The objective node displays an extra yellow ring.
- Clicking owned nodes reports integrity, threat, instability progress, and the core-generated danger reason.
- Collapse events are reported through the status text after enemy-turn resolution.

Excluded:

- Layers.
- Cube visualization.
- Advanced AI.
- Save/load.
- Production art or animation.

## Launch

The Godot launch path is legacy-only:

```text
res://scenes/Main.tscn
```

The scene script is:

```text
res://src/CodecTactics.Godot/PrototypeScene.cs
```

The root Godot C# project referenced `src/CodecTactics.Core` so scene scripts could call the tested domain model directly.

## Validation

Run the standard repository validation for the active MonoGame workflow:

```powershell
.\scripts\validate.ps1
```

Validation no longer invokes Godot. It checks repository structure, builds the .NET solution, and runs the core tests.
