# -*- coding: utf-8 -*-
"""Compare ChessData/Player PropertyCreator assets vs GameManage.allChess."""
import os
import re

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
PLAYER = os.path.join(ROOT, "Assets", "Resources", "ChessData", "Player")
SCENE = os.path.join(ROOT, "Assets", "Scenes", "开始.unity")
OUT = os.path.join(ROOT, "tools", "plants_not_in_allchess.md")


def read_guid(meta_path: str) -> str | None:
    with open(meta_path, encoding="utf-8") as f:
        m = re.search(r"^guid:\s*(\w+)", f.read(), re.M)
        return m.group(1) if m else None


def read_chess_name(asset_path: str) -> str:
    with open(asset_path, encoding="utf-8") as f:
        c = f.read()
    m = re.search(r'chessName:\s*"([^"]*)"', c)
    if m and m.group(1).strip():
        return decode_unity(m.group(1))
    m = re.search(r"^  chessName:\s+(\S+)\s*$", c, re.M)
    if m and m.group(1).strip() and ":" not in m.group(1):
        return decode_unity(m.group(1))
    m = re.search(r'm_Name:\s*"([^"]+)"', c)
    if m:
        return decode_unity(m.group(1))
    return os.path.basename(asset_path).replace(".asset", "")


def decode_unity(s: str) -> str:
    if "\\u" in s:
        try:
            return s.encode("utf-8").decode("unicode_escape")
        except Exception:
            pass
    return s


def read_plant_type(asset_path: str) -> str:
    with open(asset_path, encoding="utf-8") as f:
        c = f.read()
    m = re.search(r"type:\s*\{class:\s*(\w+)", c)
    return m.group(1) if m else "?"


assets: dict[str, dict] = {}
for fn in sorted(os.listdir(PLAYER)):
    if not fn.endswith(".asset"):
        continue
    path = os.path.join(PLAYER, fn)
    guid = read_guid(path + ".meta")
    if not guid:
        continue
    assets[guid] = {
        "file": fn,
        "name": read_chess_name(path),
        "func": read_plant_type(path),
    }

with open(SCENE, encoding="utf-8") as f:
    scene = f.read()

block = re.search(r"allChess:\n((?:  - \{fileID:.*\n)+)", scene)
all_guids = re.findall(r"guid:\s*([0-9a-f]+)", block.group(1) if block else "")
all_set = set(all_guids)

missing = []
for guid, info in sorted(assets.items(), key=lambda x: x[1]["name"]):
    if guid not in all_set:
        missing.append((info["name"], info["file"], info["func"], guid))

extra = [g for g in all_guids if g not in assets]

lines = [
    "# Player PropertyCreator 未注册 allChess",
    "",
    f"> Player 目录 asset：**{len(assets)}**；`GameManage.allChess`：**{len(all_guids)}** 条（唯一 {len(all_set)}）",
    "",
    f"## 有 creator、未进 allChess（{len(missing)}）",
    "",
    "| chessName | asset 文件 | plantFunction |",
    "|-----------|------------|---------------|",
]
for name, fn, func, _ in missing:
    lines.append(f"| {name} | {fn} | {func} |")

lines.extend(["", f"## allChess 引用但 Player 目录无 asset（{len(extra)}）", ""])
if extra:
    for g in extra:
        lines.append(f"- `{g}`")
else:
    lines.append("（无）")

with open(OUT, "w", encoding="utf-8") as out:
    out.write("\n".join(lines))
    out.write("\n")

print("Wrote", OUT)
print("NOT in allChess:", len(missing))
for name, fn, func, _ in missing:
    print(f"  {name} ({fn}) [{func}]")
