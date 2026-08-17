# -*- coding: utf-8 -*-
"""Build plant roster table for categorization (七类 / 软体系 / 乐器 / 乐队)."""
import csv
import os
import re

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
PLAYER = os.path.join(ROOT, "Assets", "Resources", "ChessData", "Player")
OUT_MD = os.path.join(ROOT, "docs", "game-design", "plant-roster.md")
OUT_CSV = os.path.join(ROOT, "docs", "game-design", "plant-roster.csv")
OUT_MATRIX = os.path.join(ROOT, "tools", "profession_type_matrix.md")

# 策划点名归类（2026-08-13）kind, group, note
# kind: 乐队 / 软体系 / 通用 / 剔除
CLASS_BY_FILE = {
    # —— 剔除：召唤 / 衍生 / 消耗 / 弃用 ——
    "重构体.asset": ("剔除", "衍生", "M3 召唤"),
    "魂灵之影.asset": ("剔除", "衍生", "维什戴尔召唤"),
    "多首幽灵.asset": ("剔除", "衍生", "多首技能召唤"),
    "石头.asset": ("剔除", "衍生", "灯额外部署"),
    "KFC僵尸.asset": ("剔除", "衍生", "小游戏占位"),
    "猫猫.asset": ("剔除", "衍生", "召唤/变身目标"),
    "丰川清告.asset": ("剔除", "衍生", "0 费占位"),
    "槌蛇波奇.asset": ("剔除", "衍生", "结束乐队衍生物"),
    "小虹夏.asset": ("剔除", "衍生", "结束乐队衍生物"),
    "吃豆凉.asset": ("剔除", "衍生", "结束乐队衍生物"),
    "喜多遗照.asset": ("剔除", "衍生", "结束乐队衍生物"),
    "仪玄.asset": ("剔除", "衍生", "衍生卡"),
    "潘引壶.asset": ("剔除", "衍生", "衍生卡"),
    "迈巴赫.asset": ("剔除", "衍生", "衍生卡"),
    "橘福福.asset": ("剔除", "衍生", "衍生卡"),
    "希希芙.asset": ("剔除", "衍生", "衍生卡"),
    "Soldier.asset": ("剔除", "衍生", "衍生卡"),
    "Saki.asset": ("剔除", "弃用", "舞台祥子，之后删除"),
    "H.asset": ("剔除", "消耗品", ""),
    "蛋包饭.asset": ("剔除", "消耗品", ""),
    "牛肉饭.asset": ("剔除", "消耗品", ""),
    "甜甜圈.asset": ("剔除", "消耗品", ""),
    "抹茶芭菲.asset": ("剔除", "消耗品", ""),
    # —— MyGO（不再新做；灯异形态算 MyGO）——
    "千早爱音.asset": ("乐队", "MyGO", ""),
    "常服高松灯.asset": ("乐队", "MyGO", ""),
    "天素罗.asset": ("乐队", "MyGO", "素世异形态"),
    "芭菲猫.asset": ("乐队", "MyGO", "控制"),
    "压力希.asset": ("乐队", "MyGO", "压力"),
    "立希汪.asset": ("乐队", "MyGO", "压力"),
    "宇宙高松灯.asset": ("乐队", "MyGO", ""),
    "灯.asset": ("乐队", "MyGO", ""),
    "粉色奶龙.asset": ("乐队", "MyGO", "针对"),
    "要乐奈.asset": ("乐队", "MyGO", ""),
    "椎名立希.asset": ("乐队", "MyGO", ""),
    "长崎素世.asset": ("乐队", "MyGO", ""),
    "企鹅高松灯.asset": ("乐队", "MyGO", "灯的动物形态"),
    "灵感菇.asset": ("乐队", "MyGO", "灯的其他形态"),
    # —— Ave Mujica（暂不新做；祥子/小推车算 Mujica）——
    "Mortis.asset": ("乐队", "Ave Mujica", ""),
    "揭幕喵梦.asset": ("乐队", "Ave Mujica", "一次性产阳"),
    "三角初华.asset": ("乐队", "Ave Mujica", "兼 sumimi"),
    "黄瓜睦.asset": ("乐队", "Ave Mujica", ""),
    "八幡海玲.asset": ("乐队", "Ave Mujica", ""),
    "多首的怪物.asset": ("乐队", "Ave Mujica", "压力"),
    "祐天寺若麦.asset": ("乐队", "Ave Mujica", ""),
    "Oblivionis.asset": ("乐队", "Ave Mujica", ""),
    "丰川祥子.asset": ("乐队", "Ave Mujica", "无 Mujica Fetter，仍算 Mujica"),
    "初音小推车.asset": ("乐队", "Ave Mujica", "初华的其他体系"),
    # —— 结束乐队（+星歌 +菊里；将新做 3 张）——
    "波奇.asset": ("乐队", "结束乐队", ""),
    "虹夏.asset": ("乐队", "结束乐队", ""),
    "喜多.asset": ("乐队", "结束乐队", ""),
    "凉.asset": ("乐队", "结束乐队", ""),
    "吉他英雄.asset": ("乐队", "结束乐队", ""),
    "星歌.asset": ("乐队", "结束乐队", "补算进结束乐队"),
    "广井菊里.asset": ("乐队", "结束乐队", "补算进结束乐队"),
    # —— 无刺有刺（可能 +1～2，配合压力）——
    "486.asset": ("乐队", "无刺有刺", "压力"),
    "Tomo.asset": ("乐队", "无刺有刺", "压力"),
    "rupa.asset": ("乐队", "无刺有刺", "压力"),
    "河源木桃香.asset": ("乐队", "无刺有刺", ""),
    "主唱Nina.asset": ("乐队", "无刺有刺", "压力"),
    "井芹仁菜.asset": ("乐队", "无刺有刺", "压力"),
    # —— 放学后茶会（治疗；差律/梓/梓升级/佐和子）——
    "秋山澪.asset": ("乐队", "放学后茶会", "治疗"),
    "琴吹䌷.asset": ("乐队", "放学后茶会", "治疗"),
    "平泽唯.asset": ("乐队", "放学后茶会", "治疗"),
    "田井中律.asset": ("乐队", "放学后茶会", "治疗 · 半成品"),
    # —— sumimi（未改）——
    "纯田真奈.asset": ("乐队", "sumimi", ""),
    # —— 软体系 ——
    "今井莉莎.asset": ("软体系", "棒球", ""),
    "山吹沙绫.asset": ("软体系", "棒球", ""),
    "凑友希那.asset": ("软体系", "压力", ""),
    "老仓育.asset": ("软体系", "压力", "半成品"),
    "羽川翼.asset": ("软体系", "压力", "半成品"),
    "战场原黑仪.asset": ("软体系", "压力", "半成品"),
    "章鱼噼.asset": ("软体系", "压力", "消耗种植，但算压力体系"),
    "薇薇安.asset": ("软体系", "撑伞", ""),
    "惊蜇.asset": ("软体系", "飞天", ""),
    "叶瞬光.asset": ("软体系", "飞天", "半成品"),
    "可露希尔.asset": ("软体系", "召唤物", "半成品"),
    "M3.asset": ("软体系", "召唤物", ""),
    "爱芮.asset": ("软体系", "妄想天使", ""),
    "南宫羽.asset": ("软体系", "妄想天使", ""),
    "千夏.asset": ("软体系", "妄想天使", "未完成"),
    # —— 通用不归类 ——
    "嘉然土豆雷.asset": ("通用", "通用", ""),
    "南瓜罩.asset": ("通用", "通用", ""),
    "八九寺真宵.asset": ("通用", "通用", ""),
    "桃金娘.asset": ("通用", "通用", ""),
    "史尔特尔.asset": ("通用", "通用", ""),
    "苍角.asset": ("通用", "通用", ""),
    "凛御银灰.asset": ("通用", "通用", "半成品"),
    "大和麻弥.asset": ("通用", "通用", ""),
    "若宫伊芙.asset": ("通用", "通用", ""),
    "维什戴尔.asset": ("通用", "通用", ""),
    "圣聆初雪.asset": ("通用", "通用", ""),
    "睡莲.asset": ("通用", "通用", "地形"),
}

