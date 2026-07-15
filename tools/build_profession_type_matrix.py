# -*- coding: utf-8 -*-
"""Build profession × role matrix for plant-deckbuilding-design.md"""
import json
import os
import re

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DUMP = os.path.join(ROOT, "tools", "plants_dump.json")

# §9.1 策划定稿 v1 — chessName → 定位键
ROLE_MAP = {}
groups = {
    "1": [
        "波奇", "rupa", "槌蛇波奇", "南瓜罩", "琴吹䌷", "长崎素世", "Mortis",
    ],
    "2": [
        "井芹仁菜", "主唱 Nina", "Oblivionis", "八幡海玲 Timoris", "椎名立希", "凉",
        "圣聆初雪", "要乐奈", "祐天寺若麦", "河源木桃香", "吃豆凉", "广井菊里",
        "常服高松灯", "Saki", "八九寺真宵", "吉他英雄", "黄瓜睦", "平泽唯",
    ],
    "3": ["星歌", "三角初华", "Tomo", "M3", "秋山澪", "宇宙高松灯"],
    "4": ["486", "虹夏", "小虹夏", "桃金娘", "纯田真奈", "千早爱音"],
    "5": ["丰川祥子", "灵感菇", "爱芮", "嘉然土豆雷", "橘福福"],
    "6": [
        "喜多", "喜多遗照", "史尔特尔", "苍角", "企鹅高松灯", "初音小推车",
        "仪玄", "迈巴赫", "潘引壶",
    ],
    "7": ["薇薇安", "南宫羽"],
    "消耗品": ["H弹", "蛋包饭", "牛肉饭", "甜甜圈", "抹茶芭菲"],
    "地形": ["睡莲"],
    "未完成": ["凛御银灰", "希希芙", "猫猫", "千夏", "田井中律", "叶瞬光"],
    "待确认": ["Soldier", "丰川清告"],
}
for role, names in groups.items():
    for n in names:
        ROLE_MAP[n] = role

# 乐器职业（plantTags 中的职业向标签，非乐队名）
PROFESSION_TAGS = ["鼓手", "贝斯", "吉他", "键盘", "键盘手", "主唱"]
PROF_ROW_ORDER = ["鼓手", "贝斯", "吉他", "键盘", "主唱", "无职业标签"]

TYPE_COLS = [
    ("1", "1 前排"),
    ("2", "2 输出"),
    ("3", "3 辅助"),
    ("4", "4 产阳"),
    ("5", "5 过度"),
    ("6", "6 灰烬"),
    ("7", "7 针对"),
    ("消耗品", "消耗品"),
    ("地形", "地形"),
    ("未完成", "未完成"),
    ("待确认", "待确认"),
]

BAND_LIKE = {
    "放学后茶会", "无刺有刺", "AveMujica", "Mygo", "结束乐队", "sumimi",
    "妄想天使", "谢拉格", "库莱西裤", "奇葩塔", "明日方舟", "酒鬼", "反舌鸟",
    "保镖", "士兵",
}


def decode_unicode_escapes(s: str) -> str:
    if "\\u" in s:
        try:
            return s.encode("utf-8").decode("unicode_escape")
        except Exception:
            return s
    return s


def get_profession(tags: list[str]) -> str:
    decoded = [decode_unicode_escapes(t).strip() for t in tags]
    for p in PROFESSION_TAGS:
        if p in decoded:
            return "键盘" if p == "键盘手" else p
    return "无职业标签"


with open(DUMP, encoding="utf-8") as f:
    data = json.load(f)

matrix: dict[str, dict[str, list[str]]] = {p: {c[0]: [] for c in TYPE_COLS} for p in PROF_ROW_ORDER}

for plant in data["plants"]:
    name = decode_unicode_escapes(plant["name"])
    role = ROLE_MAP.get(name, "?")
    if role == "?":
        # fallback file stem
        stem = plant["file"].replace(".asset", "")
        role = ROLE_MAP.get(stem, "?")

    prof = get_profession([decode_unicode_escapes(t) for t in plant.get("tags", [])])
    if role not in matrix[prof]:
        continue
    matrix[prof][role].append(name)

# Markdown output
lines = []
lines.append("| 职业（乐器） | " + " | ".join(c[1] for c in TYPE_COLS) + " |")
lines.append("|--------------|" + "|".join(["---"] * len(TYPE_COLS)) + "|")

for prof in PROF_ROW_ORDER:
    cells = []
    for col_id, _ in TYPE_COLS:
        names = matrix[prof][col_id]
        if not names:
            cells.append("—")
        else:
            cells.append("、".join(sorted(names, key=lambda x: x)))
    lines.append(f"| **{prof}** | " + " | ".join(cells) + " |")

out_path = os.path.join(ROOT, "tools", "profession_type_matrix.md")
with open(out_path, "w", encoding="utf-8") as out:
    out.write("\n".join(lines))
    out.write("\n")
print("Wrote", out_path)
