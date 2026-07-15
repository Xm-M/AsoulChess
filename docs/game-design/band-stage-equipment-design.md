# 乐队舞台器具 — 策划设计

> **文档性质**：玩法与数值设计（扩音箱 / 麦克风 / 聚光灯 / 节拍器 / 调音机）  
> **最后更新**：2026-07-01  
> **文档状态**：**概念与结构已写入**；具体数值与节拍器/调音机等细节 **后续再定**，暂不进入开发  
> **关联**：[乐队 × 乐器羁绊词条库](./band-instrument-bonds-reference.md) · [植物构筑设计](./plant-deckbuilding-design.md) · [需求卡片](../requirements/band-stage-equipment.md)

---

## 1. 设计定位

在「乐器 = 职业棋子」之外，补充 **舞台扩声与排练器具** 作为可种植单位，强化少女乐队 PVZ 的 Live 演出感。

| 器具 | PVZ 类比 | 定位类型 | 占用关系 |
|------|----------|----------|----------|
| **扩音箱** | 火炬树桩 | `MainPlant` | 独占一格，子弹**穿过**该格时被强化 |
| **麦克风** | 南瓜罩 | `SupportPlant` | 套在已有 Main 上，持续存在 |
| **聚光灯** | 南瓜罩 | `SupportPlant` | 套在 Main 上；宿主**四邻不同友军**加攻 |
| **节拍器** | 咖啡豆类 | `Consume` + `ConsumePlant` | 种在 Main 上，一次性消耗 |
| **调音机** | 咖啡豆类 | `Consume` + `ConsumePlant` | 种在 Main 上，一次性消耗 |

**主题标签**（`plantTags` 建议）：`舞台器具`（五者共有，便于羁绊或商店筛选，可选）

**Support 互斥**：同格 Main 仅能挂 **1 个** Support → 麦克风 / 聚光灯 / 南瓜罩 **不可同格叠放**。

---

## 2. 扩音箱（Speaker / 主扩）

### 2.1 玩家理解

「把音箱种在子弹路径上，穿过的攻击会被**扩音**——更响、更伤。」

### 2.2 种植规则

- `PlantType`: **MainPlant**
- `IPlantFunction`: **MainPlant**（与豌豆射手相同，独占草地格）
- `chessTileType`: **Grass**（默认；后续可扩展 Livehouse 专用地格）
- 同格不可与其他 Main 共存；可与 **Support（麦克风）** 叠放（若该格另有 Main——实际上扩音箱自己是 Main，麦克风应种在**射手 Main** 上而非音箱格，见 §3）

### 2.3 核心机制（火炬树桩）

**触发**：友方 `ElementType.Bullet` 弹道在飞行过程中 **经过** 扩音箱所在格（中心点进入该 `Tile` 范围，或沿射线检测到该格）。

**效果（方向已定，数值待定）**：

| 参数 | 推荐值 | 说明 |
|------|--------|------|
| 伤害倍率 | **×2**（待定） | 对该发子弹后续命中均生效（与 PVZ 火炬一致） |
| 元素转化 | **声波 / 扩音**（新 `ElementType` 子类或 Buff 标记） | 视觉：子弹变色 + 轻微震屏；便于与「火焰 pea」区分 |
| 叠层 | **每格最多强化 1 次** | 同一子弹重复穿过多个音箱仍只乘 2（或策划改为叠乘，默认不叠） |
| 敌我 | **仅友方子弹** | 僵尸弹幕忽略 |

**不生效**：

- 近战、`CloseAttack`、AOE 地面技能、抛物线落点伤害（不经过格心）
- 已标记「不可扩音」的特殊弹（Boss 机制预留）

### 2.4 数值与池子

| 字段 | 建议 |
|------|------|
| 阳光/费用 | **175**（对齐 PVZ 火炬树桩档位） |
| HP | **3000～4000**（介于输出 plant 与南瓜罩之间） |
| 进池 | 冒险 / 肉鸽均可；非乐队专属，作为 **通用舞台工具 plant** |

### 2.5 实现要点（程序）

- 当前弹道 **`IBulletMove_LineMove` 无穿格检测**，需新增 **BulletPassTile / 格增强注册表**（扩音箱入场注册、离场注销）。
- 参考：`TileEffect` 生命周期、`PassiveSkillEffect_PumpkinShell` 的进战/离场监听模式。
- 存档：扩音箱为普通 Main，无额外状态；子弹强化为运行时标记，不需序列化。

