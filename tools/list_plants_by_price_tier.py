# -*- coding: utf-8 -*-
"""Group plants by roguelike price tier (RoguelikePlantPriceBands)."""
import json
import os

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DUMP = os.path.join(ROOT, "tools", "plants_dump.json")
OUT = os.path.join(ROOT, "tools", "plants_by_price_tier.md")

LOW_MAX = 99
MID_MAX = 175


def decode(s: str) -> str:
    if "\\u" in s:
        try:
            return s.encode("utf-8").decode("unicode_escape")
        except Exception:
            return s
    return s


def tier(price: int) -> str:
    if price <= LOW_MAX:
        return "low"
    if price <= MID_MAX:
        return "mid"
    return "high"


TIER_LABEL = {
    "low": f"低档 Common（price ≤ {LOW_MAX}）",
    "mid": f"中档 Uncommon（{LOW_MAX + 1}–{MID_MAX}）",
    "high": f"高档 Rare（price > {MID_MAX}）",
}


with open(DUMP, encoding="utf-8") as f:
    data = json.load(f)

by_tier: dict[str, list] = {"low": [], "mid": [], "high": []}
by_price: dict[int, list] = {}

for plant in data["plants"]:
    name = decode(plant["name"])
    price = plant["price"]
    func = plant.get("func", "")
    buy = plant.get("buy", "")
    by_tier[tier(price)].append((price, name, func, buy))
    by_price.setdefault(price, []).append((name, func, buy))

for k in by_tier:
    by_tier[k].sort(key=lambda x: (x[0], x[1]))

lines = [
    f"# Plant 肉鸽价格分档（lowMax={LOW_MAX}, midMax={MID_MAX}）",
    "",
    "> 数据源：`Property.baseProperty.price` + `RoguelikeEconomyConfig.plantPriceBands`",
    "> 肉鸽三选一默认 `excludeLevelUpPlants: true`，升级卡标注但不进池。",
    "",
]

for key in ("low", "mid", "high"):
    items = by_tier[key]
    lines.append(f"## {TIER_LABEL[key]} — {len(items)} 张")
    lines.append("")
    lines.append("| price | chessName | 种植类型 | 肉鸽池 |")
    lines.append("|------:|-----------|----------|--------|")
    for price, name, func, buy in items:
        pool = "不进池（升级卡）" if func == "LevelUpPlant" else "可进池"
        if buy == "OnlyOne":
            pool += " / OnlyOne"
        lines.append(f"| {price} | {name} | {func or '—'} | {pool} |")
    lines.append("")

lines.append("## 按精确 price 汇总")
lines.append("")
for price in sorted(by_price.keys()):
    names = sorted(n for n, _, _ in by_price[price])
    lines.append(f"- **{price}**（{len(names)}）：{'、'.join(names)}")

with open(OUT, "w", encoding="utf-8") as out:
    out.write("\n".join(lines))
    out.write("\n")

print("Wrote", OUT)