# 尚未点名：生成时若缺 CLASS 则进「未点名」
PLANNED_NEW = [
    ("结束乐队", 3, 3, "尚未点名，3 张新卡"),
    ("无刺有刺", 1, 2, "尚未点名，配合压力流"),
    ("放学后茶会", 3, 3, "中野梓、中野梓升级、山中佐和子"),
    ("棒球", 3, 3, "尚未点名，莉莎/沙绫之外 +3"),
    ("撑伞", 3, 3, "柚也、嘉辛塔、莴苣"),
    ("飞天", 3, 3, "凯尔希、思衡托、安洁莉娜"),
    ("召唤物", 3, 3, "尚未点名，可露希尔/M3 之外 +3"),
]

NAMED_NEW = [
    ("放学后茶会", "中野梓"),
    ("放学后茶会", "中野梓升级"),
    ("放学后茶会", "山中佐和子"),
    ("撑伞", "柚也"),
    ("撑伞", "嘉辛塔"),
    ("撑伞", "莴苣"),
    ("飞天", "凯尔希"),
    ("飞天", "思衡托"),
    ("飞天", "安洁莉娜"),
]

BAND_ORDER = ["MyGO", "Ave Mujica", "结束乐队", "无刺有刺", "放学后茶会", "sumimi"]
SYSTEM_ORDER = ["棒球", "压力", "撑伞", "飞天", "召唤物", "妄想天使"]