### 2.6 美术 / 演出

- 待机：音箱箱体 + 轻微低音震动
- 触发：格内一圈声波环扩散；子弹穿过时拖尾变亮
- Reanim 参考：`reanim_all` 中若有 Torchwood 资源可改色复用

---

## 3. 麦克风（Mic / 麦架）

### 3.1 玩家理解

「给队友架上麦，让她像**主唱**一样有概率多唱一句（额外子弹）。」

### 3.2 种植规则

- `PlantType`: **SupportPlant**
- `IPlantFunction`: **SupportPlantRequireMain**（新类或配置约束）：**必须** `tile.stander != null` 且 stander 为 `MainPlant`
- 同格仅允许 **1 个 Support**（沿用现有 `SupportPlant` 互斥规则）
- **不可**种在空地上

### 3.3 核心机制（南瓜罩式附着）

**触发**：种植成功瞬间（`ConsumablesSkill` 式入场 **`UseSkill` 不适用**——麦克风是持久 Support，用 **PassiveSkill + Support 入场**）。

**效果（方向已定，数值待定）**：

对 **同格 `tile.stander`（宿主 Main）** 施加 **`Buff_MicVocal`**（麦克风主唱 Buff）：

| 参数 | 推荐值 | 说明 |
|------|--------|------|
| `extraBulletChance` | **0.35**（待定） | 每次普攻额外 1 发子弹 |
| 额外子弹 | 复用宿主 `equipWeapon` 同款弹 或 轻量声波弹 | 伤害 = 宿主攻击力 × 0.5 |
| 持续时间 | **永久**（直到麦克风被毁或宿主死亡） | 与羁绊 `Buff_Vocal` 可叠加规则见下 |
| 与羁绊主唱 | **可共存** | 羁绊 `Buff_Vocal` 按人数给概率；麦克风给 **固定 35%**；两者独立 roll |

**生命周期**：

- 宿主 Main 死亡 → 移除 `Buff_MicVocal`；麦克风可保留或随 Main 一起清除（**推荐：麦克风仍占 Support 位但失效，直至玩家铲除**）
- 麦克风死亡 → 移除宿主身上由该麦克风施加的 Buff

### 3.4 数值与池子

| 字段 | 建议 |
|------|------|
| 阳光/费用 | **125**（对齐南瓜罩） |
| HP | **4000**（承伤可选：默认 **不替 Main 承伤**，仅提供 Buff；若需承伤再开分支） |
| 进池 | 通用；对 **非主唱 tag** 的 Main 价值更高 |

### 3.5 与 `Buff_Vocal` 关系

| 来源 | 概率 | 备注 |
|------|------|------|
| 羁绊 `Vocal` Fetter | 2～5 人：20%～100% | 已有实现 |
| 麦克风 `Buff_MicVocal` | 固定 35% | 装备型，不随乐队人数变 |

---

## 4. 聚光灯（Spotlight / 追光）

### 4.1 玩家理解

「给 C 位打追光——**周围队友越多、越分散，她越能打**。」

### 4.2 种植规则

- `PlantType`: **SupportPlant**
- `IPlantFunction`: **SupportPlantRequireMain**（与麦克风相同）：**必须** `tile.stander != null` 且 stander 为 `MainPlant`
- 同格仅 **1 个 Support**（与麦克风、南瓜罩互斥）
- **不可**种在空地上
- **不替 Main 承伤**（与麦克风一致，与南瓜罩差异化）

### 4.3 核心机制

**受益对象**：同格 **`tile.stander`（宿主 Main）** 的攻击力。

**统计范围**：宿主所在格的 **四邻格**（上下左右十字 4 格，与 `MapManage.NearTile` / 工程里「四邻」一致；**不含对角**）。

**计数规则（方向已定）**：

1. 遍历四邻每一格，若该格存在存活友方 **`stander`（MainPlant）**，则计为 **1 名友军**。
2. **「不同友军」**：按 **棋子实例** 去重（同一格最多 1 个 stander）；四邻最多贡献 **4** 个计数。
3. **是否要求「不同 chessName / 不同成员」** → **待定**（见 §9 待确认 #7）。

**攻击加成（公式草案）**：

