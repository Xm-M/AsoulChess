# 4242 Explosions & Magic Collection — 特效资源目录

**来源**：Ansimuz（Gothicvania / Warped 系列）  
**整理日期**：2026-05-20  
**路径**：`docs/4242_Explosions & Magic Collection/4242_Explosions & Magic Collection/`

## 解压状态

共 **18 个 zip**，已全部解压（原先未解压的 14 个已于 2026-05-20 解压完成）。

| 状态 | 包 |
|------|-----|
| 此前已解压 | Pack 1、2、7、12、15 |
| 本次新解压 | Pack 3～6、8～11、13、14、16～18；Pack 9 的 `Mystery Track.zip` |

解压后约 **1384 PNG**、**138 GIF 预览**（含 macOS `__MACOSX` / `._` 元数据文件，导入 Unity 前可删）。

## 每个包的文件结构（通用）

多数包内含：

| 文件夹 | 内容 |
|--------|------|
| `sprites/` | 逐帧 PNG |
| `spritesheets/` | 整图序列（导入 Unity 后切 Sprite） |
| `aseprite/` 或 `Aseprite files/` | 源 Aseprite 工程 |
| `Previews/` 或根目录 GIF | 动效预览，**文件名即特效名** |

---

## 一、Gothicvania Magic Pack（魔法类，12 包）

### 1. Magic Pack N1
| 特效 | 说明 |
|------|------|
| air | 风/气 |
| ice | 冰 |
| thunder | 雷 |

### 2. Magic Pack N2 — Fire
| 特效 | 说明 |
|------|------|
| fire | 火焰 |
| fire_aura | 火焰光环 |
| flame | 火苗 |

### 3. Magic Pack N3
| 特效 | 说明 |
|------|------|
| big-bolt | 大闪电 |
| spark | 火花 |
| small-spark / small-spark-2 / small-spark-3 | 小火花变体 |
| thunder-rays | 雷射线 |

### 4. Magic Pack 4
| 特效 | 说明 |
|------|------|
| cure | 治疗 |
| Radial Explosion | 径向爆炸 |
| sparks | 火花 |
| water | 水 |
| wisp | 鬼火/灵火 |
| wisp-loop | 鬼火循环 |

### 5. Magic Pack 5
| 特效 | 说明 |
|------|------|
| fireball | 火球 |
| fire-missile | 火焰弹 |
| flash | 闪光 |
| impact-dust | 撞击尘土 |
| puff |  puff 烟 |
| smoke | 烟雾 |
| vertical-puff | 竖向 puff |
| water-splash | 水花 |

### 6. Magic Pack 6
| 特效 | 说明 |
|------|------|
| slash / slash-horizontal | 斩击（普通/横） |
| slash_b / Slash-e-slash | 斩击变体 |
| electric-slash / electric-slash-horizontal | 电斩 |
| FireSlash / fire-slash-horizontal | 火斩 |
| horizonta-fire-slashl | 横火斩（文件名拼写如此） |

附赠：`Magic Fx 6 Music/` 背景音乐。

### 7. Magic Pack 7
| 特效 | 说明 |
|------|------|
| vfx-a ~ vfx-e | 5 种通用魔法 VFX |

### 8. Magic Pack 8
| 特效 | 说明 |
|------|------|
| beam-slash | 光束斩 |
| burst | 爆发 |
| spark | 火花 |
| water | 水 |

### 10. Magic Pack 10
| 特效 | 说明 |
|------|------|
| blast | 爆炸冲击 |
| Lightning Bolt | 闪电束 |
| ray | 射线 |
| Raybolt | 雷箭 |
| sparks | 火花 |

### 11. Magic Pack 11
| 特效 | 说明 |
|------|------|
| ice_a / ice_b / ice_c | 三种冰系特效 |

### 12. Magic Pack 12
| 特效 | 说明 |
|------|------|
| Air-Slash | 风斩 |
| Dragon-Breath | 龙息 |
| Flames | 火焰 |
| ground-slash | 地斩 |
| Thunder-Ray | 雷射线 |
| Venom-Cloud / Small-Venom-Cloud | 毒云（大/小） |

---

## 二、Warped Explosion Pack（爆炸类，6 包）

各包均为像素爆炸序列，命名 `explosion-a` … 字母递增。

| 包 | 特效数量 | 特效列表 |
|----|----------|----------|
| **Explosion Pack 3** | 10 | explosion-a ~ explosion-j |
| **Explosion Pack 4** | 10 | explosion-a ~ explosion-j |
| **Explosion Pack 5** | 3 | Explosion-A / B / C（大尺寸） |
| **Explosions Pack 6** | 12 | explosion-a ~ explosion-l |
| **Explosions Pack 7** | 8 | explosion-a ~ explosion-h |
| **Explosions Pack 8** | 9 | explosion-a ~ explosion-i |

**Pack 5 附赠**：`Mystery Track/` — `mystery.mp3` / `.ogg` / `.wav`（BGM，非特效）。

---

## 三、Warped VFX Pack 1（受击 / 血液，1 包）

| 类别 | 特效 |
|------|------|
| 血液 | blood, blood-2, blood-3, blood-small |
| 受击 | hit-a ~ hit-l（12 种受击火花/碎屑） |

适合：普攻命中、暴击、僵尸受伤反馈。

---

## 四、按用途快速索引

| 用途 | 推荐包 / 特效 |
|------|----------------|
| 植物技能 — 火 | Pack 2 fire/flame；Pack 5 fireball；Pack 12 Flames/Dragon-Breath |
| 植物技能 — 冰/雷 | Pack 1 ice/thunder；Pack 3 闪电；Pack 11 ice_a/b/c；Pack 12 Thunder-Ray |
| 植物技能 — 水/毒 | Pack 4/5/8 water；Pack 12 Venom-Cloud |
| 植物技能 — 斩击 | Pack 6 各 slash；Pack 8 beam-slash；Pack 12 Air-Slash/ground-slash |
| 爆炸/AOE | Warped Explosion Pack 3～8（共 52 种爆炸变体） |
| 受击反馈 | Warped VFX Pack 1 hit-a~l |
| 治疗/增益 | Pack 4 cure |
| 通用魔法 | Pack 7 vfx-a~e |

---

## 五、导入 Unity 建议

1. 优先用 **`spritesheets/`** 整图 → Sprite Editor 切片，或 **`sprites/`** 逐帧做 Animation Clip。
2. 删除 `__MACOSX/` 和以 `._` 开头的 macOS 元数据文件。
3. 像素风建议 Filter Mode = Point，Compression = None。
4. 预览动效：各包 `Previews/*.gif` 或根目录 GIF，导入前可先肉眼看效果选型。

---

*本目录由资源文件夹自动扫描生成；特效名以 GIF 预览 / sprites 文件夹名为准。*
