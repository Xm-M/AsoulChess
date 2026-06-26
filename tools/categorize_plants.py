# -*- coding: utf-8 -*-
"""Auto-suggest plant role from prefab skills + PropertyCreator stats."""
import json, re, os

ROLES = {
    "sun": "4-产出阳光",
    "ash": "6-缓解压力/灰烬",
    "transition": "5-过度",
    "counter": "7-针对类",
    "support": "3-辅助",
    "front": "1-前排",
    "output": "2-输出",
    "upgrade": "升级卡",
    "terrain": "地形/消耗品",
    "wip": "未完成/占位",
    "other": "待确认",
}

SUN_KW = ("CreateSunLight",)
ASH_KW = ("KitaExplode", "SkillEffect_Explode", "SkillEffect_Jalapeno", "SkillEffect_IceJalapeno", "SkillEffect_TriggerRain")
TRANS_KW = ("PotatoMine", "SkillEffect_Saki", "MushRoom", "ConsumePlant")
COUNTER_KW = ("LettuceUmbrella", "PassiveSkill_LettuceUmbrella")
SUPPORT_KW = ("Heal", "Tomo", "OverflowHeal", "YuiToggleHealMode", "Doloris", "Mujica_Nyamu", "PumpkinShell", "Mon3tr", "M3Rebuild")
FRONT_KW = ("Passive_Boqi", "Rupa", "Taunt", "NinaTaunt")
OUTPUT_KW = ("ShootBullet", "SkillEffect_Nina", "SkillEffect_Oblivionis", "SkillEffect_Timoris", "Weapon_Sample")


def decode_name(raw, filename):
    if raw and not re.search(r"\\u", raw):
        return raw
    # read chessName from asset directly
    ap = os.path.join(r"g:\AsoulChess\AVZ\Assets\Resources\ChessData\Player", filename)
    with open(ap, encoding="utf-8") as f:
        c = f.read()
    m = re.search(r'chessName:\s*"([^"]+)"', c)
    if m:
        return m.group(1)
    m = re.search(r"chessName:\s*(.+)", c)
    if m:
        s = m.group(1).strip()
        if "\\u" in s:
            return s.encode("utf-8").decode("unicode_escape")
        return s
    return filename.replace(".asset", "")


def suggest(p):
    func = p.get("func") or ""
    pt = p.get("plantType") or 0
    hp, atk, price = p.get("hp", 0), p.get("atk", 0), p.get("price", 0)
    skills = " ".join(p.get("skills") or [])
    buy = p.get("buy") or ""

    if pt == 0 and func in ("", "NonePlant") and price == 50 and hp == 300:
        return ROLES["wip"], "plantType=0，配置未完成"
    if func == "LevelUpPlant" or "LevelUpPlant" in func or buy == "OnlyOne" and func.startswith("LevelUp"):
        if any(k in skills for k in SUN_KW):
            pass  # may still be upgrade sun plant
        elif not any(k in skills for k in ASH_KW + TRANS_KW):
            if func.startswith("LevelUp") or buy == "OnlyOne":
                return ROLES["upgrade"], func or buy

    if func == "PotPlant":
        return ROLES["terrain"], "PotPlant 地形"
    if func == "ConsumePlant" or pt == 8:
        return ROLES["terrain"], "ConsumePlant 一次性消耗品"
    if func == "ExclusivePlant":
        return ROLES["other"], "ExclusivePlant 附着道具"

    if any(k in skills for k in SUN_KW):
        return ROLES["sun"], "CreateSunLight"
    if any(k in skills for k in ASH_KW):
        return ROLES["ash"], "爆炸/灰烬类技能"
    if any(k in skills for k in TRANS_KW) or (price <= 75 and atk >= 500 and hp <= 5000):
        if "PotatoMine" in skills or "SkillEffect_Saki" in skills or "MushRoom" in skills:
            return ROLES["transition"], skills or "低费一次性机制"
        if atk >= 1800 and price <= 50:
            return ROLES["transition"], "高爆发低费"

    if any(k in skills for k in COUNTER_KW):
        return ROLES["counter"], "针对/反制"
    if any(k in skills for k in SUPPORT_KW) or func == "SupportPlant":
        if "PumpkinShell" in skills:
            return ROLES["front"], "南瓜罩=套壳防护"
        return ROLES["support"], skills or "SupportPlant"

    if any(k in skills for k in FRONT_KW) or (hp >= 2000 and atk <= 60 and func == "MainPlant"):
        return ROLES["front"], f"高HP({hp})低攻"
    if func == "AimTargetPlant":
        return ROLES["output"], "AimTargetPlant 附着输出"

    if atk >= 80 or any(k in skills for k in OUTPUT_KW):
        return ROLES["output"], f"atk={atk}或射击技能"

    if atk == 0 and hp >= 300:
        return ROLES["other"], "无攻击，待策划定"

    return ROLES["other"], "默认待确认"


with open(r"g:\AsoulChess\AVZ\tools\plants_with_skills.json", encoding="utf-8") as f:
    data = json.load(f)

rows = []
for p in data["plants"]:
    name = decode_name(p.get("name"), p["file"])
    role, reason = suggest(p)
    rows.append(
        {
            "name": name,
            "file": p["file"],
            "price": p["price"],
            "hp": p["hp"],
            "atk": p["atk"],
            "func": p.get("func") or "-",
            "skills": ", ".join(p.get("skills") or []) or "-",
            "role": role,
            "reason": reason,
        }
    )

rows.sort(key=lambda r: (r["role"], r["name"]))

# group counts
from collections import Counter

cnt = Counter(r["role"] for r in rows)
print("COUNTS:", dict(cnt))
for role in sorted(set(r["role"] for r in rows)):
    print("\n##", role)
    for r in rows:
        if r["role"] == role:
            print(f"| {r['name']} | {r['price']} | {r['hp']} | {r['atk']} | {r['func']} | {r['skills'][:40]} | {r['reason']} |")

with open(r"g:\AsoulChess\AVZ\tools\plants_role_suggest.json", "w", encoding="utf-8") as f:
    json.dump({"counts": dict(cnt), "plants": rows}, f, ensure_ascii=False, indent=2)
