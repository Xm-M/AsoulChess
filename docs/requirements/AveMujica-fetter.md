# 功能需求卡片: AveMujica 羁绊（实现落地）

## 基本信息

- **功能名称**: AveMujica 乐队羁绊（Fever / Buff_AveMujica）
- **所属模块**: Fetter / Buff / State
- **需求类型**: 功能扩展
- **优先级**: P1
- **提出日期**: 2026-03-30

## 功能描述

羁绊激活后监听 `WhenPlantChess`：对种下且 `plantTags` 含 **`AveMujica`** 的棋子施加 **`Buff_AveMujica`**。全羁绊共享 **fever** 条（默认上限 **450**，仅通过各途径累加，**不会超过** feverMax）。**仅当任意 AveMujica 成员释放技能且此时 fever ≥ feverMax** 时 **清零** 并触发 Fever + 不屈；Fever **持续 20s** 期间 fever **不增长**；结束后 **15s** 内 fever **不增长**。八幡海玲在「用于判定本次是否满条」的技能上仍会先 **+10 fever**（再与封顶、触发判定）。

成员按 `chessName` 子串匹配上报 fever：祥子 `onTakeDamage`（自身造成伤害）；初华 `onHealDamage`（治疗对象为 AveMujica）；睦 `onSetDamage`（非 Heal 受伤）；海铃 `onUseSkill` +10；若麦 **每秒 +1**。

## 已实现代码

| 文件 | 说明 |
|------|------|
| `Assets/Script/Fetter/Fetter_AveMujica.cs` | `AveMujica : Fetter`，`Instance`、`TryAddFever`、`TriggerFever`、`OnPlantChess` |
| `Assets/Script/Buff/Buff_AveMujica.cs` | 五名成员分支与事件订阅 |
| `Assets/Script/State/State/FeverState.cs` | `Enter` 播放 `"fever"`；`Exit` 若 `preState` 为 `SkillState` 则 `ReturnCD()` |
| `Assets/Script/State/State/StateGraph.cs` | 枚举增加 **`StateName.FeverState`**（未改任何 .asset） |

## 策划 / 配置清单（由你本地完成）

1. **FetterDataList 资产**：`detectMode`、**Member** 或 **Tag**、`requiredMemberIds` / `tag`、`tierThresholds` 等按原规则配置。
2. **`AveMujica` 序列化字段**：拖入 **`buffTemplate`**（`Buff_AveMujica`）、**`unyieldingTemplate`**（`Buff_Unyielding`，建议预制里 `continueTime` 与 `feverPhaseDuration` 一致，默认 20）。
3. **每个 AveMujica 棋子 StateGraph**：自行添加 **`FeverState`** 节点与迁移（代码未改 `.asset`）。
4. **Animator**：各角色 Controller 需存在 **`fever`** 状态/剪辑，否则 `Play("fever")` 无表现或报警。
5. **角色名**：`Buff_AveMujica` 使用 `chessName.Contains` 匹配「丰川祥子」「三角初华」「若叶睦」「八幡海玲」「祐天寺若麦」；若卡面名称不一致需在 `Buff_AveMujica.cs` 中调整常量。

## 验收标准

- [ ] 羁绊亮后种植 AveMujica 成员会获得 `Buff_AveMujica`
- [ ] fever 满 450 后清零，全员尝试切入 `FeverState` 并获得 20s 不屈
- [ ] Fever 持续期与结束后 15s 内 fever 不增加
- [ ] 五名成员 fever 规则与事件一致
- [ ] 从 Fever 离开且进 Fever 前为技能态时 `ReturnCD` 被调用

## 关联 Context

- `context/modules/Buff.md`、`context/modules/Chess.md`（事件与 Buff 生命周期）
