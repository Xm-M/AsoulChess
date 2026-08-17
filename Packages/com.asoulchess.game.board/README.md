# AsoulChess Game Board (`com.asoulchess.game.board`)

Rectangular grid: occupy / vacate / neighbors. No ice tiles, doors, or AVZ map rules.

## Types

- `IBoard`, `ITile`, `IBoardOccupant`
- `GridBoard`, `GridTile`

## Example

```csharp
var board = new GridBoard(8, 5, cellSize: 1f);
board.TryOccupy(0, 0, occupant);
foreach (var n in board.GetNeighbors(0, 0)) { /* ... */ }
```

Depends on `com.asoulchess.game.core`. Used by Entity Move/Attack (v0.2+).
