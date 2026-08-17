# 植物构筑设计指南（Plant Deckbuilding Design）

> **文档性质**：策划 / 设计记录（**非**实现需求卡片）。  
> **最后更新**：2026-08-13（独立植物表单刷新：85 张 / 15 张待归类）

---
> **维护方式**：讨论定稿、数值表、新 plant 定位时更新本文；具体单位实现仍走 [单位制作.md](../单位制作.md)。

---

## 0. 本文档解决什么问题

| 已有文档 | 写什么 | 缺什么 |
|----------|--------|--------|
| [单位制作.md](../单位制作.md) | 一个 plant **在 Unity 里怎么配**（PropertyCreator、Skill、Prefab） | 不讲 Run 构筑、体系、平衡 |
| [roguelike-plant-pick-reward.md](../requirements/roguelike-plant-pick-reward.md) | 肉鸽 **怎么抽** plant 奖励 | 不定义 plant 本身该怎么设计 |
| [band-instrument-bonds-reference.md](./band-instrument-bonds-reference.md) | 乐队 / 乐器 **羁绊词条库** | 不与单张 plant 数值联动 |

**本文档**：记录 **PVZ + 肉鸽（尖塔式 Run）** 语境下，plant 作为「牌」的 **构筑维度、资源轴、分类与检查清单**。

> 设计演进与「冒险 vs 肉鸽 vs 羁绊」决策：[plant-band-design-evolution.md](./plant-band-design-evolution.md)

---

## 1. 构筑的两个维度（对齐杀戮尖塔）

```text
┌─────────────────────────────────────────────────┐
│  Plant 池 / 本关手牌  →  「体系 / 怎么赢」      │
│  （联动、引擎、输出、地形）                      │
├─────────────────────────────────────────────────┤
│  Run 道具 (Prop)      →  「加速器 / 改规则」    │
│  （+费、+槽、减 CD、放大某标签）                │
└─────────────────────────────────────────────────┘
```

- **Plant**：多张在一起 **1+1>2**；三选一 / 商店要问「进不进我的体系？」
- **Prop（≈ 遗物）**：不占手牌槽，改 **全局规则** 或 **放大某一类 plant**；有时也会 **反向定体系**（先拿 Prop 再按 Prop 抓 plant）

**本项目映射**：

| 尖塔 | AVZ |
|------|-----|
| 卡组 | Run `ownedPlantCreatorIds`（仓库池）+ 本关 `PlantsShop` 顶栏手牌 |
| 遗物 | `ownedPropIds` / `PropItemData` |
| 抓牌 | 三选一 PlantPick、地图商店、`runGold` 购买 |
| 删牌 / 升级 | 休息房（待扩展，见 [roguelike-rest-node.md](../requirements/roguelike-rest-node.md)） |

---

## 2. 资源三层模型

设计任意 plant / Prop / 奖励前，先标明动哪一层：

### 2.1 Run 层（地图之间持久）

| 资源 | 代码 / 配置 | 设计用途 |
|------|-------------|----------|
| Run 金币 | `RoguelikeRunState.runGold` | 商店、事件消费 |
| 植物池 | `ownedPlantCreatorIds` | 扩大 **可选 plant 集合**（≈ 往牌库加牌） |
| Run 道具 | `ownedPropIds` | 全局被动 / 主动（≈ 遗物） |
| 手牌槽上限 | `PlantsShop.maxCount`（默认 10，可扩展） | 「本关能带几张上场」 |
| Run 命（可选） | 尚未统一抽象 | 失败即 End Run；休息房可回血类扩展 |

### 2.2 战斗层（单关内）

| 资源 | 代码 / 配置 | 设计用途 |
|------|-------------|----------|
| **阳光 Sun** | `SunLightPanel`、`Property.price` | **主费用**（≈ 尖塔 Energy） |
| **顶栏手牌** | `PlantsShop.currentShopIcons` | 本关已选 plant，点击消耗 sun 种植 |
| **场地格子** | `Tile`、`PlantType`、占格规则 | **空间费用**（尖塔没有） |
| **波次 / 时间** | `LevelController`、Wave | 压力轴，非回合制 |
| **CD / 攻速** | `Property.CD`、`acceleRated` | 部署后的 **持续资源** |

### 2.3 主题层（乐队 / 元素，可选）

