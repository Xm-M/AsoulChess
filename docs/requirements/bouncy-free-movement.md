# 功能需求卡片: 自由反弹移动（非格子驱动）

## 基本信息
- **功能名称**: Chess 自由方向匀速移动 + 地图边界反弹（角度带随机扰动）
- **所属模块**: Chess / `MoveController` + `FindTileMethod`
- **需求类型**: 功能扩展（新增一种 `FindTileMethod` 策略，沿用现有 `MoveState` 驱动）
- **优先级**: P1（按实际棋子投放量调整）
- **预估复杂度**: L2
- **预估耗时**: 6–14 小时（含边界定义、`standTile` 策略联调、回归）
- **提出日期**: 2026-04-04

## 功能描述

### 详细描述
某类 Chess 在入场后获得**随机初始移动方向**（单位向量 × 移速），在**世界坐标**下沿该方向持续移动，**不依赖 `nextTile` 格子寻路**。当运动轨迹触及**地图可配置边界**时，按**镜面反射**或等价物理反弹更新速度方向；并在反弹时于**合理角度范围内**施加随机扰动，避免长期平行贴墙振荡。之后沿新方向继续运动，循环往复。

### 用户故事
作为关卡设计者，我希望部分单位以「弹球式」在场地内游荡，以便做出与僵尸横移不同的压迫感与不可预测路径。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 |
|------|------|------|
| `MoveController.WhenMoving` | `Assets/Script/Chess/MoveController/MoveController.cs` | 先调 `tileMethod.WhenMoving`，再走 `nextTile` 插值 |
| `FindTileMethod` 策略 | `FindTileMethord/*.cs` | **扩展点**：`HorMove` 格子、`RightMove`/`BowlingMove` 自管位移 |
| `MoveState` | `State/State/MoveState.cs` | `Enter`→`StartMoving`，`Execute`→`WhenMoving` |
| 地图范围 | `MapManage` / `MapManage_PVZ` | `mapSize`、`tileSize`、`IfInMapRange`、死亡格等 |

### 需求类型判定理由
在既有 **策略模式**（`FindTileMethod`）上增加一种实现即可与 `MoveController` 兼容；**不改** `MoveState` 主流程即可接入（与 `BowlingMove` 同类）。

### 集成点
- **实现**: 新建 `FindTileMethod` 子类（建议名：`ReflectiveFreeMove` / `BilliardBounceMove`），`FindNextTile` 返回 `null`，在 `WhenMoving` 内 `transform.position += velocity * dt`。
- **边界**: 使用地图四角世界坐标 + `tileSize` 构成 AABB，或与 `MapManage_PVZ` 中「出界即死」的列一致（需产品确认）。
- **反弹角度**: 反射后绕法线或绕速度方向旋转 `Random.Range(-θ, θ)`，并对 **接近平行墙** 的速度做下限夹紧，避免数值抖动。

### 对现有功能的影响（重要）
- **`standTile`**: 当前大量逻辑（Buff、Armor、索敌、`OnReachTile`）依赖格子。若本移动**与格子无关**，需二选一或组合：  
  1) **每帧/固定间隔**用世界坐标换算「最近格」写回 `standTile`（近似占用）；  
  2) **仅在反弹/进入新格时**更新；  
  3) 明确该棋子**不参与**依赖 `standTile` 的系统（影响面大）。  
  **必须在开发前选定**，否则易出隐性 Bug。

### 接口变更
- 无强制公共 API 变更；仅在 `PropertyCreator` 或 prefab 上挂新 `FindTileMethod` 序列化实例。

## 技术要求
- Unity 2D；与现有 `PropertyController.GetMoveSpeed()` 一致或单独系数。
- 不使用 `Rigidbody2D` 动力学反弹亦可（与项目「少物理」风格一致）；若用触发器需评估与现有 `Collider2D` 关系。

## 功能清单

### 核心功能（必须）
- [ ] 新 `FindTileMethod`：随机初速方向、匀速直线运动  
- [ ] 与地图边界相交检测 + 反弹 + 角度扰动范围可配置  
- [ ] `MoveController.ifMove` 与协程移动互斥行为保持正确  

### 扩展功能（可选）
- [ ] 与 `AnimatorController` 朝向联动（速度方向）  
- [ ] Debug Gizmo 画速度与边界  

## 验收标准
- [ ] 单位在边界内持续运动不反弹穿墙、无卡死在角上（或角上有明确脱困规则）  
- [ ] 与 `MoveState`、现有 `StartMoving`/`EndMoving` 生命周期无冲突  
- [ ] `standTile` 策略与策划约定一致并通过相关关卡 smoke test  

## 风险评估
| 风险 | 概率 | 影响 | 应对 |
|------|------|------|------|
| `standTile` 未更新导致技能/判定异常 | 中 | 高 | 事先定更新策略；必要时只对该棋子关闭部分依赖 |
| 边界与 `deathTile`/房间入口不一致 | 中 | 中 | 与 `MapManage_PVZ` 统一数据源 |
| 贴墙无限反弹 | 低 | 中 | 扰动角 + 最小法向分量 |

## 关联 Context
- `workflow_v2/context/index.md`
- `workflow_v2/context/modules/chess-data.md`（若存在 Chess 模块细表）
