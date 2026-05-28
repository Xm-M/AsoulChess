# 功能需求卡片: GameStartPlugin_LittleZombie

## 基本信息
- **功能名称**: GameStartPlugin_LittleZombie + Buff_ChangeSize
- **所属模块**: LevelSystem / Buff
- **需求类型**: 功能扩展
- **提出日期**: 2026-05-20

## 功能描述
关卡开局插件：僵尸入场时施加 `Buff_ChangeSize`，将 `Transform.localScale` 设为配置值；Buff 结束时恢复原缩放。

## 配置
- `changeSizeBuff`：可选自定义 Buff
- `scale`：默认 `(0.7, 0.7, 0.7)`

## 验收标准
- [ ] 仅 Enemy 入场缩小
- [ ] 离场/ BuffOver 恢复原始 localScale
- [ ] 离开关卡解除事件监听
