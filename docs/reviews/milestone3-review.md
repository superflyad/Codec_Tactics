# Milestone 3 Review: Network Integrity and Threat System

## Review Scope

This review audits Milestone 3 before adding layers, cube faces, or multiple board sizes. It covers the documented mechanics, the deterministic C# core, the Godot presentation boundary, and the console test coverage.

No gameplay behavior was changed as part of this review.

## Summary of Milestone 3

Milestone 3 adds calculated network integrity, calculated threat, instability tracking, collapse into corruption, deterministic corruption target priority, and visible risk information in the Godot prototype. The system makes topology matter: connected clusters are safer, long chains and isolated nodes are fragile, Relay and Firewall nodes provide structural support, and corruption pressure creates predictable escalation.

The current implementation is a playable and testable 4x4 2D strategy slice. It is not yet ready to carry layers or cube faces without a small hardening pass.

## Strengths

- The core rules remain outside Godot. `CodecTactics.Core` owns board state, actions, turn resolution, integrity, threat, collapse, and corruption targeting.
- Behavior is deterministic. Node ordering, row-major tie breaks, fixed pressure increases, and no random inputs make tests reproducible.
- The formulas are simple enough to inspect and explain in documentation.
- Invalid player actions do not mutate the board or advance corruption, which preserves player trust.
- Collapse events are returned through `GameActionResult`, so the UI can explain major state changes instead of silently changing ownership.
- Milestone 3 has tests for integrity calculation, isolation risk, Relay support, Firewall support, pressure progression, instability, collapse, and deterministic targeting.
- Documentation is aligned with the implemented prototype at a high level.

## Weaknesses

- The game is still effectively a single hard-coded scenario. `NetworkGame.CreateDefault()` assumes a player core at `(0,0)`, corruption at `(3,3)`, and a default 4x4 board.
- `NetworkBoard.CreateGrid(width, height)` accepts arbitrary sizes, but non-4x4 boards lose all special node placement while `NetworkGame` still uses fixed default starts. This creates an attractive API that is not actually balance-ready for other board sizes.
- The corruption targeting constants are embedded directly in `CorruptionTargetPolicy`: `+100` for unstable, `20 - integrity`, `+ threat`, and `-4` for Firewall. These values are balance-critical but are not named in `NetworkRules`.
- The integrity and threat formulas combine many unrelated concepts into one calculation pass. That is manageable now, but it will become harder to tune once layers add vertical distance, portals, or cross-face adjacency.
- `WeakenEnemyConnectionWithResult` spends `NetworkRules.ReinforceEnergyCost`. The cost is documented as "weaken cost", but there is no separate `WeakenConnectionEnergyCost` constant.
- The phrase "weak owned connections" means "0 or 1 adjacent player-owned nodes", not low `ConnectionState.Strength`. That is readable in code only after inspection and will confuse future balance work.
- Stable player-owned nodes can be selected as a corruption focus target but cannot be corrupted by expansion. This is intentional per docs, but it can make corruption appear to stall while pressure accumulates.
- The Godot prototype displays risk values, but it does not expose all available player actions. Reinforce and weaken exist in the core but are not usable through the current visual prototype.

## Technical Debt

- Scenario configuration is missing. Start positions, node type placement, board size, and rule constants should be scenario data or a clearly named ruleset before layer work.
- Balance constants are split between `NetworkRules` and inline targeting literals.
- Outcome rules are still placeholders and may produce misleading wins/losses once the board topology changes.
- `NodeState.SetOwner` preserves the previous player danger reason when ownership changes to player. The next risk refresh corrects this in normal flows, but the state transition itself is not self-contained.
- The core board model is 2D coordinate-first. That is fine for now, but cube faces and layers will need an adjacency abstraction that is not tied to grid X/Y assumptions.
- There is no snapshot or scenario test harness for comparing balance across multiple scripted turns.

## Missing Tests