| 资源 | 代码 / 配置 | 设计用途 |
|------|-------------|----------|
| 羁绊层数 | `Fetter`、`plantTags` | 同乐队 / 同乐器计数 |
| Fever 等 | 如 `Buff_AveMujica` | **特殊计数条**（≈ 职业机制） |

**原则**：Run 层改 **池子和规则**；战斗层改 **sun 与场面**；不要在一个效果里混三层 unless 刻意设计。

---

## 3. 与杀戮尖塔的对照（不必硬抄）

| 尖塔机制 | PVZ / 本项目 | 建议 |
|----------|--------------|------|
| 每回合 Energy | 阳光（开局 + 产 sun plant） | ✅ 主费用轴 |
| 手牌上限 | `maxCount` 顶栏槽 | ✅ |
| 抽牌堆循环 | 弱 | ❌ 不硬做弃牌堆；用 **池 + 10 槽** 代替 |
| Block 护甲 | 无经典 Block | ⚠️ 用 plant Buff / AR，不做全局 Block |
| 消耗生命出牌 | 无 | ❌ |
| 0 费过牌 | 产 sun 不攻击 plant | ✅ 引擎牌 |
| 删牌 | 休息房删弱 plant（待做） | ⚠️ P1 |
| 升级牌 | 休息房升数值（待做） | ⚠️ P1 |

---

## 4. Plant 战斗定位（七类 — 策划主标签）

> **状态**：策划定稿 v1（2026-06-11 第二轮）。后续在 `PropertyCreator` 增加 `plantRoles` + `catalogKind`（消耗品单列）。  
> **数据源**：`Assets/Resources/ChessData/Player/*.asset`（**67** 张）。

与 PVZ 经典定位的对照：

| ID | 定位 | PVZ 参照 | 设计要点 |
|----|------|----------|----------|
| **1** | **前排** | 坚果墙、高坚果 | 高 HP / 减伤 / 套壳；挡路换时间 |
| **2** | **输出** | 豌豆、西瓜 | 持续或爆发伤害；构筑「怎么赢」 |
| **3** | **辅助** | 向日葵（非产 sun 的增益）、医疗 | 回血、Buff、团队增益；低直伤 |
| **4** | **产出阳光** | 向日葵、双发阳光 | 产 sun / 减费；Run 经济引擎 |
| **5** | **过度** | 土豆雷、倭瓜 | 低费、一次性、换波次节奏 |
| **6** | **缓解压力 / 灰烬** | 樱桃炸弹、火爆辣椒 | 全屏 / 行 / 大范围清场 |
| **7** | **针对类** | 三叶草、叶子保护伞、墓碑吞噬 | 反制特定僵尸 / 场地机制 |

**目录类型**（与七类正交，不参与战斗定位统计）：

| 类型 | 含义 | 例子 |
|------|------|------|
| **消耗品** | `ConsumePlant` / 一次性道具牌 | H弹、蛋包饭、牛肉饭、甜甜圈、抹茶芭菲 |
| **地形** | `PotPlant` | 睡莲 |
| **升级卡** | `LevelUpPlant`（叠在基础 plant 上） | Oblivionis、主唱 Nina、吉他英雄、Mortis、宇宙高松灯 |
| **未完成** | `plantType=0` 或 Prefab 未绑全 | 千夏、田井中律、叶瞬光 等 |

**与 §旧四象限的关系**：四象限偏 **构筑**；七类偏 **关卡内职责**。可双标签，例如「秋山澪 = 4 产出阳光 + 3 辅助」。

### 4.5 职业 × 类型矩阵（乐器 × 战斗定位）

> **横轴（类型）**：§4 七类战斗定位 + 目录类型（消耗品 / 地形 / 未完成 / 待确认 / **待归类**）。  
> **纵轴（职业）**：`plantTags` 中的**乐器职业**（鼓手、贝斯、吉他、键盘、主唱），**不是**乐队名（Mygo、AveMujica 等）。无乐器标签的 plant 归入「无职业标签」行。  
> **全量表（2026-08-13 刷新）**：[plant-roster.md](./plant-roster.md)（独立 85 张；待归类 15 张）  
> **复现**：`python tools/build_plant_roster.py` → `docs/game-design/plant-roster.md`、`tools/profession_type_matrix.md`、`tools/plant-roster.csv`。

