## Context

当前设计关卡编辑状态仅在内存中维护，应用中断后无法恢复。该变更需要覆盖多类中断场景（主动退出、应用暂停、异常关闭），并保证恢复过程不破坏原有关卡编辑主流程。约束包括：优先使用现有本地存储能力、不引入联网依赖、保持对后续数据结构演进的兼容能力。

## Goals / Non-Goals

**Goals:**
- 在设计关卡关键生命周期节点触发稳定的本地存档写入。
- 在重新进入设计关卡时发现可恢复档并执行恢复决策流程（继续/放弃）。
- 为存档格式增加版本和完整性校验，确保损坏/不兼容时可安全降级。
- 将存读档流程与关卡编辑逻辑解耦，便于后续扩展自动保存间隔、云同步等能力。

**Non-Goals:**
- 不实现跨设备同步与云端备份。
- 不覆盖普通关卡游玩模式的存档机制。
- 不在本次变更中提供复杂的多历史版本回溯，仅保留最近一次有效中断档。

## System Scope and Required Changes

本次设计关卡中断存读档会涉及以下系统，并明确新增职责：

1. `LevelManage`（关卡流程编排）
   - 新增职责：
     - 统一触发中断存档入口（关卡离开、重新开始、返回菜单前）。
     - 在进入关卡流程时触发恢复检测。
   - 接入点：
     - `LeaveState()`
     - `RestartLevel()`
     - `ReturnMenu()`
     - `ChangeLevel(LevelData levelData)` 前置恢复检查。

2. `SceneManage`（场景切换生命周期）
   - 新增职责：
     - 在 `LoadScene`/`AsyncLoading` 切换前后提供可挂接回调，确保“先存档后切场景”。
   - 接入点：
     - `AsyncLoading(... beforeLoad ...)` 的 `beforeLoad` 回调保持存档触发优先级。

3. `EventController` + `EventName`（事件总线）
   - 新增职责：
     - 增加中断存读档相关事件，解耦 UI、流程层和持久化层。
   - 建议新增事件：
     - `WhenInterruptionSaveRequested`
     - `WhenInterruptionSaveSucceeded`
     - `WhenInterruptionSaveFailed`
     - `WhenInterruptionRecoveryDetected`
     - `WhenInterruptionRecoveryApplied`
     - `WhenInterruptionRecoveryDiscarded`

4. `ParsePanel`（暂停面板）
   - 新增职责：
     - 在暂停/返回菜单/重开按钮路径中触发“请求存档”事件（而非直接写文件）。
     - 在检测到可恢复档时承载“继续/放弃”选择交互（可复用或新建恢复弹窗）。

5. `LevelData` / 当前关卡运行态（关卡核心数据）
   - 新增职责：
     - 定义可序列化的关卡编辑快照映射（只存必要字段，不直接存 Unity 对象引用）。
     - 提供 `ToSnapshot()` / `ApplySnapshot()` 的边界适配（可由单独 Mapper 实现）。

6. 新增 `LevelInterruptionSaveService`（建议新模块）
   - 核心职责：
     - 编排保存与恢复流程，不直接绑定 UI。
     - 提供 `RequestSave(reason)` / `TryLoadRecovery()` / `DiscardRecovery()`.
   - 依赖：
     - `InterruptionSnapshotRepository`（文件读写）
     - `InterruptionSnapshotValidator`（版本/完整性校验）
     - `LevelSnapshotMapper`（运行态 <-> 快照 DTO）

7. 新增 `InterruptionSnapshotRepository`（建议新模块）
   - 核心职责：
     - 文件路径管理、临时文件写入、原子替换、读取与删除。
   - 技术要求：
     - 统一存储位置（如 `Application.persistentDataPath` 子目录）。
     - 使用 `*.tmp` 写入后替换正式文件。

## Data Contract

中断档建议采用“头部元信息 + 业务快照”结构：

- `schemaVersion`: 存档结构版本，用于迁移和兼容。
- `savedAt`: 存档时间戳。
- `reason`: 触发原因（manual/leave-level/pause/quit）。
- `levelId` or `sceneName`: 关联关卡标识。
- `checksum`: 快照完整性校验值。
- `payload`: 关卡编辑状态（布局、对象状态、波次配置、关键计数器等可恢复最小集）。