# §9.1 策划定稿 v1 — 用文件名对齐（比 chessName 稳）
DISPLAY_NAME = {
    "天素罗.asset": "天素罗",
}

ROLE_BY_FILE = {
    "波奇.asset": "1 前排",
    "rupa.asset": "1 前排",
    "槌蛇波奇.asset": "1 前排",
    "南瓜罩.asset": "1 前排",
    "琴吹䌷.asset": "1 前排",
    "长崎素世.asset": "1 前排",
    "Mortis.asset": "1 前排",
    "天素罗.asset": "1 前排",
    "井芹仁菜.asset": "2 输出",
    "主唱Nina.asset": "2 输出",
    "Oblivionis.asset": "2 输出",
    "八幡海玲.asset": "2 输出",
    "椎名立希.asset": "2 输出",
    "凉.asset": "2 输出",
    "圣聆初雪.asset": "2 输出",
    "要乐奈.asset": "2 输出",
    "祐天寺若麦.asset": "2 输出",
    "河源木桃香.asset": "2 输出",
    "吃豆凉.asset": "2 输出",
    "广井菊里.asset": "2 输出",
    "常服高松灯.asset": "2 输出",
    "Saki.asset": "2 输出",
    "八九寺真宵.asset": "2 输出",
    "吉他英雄.asset": "2 输出",
    "黄瓜睦.asset": "2 输出",
    "平泽唯.asset": "2 输出",
    "压力希.asset": "2 输出",
    "灯.asset": "2 输出",
    "立希汪.asset": "2 输出",
    "今井莉莎.asset": "2 输出",
    "山吹沙绫.asset": "2 输出",
    "凑友希那.asset": "2 输出",
    "惊蜇.asset": "2 输出",
    "大和麻弥.asset": "2 输出",
    "若宫伊芙.asset": "2 输出",
    "维什戴尔.asset": "2 输出",
    "多首的怪物.asset": "2 输出",
    "星歌.asset": "3 辅助",
    "三角初华.asset": "3 辅助",
    "Tomo.asset": "3 辅助",
    "M3.asset": "3 辅助",
    "秋山澪.asset": "3 辅助",
    "宇宙高松灯.asset": "3 辅助",
    "芭菲猫.asset": "3 辅助",
    "486.asset": "4 产阳",
    "虹夏.asset": "4 产阳",
    "小虹夏.asset": "4 产阳",
    "桃金娘.asset": "4 产阳",
    "纯田真奈.asset": "4 产阳",
    "千早爱音.asset": "4 产阳",
    "揭幕喵梦.asset": "4 产阳",
    "丰川祥子.asset": "5 过度",
    "灵感菇.asset": "5 过度",
    "爱芮.asset": "5 过度",
    "嘉然土豆雷.asset": "5 过度",
    "橘福福.asset": "5 过度",
    "喜多.asset": "6 灰烬",
    "喜多遗照.asset": "6 灰烬",
    "史尔特尔.asset": "6 灰烬",
    "苍角.asset": "6 灰烬",
    "企鹅高松灯.asset": "6 灰烬",
    "初音小推车.asset": "6 灰烬",
    "仪玄.asset": "6 灰烬",
    "迈巴赫.asset": "6 灰烬",
    "潘引壶.asset": "6 灰烬",
    "薇薇安.asset": "7 针对",
    "南宫羽.asset": "7 针对",
    "粉色奶龙.asset": "7 针对",
    "H.asset": "消耗品",
    "蛋包饭.asset": "消耗品",
    "牛肉饭.asset": "消耗品",
    "甜甜圈.asset": "消耗品",
    "抹茶芭菲.asset": "消耗品",
    "睡莲.asset": "地形",
    "凛御银灰.asset": "未完成",
    "希希芙.asset": "未完成",
    "千夏.asset": "未完成",
    "田井中律.asset": "未完成",
    "叶瞬光.asset": "未完成",
    "Soldier.asset": "待确认",
}

