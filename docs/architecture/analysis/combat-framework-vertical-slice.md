# 架构设计与影响面分析：战斗框架竖切

### 基本信息
- **需求名称**: Board + Entity 加厚（数值/战斗/移动竖切）
- **分析日期**: 2026-07-28
- **复杂度**: A
- **选型**: 竖切 Demo / Monorepo Packages / AVZ 不迁

---

### 一、系统定位

```
Core
 ├─ Board          （新建）
 └─ Entity 0.2     （加厚；Move 依赖 Board）
      └─ Samples~/CombatGridDemo
```

- Board **不**引用 Entity（占位存 `object` / `IBoardOccupant`）  
- Entity 引用 Core + Board  

---

### 二、设计决策

**推荐**: 精简重写 + 接口占位 + Sample 驱动验收  

| 模块 | 最小 API |
|------|----------|
| Board | `GridBoard(width,height)`，`TryGetTile`，`TryOccupy`/`Vacate`，`GetNeighbors` |
| Move | `TryMoveTo(tile)` / `TryMoveToward(target)` |
| Attack | `range`（格数）、`cooldown`、`TryAttack(target)` |
| State | Idle↔Attack↔Dead（攻击时切入 Attack） |
| Anim | `IAnimBridge` 默认 `NullAnimBridge` |

**不采用**: 剪切 AVZ MapManage；完整 Transition SO；A*  

---

### 三、影响面（本期）

| 项 | 结论 |
|----|------|
| `Assets/Script/**` | 不改 |
| 新增 | `Packages/...board/**` |
| 修改 | `Packages/...entity/**`（加厚、升版本） |
| 风险 | Entity 0.1 API 扩展；Sample 需同步 |

---

### 四、风险

| 风险 | 缓解 |
|------|------|
| 做成大而全 | 验收只认 Sample 互殴 |
| 循环依赖 | Board 零引用 Entity |
| 用户期望=AVZ 配置体验 | README 说明「代码/预制组装，非 Odin 长列表搬家」 |

---

### 五、结论

批准后按 P0 竖切实现 Board + Entity0.2 + CombatGridDemo。
