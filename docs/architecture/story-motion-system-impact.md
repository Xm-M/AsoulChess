# 影响面分析: StoryMotion

## 改动范围
| 文件 | 类型 |
|------|------|
| `Assets/Script/Story/StoryMotionKind.cs` | 新增 |
| `Assets/Script/Story/StoryMotionPreset.cs` | 新增 |
| `Assets/Script/Story/StoryMotionPlayer.cs` | 新增 |
| `Assets/Script/Story/StoryMotionSignalBinding.cs` | 新增 |
| `Assets/Script/Story/StoryMotionBinder.cs` | 新增 |
| `Assets/Script/Story/StorySignalNames.cs` | 修改 |
| `Assets/Script/LevelSystem/.../EnterMapPlugin_StoryStage.cs` | 修改 |

## 回归建议
- [ ] 无 Binder 的剧情关：对话 + Complete 正常
- [ ] 有 Jump Binding：Signal 后跳跃到位再继续 Timeline
- [ ] 双人同 Signal：两人到齐再 Resume
- [ ] 中途离关 / OverPlugin：协程停止、stage 销毁无残留
