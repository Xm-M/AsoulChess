This is a Unity game project.

Important folders:
Assets/Script/       - Core game scripts (~232 files)
Assets/Prefab/ChessPrefab/ - Chess prefabs
Assets/Scenes/       - Game scenes

Project rules and skills:
.codebuddy/skills/               - CodeBuddy skills
.codebuddy/skills/asoulchess-context/  - Project context knowledge base
.codebuddy/skills/requirement-workflow/ - Development workflow orchestrator
.cursor/rules/                   - Cursor IDE rules (legacy)
.cursor/skills/                  - Cursor IDE skills (legacy)
workflow_v2/context/             - Raw project context source

Key conventions:
- Script directory is `Assets/Script/` (singular, not Scripts)
- GameManage.instance is the global singleton
- UIManage is a static class (no instance, use UIManage.GetView<T>())
- EventController.Instance for event-driven communication
- New features should extend existing controllers, avoid new MonoBehaviours