兼容策略：
- 版本一致：直接恢复。
- 版本可迁移：走 `MigrationHandler` 转换。
- 版本不兼容：拒绝恢复并提示，允许继续空白流程。

## Decisions

1. 存档模型采用“快照 + 元信息”结构  
   - 选择：单文件快照（关卡数据）+ 元信息（schemaVersion、saveTime、checksum、sourceState）。  
   - 理由：可快速恢复，读取路径简单，且可通过版本字段支撑未来迁移。  
   - 备选：仅保存增量操作日志。未采用原因是恢复链路更复杂且更易受日志损坏影响。

2. 恢复入口统一在“进入设计关卡”流程前置检查  
   - 选择：在关卡编辑初始化前先检查可恢复档，给出“继续/放弃”决策。  
   - 理由：避免初始化后再回滚导致状态抖动，流程更可控。  
   - 备选：初始化后异步恢复。未采用原因是可能与现有编辑初始化产生竞态。

3. 写入策略采用“事件触发 + 原子替换”  
   - 选择：在手动保存、退出、应用暂停时触发写入，写入到临时文件后原子替换正式档。  
   - 理由：降低频繁 IO 成本，同时减少中断时文件半写入风险。  
   - 备选：固定间隔自动保存。未采用原因是需要额外调度与性能评估，放入后续扩展。

4. 异常处理采用“失败可见但不中断编辑”  
   - 选择：读档失败（损坏/版本不兼容）时记录日志并提示用户，自动回退到空白编辑态。  
   - 理由：保证核心编辑流程可用，避免因存档异常导致功能不可用。  
   - 备选：读档失败阻断进入。未采用原因是会将可恢复机制变成单点故障。

## Save/Load Flow (Sequence)

1. 保存流程（中断前）
   - 触发源：`ParsePanel` 操作、`LevelManage.LeaveState()`、应用暂停/退出钩子。
   - `LevelInterruptionSaveService.RequestSave(reason)` 被调用。
   - `LevelSnapshotMapper` 从当前关卡运行态提取 DTO。
   - `Validator` 生成校验信息，`Repository` 临时写入并原子替换。
   - 事件总线广播成功/失败事件，失败仅提示不阻断。

2. 恢复流程（进入关卡时）
   - 进入关卡前由 `LevelManage` 调用 `TryLoadRecovery()`。
   - `Repository` 读档，`Validator` 校验版本和完整性。
   - 若可恢复，UI 弹出“继续/放弃”。
   - 继续：`ApplySnapshot()` 恢复运行态并进入关卡。
   - 放弃：删除/失效化存档，按正常初始化进入关卡。

3. 失败降级路径
   - 任意读写失败均记录日志和指标。
   - 恢复失败自动回退到空白初始化，不阻断进入关卡。

## Risks / Trade-offs

- [频繁生命周期事件触发多次写入] → 通过写入去抖与最小间隔控制，避免无意义重复落盘。  
- [存档文件损坏导致恢复失败] → 增加 checksum 校验与临时文件原子替换，失败时自动降级。  
- [后续字段扩展破坏兼容] → 引入 schemaVersion 并预留迁移函数入口。  
- [恢复弹窗影响用户流程] → 仅在检测到有效中断档时提示，且提供默认安全选项。

## Migration Plan

1. 新增快照 DTO、Repository、Validator、Service 四层基础模块，先在隔离路径完成单元测试。  
2. 将保存触发点接入 `LevelManage`、`ParsePanel`、应用生命周期钩子。  
3. 在 `LevelManage` 入场路径接入恢复检测与恢复决策 UI。  
4. 增加事件、日志、埋点，统计触发次数、成功率、失败原因。  
5. 通过开关灰度启用恢复入口；异常率超阈值时仅关闭恢复，保留原主流程（回滚策略）。

## Open Questions

- 是否需要为“继续恢复”提供预览信息（如最后保存时间、关卡名）以提升可用性？  
- 是否需要在后续版本加入自动保存频率配置？  
- 当前项目中断事件来源是否统一，是否需要平台层补齐某些生命周期回调？
