# -*- coding: utf-8 -*-
import os, re, json

dir_path = r"g:\AsoulChess\AVZ\Assets\Resources\ChessData\Player"
results = []

for fn in sorted(os.listdir(dir_path)):
    if not fn.endswith(".asset"):
        continue
    path = os.path.join(dir_path, fn)
    with open(path, encoding="utf-8") as f:
        c = f.read()

    def m(pat, default=""):
        r = re.search(pat, c, re.M)
        return r.group(1) if r else default

    name = m(r'chessName:\s*"([^"]+)"') or fn.replace(".asset", "")
    price = int(m(r"^\s+price:\s*(\d+)", "0") or 0)
    hp = int(m(r"^\s+HpMax:\s*(\d+)", m(r"^\s+Hp:\s*(\d+)", "0")) or 0)
    atk = int(m(r"^\s+attack:\s*(\d+)", "0") or 0)
    pt = int(m(r"^\s+plantType:\s*(\d+)", "0") or 0)
    func = m(r"type:\s*\{class:\s*(\w+)")
    tag_block = re.search(r"plantTags:(.*?)(?:\n  fetterMemberId)", c, re.S)
    taglist = re.findall(r'-\s+"?([^"\n]+)"?', tag_block.group(1)) if tag_block else []
    effect = m(r"^\s+chessEffect:\s*(.*)", "").strip()
    short = m(r"^\s+chessShortDescription:\s*(.*)", "").strip()
    buy = ""
    if "OnlyOne_Limit" in c:
        buy = "OnlyOne"
    elif "LevelUp_Limit" in c:
        buy = "LevelUp"
    cd = m(r"^\s+CD:\s*([\d.]+)", "")

    results.append(
        {
            "file": fn,
            "name": name,
            "price": price,
            "hp": hp,
            "atk": atk,
            "cd": cd,
            "plantType": pt,
            "func": func,
            "tags": taglist,
            "effect": effect[:100],
            "buy": buy,
        }
    )

out_path = os.path.join(os.path.dirname(__file__), "plants_dump.json")
with open(out_path, "w", encoding="utf-8") as out:
    json.dump({"count": len(results), "plants": results}, out, ensure_ascii=False, indent=2)
print("Wrote", out_path, "count", len(results))