- Alternate board sizes: 2x2, rectangular grids, and boards larger than 4x4.
- `NetworkGame` behavior when a board is not the default 4x4 scenario.
- Corruption targeting when stable player-owned nodes compete with neutral nodes.
- Weakened connection effects on reach, integrity, threat, and corruption pathing.
- Named tests for cost values, especially weaken cost as distinct from reinforce cost.
- Dense network integrity bonus at exactly three adjacent player-owned nodes.
- Long-chain distance penalties across several depths.
- Recovery from instability when reinforcement or topology brings threat back under integrity.
- Multiple simultaneous collapses and the ordering of collapse reporting.
- Godot-facing action coverage for reinforce and weaken workflows.

## Balance Table

| Constant or value | Current value | Source | Readiness concern |
|---|---:|---|---|
| Initial player energy | 5 | `NetworkRules.InitialPlayerEnergy` | Allows two claims plus one reinforce, or five reinforces. Reasonable for 4x4 only. |
| Claim cost | 2 | `NetworkRules.ClaimEnergyCost` | Aggressive expansion is expensive but still viable with Resource income. |
| Reinforce cost | 1 | `NetworkRules.ReinforceEnergyCost` | Cheap compared with collapse pressure; may dominate once the UI exposes reinforce. |
| Weaken cost | 1 | Uses `ReinforceEnergyCost` | Needs its own constant before tuning. |
| Resource income | 2 per owned Resource per new player turn | `NetworkRules.ResourceEnergyPerTurn` | Resource nodes can refund a claim immediately after turn resolution. Watch runaway energy on larger boards. |
| Corruption pressure increase | +1 each enemy turn | `NetworkGame.ResolveEnemyTurn` | Predictable and testable. May be too linear for larger or layered maps. |
| Standard corruption resistance | 1 | `NetworkRules.StandardCorruptionResistance` | One pressure is enough to spread into normal neutral nodes. |
| Firewall corruption resistance | 2 | `NetworkRules.FirewallCorruptionResistance` | Only one turn stronger than standard. This may be too weak for a defensive anchor. |
| Base integrity | 4 | `NetworkRules.BaseNetworkIntegrity` | Fine for the current formula, but several bonuses exceed it quickly. |
| Core connection bonus | +3 | `NetworkRules.CoreConnectionIntegrityBonus` | Strong and clear. Ties safety heavily to reachability from one core. |
| Isolation integrity penalty | -4 | `NetworkRules.IsolationIntegrityPenalty` | Severe; isolated nodes clamp to 1 unless heavily supported. |
| Relay integrity support | +2 | `NetworkRules.RelayIntegritySupport` | Relay is both reach and integrity support, which may make it overly central. |
| Firewall integrity support | +3 | `NetworkRules.FirewallIntegritySupport` | Strong support, but neutral Firewall resistance is modest. |
| Adjacent owned support | +1 each | `NetworkRules.AdjacentSupportIntegrityBonus` | Good local clustering incentive. |
| Dense network bonus | +2 at 3+ owned adjacent nodes | `NetworkRules.DenseNetworkIntegrityBonus` | Only possible in limited grid positions; board topology will skew this sharply. |
| Long-chain penalty | -1 per core distance after first step | `NetworkRules.LongChainDistancePenalty` | Works in 2D; needs review for vertical/layer distance. |
| Adjacent corruption threat | +4 per adjacent enemy node | `NetworkRules.NearbyCorruptionThreat` | High enough to force urgent response. |
| Corruption pressure threat | pressure / 2, rounded down | `NetworkRules.CorruptionPressureThreatDivisor` | Slow global pressure. May hide danger until late. |
| Weak owned connection threat | +2 at 0 or 1 owned adjacent nodes | `NetworkRules.WeakConnectionThreat` | Name conflicts with physical connection strength. |
| Frontier exposure threat | +1 per adjacent neutral node | `NetworkRules.FrontierExposureThreat` | Encourages filling borders, but larger boards will create constant frontier tax. |
| Isolation threat penalty | +4 | `NetworkRules.IsolationThreatPenalty` | Pairs with isolation integrity penalty for immediate danger. |
| Collapse threshold | 2 unstable enemy turns | `NetworkRules.InstabilityTurnsBeforeCollapse` | Readable and fair for 4x4. Might be too fast for layered maps. |
| Unstable target priority | +100 | Inline in `CorruptionTargetPolicy` | Hard override; should be named and tuned. |
| Low-integrity target priority | max(0, 20 - integrity) | Inline in `CorruptionTargetPolicy` | The `20` anchor is unexplained and likely brittle. |
| Firewall target priority modifier | -4 | Inline in `CorruptionTargetPolicy` | Duplicates the defensive concept outside resistance/integrity. |