完整矩阵见 `tools/profession_type_matrix.md`（含「待归类」列）。归类时优先打开 [plant-roster.md](./plant-roster.md) 的「待归类」表。

**读表提示**：

- 同一 plant 只出现在一个类型列（与 §9.1 主定位一致）；双标签 plant 在 §9.1 备注中说明（如秋山澪 4+3）。
- 「键盘」行合并 `plantTags` 中的「键盘」与「键盘手」。
- 升级卡（Oblivionis、Mortis 等）按**基础 plant 名**计入矩阵，与 §9.1 一致。
- 乐队羁绊见 [band-instrument-bonds-reference.md](./band-instrument-bonds-reference.md) §1；本表仅统计乐器职业维度。

---

## 5. `price`（阳光费）与肉鸽档位

### 5.1 单局战斗

- `Property.price` = 种植时 **一次性 sun 消耗**（见 [单位制作.md](../单位制作.md)）
- 僵尸 `price ≈ 25` 为 **一波预算单位**（与 plant 费无关，但可 mental 对照强度）

### 5.2 肉鸽奖励 / 商店分档

`RoguelikeEconomyConfig.plantPriceBands`（默认参考尖塔体感）：

| 档位 | price 区间（默认） | 映射 | 三选一权重（Normal / Elite / Boss） |
|------|-------------------|------|-------------------------------------|
| **低档 Common** | ≤ 99 | `RoguelikePlantPriceTier.Low` | 60 / 45 / 20 |
| **中档 Uncommon** | 100–175 | `Mid` | 37 / 40 / 35 |
| **高档 Rare** | ≥ 176（含 200+） | `High` | 3 / 15 / 45 |

地图商店（5 格）档位权重：49 / 37 / 14；售价 = `baseProperty.price × 档位倍率`（低 1.0 / 中 1.15 / 高 1.35），最低 25。

**设计约定**：

| 档位 | 预期单 plant 强度 | 出现渠道 |
|------|-------------------|----------|
| 低 | 小机制 / 组件牌 | Normal 一般不送 PlantPick；商店可有 |
| 中 | 体系核心组件 | Elite 三选一 |
| 高 | 终结技 / 大机制 | Boss 三选一 |

**全量 plant 分档清单** → 见下方 **§5.3**。

### 5.3 全量 Plant price 分档清单（70）

> **数据源**：`Assets/Resources/ChessData/Player/*.asset` 的 `Property.baseProperty.price` + `RoguelikeEconomyConfig.plantPriceBands`（`lowMaxPrice=99`，`midMaxPrice=175`）。  
> **肉鸽池过滤**：默认 `excludeLevelUpPlants: true`（`plantIfCanBuyCard` 为 `LevelUp_Limit` 的升级卡不进三选一）；表中「不进池」以此为准。  
> **复现**：`python tools/list_plants.py` → `python tools/list_plants_by_price_tier.py` → `tools/plants_by_price_tier.md`。

**分布概览**（2026-07-14，`lowMax=99` / `midMax=175`）：

| 肉鸽档位 | 数量 | 占比 |
|----------|-----:|-----:|
| 低档 Common（≤99） | 45 | 64% |
| 中档 Uncommon（100–175） | 16 | 23% |
| 高档 Rare（>175，含 200+） | 9 | 13% |

**策划侧待关注**：

- 低档 plant 仍偏多，Common 三选一易重复；后续可上调部分 plant 的 `price`。
- **0 费** plant 有 7 张，与 25 费同属低档，肉鸽体感接近「白送档」。
- 150 / 175 已并入中档；高档从 200 起，跨度仍大（200–1000），强度梯度待数值复核。
- 重构体、魂灵之影等为技能衍生 plant，是否应进 Run 池待确认。

#### 低档 Common（price ≤ 99）— 45 张