# 软体系（羁绊外，靠 Buff / 技能互听）
SOFT_BY_FILE = {
    "压力希.asset": "压力",
    "天素罗.asset": "压力",
    "老仓育.asset": "压力",
    "凑友希那.asset": "压力",
    "立希汪.asset": "压力",
    "486.asset": "压力（GBC）",
    "Tomo.asset": "压力（GBC）",
    "rupa.asset": "压力（GBC）",
    "井芹仁菜.asset": "压力（GBC）",
    "主唱Nina.asset": "压力（GBC）",
    "多首的怪物.asset": "压力",
}

PROFESSION_TAGS = ["鼓手", "贝斯", "吉他", "键盘", "键盘手", "主唱"]
BAND_TAGS = [
    "Mygo", "AveMujica", "结束乐队", "放学后茶会", "无刺有刺", "sumimi",
    "Roselia", "Poppin'Party", "Pastel*Palettes", "明日方舟",
]
PLANT_TYPE_NAMES = {
    0: "未完成",
    1: "主力",
    2: "辅助占格",
    4: "地形",
    8: "消耗品",
}


def u(s: str) -> str:
    if not s:
        return ""
    s = s.strip().strip('"')
    if "\\u" in s:
        try:
            return s.encode("utf-8").decode("unicode_escape")
        except Exception:
            return s
    return s


def parse_asset(path: str, fn: str) -> dict:
    with open(path, encoding="utf-8") as f:
        c = f.read()

    def m(pat, default=""):
        r = re.search(pat, c, re.M)
        return r.group(1) if r else default

    raw_name = u(m(r'^  chessName:\s*(.*)$'))
    m_name = u(m(r'^  m_Name:\s*(.+)$'))
    if not raw_name or raw_name.startswith("chess") or ":" in raw_name:
        name = m_name or fn.replace(".asset", "")
    else:
        name = raw_name
    name = DISPLAY_NAME.get(fn, name)
    price = int(m(r"^\s+price:\s*(-?\d+)", "0") or 0)
    hp = int(m(r"^\s+HpMax:\s*(-?\d+)", m(r"^\s+Hp:\s*(-?\d+)", "0")) or 0)
    atk = int(m(r"^\s+attack:\s*(-?\d+)", "0") or 0)
    pt = int(m(r"^\s+plantType:\s*(\d+)", "0") or 0)
    pre = m(r"^  chessPre:\s*\{fileID:\s*(-?\d+)", "0")
    has_pre = pre not in ("", "0")
    fetter = u(m(r'^  fetterMemberId:\s*(.*)$'))
    if not fetter or fetter.startswith("chess"):
        fetter = ""
    tag_block = re.search(r"plantTags:(.*?)(?:\n  fetterMemberId:|\n  chessTileType:)", c, re.S)
    tags = []
    if tag_block:
        tags = [u(x) for x in re.findall(r'-\s+"?([^"\n]+)"?', tag_block.group(1))]
        tags = [t for t in tags if t and t != "[]"]
    func = m(r"type:\s*\{class:\s*(\w+)")
    return {
        "file": fn,
        "name": name,
        "price": price,
        "hp": hp,
        "atk": atk,
        "plantType": pt,
        "has_pre": has_pre,
        "fetter": fetter,
        "tags": tags,
        "func": func,
    }


def profession(tags):
    for p in PROFESSION_TAGS:
        if p in tags:
            return "键盘" if p == "键盘手" else p
    return "—"


def mature_bands_of(tags):
    seen = []
    for t in tags:
        b = TAG_TO_MATURE_BAND.get(t)
        if b and b not in seen:
            seen.append(b)
    return seen


def band(tags):
    mature = mature_bands_of(tags)
    extra = [
        t for t in tags
        if t not in PROFESSION_TAGS and t not in TAG_TO_MATURE_BAND and t
    ]
    parts = mature + extra
    return " / ".join(parts) if parts else "—"


