# 需求开发审批报告: 推车列铺冰 — 小推车冰块占位

## 基本信息
- **需求名称**: ICE-MOWER-COVER-001
- **所属模块**: Effect / Map
- **分析日期**: 2026-07-06

## 需求摘要
推车列铺冰且格上有小推车时，系统生成南瓜罩作为冰块视觉占位；冰融化时移除。

**需求类型**: 功能扩展  
**与现有功能的关系**: 基于 `ice-hatsuyuki-enter-damage` 推车列铺冰扩展视觉表现

## 分析结果汇总

### Context 复用
✅ 已读取 `context/index.md`  
- 涉及模块: Map, Effect, LevelSystem

### 现有业务分析
- **相关现有功能**: 推车列铺冰、小推车生成、南瓜罩 Support
- **需求类型判定**: 功能扩展 — 视觉占位，不改动承伤规则
- **集成点**: `PlaceOrRefreshIce` 成功后生成；`UnregisterIce` 时销毁
- **对现有功能的影响**: 仅推车列有冰+有推车时多一株系统植物

### 需求卡片
✅ `docs/requirements/ice-mower-column-cover-plant.md`

### Skills 白名单检查
| 技术点 | Skill | 状态 |
|--------|-------|------|
| 预制体/棋子实例化 | unity-prefab-system | ✅ |
| ScriptableObject 配置 | unity-scriptableobject-config | ✅ |
| 协程/计时（冰融化） | unity-coroutine-system | ✅（沿用现有） |

**结论**: 全部覆盖

### 架构分析
✅ `docs/architecture/ice-mower-column-cover-plant.md`  
- 设计: `Effect_Snow` 集中管理映射  
- 风险等级: 低

### 影响面分析
- **影响文件数**: 1（核心）+ 可选预制体 Inspector
- **高风险点**: 0
- **回归测试项**: 主网格铺冰、初雪进格伤害、推车列无推车时不生成

## 风险总评
| 维度 | 评级 | 说明 |
|------|------|------|
| 技术可行性 | 高 | 复用 CreateChess + PlantChess |
| 改动范围 | 小 | 单类扩展 |
| 性能风险 | 低 | 每行至多 1 株 |
| 时间评估 | 2–4h | 含 Unity 验证 |

## 建议的 Skills 使用清单
- @unity-prefab-system — 占位预制体
- @unity-scriptableobject-config — PropertyCreator 配置

## 开发优先级
- **P0**: 铺冰生成 / 融化回收 / 南瓜罩回退查找
- **P2**: 冰块美术替换、推车离场同步

## 决策审批
✅ **通过** — 已实现 `Effect_Snow` 核心逻辑（待 Unity 实机验收）

审批意见: 占位仅视觉，不扩展南瓜承伤到小推车  
日期: 2026-07-06
