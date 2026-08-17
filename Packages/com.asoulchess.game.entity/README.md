# AsoulChess Game Entity (`com.asoulchess.game.entity`) v0.6 — P1-3

Framework analogue of AVZ `Chess` **mechanics** — not character content.

## Depends on

- `com.asoulchess.game.core` ≥ 0.2.0

Grid Move/Attack: `Optional/Board/` + `com.asoulchess.game.board`.

## P1-3: State

| Type | Role |
|------|------|
| `IEntityState` | Enter / Execute / Exit (AVZ `State`) |
| `EntityStateGraph` | Nodes + transitions (AVZ `StateGraph`) |
| `StateController` | Tick, ChangeState, overlay, RevertToPrevious |
| Built-in states | Idle, Attack, Skill, Dead, Dizzy, Prepare, Move |
| `ISimulationGate` | Not running → Prepare state |
| `IAnimBridge.PlayDizzy` | Stun animation hook |

`PropertyController.ChangeDizzinessTime` enters **Dizzy** when duration &gt; 0 (AVZ-aligned).

## P1-2: Buff

`Buff`, `BuffController`, `StatModifierBuff`, `TimedBuff`, `DamageInfo.AttachedBuff`.

## P1-1: Property

`EntityStats`, `DamageInfo`, `PropertyController` combat pipeline.

## Quick start

```csharp
entity.State.Anim = myAnimBridge;
entity.EnterCombat();
entity.State.ChangeState(EntityStateId.Attack);

entity.Property.ChangeDizzinessTime(2f); // → Dizzy, auto revert

entity.State.AddTransition(EntityStateId.Idle,
    new FuncStateTransition(e => someCondition),
    EntityStateId.Attack);
```

## Roadmap

- **P1-4** Skill → **P1-5** Attack → **P1-6** IAnimBridge expansion
- StateGraph SO / AVZ adapter: optional, later