TAG_TO_MATURE_BAND = {
    "Mygo": "MyGO",
    "AveMujica": "Ave Mujica",
    "结束乐队": "结束乐队",
    "无刺有刺": "无刺有刺",
    "放学后茶会": "放学后茶会",
    "sumimi": "sumimi",
}


def class_of(p):
    return CLASS_BY_FILE.get(p["file"], ("未点名", "未点名", "策划未点名"))


def is_excluded(p: dict) -> bool:
    return class_of(p)[0] == "剔除"


def exclude_reason(p: dict) -> str:
    kind, group, note = class_of(p)
    return f"{group}" + (f" · {note}" if note else "")


def group_of(p: dict) -> str:
    kind, group, _ = class_of(p)
    if kind == "剔除":
        return "剔除"
    return group


def status_of(p: dict) -> str:
    if is_excluded(p):
        return "剔除"
    if p["plantType"] == 0 or not p["has_pre"]:
        return "半成品"
    return "可种"


def plant_type_label(pt: int) -> str:
    if pt == 0:
        return "未完成"
    parts = [n for bit, n in PLANT_TYPE_NAMES.items() if bit and (pt & bit)]
    return "+".join(parts) if parts else str(pt)


def role_of(p: dict) -> str:
    fn = p["file"]
    if fn in ROLE_BY_FILE:
        return ROLE_BY_FILE[fn]
    if p["plantType"] & 8:
        return "消耗品"
    if p["plantType"] & 4:
        return "地形"
    if p["plantType"] == 0:
        return "未完成"
    return "待归类"


def load_all():
    rows = []
    for fn in sorted(os.listdir(PLAYER)):
        if not fn.endswith(".asset"):
            continue
        rows.append(parse_asset(os.path.join(PLAYER, fn), fn))
    return rows


ROLE_ORDER = [
    "1 前排", "2 输出", "3 辅助", "4 产阳", "5 过度", "6 灰烬", "7 针对",
    "消耗品", "地形", "未完成", "待确认", "待归类",
]


def md_cell(s: str) -> str:
    return (s or "—").replace("|", "/")