```text
宿主额外攻击 = uniqueAllyCount × attackPerAlly

uniqueAllyCount = 四邻格内存活友方 Main 的数量（0～4）
attackPerAlly    = 每名友军提供的攻击力（待定，如 +20 / +10% 基础攻击）
```

**刷新时机**（待定，推荐）：

- 聚光灯种下时立即计算一次；
- 之后 **周期性刷新**（如 0.5s，参考 `Buff_DrummerAura`）或 **邻格 planting/death 事件** 时重算；
- 聚光灯或宿主死亡 → 移除 `Buff_SpotlightAttack` 并停止刷新。

**与鼓手羁绊**：鼓手给 **百分比增伤光环**；聚光灯给 **固定/按邻格计数的攻击加值** → 鼓励「人墙围 C 位」布局，与鼓手 **可叠加**（数值待平衡）。

### 4.4 数值与池子（待定）

| 字段 | 建议 |
|------|------|
| 阳光/费用 | **125～150**（Support 档，略高于南瓜因需布局） |
| HP | **3000～4000** |
| `attackPerAlly` | **待定**（占位：+20 固定攻击，或 +8% 宿主基础攻击） |
| 进池 | 通用；乐队关 / 舞台区关卡权重可略高 |

### 4.5 演出

- 宿主头顶：追光锥形束 + 亮度随 `uniqueAllyCount` 增强
- 四邻有友军时：邻格地面小光斑连到宿主

### 4.6 实现要点（程序，后续）

- 被动 `PassiveSkillEffect_Spotlight` + `Buff_SpotlightAttack`（内装 `ChangeAttack` 或 `ExtraDamage`）
- 复用 `MapManage.instance` 四邻格查询；**勿**用八邻（与需求「四格」一致）
- 存档：Buff 需保存当前 `uniqueAllyCount` 或等效攻击增量

---

## 5. 节拍器（Metronome）— 消耗品

### 5.1 玩家理解

「给队友卡节拍——一段时间内打得更稳、更快。」

### 5.2 种植规则

- `PlantType`: **Consume**
- `IPlantFunction`: **ConsumePlant**（仅 `tile.stander != null`）
- `activeSkill`: **ConsumablesSkill**（入场即 `UseSkill`，结束后 `Death()`）
- 白卡消耗品 UI

### 5.3 效果方案（待策划拍板，细节后定）

| 方案 | 效果 | 优点 | 缺点 |
|------|------|------|------|
| **A（推荐）** | 宿主 **攻速 +30%**，持续 **15s** | 直观、易调、与鼓手羁绊不重复（鼓手是增伤光环） | 需 Buff 支持 `ChangeAcceleRate` |
| B | 全场友方攻速 **+10%**，持续 10s | 团队向 | 与鼓手 aura 重叠感强 |
| C | 宿主 **下一次主动技能 CD 归零** | 爆发感强 | 仅对有主动技 plant 有价值 |
| D | 接下来 **20 次攻击** 必触发额外子弹（若已有 Vocal） | 与麦克风 synergize | 规则较绕 |

**文档暂记推荐：方案 A**（未拍板）

```text
Buff_MetronomeHaste:
  attackSpeedBonus = +0.30   // ChangeAcceleRate
  duration = 15s
  视觉：宿主脚下节拍圈闪烁（1/4 拍粒子）
```

### 5.4 数值与池子

| 字段 | 建议 |
|------|------|
| 阳光/费用 | **50～75** |
| 单关携带 | 不限（与咖啡豆同类） |
| 进池 | 冒险早期即可进；肉鸽商店/Common 奖 |

---

## 6. 调音机（Tuner）— 消耗品

### 6.1 玩家理解

「校准音准——把队友调回最佳状态。」

### 6.2 种植规则

同节拍器：**Consume + ConsumePlant + ConsumablesSkill**。

### 6.3 效果方案（待策划拍板，细节后定）

| 方案 | 效果 | 优点 | 缺点 |
|------|------|------|------|
| **A（推荐）** | 宿主 **HP 回满** + **清除所有 Debuff** | 救援向，主题贴合「调音校准」 | 强恢复需控费 |
| B | 宿主 **最大生命 +15%**（本场永久，可叠 1 次） | 长期价值 | 与贝斯羁绊 HP 重叠 |
| C | 宿主 **护甲 +50**，持续 20s | 偏防守 | 与键盘羁绊重叠 |
| D | 随机 **乐器羁绊 +1 层**（伪计数，仅该 plant 获得一层临时职业 Buff） | 构筑向 | 实现复杂 |

