# 功能需求卡片: GameStartPlugin_ZombieFast

## 基本信息
- **功能名称**: GameStartPlugin_ZombieFast
- **所属模块**: LevelSystem / Buff
- **需求类型**: 功能扩展
- **优先级**: P1
- **提出日期**: 2026-05-20

## 功能描述
关卡 `GameStartPlugin`：注册棋子入场事件，对**敌我全部**单位增加攻速 Buff（`Buff_BaseValueBuff_AttackSpeed` / `acceleRated`）。

## 集成点
- `EventName.WhenChessEnterWar` — 新入场单位
- `EventName.GameStart` — 读档恢复植物补 Buff
- `EventName.WhenLeaveLevel` / `OverPlugin` — 解除监听

## 配置
- `attackSpeedBuff`：可选 SerializeReference 自定义 Buff
- `attackSpeedBonus`：默认 +0.5 攻速（Buff 为空时使用）

## 验收标准
- [ ] 僵尸、植物入场后攻速提升
- [ ] 读档恢复植物也有 Buff
- [ ] 离开关卡后不再泄漏监听