def write_md(all_rows):
    kept = [p for p in all_rows if not is_excluded(p)]
    excluded = [p for p in all_rows if is_excluded(p)]
    unnamed = [p for p in all_rows if class_of(p)[0] == "未点名"]
    wip = [p for p in kept if status_of(p) == "半成品"]
    playable = [p for p in kept if status_of(p) == "可种"]

    def in_kind(kind):
        return [p for p in kept if class_of(p)[0] == kind]

    bands = in_kind("乐队")
    systems = in_kind("软体系")
    generic = in_kind("通用")
    plan_lo = sum(x[1] for x in PLANNED_NEW)
    plan_hi = sum(x[2] for x in PLANNED_NEW)

    def rows_for(group_name):
        return [p for p in kept if class_of(p)[1] == group_name]

    def table(plants):
        out = [
            "| 名称 | 阳光 | 乐器 | 七类 | 备注 | 状态 |",
            "|------|------|------|------|------|------|",
        ]
        for p in sorted(plants, key=lambda x: (x["price"], x["name"])):
            note = class_of(p)[2] or "—"
            out.append(
                f"| {md_cell(p['name'] or p['file'])} | {p['price']} | "
                f"{profession(p['tags'])} | {role_of(p)} | {md_cell(note)} | {status_of(p)} |"
            )
        return out

    def role_names(role):
        return [p for p in kept if role_of(p) == role]

    lines = [
        "# 植物表单（独立植物归类）",
        "",
        "> **更新日期**：2026-08-13（按策划点名重分）  ",
        "> **口径**：100 张 = 独立可种植物。剔除召唤/衍生物、食物消耗品、弃用卡。章鱼噼算压力体系。  ",
        "> MyGO / Mujica **不再新做**。复现：`python tools/build_plant_roster.py`",
        "",
        "## 数量",
        "",
        "| 口径 | 张数 |",
        "|------|------|",
        f"| Player SO 总数 | {len(all_rows)} |",
        f"| 剔除 | {len(excluded)} |",
        f"| **计入 100** | **{len(kept)}** |",
        f"| 乐队 | {len(bands)} |",
        f"| 软体系 | {len(systems)} |",
        f"| 通用不归类 | {len(generic)} |",
    ]
    if unnamed:
        lines.append(f"| 未点名 | {len(unnamed)} |")
    lines += [
        f"| 可种 / 半成品 | {len(playable)} / {len(wip)} |",
        f"| 已规划新做 | {plan_lo}～{plan_hi} |",
        f"| 到 100 还需新做 | **{max(0, 100 - len(kept))}** |",
        f"| 规划新做之后还差 | {max(0, 100 - len(kept) - plan_lo)}～{max(0, 100 - len(kept) - plan_hi)}（倾向做成通用） |",
        f"| **工作件数** | **{max(0, 100 - len(kept)) + len(wip)}**（新做 {max(0, 100 - len(kept))} + 做完半成品 {len(wip)}） |",
        "",
        "## 七类定位（计入 69）",
        "",
        "战斗定位按策划七类；消耗品/地形/未完成/待归类单独计。下面只统计**计入 100** 的卡。",
        "",
        "| 定位 | 张数 | 名单 |",
        "|------|------|------|",
    ]
    for role in ROLE_ORDER:
        plants = role_names(role)
        if not plants:
            continue
        names = "、".join(sorted(p["name"] or p["file"] for p in plants))
        lines.append(f"| {role} | {len(plants)} | {names} |")
    seven = sum(len(role_names(r)) for r in ROLE_ORDER[:7])
    lines += [
        f"| **七类合计** | **{seven}** | 不含地形/未完成/待归类 |",
        "",
        "## A. 乐队",
        "",
        "MyGO、Mujica 不再追加新植物。结束乐队将 +3；无刺有刺可能 +1～2；放学后茶会补齐治疗线。",
        "",
    ]
    for b in BAND_ORDER:
        plants = rows_for(b)
        extra = ""
        if b == "MyGO":
            extra = "不再新做。"
        elif b == "Ave Mujica":
            extra = "暂不新做。祥子（倭瓜）与初华小推车算 Mujica。"
        elif b == "结束乐队":
            extra = "将新做 3 张。星歌、广井菊里已算入。"
        elif b == "无刺有刺":
            extra = "可能再 +1～2，配合压力流。"
        elif b == "放学后茶会":
            extra = "治疗体系。还差田井中律（半成品）、中野梓、梓升级、山中佐和子。"
        lines.append(f"### {b}（{len(plants)}）")
        lines.append("")
        if extra:
            lines.append(extra)
            lines.append("")
        lines += table(plants)
        lines.append("")

    lines += [
        "## B. 软体系（羁绊外）",
        "",
    ]
    for s in SYSTEM_ORDER:
        plants = rows_for(s)
        extra = {
            "棒球": "莉莎、沙绫已有；再 +3。",
            "压力": "凑友希那 / 老仓育 / 羽川翼 / 黑仪 / 章鱼噼。乐队内压力卡仍算各自乐队。",
            "撑伞": "薇薇安已有；再加柚也、嘉辛塔、莴苣。",
            "飞天": "惊蛰、叶瞬光已有；再加凯尔希、思衡托、安洁莉娜。",
            "召唤物": "可露希尔、M3 已有；再 +3。",
            "妄想天使": "爱芮、南宫羽、千夏（未完成）。本体系只这三张。",
        }.get(s, "")
        lines.append(f"### {s}（{len(plants)}）")
        lines.append("")
        if extra:
            lines.append(extra)
            lines.append("")
        lines += table(plants)
        lines.append("")

    lines += [
        "## C. 通用（不归类）",
        "",
        "不当作乐队或软体系核心。",
        "",
    ]
    lines += table(generic)
    lines.append("")

    if unnamed:
        lines += ["## 未点名", "", "下面这些这次没点到，暂未计入任何体系。", ""]
        lines += table(unnamed)
        lines.append("")

    lines += [
        "## D. 计划新做（尚无 SO）",
        "",
        "| 体系 | 新做 | 内容 |",
        "|------|------|------|",
    ]
    for name, lo, hi, desc in PLANNED_NEW:
        n = str(lo) if lo == hi else f"{lo}～{hi}"
        lines.append(f"| {name} | {n} | {desc} |")
    lines += [
        f"| **合计** | **{plan_lo}～{plan_hi}** | MyGO / Mujica / 妄想天使 / 通用不再加 |",
        "",
        "### 已点名、还没 SO 的新卡",
        "",
        "| 体系 | 名称 |",
        "|------|------|",
    ]
    for sys, name in NAMED_NEW:
        lines.append(f"| {sys} | {name} |")
    unnamed_slots = plan_hi - len(NAMED_NEW)
    lines += [
        "",
        f"### 只定数、还没点名的新卡槽（{unnamed_slots} 张量级）",
        "",
        "- 结束乐队：**3** 张（未点名）",
        "- 棒球：**3** 张（未点名）",
        "- 召唤物：**3** 张（未点名）",
        "- 无刺有刺：**1～2** 张（未点名）",
        "",
        "### 已有 SO、还要做完的半成品（已计入 69，不另占新卡名额）",
        "",
        "- 放学后茶会：田井中律",
        "- 压力：老仓育、羽川翼、战场原黑仪",
        "- 飞天：叶瞬光",
        "- 召唤物：可露希尔",
        "- 妄想天使：千夏",
        "- 通用：凛御银灰",
        "",
        f"到 100 还缺 **{max(0, 100 - len(kept))}** 张新 SO。规划 {plan_lo}～{plan_hi} 张之后，还剩 **{max(0, 100 - len(kept) - plan_lo)}～{max(0, 100 - len(kept) - plan_hi)}** 张没有归属，**倾向做成通用**，定位后补。",
        "",
        f"工作件数 = 新做 {max(0, 100 - len(kept))} + 做完半成品 {len(wip)} = **{max(0, 100 - len(kept)) + len(wip)}**。",
        "",
        "## E. 已剔除（不计 100）",
        "",
        "| 名称 | 原因 |",
        "|------|------|",
    ]
    for p in sorted(excluded, key=lambda x: (class_of(x)[1], x["name"])):
        lines.append(f"| {p['name'] or p['file']} | {exclude_reason(p)} |")

    lines += [
        "",
        "## 读表说明",
        "",
        "- 乐队成员的压力联动（立希汪、天素罗、GBC）仍算**所在乐队**，软体系只是叠加。",
        "- 章鱼噼按压力体系计入 100，不再当普通食物消耗品剔除。",
        "- 舞台祥子（Saki）弃用，后续删除。",
        "",
    ]
    with open(OUT_MD, "w", encoding="utf-8") as f:
        f.write("\n".join(lines))


