# UI 模块

## 定位

局内/局外界面：**血条、伤害飘字、商店、编队、图鉴、开始菜单**等，通过 `UIManage` 或场景引用与 `GameManage`、`Chess`、`EventController` 联动。

## 核心类型（节选）

| 类型 | 职责 |
|------|------|
| `UIRoot` / `UIManage` | 面板根与打开关闭 |
| `DamagePanel` | 伤害数字 |
| `StartUI`、`PlantsPanel`、`Shop` 相关 | 流程与商店 |
| `DialoguePanel` | 对话演出 |

## 依赖

- 监听 **Event**；读取 **Chess** 数据展示；不反向持有长生命周期 Chess 引用 unless 必要。  

## 扩展

- 静态 UI 与频繁刷新 UI **分层 Canvas**（性能习惯，见 unity-ui-system Skill）。  
- 新面板：Prefab + 脚本；入口在 `UIManage` 或关卡流程中注册。
