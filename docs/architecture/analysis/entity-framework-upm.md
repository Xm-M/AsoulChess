# 架构设计与影响面分析：Entity Framework UPM

### 基本信息
- **需求名称**: Entity Framework UPM（Chess 体系骨架）
- **所属模块**: `com.asoulchess.game.entity`
- **分析日期**: 2026-07-27
- **复杂度**: A
- **选型**: 范围2 + AVZ选项A + 无地图

---

### 一、系统定位

```
UnityEngine
    → com.asoulchess.game.core
        → com.asoulchess.game.entity   ★ 本需求
            → Samples~ / 未来游戏内容
            → （远期）AVZ Chess 适配层
```

**上游**: Core（Event / Timer / Pool）  
**下游（本期）**: 仅 Sample  
**横向**: 概念对齐 Chess / Skill / Buff；**不**引用 AVZ 程序集  

---

### 二、设计决策

**推荐**: 精简重写 + 组件化 Controllers + 依赖 Core 接口注入  

| AVZ 概念 | 包内建议 | 备注 |
|----------|----------|------|
| `Chess` | `GameEntity` | MonoBehaviour 根 |
| `Controller` | `IEntityController` | Init / EnterCombat / LeaveCombat |
| `PropertyController` | `PropertyController`（精简） | HP、ApplyDamage、Death |
| `SkillController` + `ISkill` | 同名职责精简版 | 无具体 Effect 实现库 |
| `Buff` / `TimeBuff` | `Buff` / `TimedBuff` | Timer 来自 Core |
| `StateController` | 最小 FSM | Idle / Dead（+可选 Attack） |
| `MoveController` + Tile | **不做** | 可选 `IBoardOccupant` 空接口 |
| `EventName` | `EntityEvents` 字符串常量 | 不进 Core、不进 AVZ 枚举 |

**备选（不采用本期）**:
- 剪切粘贴 Chess 文件夹 — 失败风险极高  
- 先迁 AVZ 再抽包 — 与选项 A 冲突  

**Sample 数据流**:
```
Spawn GameEntity
  → Add TimedBuff (ATK+)
  → Cast demo ISkill / ApplyDamage
  → HP<=0 → Trigger entity.death → 回收或销毁
```

---

### 三、影响面（本期）

| 项 | 结论 |
|----|------|
| `Assets/Script/**` | **不改** |
| 新增 | `Packages/com.asoulchess.game.entity/**` |
| Core | 只读依赖，预期不改 API（若缺能力再开小需求） |

---

### 四、风险

| 风险 | 等级 | 缓解 |
|------|------|------|
| 做成「大而全」 | 高 | P0 竖切清单；每周对照黑名单 |
| 技能规则与 AVZ 规则文档冲突 | 中 | 包内同样禁止技能专用运行时 MonoBehaviour |
| 用户期望「一引用就有全部棋子」 | 高 | README 明确：机制≠内容 |

风险在选项 A 下可接受。

---

### 五、结论

批准后按 A 级工期实现 **entity 包 + Sample**；地图与 AVZ 迁移另立需求。
