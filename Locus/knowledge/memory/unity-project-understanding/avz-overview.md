---
id: kd_d4a79af1-b576-440f-adac-eaf27fbc567b
type: memory
path: unity-project-understanding/avz-overview.md
title: avz-overview
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
inheritAiConfig: true
createdAt: 1778029704618
updatedAt: 1778029704619
---

# avz-overview

## Summary
Structural cache for AVZ/AsoulChess Unity project: 2D PVZ-like strategy/autochess game with GameManage hub, Chess controller architecture, ISkill-based skill system, and key asset/code directories.

<!-- locus:body:start -->
# AVZ Unity Project Overview

- Unity 2022.3.15f1c1 2D project, URP, Legacy Input Manager.
- Project appears to be a PVZ-like strategy/autochess game: grid maps, plant/player chess units, zombie/enemy units, skills, buffs, levels, UI, shop, save, dialogue, and special modes.
- Main gameplay code is under `Assets/Script` (singular), organized by modules: `Manage`, `Chess`, `Skill`, `Buff`, `State`, `LevelSystem`, `Map`, `Weapon`, `bullet`, `UI`, `Fetter`, `Dialogue`, `SaveSystem`, `Snake`.
- Central runtime pattern uses `GameManage.instance` as the main hub; it coordinates `LevelManage`, chess/team managers, UI, timer, object pool, weather, and event systems.
- Unit architecture centers on `Chess` MonoBehaviour with controllers for properties, skills, state, buffs, movement, attacks, equipment, and animation.
- Skills are built around `ISkill`, `ISkillEffect`, `SkillController`, `SkillContext`, and state/animation events; project guidance says avoid adding runtime-only MonoBehaviours for skills under `Assets/Script/Skill`.
- Main scenes are in `Assets/Scenes`: `开始`, `前院`, `后院`, `学校`, `北白蛇神社`, `寂静村`, `Ring`, `锤僵尸`, `贪吃希希芙`.
- Unit prefabs are mainly under `Assets/Prefab/ChessPrefab`, with groups such as Asoul, AveMujica, Mygo, GBC, Zombie, 明日方舟, 绝区零, 生化幽灵, 结束乐队, etc.
- Project context documents exist under `context/`, especially `context/overview.md`, `context/index.md`, and `context/modules/*.md`; use these before deeper exploration.
- Current repository has many pre-existing uncommitted asset/meta/material/package/project setting changes; avoid overwriting or cleaning them without explicit user approval.
<!-- locus:body:end -->