## Balance Risks

- Relay nodes currently provide reach and integrity support. If layers add sparse vertical links, Relay nodes may become mandatory rather than strategic.
- Resource income can scale linearly with map size while costs stay flat. Larger boards may turn early Resource capture into an energy snowball.
- Firewall identity is split: it gives high integrity support when player-owned but only +1 extra resistance while neutral. That may make Firewalls more valuable after capture than as map obstacles.
- The collapse threshold of two enemy turns is clear but unforgiving. On layered maps, players may need more time to notice and respond to remote instability.
- Corruption pressure has no scenario scaling. A 4x4 board and a larger layered board receive the same +1 pressure per enemy turn.
- Dense bonuses depend on local adjacency count. Cube faces, layer portals, and non-grid topologies may accidentally make some regions much safer than others.

## UX and Readability Risks

- The prototype can show integrity, threat, instability, and danger reasons, but the text is dense and debug-like.
- The player can click to claim and end turn in Godot, but cannot reinforce or weaken from the UI despite those being important responses to Milestone 3 danger.
- Collapse messages are present, but there is no visual history or preview that explains "this node will collapse next enemy turn."
- `I#/T#` labels are compact but not self-explanatory for non-developer playtesting.
- Stable corruption focus without spread may look like nothing happened unless the HUD message is read carefully.

## Recommended Fixes Before Layers

1. Introduce a scenario/rules configuration object for board size, start positions, node type placement, and balance constants.
2. Move corruption target priority values into named constants or a tunable targeting rules object.
3. Add a separate `WeakenConnectionEnergyCost` constant and update docs/tests to use that name.
4. Rename or clarify `WeakConnectionThreat` so it describes weak ownership support rather than weakened connection strength.
5. Add tests for alternate board sizes and explicitly document which sizes are supported by Milestone 3.
6. Add tests for weakened/inactive links affecting reachability and risk calculations.
7. Add instability recovery tests so reinforcement/topology repair behavior is protected.
8. Expose reinforce and weaken in the Godot prototype before judging balance from play.
9. Add a turn-by-turn scripted scenario test that asserts ownership, pressure, energy, and collapse events across several turns.

## Recommended Fixes Before Cube Faces

1. Separate topology from rectangular X/Y grid generation. Cube faces need explicit adjacency, not only coordinate neighbors.
2. Treat row-major ordering as a deterministic policy, not a universal map ordering. Cube faces may need stable scenario-defined node order.
3. Make distance calculations topology-based and label whether they measure path distance, same-face distance, or cross-face distance.
4. Decide how Relay reach, long-chain penalties, dense bonuses, and nearby corruption work across face edges.
5. Add tests for non-orthogonal adjacency before rendering cube geometry.

## Recommended Fixes Before Multiple Board Sizes

1. Stop presenting `CreateGrid(width, height)` as equivalent to a balanced scenario.
2. Define node type distribution per size or provide deterministic generation rules.
3. Scale Resource availability, corruption pressure, collapse threshold, or costs by scenario if needed.
4. Test minimum board sizes and rectangular boards.
5. Verify the Godot layout does not assume only 4x4 placement.

## Recommended Next Milestone

The next milestone should be a Milestone 3.25 hardening and balance-readiness pass, not layers yet.

Suggested scope:

- Add scenario configuration.
- Name all balance constants.
- Add alternate-board and topology tests.
- Add reinforce and weaken controls to the visible prototype.
- Run a small scripted balance suite over the current 4x4 scenario.

After that, Milestone 3.5 Layers can be added on a safer foundation.

## Go/No-Go Recommendation for Adding Layers

No-go for layers right now.

Milestone 3 is a solid deterministic prototype, but layer work would currently stack new topology, distance, balance, and UX complexity on top of hard-coded 4x4 assumptions. The system should first receive a small configuration and test hardening pass so layers are added to a rule model that can actually scale.

Go condition: add layers only after scenario configuration, named targeting constants, alternate-size tests, inactive-link risk tests, and visible reinforce/weaken player responses are in place.
