# 内容配置缺口扫描（RG-CONTENT / RG-A01 / RG-C06 / RG-C07）

> 自动生成日期：2026-06-30  
> 脚本：`tools/audit_content_config.py`  
> 原始数据：`tools/content_config_gaps.json`

> **说明**：Unity 空字符串在 YAML 中为 `field:\n`（非 `field: ""`）。脚本已过滤「下一行 key 被误判为值」的情况。

## 摘要

| 类别 | 总数 | 三字段全空 | 描述+简介都缺 | 有部分文案 | 三字段齐全 |
| --- | --- | --- | --- | --- | --- |
| 植物 Player SO | 70 | 0 | 0 | 0 | 70 |
| 僵尸 Enemy SO | 54 | 0 | 0 | 0 | 54 |
| 羁绊 Fetter SO | 11 | 0 | - | - | 11 |

字段：`chessDescription`（描述）、`chessShortDescription`（简介）、`chessEffect`（效果）

### 植物其他缺口

- **无 plantTags**：27 张 — H弹, 丰川祥子(舞台), 仪玄, 企鹅高松灯, 八九寺真宵, 凛御银灰, 千夏, 南宫羽, 史尔特尔, 叶瞬光, 嘉然土豆雷, 希希芙, 抹茶芭菲, 伊地知星歌, 橘福福…
- **缺 chessSprite 图标**：7 张

## 植物 · 已有任意文案（70 张）

486, H弹, Mon3tr, 若叶睦Moritis, 丰川祥子Oblivionis, 丰川祥子(舞台), Soldier, Tomo, rupa, 三角初华, 丰川清告, 丰川祥子, 井芹仁菜(主唱), 井芹仁菜, 仪玄, 企鹅高松灯, 八九寺真宵, 八幡海玲Timoris, 凉, 凛御银灰…

## 植物 · 文案全空（0 张）

_无_

## 植物 · 有部分文案（缺描述/简介/效果之一）

_无_

## 僵尸 · 文案全空（0 张）

_无_

## 僵尸 · 有部分文案

| 名称 | 描述 | 简介 | 效果 |
| --- | --- | --- | --- |
| IceMan | Y | Y | Y |
| PRTS | Y | Y | Y |
| 丰川祥子C团 | Y | Y | Y |
| 僵王博士 | Y | Y | Y |
| 冰车僵尸 | Y | Y | Y |
| 包庇者 | Y | Y | Y |
| 博士感染者 | Y | Y | Y |
| 变异螃蟹 | Y | Y | Y |
| 墓碑 | Y | Y | Y |
| 墓碑普通僵尸 | Y | Y | Y |
| 墓碑路障僵尸 | Y | Y | Y |
| 墓碑铁桶僵尸 | Y | Y | Y |
| 大兔 | Y | Y | Y |
| 女士感染者 | Y | Y | Y |
| 小丑僵尸 | Y | Y | Y |
| 小鬼僵尸 | Y | Y | Y |
| 巨人僵尸 | Y | Y | Y |
| 巨型变异蟹 | Y | Y | Y |
| 投小丑盒僵尸 | Y | Y | Y |
| 拟态机械 | Y | Y | Y |
| 撑杆跳僵尸 | Y | Y | Y |
| 攀附者 | Y | Y | Y |
| 旁观者 | Y | Y | Y |
| 旗帜僵尸 | Y | Y | Y |
| 普通僵尸 | Y | Y | Y |
| 普通感染者 | Y | Y | Y |
| 普通雪橇僵尸 | Y | Y | Y |
| 木偶僵尸 | Y | Y | Y |
| 木偶园丁 | Y | Y | Y |
| 木偶女巫 | Y | Y | Y |
| 木偶骑士 | Y | Y | Y |
| 木桩 | Y | Y | Y |
| 椎名立希C团 | Y | Y | Y |
| 毒舌妇 | Y | Y | Y |
| 气球小丑僵尸 | Y | Y | Y |
| 海豚僵尸 | Y | Y | Y |
| 潘妮怀斯小丑僵尸 | Y | Y | Y |
| 潜水僵尸 | Y | Y | Y |
| 狂暴感染者 | Y | Y | Y |
| 矿工小丑僵尸 | Y | Y | Y |
| 红色大螃蟹 | Y | Y | Y |
| 胖子感染者 | Y | Y | Y |
| 自爆感染者 | Y | Y | Y |
| 若叶睦C团 | Y | Y | Y |
| 蜘蛛 | Y | Y | Y |
| 路障僵尸 | Y | Y | Y |
| 蹦极木偶 | Y | Y | Y |
| 铁桶僵尸 | Y | Y | Y |
| 长崎素世C团 | Y | Y | Y |
| 雪橇僵尸 | Y | Y | Y |
| 霸凌者 | Y | Y | Y |
| 飞鱼 | Y | Y | Y |
| 高松灯C团 | Y | Y | Y |
| 鸭子僵尸 | Y | Y | Y |

## 羁绊 · 缺 fetterEffectDescription

_全部已填_

## 羁绊 · 已填说明

| 名称 | 已填 |
| --- | --- |
| AveMujica | Y |
| Mygo | Y |
| sumimi | Y |
| 主唱 | Y |
| 吉他 | Y |
| 放学后茶会 | Y |
| 无刺有刺 | Y |
| 结束乐队 | Y |
| 贝斯 | Y |
| 键盘 | Y |
| 鼓手 | Y |

## 建议填法（Unity Inspector）

| 类型 | 路径 | 字段 |
|------|------|------|
| 植物 | `Assets/Resources/ChessData/Player/` | chessDescription / chessShortDescription / chessEffect |
| 僵尸 | `Assets/Resources/ChessData/Enemy/` | 同上 |
| 羁绊 | `Assets/SO/Fetter/` | fetterEffectDescription |

改 SO 即可，**无需改脚本**；`PlantCreatorDetailHelper` / `FetterIcon` 已读取上述字段。