**文档暂记推荐：方案 A**（未拍板）

```text
SkillEffect_Tuner:
  1. host.propertyController.HealToMax() 或等价
  2. host.buffController.ClearDebuffs()   // 需约定 Debuff 枚举
  3. 短 VFX：音叉振动 + 绿色音准环
  4. ConsumablesSkill.SkillOver → 调音机 Death()
```

**边界**：

- 宿主已满血且无 Debuff：仍消耗调音机，播放轻量 VFX（避免「用了亏费」的挫败——可选返还 25 阳光，默认不返还）
- 不可对僵尸/空格子使用

### 6.4 数值与池子

| 字段 | 建议 |
|------|------|
| 阳光/费用 | **75～100**（略高于节拍器，因清 debuff 价值高） |
| 进池 | 中期；肉鸽 Event「排练室」可奖励 |

---

## 7. 协同与布局（构筑提示）

```text
  [豌豆 Main] ──子弹──→ [扩音箱格] ──×2──→ 僵尸

  [吉他 Main] + [麦克风 Support] ──35% 双发──→ 僵尸

  四邻围合布局（聚光灯）:
        [友A]
          |
  [友B]-[C位+聚光灯]-[友C]
          |
        [友D]
  → C位攻击 + 4 × attackPerAlly（若四格都有 Main）

  [濒危 Main] + [调音机 Consume] 满血 + 清 debuff
```

| 组合 | 效果 |
|------|------|
| 扩音箱 + 后排连射 Main | 经典火炬阵， band 主题换皮 |
| 麦克风 + 非主唱输出 | 廉价「伪主唱」 |
| **聚光灯 + 四邻主力** | 围成十字阵，C 位攻击最大化 |
| 聚光灯 vs 麦克风 | 同格 **二选一** Support |
| 节拍器 → 任意输出 Main | 攻速爆发（效果待定） |
| 调音机 + 南瓜/键盘 | 承伤或护甲后拉满血线 |

---

## 8. 配置清单（实现阶段，后续）

| 资产 | 类型 | 脚本要点 |
|------|------|----------|
| `扩音箱.asset` | PropertyCreator | MainPlant；Passive 注册 bullet pass |
| `麦克风.asset` | PropertyCreator | SupportPlantRequireMain；Passive 加 Buff |
| `聚光灯.asset` | PropertyCreator | SupportPlantRequireMain；四邻扫描 + 动态加攻 |
| `节拍器.asset` | PropertyCreator | Consume；ConsumablesSkill + SkillEffect |
| `调音机.asset` | PropertyCreator | Consume；ConsumablesSkill + SkillEffect |
| `Buff_MicVocal` | Buff 类 | 参考 `Buff_Vocal`，固定概率 |
| `Buff_SpotlightAttack` | Buff 类 | 按邻格友军数动态 `ChangeAttack` |
| `Buff_MetronomeHaste` | TimeBuff | 攻速 |
| Bullet pass 服务 | 新基础设施 | 扩音箱专用 |

**Reanim / Prefab**：`Assets/Prefab/ChessPrefab/Stage/`（建议新目录）

---

## 9. 待确认项（后续再定）

| # | 问题 | 文档暂记 |
|---|------|----------|
| 1 | 扩音箱：×2 伤害 vs 声波元素？ | ×2 + VFX；细节后定 |
| 2 | 麦克风：是否承伤？ | **否** |
| 3 | 节拍器 / 调音机方案 | 暂记 A，**未拍板** |
| 4 | 舞台器具羁绊 | 第一版 **不做** |
| 5 | 进池策略 | 通用进池 |
| 6 | 聚光灯：`attackPerAlly` 用固定值还是百分比？ | 待定 |
| 7 | 聚光灯：「不同友军」= 实例即可，还是必须 **不同 chessName**？ | 待定（默认按 **四邻 stander 个数** 最简） |
| 8 | 聚光灯：邻格是 **Support / Pot** 无 Main 是否计数？ | 默认 **仅 Main stander** |

---

## 10. 版本记录

| 日期 | 说明 |
|------|------|
| 2026-07-01 | 初稿：扩音箱 / 麦克风 / 节拍器 / 调音机 |
| 2026-07-01 | 新增 **聚光灯**；全文标为「概念沉淀，细节后续拍板」 |