| price | chessName | 种植类型 | 肉鸽池 |
|------:|-----------|----------|--------|
| 0 | Soldier | MainPlant | 可进池 |
| 0 | 丰川清告 | MainPlant | 可进池 |
| 0 | 八九寺真宵 | MainPlant | 可进池 |
| 0 | 橘福福 | NonePlant | 可进池 |
| 0 | 若叶睦Moritis | LevelUpPlant | 不进池（升级卡） |
| 0 | 重构体 | MainPlant | 可进池 |
| 0 | 魂灵之影 | MainPlant | 可进池 |
| 25 | 千早爱音 | MainPlant | 可进池 |
| 25 | 嘉然土豆雷 | MainPlant | 可进池 |
| 25 | 槌蛇波奇(后藤一里) | MainPlant | 可进池 |
| 25 | 甜甜圈 | ConsumePlant | 可进池 |
| 25 | 睡莲 | PotPlant | 可进池 |
| 25 | 蛋包饭 | ConsumePlant | 可进池 |
| 25 | 高松灯 | MainPlant | 可进池 |
| 50 | 486 | MainPlant | 可进池 |
| 50 | H弹 | ExclusivePlant | 可进池 |
| 50 | Tomo | MainPlant | 可进池 |
| 50 | 三角初华 | MainPlant | 可进池 |
| 50 | 丰川祥子(舞台) | MainPlant | 可进池 |
| 50 | 仪玄 | NonePlant | 可进池 |
| 50 | 凛御银灰 | — | 可进池 |
| 50 | 初音小推车 | MainPlant | 可进池 |
| 50 | 千夏 | — | 可进池 |
| 50 | 叶瞬光 | — | 可进池 |
| 50 | 后藤一里 | MainPlant | 可进池 |
| 50 | 小虹夏 | MainPlant | 可进池 |
| 50 | 希希芙 | — | 可进池 |
| 50 | 桃金娘 | MainPlant | 可进池 |
| 50 | 潘引壶 | NonePlant | 可进池 |
| 50 | 爱芮 | MainPlant | 可进池 |
| 50 | 猫猫 | NonePlant | 可进池 |
| 50 | 田井中律 | — | 可进池 |
| 50 | 秋山澪 | MainPlant | 可进池 |
| 50 | 纯田真奈 | MainPlant | 可进池 |
| 50 | 虹夏 | MainPlant | 可进池 |
| 50 | 迈巴赫 | MainPlant | 可进池 |
| 75 | rupa | MainPlant | 可进池 |
| 75 | 丰川祥子 | MainPlant | 可进池 |
| 75 | 南宫羽 | AimTargetPlant | 可进池 |
| 75 | 喜多照片 | MainPlant | 可进池 |
| 75 | 抹茶芭菲 | ConsumePlant | 可进池 |
| 75 | 灵感菇 | MainPlant | 可进池 |
| 75 | 牛肉饭 | ConsumePlant | 可进池 |
| 75 | 琴吹䌷 | MainPlant | 可进池 |
| 75 | 若叶睦 | MainPlant | 可进池 |

#### 中档 Uncommon（100–175）— 16 张

| price | chessName | 种植类型 | 肉鸽池 |
|------:|-----------|----------|--------|
| 100 | 八幡海玲Timoris | MainPlant | 可进池 |
| 100 | 广井菊里 | MainPlant | 可进池 |
| 100 | 薇薇安 | SupportPlant | 可进池 |
| 125 | 企鹅高松灯 | MainPlant | 可进池 |
| 125 | 南瓜罩 | SupportPlant | 可进池 |
| 125 | 史尔特尔 | MainPlant | 可进池 |
| 125 | 宇宙高松灯 | LevelUpPlant_Mygo | 可进池 / OnlyOne |
| 125 | 河源木桃香 | MainPlant | 可进池 |
| 125 | 苍角 | MainPlant | 可进池 |
| 150 | 伊地知星歌 | MainPlant | 可进池 / OnlyOne |
| 150 | 吃豆凉 | MainPlant | 可进池 |
| 150 | 喜多 | MainPlant | 可进池 |
| 150 | 祐天寺若麦 | MainPlant | 可进池 |
| 175 | 井芹仁菜(主唱) | LevelUpPlant | 不进池（升级卡） / OnlyOne |
| 175 | 平泽唯 | MainPlant | 可进池 |
| 175 | 要乐奈 | MainPlant | 可进池 |

#### 高档 Rare（price > 175，含 200+）— 9 张

| price | chessName | 种植类型 | 肉鸽池 |
|------:|-----------|----------|--------|
| 200 | 凉 | MainPlant | 可进池 |
| 200 | 椎名立希 | MainPlant | 可进池 |
| 225 | 井芹仁菜 | MainPlant | 可进池 |
| 225 | 长崎素世 | MainPlant | 可进池 |
| 275 | 圣聆初雪 | MainPlant | 可进池 |
| 475 | 丰川祥子Oblivionis | LevelUpPlant | 不进池（升级卡） / OnlyOne |
| 500 | M3 | MainPlant | 可进池 |
| 500 | 吉他英雄 | LevelUpPlant | 不进池（升级卡） / OnlyOne |
| 1000 | 维什戴尔 | MainPlant | 可进池 / OnlyOne |