def write_csv(all_rows):
    kept = [p for p in all_rows if not is_excluded(p)]
    kind_rank = {"乐队": 0, "软体系": 1, "通用": 2, "未点名": 3}

    def sort_key(p):
        kind, group, _ = class_of(p)
        return (kind_rank.get(kind, 9), group, p["price"], p["name"])

    with open(OUT_CSV, "w", encoding="utf-8-sig", newline="") as f:
        w = csv.writer(f)
        w.writerow(["大类", "体系", "名称", "阳光", "乐器", "七类", "备注", "状态", "种植", "文件"])
        for p in sorted(kept, key=sort_key):
            kind, group, note = class_of(p)
            w.writerow([
                kind, group, p["name"] or p["file"], p["price"],
                profession(p["tags"]), role_of(p), note, status_of(p),
                plant_type_label(p["plantType"]), p["file"].replace(".asset", ""),
            ])


def write_matrix(all_rows):
    kept = [p for p in all_rows if not is_excluded(p)]
    lines = [
        "# 计入 100 的植物 · 按策划体系",
        "",
        "| 大类 | 体系 | 人数 | 名单 |",
        "|------|------|------|------|",
    ]
    for kind, order in (("乐队", BAND_ORDER), ("软体系", SYSTEM_ORDER), ("通用", ["通用"])):
        for g in order:
            plants = [p for p in kept if class_of(p)[0] == kind and class_of(p)[1] == g]
            names = "、".join(sorted(p["name"] or p["file"] for p in plants))
            lines.append(f"| {kind} | {g} | {len(plants)} | {names} |")
    with open(OUT_MATRIX, "w", encoding="utf-8") as f:
        f.write("\n".join(lines) + "\n")


def main():
    rows = load_all()
    write_md(rows)
    write_csv(rows)
    write_matrix(rows)
    kept = [p for p in rows if not is_excluded(p)]
    unnamed = [p for p in rows if class_of(p)[0] == "未点名"]
    print(f"SO={len(rows)} kept={len(kept)} excluded={len(rows)-len(kept)} unnamed={len(unnamed)}")
    if unnamed:
        print("UNNAMED", [p["file"] for p in unnamed])
    print("Wrote", OUT_MD)


if __name__ == "__main__":
    main()
