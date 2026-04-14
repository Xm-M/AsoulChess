# Event 模块

## 定位

全局 **字符串事件** 总线：`EventController.Instance.AddListener` / `TriggerEvent`，解耦 Manage、Chess、UI、Buff 等。

## 核心类型

| 类型 | 职责 |
|------|------|
| `EventController` | 单例；订阅与派发 |
| `EventName` | 枚举，常用 `ToString()` 作键 |

## 使用注意

- 订阅与 **RemoveListener** 成对（尤其在 Chess 死亡或 UI OnDestroy）。  
- 键名统一走 `EventName`，避免手写字符串漂移。  

## 扩展

- 新全局事件：在 `EventName` 增加枚举值 + 文档此处一行说明触发点与载荷类型。