#### 按精确 price 汇总

| price | 数量 | chessName |
|------:|-----:|-----------|
| 0 | 7 | Soldier、丰川清告、八九寺真宵、橘福福、若叶睦Moritis、重构体、魂灵之影 |
| 25 | 7 | 千早爱音、嘉然土豆雷、槌蛇波奇(后藤一里)、甜甜圈、睡莲、蛋包饭、高松灯 |
| 50 | 22 | 486、H弹、Tomo、三角初华、丰川祥子(舞台)、仪玄、凛御银灰、初音小推车、千夏、叶瞬光、后藤一里、小虹夏、希希芙、桃金娘、潘引壶、爱芮、猫猫、田井中律、秋山澪、纯田真奈、虹夏、迈巴赫 |
| 75 | 9 | rupa、丰川祥子、南宫羽、喜多照片、抹茶芭菲、灵感菇、牛肉饭、琴吹䌷、若叶睦 |
| 100 | 3 | 八幡海玲Timoris、广井菊里、薇薇安 |
| 125 | 6 | 企鹅高松灯、南瓜罩、史尔特尔、宇宙高松灯、河源木桃香、苍角 |
| 150 | 4 | 伊地知星歌、吃豆凉、喜多、祐天寺若麦 |
| 175 | 3 | 井芹仁菜(主唱)、平泽唯、要乐奈 |
| 200 | 2 | 凉、椎名立希 |
| 225 | 2 | 井芹仁菜、长崎素世 |
| 275 | 1 | 圣聆初雪 |
| 475 | 1 | 丰川祥子Oblivionis |
| 500 | 2 | M3、吉他英雄 |
| 1000 | 1 | 维什戴尔 |

---

## 6. `PlantType` 与空间设计

| PlantType | 含义 | 设计注意 |
|-----------|------|----------|
| MainPlant | 主格占 1 格 | 大多数输出 / 引擎 |
| SupportPlant | 叠在主 plant 上 | 常做功能牌；注意 **唯一** 与 `IfCanBuyCard` |
| PotPlant | 地形花盆 | 体系 enabler |
| Consume | 一次性 | 类似 0 费事件牌 |
| LimitType | 波次 / 数量限制 | 慎进肉鸽池或标 Rare |

**购买限制**（`IfCanBuyCard`）：`OnlyOne_Limit`、`LevelUp_Limit` 等 — 升级 plant **默认不进** 肉鸽三选一池（见 EconomyConfig `excludeLevelUpPlants`）。

---

## 7. 羁绊（体系的上层建筑）

- 词条库：[band-instrument-bonds-reference.md](./band-instrument-bonds-reference.md)
- 单 plant：`plantTags`、`fetterMemberId`
- **设计原则**：
  - 单角色：**1 主乐器 + 0~1 副标签**，避免组合爆炸
  - 羁绊提供 **百分比 / 触发器**，少做「只有凑满才生效」的 hard gate
  - Fever 类 = **主题层特殊资源**，与 sun **并行**，不互相替代

---

## 8. Prop（道具）设计原则

- 数据流：[prop-system.md](../requirements/prop-system.md)
- **适合 Prop**：+1 手牌槽、开局 +sun、CD 减免、某 tag 伤害 +X%、钉耙类 **一次性地形**
- **不适合 Prop**（应做 plant）：需要占格、有 CD 循环攻击、占 MainPlant 格子的主体

---

## 9. 设计记录区（持续更新）

### 9.1 全量 Plant 清单 — 策划定稿 v1 + 2026-08-13 刷新

统计范围：`Assets/Resources/ChessData/Player/`（独立植物 **85**，召唤/衍生物已剔除）。  
**七类**为战斗定位；**消耗品 / 地形**单独成栏，不计入七类计数。

**活表（归类用）**：[plant-roster.md](./plant-roster.md) · CSV：`tools/plant-roster.csv`

#### 待归类（15）— 2026-06-11 之后新增，七类未定

揭幕喵梦、天素罗（压力）、惊蛰、芭菲猫、压力希、立希汪、今井莉莎、大和麻弥、山吹沙绫、若宫伊芙、多首的怪物、灯（高松灯异形态）、凑友希那、粉色奶龙、维什戴尔

#### 按定位分组（v1 已定稿，未改）

**1 — 前排（7）**  
波奇、rupa、槌蛇波奇、南瓜罩、琴吹䌷、长崎素世、Mortis（若叶睦升级形态）

**2 — 输出（18）**  
井芹仁菜、主唱 Nina、Oblivionis、八幡海玲 Timoris、椎名立希、凉、圣聆初雪、要乐奈、祐天寺若麦、河源木桃香、吃豆凉、广井菊里、常服高松灯、丰川祥子(舞台)、八九寺真宵、吉他英雄、黄瓜睦（若叶睦）、平泽唯

**3 — 辅助（6）**  
星歌、三角初华、Tomo、M3、秋山澪（兼 **4** 产 sun）、宇宙高松灯（升级卡）

**4 — 产出阳光（6）**  
486、虹夏、小虹夏、桃金娘、纯田真奈、千早爱音

**5 — 过度（5）**  
丰川祥子、灵感菇、爱芮、嘉然土豆雷、橘福福

**6 — 缓解压力 / 灰烬（9）**  
喜多、喜多遗照、史尔特尔、苍角、企鹅高松灯、初音小推车、仪玄、迈巴赫、潘引壶

**7 — 针对类（2）**  
薇薇安（叶子保护伞）、南宫羽（吃墓碑）

**— 消耗品（5）**  
H弹、蛋包饭、牛肉饭、甜甜圈、抹茶芭菲

**— 地形（1）**  
睡莲

**— 未完成（6）**  
凛御银灰、希希芙、猫猫、千夏、田井中律、叶瞬光

**— 待确认（2）**  
Soldier、丰川清告

---

#### 全表（按名称排序）

| chessName | price | HP | ATK | 种植 | 关键技能 | 定位 |
|-----------|------:|---:|----:|------|----------|------|
| 486 | 50 | 300 | 0 | Main | CreateSunLight | **4** |
| H弹 | 50 | 500 | 0 | Consume | Item | 消耗品 |
| M3 | 500 | 300 | 80 | Main | Mon3tr 治疗 | **3** |
| Mortis | 0 | 4000 | 0 | LevelUp | — | **1**（升级） |
| Oblivionis | 475 | 300 | 40 | LevelUp | Oblivionis | **2**（升级） |
| rupa | 75 | 6000 | 20 | Main | Rupa | **1** |
| Saki / 丰川祥子(舞台) | 50 | 300 | 20 | Main | — | **2** |
| Soldier | 0 | 300 | 20 | Main | — | 待确认 |
| Tomo | 50 | 300 | 20 | Main | 群体治疗 | **3** |
| 爱芮 | 50 | 5000 | 180 | Main | KitaExplode | **5** |
| 八幡海玲 Timoris | 100 | 300 | 20 | Main | Timoris | **2** |
| 八九寺真宵 | 0 | 300 | 20 | Main | — | **2** |
| 波奇 | 50 | 4000 | 0 | Main | Passive_Boqi | **1** |
| 苍角 | 125 | 3000 | 180 | Main | IceJalapeno | **6** |
| 纯田真奈 | 50 | 300 | 50 | Main | CreateSunLight | **4** |
| 三角初华 | 50 | 300 | 60 | Main | Doloris 治疗 | **3** |
| 甜甜圈 | 25 | 300 | 0 | Consume | Item | 消耗品 |
| 薇薇安 | 100 | 300 | 20 | Support | LettuceUmbrella | **7** |
| 田井中律 | 50 | 300 | 0 | — | — | 未完成 |
| 琴吹䌷 | 75 | 1000 | 50 | Main | Tsumugi | **1** |
| 秋山澪 | 50 | 300 | 0 | Main | Sun + 溢出治疗 | **3**+**4** |
| 平泽唯 | 175 | 300 | 20 | Main | 治疗/输出模式 | **2** |
| 灵感菇 | 75 | 500 | 0 | Main | MushRoom | **5** |
| 黄瓜睦 | 75 | 5M | 15 | Main | TriggerRain | **2** |
| 丰川清告 | 0 | 300 | 0 | Main | — | 待确认 |
| 丰川祥子 | 75 | 5000 | 0 | Main | Saki 倭瓜 | **5** |
| 广井菊里 | 100 | 300 | 20 | Main | — | **2** |
| 星歌 | 150 | 300 | 300 | Main | Xingge | **3** |
| 嘉然土豆雷 | 25 | 300 | 1800 | Main | PotatoMine | **5** |
| 主唱 Nina | 175 | 300 | 180 | LevelUp | TriggerRain | **2**（升级） |
| 井芹仁菜 | 225 | 300 | 20 | Main | Nina + Taunt | **2** |
| 仪玄 | 50 | 300k | 1800 | None | — | **6** |
| 企鹅高松灯 | 125 | 8000 | 500 | Main | Bowling | **6** |
| 凉 | 200 | 300 | 180 | Main | Liang | **2** |
| 凛御银灰 | 50 | 300 | 0 | — | — | 未完成 |
| 初音小推车 | 50 | 50k | 18k | Main | — | **6** |
| 千夏 | 50 | 300 | 0 | — | — | 未完成 |
| 千早爱音 | 25 | 300 | 0 | Main | Mygo 被动 | **4** |
| 南宫羽 | 75 | 30k | 1800 | AimTarget | NGskill 墓碑 | **7** |
| 南瓜罩 | 125 | 4000 | 0 | Support | PumpkinShell | **1** |
| 史尔特尔 | 125 | 300 | 180 | Main | Jalapeno | **6** |
| 叶瞬光 | 50 | 500 | 0 | — | — | 未完成 |
| 吃豆凉 | 150 | 500 | 180 | Main | — | **2** |
| 吉他英雄 | 500 | 300 | 140 | LevelUp | GitaHero | **2**（升级） |
| 喜多 | 150 | 500k | 180 | Main | KitaExplode | **6** |
| 喜多遗照 | 75 | 500 | 90 | Main | KitaExplode | **6** |
| 圣聆初雪 | 275 | 600 | 80 | Main | Hatsuyuki | **2** |
| 宇宙高松灯 | 125 | 300 | 0 | LevelUp | BlackHole | **3**（升级） |
| 小虹夏 | 50 | 300 | 0 | Main | CreateSunLight | **4** |
| 希希芙 | 50 | 300 | 0 | — | — | 未完成 |
| 常服高松灯 | 25 | 300 | 7 | Main | ShootBullet | **2** |
| 抹茶芭菲 | 75 | 500 | 0 | Consume | Item | 消耗品 |
| 桃金娘 | 50 | 300 | 0 | Main | CreateSunLight | **4** |
| 椎名立希 | 200 | 300 | 40 | Main | TakiFear | **2** |
| 槌蛇波奇 | 25 | 2000 | 20 | Main | — | **1** |
| 橘福福 | 0 | 180k | 500 | None | Bowling | **5** |
| 河源木桃香 | 125 | 500 | 20 | Main | — | **2** |
| 潘引壶 | 50 | 30k | 180 | None | — | **6** |
| 牛肉饭 | 75 | 300 | 0 | Consume | Item | 消耗品 |
| 猫猫 | 50 | 300 | 10 | None | — | 未完成 |
| 睡莲 | 25 | 300 | 0 | Pot | — | 地形 |
| 祐天寺若麦 | 150 | 300 | 40 | Main | Nyamu | **2** |
| 虹夏 | 50 | 300 | 0 | Main | CreateSunLight | **4** |
| 蛋包饭 | 25 | 300 | 0 | Consume | Item | 消耗品 |
| 要乐奈 | 175 | 300 | 20 | Main | Rana 被动 | **2** |
| 迈巴赫 | 50 | 500k | 10k | Main | — | **6** |
| 长崎素世 | 225 | 2000 | 30 | Main | KitaExplode | **1** |

> 复现：`python tools/list_plants.py` → `tools/plants_dump.json`

### 9.2 数值 / 肉鸽分档表

肉鸽 **price → 档位** 及全量 plant 清单见 **§5.3**。本节预留「定位 × 档位 × 羁绊」交叉列，供后续数值策划填实：

| chessName | 定位（§4） | price | 肉鸽档位 | 羁绊 | 备注 |
|-----------|------------|------:|----------|------|------|
| （见 §5.3） | | | | | |

### 9.3 体系草案（TODO）

| 体系名 | 核心 4 阳光 | 核心 2 输出 | 关键 3 辅助 | 推荐 Prop |
|--------|-------------|-------------|-------------|-----------|

### 9.4 变更日志

| 日期 | 变更 |
|------|------|
| 2026-06-11 | 初版：资源三层、构筑二维、四象限 |
| 2026-06-11 | §4 改为七类战斗定位；§9.1 全量 67 plant 清单 |
| 2026-06-11 | 策划定稿 v1：灰烬/前排/对策等 15 项调整；消耗品单独成栏 |
| 2026-06-11 | §4.5 职业（乐器）× 类型（七类+目录）矩阵表；维护脚本 `tools/build_profession_type_matrix.py` |
| 2026-06-11 | §5.3 全量 plant 肉鸽 price 分档清单（70）；维护脚本 `tools/list_plants_by_price_tier.py` |
| 2026-07-14 | 肉鸽分档边界：低 ≤99 / 中 100–175 / 高 >175（含 200+）；`RoguelikeEconomyConfig` + 清单同步 |
| 2026-08-13 | 刷新独立植物表单：85 张（剔 7 张召唤/衍生）；15 张待归类；活表 `plant-roster.md` |

### 9.5 PropertyCreator 字段提案（未实现）

```csharp
// 草案 — 审批后再改 PropertyCreator.cs
public enum PlantRole {
    Frontline = 1,   // 前排
    Output = 2,      // 输出
    Support = 3,     // 辅助
    SunProducer = 4, // 产出阳光
    Transition = 5,  // 过度
    Ash = 6,         // 灰烬
    Counter = 7,     // 针对
}

/// <summary>目录类型：与 PlantRole 正交。消耗品不进七类战斗统计。</summary>
public enum PlantCatalogKind {
    Combat = 0,      // 普通 plant（填 plantRoles）
    Consumable = 1,  // H弹、蛋包饭等
    Terrain = 2,     // 睡莲等 PotPlant
}

[LabelText("目录类型")]
public PlantCatalogKind catalogKind;
[LabelText("战斗定位（可多选；仅 Combat）")]
[ShowIf("@catalogKind == PlantCatalogKind.Combat")]
public List<PlantRole> plantRoles;
[LabelText("升级卡（LevelUpPlant）")]
public bool isLevelUpCard;
```

与 `plantTags`（羁绊）、`PlantType`（占格）独立；肉鸽 `RollPlantPickOptions` 可按 `plantRoles` 做池权重（后续需求）。

---

## 10. 新 Plant / 新 Prop 设计检查清单

动手写需求或配表前，回答：

1. **动哪层资源？** Run 池 / sun / 槽位 / 格 / CD / 羁绊 / Prop 规则  
2. **战斗定位？** §4 七类，或 **消耗品 / 地形** 单列；可双标签（如秋山澪 4+3）  
3. **Run 持久还是本关临时？** 三选一进池 vs 关卡 `cards` 临时  
4. **price 档位？** 与 §5.2、EconomyConfig 是否一致  
5. **羁绊标签？** 是否触发已有 Fetter，还是新羁绊需单独立项  
6. **与现有 plant 是否重复？** 同象限同档位要有 **差异点**（距离 / 元素 / 触发条件）

通过清单后：

- **实现** → [单位制作.md](../单位制作.md)  
- **新机制 / Boss** → `@requirement-workflow` → `docs/requirements/[名].md`  
- **肉鸽发放规则** → [roguelike-economy-config.md](../requirements/roguelike-economy-config.md)

---

## 11. 关联文档

| 文档 | 关系 |
|------|------|
| [单位制作.md](../单位制作.md) | 实现流程 |
| [roguelike-economy-config.md](../requirements/roguelike-economy-config.md) | 金币 / 三选一 / 商店 |
| [roguelike-plant-pick-reward.md](../requirements/roguelike-plant-pick-reward.md) | PlantPick 实现需求 |
| [prop-system.md](../requirements/prop-system.md) | 道具（遗物） |
| [roguelike-rest-node.md](../requirements/roguelike-rest-node.md) | 休息 / 升级 / 删牌扩展 |
| [band-instrument-bonds-reference.md](./band-instrument-bonds-reference.md) | 羁绊词库 |
| [roguelike-progress.md](../roguelike-progress.md) | 肉鸽功能进度 |

**Context**：`context/modules/Chess.md`、`context/modules/Skill.md`
